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
#if UNITY_2017_3_OR_NEWER
    using UnityEngine.Networking;
#else
    using UnityEngine;

#endif

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
                }

                return sInstance;
            }
        }

        private bool mDirty = false;
        private readonly string mFile;
        private Dictionary<string, string> mDict = new Dictionary<string, string>();

        private GPGSProjectSettings()
        {
            mFile = GPGSUtil.SlashesToPlatformSeparator("ProjectSettings/GooglePlayGameSettings.txt");

            StreamReader rd = null;

            // read the settings file, this list is all the locations it can be in order of precedence.
            string[] fileLocations =
            {
                mFile,
                GPGSUtil.SlashesToPlatformSeparator(Path.Combine(GPGSUtil.RootPath, "Editor/projsettings.txt")),
                GPGSUtil.SlashesToPlatformSeparator("Assets/Editor/projsettings.txt")
            };

            foreach (string f in fileLocations)
            {
                if (File.Exists(f))
                {
                    // assign the reader and break out of the loop
                    rd = new StreamReader(f);
                    break;
                }
            }

            if (rd != null)
            {
                while (!rd.EndOfStream)
                {
                    string line = rd.ReadLine();
                    if (line == null || line.Trim().Length == 0)
                    {
                        break;
                    }

                    line = line.Trim();
                    string[] p = line.Split(new char[] {'='}, 2);
                    if (p.Length >= 2)
                    {
                        mDict[p[0].Trim()] = p[1].Trim();
                    }
                }

                rd.Close();
            }
        }

        public string Get(string key, Dictionary<string, string> overrides)
        {
            if (overrides.ContainsKey(key))
            {
                return overrides[key];
            }
            else if (mDict.ContainsKey(key))
            {
#if UNITY_2017_3_OR_NEWER
                return UnityWebRequest.UnEscapeURL(mDict[key]);
#else
                return WWW.UnEscapeURL(mDict[key]);
#endif
            }
            else
            {
                return string.Empty;
            }
        }

        public string Get(string key, string defaultValue)
        {
            if (mDict.ContainsKey(key))
            {
#if UNITY_2017_3_OR_NEWER
                return UnityWebRequest.UnEscapeURL(mDict[key]);
#else
                return WWW.UnEscapeURL(mDict[key]);
#endif
            }
            else
            {
                return defaultValue;
            }
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
#if UNITY_2017_3_OR_NEWER
            string escaped = UnityWebRequest.EscapeURL(val);
#else
            string escaped = WWW.EscapeURL(val);
#endif
            mDict[key] = escaped;
            mDirty = true;
        }

        public void Set(string key, bool val)
        {
            Set(key, val ? "true" : "false");
        }

        public void Save()
        {
            // See if we are building the plugin, and don't write the settings file
            string[] args = System.Environment.GetCommandLineArgs();
            foreach (string a in args)
            {
                if (a == "-g.building")
                {
                    mDirty = false;
                    break;
                }
            }

            if (!mDirty)
            {
                return;
            }

            StreamWriter wr = new StreamWriter(mFile, false);
            foreach (string key in mDict.Keys)
            {
                wr.WriteLine(key + "=" + mDict[key]);
            }

            wr.Close();
            mDirty = false;
        }

        public static void Reload()
        {
            sInstance = new GPGSProjectSettings();
        }
    }
}
