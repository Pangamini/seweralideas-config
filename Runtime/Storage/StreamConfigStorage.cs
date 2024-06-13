using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.Pool;
namespace SeweralIdeas.Config
{
    public abstract class StreamConfigStorage : ConfigStorage
    {
        private const char EscapeCharacter = '\\';
        private static readonly Encoding s_streamEncoding = Encoding.UTF8;
        private static readonly Dictionary<char, char> s_unescapedChars = new Dictionary<char, char>();
        private static readonly Dictionary<char, char> s_escapedChars = new Dictionary<char, char>
        {
            { '=', '=' },
            { ' ', '_' },
            { '\n', 'n' },
            { '\\', '\\' }
        };

        private readonly Dictionary<string, string> m_loadedStringValues = new();

        static StreamConfigStorage()
        {
            foreach(var pair in s_escapedChars)
                s_unescapedChars.Add(pair.Value, pair.Key);
        }

        public override void LoadField(ConfigField field)
        {
            if(m_loadedStringValues.TryGetValue(field.Key, out string stringValue))
            {
                field.SetStringValue(stringValue);
                m_loadedStringValues.Remove(field.Key);
                return;
            }
            
            field.SetDefaultValue();
        }

        public override sealed bool PreLoad(Config config)
        {
            m_loadedStringValues.Clear();
            
            using var stream = CreateReadStream(config);
            if(stream == null)
                return false;
            
            using var reader = new StreamReader(stream, s_streamEncoding);

            string key = null;
            StringBuilder sb = new();
            bool escaping = false;

            void FinishLine()
            {
                string value = sb.ToString();
                sb.Clear();
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
                    if(!s_unescapedChars.TryGetValue(ch, out var unescaped))
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

            return true;
        }
        
        public override sealed bool Save(Config config)
        {
            using var stream = CreateWriteStream(config);
            using var writer = new StreamWriter(stream, s_streamEncoding);
            using (ListPool<ConfigField>.Get(out var fields))
            {
                foreach(var field in config.Fields)
                    fields.Add(field);
                
                fields.Sort((lhs, rhs) => String.Compare(lhs.Key, rhs.Key, StringComparison.Ordinal));
                
                using var enumerator = fields.GetEnumerator();

                void WriteField(ConfigField configField)
                {
                    string key = configField.Key;
                    string value = configField.GetStringValue();
                    WriteEscaped(writer, key);
                    writer.Write(" = ");
                    WriteEscaped(writer, value);
                }

                if(enumerator.MoveNext())
                {
                    ConfigField current = enumerator.Current;
                    if(current != null)
                        WriteField(current);
                }

                while(enumerator.MoveNext())
                {
                    ConfigField current = enumerator.Current;
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
                if(s_escapedChars.TryGetValue(ch, out var escaped))
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
        protected abstract Stream CreateReadStream(Config config);
    }
}
