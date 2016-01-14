// <copyright file="IOSClient.cs" company="Google Inc.">
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

#if (UNITY_IPHONE && !NO_GPGS)

namespace GooglePlayGames.IOS
{
    using System;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.Native.PInvoke;
    using UnityEngine;

    internal class IOSClient : IClientImpl
    {

        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern void _GooglePlayEnableProfileScope();

        public PlatformConfiguration CreatePlatformConfiguration()
        {
            if (!GameInfo.IosClientIdInitialized())
            {
                throw new InvalidOperationException("Could not locate the OAuth Client ID, " +
                    "provide this by navigating to Google Play Games > iOS Setup");
            }

            if (GameInfo.WebClientIdInitialized())
            {
                _GooglePlayEnableProfileScope();
            }

            var config = IosPlatformConfiguration.Create();
            config.SetClientId(GameInfo.IosClientId);
            return config;
        }

        /// <summary>
        /// Creates the token client.
        /// </summary>
        /// <returns>The token client.</returns>
        /// <param name="reset">not used for iOS</param>
        public TokenClient CreateTokenClient(bool reset)
        {
            return new IOSTokenClient();
        }
    }
}
#endif
