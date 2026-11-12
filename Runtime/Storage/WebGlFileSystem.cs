using System.Runtime.InteropServices;

namespace SeweralIdeas.Config
{
    /// <summary>
    /// On WebGL the persistent data folder lives in memory and reaches the browser's IndexedDB only when synced. Nothing else
    /// is guaranteed to do that, so a write calls <see cref="Sync"/>. Does nothing on other platforms.
    /// </summary>
    internal static class WebGlFileSystem
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SeweralIdeasConfig_SyncFileSystem();
#endif

        public static void Sync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SeweralIdeasConfig_SyncFileSystem();
#endif
        }
    }
}
