// <copyright file="BackgroundResolution.cs" company="Google Inc.">
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

#if UNITY_ANDROID || UNITY_EDITOR
namespace GooglePlayGames
{
    using UnityEditor;
    using Google.GamesJarResolver;
    using System.Collections.Generic;
    using UnityEngine;


    /// <summary>
    /// Background resolution of Play services client jars.  This only
    /// runs when building for Android.
    /// </summary>
    [InitializeOnLoad]
    public class BackgroundResolution : AssetPostprocessor
    {
        private static PlayServicesSupport svcSupport;

        /// <summary>
        /// Initializes the <see cref="GooglePlayGames.BackgroundResolution"/> class.
        /// </summary>
        static BackgroundResolution()
        {
            svcSupport = PlayServicesSupport.CreateInstance(
                "Google.GPGS",
                EditorPrefs.GetString("AndroidSdkRoot"),
                "ProjectSettings");

            AddDependencies();
        }

        /// <summary>
        /// Adds the dependencies needed by the plugin.
        /// </summary>
        private static void AddDependencies()
        {
            svcSupport.DependOn("com.google.android.gms",
                "play-services-games",
                PluginVersion.PlayServicesVersionConstraint);

            // need nearby too, even if it is not used.
            svcSupport.DependOn("com.google.android.gms",
                "play-services-nearby",
                PluginVersion.PlayServicesVersionConstraint);

            // Plus is needed if Token support is enabled.
            svcSupport.DependOn("com.google.android.gms",
                "play-services-plus",
                PluginVersion.PlayServicesVersionConstraint);

            //Marshmallow permissions requires app-compat
            svcSupport.DependOn("com.android.support",
                "appcompat-v7",
                "23.1.0+");
        }

#if UNITY_5
        /// <summary>
        /// Resolve the dependencies.  This can added to the menu.
        /// </summary>
        [MenuItem("Window/Google Play Games/Resolve Client Jars")]
        public static void Resolve()
        {

            svcSupport.ClearDependencies();

            AddDependencies();
            Dictionary<string, Dependency> deps =
                svcSupport.ResolveDependencies(true);

            svcSupport.CopyDependencies(deps, "Assets/Plugins/Android",
                HandleOverwriteConfirmation);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Android Jar Dependencies",
                "Resolution Complete", "OK");
        }
#endif
        // Called when assets have changed.
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
#if UNITY_5
            if (GPGSUtil.GetUnityMajorVersion() >= 5)
            {
                Dictionary<string, Dependency> deps =
                    svcSupport.ResolveDependencies(true);
                svcSupport.CopyDependencies(deps, "Assets/Plugins/Android", HandleOverwriteConfirmation);

                AssetDatabase.Refresh();
                Debug.Log("Android Jar Dependencies: Resolution Complete");
            }
#endif
        }

        /// <summary>
        /// Handles the overwrite confirmation.
        /// </summary>
        /// <returns><c>true</c>, if overwrite confirmation was handled, <c>false</c> otherwise.</returns>
        /// <param name="oldDep">Old dep.</param>
        /// <param name="newDep">New dep.</param>
        static bool HandleOverwriteConfirmation(Dependency oldDep, Dependency newDep)
        {
            // Don't prompt overwriting the same version, just do it.
            if (oldDep.BestVersion != newDep.BestVersion)
            {
                string msg = "Remove or replace " + oldDep.Artifact + " version " +
                             oldDep.BestVersion + " with version " + newDep.BestVersion + "?";
                return EditorUtility.DisplayDialog("Android Jar Dependencies",
                    msg, "OK", "Keep");
            }
            return true;
        }
    }
}
#endif
