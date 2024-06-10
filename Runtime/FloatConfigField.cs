using System.Globalization;
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

        public override string GetStringValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override bool SetStringValue(string value)
        {
            if(!float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                return false;
            
            Value = floatValue;
            return true;
        }

        public override void OnConfigGUI(Rect rect)
        {
            string newStrVal = GUI.TextField(rect, Value.ToString(CultureInfo.InvariantCulture));
            if (float.TryParse(newStrVal, NumberStyles.Any, CultureInfo.InvariantCulture, out float newValue))
                Value = newValue;
        }
    }
}