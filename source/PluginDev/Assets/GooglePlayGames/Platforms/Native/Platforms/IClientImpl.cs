// <copyright file="IClientImpl.cs" company="Google Inc.">
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

#if UNITY_ANDROID

using System;
using GooglePlayGames.BasicApi;
using GooglePlayGames.Native.PInvoke;

namespace GooglePlayGames.Native
{
    /// <summary>
    /// Interface defining platform specific functionality.
    /// </summary>
    internal interface IClientImpl
    {
        PlatformConfiguration CreatePlatformConfiguration (PlayGamesClientConfiguration clientConfig);

        TokenClient CreateTokenClient (bool reset);

        void GetPlayerStats(IntPtr apiClientPtr, Action<CommonStatusCodes, GooglePlayGames.BasicApi.PlayerStats> callback);

        /// <summary>
        /// Sets the gravity for popups (Android only).
        /// </summary>
        /// <remarks>This can only be called after authentication.  It affects
        /// popups for achievements and other game services elements.</remarks>
        /// <param name="apiClient">Pointer to the Google API client.</param>
        /// <param name="gravity">Gravity for the popup.</param>
        void SetGravityForPopups(IntPtr apiClient, Gravity gravity);
    }
}

#endif
