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
    using Com.Google.Android.Gms.Common.Api;
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

        public static void ShowAchievementsUI(Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            {
                using(var task = helperFragment.CallStatic<AndroidJavaObject>("showAchievementUi", AndroidHelperFragment.GetActivity()))
                {
                    task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<int>(
                        uiCode => {
                            Debug.Log("ShowAchievementsUI result " + uiCode);
                            if (cb != null) 
                            {
                                PlayGamesHelperObject.RunOnGameThread(() => cb.Invoke((UIStatus)uiCode));
                            }
                        }
                    ));
                    task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                        exception => {
                            Debug.Log("ShowAchievementsUI failed with exception");
                            if (cb != null) 
                            {
                                PlayGamesHelperObject.RunOnGameThread(() => cb.Invoke(UIStatus.InternalError));
                            }
                        }
                    ));
                }
            }
        }

        public static void ShowSelectSnapshotUI(bool showCreateSaveUI, bool showDeleteSaveUI,
                int maxDisplayedSavedGames, string uiTitle, Action<SelectUIStatus, ISavedGameMetadata> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            {
                using(var task = helperFragment.CallStatic<AndroidJavaObject>("showSelectSnapshotUi", 
                    AndroidHelperFragment.GetActivity(), uiTitle, showCreateSaveUI, showDeleteSaveUI, maxDisplayedSavedGames))
                {
                    task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                        // SelectSnapshotUiRequest.Result result
                        result => {
                            SelectUIStatus status = (SelectUIStatus)result.Get<int>("status");
                            AndroidJavaObject javaMetadata = result.Get<AndroidJavaObject>("metadata");
                            AndroidSnapshotMetadata metadata = javaMetadata == null ? null : new AndroidSnapshotMetadata(javaMetadata, /* contents= */null);
                            Debug.Log("ShowSelectSnapshotUI result " + status);
                            PlayGamesHelperObject.RunOnGameThread(() => cb.Invoke(status, metadata));
                        }
                    ));
                    task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                        exception => {
                            Debug.Log("ShowSelectSnapshotUI failed with exception");
                            if (cb != null) 
                            {
                                PlayGamesHelperObject.RunOnGameThread(() => cb.Invoke(SelectUIStatus.InternalError, null));
                            }
                        }
                    ));
                }
            }
        }
    }
}
#endif
