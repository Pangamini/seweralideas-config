#nullable enable
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(ConfigField), true)]
    public class ConfigFieldEditor : UnityEditor.Editor
    {
        private static readonly string[] ExcludeConfig = {"m_Script", "m_config"};
        private static readonly string[] Exclude = {"m_Script"};
        
        public override void OnInspectorGUI()
        {
            ConfigField field = (ConfigField)target;

            if(AssetDatabase.GetAssetPath(target) == AssetDatabase.GetAssetPath(field.Config))
                DrawPropertiesExcluding(serializedObject, ExcludeConfig);
            else
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