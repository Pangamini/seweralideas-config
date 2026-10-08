#nullable enable
using System;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    /// <summary>
    /// A config's fields as a table of their current values, which can be edited in place, in the order of the list, which
    /// can be dragged. A field is a nested asset of the config: "+" creates one, "-" removes (and destroys) the selected
    /// one, and the selected field is edited below the table (its name, which is its storage key, and its default value).
    /// </summary>
    [CustomEditor(typeof(Config))]
    public class ConfigEditor : UnityEditor.Editor
    {
        private const string FieldsProperty = "m_fields";
        private const float  RowPadding     = 2;

        private static readonly string[] Exclude = {FieldsProperty};

        private ReorderableList m_list = null!;
        private UnityEditor.Editor m_fieldEditor = null!; // of the selected field, kept between repaints

        protected void OnEnable()
        {
            m_list = new ReorderableList(serializedObject, serializedObject.FindProperty(FieldsProperty), true, true, true, true)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = GetElementHeight,
                onAddDropdownCallback = OnAddDropdown,
                onRemoveCallback = OnRemove
            };
        }

        protected void OnDisable()
        {
            if(m_fieldEditor != null)
                DestroyImmediate(m_fieldEditor);
            m_fieldEditor = null!;
        }

        // The values change while the game runs
        public override bool RequiresConstantRepaint() => Application.isPlaying;

        public override void OnInspectorGUI()
        {
            var config = (Config)target;

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, Exclude);
            StorageGUI(config);

            GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            m_list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            SelectedFieldGUI(config);
        }

        private static void StorageGUI(Config config)
        {
            using (new EditorGUI.DisabledScope(config.StoragePlan == null))
            using (new GUILayout.HorizontalScope())
            {
                if(GUILayout.Button("Load", EditorStyles.miniButtonLeft))
                    config.Load();

                using (new EditorGUI.DisabledScope(!config.IsDirty))
                {
                    if(GUILayout.Button("Save", EditorStyles.miniButtonMid))
                        config.Save();
                }

                var showContent = new GUIContent("Show", "Shows where the config is stored: the file in the file browser, or the " +
                                                         "PlayerPrefs of this project (regedit on Windows).");
                if(GUILayout.Button(showContent, EditorStyles.miniButtonRight))
                    config.RevealStorageInEditor();
            }

            if(config.IsDirty)
                EditorGUILayout.LabelField("Unsaved changes", EditorStyles.miniLabel);
        }

        #region Table
        private float NameWidth(Rect rect) => Mathf.Min(rect.width * 0.4f, 180);

        private void DrawHeader(Rect rect)
        {
            float nameWidth = NameWidth(rect);
            EditorGUI.LabelField(new Rect(rect.x, rect.y, nameWidth, rect.height), "Fields");
            EditorGUI.LabelField(new Rect(rect.x + nameWidth, rect.y, rect.width - nameWidth, rect.height), "Value");
        }

        private ConfigField? GetField(int index) => m_list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue as ConfigField;

        private float GetElementHeight(int index)
        {
            ConfigField? field = GetField(index);
            float height = field != null ? Mathf.Max(EditorGUIUtility.singleLineHeight, field.GetRowHeight()) : EditorGUIUtility.singleLineHeight;
            return height + RowPadding * 2;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            ConfigField? field = GetField(index);
            rect.y += RowPadding;
            rect.height -= RowPadding * 2;

            if(field == null) // a destroyed asset, or an entry cleared by hand: it can still be removed
            {
                EditorGUI.LabelField(rect, "(missing)", EditorStyles.miniLabel);
                return;
            }

            float nameWidth = NameWidth(rect);
            GUIContent icon = EditorGUIUtility.ObjectContent(field, field.GetType());
            var nameRect = new Rect(rect.x, rect.y, nameWidth - 4, EditorGUIUtility.singleLineHeight);
            GUI.Label(nameRect, new GUIContent(field.name, icon.image, field.GetType().Name));

            // The current value (the runtime one, not the default: that is edited below, in the selected field).
            field.OnConfigRowGUI(new Rect(rect.x + nameWidth, rect.y, rect.width - nameWidth, field.GetRowHeight()));
        }
        #endregion

        #region Selected field
        private void SelectedFieldGUI(Config config)
        {
            int index = m_list.index;
            if(index < 0 || index >= m_list.count)
                return;

            ConfigField? selected = GetField(index);
            if(selected == null)
                return;

            using (new GUILayout.VerticalScope(GUIContent.none, "box"))
            {
                string newName = EditorGUILayout.DelayedTextField("Name", selected.name);
                if(newName != selected.name)
                    RenameField(config, selected, newName);

                // Kept, not created on every repaint: an Editor is an object, and a leaked one stays until the domain reloads.
                // (It applies its own edits, see ConfigFieldEditor.)
                CreateCachedEditor(selected, null, ref m_fieldEditor);
                m_fieldEditor.OnInspectorGUI();
            }
        }

        // The name of a field is the key it is stored under, so no two fields of a config can share one.
        private static void RenameField(Config config, ConfigField field, string newName)
        {
            if(string.IsNullOrWhiteSpace(newName))
                return;

            foreach (ConfigField other in config.Fields)
            {
                if(other != null && other != field && other.name == newName)
                {
                    Debug.LogWarning($"Config {config.name} already has a field called '{newName}': the name is its storage key.", config);
                    return;
                }
            }

            Undo.RecordObject(field, "Rename ConfigField");
            field.name = newName;
            EditorUtility.SetDirty(field); // changing the name of an asset isn't noticed otherwise, and it would not be saved
            SaveAndRefresh();
        }
        #endregion

        #region Add / remove
        private void OnAddDropdown(Rect buttonRect, ReorderableList list)
        {
            var config = (Config)target;
            TypeDropdown.ShowTypeDropdown(buttonRect, new(typeof(ConfigField), false, true), type => AddField_Delayed(type, config));
        }

        private void OnRemove(ReorderableList list)
        {
            var config = (Config)target;
            int index = list.index;
            EditorApplication.delayCall += () => RemoveField_Delayed(config, index);
        }

        private void RemoveField_Delayed(Config config, int index)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Remove ConfigField");
            int undoGroup = Undo.GetCurrentGroup();

            using SerializedObject sObj = new SerializedObject(config);

            SerializedProperty fieldsArrayProp = sObj.FindProperty(FieldsProperty);
            if(index < 0 || index >= fieldsArrayProp.arraySize)
                return;

            SerializedProperty fieldProp = fieldsArrayProp.GetArrayElementAtIndex(index);
            ConfigField? field = fieldProp.objectReferenceValue as ConfigField;

            // Deleting an element that holds an object only clears it; the second call removes the element. Without
            // this, the list is left with an empty entry.
            if(fieldProp.objectReferenceValue != null)
                fieldProp.objectReferenceValue = null;
            fieldsArrayProp.DeleteArrayElementAtIndex(index);
            sObj.ApplyModifiedProperties();

            if(field != null)
                Undo.DestroyObjectImmediate(field);

            Undo.CollapseUndoOperations(undoGroup);
            m_list.index = Mathf.Clamp(m_list.index, 0, fieldsArrayProp.arraySize - 1);

            SaveAndRefresh();
        }

        private void AddField_Delayed(Type type, Config config)
        {
            if(!EditorUtility.IsPersistent(config))
            {
                Debug.LogError("Fields are sub-assets of the config, which has to be an asset first.", config);
                return;
            }

            // One undo step for the whole thing: the object, and its place in the list
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Create ConfigField");
            int undoGroup = Undo.GetCurrentGroup();

            ConfigField newField = (ConfigField)CreateInstance(type);
            newField.name = GetUniqueName(config, type.Name);
            using (SerializedObject fieldSObj = new(newField))
            {
                fieldSObj.FindProperty("m_config").objectReferenceValue = config;
                fieldSObj.ApplyModifiedPropertiesWithoutUndo(); // not an asset yet: nothing to undo
            }

            AssetDatabase.AddObjectToAsset(newField, config);
            Undo.RegisterCreatedObjectUndo(newField, "Create ConfigField");

            using SerializedObject sObj = new (config);
            SerializedProperty fieldsArrayProp = sObj.FindProperty(FieldsProperty);
            int index = fieldsArrayProp.arraySize;
            fieldsArrayProp.InsertArrayElementAtIndex(index);
            fieldsArrayProp.GetArrayElementAtIndex(index).objectReferenceValue = newField;
            m_list.index = index;

            sObj.ApplyModifiedProperties();
            Undo.CollapseUndoOperations(undoGroup);
            EditorUtility.SetDirty(config);
            SaveAndRefresh();
        }

        // The name is the key the field is stored under, so it has to differ from the other fields' (FloatConfigField, FloatConfigField 1, ...).
        private static string GetUniqueName(Config config, string baseName)
        {
            var taken = new HashSet<string>();
            foreach (ConfigField field in config.Fields)
            {
                if(field != null)
                    taken.Add(field.name);
            }

            string name = baseName;
            for (int i = 1; taken.Contains(name); ++i)
                name = $"{baseName} {i}";
            return name;
        }

        private static void SaveAndRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }
        #endregion
    }
}
