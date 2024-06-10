using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class AudioConfigReader : MonoBehaviour
    {
        [SerializeField] private FloatConfigField m_configField;
        [SerializeField] public string mixerExposedName;
        [SerializeField] public UnityEngine.Audio.AudioMixer mixer;

        private void Start()
        {
            if (m_configField)
            {
                ValueChanged(m_configField.Value);
                m_configField.ValueChanged += ValueChanged;
            }
        }

        private void OnDestroy()
        {
            if (m_configField)
            {
                m_configField.ValueChanged -= ValueChanged;
            }
        }

        private void ValueChanged(float value)
        {
            float dbValue;
            if (value <= 0)
                dbValue = -80;
            else
                dbValue = Mathf.Log(value) * 20;
            mixer.SetFloat(mixerExposedName, dbValue);
        }
    }
}