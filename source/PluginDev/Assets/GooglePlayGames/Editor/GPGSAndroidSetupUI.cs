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


namespace GooglePlayGames.Editor
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Google Play Game Services Setup dialog for Android.
    /// </summary>
    public class GPGSAndroidSetupUI : EditorWindow
    {
        /// <summary>
        /// The configuration data from the play games console "resource data"
        /// </summary>
        private string mConfigData = string.Empty;

        /// <summary>
        /// The name of the class to generate containing the resource constants.
        /// </summary>
        private string mClassName = "GPGSIds";

        /// <summary>
        /// The scroll position
        /// </summary>
        private Vector2 scroll;

        /// <summary>
        /// The directory for the constants class.
        /// </summary>
        private string mConstantDirectory = "Assets";

        /// <summary>
        /// The web client identifier.
        /// </summary>
        private string mWebClientId = string.Empty;

        /// <summary>
        /// Menus the item for GPGS android setup.
        /// </summary>
        [MenuItem("Window/Google Play Games/Setup/Android setup...", false, 1)]
        public static void MenuItemFileGPGSAndroidSetup()
        {
            EditorWindow window = EditorWindow.GetWindow(
                                      typeof(GPGSAndroidSetupUI), true, GPGSStrings.AndroidSetup.Title);
            window.minSize = new Vector2(500, 400);
        }

        [MenuItem("Window/Google Play Games/Setup/Android setup...", true)]
        public static bool EnableAndroidMenuItem() {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Performs setup using the Android resources downloaded XML file
        /// from the play console.
        /// </summary>
        /// <returns><c>true</c>, if setup was performed, <c>false</c> otherwise.</returns>
        /// <param name="clientId">The web client id.</param>
        /// <param name="classDirectory">the directory to write the constants file to.</param>
        /// <param name="className">Fully qualified class name for the resource Ids.</param>
        /// <param name="resourceXmlData">Resource xml data.</param>
        /// <param name="nearbySvcId">Nearby svc identifier.</param>
        /// <param name="requiresGooglePlus">Indicates this app requires G+</param>
        public static bool PerformSetup(
            string clientId,
            string classDirectory,
            string className,
            string resourceXmlData,
            string nearbySvcId)
        {
            if (string.IsNullOrEmpty(resourceXmlData) &&
                !string.IsNullOrEmpty(nearbySvcId))
            {
                return PerformSetup(
                    clientId,
                    GPGSProjectSettings.Instance.Get(GPGSUtil.APPIDKEY),
                    nearbySvcId);
            }

            if (ParseResources(classDirectory, className, resourceXmlData))
            {
                GPGSProjectSettings.Instance.Set(GPGSUtil.CLASSDIRECTORYKEY, classDirectory);
                GPGSProjectSettings.Instance.Set(GPGSUtil.CLASSNAMEKEY, className);
                GPGSProjectSettings.Instance.Set(GPGSUtil.ANDROIDRESOURCEKEY, resourceXmlData);

                // check the bundle id and set it if needed.
                CheckBundleId();

                GPGSUtil.CheckAndFixDependencies();
                GPGSUtil.CheckAndFixVersionedAssestsPaths();
                AssetDatabase.Refresh();

                Google.VersionHandler.VerboseLoggingEnabled = true;
                Google.VersionHandler.UpdateVersionedAssets(forceUpdate: true);
                Google.VersionHandler.Enabled = true;
                AssetDatabase.Refresh();

                Google.VersionHandler.InvokeStaticMethod(
                    Google.VersionHandler.FindClass(
                   "Google.JarResolver",
                   "GooglePlayServices.PlayServicesResolver"),
                   "MenuResolve", null);

                return PerformSetup(
                    clientId,
                    GPGSProjectSettings.Instance.Get(GPGSUtil.APPIDKEY),
                    nearbySvcId);
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
        /// <param name="requiresGooglePlus">Indicates that GooglePlus should be enabled</param>
        /// <returns>true if successful</returns>
        public static bool PerformSetup(string webClientId, string appId, string nearbySvcId)
        {
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
            }

            // check for valid app id
            if (!GPGSUtil.LooksLikeValidAppId(appId) && string.IsNullOrEmpty(nearbySvcId))
            {
                GPGSUtil.Alert(GPGSStrings.Setup.AppIdError);
                return false;
            }

            if (nearbySvcId != null) {
#if UNITY_ANDROID
                if (!NearbyConnectionUI.PerformSetup(nearbySvcId, true))
                {
                    return false;
                }
#endif
            }

            GPGSProjectSettings.Instance.Set(GPGSUtil.APPIDKEY, appId);
            GPGSProjectSettings.Instance.Set(GPGSUtil.WEBCLIENTIDKEY, webClientId);
            GPGSProjectSettings.Instance.Save();
            GPGSUtil.UpdateGameInfo();

            // check that Android SDK is there
            if (!GPGSUtil.HasAndroidSdk())
            {
                Debug.LogError("Android SDK not found.");
                EditorUtility.DisplayDialog(
                    GPGSStrings.AndroidSetup.SdkNotFound,
                    GPGSStrings.AndroidSetup.SdkNotFoundBlurb,
                    GPGSStrings.Ok);
                return false;
            }

            // Generate AndroidManifest.xml
            GPGSUtil.GenerateAndroidManifest();

            // refresh assets, and we're done
            AssetDatabase.Refresh();
            GPGSProjectSettings.Instance.Set(GPGSUtil.ANDROIDSETUPDONEKEY, true);
            GPGSProjectSettings.Instance.Save();

            return true;
        }

        /// <summary>
        /// Called when this object is enabled by Unity editor.
        /// </summary>
        public void OnEnable()
        {
            GPGSProjectSettings settings = GPGSProjectSettings.Instance;
            mConstantDirectory = settings.Get(GPGSUtil.CLASSDIRECTORYKEY, mConstantDirectory);
            mClassName = settings.Get(GPGSUtil.CLASSNAMEKEY, mClassName);
            mConfigData = settings.Get(GPGSUtil.ANDROIDRESOURCEKEY);
            mWebClientId = settings.Get(GPGSUtil.WEBCLIENTIDKEY);
        }

        /// <summary>
        /// Called when the GUI should be rendered.
        /// </summary>
        public void OnGUI()
        {
            GUI.skin.label.wordWrap = true;
            GUILayout.BeginVertical();

            GUIStyle link = new GUIStyle(GUI.skin.label);
            link.normal.textColor = new Color(0f, 0f, 1f);

            GUILayout.Space(10);
            GUILayout.Label(GPGSStrings.AndroidSetup.Blurb);
            if (GUILayout.Button("Open Play Games Console", link, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL("https://play.google.com/apps/publish");
            }

            Rect last = GUILayoutUtility.GetLastRect();
            last.y += last.height - 2;
            last.x += 3;
            last.width -= 6;
            last.height = 2;

            GUI.Box(last, string.Empty);

            GUILayout.Space(15);
            GUILayout.Label("Constants class name", EditorStyles.boldLabel);
            GUILayout.Label("Enter the fully qualified name of the class to create containing the constants");
            GUILayout.Space(10);

            mConstantDirectory = EditorGUILayout.TextField(
                "Directory to save constants",
                mConstantDirectory,
                GUILayout.MinWidth(480));

            mClassName = EditorGUILayout.TextField(
                "Constants class name",
                mClassName,
                GUILayout.MinWidth(480));

            GUILayout.Label("Resources Definition", EditorStyles.boldLabel);
            GUILayout.Label("Paste in the Android Resources from the Play Console");
            GUILayout.Space(10);

            scroll = GUILayout.BeginScrollView(scroll);
            mConfigData = EditorGUILayout.TextArea(
                mConfigData,
                GUILayout.MinWidth(475),
                GUILayout.Height(Screen.height));
            GUILayout.EndScrollView();
            GUILayout.Space(10);

            // Client ID field
            GUILayout.Label(GPGSStrings.Setup.WebClientIdTitle, EditorStyles.boldLabel);
            GUILayout.Label(GPGSStrings.AndroidSetup.WebClientIdBlurb);

            mWebClientId = EditorGUILayout.TextField(
                GPGSStrings.Setup.ClientId,
                mWebClientId,
                GUILayout.MinWidth(450));

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
                        return;
                    }
                }
                catch (Exception e)
                {
                    GPGSUtil.Alert(
                        GPGSStrings.Error,
                        "Invalid classname: " + e.Message);
                }
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Close();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Starts the setup process.
        /// </summary>
        public void DoSetup()
        {
            if (PerformSetup(mWebClientId, mConstantDirectory, mClassName, mConfigData, null))
            {
                CheckBundleId();

                EditorUtility.DisplayDialog(
                    GPGSStrings.Success,
                    GPGSStrings.AndroidSetup.SetupComplete,
                    GPGSStrings.Ok);

                GPGSProjectSettings.Instance.Set(GPGSUtil.ANDROIDSETUPDONEKEY, true);
                Close();
            }
            else
            {
                GPGSUtil.Alert(
                    GPGSStrings.Error,
                    "Invalid or missing XML resource data.  Make sure the data is" +
                    " valid and contains the app_id element");
            }
        }

        /// <summary>
        /// Checks the bundle identifier.
        /// </summary>
        /// <remarks>
        /// Check the package id.  If one is set the gpgs properties,
        /// and the player settings are the default or empty, set it.
        /// if the player settings is not the default, then prompt before
        /// overwriting.
        /// </remarks>
        public static void CheckBundleId()
        {
            string packageName = GPGSProjectSettings.Instance.Get(
                GPGSUtil.ANDROIDBUNDLEIDKEY, string.Empty);
            string currentId;
#if UNITY_5_6_OR_NEWER
            currentId = PlayerSettings.GetApplicationIdentifier(
                BuildTargetGroup.Android);
#else
            currentId = PlayerSettings.bundleIdentifier;
#endif
            if (!string.IsNullOrEmpty(packageName))
            {
                if (string.IsNullOrEmpty(currentId) ||
                    currentId == "com.Company.ProductName")
                {
#if UNITY_5_6_OR_NEWER
                    PlayerSettings.SetApplicationIdentifier(
                        BuildTargetGroup.Android, packageName);
#else
                    PlayerSettings.bundleIdentifier = packageName;
#endif
                }
                else if (currentId != packageName)
                {
                    if (EditorUtility.DisplayDialog(
                        "Set Bundle Identifier?",
                        "The server configuration is using " +
                        packageName + ", but the player settings is set to " +
                        currentId + ".\nSet the Bundle Identifier to " +
                        packageName + "?",
                        "OK",
                        "Cancel"))
                    {
#if UNITY_5_6_OR_NEWER
                        PlayerSettings.SetApplicationIdentifier(
                            BuildTargetGroup.Android, packageName);
#else
                        PlayerSettings.bundleIdentifier = packageName;
#endif
                    }
                }
            }
            else
            {
                Debug.Log("NULL package!!");
            }
        }

        /// <summary>
        /// Parses the resources xml and set the properties.  Also generates the
        /// constants file.
        /// </summary>
        /// <returns><c>true</c>, if resources was parsed, <c>false</c> otherwise.</returns>
        /// <param name="classDirectory">Class directory.</param>
        /// <param name="className">Class name.</param>
        /// <param name="res">Res. the data to parse.</param>
        private static bool ParseResources(string classDirectory, string className, string res)
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
                        else if (lastProp == "package_name")
                        {
                            GPGSProjectSettings.Instance.Set(GPGSUtil.ANDROIDBUNDLEIDKEY, reader.Value);
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
                GPGSUtil.WriteResourceIds(classDirectory, className, resourceKeys);
            }

            return appId != null;
        }
    }
}
