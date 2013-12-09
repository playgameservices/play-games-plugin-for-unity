/*
 * Copyright (C) 2013 Google Inc.
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
    string mFile;
    Dictionary<string, string> mDict = new Dictionary<string, string>();

    private GPGSProjectSettings() {
        string ds = Path.DirectorySeparatorChar.ToString();
        mFile = "Assets/Editor/projsettings.txt".Replace("/", ds);

        if (File.Exists(mFile)) {
            StreamReader rd = new StreamReader(mFile);
            while (!rd.EndOfStream) {
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

