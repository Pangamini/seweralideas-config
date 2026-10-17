#nullable enable
using UnityEditor;
using UnityEngine;

namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(ConfigField), true)]
    public class ConfigFieldEditor : UnityEditor.Editor
    {
        protected static readonly string[] Exclude = {"m_Script", "m_config"};

        /// <summary>Draws the serialized properties of the field (the default value, say); the value itself is drawn below by the field.</summary>
        protected virtual void DrawFields() => DrawPropertiesExcluding(serializedObject, Exclude);

        public override void OnInspectorGUI()
        {
            ConfigField field = (ConfigField)target;

            // Applied here, not by whoever draws this editor: selected on its own in the Project window, nobody else would,
            // and the edits to the serialized properties (the default value, say) would never reach the asset.
            serializedObject.Update();
            DrawFields();
            serializedObject.ApplyModifiedProperties();

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