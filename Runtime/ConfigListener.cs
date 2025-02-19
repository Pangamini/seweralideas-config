#nullable enable
using UnityEngine;
using UnityEngine.Serialization;

namespace SeweralIdeas.Config
{
    public abstract class ConfigListener<T, TVal> : MonoBehaviour
        where T : System.IEquatable<T> 
        where TVal: ConfigField<T>
    {

        [FormerlySerializedAs("m_configValue")]
        [SerializeField] private TVal? m_configField;
        
        protected virtual void Start()
        {
            if(m_configField == null)
                return;
            
            FieldChanged(m_configField.Value);
            m_configField.ValueChanged += FieldChanged;
        }

        protected virtual void OnDestroy()
        {
            if (m_configField != null)
            {
                m_configField.ValueChanged -= FieldChanged;
            }
        }

        protected abstract void FieldChanged(T value);
    }
}