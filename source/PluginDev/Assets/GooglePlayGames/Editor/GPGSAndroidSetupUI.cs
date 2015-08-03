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
        private string mWebClientId = string.Empty;
        private string mAppId = string.Empty;

        [MenuItem("Window/Google Play Games/Setup/Android setup...", false, 1)]
        public static void MenuItemFileGPGSAndroidSetup()
        {
            EditorWindow window = EditorWindow.GetWindow(
                typeof(GPGSAndroidSetupUI), true, GPGSStrings.AndroidSetup.Title);
            window.minSize = new Vector2(400, 300);
        }

        public void OnEnable()
        {
            mAppId = GPGSProjectSettings.Instance.Get(GPGSUtil.APPIDKEY);
            mWebClientId = GPGSProjectSettings.Instance.Get(GPGSUtil.WEBCLIENTIDKEY);
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

            GUILayout.Space(30);

            // Client ID field
            GUILayout.Label(GPGSStrings.Setup.WebClientIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.AndroidSetup.WebClientIdBlurb);
      
            GUILayout.Space(10);

            mWebClientId = EditorGUILayout.TextField(GPGSStrings.Setup.WebAppClientId,
                mWebClientId, GUILayout.Width(450));

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
            if (PerformSetup(mWebClientId, mAppId, null))
            {
                EditorUtility.DisplayDialog(GPGSStrings.Success,
                    GPGSStrings.AndroidSetup.SetupComplete, GPGSStrings.Ok);
                this.Close();
            }
           
        }

        /// <summary>
        /// Provide static access to setup for facilitating automated builds.
        /// </summary>
        /// <param name="webClientId">The oauth2 client id for the game.  This is only
        /// needed if the ID Token or access token are needed.</param> 
        /// <param name="appId">App identifier.</param>
        /// <param name="nearbySvcId">Optional nearby connection serviceId</param>
        public static bool PerformSetup(string webClientId, string appId, string nearbySvcId)
        {
            bool needTokenPermissions = false;

            if( !string.IsNullOrEmpty(webClientId) )
            {
                if (!GPGSUtil.LooksLikeValidClientId(webClientId))
                {
                    GPGSUtil.Alert(GPGSStrings.Setup.ClientIdError);
                    return false;
                }
                string serverAppId = webClientId.Split('-')[0];
                if (!serverAppId.Equals(appId)) {
                    GPGSUtil.Alert(GPGSStrings.Setup.AppIdMismatch);
                    return false;
                }
                needTokenPermissions = true;
            }
            else
            {
                // check for valid app id
                if (!GPGSUtil.LooksLikeValidAppId(appId))
                {
                    GPGSUtil.Alert(GPGSStrings.Setup.AppIdError);
                    return false;
                }
            }

            if (nearbySvcId != null)
            {
                if (!NearbyConnectionUI.PerformSetup(nearbySvcId, true))
                {
                    return false;
                }
            }

            GPGSProjectSettings.Instance.Set(GPGSUtil.APPIDKEY, appId);
            GPGSProjectSettings.Instance.Set(GPGSUtil.WEBCLIENTIDKEY, webClientId);
            GPGSProjectSettings.Instance.Save();
            GPGSUtil.UpdateGameInfo();

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
            GPGSUtil.GenerateAndroidManifest(needTokenPermissions);

            // refresh assets, and we're done
            AssetDatabase.Refresh();
            GPGSProjectSettings.Instance.Set("android.SetupDone", true);
            GPGSProjectSettings.Instance.Save();

            return true;
        }
    }
}
