#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;
namespace SeweralIdeas.Config
{
    public abstract class StreamConfigStorage : ConfigStorage
    {
        private const char EscapeCharacter = '\\';
        private static readonly Encoding StreamEncoding = Encoding.UTF8;
        private static readonly Dictionary<char, char> UnescapedChars = new Dictionary<char, char>();
        private static readonly Dictionary<char, char> EscapedChars = new Dictionary<char, char>
        {
            { '=', '=' },
            { ' ', '_' },
            { '\n', 'n' },
            { '\\', '\\' }
        };

        private readonly Dictionary<string, string> m_loadedStringValues = new();

        static StreamConfigStorage()
        {
            foreach(var pair in EscapedChars)
                UnescapedChars.Add(pair.Value, pair.Key);
        }

        public override void LoadField(Config config, string key, ConfigField field)
        {
            if(m_loadedStringValues.TryGetValue(key, out string stringValue))
            {
                field.SetStringValue(stringValue);
                m_loadedStringValues.Remove(key);
                return;
            }
            
            field.SetDefaultValue();
        }

        public sealed override bool PreLoad(Config config)
        {
            m_loadedStringValues.Clear();
            
            using var stream = CreateReadStream(config);
            if(stream == null)
                return false;

            try
            {
                Parse(stream);
                return true;
            }
            catch (Exception e)
            {
                // Damaged data: none of it is used, so every field gets its default, and the next save replaces it.
                m_loadedStringValues.Clear();
                Debug.LogError($"Config {config.GlobalName} could not be read, its fields get their default values: {e.Message}", config);
                return false;
            }
        }

        private void Parse(Stream stream)
        {
            using var reader = new StreamReader(stream, StreamEncoding);
            ParseText(reader, m_loadedStringValues);
        }

        // A line is "key = value". The key and an unquoted value ignore every space except the escaped ones (\_), so the
        // spaces around the = (or a missing one) don't matter. A value that starts with a quote is read as it is, spaces,
        // = and all, up to the closing quote: \" is a quote, \\ a backslash, \n and \r the line breaks, and a backslash
        // before anything else is just a backslash (so a path can be written as it is). It can go over several lines.
        internal static void ParseText(TextReader reader, Dictionary<string, string> values)
        {
            string? key = null;
            StringBuilder sb = new();
            bool escaping = false;
            bool quoted = false;      // inside the quotes of a value
            bool afterQuote = false;  // the closing quote has been read: the rest of the line has to be empty
            bool quotedEscaping = false;

            void FinishLine()
            {
                string value = sb.ToString();
                sb.Clear();
                if(!string.IsNullOrWhiteSpace(key))
                    values.Add(key, value);
                key = null;
                afterQuote = false;
            }

            while(true)
            {
                int val = reader.Read();
                if(val < 0)
                {
                    if(quoted)
                        throw new Exception("A quoted value is not closed");
                    FinishLine();
                    break;
                }

                char ch = (char)val;

                if(quoted)
                {
                    if(quotedEscaping)
                    {
                        quotedEscaping = false;
                        switch(ch)
                        {
                            case '"':  sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case 'n':  sb.Append('\n'); break;
                            case 'r':  sb.Append('\r'); break;
                            default:   sb.Append('\\').Append(ch); break; // verbatim
                        }
                    }
                    else if(ch == '\\')
                        quotedEscaping = true;
                    else if(ch == '"')
                    {
                        quoted = false;
                        afterQuote = true;
                    }
                    else if(ch == '\r' && reader.Peek() == '\n')
                    {
                        // The line break of a file with Windows line endings is a \n, as everywhere else
                    }
                    else
                        sb.Append(ch);
                    continue;
                }

                if(afterQuote)
                {
                    if(ch == '\n')
                        FinishLine();
                    else if(ch != ' ' && ch != '\t' && ch != '\r')
                        throw new Exception($"Unexpected '{ch}' after the closing quote of the value of {key}");
                    continue;
                }

                if(escaping)
                {
                    if(!UnescapedChars.TryGetValue(ch, out var unescaped))
                        throw new Exception($"Unknown escape '{ch}'");
                    sb.Append(unescaped);
                    escaping = false;
                    continue;
                }
                if(ch == EscapeCharacter)
                {
                    escaping = true;
                    continue;
                }

                switch (ch)
                {
                    case '\n':
                        FinishLine();
                        break;
                    case ' ':
                    case '\r':
                        break;
                    case '=' when key == null: // a second one is part of the value
                        key = sb.ToString();
                        sb.Clear();
                        break;
                    case '"' when key != null && sb.Length == 0:
                        quoted = true;
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }

            }
        }

        public override sealed bool Save(Config config)
        {
            using var stream = CreateWriteStream(config);
            using var writer = new StreamWriter(stream, StreamEncoding);
            using (ListPool<ConfigField>.Get(out var fields))
            {
                foreach(var pair in config.Fields)
                    if(pair != null) fields.Add(pair); // (an entry left empty by a removed field is skipped)
                
                fields.Sort((lhs, rhs) => String.Compare(lhs.name, rhs.name, StringComparison.Ordinal));
                
                using var enumerator = fields.GetEnumerator();

                void WriteField(ConfigField field)
                {
                    // A string goes in quotes, as it is, so it can have spaces and still be easy to edit by hand
                    WriteEntry(writer, field.name, field.GetStringValue(), field is ConfigField<string>);
                }

                if(enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if(current != null)
                        WriteField(current);
                }

                while(enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if(current == null)
                        continue;

                    writer.Write("\n");
                    WriteField(current);
                }

                return true;
            }
        }

        
        internal static void WriteEntry(TextWriter writer, string key, string? value, bool quote)
        {
            WriteEscaped(writer, key);
            writer.Write(" = ");
            if(!quote)
            {
                WriteEscaped(writer, value ?? string.Empty);
                return;
            }

            writer.Write('"');
            foreach(char ch in value ?? string.Empty)
            {
                switch(ch)
                {
                    case '"':  writer.Write("\\\""); break;
                    case '\\': writer.Write("\\\\"); break;
                    case '\n': writer.Write("\\n"); break;
                    case '\r': writer.Write("\\r"); break;
                    default:   writer.Write(ch); break;
                }
            }
            writer.Write('"');
        }

        private static void WriteEscaped(TextWriter writer, string data)
        {
            foreach (char ch in data)
            {
                if(EscapedChars.TryGetValue(ch, out var escaped))
                {
                    writer.Write(EscapeCharacter);
                    writer.Write(escaped);
                }
                else
                {
                    writer.Write(ch);
                }
            }
        }

        protected abstract Stream CreateWriteStream(Config config);
        protected abstract Stream? CreateReadStream(Config config);
    }
}
