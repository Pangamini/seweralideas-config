#nullable enable
using System;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    /// <summary>
    /// A slider that goes through the options of an enum, in the order they are listed here (chosen in the inspector from the names of
    /// the field's enum). For an enum that is not a <c>[Flags]</c> one.
    /// </summary>
    public class ConfigGuiEnumSwitchSlider : ConfigGuiElement<string, EnumConfigField>
    {
        [SerializeField] private Option[]    m_options = Array.Empty<Option>();
        [SerializeField] private Slider      m_slider = default!;
        [SerializeField] private GameObject? m_activeWhenValidValue;

        [Serializable]
        public struct Option
        {
            public string m_option; // the name of the option of the enum
        }

        protected override void OnFieldValueChanged(string value)
        {
            int index = Array.FindIndex(m_options, option => string.Equals(option.m_option, value, StringComparison.Ordinal));
            m_slider.SetValueWithoutNotify(index);

            if(m_activeWhenValidValue)
                m_activeWhenValidValue!.SetActive(index >= 0);
        }

        protected void Awake()
        {
            m_slider.onValueChanged.AddListener(OnSliderValueChanged);
            m_slider.wholeNumbers = true;
            m_slider.minValue = 0;
            m_slider.maxValue = Mathf.Max(m_options.Length - 1, 0);
        }

        protected void OnDestroy()
        {
            m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if(Field != null && Field.IsFlags)
                Debug.LogWarning($"{Field.name} is a flags enum: a slider can't set several options", this);
        }

        private void OnSliderValueChanged(float sliderValue)
        {
            int index = Mathf.RoundToInt(sliderValue);
            if(index >= 0 && index < m_options.Length)
                OnGuiValueChanged(m_options[index].m_option);
        }
    }
}
