#nullable enable
using System;
using System.Collections.Generic;
using SeweralIdeas.UnityUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(Config))]
    public class ConfigEditor : UnityEditor.Editor
    {
        private        int       m_selectedField;
        private static GUIStyle? s_toggleStyle;
        private static GUIStyle? s_minusButtonStyle;
        private        Rect      m_createRect;

        private static readonly string[] Exclude = {"m_fields"};
        
        public override void OnInspectorGUI()
        {
            Config config = (Config)target;
            SerializedProperty foldout = serializedObject.FindProperty("m_globalName");

            s_toggleStyle ??= new GUIStyle(EditorStyles.miniButton) { alignment = TextAnchor.LowerLeft };
            s_minusButtonStyle ??= new GUIStyle("OL Minus") { fixedWidth = 24 };

            DrawPropertiesExcluding(serializedObject, Exclude);
            
            StorageGUI(config);

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            foldout.isExpanded = EditorGUILayout.Foldout(foldout.isExpanded, "Fields");

            if(foldout.isExpanded)
            {
                FieldsGUI(config);
            }

            serializedObject.ApplyModifiedProperties();
        }
        private static void StorageGUI(Config config)
        {
            GUI.enabled = config.StoragePlan != null;
            using (new GUILayout.HorizontalScope())
            {
                if(GUILayout.Button("Load", EditorStyles.miniButtonLeft))
                {
                    config.Load();
                }

                GUI.enabled &= config.IsDirty;
                if(GUILayout.Button("Save", EditorStyles.miniButtonRight))
                {
                    config.Save();
                }
            }
            GUI.enabled = true;
        }
        
        private void FieldsGUI(Config config)
        {
            // Creation Dropdown
            bool clicked = GUILayout.Button("Create", "MiniPopup");
            if(Event.current.type == EventType.Repaint)
                m_createRect = GUILayoutUtility.GetLastRect();
            
            if(clicked)
            {
                TypeDropdown.ShowTypeDropdown(m_createRect, new(typeof( ConfigField ), false, true), type => AddField_Delayed(type, config));
            }
            
            if(config.Fields.Count > 0)
            {
                for( int i = 0; i < config.Fields.Count; ++i )
                {
                    FieldGUI(config, i);
                }
            }
            
            SelectedFieldGUI(config);
        }
        
        private void FieldGUI(Config config, int index)
        {
            ConfigField field = config.Fields[index];
            GUIContent content = EditorGUIUtility.ObjectContent(field, field.GetType());
            content.text = field.name;

            using (new GUILayout.HorizontalScope())
            {
                bool newSelected = GUILayout.Toggle(m_selectedField == index, content, s_toggleStyle);
                if(newSelected)
                    m_selectedField = index;

                if(GUILayout.Button(GUIContent.none, s_minusButtonStyle))
                {
                    EditorApplication.delayCall += () => RemoveField_Delayed(config, index);
                }
            }
        }

        private void SelectedFieldGUI(Config config)
        {
            static bool Contains(IReadOnlyList<ConfigField> list, ConfigField field)
            {
                int count = list.Count;
                for(int i = 0; i< count; ++i)
                    if(list[i] == field)
                        return true;
                return false;
            }
            
            // Selected field editor
            if(m_selectedField < 0 || m_selectedField >= config.Fields.Count)
                return;
            ConfigField selectedFieldObj = config.Fields[m_selectedField];
            
            GUILayout.BeginVertical(GUIContent.none, "box");
            
            string newName = EditorGUILayout.DelayedTextField("Name", selectedFieldObj.name);
            if(newName != selectedFieldObj.name)
            {
                Undo.RegisterCompleteObjectUndo(selectedFieldObj, "Rename ConfigField");
                selectedFieldObj.name = newName;
            }

            if(selectedFieldObj)
            {
                UnityEditor.Editor editor = CreateEditor(selectedFieldObj);
                editor.OnInspectorGUI();
                editor.serializedObject.ApplyModifiedProperties();
            }
            GUILayout.EndVertical();
        }

        private static void RemoveField_Delayed(Config config, int index)
        {
            Undo.RegisterCompleteObjectUndo(config,"Remove Field");
            SerializedObject sObj = new SerializedObject(config);
            
            SerializedProperty? fieldsArrayProp = sObj.FindProperty("m_fields");
            SerializedProperty? fieldProp = fieldsArrayProp.GetArrayElementAtIndex(index);
            ConfigField? field = fieldProp.objectReferenceValue as ConfigField;
            fieldsArrayProp.DeleteArrayElementAtIndex(index);
            
            if(field != null)
                Undo.DestroyObjectImmediate(field);
            
            sObj.ApplyModifiedProperties();
            SaveAndRefresh();
        }
        
        private void AddField_Delayed(Type type, Config config)
        {
            SerializedObject sObj = new SerializedObject(config);
            
            ConfigField newField = (ConfigField)CreateInstance(type);
            newField.name = type.Name;
            AssetDatabase.AddObjectToAsset(newField, config);
            Undo.RegisterCreatedObjectUndo(newField, "Created ConfigField");

            SerializedProperty? fieldsArrayProp = sObj.FindProperty("m_fields");
            int index = fieldsArrayProp.arraySize;
            fieldsArrayProp.InsertArrayElementAtIndex(index);
            SerializedProperty? fieldProp = fieldsArrayProp.GetArrayElementAtIndex(index);
            fieldProp.objectReferenceValue = newField;
            m_selectedField = index;
            
            sObj.ApplyModifiedProperties();
            SaveAndRefresh();
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
