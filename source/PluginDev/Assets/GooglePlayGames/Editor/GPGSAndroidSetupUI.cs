// <copyright file="GPGSAndroidSetupUI.cs" company="Google Inc.">
// Copyright (C) Google Inc. All Rights Reserved.
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

    public class GPGSAndroidSetupUI : EditorWindow
    {
        private string mAppId = string.Empty;

        [MenuItem("Window/Google Play Games/Setup/Android setup...", false, 1)]
        public static void MenuItemFileGPGSAndroidSetup()
        {
            EditorWindow window = EditorWindow.GetWindow(
                typeof(GPGSAndroidSetupUI), true, GPGSStrings.AndroidSetup.Title);
            window.minSize = new Vector2(400, 200);
        }

        public void OnEnable()
        {
            mAppId = GPGSProjectSettings.Instance.Get("proj.AppId");
        }

        public void OnGUI()
        {
            GUI.skin.label.wordWrap = true;
            GUILayout.BeginVertical();

            GUILayout.Space(10);
            GUILayout.Label(GPGSStrings.AndroidSetup.Blurb);

            GUILayout.Label(GPGSStrings.Setup.AppId, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.Setup.AppIdBlurb);
            GUILayout.Space(10);

            mAppId = EditorGUILayout.TextField(GPGSStrings.Setup.AppId,
                mAppId,GUILayout.Width(300));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GPGSStrings.Setup.SetupButton,
                GUILayout.Width(100)))
            {
                DoSetup();
            }

            if (GUILayout.Button("Cancel",GUILayout.Width(100)))
            {
                this.Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.EndVertical();
        }

        public void DoSetup()
        {
            if (PerformSetup(mAppId, null))
            {
                EditorUtility.DisplayDialog(GPGSStrings.Success,
                    GPGSStrings.AndroidSetup.SetupComplete, GPGSStrings.Ok);
                this.Close();
            }
           
        }

        /// <summary>
        /// Provide static access to setup for facilitating automated builds.
        /// </summary>
        /// <param name="appId">App identifier.</param>
        /// <param name="nearbySvcId">Optional nearby connection serviceId</param>
        public static bool PerformSetup(string appId, string nearbySvcId)
        {
            // check for valid app id
            if (!GPGSUtil.LooksLikeValidAppId(appId))
            {
                GPGSUtil.Alert(GPGSStrings.Setup.AppIdError);
                return false;
            }

            if (nearbySvcId != null)
            {
                if (!NearbyConnectionUI.PerformSetup(nearbySvcId, true))
                {
                    return false;
                }
            }

            GPGSProjectSettings.Instance.Set("proj.AppId", appId);
            GPGSProjectSettings.Instance.Save();

            // check that Android SDK is there
            if (!GPGSUtil.HasAndroidSdk())
            {
                Debug.LogError("Android SDK not found.");
                EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.SdkNotFound,
                    GPGSStrings.AndroidSetup.SdkNotFoundBlurb, GPGSStrings.Ok);
                return false;
            }

            GPGSUtil.CopySupportLibs();

            // Generate AndroidManifest.xml
            GPGSUtil.GenerateAndroidManifest();

            // refresh assets, and we're done
            AssetDatabase.Refresh();
            GPGSProjectSettings.Instance.Set("android.SetupDone", true);
            GPGSProjectSettings.Instance.Save();

            return true;
        }
    }
}
