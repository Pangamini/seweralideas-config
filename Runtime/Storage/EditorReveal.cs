#if UNITY_EDITOR
#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SeweralIdeas.Config
{
    /// <summary>
    /// What <see cref="ConfigStorage.RevealInEditor"/> does for the storages that are a file or PlayerPrefs: a file is
    /// shown in the file browser, and the PlayerPrefs where the editor (not a built game) keeps them - in regedit on
    /// Windows, at the key of the project, and in the file browser at the plist or the prefs file on macOS and Linux.
    /// </summary>
    public static class EditorReveal
    {
        // The file if it is there, or else the nearest folder above it that is.
        public static void RevealFile(string? path)
        {
            if(string.IsNullOrEmpty(path))
                return;

            string? existing = path;
            while(!string.IsNullOrEmpty(existing) && !System.IO.File.Exists(existing) && !Directory.Exists(existing))
                existing = Path.GetDirectoryName(existing);

            if(string.IsNullOrEmpty(existing))
            {
                Debug.LogWarning($"Nothing of {path} exists yet.");
                return;
            }

            if(existing != path)
                Debug.Log($"{path} does not exist yet (it is saved when the config is), showing {existing}.");
            EditorUtility.RevealInFinder(existing);
        }

        // PlayerPrefs are kept by the company and the product name.
        public static void RevealPlayerPrefs()
        {
            string company = PlayerSettings.companyName;
            string product = PlayerSettings.productName;
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    RevealInRegedit($@"HKEY_CURRENT_USER\Software\Unity\UnityEditor\{company}\{product}");
                    break;
                case RuntimePlatform.OSXEditor:
                    RevealFile(Path.Combine(home, "Library", "Preferences", $"unity.{company}.{product}.plist"));
                    break;
                case RuntimePlatform.LinuxEditor:
                    RevealFile(Path.Combine(home, ".config", "unity3d", company, product, "prefs"));
                    break;
                default:
                    Debug.Log("The PlayerPrefs of this platform can't be shown.");
                    break;
            }
        }

        // Regedit has no command line to open a key, but it opens at the last key it showed, which is a value in the registry.
        // (A regedit that is already open stays where it is.) Windows asks for the permission to run it.
        private static void RevealInRegedit(string key)
        {
            const string regeditSettings = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit";
            try
            {
                string lastKey = @"Computer\" + key; // regedit shows the keys under "Computer"
                var setLastKey = new ProcessStartInfo("reg", $"add \"{regeditSettings}\" /v LastKey /t REG_SZ /d \"{lastKey}\" /f")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process? process = Process.Start(setLastKey))
                    process?.WaitForExit(3000);

                Process.Start(new ProcessStartInfo("regedit.exe") { UseShellExecute = true });
                Debug.Log($"Opening regedit at {key}. If it was open already, go to the key by hand.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not open regedit: {e.Message}\nThe PlayerPrefs are at {key}");
            }
        }
    }
}
#endif
