#nullable enable
using SeweralIdeas.UnityUtils;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class StringConfigField : ConfigField<string>
    {
        public override string GetStringValue() => Value;

        public override void OnConfigGUI(Rect rect)
        {
            Value = SeweralGUI.DelayedTextArea(rect, Value);
        }

        public override float GetGUIHeight()
        {
            return 24 * 8;
        }

        public override bool TryParse(string stringValue, out string value)
        {
            value = stringValue;
            return true;
        }
    }
}