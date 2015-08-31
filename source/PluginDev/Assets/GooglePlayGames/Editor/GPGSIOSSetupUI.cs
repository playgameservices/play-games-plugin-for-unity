// <copyright file="GPGSIOSSetupUI.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using UnityEngine;
    using UnityEditor;

    public class GPGSIOSSetupUI : EditorWindow
    {
        private string mClientId = string.Empty;
        private string mBundleId = string.Empty;
        private string mWebClientId = string.Empty;

        [MenuItem("Window/Google Play Games/Setup/iOS setup...", false, 2)]
        public static void MenuItemGPGSIOSSetup()
        {
            EditorWindow window = EditorWindow.GetWindow(
                    typeof(GPGSIOSSetupUI), true, GPGSStrings.IOSSetup.Title);
            window.minSize = new Vector2(500, 300);
        }

        public void OnEnable()
        {
            mClientId = GPGSProjectSettings.Instance.Get(GPGSUtil.IOSCLIENTIDKEY);
            mBundleId = GPGSProjectSettings.Instance.Get(GPGSUtil.IOSBUNDLEIDKEY);
            mWebClientId = GPGSProjectSettings.Instance.Get(GPGSUtil.WEBCLIENTIDKEY);

            if (mBundleId.Trim().Length == 0)
            {
                mBundleId = PlayerSettings.bundleIdentifier;
            }
        }

        /// <summary>
        /// Save the specified clientId and bundleId to properties file.
        /// This maintains the configuration across instances of running Unity.
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="bundleId">Bundle identifier.</param>
        /// <param name="webClientId">web app clientId.</param>
        static void Save(string clientId, string bundleId, string webClientId)
        {
            GPGSProjectSettings.Instance.Set(GPGSUtil.IOSCLIENTIDKEY, clientId);
            GPGSProjectSettings.Instance.Set(GPGSUtil.IOSBUNDLEIDKEY, bundleId);
            GPGSProjectSettings.Instance.Set(GPGSUtil.WEBCLIENTIDKEY, webClientId);
            GPGSProjectSettings.Instance.Save();
        }

        public void OnGUI()
        {
            // Title
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label(GPGSStrings.IOSSetup.Blurb);
            GUILayout.Space(10);

            // Client ID field
            GUILayout.Label(GPGSStrings.IOSSetup.ClientIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.IOSSetup.ClientIdBlurb);
      
            mClientId = EditorGUILayout.TextField(GPGSStrings.IOSSetup.ClientId,
                mClientId, GUILayout.Width(450));

            GUILayout.Space(10);

            // Bundle ID field
            GUILayout.Label(GPGSStrings.IOSSetup.BundleIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.IOSSetup.BundleIdBlurb);
            mBundleId = EditorGUILayout.TextField(GPGSStrings.IOSSetup.BundleId, mBundleId,
                GUILayout.Width(450));
            
            GUILayout.Space(30);
            // Client ID field
            GUILayout.Label(GPGSStrings.Setup.WebClientIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.IOSSetup.ClientIdBlurb);

            mWebClientId = EditorGUILayout.TextField(GPGSStrings.Setup.WebAppClientId,
                mWebClientId, GUILayout.Width(450));

            GUILayout.Space(10);
            GUILayout.FlexibleSpace();
      
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Setup button
            if (GUILayout.Button(GPGSStrings.Setup.SetupButton))
            {
                DoSetup();
            }

            if (GUILayout.Button(GPGSStrings.Cancel))
            {
                this.Close();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Called by the UI to process the configuration.
        /// </summary>
        void DoSetup()
        {
            if (PerformSetup(mClientId, mBundleId, mWebClientId, null))
            {
                GPGSUtil.Alert(GPGSStrings.Success, GPGSStrings.IOSSetup.SetupComplete);
                Close();
            }
        }

        /// <summary>
        /// Performs the setup.  This is called externally to facilitate
        /// build automation.
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="bundleId">Bundle identifier.</param>
        /// <param name="webClientId">web app client id.</param>
        /// <param name="nearbySvcId">Nearby connections service Id.</param>
        public static bool PerformSetup(string clientId, string bundleId,
            string webClientId, string nearbySvcId)
        {

            if (!GPGSUtil.LooksLikeValidClientId(clientId))
            {
                GPGSUtil.Alert(GPGSStrings.Setup.ClientIdError);
                return false;
            }
            if (!GPGSUtil.LooksLikeValidBundleId(bundleId))
            {
                GPGSUtil.Alert(GPGSStrings.IOSSetup.BundleIdError);
                return false;
            }

            // nearby is optional - only set it up if present.
            if (nearbySvcId != null)
            {
                bool ok = NearbyConnectionUI.PerformSetup(nearbySvcId, false);
                if (!ok)
                {
                    return false;
                }
            }

            Save(clientId, bundleId, webClientId);
            GPGSUtil.UpdateGameInfo();

            // Finished!
            GPGSProjectSettings.Instance.Set(GPGSUtil.IOSSETUPDONEKEY, true);
            GPGSProjectSettings.Instance.Save();
            AssetDatabase.Refresh();
            return true;
        }
    }
}
