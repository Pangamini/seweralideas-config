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

        public override bool SetStringValue(string value)
        {
            if(!int.TryParse(value, out int intValue))
                return false;
            
            Value = intValue;
            return true;
        }

        public override void OnConfigGUI(Rect rect)
        {
            if (int.TryParse(SeweralGUI.DelayedTextField(rect, Value.ToString(CultureInfo.InvariantCulture)), out int newValue))
                Value = newValue;
        }
    }
}