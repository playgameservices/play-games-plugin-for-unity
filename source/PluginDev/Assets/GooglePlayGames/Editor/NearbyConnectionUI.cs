// <copyright file="NearbyConnectionUI.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using UnityEngine;
    using UnityEditor;

    public class NearbyConnectionUI : EditorWindow
    {
        private string mNearbyServiceId = string.Empty;

        [MenuItem("Window/Google Play Games/Setup/Nearby Connections setup...", false, 3)]
        public static void MenuItemFileGPGSAndroidSetup()
        {
            EditorWindow.GetWindow(typeof(NearbyConnectionUI));
        }

        public void OnEnable()
        {
            mNearbyServiceId = GPGSProjectSettings.Instance.Get(GPGSUtil.SERVICEIDKEY);
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, position.width - 40, position.height - 40));
            GUILayout.Label(GPGSStrings.NearbyConnections.Title, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.NearbyConnections.Blurb);
            GUILayout.Space(10);
            
            GUILayout.Label(GPGSStrings.Setup.NearbyServiceId, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.Setup.NearbyServiceBlurb);
            mNearbyServiceId = EditorGUILayout.TextField(GPGSStrings.Setup.NearbyServiceId,
                mNearbyServiceId);
            
            GUILayout.Space(10);
            if (GUILayout.Button(GPGSStrings.Setup.SetupButton))
            {
                DoSetup();
            }

            GUILayout.EndArea();
        }

        private void DoSetup()
        {
            PerformSetup(mNearbyServiceId);
        }

        /// Provide static access to setup for facilitating automated builds.
        public static void PerformSetup(string nearbyServiceId)
        {
            // check for valid app id
            if (!GPGSUtil.LooksLikeValidServiceId(nearbyServiceId))
            {
                GPGSUtil.Alert(GPGSStrings.Setup.ServiceIdError);
                return;
            }

            GPGSProjectSettings.Instance.Set(GPGSUtil.SERVICEIDKEY, nearbyServiceId);
            GPGSProjectSettings.Instance.Save();

            // create needed directories
            GPGSUtil.EnsureDirExists("Assets/Plugins");
            GPGSUtil.EnsureDirExists("Assets/Plugins/Android");
 
            GPGSUtil.CopySupportLibs();

            // Generate AndroidManifest.xml
            GPGSUtil.GenerateAndroidManifest();

            // refresh assets, and we're done
            AssetDatabase.Refresh();
            GPGSProjectSettings.Instance.Set("android.NearbySetupDone", true);
            GPGSProjectSettings.Instance.Save();
            EditorUtility.DisplayDialog(GPGSStrings.Success,
                GPGSStrings.NearbyConnections.SetupComplete, GPGSStrings.Ok);
        }
    }
}
