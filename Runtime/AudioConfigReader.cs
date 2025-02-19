#nullable enable
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace SeweralIdeas.Config
{
    public class AudioConfigReader : MonoBehaviour
    {
        [SerializeField] 
        private FloatConfigField? m_configField;

        [FormerlySerializedAs("mixerExposedName")]
        [SerializeField]
        private string? m_mixerExposedName;

        [FormerlySerializedAs("mixer")]
        [SerializeField]
        private AudioMixer? m_mixer;

        private void Start()
        {
            if(m_configField == null)
                return;
            
            ValueChanged(m_configField.Value);
            m_configField.ValueChanged += ValueChanged;
        }

        private void OnDestroy()
        {
            if(m_configField == null)
                return;
            
            m_configField.ValueChanged -= ValueChanged;
        }

        private void ValueChanged(float value)
        {
            float dbValue;
            if (value <= 0)
                dbValue = -80;
            else
                dbValue = Mathf.Log(value) * 20;
            m_mixer?.SetFloat(m_mixerExposedName, dbValue);
        }
    }
}