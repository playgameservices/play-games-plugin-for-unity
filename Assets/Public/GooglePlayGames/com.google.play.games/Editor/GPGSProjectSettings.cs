// <copyright file="GPGSProjectSettings.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

// Keep this file even on unsupported configurations.

namespace GooglePlayGames.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
#if UNITY_2017_3_OR_NEWER
    using UnityEngine.Networking;
#endif

    [System.Obsolete("GPGSProjectSettings is deprecated. Settings are read/written directly to PlayGamesSettings.")]
    public class GPGSProjectSettings
    {
        private static GPGSProjectSettings sInstance = null;

        public static GPGSProjectSettings Instance
        {
            get
            {
                if (sInstance == null)
                {
                    sInstance = new GPGSProjectSettings();
                    sInstance.LoadAndMigrateLegacySettings();
                }

                return sInstance;
            }
        }

        private readonly string mFile;
        private bool mDirty = false;
        private Dictionary<string, string> mDict = new Dictionary<string, string>();

        private GPGSProjectSettings()
        {
            mFile = GPGSUtil.SlashesToPlatformSeparator("ProjectSettings/GooglePlayGameSettings.txt");
        }

        private void LoadAndMigrateLegacySettings()
        {
            string legacyFile1 = mFile;
            string legacyFile2 = GPGSUtil.SlashesToPlatformSeparator("Assets/Editor/projsettings.txt");
            string legacyFile3 = GPGSUtil.SlashesToPlatformSeparator(Path.Combine(GPGSUtil.RootPath, "Editor/projsettings.txt"));

            string fileToRead = null;
            if (File.Exists(legacyFile1)) fileToRead = legacyFile1;
            else if (File.Exists(legacyFile2)) fileToRead = legacyFile2;
            else if (File.Exists(legacyFile3)) fileToRead = legacyFile3;

            if (fileToRead != null)
            {
                StreamReader rd = new StreamReader(fileToRead);
                while (!rd.EndOfStream)
                {
                    string line = rd.ReadLine();
                    if (line == null || line.Trim().Length == 0) break;
                    
                    line = line.Trim();
                    string[] p = line.Split(new char[] { '=' }, 2);
                    if (p.Length >= 2)
                    {
                        string key = p[0].Trim();
                        string val = p[1].Trim();
#if UNITY_2017_3_OR_NEWER
                        val = UnityWebRequest.UnEscapeURL(val);
#else
                        val = WWW.UnEscapeURL(val);
#endif
                        Set(key, val);
                    }
                }
                rd.Close();
                Save();
            }
        }

        public string Get(string key, Dictionary<string, string> overrides)
        {
            if (overrides.ContainsKey(key))
            {
                return overrides[key];
            }
            return Get(key);
        }

        public string Get(string key, string defaultValue)
        {
            PlayGamesSettings settings = PlayGamesSettings.LoadInstance();
            if (settings != null)
            {
                if (key == GPGSUtil.APPIDKEY && !string.IsNullOrEmpty(settings.AppId)) return settings.AppId;
                if (key == GPGSUtil.WEBCLIENTIDKEY && !string.IsNullOrEmpty(settings.WebClientId)) return settings.WebClientId;
                if (key == GPGSUtil.SERVICEIDKEY && !string.IsNullOrEmpty(settings.NearbyServiceId)) return settings.NearbyServiceId;
            }

            if (mDict.ContainsKey(key))
            {
#if UNITY_2017_3_OR_NEWER
                return UnityWebRequest.UnEscapeURL(mDict[key]);
#else
                return WWW.UnEscapeURL(mDict[key]);
#endif
            }
            return defaultValue;
        }

        public string Get(string key)
        {
            return Get(key, string.Empty);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return Get(key, defaultValue ? "true" : "false").Equals("true");
        }

        public bool GetBool(string key)
        {
            return Get(key, "false").Equals("true");
        }

        public void Set(string key, string val)
        {
            val = val ?? string.Empty;
#if UNITY_2017_3_OR_NEWER
            string escaped = UnityWebRequest.EscapeURL(val);
#else
            string escaped = WWW.EscapeURL(val);
#endif

            bool dictChanged = !mDict.ContainsKey(key) || mDict[key] != escaped;

            PlayGamesSettings settings = GetOrCreateSettings();
            bool settingsChanged = false;

            if (key == GPGSUtil.APPIDKEY && settings.AppId != val)
            {
                settings.AppId = val;
                settingsChanged = true;
            }
            else if (key == GPGSUtil.WEBCLIENTIDKEY && settings.WebClientId != val)
            {
                settings.WebClientId = val;
                settingsChanged = true;
            }
            else if (key == GPGSUtil.SERVICEIDKEY && settings.NearbyServiceId != val)
            {
                settings.NearbyServiceId = val;
                settingsChanged = true;
            }

            if (!dictChanged && !settingsChanged)
            {
                return; // no-op — breaks the infinite loop
            }

            mDict[key] = escaped;
            mDirty = true;
        }

        public void Set(string key, bool val)
        {
            Set(key, val ? "true" : "false");
        }

        public void Save()
        {
            if (!mDirty)
            {
                return;
            }

            PlayGamesSettings settings = PlayGamesSettings.LoadInstance();
            if (settings != null)
            {
                EditorUtility.SetDirty(settings); // let Unity flush on its own schedule
            }

            // Write back to legacy file for backward compatibility (e.g. Kokoro/CI)
            try
            {
                using (StreamWriter wr = new StreamWriter(mFile, false))
                {
                    foreach (string key in mDict.Keys)
                    {
                        wr.WriteLine(key + "=" + mDict[key]);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to write legacy settings file: " + e.Message);
            }
            mDirty = false;
        }

        public static void Reload()
        {
            sInstance = new GPGSProjectSettings();
        }

        private PlayGamesSettings GetOrCreateSettings()
        {
            PlayGamesSettings settings = PlayGamesSettings.LoadInstance();
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PlayGamesSettings>();
                string resDir = "Assets/GooglePlayGames/Resources";
                if (!Directory.Exists(resDir))
                {
                    Directory.CreateDirectory(resDir);
                }
                AssetDatabase.CreateAsset(settings, resDir + "/PlayGamesSettings.asset");
            }
            return settings;
        }
    }
}
