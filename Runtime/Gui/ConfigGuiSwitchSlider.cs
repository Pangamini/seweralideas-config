#nullable enable
using System;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    public class ConfigGuiSwitchSlider : ConfigGuiElement<string, StringConfigField>
    {
        [SerializeField] private Option[]   m_options;
        [SerializeField] private Slider     m_slider = default!;
        [SerializeField] private GameObject m_activeWhenValidValue;
        
        [Serializable]
        public struct Option
        {
            public string Value;
        }
        
        protected override void OnFieldValueChanged(string value)
        {
            int index = Array.FindIndex(m_options, option => string.Equals(option.Value, value, StringComparison.Ordinal));
            m_slider.value = index;
            
            if(m_activeWhenValidValue)
                m_activeWhenValidValue.SetActive(index >= 0);
        }
        
        protected void Awake()
        {
            m_slider.onValueChanged.AddListener(OnSliderValueChanged);
            m_slider.wholeNumbers = true;
            m_slider.minValue = 0;
            m_slider.maxValue = m_options.Length - 1;
        }

        protected void OnDestroy()
        {
            m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float sliderValue)
        {
            int value = Mathf.RoundToInt(sliderValue);
            string strVal = value >= 0 && value < m_options.Length ? m_options[value].Value : string.Empty;
            OnGuiValueChanged(strVal);
        }
    }
}