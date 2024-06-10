using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SeweralIdeas.Config
{
    public class ConfigReader<T, TVal> : MonoBehaviour where T : System.IEquatable<T> where TVal: ConfigField<T>
    {
        
        [SerializeField] private TVal m_configValue;
        [SerializeField] private UnityEvent<T> m_onChanged = new UnityEvent<T>();

        public UnityEvent<T> onChanged => m_onChanged;
        private bool m_started = false;

        public T Value
        {
            get { return m_configValue.Value; }
            set
            {
                if(m_started)
                    m_configValue.Value = value;
            }
        }

        protected void Start()
        {
            if (m_configValue)
            {
                ValueChanged(m_configValue.Value);
                m_configValue.ValueChanged += ValueChanged;
            }
            m_started = true;
        }

        protected void OnDestroy()
        {
            if (m_configValue)
            {
                m_configValue.ValueChanged -= ValueChanged;
            }
        }

        protected virtual void ValueChanged(T value)
        {
            onChanged.Invoke(PostprocessValue(value));
        }

        protected virtual T PostprocessValue(T value)
        {
            return value;
        }
    }
}