#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace SeweralIdeas.ConfigGui
{
    public class ConfigGuiToggle : ConfigGuiElement<bool>
    {
        [SerializeField] private Toggle m_toggle = default!;
        
        protected override void OnFieldValueChanged(bool value)
        {
            m_toggle.isOn = value;
        }

        protected void Awake()
        {
            m_toggle.onValueChanged.AddListener(OnGuiValueChanged);
        }

        protected void OnDestroy()
        {
            m_toggle.onValueChanged.RemoveListener(OnGuiValueChanged);
        }
    }
}
