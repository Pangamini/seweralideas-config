using UnityEngine;

namespace SeweralIdeas.Config
{
    public class StringConfigField : ConfigField<string>
    {
        public override string GetStringValue()
        {
            return Value;
        }

        public override bool SetStringValue(string value)
        {
            if(value == null)
                return false;
            
            Value = value;
            return true;
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