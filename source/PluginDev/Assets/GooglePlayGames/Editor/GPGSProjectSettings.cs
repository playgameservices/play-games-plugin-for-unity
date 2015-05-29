/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace GooglePlayGames {
    public class GPGSProjectSettings {
        private static GPGSProjectSettings sInstance = null;
        
        public static GPGSProjectSettings Instance {
            get {
                if (sInstance == null) {
                    sInstance = new GPGSProjectSettings();
                }
                return sInstance;
            }
        }
        
        bool mDirty = false;
        readonly string mFile;
        Dictionary<string, string> mDict = new Dictionary<string, string>();
        
        private GPGSProjectSettings() {
            string ds = Path.DirectorySeparatorChar.ToString();
            mFile = "Assets/GooglePlayGames/Editor/projsettings.txt".Replace("/", ds);
            
            StreamReader rd = null;
            // check for the file in the old location
            if (!File.Exists(mFile)) {
                string oldFile = "Assets/Editor/projsettings.txt".Replace("/", ds);
                if (File.Exists(oldFile)) {
                    rd = new StreamReader(oldFile);
                }
            }
            else {
                rd = new StreamReader(mFile);
            }
            
            
            if (rd != null) {
                while(!rd.EndOfStream) {
                    string line = rd.ReadLine();
                    if (line == null || line.Trim().Length == 0) {
                        break;
                    }
                    line = line.Trim();
                    string[] p = line.Split(new char[] { '=' }, 2);
                    if (p.Length >= 2) {
                        mDict[p[0].Trim()] = p[1].Trim();
                    }
                }
                rd.Close();
            }
            
        }
        
        public string Get(string key, string defaultValue) {
            if (mDict.ContainsKey(key)) {
                return mDict[key];
            } else {
                return defaultValue;
            }
        }
        
        public string Get(string key) {
            return Get(key, "");
        }
        
        public bool GetBool(string key, bool defaultValue) {
            return Get(key, defaultValue ? "true" : "false").Equals("true");
        }
        
        public bool GetBool(string key) {
            return Get(key, "false").Equals("true");
        }
        
        public void Set(string key, string val) {
            mDict[key] = val;
            mDirty = true;
        }
        
        public void Set(string key, bool val) {
            Set(key, val ? "true" : "false");
        }
        
        public void Save() {
            if (!mDirty) {
                return;
            }
            StreamWriter wr = new StreamWriter(mFile, false);
            foreach (string key in mDict.Keys) {
                wr.WriteLine(key + "=" + mDict[key]);
            }
            wr.Close();
            mDirty = false;
        }
    }
}

