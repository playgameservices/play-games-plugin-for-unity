// <copyright file="GPGSPostBuild.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor.Callbacks;
    using UnityEditor;

    // Use the included xcode support for unity 5+,
    // otherwise use the backported code.
#if UNITY_5
    using UnityEditor.iOS.Xcode;
#else
    using GooglePlayGames.xcode;
#endif
    using GooglePlayGames;
    using GooglePlayGames.Editor.Util;
    using UnityEngine;

    public static class GPGSPostBuild
    {
        private const string UrlTypes = "CFBundleURLTypes";
        private const string UrlBundleName = "CFBundleURLName";
        private const string UrlScheme = "CFBundleURLSchemes";
        private const string PrincipalClass = "NSPrincipalClass";
        private const string PrincipalClassName = "CustomWebViewApplication";

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#if UNITY_5
            if (target != BuildTarget.iOS)
            {
                return;
            }
#else
            if (target != BuildTarget.iPhone)
            {
                return;
            }
#endif

            #if NO_GPGS
            Debug.Log("Removing AppController code since NO_GPGS is defined");
            // remove plugin code from generated project
            string pluginDir = pathToBuiltProject + "/Libraries/Plugins/iOS";
            if (System.IO.Directory.Exists(pluginDir))
            {
                GPGSUtil.WriteFile(pluginDir + "/GPGSAppController.mm", "// Empty since NO_GPGS is defined\n");
                return;
            }
            #else

            if (GetBundleId() == null)
            {
                UnityEngine.Debug.LogError("The iOS bundle ID has not been set up through the " +
                    "'iOS Setup' submenu of 'Google Play Games' - the generated xcode project will " +
                    "not work properly.");
                return;
            }

            //Copy the podfile into the project.
            string podfile = "Assets/GooglePlayGames/Editor/Podfile.txt";
            string destpodfile = pathToBuiltProject + "/Podfile";
            if (!System.IO.File.Exists(destpodfile))
            {
                FileUtil.CopyFileOrDirectory(podfile, destpodfile);
            }

            GPGSInstructionWindow w = EditorWindow.GetWindow<GPGSInstructionWindow>(
                true,
                "Building for IOS",
                true);
            w.UsingCocoaPod = CocoaPodHelper.Update(pathToBuiltProject);

            UnityEngine.Debug.Log("Adding URL Types for authentication using PlistBuddy.");

            UpdateGeneratedInfoPlistFile(pathToBuiltProject + "/Info.plist");
            UpdateGeneratedPbxproj(pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj");

        #endif
        }

        /// <summary>
        /// Updates the new project's Info.plist file to include an entry for the Url scheme mandated
        /// by the Google+ login. This means that the plist file needs to have an entry in the for
        /// indicated here: <see cref="https://developers.google.com/+/mobile/ios/getting-started#step_3_add_a_url_type"/>
        /// <para>This boils down to having an entry in the CFBundleURLTypes top level plist field with
        /// a CFBundleURLName equal to the bundle ID of the game, and a single element array for
        /// CFBundleURLSchemes also containing the bundle ID.</para>
        /// <para>We make use of the apple-provided PlistBuddy utility to edit the plist file.</para>
        /// </summary>
        /// <param name="pathToPlist">Path to plist.</param>
        private static void UpdateGeneratedInfoPlistFile(string pathToPlist)
        {
            PlistBuddyHelper buddy = PlistBuddyHelper.ForPlistFile(pathToPlist);

            // If the top-level UrlTypes field doesn't exist, add it here.
            if (buddy.EntryValue(UrlTypes) == null)
            {
                buddy.AddArray(UrlTypes);
            }

            var gamesSchemeIndex = GamesUrlSchemeIndex(buddy);

            EnsureGamesUrlScheme(buddy, gamesSchemeIndex);
            EnsurePrincipalClass(buddy);
        }


        /// <summary>
        /// Ensures the games URL scheme is well formed. This is done by removing the UrlScheme field
        /// and adding a fresh one in a known-good state.
        /// </summary>
        /// <param name="buddy">Buddy.</param>
        /// <param name="index">Index.</param>
        private static void EnsureGamesUrlScheme(PlistBuddyHelper buddy, int index)
        {
            buddy.RemoveEntry(UrlTypes, index, UrlScheme);
            buddy.AddArray(UrlTypes, index, UrlScheme);
            buddy.AddString(PlistBuddyHelper.ToEntryName(UrlTypes, index, UrlScheme, 0),
                GetBundleId());
        }

        /// <summary>
        /// Ensures the PrincipalClass is set and correct.
        /// </summary>
        /// <param name="buddy">Buddy.</param>
        private static void EnsurePrincipalClass(PlistBuddyHelper buddy)
        {
            buddy.RemoveEntry(PrincipalClass);
            buddy.AddString(PrincipalClass, PrincipalClassName);
        }

        private static string GetBundleId()
        {
            return GPGSProjectSettings.Instance.Get(GPGSUtil.IOSBUNDLEIDKEY);
        }

        /// <summary>
        /// Finds the index of the CFBundleURLTypes array where the entry for Play Games is stored. If
        /// this is not present, a new entry will be appended to the end of this array.
        /// </summary>
        /// <returns>The index in the CFBundleURLTypes array corresponding to Play Games.</returns>
        /// <param name="buddy">The helper corresponding to the plist file.</param>
        private static int GamesUrlSchemeIndex(PlistBuddyHelper buddy)
        {
            int index = 0;

            while (buddy.EntryValue(UrlTypes, index) != null)
            {
                var urlName = buddy.EntryValue(UrlTypes, index, UrlBundleName);

                if (GetBundleId().Equals(urlName))
                {
                    return index;
                }

                index++;
            }

            // The current array does not contain the Games url scheme - add a value to the end.
            buddy.AddDictionary(UrlTypes, index);
            buddy.AddString(PlistBuddyHelper.ToEntryName(UrlTypes, index, UrlBundleName),
                GetBundleId());

            return index;
        }

        /// <summary>
        /// Updates the generated pbxproj to reduce manual work required by developers. Currently
        /// this adds the '-fobjc-arc' flag for the Play Games ObjC source file.
        /// </summary>
        /// <param name="pbxprojPath">Pbxproj path.</param>
        private static void UpdateGeneratedPbxproj(string pbxprojPath)
        {
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(pbxprojPath));

            string target =
                proj.TargetGuidByName(PBXProject.GetUnityTargetName());
            string testTarget =
                proj.TargetGuidByName(PBXProject.GetUnityTestTargetName());

            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "$(inherited)");
            proj.AddBuildProperty(testTarget, "OTHER_LDFLAGS", "$(inherited)");
            proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(inherited)");
            proj.AddBuildProperty(testTarget, "HEADER_SEARCH_PATHS", "$(inherited)");
            proj.AddBuildProperty(target, "OTHER_CFLAGS", "$(inherited)");
            proj.AddBuildProperty(testTarget, "OTHER_CFLAGS", "$(inherited)");

            string fileGuid =
                 proj.FindFileGuidByProjectPath("Libraries/Plugins/iOS/GPGSAppController.mm");

            if (fileGuid == null)
            {
                // look in the legacy location
                fileGuid =
                    proj.FindFileGuidByProjectPath("Libraries/GPGSAppController.mm");
            }


            List<string> list = new List<string>();
            list.Add("-fobjc-arc");

            proj.SetCompileFlagsForFile(target, fileGuid, list);

            File.WriteAllText(pbxprojPath, proj.WriteToString());
        }
    }
}

