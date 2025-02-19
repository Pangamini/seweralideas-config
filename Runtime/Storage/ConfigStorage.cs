#nullable enable
namespace SeweralIdeas.Config
{
    public abstract class ConfigStorage
    {
        public abstract bool Save(Config config);
        public abstract bool PreLoad(Config config);
        public abstract void LoadField(ConfigField field);
    }
}
