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
        protected void OnChanged() => Changed?.Invoke();
        public event Action? Changed;
        public virtual float GetGUIHeight() => 24;
        public virtual void OnConfigGUI(Rect rect) { }
        public abstract string GetStringValue();
        public abstract bool SetStringValue(string value);
        public abstract void SetDefaultValue();
        public abstract string? StringValue { get; }
        public abstract object? GetValue();
    }
    
    public abstract class ConfigField<T> : ConfigField, IConfigValue<T>
    {
        [SerializeField] private T m_defaultValue = default!;
        private                  T m_value = default!;
        
        public event Action<T>? ValueChanged;
        
        public override void SetDefaultValue() => Value = m_defaultValue;

        public override string? StringValue => Value?.ToString()??null;

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
                    if (EqualityComparer<T>.Default.Equals(m_value, value!))
                        return;
                }
                
                m_value = value;
                ValueChanged?.Invoke(m_value);
                OnChanged();
            }
        }

        public override object? GetValue() => Value;
    }
}