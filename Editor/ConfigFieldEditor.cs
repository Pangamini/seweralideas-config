using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(ConfigField), true)]
    public class ConfigFieldEditor : UnityEditor.Editor
    {
        private static readonly GUIContent s_valueLabel = new GUIContent("Value");
        private static readonly string[] s_excludeConfig = {"m_Script", "m_config"};
        private static readonly string[] s_exclude = {"m_Script"};
        
        public override void OnInspectorGUI()
        {
            ConfigField field = (ConfigField)target;

            if(AssetDatabase.GetAssetPath(target) == AssetDatabase.GetAssetPath(field.Config))
                DrawPropertiesExcluding(serializedObject, s_excludeConfig);
            else
                DrawPropertiesExcluding(serializedObject, s_exclude);
            

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