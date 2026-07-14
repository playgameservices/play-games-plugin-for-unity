// <copyright file="PlayGamesSettings.cs" company="Google Inc.">
// Copyright (C) 2026 Google Inc. All Rights Reserved.
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

    public class PlayGamesSettings : ScriptableObject
    {
        private const string SettingsFile = "PlayGamesSettings";

        [SerializeField]
        private string mAppId = string.Empty;

        [SerializeField]
        private string mWebClientId = string.Empty;

        [SerializeField]
        private string mNearbyServiceId = string.Empty;

        public string AppId
        {
            get { return mAppId; }
            set { mAppId = value; }
        }

        public string WebClientId
        {
            get { return mWebClientId; }
            set { mWebClientId = value; }
        }

        public string NearbyServiceId
        {
            get { return mNearbyServiceId; }
            set { mNearbyServiceId = value; }
        }

        private static PlayGamesSettings sInstance;

        public static PlayGamesSettings LoadInstance()
        {
            if (sInstance == null)
            {
                sInstance = Resources.Load<PlayGamesSettings>(SettingsFile);
            }
            return sInstance;
        }
    }
}
