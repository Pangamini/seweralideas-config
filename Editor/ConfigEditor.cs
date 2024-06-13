using System;
using SeweralIdeas.UnityUtils.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(Config))]
    public class ConfigEditor : UnityEditor.Editor
    {
        private static ConfigField s_selectedField;
        private static GUIStyle s_toggleStyle;
        private static GUIStyle s_minusButtonStyle;
        private Rect m_createRect;

        public override void OnInspectorGUI()
        {
            Config config = (Config)target;
            SerializedProperty foldout = serializedObject.FindProperty("m_globalName");

            s_toggleStyle ??= new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.LowerLeft };
            s_minusButtonStyle ??= new GUIStyle("OL Minus") { fixedWidth = 24 };

            base.OnInspectorGUI();

            
            StorageGUI(config);

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            foldout.isExpanded = EditorGUILayout.Foldout(foldout.isExpanded, "Fields");

            if(foldout.isExpanded)
            {
                FieldsGUI(config);
            }
        }
        private static void StorageGUI(Config config)
        {

            using (new GUILayout.HorizontalScope())
            {
                if(GUILayout.Button("Load", EditorStyles.miniButtonLeft))
                {
                    config.Load();
                }

                GUI.enabled = config.IsDirty;
                if(GUILayout.Button("Save", EditorStyles.miniButtonRight))
                {
                    config.Save();
                }
                GUI.enabled = true;
            }
        }
        private void FieldsGUI(Config config)
        {
            // Creation Dropdown
            bool clicked = GUILayout.Button("Create", "MiniPopup");
            if(Event.current.type == EventType.Repaint)
                m_createRect = GUILayoutUtility.GetLastRect();
            if(clicked)
            {
                TypeDropdown.ShowTypeDropdown(m_createRect, new(typeof( ConfigField ), false, true), AddField);
            }


            // ConfigField list
            if(config.Fields.Count > 0)
            {
                using (ListPool<ConfigField>.Get(out var configFields))
                {
                    foreach (var field in config.Fields)
                    {
                        if(!field)
                            continue;
                        configFields.Add(field);
                    }
                    configFields.Sort((lhs, rhs) => String.Compare(lhs.name, rhs.name, StringComparison.Ordinal));

                    for( int i = 0; i < configFields.Count; ++i )
                    {
                        ConfigField field = configFields[i];
                        GUIContent content = EditorGUIUtility.ObjectContent(field, field.GetType());
                        content.text = field.name;

                        using (new GUILayout.HorizontalScope())
                        {
                            bool newSelected = GUILayout.Toggle(s_selectedField == field, content, s_toggleStyle);
                            if(newSelected)
                                s_selectedField = field;

                            if(GUILayout.Button(GUIContent.none, s_minusButtonStyle))
                            {
                                RemoveField(field);
                            }
                        }
                    }
                }
            }
            
            // Selected field editor
            if(s_selectedField && config.Fields.Contains(s_selectedField))
            {
                GUILayout.BeginVertical(GUIContent.none, "box");
                string newName = EditorGUILayout.DelayedTextField("Name", s_selectedField.name);
                if(newName != s_selectedField.name)
                {
                    Undo.RegisterCompleteObjectUndo(s_selectedField, "Rename ConfigField");
                    s_selectedField.name = newName;
                }

                UnityEditor.Editor editor = CreateEditor(s_selectedField);
                editor.OnInspectorGUI();
                editor.serializedObject.ApplyModifiedProperties();
                GUILayout.EndVertical();
            }
        }

        private void RemoveField(ConfigField field)
        {
            Undo.DestroyObjectImmediate(field);
            SaveAndRefresh();
        }

        private void AddField(Type type)
        {
            ConfigField newField = (ConfigField)CreateInstance(type);
            newField.name = type.Name;
            using var so = new SerializedObject(newField);
            SerializedProperty configProp = so.FindProperty("m_config");
            configProp.objectReferenceValue = target;
            so.ApplyModifiedProperties();
            AssetDatabase.AddObjectToAsset(newField, target);
            serializedObject.ApplyModifiedProperties();
            SaveAndRefresh();
            Undo.RegisterCreatedObjectUndo(newField, "Created ConfigField");
            s_selectedField = newField;
        }
        
        private static void SaveAndRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }
    }
}
