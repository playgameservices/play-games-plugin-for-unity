// <copyright file="VideoGUI.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All Rights Reserved.
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

namespace SmokeTest
{
  using System;
  using System.Collections.Generic;
  using GooglePlayGames;
  using GooglePlayGames.BasicApi;
  using GooglePlayGames.BasicApi.Video;
  using UnityEngine;
  using UnityEngine.SocialPlatforms;

  public class VideoGUI : MonoBehaviour, CaptureOverlayStateListener
  {

    private MainGui mOwner;
    private string mStatus;

    // Constructed by the main gui
    internal VideoGUI(MainGui owner)
    {
      mOwner = owner;
      mStatus = "";
    }

    internal void OnGUI()
    {
      float height = Screen.height / 11f;
      GUILayout.BeginVertical(GUILayout.Height(Screen.height), GUILayout.Width(Screen.width));
      GUILayout.Label("SmokeTest: Video", GUILayout.Height(height));
      GUILayout.BeginHorizontal(GUILayout.Height(height));
      if (GUILayout.Button("Get Capture Capabilities", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
      {
        DoCaptureCapabilities();
      }
      if (GUILayout.Button("Show Capture Overlay", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
      {
        DoCaptureOverlay();
      }
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal(GUILayout.Height(height));
      if (GUILayout.Button("Get Capture State", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
      {
        DoCaptureState();
      }
      if (GUILayout.Button("Is File Capture Available?", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
      {
        DoCaptureAvailable();
      }
      GUILayout.EndHorizontal();
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Is Capture Supported?", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
      {
        DoCaptureSupported();
      }
      if (GUILayout.Button("Setup Listener", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
      {
        DoRegisterListener();
      }
      GUILayout.EndHorizontal();
      GUILayout.Space(20);
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Back", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
      {
        mOwner.SetUI(MainGui.Ui.Main);
      }
      GUILayout.EndHorizontal();
      GUILayout.FlexibleSpace();
      GUILayout.Label(mStatus);
      GUILayout.EndVertical();
    }

    internal void DoCaptureCapabilities()
    {
      PlayGamesPlatform.Instance.Video.GetCaptureCapabilities(
        (status, capabilities) =>
        {
          bool isSuccess = CommonTypesUtil.StatusIsSuccess(status);
          ShowEffect(isSuccess);
          if (isSuccess)
          {
            mStatus = "Capture Capabilities:\n";
            mStatus += "Camera: " + capabilities.IsCameraSupported;
            mStatus += ", Mic: " + capabilities.IsMicSupported;
            mStatus += ", Write Storage: " + capabilities.IsWriteStorageSupported + "\n";
            mStatus += "Capture Modes - File: " + capabilities.SupportsCaptureMode(VideoCaptureMode.File);
            mStatus += ", Stream: " + capabilities.SupportsCaptureMode(VideoCaptureMode.Stream) + "\n";
            mStatus += "Quality Levels - SD: " + capabilities.SupportsQualityLevel(VideoQualityLevel.SD);
            mStatus += ", HD: " + capabilities.SupportsQualityLevel(VideoQualityLevel.HD);
            mStatus += ", XHD: " + capabilities.SupportsQualityLevel(VideoQualityLevel.XHD);
            mStatus += ", FullHD: " + capabilities.SupportsQualityLevel(VideoQualityLevel.FullHD);
          }
          else {
            mStatus = "Error: " + status.ToString();
          }
        });
    }

    internal void DoCaptureOverlay()
    {
      PlayGamesPlatform.Instance.Video.ShowCaptureOverlay();
    }

    internal void DoCaptureState()
    {
      PlayGamesPlatform.Instance.Video.GetCaptureState(
        (status, state) =>
        {
          bool isSuccess = CommonTypesUtil.StatusIsSuccess(status);
          ShowEffect(isSuccess);
          if (isSuccess)
          {
            mStatus = "Capture State:\n";
            mStatus += "Capturing? " + state.IsCapturing + "\n";
            mStatus += "Mode: " + state.CaptureMode.ToString();
            mStatus += ", Quality: " + state.QualityLevel.ToString() + "\n";
            mStatus += "Overlay Visible? " + state.IsOverlayVisible + "\n";
            mStatus += "Paused? " + state.IsPaused + "\n";
          }
          else {
            mStatus = "Error: " + status.ToString();
          }
        });
    }

    internal void DoCaptureAvailable()
    {
      PlayGamesPlatform.Instance.Video.IsCaptureAvailable(VideoCaptureMode.File,
        (status, isAvailable) =>
        {
          bool isSuccess = CommonTypesUtil.StatusIsSuccess(status);
          ShowEffect(isSuccess);
          if (isSuccess)
          {
            mStatus = "Is video capture to file available? " + isAvailable;
          }
          else {
            mStatus = "Error: " + status.ToString();
          }
        });
    }

    internal void DoCaptureSupported()
    {
      bool isSupported = PlayGamesPlatform.Instance.Video.IsCaptureSupported();
      ShowEffect(isSupported);
      mStatus = "Is Capture Supported? " + isSupported;
    }

    internal void DoRegisterListener()
    {
      PlayGamesPlatform.Instance.Video.RegisterCaptureOverlayStateChangedListener(this);
    }

    public void OnCaptureOverlayStateChanged(VideoCaptureOverlayState overlayState)
    {
      mStatus = "Overlay State is now " + overlayState.ToString();
    }

    internal void ShowEffect(bool success)
    {
      Camera.main.backgroundColor = success ?
        new Color(0.0f, 0.0f, 0.8f, 1.0f) :
        new Color(0.8f, 0.0f, 0.0f, 1.0f);
    }

  }

}
