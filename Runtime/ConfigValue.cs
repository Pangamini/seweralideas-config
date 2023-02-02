using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public interface IConfigValue<T>
    {
        T Value { get; set; }
        event System.Action onChanged;
    }

    public abstract class ConfigValue : ScriptableObject
    {
        private static HashSet<ConfigValue> s_allConfigValues = new HashSet<ConfigValue>();
        private string m_key;
        protected bool m_dirty;

        protected abstract string GetStringValue();
        protected abstract void SetStringValue(string value);
        public abstract void SetDefaultValue();

        public event System.Action onChanged;

        protected void OnEnable()
        {
            m_key = name;
            /// right now using PlayerPrefs, maybe later use some custom config file or something
            var str = PlayerPrefs.GetString(m_key);
            if (string.IsNullOrEmpty(str))
                SetDefaultValue();
            else
            {
                SetStringValue(str);
                m_dirty = false;
            }
            s_allConfigValues.Add(this);
        }

        protected void OnDisable()
        {
            Save();
            s_allConfigValues.Remove(this);
        }

        public static void SaveAll()
        {
            foreach (var val in s_allConfigValues)
                val.Save();
        }

        public void Save()
        {
            if (m_dirty)
            {
                m_dirty = false;
                var str = GetStringValue();
                PlayerPrefs.SetString(m_key, str);
                //Debug.Log($"Saving {m_key}: {str}");
            }
        }

        private void OnValidate()
        {
            m_key = name;
        }

        public virtual float GetGUIHeight()
        {
            return 24;
        }

        virtual public void OnConfigGUI(Rect rect)
        {
        }

        protected void OnChanged()
        {
            onChanged?.Invoke();
        }

        public abstract string StringValue { get; }
        public abstract object GetValue();
    }

    public abstract class ConfigValue<T> : ConfigValue, IConfigValue<T>
    {
        private T m_value;
        [SerializeField] private T m_defaultValue;
        [SerializeField] private T m_defaultValueMobile;
        public event System.Action<T> onValueChanged;
        
        public override void SetDefaultValue()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Value = m_defaultValue;
#else
            Value = m_defaultValueMobile;
#endif
        }

        public override string StringValue => Value.ToString();

        public T Value
        {
            get { return m_value; }
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
                m_dirty = true;
                onValueChanged?.Invoke(m_value);
                OnChanged();
            }
        }

        public override object GetValue() => Value;
    }
}