using UnityEngine;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/BoolValue")]
    public class BoolConfigValue : ConfigValue<bool>
    {
        protected override string GetStringValue()
        {
            return Value.ToString();
        }

        protected override void SetStringValue(string value)
        {
            Value = bool.Parse(value);
        }


        override public void OnConfigGUI(Rect rect)
        {
            Value = GUI.Toggle(rect, Value, "");
        }
    }
}