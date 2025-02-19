#nullable enable
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;
namespace SeweralIdeas.Config
{
    [Preserve]
    public class FileConfigStorageDescription : StorageDescription
    {
        public enum RootFolder
        {
            StreamingAssets = 1,
            PersistentDataPath = 2
        }

        [field: SerializeField] public RootFolder Root { get; set; } = RootFolder.PersistentDataPath;
        [field: SerializeField] public string Extension { get; set; } = ".cfg";
        [field: SerializeField] public string SubFolder { get; set; } = "";

        public override bool Available => Application.platform != RuntimePlatform.WebGLPlayer;
        public override ConfigStorage GetStorage() => new FileConfigStorage(this);
    }

    public class FileConfigStorage : StreamConfigStorage
    {
        private readonly FileConfigStorageDescription m_description;

        public FileConfigStorage(FileConfigStorageDescription description) => m_description = description;

        protected override Stream CreateWriteStream(Config config)
        {
            FileInfo file = GetFileInfo(config);
            Directory.CreateDirectory(file.Directory!.FullName);
            return file.Open(FileMode.Create, FileAccess.Write);
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
