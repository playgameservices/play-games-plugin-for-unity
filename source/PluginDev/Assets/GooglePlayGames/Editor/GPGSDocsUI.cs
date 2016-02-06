// <copyright file="GPGSDocsUI.cs" company="Google Inc.">
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
    using UnityEngine;
    using UnityEditor;

    public class GPGSDocsUI
    {
        [MenuItem("Window/Google Play Games/Documentation/Plugin Getting Started Guide...", false, 100)]
        public static void MenuItemGettingStartedGuide()
        {
            Application.OpenURL(GPGSStrings.ExternalLinks.GettingStartedGuideURL);
        }

        [MenuItem("Window/Google Play Games/Documentation/Google Play Games API...", false, 101)]
        public static void MenuItemPlayGamesServicesAPI()
        {
            Application.OpenURL(GPGSStrings.ExternalLinks.PlayGamesServicesApiURL);
        }

        [MenuItem("Window/Google Play Games/Downloads/Google+ SDK (iOS)...", false, 200)]
        public static void MenuItemGooglePlusSdk()
        {
            EditorUtility.DisplayDialog(GPGSStrings.ExternalLinks.GooglePlusSdkTitle,
                GPGSStrings.ExternalLinks.GooglePlusSdkBlurb, GPGSStrings.Ok);
            Application.OpenURL(GPGSStrings.ExternalLinks.GooglePlusSdkUrl);
        }

        [MenuItem("Window/Google Play Games/Downloads/Google Play Games C++ SDK (iOS)...", false, 201)]
        public static void MenuItemGooglePlayGamesSdk()
        {
            EditorUtility.DisplayDialog(GPGSStrings.ExternalLinks.GooglePlayGamesSdkTitle,
                GPGSStrings.ExternalLinks.GooglePlayGamesSdkBlurb, GPGSStrings.Ok);
            Application.OpenURL(GPGSStrings.ExternalLinks.GooglePlayGamesUrl);
        }

        [MenuItem("Window/Google Play Games/Downloads/Google Play Games SDK (Android)...", false, 203)]
        public static void MenuItemGooglePlayGamesAndroidSdk()
        {
            // check that Android SDK is there
            string sdkPath = GPGSUtil.GetAndroidSdkPath();
            if (!GPGSUtil.HasAndroidSdk())
            {
                EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.SdkNotFound,
                    GPGSStrings.AndroidSetup.SdkNotFoundBlurb, GPGSStrings.Ok);
                return;
            }

            bool launch = EditorUtility.DisplayDialog(
                              GPGSStrings.ExternalLinks.GooglePlayGamesAndroidSdkTitle,
                              GPGSStrings.ExternalLinks.GooglePlayGamesAndroidSdkBlurb, GPGSStrings.Yes,
                              GPGSStrings.No);
            if (launch)
            {
                string exeName =
                    sdkPath + GPGSUtil.SlashesToPlatformSeparator("/tools/android");
                string altExeName =
                    sdkPath + GPGSUtil.SlashesToPlatformSeparator("/tools/android.bat");

                EditorUtility.DisplayDialog(
                    GPGSStrings.ExternalLinks.GooglePlayGamesAndroidSdkTitle,
                    GPGSStrings.ExternalLinks.GooglePlayGamesAndroidSdkInstructions,
                    GPGSStrings.Ok);

                if (System.IO.File.Exists(exeName))
                {
                    System.Diagnostics.Process.Start(exeName);
                }
                else if (System.IO.File.Exists(altExeName))
                {
                    System.Diagnostics.Process.Start(altExeName);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        GPGSStrings.ExternalLinks.GooglePlayGamesSdkTitle,
                        GPGSStrings.ExternalLinks.GooglePlayGamesAndroidSdkManagerFailed,
                        GPGSStrings.Ok);
                }
            }
        }

        [MenuItem("Window/Google Play Games/About/About the Plugin...", false, 300)]
        public static void MenuItemAbout()
        {
            string msg = GPGSStrings.AboutText +
                PluginVersion.VersionString + " (" +
                         string.Format("0x{0:X8}", GooglePlayGames.PluginVersion.VersionInt) + ")";
            EditorUtility.DisplayDialog(GPGSStrings.AboutTitle, msg,
                GPGSStrings.Ok);
        }

        [MenuItem("Window/Google Play Games/About/License...", false, 301)]
        public static void MenuItemLicense()
        {
            EditorUtility.DisplayDialog(GPGSStrings.LicenseTitle, GPGSStrings.LicenseText,
                GPGSStrings.Ok);
        }
    }
}
#endif
