#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SeweralIdeas.ConfigGui
{
    /// <summary>
    /// A composite format string for one number ("{0:F2} dB", "{{{0}}}"), parsed once. <see cref="Build(float)"/> writes the
    /// text into a <see cref="StringBuilder"/> that is reused, formatting the number into a stack buffer, so it allocates
    /// nothing: that is what <c>TMP_Text.SetText(StringBuilder)</c> takes. Only the argument <c>{0}</c> exists, and a format
    /// item has no alignment ("{0,5}").
    /// </summary>
    internal sealed class ValueFormat
    {
        private readonly struct Part
        {
            public readonly string? Literal; // null for the number
            public readonly string? Spec;    // the number's format specifier, if any

            public Part(string? literal, string? spec)
            {
                Literal = literal;
                Spec = spec;
            }
        }

        private readonly List<Part>    m_parts = new();
        private readonly CultureInfo   m_culture;
        private readonly StringBuilder m_builder = new();

        public ValueFormat(string format, CultureInfo culture)
        {
            m_culture = culture;
            Parse(format);
        }

        private void Parse(string format)
        {
            var literal = new StringBuilder();

            void FlushLiteral()
            {
                if(literal.Length == 0)
                    return;
                m_parts.Add(new Part(literal.ToString(), null));
                literal.Clear();
            }

            for(int i = 0; i < format.Length; i++)
            {
                char c = format[i];
                if(c == '{')
                {
                    if(i + 1 < format.Length && format[i + 1] == '{')
                    {
                        literal.Append('{');
                        i++;
                        continue;
                    }

                    int end = format.IndexOf('}', i);
                    if(end < 0)
                        throw new FormatException("Input string was not in a correct format.");

                    string item = format.Substring(i + 1, end - i - 1);
                    int colon = item.IndexOf(':');
                    string index = colon < 0 ? item : item.Substring(0, colon);
                    string? spec = colon < 0 ? null : item.Substring(colon + 1);
                    if(!int.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out int argument))
                        throw new FormatException("Input string was not in a correct format.");

                    FlushLiteral();
                    // Only {0} exists; a format item for another argument shows nothing, as it did.
                    if(argument == 0)
                        m_parts.Add(new Part(null, string.IsNullOrEmpty(spec) ? null : spec));
                    i = end;
                }
                else if(c == '}')
                {
                    if(i + 1 < format.Length && format[i + 1] == '}')
                    {
                        literal.Append('}');
                        i++;
                        continue;
                    }
                    throw new FormatException("Input string was not in a correct format.");
                }
                else
                {
                    literal.Append(c);
                }
            }
            FlushLiteral();
        }

        public string Format(float value) => Build(value).ToString();
        public string Format(int value) => Build(value).ToString();

        /// <summary>The text of the value, in a builder that is the same one on every call: use it before the next.</summary>
        public StringBuilder Build(float value)
        {
            m_builder.Clear();
            Span<char> buffer = stackalloc char[64];
            foreach(Part part in m_parts)
            {
                if(part.Literal != null)
                    m_builder.Append(part.Literal);
                else if(value.TryFormat(buffer, out int written, part.Spec, m_culture))
                    m_builder.Append(buffer.Slice(0, written));
                else
                    m_builder.Append(value.ToString(part.Spec, m_culture));
            }
            return m_builder;
        }

        /// <inheritdoc cref="Build(float)"/>
        public StringBuilder Build(int value)
        {
            m_builder.Clear();
            Span<char> buffer = stackalloc char[64];
            foreach(Part part in m_parts)
            {
                if(part.Literal != null)
                    m_builder.Append(part.Literal);
                else if(value.TryFormat(buffer, out int written, part.Spec, m_culture))
                    m_builder.Append(buffer.Slice(0, written));
                else
                    m_builder.Append(value.ToString(part.Spec, m_culture));
            }
            return m_builder;
        }
    }
}
