#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    internal static class EnumConfigReaderDrawerUtil
    {
        // The enum of the field the reader listens to. Null while there is none.
        public static Type? GetEnumType(SerializedProperty property)
        {
            SerializedProperty fieldProperty = property.serializedObject.FindProperty("m_configField");
            return (fieldProperty?.objectReferenceValue as EnumConfigField)?.EnumType;
        }
    }

    /// <summary>A target and the member of it to write the enum to: a popup of the members that are of the enum's type.</summary>
    [CustomPropertyDrawer(typeof(EnumConfigReader.Binding))]
    internal class EnumConfigReaderBindingDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty target = property.FindPropertyRelative(nameof(EnumConfigReader.Binding.Target));
            SerializedProperty member = property.FindPropertyRelative(nameof(EnumConfigReader.Binding.Member));

            Rect line = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(line, target, new GUIContent("Target"));
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Type? enumType = EnumConfigReaderDrawerUtil.GetEnumType(property);
            UnityEngine.Object? targetObject = target.objectReferenceValue;
            if(enumType == null || targetObject == null)
            {
                EditorGUI.PropertyField(line, member, new GUIContent("Member"));
                return;
            }

            // A game object was dropped: the member is on one of its components, which becomes the target
            if(targetObject is GameObject gameObject)
            {
                DrawComponentChoice(line, target, gameObject, enumType);
                return;
            }

            List<string> candidates = FindMembers(targetObject.GetType(), enumType);
            int index = candidates.IndexOf(member.stringValue);

            // Only one member fits: no choice to make
            if(index < 0 && candidates.Count == 1)
            {
                member.stringValue = candidates[0];
                index = 0;
            }

            var shown = new List<string>(candidates);
            int offset = 0;
            if(index < 0)
            {
                // What is stored is not one of them: shown first, as the current choice
                shown.Insert(0, string.IsNullOrEmpty(member.stringValue) ? "(none)" : $"{member.stringValue} (not of {enumType.Name})");
                offset = 1;
            }

            int selected = EditorGUI.Popup(line, "Member", index + offset, shown.ToArray());
            if(selected - offset >= 0 && selected - offset != index)
                member.stringValue = candidates[selected - offset];
        }

        // The components of the game object that have a member to write the enum to. If there is just one, it is the target at once;
        // otherwise the component is chosen from a popup.
        private static void DrawComponentChoice(Rect line, SerializedProperty target, GameObject gameObject, Type enumType)
        {
            var components = new List<Component>();
            foreach(Component component in gameObject.GetComponents<Component>())
            {
                if(component != null && FindMembers(component.GetType(), enumType).Count > 0)
                    components.Add(component);
            }

            if(components.Count == 0)
            {
                EditorGUI.LabelField(line, "Component", $"None of {gameObject.name}'s has a member of {enumType.Name}");
                return;
            }

            if(components.Count == 1)
            {
                target.objectReferenceValue = components[0];
                return;
            }

            var names = new string[components.Count + 1];
            names[0] = "(choose a component)";
            for(int i = 0; i < components.Count; i++)
                names[i + 1] = components[i].GetType().Name;

            int selected = EditorGUI.Popup(line, "Component", 0, names);
            if(selected > 0)
                target.objectReferenceValue = components[selected - 1];
        }

        private static List<string> FindMembers(Type targetType, Type enumType)
        {
            var names = new List<string>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for(Type? current = targetType; current != null && current != typeof(UnityEngine.Object); current = current.BaseType)
            {
                foreach(FieldInfo field in current.GetFields(flags))
                {
                    if(field.FieldType == enumType && !names.Contains(field.Name))
                        names.Add(field.Name);
                }

                foreach(PropertyInfo property in current.GetProperties(flags))
                {
                    if(property.PropertyType == enumType && property.CanWrite && !names.Contains(property.Name))
                        names.Add(property.Name);
                }
            }
            return names;
        }
    }

    /// <summary>An option of the enum (a popup of its names) with the event that says whether it is on.</summary>
    [CustomPropertyDrawer(typeof(EnumConfigReader.Option))]
    internal class EnumConfigReaderOptionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty onActive = property.FindPropertyRelative(nameof(EnumConfigReader.Option.OnActive));
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(onActive);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative(nameof(EnumConfigReader.Option.Name));
            SerializedProperty onActive = property.FindPropertyRelative(nameof(EnumConfigReader.Option.OnActive));

            Rect line = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Type? enumType = EnumConfigReaderDrawerUtil.GetEnumType(property);
            if(enumType == null)
            {
                EditorGUI.PropertyField(line, name, new GUIContent("Option"));
            }
            else
            {
                string[] names = Enum.GetNames(enumType);
                int index = Array.IndexOf(names, name.stringValue);
                var shown = new List<string>(names);
                if(index < 0)
                    shown.Insert(0, string.IsNullOrEmpty(name.stringValue) ? "(none)" : $"{name.stringValue} (not an option)");

                int offset = index < 0 ? 1 : 0;
                int selected = EditorGUI.Popup(line, "Option", index + offset, shown.ToArray());
                if(selected - offset >= 0 && selected - offset != index)
                    name.stringValue = names[selected - offset];
            }

            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            line.height = EditorGUI.GetPropertyHeight(onActive);
            EditorGUI.PropertyField(line, onActive, new GUIContent("On Active"));
        }
    }
}
