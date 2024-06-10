using System.Globalization;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class BoolConfigField : ConfigField<bool>
    {
        public override string GetStringValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override bool SetStringValue(string value)
        {
            if(!bool.TryParse(value, out var boolValue))
                return false;
            
            Value = boolValue;
            return true;
        }


        public override void OnConfigGUI(Rect rect)
        {
            Value = GUI.Toggle(rect, Value, "");
        }
    }
}