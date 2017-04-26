// <copyright file="IOSTokenClient.cs" company="Google Inc.">
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
namespace GooglePlayGames.IOS {
    using System;
    using System.Linq;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.Native; // Token retrieval
    using GooglePlayGames.Native.PInvoke;
    using GooglePlayGames.OurUtils;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEngine;

    using C = GooglePlayGames.Native.Cwrapper.InternalHooks;

    internal class IOSTokenClient: TokenClient
    {
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GooglePlayGetIdToken();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GooglePlayGetAccessToken();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GooglePlayGetUserEmail();

        /// <summary>
        /// Sets the rationale.  Not used for ios.
        /// </summary>
        /// <param name="rationale">Rationale.</param>
        public void SetRationale(string rationale)
        {
            // not used for iOS.
        }


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
        public string GetEmail()
        {
            return _GooglePlayGetUserEmail();
        }

        /// <summary>
        /// Gets the user's email with a callback.
        /// </summary>
        /// <remarks>The email address returned is selected by the user from the accounts present
        /// on the device. There is no guarantee this uniquely identifies the player.
        /// For unique identification use the id property of the local player.
        /// The user can also choose to not select any email address, meaning it is not
        /// available.</remarks>
        /// <param name="callback">The callback with a status code of the request,
        /// and string which is the email. It can be null.</param>
        public void GetEmail(Action<CommonStatusCodes, string> callback)
        {
            string email = GetEmail();
            CommonStatusCodes status =
                string.IsNullOrEmpty(email) ? CommonStatusCodes.Error : CommonStatusCodes.Success;
            if (callback != null) {
                callback(status, email);
            }
        }


        /// <summary>Gets the access token currently associated with the Unity activity.</summary>
        /// <returns>The OAuth 2.0 access token.</returns>
        public string GetAccessToken()
        {
            return _GooglePlayGetAccessToken();
        }

        /// <summary>
        /// Gets the OpenID Connect ID token for authentication with a server backend.
        /// </summary>
        /// <param name="idTokenCallback"> A callback to be invoked after token is retrieved. Will be passed null value
        /// on failure. </param>
        /// <param name="serverClientID">Server client ID from console.developers.google.com or the Play Games
        /// services console.</param>
        public void GetIdToken(string serverClientID, Action<string> idTokenCallback)
        {
            var token =  _GooglePlayGetIdToken();
            if(String.IsNullOrEmpty(token))
            {
                idTokenCallback(null);
            }
            else
            {
                idTokenCallback(token);
            }
        }

        public string GetAuthCode()
        {
            throw new NotImplementedException();
        }

        public string GetIdToken()
        {
            throw new NotImplementedException();
        }

        public void Signout()
        {
            throw new NotImplementedException();
        }

        public void SetRequestAuthCode(bool flag, bool forceRefresh)
        {
            throw new NotImplementedException();
        }

        public void SetRequestEmail(bool flag)
        {
            throw new NotImplementedException();
        }

        public void SetRequestIdToken(bool flag)
        {
            throw new NotImplementedException();
        }

        public void SetWebClientId(string webClientId)
        {
            throw new NotImplementedException();
        }

        public void SetAccountName(string accountName)
        {
            throw new NotImplementedException();
        }

        public void AddOauthScopes(string[] scopes)
        {
            throw new NotImplementedException();
        }

        public void SetHidePopups(bool flag)
        {
            throw new NotImplementedException();
        }

        public bool NeedsToRun()
        {
            throw new NotImplementedException();
        }

        public void FetchTokens(Action callback)
        {
            throw new NotImplementedException();
        }
    }
}
#endif
