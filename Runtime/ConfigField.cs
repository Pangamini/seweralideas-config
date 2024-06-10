using System;
using System.Collections.Generic;
using UnityEngine;
namespace SeweralIdeas.Config
{
    public interface IConfigValue<T>
    {
        T Value { get; set; }
        event Action Changed;
    }

    public abstract class ConfigField : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private Config m_config;
        private Config m_registeredTo;
        private bool m_enabled;

        public abstract string GetStringValue();
        public abstract bool SetStringValue(string value);
        public abstract void SetDefaultValue();

        public event Action Changed;

        protected void OnEnable()
        {
            m_registeredTo = Config;
            if(m_registeredTo)
                m_registeredTo.RegisterField(this);
            m_enabled = true;
        }
        
        protected void OnDisable()
        {
            m_enabled = false;
            if(m_registeredTo)
                m_registeredTo.UnregisterField(this);
            m_registeredTo = null;
        }
        
        public virtual float GetGUIHeight() => 24;

        public virtual void OnConfigGUI(Rect rect) { }

        protected void OnChanged() => Changed?.Invoke();

        public abstract string StringValue { get; }
        public Config Config => m_config;

        public abstract object GetValue();
        
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if(!m_enabled)
                return;
            
            if(m_registeredTo == Config)
                return;
            if(m_registeredTo)
                m_registeredTo.UnregisterField(this);
            m_registeredTo = Config;
            if(m_registeredTo)
                m_registeredTo.RegisterField(this);
        }
    }
    

    public abstract class ConfigField<T> : ConfigField, IConfigValue<T>
    {
        private T m_value;
        [SerializeField] private T m_defaultValue;
        public event Action<T> ValueChanged;
        
        public override void SetDefaultValue() => Value = m_defaultValue;

        public override string StringValue => Value.ToString();

        public T Value
        {
            get
            {
                Config.EnsureInitialized();
                return m_value;
            }
            set
            {
                if (m_value == null)
                {
                    if (value == null)
                        return;
                }
                else
                {
                    if (EqualityComparer<T>.Default.Equals(m_value, value))
                        return;
                }

                m_value = value;
                Config.SetFieldsDirty();
                ValueChanged?.Invoke(m_value);
                OnChanged();
            }
        }

        public override object GetValue() => Value;
    }
}