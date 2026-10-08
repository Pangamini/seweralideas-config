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
        
        [SerializeField]
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

        protected void OnEnable()
        {
            SetRegisteredFields(m_fields);
            UpdateAutoSaveHooks();
            if(m_autoSaveHooked)
                Load();
        }

        protected void OnDisable()
        {
            SetRegisteredFields(null);

            // The quitting event is static: left subscribed, the handler would outlive the asset's enabled state (and, without
            // a domain reload, pile up with the next OnEnable's).
            if(m_autoSaveHooked)
            {
                Application.quitting -= OnQuitWithAutoSave;
                m_autoSaveHooked = false;
            }
        }

        // Puts the runtime state back to what a freshly loaded asset has: the fields without their values and listeners, nothing
        // unsaved, and OnEnable's registering, hooking and (auto) loading done again.
        private void ResetRuntimeState()
        {
            foreach (ConfigField field in m_fields)
            {
                if(field != null)
                    field.ResetRuntimeState();
            }

            OnDisable();
            m_dirty = false;
            OnEnable();
        }

        #if UNITY_EDITOR
        // With the domain reload on entering play mode switched off, the assets that are loaded keep the runtime state of
        // the previous session: field values, listeners of destroyed objects, a dirty flag. A new domain would have none of
        // it, and a build never has any, so each session starts by resetting it - and only then, as a reload has done it.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetLoadedConfigsWithoutDomainReload()
        {
            bool domainReloaded = !UnityEditor.EditorSettings.enterPlayModeOptionsEnabled
                                  || (UnityEditor.EditorSettings.enterPlayModeOptions & UnityEditor.EnterPlayModeOptions.DisableDomainReload) == 0;
            if(domainReloaded)
                return;

            foreach (Config config in Resources.FindObjectsOfTypeAll<Config>())
                config.ResetRuntimeState();
        }
        #endif

        private void OnValidate()
        {
            m_fields.RemoveAll(field => field == null); // entries left empty by a removed field (see ConfigEditor)
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
        
        private ConfigStorage? GetStorage() => m_storagePlan != null ? m_storagePlan.CreateAvailableStorage() : null;

        #if UNITY_EDITOR
        /// <summary>Shows where the config is stored now (in the storage that is available, as for Save and Load) in the file browser or regedit.</summary>
        public void RevealStorageInEditor()
        {
            var storage = GetStorage();
            if(storage == null)
                Debug.LogWarning($"Config {GlobalName} has no storage available, so there is nothing to show.", this);
            else
                storage.RevealInEditor(this);
        }
        #endif

        /// <summary>
        /// Saves the fields to the storage if anything changed since the last save or load. Without an available storage
        /// there is nowhere to save to: it warns and leaves the config dirty, and does not throw, as this runs when quitting.
        /// </summary>
        public void Save()
        {
            if(!IsDirty)
                return;

            var storage = GetStorage();
            if(storage == null)
            {
                Debug.LogWarning($"Config {GlobalName} has no storage available on this platform, so it is not saved.", this);
                return;
            }

            Debug.Log($"Saving Config {GlobalName} to {storage}", this);
            storage.Save(this);
            m_dirty = false;
        }

        /// <summary>
        /// Loads the fields from the storage, the ones it has no value for getting their default. Without an available
        /// storage nothing is stored, so every field gets its default.
        /// </summary>
        public void Load()
        {
            var storage = GetStorage();
            if(storage == null)
            {
                Debug.LogWarning($"Config {GlobalName} has no storage available on this platform, so its fields have their default values.", this);
                foreach (var field in m_fields)
                {
                    if(field != null)
                        field.SetDefaultValue();
                }
                m_dirty = false;
                return;
            }

            Debug.Log($"Loading Config {GlobalName} from {storage}", this);

            storage.PreLoad(this);

            foreach (var field in m_fields)
            {
                if(field != null) // an entry left empty by a removed field
                    storage.LoadField(this, field.name, field);
            }

            m_dirty = false;
        }
    }
}
