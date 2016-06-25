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

// Keep this file even if NO_GPGS is defined, so the xcode project can be cleaned up.
#if (UNITY_ANDROID || UNITY_IPHONE)

namespace GooglePlayGames.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

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
            string ds = Path.DirectorySeparatorChar.ToString();
            mFile = "ProjectSettings/GooglePlayGameSettings.txt".Replace("/", ds);

            StreamReader rd = null;

            // read the settings file, this list is all the locations it can be in order of precedence.
            string[] fileLocations =
                {
                    mFile,
                    "Assets/GooglePlayGames/Editor/projsettings.txt".Replace("/", ds),
                    "Assets/Editor/projsettings.txt".Replace("/", ds)
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
                    string[] p = line.Split(new char[] { '=' }, 2);
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
                return WWW.UnEscapeURL(mDict[key]);
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
                string val = WWW.UnEscapeURL(mDict[key]);
                return val;
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
            string escaped = WWW.EscapeURL(val);
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
    }
}
#endif
