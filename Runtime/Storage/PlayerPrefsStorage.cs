#nullable enable
using UnityEngine;
namespace SeweralIdeas.Config
{
    /// <summary>
    /// A config stored in PlayerPrefs with a key for each field: <c>GlobalName/fieldName</c>. An int, a float and a bool
    /// field are stored as such (a bool as 0 or 1), so they are exact and can be read with PlayerPrefs; any other field is
    /// stored as the string of its value. The alternative, <see cref="PlayerPrefsBlobStorage"/>, stores the whole config as
    /// one string.
    ///
    /// PlayerPrefs does not remember what a key was stored as: reading it as another type gives the default of that type.
    /// A field whose type changes therefore reads as if it had no value, and takes its default value.
    /// </summary>
    public class PlayerPrefsStorage : ConfigStorage
    {
        public static readonly PlayerPrefsStorage Instance = new();

        // PlayerPrefs has one namespace for the whole game, so the key has the config's global name in front: two configs
        // with a field of the same name don't share a value. A config without a global name has the bare field name.
        private static string GetKey(Config config, string fieldName) =>
            config.GlobalName.Length == 0 ? fieldName : config.GlobalName + "/" + fieldName;

        public override bool Save(Config config)
        {
            foreach (var field in config.Fields)
            {
                if(field == null)
                    continue;

                string key = GetKey(config, field.name);
                switch (field)
                {
                    case ConfigField<int> intField:
                        PlayerPrefs.SetInt(key, intField.Value);
                        break;
                    case ConfigField<float> floatField:
                        PlayerPrefs.SetFloat(key, floatField.Value);
                        break;
                    case ConfigField<bool> boolField:
                        PlayerPrefs.SetInt(key, boolField.Value ? 1 : 0);
                        break;
                    default:
                        PlayerPrefs.SetString(key, field.GetStringValue());
                        break;
                }
            }
            PlayerPrefs.Save();
            return true;
        }

        public override bool PreLoad(Config config) => true;

        #if UNITY_EDITOR
        public override void RevealInEditor(Config config) => EditorReveal.RevealPlayerPrefs();
        #endif

        public override void LoadField(Config config, string key, ConfigField field)
        {
            string prefsKey = GetKey(config, key);
            if(!PlayerPrefs.HasKey(prefsKey))
            {
                field.SetDefaultValue();
                return;
            }

            switch (field)
            {
                case ConfigField<int> intField:
                    intField.Value = PlayerPrefs.GetInt(prefsKey);
                    return;
                case ConfigField<float> floatField:
                    floatField.Value = PlayerPrefs.GetFloat(prefsKey);
                    return;
                case ConfigField<bool> boolField:
                    boolField.Value = PlayerPrefs.GetInt(prefsKey) != 0;
                    return;
            }

            string str = PlayerPrefs.GetString(prefsKey);
            if(str != null && field.SetStringValue(str))
                return;
            field.SetDefaultValue();
        }
    }
}
