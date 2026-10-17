#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeweralIdeas.Config
{
    /// <summary>
    /// A field holding the value of an enum, of any enum type: the type is a setting of the field (see <see cref="EnumType"/>),
    /// so there is no class to write per enum. The value is stored as a string, the name of the option (a <c>[Flags]</c> enum
    /// as the names joined by ", "), and never as its number: renumbering the enum does not change what was saved.
    /// </summary>
    /// <remarks>
    /// It is a <see cref="StringConfigField"/>, so everything that takes a string field takes this one (the storages, the string
    /// reader, the GUI switches). Strings that are not an option of the enum are not accepted. With no type set (or one that
    /// can't be found any more) it accepts any string, so no value is lost.
    /// </remarks>
    public class EnumConfigField : StringConfigField
    {
        [SerializeField] private string m_enumTypeName = string.Empty;

        [NonSerialized] private Type?     m_enumType;
        [NonSerialized] private bool      m_typeResolved;
        [NonSerialized] private string[]? m_names;
        [NonSerialized] private bool      m_isFlags;

        /// <summary>The type as stored, <c>"Namespace.Outer+Nested, Assembly"</c> (see <see cref="FormatTypeName"/>); empty if none is set.</summary>
        public string EnumTypeName => m_enumTypeName;

        /// <summary>The enum, or null if no type is set or it can't be found.</summary>
        public Type? EnumType
        {
            get
            {
                if(!m_typeResolved)
                {
                    m_enumType     = ResolveType(m_enumTypeName);
                    m_names        = m_enumType != null ? Enum.GetNames(m_enumType) : Array.Empty<string>();
                    m_isFlags      = m_enumType != null && m_enumType.IsDefined(typeof(FlagsAttribute), false);
                    m_typeResolved = true;
                }
                return m_enumType;
            }
        }

        /// <summary>Whether the enum is a <c>[Flags]</c> one: the value is any combination of its options.</summary>
        public bool IsFlags
        {
            get
            {
                _ = EnumType;
                return m_isFlags;
            }
        }

        /// <summary>The names of the options of the enum, in the order of their declaration (empty without a type).</summary>
        public IReadOnlyList<string> Names
        {
            get
            {
                _ = EnumType;
                return m_names!;
            }
        }

        /// <summary>Sets the enum of the field. The value is kept as it is, even if it is not an option of the new type.</summary>
        public void SetEnumType(Type? type)
        {
            if(type != null && !type.IsEnum)
                throw new ArgumentException($"{type} is not an enum", nameof(type));

            m_enumTypeName = type != null ? FormatTypeName(type) : string.Empty;
            m_typeResolved = false;
        }

        protected void OnValidate() => m_typeResolved = false;

        internal override void ResetRuntimeState()
        {
            base.ResetRuntimeState();
            m_typeResolved = false;
        }

        public override void SetDefaultValue()
        {
            base.SetDefaultValue();

            // Whatever the way the default was written, the value is the canonical one
            if(Value != null && EnumType != null && TryParse(Value, out string canonical))
                Value = canonical;
        }

        public override bool TryParse(string stringValue, out string value)
        {
            Type? type = EnumType;
            if(type == null)
                return base.TryParse(stringValue, out value);

            if(TryParseValue(type, stringValue, out object parsed))
            {
                value = parsed.ToString();
                return true;
            }

            value = stringValue;
            return false;
        }

        // ---- Typed access -----------------------------------------------------------------------------------------------

        /// <summary>The value as <typeparamref name="TEnum"/>. False if the field is of another enum, or has no valid value.</summary>
        public bool TryGetValue<TEnum>(out TEnum value) where TEnum : struct, Enum
        {
            value = default;
            if(EnumType != typeof(TEnum) || Value == null)
                return false;
            return Enum.TryParse(Value, true, out value);
        }

        public TEnum GetValue<TEnum>(TEnum fallback = default) where TEnum : struct, Enum
            => TryGetValue(out TEnum value) ? value : fallback;

        /// <summary>Sets the value. False (and an error in the log) if the field is of another enum.</summary>
        public bool SetValue<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            if(EnumType != typeof(TEnum))
            {
                Debug.LogError($"{name} holds {(EnumType?.Name ?? "no enum")}, not {typeof(TEnum).Name}", this);
                return false;
            }

            Value = value.ToString();
            return true;
        }

        /// <summary>
        /// Calls <paramref name="callback"/> with the value, as <typeparamref name="TEnum"/>, every time it changes (and now, unless
        /// <paramref name="invokeNow"/> is false). Disposing the result stops it. Returns a handle that does nothing if the field is of another enum.
        /// </summary>
        public Subscription Subscribe<TEnum>(Action<TEnum> callback, bool invokeNow = true) where TEnum : struct, Enum
        {
            if(EnumType != typeof(TEnum))
            {
                Debug.LogError($"{name} holds {(EnumType?.Name ?? "no enum")}, not {typeof(TEnum).Name}", this);
                return default;
            }

            void Handler(string text)
            {
                if(Enum.TryParse(text, true, out TEnum value))
                    callback(value);
            }

            ValueChanged += Handler;
            if(invokeNow && Value != null)
                Handler(Value);
            return new Subscription(this, Handler);
        }

        public readonly struct Subscription : IDisposable
        {
            private readonly EnumConfigField?  m_field;
            private readonly Action<string>?   m_handler;

            internal Subscription(EnumConfigField field, Action<string> handler)
            {
                m_field   = field;
                m_handler = handler;
            }

            public void Dispose()
            {
                if(m_field != null)
                    m_field.ValueChanged -= m_handler;
            }
        }

        // ---- Helpers, also for the readers and the editors ---------------------------------------------------------------

        /// <summary>The way a type is stored: its full name and the name of its assembly.</summary>
        public static string FormatTypeName(Type type) => type.FullName + ", " + type.Assembly.GetName().Name;

        /// <summary>Finds an enum by the name stored by <see cref="FormatTypeName"/>. Falls back to the full name alone, in any loaded assembly, if the assembly changed.</summary>
        public static Type? ResolveType(string? typeName)
        {
            if(string.IsNullOrWhiteSpace(typeName))
                return null;

            Type? type = Type.GetType(typeName, false);
            if(type == null)
            {
                int comma = typeName.IndexOf(',');
                string fullName = (comma >= 0 ? typeName.Substring(0, comma) : typeName).Trim();
                foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(fullName, false);
                    if(type != null)
                        break;
                }
            }

            return type is { IsEnum: true } ? type : null;
        }

        /// <summary>
        /// Reads an option name (a <c>[Flags]</c> enum: names joined by commas; or 0), ignoring the case. Numbers and names that are not
        /// options are not accepted. The result is a value of <paramref name="enumType"/>; its <c>ToString()</c> is the canonical string.
        /// </summary>
        public static bool TryParseValue(Type enumType, string? text, out object value)
        {
            value = null!;
            if(string.IsNullOrWhiteSpace(text))
                return false;

            string[] names = Enum.GetNames(enumType);
            bool flags = enumType.IsDefined(typeof(FlagsAttribute), false);
            string[] parts = flags ? text.Split(',') : new[] { text };

            var canonical = new List<string>(parts.Length);
            foreach(string rawPart in parts)
            {
                string part = rawPart.Trim();
                if(part.Length == 0)
                    return false;

                if(flags && parts.Length == 1 && part == "0")
                {
                    value = Enum.ToObject(enumType, 0);
                    return true;
                }

                int index = Array.IndexOf(names, part);
                if(index < 0)
                    index = Array.FindIndex(names, name => string.Equals(name, part, StringComparison.OrdinalIgnoreCase));
                if(index < 0)
                    return false;

                canonical.Add(names[index]);
            }

            value = Enum.Parse(enumType, string.Join(", ", canonical));
            return true;
        }

        /// <summary>Whether the option is on in the value: equal to it, and, of a <c>[Flags]</c> enum, contained in it (the option 0, when the value is 0).</summary>
        public static bool IsActive(Type enumType, object current, object option)
        {
            if(!enumType.IsDefined(typeof(FlagsAttribute), false))
                return current.Equals(option);

            object zero = Enum.ToObject(enumType, 0);
            if(option.Equals(zero))
                return current.Equals(zero);
            return ((Enum)current).HasFlag((Enum)option);
        }

        /// <summary>Of a <c>[Flags]</c> value, turns the option on or off (the flags it consists of).</summary>
        public static object SetFlag(Type enumType, object current, object option, bool on)
        {
            long currentBits = ToBits(enumType, current);
            long optionBits = ToBits(enumType, option);
            return Enum.ToObject(enumType, on ? currentBits | optionBits : currentBits & ~optionBits);
        }

        private static long ToBits(Type enumType, object value)
            => Enum.GetUnderlyingType(enumType) == typeof(ulong) ? unchecked((long)Convert.ToUInt64(value)) : Convert.ToInt64(value);

