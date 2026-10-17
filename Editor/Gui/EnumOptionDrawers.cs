#nullable enable
using System;
using System.Collections.Generic;
using SeweralIdeas.Config;
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.ConfigGui.Editor
{
    internal static class EnumOptionDrawerUtil
    {
        // The name of the option as a popup of the names of the enum of the field of the element (a text field while there is none)
        public static void DrawOptionName(Rect rect, SerializedProperty property, SerializedProperty name)
        {
            SerializedProperty fieldProperty = property.serializedObject.FindProperty("m_field");
            Type? enumType = (fieldProperty?.objectReferenceValue as EnumConfigField)?.EnumType;
            if(enumType == null)
            {
                EditorGUI.PropertyField(rect, name, GUIContent.none);
                return;
            }

            string[] names = Enum.GetNames(enumType);
            int index = Array.IndexOf(names, name.stringValue);
            var shown = new List<string>(names);
            int offset = 0;
            if(index < 0)
            {
                shown.Insert(0, string.IsNullOrEmpty(name.stringValue) ? "(none)" : $"{name.stringValue} (not an option)");
                offset = 1;
            }

            int selected = EditorGUI.Popup(rect, index + offset, shown.ToArray());
            if(selected - offset >= 0 && selected - offset != index)
                name.stringValue = names[selected - offset];
        }
    }

    /// <summary>An option of a switch: a popup of the names of the enum, and the toggle of it.</summary>
    [CustomPropertyDrawer(typeof(ConfigGuiEnumSwitch.Option))]
    internal class ConfigGuiEnumSwitchOptionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float gap = 4;
            float half = (position.width - gap) * 0.5f;
            var nameRect = new Rect(position.x, position.y, half, position.height);
            var toggleRect = new Rect(position.x + half + gap, position.y, half, position.height);

            EnumOptionDrawerUtil.DrawOptionName(nameRect, property, property.FindPropertyRelative(nameof(ConfigGuiEnumSwitch.Option.m_option)));
            EditorGUI.PropertyField(toggleRect, property.FindPropertyRelative(nameof(ConfigGuiEnumSwitch.Option.m_toggle)), GUIContent.none);
        }
    }

    /// <summary>An option of a slider: a popup of the names of the enum.</summary>
    [CustomPropertyDrawer(typeof(ConfigGuiEnumSwitchSlider.Option))]
    internal class ConfigGuiEnumSwitchSliderOptionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EnumOptionDrawerUtil.DrawOptionName(position, property, property.FindPropertyRelative(nameof(ConfigGuiEnumSwitchSlider.Option.m_option)));
        }
    }
}
