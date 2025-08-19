#nullable enable
using System.Globalization;
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class BoolConfigField : ConfigField<bool>
    {
        public override string GetStringValue() => Value.ToString(CultureInfo.InvariantCulture);

        public override bool TryParse(string stringValue, out bool value) => bool.TryParse(stringValue, out value);
        public override void OnConfigGUI(Rect rect) => Value = GUI.Toggle(rect, Value, "");
    }
}