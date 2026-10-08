#nullable enable
using UnityEngine;

namespace SeweralIdeas.Config
{
    /// <summary>
    /// Where a <see cref="Config"/> is stored: the first of the descriptions that is available on the platform is used.
    /// The descriptions are assets (Create > SeweralIdeas > Config > ...), put in the list in order of preference.
    /// </summary>
    [CreateAssetMenu(menuName = "SeweralIdeas/"+nameof(ConfigStoragePlan))]
    public class ConfigStoragePlan : ScriptableObject
    {
        [SerializeField]
        private StorageDescription[]? m_descriptions;

        public ConfigStorage? CreateAvailableStorage()
        {
            if(m_descriptions == null)
                return null;

            foreach (StorageDescription storageDescription in m_descriptions)
            {
                if(storageDescription == null || !storageDescription.Available)
                    continue;
                return storageDescription.GetStorage();
            }
            return null;
        }
    }
}
