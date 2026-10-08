#nullable enable
using System.IO;
using System.Text;
using UnityEngine;
namespace SeweralIdeas.Config
{
    /// <summary>
    /// A config stored as one PlayerPrefs string under its global name, in the text format of the file storage. The
    /// alternative, <see cref="PlayerPrefsStorage"/>, has one key per field.
    ///
    /// One key means a removed or renamed field leaves nothing behind (PlayerPrefs can't list or delete keys it doesn't
    /// know), and the fields are saved together. In exchange, the values can't be read one by one with PlayerPrefs.
    /// </summary>
    public class PlayerPrefsBlobStorage : StreamConfigStorage
    {
        // A config without a global name has nothing else to be known by.
        private static string GetKey(Config config) => config.GlobalName.Length == 0 ? config.name : config.GlobalName;

        protected override Stream? CreateReadStream(Config config)
        {
            string key = GetKey(config);
            if(!PlayerPrefs.HasKey(key))
                return null;

            return new MemoryStream(Encoding.UTF8.GetBytes(PlayerPrefs.GetString(key)));
        }

        protected override Stream CreateWriteStream(Config config) => new PlayerPrefsWriteStream(GetKey(config));

        #if UNITY_EDITOR
        public override void RevealInEditor(Config config) => EditorReveal.RevealPlayerPrefs();
        #endif

        // Collects what is written, and puts it in PlayerPrefs when it is closed.
        private sealed class PlayerPrefsWriteStream : MemoryStream
        {
            private readonly string m_key;

            public PlayerPrefsWriteStream(string key) => m_key = key;

            protected override void Dispose(bool disposing)
            {
                if(disposing && CanWrite) // not when it has been closed already (the writer closes it, and then it is disposed again)
                {
                    // The writer's byte order mark is not part of the text.
                    string text = Encoding.UTF8.GetString(ToArray()).TrimStart('﻿');
                    PlayerPrefs.SetString(m_key, text);
                    PlayerPrefs.Save();
                }

                base.Dispose(disposing);
            }
        }
    }
}
