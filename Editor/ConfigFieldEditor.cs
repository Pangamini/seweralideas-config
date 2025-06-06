﻿#nullable enable
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(ConfigField), true)]
    public class ConfigFieldEditor : UnityEditor.Editor
    {
        private static readonly string[] Exclude = {"m_Script", "m_config"};
        
        public override void OnInspectorGUI()
        {
            ConfigField field = (ConfigField)target;
            DrawPropertiesExcluding(serializedObject, Exclude);
            
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Value:");
                //GUILayoutUtility.GetRect(s_valueLabel, "Label");
                var rect = GUILayoutUtility.GetRect(0, field.GetGUIHeight());
                field.OnConfigGUI(rect);
            }
        }
    }
}