#nullable enable
using System;
using SeweralIdeas.UnityUtils;
using UnityEngine;

namespace SeweralIdeas.Config
{
    [Serializable]
    public abstract class StorageDescription
    {
        public abstract bool Available { get; }
        public abstract ConfigStorage GetStorage();
    }
    
    [CreateAssetMenu(menuName = "SeweralIdeas/"+nameof(ConfigStoragePlan))]
    public class ConfigStoragePlan : ScriptableObject
    {
        [InstantiateGUI(typeof( StorageDescription ))]
        [SerializeReference]
        private StorageDescription[]? m_descriptions;
        
        public ConfigStorage? CreateAvailableStorage()
        {
            if(m_descriptions == null)
                return null;
            
            foreach (StorageDescription storageDescription in m_descriptions)
            {
                if(!storageDescription.Available)
                    continue;
                return storageDescription.GetStorage();
            }
            return null;
        }
    }
}
