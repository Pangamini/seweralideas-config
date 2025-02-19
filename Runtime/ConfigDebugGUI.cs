#nullable enable
using System;
using UnityEngine;

namespace SeweralIdeas.Config
{
    [ExecuteAlways]
    public class ConfigDebugGUI : MonoBehaviour
    {
        [SerializeField] private Config[] m_configs = Array.Empty<Config>();
        [SerializeField] private Rect     m_screenRect;

        private Config? m_selectedConfig;
        private Vector2 m_fieldScrollPos;

        private void Start()
        {
            m_selectedConfig = m_selectedConfig ? m_selectedConfig : m_configs.Length > 0 ? m_configs[0] : null;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(m_screenRect, name, GUI.skin.window);
            try
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                foreach (var config in m_configs)
                {
                    if (GUILayout.Toggle(m_selectedConfig == config, config.name, GUI.skin.button))
                        m_selectedConfig = config;
                }
                GUILayout.EndHorizontal();

                if (m_selectedConfig != null)
                {
                    ConfigGUI(m_selectedConfig);
                }
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        private void ConfigGUI(Config config)
        {
            m_fieldScrollPos = GUILayout.BeginScrollView(m_fieldScrollPos, GUI.skin.box);
            try
            {
                float labelWidth = 100;
                foreach (var field in config.Fields)
                {
                    var rect = GUILayoutUtility.GetRect(0, field.GetGUIHeight());
                    var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
                    var valueRect = new Rect(labelRect.xMax, rect.y, rect.xMax - labelRect.xMax, rect.height);
                    GUI.Label(labelRect, field.name);
                    field.OnConfigGUI(valueRect);
                }
            }
            finally
            {
                GUILayout.EndScrollView();
            }
        }
    }
}
