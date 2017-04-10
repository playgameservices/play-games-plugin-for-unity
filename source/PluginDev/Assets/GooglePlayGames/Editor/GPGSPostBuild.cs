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

// Keep this file if NO_GPGS so we can clean up the xcode project
#if (UNITY_ANDROID || UNITY_IPHONE )

namespace GooglePlayGames.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor.Callbacks;
    using UnityEditor;
    using UnityEngine;

    // Use the included xcode support for unity 5+,
    // otherwise use the backported code.
#if (UNITY_IOS || UNITY_IPHONE)
    #if UNITY_5
        using UnityEditor.iOS.Xcode;
    #else
        using GooglePlayGames.xcode;
    #endif
#endif

    public static class GPGSPostBuild
    {
        private const string UrlTypes = "CFBundleURLTypes";
        private const string UrlBundleName = "CFBundleURLName";
        private const string UrlScheme = "CFBundleURLSchemes";
        private const string BundleSchemeKey = "com.google.BundleId";
        private const string ReverseClientIdSchemeKey = "com.google.ReverseClientId";

        [PostProcessBuild (99999)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#if UNITY_5
            if (target != BuildTarget.iOS)
            {
                if (!GPGSProjectSettings.Instance.GetBool(GPGSUtil.ANDROIDSETUPDONEKEY, false))
                {
                    EditorUtility.DisplayDialog("Google Play Games not configured!",
                        "Warning!!  Google Play Games was not configured, Game Services will not work correctly.",
                        "OK");
                }
                return;
            }
#else
            if (target != BuildTarget.iPhone)
            {
                return;
            }
#endif

#if UNITY_IOS
            #if NO_GPGS

            string[] filesToRemove = {
                "Libraries/Plugins/IOS/GPGSAppController.mm",
                "Libraries/Plugins/iOS/GPGSAppController.mm",
                "Libraries/GPGSAppController.mm",
                "Libraries/Plugins/IOS/GPGSAppController.h",
                "Libraries/Plugins/iOS/GPGSAppController.h",
                "Libraries/GPGSAppController.h",
                "Libraries/Plugins/IOS/CustomWebViewApplication.h",
                "Libraries/Plugins/iOS/CustomWebViewApplication.h",
                "Libraries/CustomWebViewApplication.h",
                "Libraries/Plugins/IOS/CustomWebViewApplication.mm",
                "Libraries/Plugins/iOS/CustomWebViewApplication.mm",
                "Libraries/CustomWebViewApplication.mm"
            };

            string pbxprojPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(pbxprojPath));

            foreach(string name in filesToRemove)
            {
                string fileGuid = proj.FindFileGuidByProjectPath(name);
                if (fileGuid != null)
                {
                    Debug.Log ("Removing " + name + " from xcode project");
                    proj.RemoveFile(fileGuid);
                }
            }

            File.WriteAllText(pbxprojPath, proj.WriteToString());

            #else

            if (!GPGSProjectSettings.Instance.GetBool(GPGSUtil.IOSSETUPDONEKEY, false))
            {
                EditorUtility.DisplayDialog("Google Play Games not configured!",
                    "Warning!!  Google Play Games was not configured, Game Services will not work correctly.",
                    "OK");
            }

            if (GetBundleId() == null)
            {
                UnityEngine.Debug.LogError("The iOS bundle ID has not been set up through the " +
                    "'iOS Setup' submenu of 'Google Play Games' - the generated xcode project will " +
                    "not work properly.");
                return;
            }

            UpdateGeneratedInfoPlistFile(pathToBuiltProject + "/Info.plist");
            UpdateGeneratedPbxproj(pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj");

            UnityEngine.Debug.Log("Adding URL Types for authentication using PlistBuddy.");
        #endif
#endif
        }

#if UNITY_IPHONE && !NO_GPGS
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

            AddURLScheme (buddy, BundleSchemeKey, GetBundleId ());
            AddURLScheme (buddy, ReverseClientIdSchemeKey, GetReverseClientId());
        }

        /// <summary>
        /// Adds the URL scheme to the plist.
        /// If the key already exists, the value is updated to the given value.
        /// If the key cannot be found, it is added.
        /// </summary>
        /// <param name="buddy">buddy - the plist helper to use.</param>
        /// <param name="key">Key - the url scheme key to look for</param>
        /// <param name="value">Value - the value of the scheme.</param>
        private static void AddURLScheme (PlistBuddyHelper buddy, string key, string value)
        {
            int index = 0;

            while (buddy.EntryValue(UrlTypes, index) != null)
            {
               string urlName = buddy.EntryValue(UrlTypes, index, UrlBundleName);

                if (key.Equals(urlName))
                {
                    // remove the existing value
                    buddy.RemoveEntry (UrlTypes, index, UrlScheme);
                    //add the array back
                    buddy.AddArray(UrlTypes, index, UrlScheme);
                    //add the value
                    buddy.AddString (PlistBuddyHelper.ToEntryName (UrlTypes, index, UrlScheme, 0),
                        value);
                    return;
                }

                index++;
            }

            // not found, add new entry
            buddy.AddDictionary(UrlTypes, index);
            buddy.AddString(PlistBuddyHelper.ToEntryName(UrlTypes, index, UrlBundleName),
                key);
            //add the array
            buddy.AddArray(UrlTypes, index, UrlScheme);
            //add the value
            buddy.AddString (PlistBuddyHelper.ToEntryName (UrlTypes, index, UrlScheme, 0),
                value);
        }
#endif

        private static string GetBundleId()
        {
            return GPGSProjectSettings.Instance.Get(GPGSUtil.IOSBUNDLEIDKEY);
        }

        private static string GetReverseClientId()
        {
            string clientId = GPGSProjectSettings.Instance.Get(GPGSUtil.IOSCLIENTIDKEY);
            string[] parts = clientId.Split ('.');
            string revClientId = "";
            foreach (string p in parts)
            {
                if (revClientId.Length == 0)
                {
                    revClientId = p;
                }
                else
                {
                    revClientId = p + "." + revClientId;
                }
            }
            return revClientId;
        }

#if UNITY_IOS || UNITY_IPHONE
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
#endif
    }
}
#endif
