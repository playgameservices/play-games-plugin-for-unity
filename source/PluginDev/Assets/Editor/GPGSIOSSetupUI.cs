/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class GPGSIOSSetupUI : EditorWindow {
    private string mAppId = "";
    private string mClientId = "";
    private string mBundleId = "";

    [MenuItem("Google Play Games/iOS Setup...", false, 1)]
    public static void MenuItemGPGSIOSSetup() {
        EditorWindow.GetWindow(typeof(GPGSIOSSetupUI));
    }

    void OnEnable() {
        mAppId = GPGSProjectSettings.Instance.Get("proj.AppId");
        mClientId = GPGSProjectSettings.Instance.Get("ios.ClientId");
        mBundleId = GPGSProjectSettings.Instance.Get("ios.BundleId");

        if (mBundleId.Trim().Length == 0) {
            mBundleId = PlayerSettings.bundleIdentifier;
        }
    }

    void Save() {
        GPGSProjectSettings.Instance.Set("proj.AppId", mAppId);
        GPGSProjectSettings.Instance.Set("ios.ClientId", mClientId);
        GPGSProjectSettings.Instance.Set("ios.BundleId", mBundleId);
        GPGSProjectSettings.Instance.Save();
    }

    void OnGUI() {
        // Title
        GUILayout.BeginArea(new Rect(20, 20, position.width - 40, position.height - 40));
        GUILayout.Label(GPGSStrings.IOSSetup.Title, EditorStyles.boldLabel);
        GUILayout.Label(GPGSStrings.IOSSetup.Blurb);
        GUILayout.Space(10);

        // App ID field
        GUILayout.Label(GPGSStrings.Setup.AppIdTitle, EditorStyles.boldLabel);
        GUILayout.Label(GPGSStrings.Setup.AppIdBlurb);
        mAppId = EditorGUILayout.TextField(GPGSStrings.Setup.AppId, mAppId);
        GUILayout.Space(10);

        // Client ID field
        GUILayout.Label(GPGSStrings.IOSSetup.ClientIdTitle, EditorStyles.boldLabel);
        GUILayout.Label(GPGSStrings.IOSSetup.ClientIdBlurb);
        mClientId = EditorGUILayout.TextField(GPGSStrings.IOSSetup.ClientId, mClientId);
        GUILayout.Space(10);

        // Bundle ID field
        GUILayout.Label(GPGSStrings.IOSSetup.BundleIdTitle, EditorStyles.boldLabel);
        GUILayout.Label(GPGSStrings.IOSSetup.BundleIdBlurb);
        mBundleId = EditorGUILayout.TextField(GPGSStrings.IOSSetup.BundleId, mBundleId);
        GUILayout.Space(10);

        // Setup button
        if (GUILayout.Button(GPGSStrings.Setup.SetupButton)) {
            DoSetup();
        }
        GUILayout.EndArea();
    }

    void DoSetup() {
        Save();
        if (!GPGSUtil.LooksLikeValidAppId(mAppId)) {
            GPGSUtil.Alert(GPGSStrings.Setup.AppIdError);
            return;
        }
        if (!GPGSUtil.LooksLikeValidClientId(mClientId)) {
            GPGSUtil.Alert(GPGSStrings.IOSSetup.ClientIdError);
            return;
        }
        if (!GPGSUtil.LooksLikeValidBundleId(mBundleId)) {
            GPGSUtil.Alert(GPGSStrings.IOSSetup.BundleIdError);
            return;
        }

        // Write GPGSParams.h with the app's parameters
        string paramsFileBody = GPGSUtil.ReadTextFile("template-GPGSParams");
        paramsFileBody = paramsFileBody.Replace("__APPID__", mAppId);
        paramsFileBody = paramsFileBody.Replace("__CLIENTID__", mClientId);
        paramsFileBody = paramsFileBody.Replace("__BUNDLEID__", mBundleId);
        GPGSUtil.WriteFile("Assets/Plugins/iOS/GPGSParams.h", paramsFileBody);

        // Finished!
        GPGSProjectSettings.Instance.Set("ios.SetupDone", true);
        GPGSProjectSettings.Instance.Save();
        AssetDatabase.Refresh();
        GPGSUtil.Alert(GPGSStrings.Success, GPGSStrings.IOSSetup.SetupComplete);
    }
}
