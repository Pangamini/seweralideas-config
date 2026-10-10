#nullable enable
using System.Text;
using SeweralIdeas.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    public class ConfigGuiIntSlider : ConfigGuiTextElement<int, IntConfigField>
    {
        [SerializeField] private Slider m_slider = default!;

        private protected override StringBuilder BuildText(ValueFormat format, int value) => format.Build(value);

        protected override void OnFieldValueChanged(int value)
        {
            m_slider.value = value;
            TryUpdateText();
        }

        protected void Awake()
        {
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
