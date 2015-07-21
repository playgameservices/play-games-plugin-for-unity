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
		private const string GameInfoPath = "Assets/GooglePlayGames/GameInfo.cs";
		
		private string mClientId = string.Empty;
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
            mAppId = GPGSProjectSettings.Instance.Get("proj.AppId");
            mClientId = GPGSProjectSettings.Instance.Get(GPGSUtil.ANDROIDCLIENTIDKEY);
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

            // Client ID field
            GUILayout.Label(GPGSStrings.Setup.ClientIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.AndroidSetup.ClientIdBlurb);
      
            mClientId = EditorGUILayout.TextField(GPGSStrings.Setup.ClientId,
                mClientId, GUILayout.Width(450));

            GUILayout.Space(10);

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
            if (PerformSetup(mClientId, mAppId, null))
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
		public static bool PerformSetup(string clientId, string appId, string nearbySvcId)
		{
			if(clientId != null)
			{
				if (!GPGSUtil.LooksLikeValidClientId(clientId))
				{
					GPGSUtil.Alert(GPGSStrings.Setup.ClientIdError);
					return false;
				}
				appId = clientId.Split('-')[0];
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

            GPGSProjectSettings.Instance.Set("proj.AppId", appId);
			GPGSProjectSettings.Instance.Set(GPGSUtil.ANDROIDCLIENTIDKEY, clientId);
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
            GPGSUtil.GenerateAndroidManifest();
			
			FillInAppData(GameInfoPath, GameInfoPath, clientId);

            // refresh assets, and we're done
            AssetDatabase.Refresh();
            GPGSProjectSettings.Instance.Set("android.SetupDone", true);
            GPGSProjectSettings.Instance.Save();

            return true;
        }
		
		/// <summary>
		/// Helper function to do search and replace of the client and bundle ids.
		/// </summary>
		/// <param name="sourcePath">Source path.</param>
		/// <param name="outputPath">Output path.</param>
		/// <param name="clientId">Client identifier.</param>
		/// <param name="bundleId">Bundle identifier.</param>
		private static void FillInAppData(string sourcePath,
		                                  string outputPath,
		                                  string clientId)
		{
			string fileBody = GPGSUtil.ReadFully(sourcePath);
			fileBody = fileBody.Replace(GPGSUtil.ANDROIDCLIENTIDPLACEHOLDER, clientId);
			GPGSUtil.WriteFile(outputPath, fileBody);
		}
    }
}
