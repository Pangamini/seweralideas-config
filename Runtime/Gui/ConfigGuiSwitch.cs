using System;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    public class ConfigGuiSwitch : ConfigGuiElement<string, StringConfigField>
    {
        [SerializeField] private Option[] m_options = Array.Empty<Option>();
        
        [Serializable]
        public struct Option
        {
            public string m_value;
            public Toggle m_toggle;
        }

        protected void Start()
        {
            var group = gameObject.AddComponent<ToggleGroup>();
            group.allowSwitchOff = false;
            foreach (var option in m_options)
                option.m_toggle.group = group;
        }

        private UnityAction<bool>[] m_listeners; // made on the first enable, so the same ones can be removed again

        protected override void OnEnable()
        {
            base.OnEnable();

            if(m_listeners == null)
            {
                m_listeners = new UnityAction<bool>[m_options.Length];
                for(int i = 0; i < m_options.Length; i++)
                {
                    Option option = m_options[i];
                    m_listeners[i] = isOn =>
                    {
                        if(isOn)
                            OnToggle(option);
                    };
                }
            }

            for(int i = 0; i < m_options.Length; i++)
                m_options[i].m_toggle.onValueChanged.AddListener(m_listeners[i]);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if(m_listeners == null)
                return;

            for(int i = 0; i < m_options.Length; i++)
                m_options[i].m_toggle.onValueChanged.RemoveListener(m_listeners[i]);
        }
        
        private void OnToggle(Option option) => OnGuiValueChanged(option.m_value);

        protected override void OnFieldValueChanged(string value)
        {
            foreach (var option in m_options)
            {
                bool equals = string.Equals(option.m_value, value, StringComparison.Ordinal);
                option.m_toggle.SetIsOnWithoutNotify(equals);
            }
        }
    }
}