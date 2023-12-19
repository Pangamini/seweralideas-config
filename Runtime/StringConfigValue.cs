using UnityEngine;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/StringValue")]
    public class StringConfigValue : ConfigValue<string>
    {
        protected override string GetStringValue()
        {
            return Value;
        }

        protected override void SetStringValue(string value)
        {
            Value = value;
        }


        public override void OnConfigGUI(Rect rect)
        {
            Value = GUI.TextArea(rect, Value);
        }

        public override float GetGUIHeight()
        {
            return 24 * 8;
        }
    }
}