#nullable enable
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

    public abstract class ConfigField : ScriptableObject
    {
        [SerializeField] private Config m_config;
        protected void OnChanged() => Changed?.Invoke();
        public event Action? Changed;
        public virtual float GetGUIHeight() => 24;
        public virtual void OnConfigGUI(Rect rect) { }
        public abstract string GetStringValue();
        public abstract bool SetStringValue(string value);
        public abstract void SetDefaultValue();
        public abstract string? StringValue { get; }
        public abstract object? GetValue();
        public abstract bool StringValueEquals(string value);
    }
    
    public abstract class ConfigField<T> : ConfigField, IConfigValue<T>
    {
        [SerializeField] private T m_defaultValue = default!;
        private                  T m_value = default!;
        
        public event Action<T>? ValueChanged;
        
        public override void SetDefaultValue() => Value = m_defaultValue;

        public override string? StringValue => Value?.ToString()??null;

        public abstract bool TryParse(string stringValue, out T value);

        public override bool StringValueEquals(string stringValue)
        {
            if (!TryParse(stringValue, out var value))
                return false;
            return ValueEquals(value, m_value);
        }

        public sealed override bool SetStringValue(string value)
        {
            if(!TryParse(value, out var boolValue))
                return false;
            
            Value = boolValue;
            return true;
        }
        
        public T Value
        {
            get => m_value;
            set
            {
                if (m_value == null)
                {
                    if (value == null)
                        return;
                }
                else
                {
                    if (ValueEquals(value, m_value))
                        return;
                }
                
                m_value = value;
                ValueChanged?.Invoke(m_value);
                OnChanged();
            }
        }

        public bool ValueEquals(T lhs, T rhs) => EqualityComparer<T>.Default.Equals(lhs, rhs);

        public override object? GetValue() => Value;
    }
}