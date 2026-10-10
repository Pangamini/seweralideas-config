#nullable enable
using System.Text;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    public class ConfigGuiSlider : ConfigGuiTextElement<float, FloatConfigField>
    {
        [SerializeField] private Slider m_slider = default!;

        private protected override StringBuilder BuildText(ValueFormat format, float value) => format.Build(value);

        protected override void OnFieldValueChanged(float value)
        {
            m_slider.value = value;
            TryUpdateText();
        }

        protected void Awake()
        {
            m_slider.onValueChanged.AddListener(OnGuiValueChanged);
            TryUpdateText();
        }

        protected void OnDestroy()
        {
            m_slider.onValueChanged.RemoveListener(OnGuiValueChanged);
        }
    }
}
