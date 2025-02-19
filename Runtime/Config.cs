#nullable enable
using System;
using System.Collections.Generic;
using SeweralIdeas.Collections;
using UnityEngine;
namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/"+nameof(Config))]
    public class Config : ScriptableObject
    {
        [SerializeField] private string? m_globalName;
        [SerializeField] private bool    m_autoLoadSave;

        [SerializeReference]
        private ConfigStoragePlan? m_storagePlan;
        
        private readonly HashSet<ConfigField> m_fields = new();
        
        private                 bool           m_dirty;
        private                 ConfigStorage? m_currentStorage;
        [NonSerialized] private bool           m_autoSaveHooked;
        
        public ReadonlySetView<ConfigField> Fields => new ReadonlySetView<ConfigField>(m_fields);
        public bool IsDirty => m_dirty;
        public string GlobalName => m_globalName ?? string.Empty;

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
            m_currentStorage?.LoadField(field);
        }

        internal void UnregisterField(ConfigField field)
        {
            m_fields.Remove(field);
        }

        private ConfigStorage EnsureStorage()
        {
            m_currentStorage ??= m_storagePlan != null ? m_storagePlan.CreateAvailableStorage() : null;
            return m_currentStorage ?? throw new InvalidOperationException("Storage is null");
        }
        
        public void Save()
        {
            if(!IsDirty)
                return;
            
            Debug.Log($"Saving Config {GlobalName} to {m_currentStorage}", this);
            
            EnsureStorage().Save(this);
            m_dirty = false;
        }

        public void Load()
        {
            Debug.Log($"Loading Config {GlobalName} from {m_currentStorage}", this);

            ConfigStorage storage = EnsureStorage();
            storage.PreLoad(this);

            foreach (var field in m_fields)
                storage.LoadField(field);
            
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
