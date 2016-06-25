// <copyright file="NearbyConnectionUI.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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

    public class NearbyConnectionUI : EditorWindow
    {
        private string mNearbyServiceId = string.Empty;

        [MenuItem("Window/Google Play Games/Setup/Nearby Connections setup...", false, 3)]
        public static void MenuItemFileGPGSAndroidSetup()
        {
            EditorWindow window = EditorWindow.GetWindow(
                typeof(NearbyConnectionUI), true, GPGSStrings.NearbyConnections.Title);
            window.minSize = new Vector2(400, 200);
        }

        public void OnEnable()
        {
            mNearbyServiceId = GPGSProjectSettings.Instance.Get(GPGSUtil.SERVICEIDKEY);
        }

        public void OnGUI()
        {
            GUI.skin.label.wordWrap = true;
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label(GPGSStrings.NearbyConnections.Blurb);
            GUILayout.Space(10);

            GUILayout.Label(GPGSStrings.Setup.NearbyServiceId, EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label(GPGSStrings.Setup.NearbyServiceBlurb);
            mNearbyServiceId = EditorGUILayout.TextField(GPGSStrings.Setup.NearbyServiceId,
                mNearbyServiceId,GUILayout.Width(350));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GPGSStrings.Setup.SetupButton,
                GUILayout.Width(100)))
            {
                DoSetup();
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                this.Close();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.EndVertical();
        }

        private void DoSetup()
        {
            if (PerformSetup(mNearbyServiceId, true))
            {
                EditorUtility.DisplayDialog(GPGSStrings.Success,
                    GPGSStrings.NearbyConnections.SetupComplete, GPGSStrings.Ok);
                this.Close();
            }
        }

        /// Provide static access to setup for facilitating automated builds.
        /// <param name="nearbyServiceId">The nearby connections service Id</param>
        /// <param name="androidBuild">true if building android</param>
        public static bool PerformSetup(string nearbyServiceId, bool androidBuild)
        {
            // check for valid app id
            if (!GPGSUtil.LooksLikeValidServiceId(nearbyServiceId))
            {
                GPGSUtil.Alert(GPGSStrings.Setup.ServiceIdError);
                return false;
            }

            GPGSProjectSettings.Instance.Set(GPGSUtil.SERVICEIDKEY, nearbyServiceId);
            GPGSProjectSettings.Instance.Save();

            if (androidBuild)
            {
                // create needed directories
                GPGSUtil.EnsureDirExists("Assets/Plugins");
                GPGSUtil.EnsureDirExists("Assets/Plugins/Android");

                // Generate AndroidManifest.xml
                GPGSUtil.GenerateAndroidManifest();

                // refresh assets, and we're done
                AssetDatabase.Refresh();
                GPGSProjectSettings.Instance.Set(GPGSUtil.NEARBYSETUPDONEKEY, true);
                GPGSProjectSettings.Instance.Save();
            }
            return true;
        }
    }
}
#endif
