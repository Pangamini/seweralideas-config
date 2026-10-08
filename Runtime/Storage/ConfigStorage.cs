#nullable enable
namespace SeweralIdeas.Config
{
    public abstract class ConfigStorage
    {
        public abstract bool Save(Config config);
        public abstract bool PreLoad(Config config);

        /// <summary>Loads one field of <paramref name="config"/>; <paramref name="key"/> is its name.</summary>
        public abstract void LoadField(Config config, string key, ConfigField field);

        #if UNITY_EDITOR
        /// <summary>
        /// Shows, in the editor, where <paramref name="config"/> is stored: the file in the file browser, or the PlayerPrefs
        /// in regedit (see <see cref="EditorReveal"/> for those).
        /// </summary>
        public abstract void RevealInEditor(Config config);
        #endif
    }
}
