// <copyright file="GPGSIOSSetupUI.cs" company="Google Inc.">
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

  public class GPGSIOSSetupUI : EditorWindow
  {
    private const string GameInfoPath = "Assets/GooglePlayGames/GameInfo.cs";

    private string mClientId = string.Empty;
    private string mBundleId = string.Empty;

    [MenuItem("Window/Google Play Games/Setup/iOS setup...", false, 2)]
    public static void MenuItemGPGSIOSSetup()
    {
      EditorWindow.GetWindow(typeof(GPGSIOSSetupUI));
    }

    public void OnEnable()
    {
      mClientId = GPGSProjectSettings.Instance.Get("ios.ClientId");
      mBundleId = GPGSProjectSettings.Instance.Get("ios.BundleId");

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
    static void Save(string clientId, string bundleId)
    {
      GPGSProjectSettings.Instance.Set("ios.ClientId", clientId);
      GPGSProjectSettings.Instance.Set("ios.BundleId", bundleId);
      GPGSProjectSettings.Instance.Save();
    }

    public void OnGUI()
    {
      // Title
      GUILayout.BeginArea(new Rect(20, 20, position.width - 40, position.height - 40));
      GUILayout.Label(GPGSStrings.IOSSetup.Title, EditorStyles.boldLabel);
      GUILayout.Label(GPGSStrings.IOSSetup.Blurb);
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
      if (GUILayout.Button(GPGSStrings.Setup.SetupButton))
        {
          DoSetup();
        }

      GUILayout.EndArea();
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
                                      string clientId, string bundleId)
    {
      string fileBody = GPGSUtil.ReadFully(sourcePath);
      fileBody = fileBody.Replace("__CLIENTID__", clientId);
      fileBody = fileBody.Replace("__BUNDLEID__", bundleId);
      GPGSUtil.WriteFile(outputPath, fileBody);
    }

    /// <summary>
    /// Called by the UI to process the configuration.
    /// </summary>
    void DoSetup()
    {
      PerformSetup(mClientId, mBundleId);
    }

    /// <summary>
    /// Performs the setup.  This is called externally to facilitate
    /// build automation.
    /// </summary>
    /// <param name="clientId">Client identifier.</param>
    /// <param name="bundleId">Bundle identifier.</param>
    public static void PerformSetup(string clientId, string bundleId)
    {

      if (!GPGSUtil.LooksLikeValidClientId(clientId))
        {
          GPGSUtil.Alert(GPGSStrings.IOSSetup.ClientIdError);
          return;
        }
      if (!GPGSUtil.LooksLikeValidBundleId(bundleId))
        {
          GPGSUtil.Alert(GPGSStrings.IOSSetup.BundleIdError);
          return;
        }

      Save(clientId, bundleId);
      GPGSUtil.UpdateGameInfo();

      FillInAppData(GameInfoPath, GameInfoPath, clientId, bundleId);

      // Finished!
      GPGSProjectSettings.Instance.Set("ios.SetupDone", true);
      GPGSProjectSettings.Instance.Save();
      AssetDatabase.Refresh();
      GPGSUtil.Alert(GPGSStrings.Success, GPGSStrings.IOSSetup.SetupComplete);
    }
  }
}
