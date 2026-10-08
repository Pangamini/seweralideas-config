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

            string? key = null;
            StringBuilder sb = new();
            bool escaping = false;

            void FinishLine()
            {
                string value = sb.ToString();
                sb.Clear();
                if(!string.IsNullOrWhiteSpace(key))
                    m_loadedStringValues.Add(key, value);
                key = null;
            }
            
            while(true)
            {
                int val = reader.Read();
                if(val < 0)
                {
                    FinishLine();
                    break;
                }

                char ch = (char)val;
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
                        break;
                    case '=':
                        key = sb.ToString();
                        sb.Clear();
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
                    string value = field.GetStringValue();
                    WriteEscaped(writer, field.name);
                    writer.Write(" = ");
                    WriteEscaped(writer, value);
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

        
        private void WriteEscaped(TextWriter writer, string data)
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
