#nullable enable
using System.Globalization;
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

#if SEWERALIDEAS_CONFIG_IMGUI
        public override void OnConfigGUI(Rect rect)
        {
            if (TryParse(DelayedText.Field(rect, Value.ToString(CultureInfo.InvariantCulture)), out int newValue))
                Value = newValue;
        }
#endif
    }
}