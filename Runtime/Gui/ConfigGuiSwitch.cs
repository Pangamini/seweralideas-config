using System;
using UnityEngine;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    public class ConfigGuiSwitch : ConfigGuiElement<string>
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

        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (var option in m_options)
            {
                option.m_toggle.onValueChanged.AddListener( isOn =>
                {
                    if(isOn) 
                        OnToggle(option);
                });
            }
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