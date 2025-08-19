#nullable enable
using System.Globalization;
using SeweralIdeas.UnityUtils;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class IntConfigField : ConfigField<int>, IConfigValue<float>
    {
        float IConfigValue<float>.Value
        {
            get => Value;
            set => Value = Mathf.FloorToInt(value);
        }


        public override string GetStringValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override bool TryParse(string stringValue, out int value) => int.TryParse(stringValue, out value);

        public override void OnConfigGUI(Rect rect)
        {
            if (TryParse(SeweralGUI.DelayedTextField(rect, Value.ToString(CultureInfo.InvariantCulture)), out int newValue))
                Value = newValue;
        }
    }
}