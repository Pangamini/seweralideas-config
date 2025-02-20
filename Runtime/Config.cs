#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace SeweralIdeas.Config
{
    [CreateAssetMenu(menuName = "SeweralIdeas/"+nameof(Config))]
    public class Config : ScriptableObject
    {
        [SerializeField] private string? m_globalName;
        [SerializeField] private bool    m_autoLoadSave;
        
        [SerializeReference]
        private ConfigStoragePlan? m_storagePlan;

        // [Serializable]
        // public struct FieldInfo
        // {
        //     [SerializeField] private string      m_key;
        //     [SerializeField] private ConfigField m_field;
        //     public string Key { get => m_key; set => m_key = value; }
        //     public ConfigField Field { get => m_field; set => m_field = value; }
        // }
        
        [SerializeField] private List<ConfigField> m_fields = new();

        private readonly        HashSet<ConfigField> m_registeredFields = new();
        private                 bool                 m_dirty;
        private readonly        Action               m_setFieldsDirty;
        [NonSerialized] private bool                 m_autoSaveHooked;
        
        public Config() => m_setFieldsDirty = () => m_dirty = true;

        public IReadOnlyList<ConfigField> Fields => m_fields;
        public bool IsDirty => m_dirty;
        public string GlobalName => m_globalName ?? string.Empty;
        public ConfigStoragePlan? StoragePlan => m_storagePlan;

        private void OnEnable()
        {
            SetRegisteredFields(m_fields);
            UpdateAutoSaveHooks();
            if(m_autoSaveHooked)
                Load();
        }

        protected void OnDisable()
        {
            SetRegisteredFields(null);
        }

        private void OnValidate()
        {
            SetRegisteredFields(m_fields);
            UpdateAutoSaveHooks();
        }
        
        private void SetRegisteredFields(List<ConfigField>? newFields)
        {
            using (HashSetPool<ConfigField>.Get(out var toUnregister))
            {
                // produce a list of registered fields not in the new fields list
                foreach(var registered in m_registeredFields)
                    toUnregister.Add(registered);

                if(newFields != null)
                {
                    foreach (var field in newFields)
                    {
                        if(field == null)
                            continue;
                        
                        toUnregister.Remove(field);
                        
                        if(m_registeredFields.Add(field))
                            field.Changed += m_setFieldsDirty;
                    }
                }

                foreach (var oldField in toUnregister)
                {
                    if(m_registeredFields.Remove(oldField))
                        oldField.Changed -= m_setFieldsDirty;
                }
            }
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
        
        private ConfigStorage GetStorage()
        {
            var currentStorage = m_storagePlan != null ? m_storagePlan.CreateAvailableStorage() : null;
            return currentStorage ?? throw new InvalidOperationException("Storage is null");
        }
        
        public void Save()
        {
            if(!IsDirty)
                return;

            var storage = GetStorage();
            Debug.Log($"Saving Config {GlobalName} to {storage}", this);
            storage.Save(this);
            m_dirty = false;
        }

        public void Load()
        {
            var storage = GetStorage();
            Debug.Log($"Loading Config {GlobalName} from {storage}", this);

            storage.PreLoad(this);

            foreach (var field in m_fields)
                storage.LoadField(field.name, field);
            
            m_dirty = false;
        }
    }
}
