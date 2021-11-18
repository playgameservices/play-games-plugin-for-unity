// <copyright file="GPGSUpgrader.cs" company="Google Inc.">
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

#if UNITY_ANDROID

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
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            Debug.Log("GPGSUpgrader start");

            GPGSProjectSettings.Instance.Set(GPGSUtil.LASTUPGRADEKEY, PluginVersion.VersionKey);
            GPGSProjectSettings.Instance.Set(GPGSUtil.PLUGINVERSIONKEY,
                PluginVersion.VersionString);
            GPGSProjectSettings.Instance.Save();

            bool isChanged = false;
            // Check that there is a AndroidManifest.xml file
            if (!GPGSUtil.AndroidManifestExists())
            {
                isChanged = true;
                GPGSUtil.GenerateAndroidManifest();
            }

            if (isChanged)
            {
                AssetDatabase.Refresh();
            }
            Debug.Log("GPGSUpgrader done");
        }
    }
}
#endif
