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
    using System;
    using BasicApi;
    using OurUtils;
    using Com.Google.Android.Gms.Common.Api;
    using UnityEngine;
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
    }
}
#endif
