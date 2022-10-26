using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class AudioConfigReader : MonoBehaviour
    {
        [SerializeField] private FloatConfigValue m_configValue;
        [SerializeField] public string mixerExposedName;
        [SerializeField] public UnityEngine.Audio.AudioMixer mixer;

        private void Start()
        {
            if (m_configValue)
            {
                OnValueChanged(m_configValue.Value);
                m_configValue.onValueChanged += OnValueChanged;
            }
        }

        private void OnDestroy()
        {
            if (m_configValue)
            {
                m_configValue.onValueChanged -= OnValueChanged;
            }
        }

        private void OnValueChanged(float value)
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