using System;
using System.Collections.Generic;
using SeweralIdeas.Collections;
using UnityEngine;
namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/"+nameof(Config))]
    public class Config : ScriptableObject
    {
        [SerializeField] private string m_globalName;
        [SerializeField] private bool m_autoLoadSave;

        [SerializeReference]
        private ConfigStoragePlan m_storagePlan;
        
        private readonly HashSet<ConfigField> m_fields = new();
        
        private bool m_dirty;
        private ConfigStorage m_currentStorage;
        [NonSerialized] private bool m_autoSaveHooked;
        
        public ReadonlySetView<ConfigField> Fields => new ReadonlySetView<ConfigField>(m_fields);
        public bool IsDirty => m_dirty;
        public string GlobalName => m_globalName;

        private void OnEnable()
        {
            UpdateAutoSaveHooks();
            if(m_autoSaveHooked)
                Load();
        }

        private void OnValidate()
        {
            UpdateAutoSaveHooks();
        }

        private void UpdateAutoSaveHooks()
        {
            if(m_autoSaveHooked == m_autoLoadSave)
                return;

            if(m_autoSaveHooked)
            {
                Application.quitting -= OnQuitWithAutoSave;
            }
            m_autoSaveHooked = m_autoLoadSave;
            if(m_autoSaveHooked)
            {
                Application.quitting += OnQuitWithAutoSave;
            }
        }
        
        private void OnQuitWithAutoSave() => Save();

        protected void Reset()
        {
            m_globalName = name;
        }
        
        internal void RegisterField(ConfigField field)
        {
            m_fields.Add(field);
            if(m_currentStorage != null)
            {
                m_currentStorage.LoadField(field);
            }
        }

        internal void UnregisterField(ConfigField field)
        {
            m_fields.Remove(field);
        }
        
        public void Save()
        {
            m_currentStorage = m_storagePlan.CreateAvailableStorage();
            
            if(!IsDirty)
                return;
            
            Debug.Log($"Saving Config {GlobalName} to {m_currentStorage}", this);
            
            m_dirty = false;

            m_currentStorage.Save(this);
        }

        public void Load()
        {
            m_currentStorage = m_storagePlan.CreateAvailableStorage();
            
            Debug.Log($"Loading Config {GlobalName} from {m_currentStorage}", this);

            m_currentStorage.PreLoad(this);

            foreach (var field in m_fields)
            {
                m_currentStorage.LoadField(field);
            }
            
            m_dirty = false;
        }
        
        internal void SetFieldsDirty()
        {
            m_dirty = true;
        }

        internal void EnsureInitialized()
        {
            
        }
    }
}
