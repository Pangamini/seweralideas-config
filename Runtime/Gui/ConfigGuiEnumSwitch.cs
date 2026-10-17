#nullable enable
using System;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    /// <summary>
    /// A toggle for each option of an enum, chosen in the inspector from the names of the field's enum. One option is on at a time;
    /// of a <c>[Flags]</c> enum, each toggle is a flag, on when the value has it, and any number of them can be on.
    /// </summary>
    public class ConfigGuiEnumSwitch : ConfigGuiElement<string, EnumConfigField>
    {
        [SerializeField] private Option[] m_options = Array.Empty<Option>();

        [Serializable]
        public struct Option
        {
            public string m_option; // the name of the option of the enum
            public Toggle? m_toggle;
        }

        private UnityAction<bool>[]? m_listeners;
        private ToggleGroup?         m_group;

        protected override void OnEnable()
        {
            if(m_listeners == null)
            {
                m_listeners = new UnityAction<bool>[m_options.Length];
                for(int i = 0; i < m_options.Length; i++)
                {
                    Option option = m_options[i];
                    m_listeners[i] = isOn => OnToggle(option, isOn);
                }
            }

            // One at a time, unless the options are flags
            bool flags = Field != null && Field.IsFlags;
            if(!flags && m_group == null)
            {
                m_group = gameObject.AddComponent<ToggleGroup>();
                m_group.allowSwitchOff = false;
            }

            for(int i = 0; i < m_options.Length; i++)
            {
                Toggle? toggle = m_options[i].m_toggle;
                if(toggle == null)
                    continue;

                if(!flags)
                    toggle.group = m_group;
                toggle.onValueChanged.AddListener(m_listeners[i]);
            }

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if(m_listeners == null)
                return;

            for(int i = 0; i < m_options.Length; i++)
            {
                Toggle? toggle = m_options[i].m_toggle;
                if(toggle != null)
                    toggle.onValueChanged.RemoveListener(m_listeners[i]);
            }
        }

        private void OnToggle(Option option, bool isOn)
        {
            EnumConfigField? field = Field;
            Type? type = field?.EnumType;
            if(field == null || type == null || !EnumConfigField.TryParseValue(type, option.m_option, out object optionValue))
                return;

            if(!field.IsFlags)
            {
                if(isOn)
                    OnGuiValueChanged(optionValue.ToString());
                return;
            }

            object current = EnumConfigField.TryParseValue(type, field.Value, out object parsed) ? parsed : Enum.ToObject(type, 0);
            OnGuiValueChanged(EnumConfigField.SetFlag(type, current, optionValue, isOn).ToString());
        }

        protected override void OnFieldValueChanged(string value)
        {
            Type? type = RegisteredField != null ? RegisteredField.EnumType : null;
            object? parsed = null;
            if(type != null && EnumConfigField.TryParseValue(type, value, out object parsedValue))
                parsed = parsedValue;

            foreach(Option option in m_options)
            {
                if(option.m_toggle == null)
                    continue;

                bool on = type != null
                          && parsed != null
                          && EnumConfigField.TryParseValue(type, option.m_option, out object optionValue)
                          && EnumConfigField.IsActive(type, parsed, optionValue);
                option.m_toggle.SetIsOnWithoutNotify(on);
            }
        }
    }
}
