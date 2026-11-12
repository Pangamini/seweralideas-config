#nullable enable
using System;
using System.IO;
using UnityEngine;
namespace SeweralIdeas.Config
{
    public class FileConfigStorage : StreamConfigStorage
    {
        private readonly FileConfigStorageDescription m_description;

        public FileConfigStorage(FileConfigStorageDescription description) => m_description = description;

        #if UNITY_EDITOR
        public override void RevealInEditor(Config config) => EditorReveal.RevealFile(GetPath(config));
        #endif

        protected override Stream CreateWriteStream(Config config)
        {
            FileInfo file = GetFileInfo(config);
            Directory.CreateDirectory(file.Directory!.FullName);
            return new SyncingFileStream(file.FullName);
        }

        // A file in the persistent data folder has to be synced to IndexedDB on WebGL once it is closed (a no-op elsewhere).
        private sealed class SyncingFileStream : FileStream
        {
            public SyncingFileStream(string path) : base(path, FileMode.Create, FileAccess.Write) { }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if(disposing)
                    WebGlFileSystem.Sync();
            }
        }
        
        protected override Stream? CreateReadStream(Config config)
        {
            FileInfo file = GetFileInfo(config);
            if(!file.Exists)
                return null;
            
            return file.Open(FileMode.Open, FileAccess.Read);
        }
        
        private FileInfo GetFileInfo(Config config)
        {
            string path = GetPath(config);
            FileInfo file = new(path);
            return file;
        }

        private string GetPath(Config config)
        {
            string root = m_description.Root switch
            {
                FileConfigStorageDescription.RootFolder.StreamingAssets => Application.streamingAssetsPath,
                FileConfigStorageDescription.RootFolder.PersistentDataPath => Application.persistentDataPath,
                _ => throw new ArgumentOutOfRangeException()
            };
            return Path.ChangeExtension(Path.Combine(root, m_description.SubFolder, config.GlobalName), m_description.Extension);
        }

    }
}
