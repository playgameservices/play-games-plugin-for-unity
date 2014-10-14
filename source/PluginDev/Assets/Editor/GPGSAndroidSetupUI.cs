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

public class GPGSAndroidSetupUI : EditorWindow {
    string mAppId = "";

    [MenuItem("Google Play Games/Android Setup...", false, 0)]
    public static void MenuItemGPGSAndroidSetup() {
        EditorWindow.GetWindow(typeof(GPGSAndroidSetupUI));
    }

    void OnEnable() {
        mAppId = GPGSProjectSettings.Instance.Get("proj.AppId");
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(20, 20, position.width - 40, position.height - 40));
        GUILayout.Label(GPGSStrings.AndroidSetup.Title, EditorStyles.boldLabel);
        GUILayout.Label(GPGSStrings.AndroidSetup.Blurb);
        GUILayout.Space(10);

        GUILayout.Label(GPGSStrings.Setup.AppId, EditorStyles.boldLabel);
        GUILayout.Label(GPGSStrings.Setup.AppIdBlurb);
        mAppId = EditorGUILayout.TextField(GPGSStrings.Setup.AppId, mAppId);

        GUILayout.Space(10);
        if (GUILayout.Button(GPGSStrings.Setup.SetupButton)) {
            DoSetup();
        }
        GUILayout.EndArea();
    }

    void DoSetup() {
        string sdkPath = GPGSUtil.GetAndroidSdkPath();
        string libProjPath = sdkPath +
            GPGSUtil.FixSlashes("/extras/google/google_play_services/libproject/google-play-services_lib");
        string libProjAM = libProjPath + GPGSUtil.FixSlashes("/AndroidManifest.xml");
        string libProjDestDir = GPGSUtil.FixSlashes("Assets/Plugins/Android/google-play-services_lib");
        string projAM = GPGSUtil.FixSlashes("Assets/Plugins/Android/MainLibProj/AndroidManifest.xml");

        GPGSProjectSettings.Instance.Set("proj.AppId", mAppId);
        GPGSProjectSettings.Instance.Save();

        // check for valid app id
        if (!GPGSUtil.LooksLikeValidAppId(mAppId)) {
            GPGSUtil.Alert(GPGSStrings.Setup.AppIdError);
            return;
        }

        // check that Android SDK is there
        if (!GPGSUtil.HasAndroidSdk()) {
            Debug.LogError("Android SDK not found.");
            EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.SdkNotFound,
                GPGSStrings.AndroidSetup.SdkNotFoundBlurb, GPGSStrings.Ok);
            return;
        }

        // check that the Google Play Services lib project is there
        if (!System.IO.Directory.Exists(libProjPath) || !System.IO.File.Exists(libProjAM)) {
            Debug.LogError("Google Play Services lib project not found at: " + libProjPath);
            EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.LibProjNotFound,
                GPGSStrings.AndroidSetup.LibProjNotFoundBlurb, GPGSStrings.Ok);
            return;
        }

        string supportJarPath = sdkPath + 
            GPGSUtil.FixSlashes(
                "/extras/android/support/v4/android-support-v4.jar");
        string supportJarDest = 
			GPGSUtil.FixSlashes("Assets/Plugins/Android/android-support-v4.jar");
        
        if (!System.IO.File.Exists(supportJarPath)) {
            Debug.LogError("Android support library v4 not found at: " + supportJarPath);
            EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.SupportJarNotFound,
                                        GPGSStrings.AndroidSetup.SupportJarNotFoundBlurb, GPGSStrings.Ok);
            return;
        }
        
                
                                
        // check lib project version
        if (!CheckAndWarnAboutGmsCoreVersion(libProjAM)) {
            return;
        }

        // create needed directories
        EnsureDirExists("Assets/Plugins");
        EnsureDirExists("Assets/Plugins/Android");

        // clear out the destination library project
        DeleteDirIfExists(libProjDestDir);
        
        // Clear out any stale version of the support jar.
        System.IO.File.Delete(supportJarDest);
        

        // Copy Google Play Services library
        FileUtil.CopyFileOrDirectory(libProjPath, libProjDestDir);

        // Copy Android Support Library
        FileUtil.CopyFileOrDirectory(supportJarPath, supportJarDest);

        // Generate AndroidManifest.xml
        string manifestBody = GPGSUtil.ReadTextFile("template-AndroidManifest");
        manifestBody = manifestBody.Replace("___APP_ID___", mAppId);
        GPGSUtil.WriteFile(projAM, manifestBody);

        // refresh assets, and we're done
        AssetDatabase.Refresh();
        GPGSProjectSettings.Instance.Set("android.SetupDone", true);
        GPGSProjectSettings.Instance.Save();
        EditorUtility.DisplayDialog(GPGSStrings.Success,
            GPGSStrings.AndroidSetup.SetupComplete, GPGSStrings.Ok);
    }
    
    private bool CheckAndWarnAboutGmsCoreVersion(string libProjAMFile) {
        string manifestContents = GPGSUtil.ReadFile(libProjAMFile);
        string[] fields = manifestContents.Split('\"');
        int i;
        long vercode = 0;
        for (i = 0; i < fields.Length; i++) {
            if (fields[i].Contains("android:versionCode") && i + 1 < fields.Length) {
                vercode = System.Convert.ToInt64(fields[i + 1]);
            }
        }
        if (vercode == 0) {
            return EditorUtility.DisplayDialog(GPGSStrings.Warning, string.Format(
                GPGSStrings.AndroidSetup.LibProjVerNotFound,
                GooglePlayGames.PluginVersion.MinGmsCoreVersionCode),
                GPGSStrings.Ok, GPGSStrings.Cancel);
        } else if (vercode < GooglePlayGames.PluginVersion.MinGmsCoreVersionCode) {
            return EditorUtility.DisplayDialog(GPGSStrings.Warning, string.Format(
                GPGSStrings.AndroidSetup.LibProjVerTooOld, vercode,
                GooglePlayGames.PluginVersion.MinGmsCoreVersionCode),
                GPGSStrings.Ok, GPGSStrings.Cancel);
        }
        return true;
    }

    private void EnsureDirExists(string dir) {
        dir = dir.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
        if (!System.IO.Directory.Exists(dir)) {
            System.IO.Directory.CreateDirectory(dir);
        }
    }

    private void DeleteDirIfExists(string dir) {
        if (System.IO.Directory.Exists(dir)) {
            System.IO.Directory.Delete(dir, true);
        }
    }
}
