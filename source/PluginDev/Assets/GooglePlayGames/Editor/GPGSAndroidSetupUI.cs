// <copyright file="GPGSAndroidSetupUI.cs" company="Google Inc.">
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

    public class GPGSAndroidSetupUI : EditorWindow
    {
        private string mAppId = string.Empty;

        [MenuItem("Window/Google Play Games/Setup/Android setup...", false, 1)]
        public static void MenuItemFileGPGSAndroidSetup()
        {
            EditorWindow.GetWindow(typeof(GPGSAndroidSetupUI));
        }

        public void OnEnable()
        {
            mAppId = GPGSProjectSettings.Instance.Get("proj.AppId");
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, position.width - 40, position.height - 40));
            GUILayout.Label(GPGSStrings.AndroidSetup.Title, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.AndroidSetup.Blurb);
            GUILayout.Space(10);

            GUILayout.Label(GPGSStrings.Setup.AppId, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.Setup.AppIdBlurb);
            mAppId = EditorGUILayout.TextField(GPGSStrings.Setup.AppId, mAppId);

            GUILayout.Space(10);
            if (GUILayout.Button(GPGSStrings.Setup.SetupButton))
            {
                DoSetup();
            }

            GUILayout.EndArea();
        }

        public void DoSetup()
        {
            PerformSetup(mAppId);
        }

        // Provide static access to setup for facilitating automated builds.
        public static void PerformSetup(string appId)
        {
            // check for valid app id
            if (!GPGSUtil.LooksLikeValidAppId(appId))
            {
                GPGSUtil.Alert(GPGSStrings.Setup.AppIdError);
                return;
            }

            GPGSProjectSettings.Instance.Set("proj.AppId", appId);
            GPGSProjectSettings.Instance.Save();

            // check that Android SDK is there
            if (!GPGSUtil.HasAndroidSdk())
            {
                Debug.LogError("Android SDK not found.");
                EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.SdkNotFound,
                    GPGSStrings.AndroidSetup.SdkNotFoundBlurb, GPGSStrings.Ok);
                return;
            }
          
            GPGSUtil.CopySupportLibs();

            // Generate AndroidManifest.xml
            GPGSUtil.GenerateAndroidManifest();
 
            // refresh assets, and we're done
            AssetDatabase.Refresh();
            GPGSProjectSettings.Instance.Set("android.SetupDone", true);
            GPGSProjectSettings.Instance.Save();
            EditorUtility.DisplayDialog(GPGSStrings.Success,
                GPGSStrings.AndroidSetup.SetupComplete, GPGSStrings.Ok);
        }
    }
}
