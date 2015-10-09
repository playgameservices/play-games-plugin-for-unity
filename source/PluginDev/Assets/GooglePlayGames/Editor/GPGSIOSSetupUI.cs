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
    using System;
    using System.IO;
    using System.Collections;
    using UnityEngine;
    using UnityEditor;

    public class GPGSIOSSetupUI : EditorWindow
    {
        private string mBundleId = string.Empty;
        private string mWebClientId = string.Empty;
        private string mClassName = "GooglePlayGames.GPGSIds";
        private string mConfigData = string.Empty;
        private Vector2 scroll;

        [MenuItem("Window/Google Play Games/Setup/iOS setup...", false, 2)]
        public static void MenuItemGPGSIOSSetup()
        {
            EditorWindow window = EditorWindow.GetWindow(
                    typeof(GPGSIOSSetupUI), true, GPGSStrings.IOSSetup.Title);
            window.minSize = new Vector2(500, 600);
        }

        public void OnEnable()
        {
            mBundleId = GPGSProjectSettings.Instance.Get(GPGSUtil.IOSBUNDLEIDKEY);

            mWebClientId = GPGSProjectSettings.Instance.Get(GPGSUtil.WEBCLIENTIDKEY);
            mClassName = GPGSProjectSettings.Instance.Get(GPGSUtil.CLASSNAMEKEY);
            mConfigData = GPGSProjectSettings.Instance.Get(GPGSUtil.IOSRESOURCEKEY);

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
        internal static void Save(string clientId, string bundleId, string webClientId)
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

            // Bundle ID field
            GUILayout.Label(GPGSStrings.IOSSetup.BundleIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.IOSSetup.BundleIdBlurb, EditorStyles.wordWrappedLabel);
            mBundleId = EditorGUILayout.TextField(GPGSStrings.IOSSetup.BundleId, mBundleId,
            GUILayout.Width(450));
            
            GUILayout.Space(30);

            // Client ID field
            GUILayout.Label(GPGSStrings.Setup.WebClientIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.AndroidSetup.WebClientIdBlurb, EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            mWebClientId = EditorGUILayout.TextField(GPGSStrings.Setup.ClientId,
            mWebClientId, GUILayout.Width(450));

            GUILayout.Space(10);
            GUILayout.FlexibleSpace();

            GUILayout.Label("Constants class name", EditorStyles.boldLabel);
            GUILayout.Label("Enter the fully qualified name of the class to create containing the constants");
            GUILayout.Space(10);

            mClassName = EditorGUILayout.TextField("Constants class name",
                mClassName, GUILayout.Width(480));

            GUILayout.Label("Resources Definition", EditorStyles.boldLabel);
            GUILayout.Label("Paste in the Objective-C Resources from the Play Console");
            GUILayout.Space(10);
            scroll = GUILayout.BeginScrollView(scroll);
            mConfigData = EditorGUILayout.TextArea(mConfigData,
                GUILayout.Width(475), GUILayout.Height(Screen.height));
            GUILayout.EndScrollView();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Setup button
            if (GUILayout.Button(GPGSStrings.Setup.SetupButton))
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
        internal void DoSetup()
        {
            if (PerformSetup(mClassName, mConfigData, mWebClientId, mBundleId, null))
            {
                GPGSUtil.Alert(GPGSStrings.Success, GPGSStrings.IOSSetup.SetupComplete);
                Close();
            }
            else
            {
                GPGSUtil.Alert(GPGSStrings.Error,
                    "Missing or invalid resource data.  Check that CLIENT_ID is defined.");
            }
        }

        /// <summary>
        /// Performs setup using the Android resources downloaded XML file
        /// from the play console.
        /// </summary>
        /// <returns><c>true</c>, if setup was performed, <c>false</c> otherwise.</returns>
        /// <param name="className">Fully qualified class name for the resource Ids.</param>
        /// <param name="resourceXmlData">Resource xml data.</param>
        /// <param name="webClientId">Client identifier.</param>
        /// <param name="bundleId">Bundle identifier.</param>
        /// <param name="nearbySvcId">Nearby svc identifier.</param>
        public static bool PerformSetup(
            string className, string resourceXmlData, 
            string webClientId, string bundleId,  string nearbySvcId)
        {
            if (ParseResources(className, resourceXmlData))
            {
                GPGSProjectSettings.Instance.Set(GPGSUtil.CLASSNAMEKEY, className);
                GPGSProjectSettings.Instance.Set(GPGSUtil.IOSRESOURCEKEY, resourceXmlData);
                GPGSProjectSettings.Instance.Set(GPGSUtil.WEBCLIENTIDKEY, webClientId);
                return PerformSetup(GPGSProjectSettings.Instance.Get(GPGSUtil.IOSCLIENTIDKEY),
                    bundleId, webClientId, nearbySvcId);
            }

            return false;
        }

        private static bool ParseResources(string className, string res)
        {
           // parse the resources, they keys are in the form of
            // #define <KEY> @"<VALUE>"

            // transform the string to make it easier to parse
            string input = res.Replace("#define ", string.Empty);
            input = input.Replace("@\"", string.Empty);
            input = input.Replace("\"", string.Empty);

            // now input is name value, one per line
            StringReader reader = new StringReader(input);
            string line = reader.ReadLine();
            string key;
            string value;
            string clientId = null;
            Hashtable resourceKeys = new Hashtable();
            while (line != null)
            {
                string[] parts = line.Split(' ');
                key = parts[0];
                if (parts.Length > 1)
                {
                    value = parts[1];
                }
                else
                {
                    value = null;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    if (key == "CLIENT_ID")
                    {
                        clientId = value;
                        GPGSProjectSettings.Instance.Set(GPGSUtil.IOSCLIENTIDKEY, clientId);
                    }
                    else if (key.StartsWith("ACH_"))
                    {
                        string prop = "achievement_" + key.Substring(4).ToLower();
                        resourceKeys[prop] = value;
                    }
                    else if (key.StartsWith("LEAD_"))
                    {
                        string prop = "leaderboard_" + key.Substring(5).ToLower();
                        resourceKeys[prop] = value;
                    }
                    else if (key.StartsWith("EVENT_"))
                    {
                        string prop = "event_" + key.Substring(6).ToLower();
                        resourceKeys[prop] = value;
                    }
                    else if (key.StartsWith("QUEST_"))
                    {
                        string prop = "quest_" + key.Substring(6).ToLower();
                        resourceKeys[prop] = value;
                    }
                    else
                    {
                        resourceKeys[key] = value;
                    }
                }

                line = reader.ReadLine();
            }

            reader.Close();
            if (resourceKeys.Count > 0)
            {
                GPGSUtil.WriteResourceIds(className, resourceKeys);
            }

            return !string.IsNullOrEmpty(clientId);
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
