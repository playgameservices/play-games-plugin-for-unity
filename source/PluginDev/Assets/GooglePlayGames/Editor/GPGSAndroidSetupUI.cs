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
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using UnityEditor;
    using UnityEngine;

    public class GPGSAndroidSetupUI : EditorWindow
    {
        private string mConfigData = string.Empty;
        private string mClassName = "GooglePlayGames.GPGSIds";
        private Vector2 scroll;

        private string mWebClientId = string.Empty;

        [MenuItem("Window/Google Play Games/Setup/Android setup...", false, 1)]
        public static void MenuItemFileGPGSAndroidSetup()
        {
            EditorWindow window = EditorWindow.GetWindow(
                    typeof(GPGSAndroidSetupUI), true, GPGSStrings.AndroidSetup.Title);
            window.minSize = new Vector2(500, 400);
        }

        public void OnEnable()
        {
            mClassName = GPGSProjectSettings.Instance.Get(GPGSUtil.CLASSNAMEKEY);
            mConfigData = GPGSProjectSettings.Instance.Get(GPGSUtil.ANDROIDRESOURCEKEY);
            mWebClientId = GPGSProjectSettings.Instance.Get(GPGSUtil.WEBCLIENTIDKEY);
        }

        public void OnGUI()
        {
            GUI.skin.label.wordWrap = true;
            GUILayout.BeginVertical();

            GUILayout.Space(10);
            GUILayout.Label(GPGSStrings.AndroidSetup.Blurb);
            GUILayout.Label("Constants class name", EditorStyles.boldLabel);
            GUILayout.Label("Enter the fully qualified name of the class to create containing the constants");
            GUILayout.Space(10);

            mClassName = EditorGUILayout.TextField("Constants class name",
                    mClassName, GUILayout.Width(480));

            GUILayout.Label("Resources Definition", EditorStyles.boldLabel);
            GUILayout.Label("Paste in the Android Resources from the Play Console");
            GUILayout.Space(10);
            scroll = GUILayout.BeginScrollView(scroll);
            mConfigData = EditorGUILayout.TextArea(mConfigData,
                GUILayout.Width(475), GUILayout.Height(Screen.height));
            GUILayout.EndScrollView();
            GUILayout.Space(10);

            // Client ID field
            GUILayout.Label(GPGSStrings.Setup.WebClientIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.AndroidSetup.WebClientIdBlurb);

            mWebClientId = EditorGUILayout.TextField(GPGSStrings.Setup.ClientId,
                mWebClientId, GUILayout.Width(450));

            GUILayout.Space(10);

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GPGSStrings.Setup.SetupButton, GUILayout.Width(100)))
            {
                // check that the classname entered is valid
                try
                {
                    if (GPGSUtil.LooksLikeValidPackageName(mClassName))
                    {
                        DoSetup();
                    }
                }
                catch (Exception e)
                {
                    GPGSUtil.Alert(GPGSStrings.Error,
                        "Invalid classname: " + e.Message);
                }

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

        public void DoSetup()
        {
            if (PerformSetup(mWebClientId, mClassName, mConfigData, null))
            {
                EditorUtility.DisplayDialog(GPGSStrings.Success,
                    GPGSStrings.AndroidSetup.SetupComplete, GPGSStrings.Ok);
                this.Close();
            }
            else
            {
                GPGSUtil.Alert(GPGSStrings.Error,
                    "Invalid or missing XML resource data.  Make sure the data is" +
                    " valid and contains the app_id element");
            }
        }

        private static bool ParseResources(string className, string res)
        {
            XmlTextReader reader = new XmlTextReader(new StringReader(res));
            bool inResource = false;
            string lastProp = null;
            Hashtable resourceKeys = new Hashtable();
            string appId = null;
            while (reader.Read())
            {
                if (reader.Name == "resources")
                {
                    inResource = true;
                }
                if (inResource && reader.Name == "string")
                {
                    lastProp = reader.GetAttribute("name");
                }
                else if (inResource && !string.IsNullOrEmpty(lastProp))
                {
                    if (reader.HasValue)
                    {
                        if (lastProp == "app_id")
                        {
                            appId = reader.Value;
                            GPGSProjectSettings.Instance.Set(GPGSUtil.APPIDKEY, appId);
                        }
                        else
                        {
                            resourceKeys[lastProp] = reader.Value;
                        }
                        lastProp = null;
                    }
                }
            }
            reader.Close();
            if (resourceKeys.Count > 0)
            {
                GPGSUtil.WriteResourceIds(className, resourceKeys);
            }
            return appId != null;
        }


        /// <summary>
        /// Performs setup using the Android resources downloaded XML file
        /// from the play console.
        /// </summary>
        /// <returns><c>true</c>, if setup was performed, <c>false</c> otherwise.</returns>
        /// <param name="className">Fully qualified class name for the resource Ids.</param>
        /// <param name="resourceXmlData">Resource xml data.</param>
        /// <param name="nearbySvcId">Nearby svc identifier.</param>
        public static bool PerformSetup(string clientId, string className, string resourceXmlData, string nearbySvcId)
        {
            if (string.IsNullOrEmpty(resourceXmlData) &&
                !string.IsNullOrEmpty(nearbySvcId))
            {
                return PerformSetup(clientId,
                    GPGSProjectSettings.Instance.Get(GPGSUtil.APPIDKEY), nearbySvcId);
            }
            if (ParseResources(className, resourceXmlData))
            {
                GPGSProjectSettings.Instance.Set(GPGSUtil.CLASSNAMEKEY, className);
                GPGSProjectSettings.Instance.Set(GPGSUtil.ANDROIDRESOURCEKEY, resourceXmlData);
                return PerformSetup(clientId,
                    GPGSProjectSettings.Instance.Get(GPGSUtil.APPIDKEY), nearbySvcId);
            }
            return false;
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

            if (!string.IsNullOrEmpty(webClientId))
            {
                if (!GPGSUtil.LooksLikeValidClientId(webClientId))
                {
                    GPGSUtil.Alert(GPGSStrings.Setup.ClientIdError);
                    return false;
                }

                string serverAppId = webClientId.Split('-')[0];
                if (!serverAppId.Equals(appId))
                {
                    GPGSUtil.Alert(GPGSStrings.Setup.AppIdMismatch);
                    return false;
                }
                needTokenPermissions = true;
            }
            else
            {
                needTokenPermissions = false;
            }

            // check for valid app id
            if (!GPGSUtil.LooksLikeValidAppId(appId) && string.IsNullOrEmpty(nearbySvcId))
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
