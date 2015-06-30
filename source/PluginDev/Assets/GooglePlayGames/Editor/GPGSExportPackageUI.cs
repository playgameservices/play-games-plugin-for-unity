// <copyright file="GPGSExportPackageUI.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.IO;
     
    public class GPGSExportPackageUI
    {
        [MenuItem("Window/Google Play Games/Export Plugin Package...", false, 999)]
        public static void MenuItemGPGSExportUIPackage()
        {
            List<string> exportList = new List<string>();

            Debug.Log("Exporting plugin.");

            AddToList(exportList, "Assets/GooglePlayGames");
            AddToList(exportList, "Assets/Plugins/Android/libs");
            AddToList(exportList, "Assets/Plugins/Android/MainLibProj");
            AddToList(exportList, "Assets/Plugins/iOS");

            exportList.Remove("Assets/GooglePlayGames/Editor/projsettings.txt");
            exportList.Remove("Assets/GooglePlayGames/Editor/GPGSExportPackageUI.cs");
            exportList.Remove("Assets/Plugins/Android/MainLibProj/AndroidManifest.xml");

            string fileList = "Final list of files to export (click for details)\n\n";
            foreach (string s in exportList)
            {
                fileList += "+ " + s + "\n";
            }

            Debug.Log(fileList);

            string msg = "GooglePlayGamesPlugin-" + GooglePlayGames.PluginVersion.VersionString + ".unitypackage";
            string path = EditorUtility.SaveFilePanel("Save plugin to", string.Empty,
                              msg, "unitypackage");

            if (path == null || path.Trim().Length == 0)
            {
                Debug.LogWarning("Cancelled plugin export.");
                return;
            }

            Debug.Log("Exporting plugin to " + path);
            File.Delete(path);
            AssetDatabase.ExportPackage(exportList.ToArray(), path);
            EditorUtility.DisplayDialog("Export finished", "Plugin exported to " + path, "OK");
        }

        private static void AddToList(List<string> exportList, string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                if (!exportList.Contains(path))
                {
                    exportList.Add(path);
                }

                foreach (string file in Directory.GetFileSystemEntries(path))
                {
                    if (!file.EndsWith(".meta"))
                    {
                        AddToList(exportList, file);
                    }
                }
            }
            else if (System.IO.File.Exists(path))
            {
                if (!exportList.Contains(path))
                {
                    exportList.Add(path);
                }
            }
            else
            {
                Debug.LogError("Can't add path to export list (not found): " + path);
            }
        }
    }
}
