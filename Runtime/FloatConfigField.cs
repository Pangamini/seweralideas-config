#nullable enable
using System.Globalization;
using SeweralIdeas.UnityUtils;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class FloatConfigField : ConfigField<float>, IConfigValue<int>
    {
        int IConfigValue<int>.Value
        {
            get => Mathf.FloorToInt(Value);
            set => Value = value;
        }

        public override string GetStringValue() => Value.ToString(CultureInfo.InvariantCulture);

        public override bool TryParse(string stringValue, out float value) => float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

        public override void OnConfigGUI(Rect rect)
        {
            string newStrVal = SeweralGUI.DelayedTextField(rect, Value.ToString(CultureInfo.InvariantCulture));
            if (TryParse(newStrVal, out float newValue))
                Value = newValue;
        }
    }
}