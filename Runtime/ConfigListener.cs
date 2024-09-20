using UnityEngine;

namespace SeweralIdeas.Config
{
    public abstract class ConfigListener<T, TVal> : MonoBehaviour
        where T : System.IEquatable<T> 
        where TVal: ConfigField<T>
    {

        [SerializeField] private TVal m_configValue;
                
        private bool m_started = false;

        public T Value
        {
            get => m_configValue.Value;
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

        protected abstract void ValueChanged(T value);
    }
}