using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SeweralIdeas.Config;
namespace Aop.Core.Config
{
    public class ConfigUiSlider : MonoBehaviour
    {
        [SerializeField] private Slider m_slider;
        [SerializeField] private FloatConfigValue m_configValue;
        private bool m_skipCallbacks = false;

        private void OnEnable()
        {
            m_slider.value = m_configValue.Value;
            m_slider.onValueChanged.AddListener(OnSliderChanged);
            m_configValue.onValueChanged += OnConfigChanged;
        }

        private void OnDisable()
        {
            m_slider.onValueChanged.RemoveListener(OnSliderChanged);
            m_configValue.onValueChanged -= OnConfigChanged;
        }

        private void OnConfigChanged(float newValue)
        {
            if (m_skipCallbacks)
                return;
            m_skipCallbacks = true;
            m_slider.value = newValue;
            m_skipCallbacks = false;
        }

        private void OnSliderChanged(float newValue)
        {
            if (m_skipCallbacks)
                return;
            m_skipCallbacks = true;
            m_configValue.Value = newValue;
            m_skipCallbacks = false;
        }
    }

}