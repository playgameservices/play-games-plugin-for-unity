// <copyright file="AndroidTokenClient.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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
//  limitations under the License.
// </copyright>

#if UNITY_ANDROID
namespace GooglePlayGames.Android
{
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using OurUtils;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    internal class AndroidHelperFragment
    {
        private const string HelperFragmentClass = "com.google.games.bridge.HelperFragment";

        public static AndroidJavaObject GetActivity()
        {
            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        public static AndroidJavaObject GetDefaultPopupView()
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var activity = AndroidHelperFragment.GetActivity())
            {
                return helperFragment.CallStatic<AndroidJavaObject>("getDecorView", activity);
            }
        }

        public static void ShowAchievementsUI(Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task =
                helperFragment.CallStatic<AndroidJavaObject>("showAchievementUi", AndroidHelperFragment.GetActivity()))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(
                    task,
                    uiCode =>
                    {
                        OurUtils.Logger.d("ShowAchievementsUI result " + uiCode);
                        cb.Invoke((UIStatus) uiCode);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        OurUtils.Logger.e("ShowAchievementsUI failed with exception");
                        cb.Invoke(UIStatus.InternalError);
                    });
            }
        }

        public static void ShowCaptureOverlayUI()
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            {
                helperFragment.CallStatic("showCaptureOverlayUi", AndroidHelperFragment.GetActivity());
            }
        }

        public static void ShowAllLeaderboardsUI(Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task =
                helperFragment.CallStatic<AndroidJavaObject>("showAllLeaderboardsUi",
                    AndroidHelperFragment.GetActivity()))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(
                    task,
                    uiCode =>
                    {
                        OurUtils.Logger.d("ShowAllLeaderboardsUI result " + uiCode);
                        cb.Invoke((UIStatus) uiCode);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        OurUtils.Logger.e("ShowAllLeaderboardsUI failed with exception");
                        cb.Invoke(UIStatus.InternalError);
                    });
            }
        }

        public static void ShowLeaderboardUI(string leaderboardId, LeaderboardTimeSpan timeSpan, Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>("showLeaderboardUi",
                AndroidHelperFragment.GetActivity(), leaderboardId,
                AndroidJavaConverter.ToLeaderboardVariantTimeSpan(timeSpan)))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(
                    task,
                    uiCode =>
                    {
                        OurUtils.Logger.d("ShowLeaderboardUI result " + uiCode);
                        cb.Invoke((UIStatus) uiCode);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        OurUtils.Logger.e("ShowLeaderboardUI failed with exception");
                        cb.Invoke(UIStatus.InternalError);
                    });
            }
        }

        public static void ShowCompareProfileWithAlternativeNameHintsUI(
            string playerId, string otherPlayerInGameName, string currentPlayerInGameName,
            Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (
                var task = helperFragment.CallStatic<AndroidJavaObject>(
                    "showCompareProfileWithAlternativeNameHintsUI",
                    AndroidHelperFragment.GetActivity(), playerId, otherPlayerInGameName,
                    currentPlayerInGameName))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(task, uiCode =>
                {
                    OurUtils.Logger.d("ShowCompareProfileWithAlternativeNameHintsUI result " + uiCode);
                    cb.Invoke((UIStatus) uiCode);
                });
                AndroidTaskUtils.AddOnFailureListener(task, exception =>
                {
                    OurUtils.Logger.e("ShowCompareProfileWithAlternativeNameHintsUI failed with exception");
                    cb.Invoke(UIStatus.InternalError);
                });
            }
        }

        public static void IsResolutionRequired(
            AndroidJavaObject friendsSharingConsentException, Action<bool> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            {
                var isResolutionRequired = helperFragment.CallStatic<bool>(
                    "isResolutionRequired", friendsSharingConsentException);
                cb.Invoke(isResolutionRequired);
            }
        }

        public static void AskForLoadFriendsResolution(
            AndroidJavaObject friendsSharingConsentException, Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (
                var task = helperFragment.CallStatic<AndroidJavaObject>(
                    "askForLoadFriendsResolution", AndroidHelperFragment.GetActivity(),
                    friendsSharingConsentException))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(task, uiCode =>
                {
                    OurUtils.Logger.d("AskForLoadFriendsResolution result " + uiCode);
                    cb.Invoke((UIStatus) uiCode);
                });

                AndroidTaskUtils.AddOnFailureListener(task, exception =>
                {
                    OurUtils.Logger.e("AskForLoadFriendsResolution failed with exception");
                    cb.Invoke(UIStatus.InternalError);
                });
            }
        }

        public static void ShowSelectSnapshotUI(bool showCreateSaveUI, bool showDeleteSaveUI,
            int maxDisplayedSavedGames, string uiTitle, Action<SelectUIStatus, ISavedGameMetadata> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>("showSelectSnapshotUi",
                AndroidHelperFragment.GetActivity(), uiTitle, showCreateSaveUI, showDeleteSaveUI,
                maxDisplayedSavedGames))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    result =>
                    {
                        SelectUIStatus status = (SelectUIStatus) result.Get<int>("status");
                        OurUtils.Logger.d("ShowSelectSnapshotUI result " + status);

                        AndroidJavaObject javaMetadata = result.Get<AndroidJavaObject>("metadata");
                        AndroidSnapshotMetadata metadata =
                            javaMetadata == null
                                ? null
                                : new AndroidSnapshotMetadata(javaMetadata, /* contents= */null);

                        cb.Invoke(status, metadata);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        OurUtils.Logger.e("ShowSelectSnapshotUI failed with exception");
                        cb.Invoke(SelectUIStatus.InternalError, null);
                    });
            }
        }
    }
}
#endif
