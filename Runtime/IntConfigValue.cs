using UnityEngine;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/IntValue")]
    public class IntConfigValue : ConfigValue<int>, IConfigValue<float>
    {
        float IConfigValue<float>.Value
        {
            get
            {
                return Value;
            }
            set
            {
                Value = Mathf.FloorToInt(value);
            }
        }


        protected override string GetStringValue()
        {
            return Value.ToString();
        }

        protected override void SetStringValue(string value)
        {
            Value = int.Parse(value);
        }

        override public void OnConfigGUI(Rect rect)
        {
            if (int.TryParse(GUI.TextField(rect, Value.ToString()), out int newValue))
                Value = newValue;
        }
    }
}