#if SEWERALIDEAS_CONFIG_IMGUI
        public override float GetGUIHeight()
        {
            if(EnumType == null)
                return base.GetGUIHeight();
            return 24 * Mathf.Max(1, CountOptions(EnumType, IsFlags));
        }

        public override float GetRowHeight() => EnumType == null ? base.GetRowHeight() : 24;

        // A row of a list is one line: the options can't be listed there. A click on the button moves on to the next one;
        // the options of flags can't be toggled there, only seen.
        public override void OnConfigRowGUI(Rect rect)
        {
            Type? type = EnumType;
            if(type == null)
            {
                base.OnConfigRowGUI(rect);
                return;
            }

            string current = Value ?? string.Empty;
            if(IsFlags)
            {
                GUI.Label(rect, current);
                return;
            }

            IReadOnlyList<string> names = Names;
            if(names.Count > 0 && GUI.Button(rect, current))
            {
                int index = -1;
                for(int i = 0; i < names.Count; i++)
                {
                    if(names[i] == current)
                        index = i;
                }
                Value = names[(index + 1) % names.Count];
            }
        }

        // A list of the options: toggles for flags, any number of them on; buttons otherwise, the current one pressed
        public override void OnConfigGUI(Rect rect)
        {
            Type? type = EnumType;
            if(type == null)
            {
                base.OnConfigGUI(rect);
                return;
            }

            bool flags = IsFlags;
            object zero = Enum.ToObject(type, 0);
            object current = TryParseValue(type, Value, out object parsed) ? parsed : zero;
            bool valid = flags || parsed != null;

            Rect line = new(rect.x, rect.y, rect.width, 24);
            foreach(string optionName in Names)
            {
                object option = Enum.Parse(type, optionName);
                if(flags && option.Equals(zero))
                    continue;

                bool on = (flags || valid) && IsActive(type, current, option);
                if(flags)
                {
                    bool newOn = GUI.Toggle(line, on, optionName);
                    if(newOn != on)
                        Value = SetFlag(type, current, option, newOn).ToString();
                }
                else if(GUI.Toggle(line, on, optionName, GUI.skin.button) && !on)
                {
                    Value = optionName;
                }
                line.y += 24;
            }
        }

        private static int CountOptions(Type? type, bool flags)
        {
            if(type == null)
                return 0;

            if(!flags)
                return Enum.GetNames(type).Length;

            int count = 0;
            object zero = Enum.ToObject(type, 0);
            foreach(string optionName in Enum.GetNames(type))
            {
                if(!Enum.Parse(type, optionName).Equals(zero))
                    count++;
            }
            return count;
        }
#endif
    }
}
