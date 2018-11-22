// <copyright file="PlayGamesClientFactory.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using UnityEngine;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.OurUtils;

    internal class PlayGamesClientFactory
    {
        internal static IPlayGamesClient GetPlatformPlayGamesClient(
            PlayGamesClientConfiguration config)
        {
            if (Application.isEditor)
            {
                GooglePlayGames.OurUtils.Logger.d("Creating IPlayGamesClient in editor, using DummyClient.");
                return new GooglePlayGames.BasicApi.DummyClient();
            }
            #if UNITY_ANDROID
                GooglePlayGames.OurUtils.Logger.d("Creating Android IPlayGamesClient Client");
                return new GooglePlayGames.Native.NativeClient(config,
                    new GooglePlayGames.Android.AndroidClient());
            #else
                GooglePlayGames.OurUtils.Logger.d("Cannot create IPlayGamesClient for unknown platform, returning DummyClient");
                return new GooglePlayGames.BasicApi.DummyClient();
            #endif
        }
    }
}
#endif
