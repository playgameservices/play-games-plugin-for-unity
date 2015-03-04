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

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[InitializeOnLoad]
public class GPGSUpgrader {
    private static string CUR_VER = "00911";

    static GPGSUpgrader() {
        string prevVer = GPGSProjectSettings.Instance.Get("lastUpgrade", "00000");
        if (prevVer != CUR_VER) {
            Upgrade(prevVer);
            string msg = GPGSStrings.PostInstall.Text.Replace("$VERSION",
                             GooglePlayGames.PluginVersion.VersionString);
            EditorUtility.DisplayDialog(GPGSStrings.PostInstall.Title, msg, "OK");
        }
    }

    private static void Upgrade(string prevVer) {
        Debug.Log("Upgrading from format version " + prevVer + " to " + CUR_VER);

        // delete obsolete files, if they are there
        string[] obsoleteFiles = {
            "Assets/GooglePlayGames/OurUtils/Utils.cs",
            "Assets/GooglePlayGames/OurUtils/Utils.cs.meta",
            "Assets/GooglePlayGames/OurUtils/MyClass.cs",
            "Assets/GooglePlayGames/OurUtils/MyClass.cs.meta",
            "Assets/Plugins/GPGSUtils.dll",
            "Assets/Plugins/GPGSUtils.dll.meta",
        };

        foreach (string file in obsoleteFiles) {
            if (File.Exists(file)) {
                Debug.Log("Deleting obsolete file: " + file);
                File.Delete(file);
            }
        }

        // delete obsolete directories, if they are there
        string[] obsoleteDirectories = {
            "Assets/GooglePlayGames/Platforms/Android",
            "Assets/GooglePlayGames/Platforms/iOS",
            "Assets/Plugins/Android/BaseGameUtils",
        };

        foreach (string directory in obsoleteDirectories) {
            if (Directory.Exists(directory)) {
                Debug.Log("Deleting obsolete directory: " + directory);
                Directory.Delete(directory, true);
            }
        }

        GPGSProjectSettings.Instance.Set("lastUpgrade", CUR_VER);
        GPGSProjectSettings.Instance.Save();
        Debug.Log("Done upgrading from format version " + prevVer + " to " + CUR_VER);
        AssetDatabase.Refresh();
    }
}
