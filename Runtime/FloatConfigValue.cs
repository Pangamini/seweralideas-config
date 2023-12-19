using System.Globalization;
using UnityEngine;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/FloatValue")]
    public class FloatConfigValue : ConfigValue<float>, IConfigValue<int>
    {
        int IConfigValue<int>.Value
        {
            get => Mathf.FloorToInt(Value);
            set => Value = value;
        }

        protected override string GetStringValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        protected override void SetStringValue(string value)
        {
            Value = float.Parse(value, CultureInfo.InvariantCulture);
        }

        public override void OnConfigGUI(Rect rect)
        {
            if (float.TryParse(GUI.TextField(rect, Value.ToString(CultureInfo.InvariantCulture)), out float newValue))
                Value = newValue;
        }
    }
}