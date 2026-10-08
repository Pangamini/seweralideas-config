#nullable enable
using UnityEngine;

namespace SeweralIdeas.Config
{
    /// <summary>The whole config in a single PlayerPrefs string (see <see cref="PlayerPrefsBlobStorage"/>).</summary>
    [CreateAssetMenu(menuName = "SeweralIdeas/Config/" + nameof(PlayerPrefsBlobStorageDescription))]
    public class PlayerPrefsBlobStorageDescription : StorageDescription
    {
        public override bool Available => true;

        // A new one every time: a stream storage keeps what it has read until the fields have taken it.
        public override ConfigStorage GetStorage() => new PlayerPrefsBlobStorage();
    }
}
