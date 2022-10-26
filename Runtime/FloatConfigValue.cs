using UnityEngine;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/FloatValue")]
    public class FloatConfigValue : ConfigValue<float>, IConfigValue<int>
    {
        int IConfigValue<int>.Value
        {
            get
            {
                return Mathf.FloorToInt(Value);
            }
            set
            {
                Value = value;
            }
        }

        protected override string GetStringValue()
        {
            return Value.ToString();
        }

        protected override void SetStringValue(string value)
        {
            Value = float.Parse(value);
        }

        override public void OnConfigGUI(Rect rect)
        {
            if (float.TryParse(GUI.TextField(rect, Value.ToString()), out float newValue))
                Value = newValue;
        }
    }
}