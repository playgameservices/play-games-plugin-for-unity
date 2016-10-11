// <copyright file="GPGSDependencies.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
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

using System;
using System.Collections.Generic;
using UnityEditor;

/// AdMob dependencies file.
[InitializeOnLoad]
public class GPGSDependencies : AssetPostprocessor
{
#if UNITY_ANDROID
        /// <summary>Instance of the PlayServicesSupport resolver</summary>
        public static object svcSupport;
#endif  // UNITY_ANDROID

        /// Initializes static members of the class.
        static GPGSDependencies() { RegisterDependencies(); }

        public static void RegisterDependencies() {
#if UNITY_ANDROID
            // Setup the resolver using reflection as the module may not be
            // available at compile time.
            Type playServicesSupport = Google.VersionHandler.FindClass(
                "Google.JarResolver", "Google.JarResolver.PlayServicesSupport");
            if (playServicesSupport == null) {
                return;
            }
            svcSupport = svcSupport ?? Google.VersionHandler.InvokeStaticMethod(
                playServicesSupport, "CreateInstance",
                new object[] {
                    "GooglePlayGames",
                    EditorPrefs.GetString("AndroidSdkRoot"),
                    "ProjectSettings"
                });

            Google.VersionHandler.InvokeInstanceMethod(
                svcSupport, "DependOn",
                new object[] { "com.google.android.gms", "play-services-games",
                               PluginVersion.PlayServicesVersionConstraint },
                namedArgs: new Dictionary<string, object>() {
                    {"packageIds", new string[] { "extra-google-m2repository" } }
                });

            Google.VersionHandler.InvokeInstanceMethod(
                svcSupport, "DependOn",
                new object[] { "com.google.android.gms", "play-services-nearby",
                               PluginVersion.PlayServicesVersionConstraint },
                namedArgs: new Dictionary<string, object>() {
                    {"packageIds", new string[] { "extra-google-m2repository" } }
                });

            // Auth is needed for getting the token and email.
            Google.VersionHandler.InvokeInstanceMethod(
                    svcSupport, "DependOn",
                    new object[] { "com.google.android.gms", "play-services-auth",
                         PluginVersion.PlayServicesVersionConstraint },
                    namedArgs: new Dictionary<string, object>() {
                        {"packageIds", new string[] { "extra-google-m2repository" } }
            });

            // if google+ is needed, add it
            if (GameInfo.RequireGooglePlus())
            {
                Google.VersionHandler.InvokeInstanceMethod(
                        svcSupport, "DependOn",
                        new object[] { "com.google.android.gms", "play-services-plus",
                         PluginVersion.PlayServicesVersionConstraint },
                        namedArgs: new Dictionary<string, object>() {
                        {"packageIds", new string[] { "extra-google-m2repository" } }
                });
            }

            Google.VersionHandler.InvokeInstanceMethod(
                svcSupport, "DependOn",
                new object[] { "com.android.support", "support-v4", "23.1+" },
                namedArgs: new Dictionary<string, object>() {
                    {"packageIds", new string[] { "extra-android-m2repository" } }
                });
#elif UNITY_IOS
            Type iosResolver = Google.VersionHandler.FindClass(
                "Google.IOSResolver", "Google.IOSResolver");
            if (iosResolver == null) {
                return;
            }
            Google.VersionHandler.InvokeStaticMethod(
                iosResolver, "AddPod",
                new object[] { "GooglePlayGames" },
                namedArgs: new Dictionary<string, object>() {
                    { "version", "5.0+" },
                    { "bitcodeEnabled", false },
                });
#endif  // UNITY_IOS
        }

        // Handle delayed loading of the dependency resolvers.
        private static void OnPostprocessAllAssets(
                string[] importedAssets, string[] deletedAssets,
                string[] movedAssets, string[] movedFromPath) {
            foreach (string asset in importedAssets) {
                if (asset.Contains("IOSResolver") ||
                    asset.Contains("JarResolver")) {
                    RegisterDependencies();
                    break;
                }
            }
        }
}

}
