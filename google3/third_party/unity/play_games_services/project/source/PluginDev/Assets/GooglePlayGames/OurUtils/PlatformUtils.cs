// <copyright file="PlayGamesHelperObject.cs" company="Google Inc.">
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

#if UNITY_ANDROID
namespace GooglePlayGames.OurUtils
{
    using UnityEngine;
    using System;

    public static class PlatformUtils
    {
        /// <summary>
        /// Check if the Google Play Games platform is supported at runtime.
        /// </summary>
        /// <value>If the platform is supported.</value>
        public static bool Supported
        {
            get
            {
#if UNITY_EDITOR
                return false;
#else
                var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var ca = up.GetStatic<AndroidJavaObject>("currentActivity");
                var packageManager = ca.Call<AndroidJavaObject>("getPackageManager");

                AndroidJavaObject launchIntent = null;
                //if the app is installed, no errors. Else, doesn't get past next line
                try
                {
                    launchIntent =
 packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", "com.google.android.play.games");
                }
                catch (Exception)
                {
                    return false;
                }

                return launchIntent != null;
#endif
            }
        }
    }
}
#endif //UNITY_ANDROID