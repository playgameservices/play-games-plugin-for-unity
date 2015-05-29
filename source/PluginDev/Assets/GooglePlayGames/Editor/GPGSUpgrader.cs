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

namespace GooglePlayGames {

    [InitializeOnLoad]
    public class GPGSUpgrader {
        
        static GPGSUpgrader() {
            string prevVer = GPGSProjectSettings.Instance.Get("lastUpgrade", "00000");
            if (prevVer != PluginVersion.VersionKey) {
                // if this is a really old version, upgrade to 911 first, then 915
                if (prevVer != PluginVersion.VersionKeyCPP) {
                    prevVer = Upgrade911(prevVer);
                }
                prevVer = Upgrade915(prevVer);
                string msg = GPGSStrings.PostInstall.Text.Replace("$VERSION",
                                                                  PluginVersion.VersionString);
                EditorUtility.DisplayDialog(GPGSStrings.PostInstall.Title, msg, "OK");
            }
            GPGSProjectSettings.Instance.Set("lastUpgrade", prevVer);
            GPGSProjectSettings.Instance.Save();
            AssetDatabase.Refresh();
        }
        
        private static string Upgrade915(string prevVer) {
            Debug.Log("Upgrading from format version " + prevVer + " to " + PluginVersion.VersionKeyU5);
            
            // all that was done was moving the Editor files to be in GooglePlayGames/Editor
            string[] obsoleteFiles = {
                "Assets/Editor/GPGSAndroidSetupUI.cs",
                "Assets/Editor/GPGSAndroidSetupUI.cs.meta",
                "Assets/Editor/GPGSDocsUI.cs",
                "Assets/Editor/GPGSDocsUI.cs.meta",
                "Assets/Editor/GPGSIOSSetupUI.cs",
                "Assets/Editor/GPGSIOSSetupUI.cs.meta",
                "Assets/Editor/GPGSInstructionWindow.cs",
                "Assets/Editor/GPGSInstructionWindow.cs.meta",
                "Assets/Editor/GPGSPostBuild.cs",
                "Assets/Editor/GPGSPostBuild.cs.meta",
                "Assets/Editor/GPGSProjectSettings.cs",
                "Assets/Editor/GPGSProjectSettings.cs.meta",
                "Assets/Editor/GPGSStrings.cs",
                "Assets/Editor/GPGSStrings.cs.meta",
                "Assets/Editor/GPGSUpgrader.cs",
                "Assets/Editor/GPGSUpgrader.cs.meta",
                "Assets/Editor/GPGSUtil.cs",
                "Assets/Editor/GPGSUtil.cs.meta",
                "Assets/Editor/GameInfo.template",
                "Assets/Editor/GameInfo.template.meta",
                "Assets/Editor/PlistBuddyHelper.cs",
                "Assets/Editor/PlistBuddyHelper.cs.meta",
                "Assets/Editor/PostprocessBuildPlayer",
                "Assets/Editor/PostprocessBuildPlayer.meta",
                "Assets/Editor/ios_instructions",
                "Assets/Editor/ios_instructions.meta",
                "Assets/Editor/projsettings.txt",
                "Assets/Editor/projsettings.txt.meta",
                "Assets/Editor/template-AndroidManifest.txt",
                "Assets/Editor/template-AndroidManifest.txt.meta",
                "Assets/Plugins/Android/libs/armeabi/libgpg.so",
                "Assets/Plugins/Android/libs/armeabi/libgpg.so.meta",
                "Assets/Plugins/iOS/GPGSAppController 1.h",
                "Assets/Plugins/iOS/GPGSAppController 1.h.meta",
                "Assets/Plugins/iOS/GPGSAppController 1.mm",
                "Assets/Plugins/iOS/GPGSAppController 1.mm.meta"
            };
            
            foreach (string file in obsoleteFiles) {
                if (File.Exists(file)) {
                    Debug.Log("Deleting obsolete file: " + file);
                    File.Delete(file);
                }
            }

            return PluginVersion.VersionKeyU5;
        }
        
        private static string Upgrade911(string prevVer) {
            Debug.Log("Upgrading from format version " + prevVer + " to " + PluginVersion.VersionKeyCPP);
            
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
            
            Debug.Log("Done upgrading from format version " + prevVer + " to " + PluginVersion.VersionKeyCPP);
            return PluginVersion.VersionKeyCPP;
        }
    }
}
