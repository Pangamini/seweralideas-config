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
        
        private readonly HashSet<ConfigField> m_fields = new();
        private bool m_dirty;
        private bool m_loaded;
        [NonSerialized] private bool m_autoSaveHooked;
        
        public ReadonlySetView<ConfigField> Fields => new ReadonlySetView<ConfigField>(m_fields);
        public bool IsDirty => m_dirty;

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
            if(m_loaded)
                LoadField(field);
        }

        internal void UnregisterField(ConfigField field)
        {
            m_fields.Remove(field);
        }
        
        public void Save()
        {
            if(!IsDirty)
                return;
            
            Debug.Log($"Config.Save {name}");
            m_dirty = false;

            foreach (ConfigField field in m_fields)
            {
                string strVal = field.GetStringValue();
                PlayerPrefs.SetString(GetFieldKey(field), strVal);
            }
            PlayerPrefs.Save();
        }

        public void Load()
        {
            Debug.Log($"Config.Load {name}");
            foreach (ConfigField field in m_fields)
            {
                LoadField(field);
            }
            
            m_loaded = true;
            m_dirty = false;
        }
        private void LoadField(ConfigField field)
        {
            string str = PlayerPrefs.GetString(GetFieldKey(field));
            if(!field.SetStringValue(str))
            {
                field.SetDefaultValue();
            }
        }

        private string GetFieldKey(ConfigField field) => $"{m_globalName}.{field.name}";

        public void SetFieldsDirty()
        {
            m_dirty = true;
        }

        public void EnsureInitialized()
        {
            
        }
    }
}
