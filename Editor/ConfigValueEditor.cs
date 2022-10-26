using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SeweralIdeas.Config.Editor
{
    [CustomEditor(typeof(ConfigValue), true)]
    public class ConfigValueEditor : UnityEditor.Editor
    {
        private static GUIContent m_valueLabel = new GUIContent("Value");
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var target = (ConfigValue)this.target;

            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Value:");
                GUILayoutUtility.GetRect(m_valueLabel, "Label");
                var rect = GUILayoutUtility.GetRect(0, target.GetGUIHeight());

                var _changed = GUI.changed;
                GUI.changed = false;
                target.OnConfigGUI(rect);
                if (GUI.changed)
                    target.Save();
                GUI.changed |= _changed;
            }
        }
    }
}