#nullable enable
using UnityEngine;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/" + nameof(PlayerPrefsStorageDescription))]
    public class PlayerPrefsStorageDescription : StorageDescription
    {
        public override bool Available => true;
        public override ConfigStorage GetStorage() => PlayerPrefsStorage.Instance;
    }
}
