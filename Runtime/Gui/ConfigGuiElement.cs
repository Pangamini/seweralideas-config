#nullable enable
using SeweralIdeas.Config;
using UnityEngine;
namespace SeweralIdeas.ConfigGui
{
    public abstract class ConfigGuiElement : MonoBehaviour
    {
    }
    
    public abstract class ConfigGuiElement<T> : ConfigGuiElement
    {
        [SerializeField] private ConfigField<T>? m_field;
        private                  ConfigField<T>? m_registeredField;
        private                  bool            m_enabled;

        public ConfigField<T>? Field
        {
            get => m_field;
            set
            {
                if (m_field == value)
                    return;
                
                m_field = value;
                if (m_enabled)
                    RegisteredField = m_field;
            }
        }

        private ConfigField<T>? RegisteredField
        {
            get => m_registeredField;
            set
            {
                if (m_registeredField != null)
                    m_registeredField.ValueChanged -= OnFieldValueChanged;
                m_registeredField = value;
                if (m_registeredField != null)
                {
                    m_registeredField.ValueChanged += OnFieldValueChanged;
                    OnFieldValueChanged(m_registeredField.Value);
                }
            }
        }

        protected abstract void OnFieldValueChanged(T value);

        protected void OnGuiValueChanged(T guiValue)
        {
            if(m_field != null)
                m_field.Value = guiValue;
        }
        
        protected void OnEnable()
        {
            m_enabled = true;
            RegisteredField = Field;
        }

        protected void OnDisable()
        {
            RegisteredField = null;
            m_enabled = false;
        }

        protected void OnValidate()
        {
            if (!m_enabled)
                return;
            RegisteredField = m_field;
        }
    }
}
