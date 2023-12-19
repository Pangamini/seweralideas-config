using System.Globalization;
using UnityEngine;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/IntValue")]
    public class IntConfigValue : ConfigValue<int>, IConfigValue<float>
    {
        float IConfigValue<float>.Value
        {
            get => Value;
            set => Value = Mathf.FloorToInt(value);
        }


        protected override string GetStringValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        protected override void SetStringValue(string value)
        {
            Value = int.Parse(value, CultureInfo.InvariantCulture);
        }

        public override void OnConfigGUI(Rect rect)
        {
            if (int.TryParse(GUI.TextField(rect, Value.ToString(CultureInfo.InvariantCulture)), out int newValue))
                Value = newValue;
        }
    }
}