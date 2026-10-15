#nullable enable
using UnityEngine;

namespace SeweralIdeas.Config
{
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
}
