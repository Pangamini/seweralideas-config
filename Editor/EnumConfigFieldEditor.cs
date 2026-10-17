#nullable enable
using System;
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(EnumConfigField), true)]
    public class EnumConfigFieldEditor : ConfigFieldEditor
    {
        protected override void DrawFields()
        {
            SerializedProperty typeProperty = serializedObject.FindProperty("m_enumTypeName");
            SerializedProperty defaultProperty = serializedObject.FindProperty("m_defaultValue");

            Type? type = EnumConfigField.ResolveType(typeProperty.stringValue);
            DrawTypeRow(typeProperty, type);

            if(type == null)
            {
                EditorGUILayout.PropertyField(defaultProperty);
                if(!string.IsNullOrEmpty(typeProperty.stringValue))
                    EditorGUILayout.HelpBox($"The enum {typeProperty.stringValue} was not found. The field takes any text until one is set.", MessageType.Warning);
                else
                    EditorGUILayout.HelpBox("No enum is set: the field takes any text.", MessageType.Info);
                return;
            }

            DrawDefaultRow(defaultProperty, type);
        }

        // The type is picked in a popup window (a search and a list); this shows the one chosen
        private void DrawTypeRow(SerializedProperty typeProperty, Type? type)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, new GUIContent("Enum Type", "Click to search the enums by name."));

            string text = type != null ? type.FullName! : string.IsNullOrEmpty(typeProperty.stringValue) ? "(none)" : typeProperty.stringValue + " (not found)";
            if(!EditorGUI.DropdownButton(rect, new GUIContent(text, typeProperty.stringValue), FocusType.Keyboard))
                return;

            var fields = Array.ConvertAll(targets, t => (EnumConfigField)t);
            PopupWindow.Show(rect, new EnumTypePopup(type, picked =>
            {
                foreach(EnumConfigField field in fields)
                {
                    Undo.RecordObject(field, "Set Enum Type");
                    field.SetEnumType(picked);
                    EditorUtility.SetDirty(field);
                }
            }));
        }

        private static void DrawDefaultRow(SerializedProperty defaultProperty, Type type)
        {
            string text = defaultProperty.stringValue;
            bool valid = EnumConfigField.TryParseValue(type, text, out object current);
            bool flags = type.IsDefined(typeof(FlagsAttribute), false);

            EditorGUILayout.LabelField("Default Value", valid ? current.ToString() : text);

            // A list of the options: toggles for flags, any number of them on; buttons otherwise, one on
            object zero = Enum.ToObject(type, 0);
            if(!valid)
                current = zero;

            EditorGUI.indentLevel++;
            foreach(string optionName in Enum.GetNames(type))
            {
                object option = Enum.Parse(type, optionName);
                if(flags && option.Equals(zero))
                    continue;

                bool on = valid && EnumConfigField.IsActive(type, current, option);
                if(flags)
                {
                    bool newOn = EditorGUILayout.ToggleLeft(optionName, on);
                    if(newOn != on)
                        defaultProperty.stringValue = EnumConfigField.SetFlag(type, current, option, newOn).ToString();
                }
                else
                {
                    Rect row = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                    if(GUI.Toggle(row, on, optionName, EditorStyles.miniButton) && !on)
                        defaultProperty.stringValue = optionName;
                }
            }
            EditorGUI.indentLevel--;

            if(!valid)
                EditorGUILayout.HelpBox(string.IsNullOrEmpty(text) ? "No default value is chosen." : $"\"{text}\" is not an option of {type.Name}.", MessageType.Warning);
            else if(defaultProperty.stringValue == text && text != current.ToString())
                defaultProperty.stringValue = current.ToString(); // written another way (the case, the order of the flags): make it the canonical one
        }
    }
}
