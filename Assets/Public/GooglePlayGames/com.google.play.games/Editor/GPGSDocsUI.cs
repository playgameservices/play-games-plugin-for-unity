﻿// <copyright file="GPGSDocsUI.cs" company="Google Inc.">
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

namespace GooglePlayGames.Editor
{
    using UnityEngine;
    using UnityEditor;

    public class GPGSDocsUI
    {
        [MenuItem("Google/Play Games/Documentation/Plugin Getting Started Guide...", false, 100)]
        public static void MenuItemGettingStartedGuide()
        {
            Application.OpenURL(GPGSStrings.ExternalLinks.GettingStartedGuideURL);
        }

        [MenuItem("Google/Play Games/Documentation/Google Play Games API...", false, 101)]
        public static void MenuItemPlayGamesServicesAPI()
        {
            Application.OpenURL(GPGSStrings.ExternalLinks.PlayGamesServicesApiURL);
        }

        [MenuItem("Google/Play Games/About/About the Plugin...", false, 300)]
        public static void MenuItemAbout()
        {
            string msg = GPGSStrings.AboutText + PluginVersion.VersionString + " (" + string.Format("0x{0:X8}", GooglePlayGames.PluginVersion.VersionInt) + ")";
            EditorUtility.DisplayDialog(GPGSStrings.AboutTitle, msg, GPGSStrings.Ok);
        }

        [MenuItem("Google/Play Games/About/License...", false, 301)]
        public static void MenuItemLicense()
        {
            EditorUtility.DisplayDialog(GPGSStrings.LicenseTitle, GPGSStrings.LicenseText,
                GPGSStrings.Ok);
        }
    }
}