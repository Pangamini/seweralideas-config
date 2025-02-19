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
            foreach (ConfigField field in config.Fields)
            {
                string strVal = field.GetStringValue();
                PlayerPrefs.SetString(GetFieldKey(field), strVal);
            }
            PlayerPrefs.Save();
            return true;
        }
        
        public override bool PreLoad(Config config) => true;

        public override void LoadField(ConfigField field)
        {
            string? str = PlayerPrefs.GetString(GetFieldKey(field));
            if(str == null || !field.SetStringValue(str))
            {
                field.SetDefaultValue();
            }
        }
        
        private string GetFieldKey(ConfigField field) => $"{field.Config!.GlobalName}.{field.Key}";

    }
}
