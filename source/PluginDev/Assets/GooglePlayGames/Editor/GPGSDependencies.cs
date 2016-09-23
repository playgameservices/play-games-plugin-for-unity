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
    using UnityEditor;

    /// <summary>
    /// Play-Services Dependencies for Google Play Games.
    /// </summary>
    [InitializeOnLoad]
    public static class GPGSDependencies
    {
#if UNITY_ANDROID
        /// <summary>
        /// The name of your plugin.  This is used to create a settings file
        /// which contains the dependencies specific to your plugin.
        /// </summary>
        private static readonly string PluginName = "GooglePlayGames";

        /// <summary>Instance of the PlayServicesSupport resolver</summary>
        public static Google.JarResolver.PlayServicesSupport svcSupport;
#endif  // UNITY_ANDROID

        /// <summary>
        /// Initializes static members of the <see cref="SampleDependencies"/> class.
        /// </summary>
        static GPGSDependencies()
        {
#if UNITY_ANDROID
            svcSupport = Google.JarResolver.PlayServicesSupport.CreateInstance(
                                             PluginName,
                                             EditorPrefs.GetString("AndroidSdkRoot"),
                                             "ProjectSettings");

#endif  // UNITY_ANDROID
            RegisterDependencies();
        }

        /// <summary>
        /// Registers the dependencies.
        /// </summary>
        public static void RegisterDependencies()
        {
#if UNITY_ANDROID
            svcSupport.DependOn("com.google.android.gms",
                "play-services-games",
                PluginVersion.PlayServicesVersionConstraint,
                packageIds: new string[] { "extra-google-m2repository" });

            // need nearby too, even if it is not used.
            svcSupport.DependOn("com.google.android.gms",
                "play-services-nearby",
                PluginVersion.PlayServicesVersionConstraint,
                packageIds: new string[] { "extra-google-m2repository" });

            // Marshmallow permissions requires app-compat
            svcSupport.DependOn("com.android.support",
                "support-v4",
                "23.1+",
                packageIds: new string[] { "extra-android-m2repository" });
#elif UNITY_IOS
            Google.IOSResolver.AddPod("GooglePlayGames", "5.0+",
                                      bitcodeEnabled: false,
                                      minTargetSdk: "7.0");
#endif
        }
    }
}

