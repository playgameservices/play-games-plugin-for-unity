// <copyright file="GameInfo.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
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
    // NOTE: The members of this class were previously accessed as if they were const strings.
    // Changing them to static properties, as is necessary to load from PlayGamesSettings,
    // is a binary breaking change for scenarios where const values are required, such as
    // in attributes or switch statements. Developers should migrate to using PlayGamesSettings
    // directly.
    [System.Obsolete("GameInfo is deprecated, use PlayGamesSettings instead.")]
    public static class GameInfo
    {
        [System.Obsolete("Use PlayGamesSettings.LoadInstance().AppId instead.")]
        public static string ApplicationId
        {
            get
            {
                var settings = PlayGamesSettings.LoadInstance();
                return settings != null ? settings.AppId : string.Empty;
            }
        }

        [System.Obsolete("Use PlayGamesSettings.LoadInstance().WebClientId instead.")]
        public static string WebClientId
        {
            get
            {
                var settings = PlayGamesSettings.LoadInstance();
                return settings != null ? settings.WebClientId : string.Empty;
            }
        }

        [System.Obsolete("Use PlayGamesSettings.LoadInstance().NearbyServiceId instead.")]
        public static string NearbyConnectionServiceId
        {
            get
            {
                var settings = PlayGamesSettings.LoadInstance();
                return settings != null ? settings.NearbyServiceId : string.Empty;
            }
        }

        [System.Obsolete]
        public static bool ApplicationIdInitialized()
        {
            var settings = PlayGamesSettings.LoadInstance();
            return settings != null && !string.IsNullOrEmpty(settings.AppId);
        }

        [System.Obsolete]
        public static bool WebClientIdInitialized()
        {
            var settings = PlayGamesSettings.LoadInstance();
            return settings != null && !string.IsNullOrEmpty(settings.WebClientId);
        }

        [System.Obsolete]
        public static bool NearbyConnectionsInitialized()
        {
            var settings = PlayGamesSettings.LoadInstance();
            return settings != null && !string.IsNullOrEmpty(settings.NearbyServiceId);
        }
    }
}