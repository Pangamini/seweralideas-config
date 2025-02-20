#nullable enable
using UnityEngine;
using UnityEngine.Scripting;
namespace SeweralIdeas.Config
{
    [Preserve]
    public class PlayerPrefsStorageDescription : StorageDescription
    {
        public override bool Available => true;
        public override ConfigStorage GetStorage() => PlayerPrefsStorage.Instance;
    }
    
    public class PlayerPrefsStorage : ConfigStorage
    {
        public static readonly PlayerPrefsStorage Instance = new();
        
        public override bool Save(Config config)
        {
            foreach (var field in config.Fields)
            {
                string strVal = field.GetStringValue();
                PlayerPrefs.SetString(field.name, strVal);
            }
            PlayerPrefs.Save();
            return true;
        }
        
        public override bool PreLoad(Config config) => true;

        public override void LoadField(string key, ConfigField field)
        {
            if(PlayerPrefs.HasKey(key))
            {
                string str = PlayerPrefs.GetString(key);
                if(str != null && field.SetStringValue(str))
                    return;
            }
            field.SetDefaultValue();
        }
    }
}
