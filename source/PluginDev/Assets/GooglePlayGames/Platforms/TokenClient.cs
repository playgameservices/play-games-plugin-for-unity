// <copyright file="TokenClient.cs" company="Google Inc.">
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
namespace GooglePlayGames
{
    using System;

    internal interface TokenClient
    {
        /// <summary>
        /// Gets the user's email.
        /// </summary>
        /// <remarks>The email address returned is selected by the user from the accounts present
        /// on the device. There is no guarantee this uniquely identifies the player.
        /// For unique identification use the id property of the local player.
        /// The user can also choose to not select any email address, meaning it is not
        /// available.</remarks>
        /// <returns>The user email or null if not authenticated or the permission is
        /// not available.</returns>
        string GetEmail();
        string GetAuthCode();
        string GetIdToken();

        void Signout();

        void SetRequestAuthCode(bool flag, bool forceRefresh);

        void SetRequestEmail(bool flag);

        void SetRequestIdToken(bool flag);

        void SetWebClientId(string webClientId);

        void SetAccountName(string accountName);

        void AddOauthScopes(string[] scopes);

        void SetHidePopups(bool flag);

        bool NeedsToRun();

        void FetchTokens(Action callback);
    }
}
#endif