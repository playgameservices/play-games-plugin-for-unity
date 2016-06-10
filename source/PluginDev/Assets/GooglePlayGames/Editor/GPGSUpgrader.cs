﻿// <copyright file="GPGSUpgrader.cs" company="Google Inc.">
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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// GPGS upgrader handles performing and upgrade tasks.
    /// </summary>
    [InitializeOnLoad]
    public class GPGSUpgrader
    {
        /// <summary>
        /// Initializes static members of the <see cref="GooglePlayGames.GPGSUpgrader"/> class.
        /// </summary>
        static GPGSUpgrader()
        {
            string prevVer = GPGSProjectSettings.Instance.Get(GPGSUtil.LASTUPGRADEKEY, "00000");
            if (prevVer != PluginVersion.VersionKey)
            {
                // if this is a really old version, upgrade to 911 first, then 915
                if (prevVer != PluginVersion.VersionKeyCPP)
                {
                    prevVer = Upgrade911(prevVer);
                }

                prevVer = Upgrade915(prevVer);

                prevVer = Upgrade927Patch(prevVer);

                // Upgrade to remove gpg version of jar resolver
                prevVer = Upgrade928(prevVer);

                prevVer = Upgrade930(prevVer);

                prevVer = Upgrade931(prevVer);

                // there is no migration needed to 930+
                if (prevVer != PluginVersion.VersionKey) {
                    Debug.Log("Upgrading from format version " + prevVer + " to " + PluginVersion.VersionKey);
                    prevVer = PluginVersion.VersionKey;
                }

                string msg = GPGSStrings.PostInstall.Text.Replace(
                                 "$VERSION",
                                 PluginVersion.VersionString);
                EditorUtility.DisplayDialog(GPGSStrings.PostInstall.Title, msg, "OK");
            }

            GPGSProjectSettings.Instance.Set(GPGSUtil.LASTUPGRADEKEY, prevVer);
            GPGSProjectSettings.Instance.Set(GPGSUtil.PLUGINVERSIONKEY,
                PluginVersion.VersionString);
            GPGSProjectSettings.Instance.Save();

            // clean up duplicate scripts if Unity 5+
            int ver = GPGSUtil.GetUnityMajorVersion();

            if (ver >= 5)
            {
                string[] paths =
                    {
                        "Assets/GooglePlayGames",
                        "Assets/Plugins/Android",
                        "Assets/PlayServicesResolver"
                    };
                foreach (string p in paths)
                {
                    CleanDuplicates(p);
                }

                // remove support lib from old location.
                string jarFile =
                    "Assets/Plugins/Android/libs/android-support-v4.jar";
                if (File.Exists(jarFile))
                {
                    File.Delete(jarFile);
                }

                // remove the massive play services client lib
                string clientDir = "Assets/Plugins/Android/google-play-services_lib";
                GPGSUtil.DeleteDirIfExists(clientDir);
            }

            // Check that there is a AndroidManifest.xml file
            if (!GPGSUtil.AndroidManifestExists())
            {
                GPGSUtil.GenerateAndroidManifest();
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Cleans the duplicate files.  There should not be any since
        /// we are keeping track of the .meta files.
        /// </summary>
        /// <param name="root">Root of the directory to clean.</param>
        private static void CleanDuplicates(string root)
        {
            string[] subDirs = Directory.GetDirectories(root);

            // look for .1 and .2
            string[] dups = Directory.GetFiles(root, "* 1.*");
            foreach (string d in dups)
            {
                Debug.Log("Deleting duplicate file: " + d);
                File.Delete(d);
            }

            dups = Directory.GetFiles(root, "* 2.*");
            foreach (string d in dups)
            {
                Debug.Log("Deleting duplicate file: " + d);
                File.Delete(d);
            }

            // recurse
            foreach (string s in subDirs)
            {
                CleanDuplicates(s);
            }
        }

        /// <summary>
        /// Upgrade to 0.9.31
        /// </summary>
        /// <remarks>
        /// This cleans up some unused files.
        /// </remarks>
        /// <param name="prevVer">Previous ver.</param>
        private static string Upgrade931(string prevVer)
        {
            string[] obsoleteFiles =
                {
                    "Assets/GooglePlayGames/Editor/GPGSExportPackageUI.cs",
                    "Assets/GooglePlayGames/Editor/GPGSExportPackageUI.cs.meta"
                };
            foreach (string file in obsoleteFiles)
            {
                if (File.Exists(file))
                {
                    Debug.Log("Deleting obsolete file: " + file);
                    File.Delete(file);
                }
            }

            return PluginVersion.VersionKey;
        }

        /// <summary>
        /// Upgrade to 930 from the specified prevVer.
        /// </summary>
        /// <param name="prevVer">Previous ver.</param>
        /// <returns>the version string upgraded to.</returns>
        private static string Upgrade930(string prevVer)
        {
            Debug.Log("Upgrading from format version " + prevVer + " to " + PluginVersion.VersionKeyNativeCRM);

            // As of 930, the CRM API is handled by the Native SDK, not GmsCore.
            string[] obsoleteFiles =
            {
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Games.cs",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Games.cs.meta",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/LoadPlayerStatsResultObject.cs",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/LoadPlayerStatsResultObject.cs.meta",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/PlayerStats.cs",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/PlayerStats.cs.meta",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/PlayerStatsObject.cs",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/PlayerStatsObject.cs.meta",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/Stats.cs",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/Stats.cs.meta",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/StatsObject.cs",
                "Assets/GooglePlayGames/Platforms/Android/Gms/Games/Stats/StatsObject.cs.meta"
            };

            // only delete these if we are not version 0.9.34
            if (PluginVersion.VersionKey !=  PluginVersion.VersionKeyJNIStats) {
                foreach (string file in obsoleteFiles)
                {
                    if (File.Exists(file))
                    {
                        Debug.Log("Deleting obsolete file: " + file);
                        File.Delete(file);
                    }
                }
            }

            return PluginVersion.VersionKeyNativeCRM;
        }

        private static string Upgrade928(string prevVer)
        {
            //remove the jar resolver and if found, then
            // warn the user that restarting the editor is required.
            string[] obsoleteFiles =
                {
                    "Assets/GooglePlayGames/Editor/JarResolverLib.dll",
                    "Assets/GooglePlayGames/Editor/JarResolverLib.dll.meta",
                    "Assets/GooglePlayGames/Editor/BackgroundResolution.cs",
                    "Assets/GooglePlayGames/Editor/BackgroundResolution.cs.meta"
                };

            bool found = File.Exists(obsoleteFiles[0]);

            foreach (string file in obsoleteFiles)
            {
                if (File.Exists(file))
                {
                    Debug.Log("Deleting obsolete file: " + file);
                    File.Delete(file);
                }
            }

            if (found)
            {
                GPGSUtil.Alert("This update made changes that requires that you restart the editor");
            }

            Debug.Log("Upgrading from version " + prevVer + " to " + PluginVersion.VersionKeyJarResolver);
            return PluginVersion.VersionKeyJarResolver;
        }

        /// <summary>
        /// Upgrade to 0.9.27a.
        /// </summary>
        /// <remarks>This removes the GPGGizmo class, which broke the editor</remarks>
        /// <returns>The patched version</returns>
        /// <param name="prevVer">Previous version</param>
        private static string Upgrade927Patch(string prevVer)
        {
            string[] obsoleteFiles =
                {
                    "Assets/GooglePlayGames/Editor/GPGGizmo.cs",
                    "Assets/GooglePlayGames/Editor/GPGGizmo.cs.meta",
                    "Assets/GooglePlayGames/BasicApi/OnStateLoadedListener.cs",
                    "Assets/GooglePlayGames/BasicApi/OnStateLoadedListener.cs.meta",
                    "Assets/GooglePlayGames/Platforms/Native/AndroidAppStateClient.cs",
                    "Assets/GooglePlayGames/Platforms/Native/AndroidAppStateClient.cs.meta",
                    "Assets/GooglePlayGames/Platforms/Native/UnsupportedAppStateClient.cs",
                    "Assets/GooglePlayGames/Platforms/Native/UnsupportedAppStateClient.cs.meta"
                };
            foreach (string file in obsoleteFiles)
            {
                if (File.Exists(file))
                {
                    Debug.Log("Deleting obsolete file: " + file);
                    File.Delete(file);
                }
            }

            return PluginVersion.VersionKey27Patch;
        }

        /// <summary>
        /// Upgrade to 915 from the specified prevVer.
        /// </summary>
        /// <param name="prevVer">Previous ver.</param>
        /// <returns>the version string upgraded to.</returns>
        private static string Upgrade915(string prevVer)
        {
            Debug.Log("Upgrading from format version " + prevVer + " to " + PluginVersion.VersionKeyU5);

            // all that was done was moving the Editor files to be in GooglePlayGames/Editor
            string[] obsoleteFiles =
                {
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

            foreach (string file in obsoleteFiles)
            {
                if (File.Exists(file))
                {
                    Debug.Log("Deleting obsolete file: " + file);
                    File.Delete(file);
                }
            }

            return PluginVersion.VersionKeyU5;
        }

        /// <summary>
        /// Upgrade to 911 from the specified prevVer.
        /// </summary>
        /// <param name="prevVer">Previous ver.</param>
        /// <returns>the version string upgraded to.</returns>
        private static string Upgrade911(string prevVer)
        {
            Debug.Log("Upgrading from format version " + prevVer + " to " + PluginVersion.VersionKeyCPP);

            // delete obsolete files, if they are there
            string[] obsoleteFiles =
                {
                    "Assets/GooglePlayGames/OurUtils/Utils.cs",
                    "Assets/GooglePlayGames/OurUtils/Utils.cs.meta",
                    "Assets/GooglePlayGames/OurUtils/MyClass.cs",
                    "Assets/GooglePlayGames/OurUtils/MyClass.cs.meta",
                    "Assets/Plugins/GPGSUtils.dll",
                    "Assets/Plugins/GPGSUtils.dll.meta",
                };

            foreach (string file in obsoleteFiles)
            {
                if (File.Exists(file))
                {
                    Debug.Log("Deleting obsolete file: " + file);
                    File.Delete(file);
                }
            }

            // delete obsolete directories, if they are there
            string[] obsoleteDirectories =
                {
                    "Assets/Plugins/Android/BaseGameUtils"
                };

            foreach (string directory in obsoleteDirectories)
            {
                if (Directory.Exists(directory))
                {
                    Debug.Log("Deleting obsolete directory: " + directory);
                    Directory.Delete(directory, true);
                }
            }

            Debug.Log("Done upgrading from format version " + prevVer + " to " + PluginVersion.VersionKeyCPP);
            return PluginVersion.VersionKeyCPP;
        }
    }
}
#endif
