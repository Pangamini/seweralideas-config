#nullable enable
using System.Globalization;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    public class ConfigGuiIntSlider : ConfigGuiElement<int, IntConfigField>
    {
        [SerializeField] private Slider             m_slider       = default!;
        [SerializeField] private string             m_numberFormat = "{0:F2}";
        [SerializeField] private UnityEvent<string> m_textOutput   = new();
        
        private Utils.CachedStringFormatter<int> m_stringFormatter;
        
        protected override void OnFieldValueChanged(int value)
        {
            m_slider.value = value;
            TryUpdateText();
        }
        
        private void TryUpdateText()
        {
            if(Field == null || m_textOutput.GetPersistentEventCount() <= 0)
                return;
            
            string text = m_stringFormatter.GetString(Field.Value);
            m_textOutput.Invoke(text);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_stringFormatter = new (m_numberFormat, CultureInfo.InvariantCulture);
            if(RegisteredField != null)
                TryUpdateText();
        }
        
        protected void Awake()
        {
            m_stringFormatter = new (m_numberFormat, CultureInfo.InvariantCulture);
            m_slider.onValueChanged.AddListener(OnSliderValueChanged);
            TryUpdateText();
        }

        protected void OnDestroy()
        {
            m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float val) => OnGuiValueChanged(Mathf.RoundToInt(val));
    }
}