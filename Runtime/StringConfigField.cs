#nullable enable
using SeweralIdeas.UnityUtils;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class StringConfigField : ConfigField<string>
    {
        public override string GetStringValue() => Value;

        public override void OnConfigGUI(Rect rect)
        {
            Value = SeweralGUI.DelayedTextArea(rect, Value);
        }

        public override float GetGUIHeight()
        {
            return 24 * 8;
        }


        private static readonly char[] LineBreaks = { '\n', '\r' };

        // A row of a list is one line: the text can be edited there if it is one line itself, and, if it has several,
        // only its first line is shown, to be edited in the text area of the field's own inspector.
        public override float GetRowHeight() => 24;

        public override void OnConfigRowGUI(Rect rect)
        {
            string text = Value ?? string.Empty;
            int lineBreak = text.IndexOfAny(LineBreaks);
            if(lineBreak >= 0)
            {
                GUI.Label(rect, new GUIContent(text.Substring(0, lineBreak) + " ...", text));
                return;
            }

            Value = SeweralGUI.DelayedTextField(rect, text);
        }

        public override bool TryParse(string stringValue, out string value)
        {
            value = stringValue;
            return true;
        }
    }
}