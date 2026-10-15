#nullable enable
using UnityEngine;

namespace SeweralIdeas.Config
{
    public class PlayerPrefsStorageDescription : StorageDescription
    {
        public override bool Available => true;
        public override ConfigStorage GetStorage() => PlayerPrefsStorage.Instance;
    }
}
