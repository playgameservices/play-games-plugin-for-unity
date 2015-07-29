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


namespace GooglePlayGames.Native {
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
internal class AndroidTokenClient: TokenClient
{
        private readonly GameServices mServices;
        internal AndroidTokenClient(GameServices services)
        {
            mServices = Misc.CheckNotNull(services);
        }

        /// <summary>
        /// Gets the Google API client Java object.
        /// </summary>
        /// <returns>The API client associated with the current Unity app.</returns>
        /// <param name="services">The Google Play Games Services object.</param>
        /// <param name="serverClientID">The OAuth 2.0 client ID for a backend server.</param>
        private AndroidJavaObject GetApiClient() {
            return JavaUtils.JavaObjectFromPointer(C.InternalHooks_GetApiClient(mServices.AsHandle()));
        }

        /// <summary>
        /// Gets the email address of the currently signed-in user to later use for selecting the account to
        /// get the token for.
        /// </summary>
        /// <remarks>Currently only used internally to encourage using the unique player ID instead.</remarks>
        /// <returns>The email address of the current user.</returns>
        private string GetEmail()
        {
            string email;
            using (AndroidJavaClass plusService = new AndroidJavaClass("com.google.android.gms.plus.Plus"))
            {
                using (AndroidJavaObject accountService = plusService.GetStatic<AndroidJavaObject>("AccountApi"))
                {
                    using (var apiClient = GetApiClient())
                    {
                        email  = accountService.Call<string>("getAccountName", apiClient);
                    }
                }
            }
            return email;
        }

        /// <summary>Gets the authZ/authN token.</summary>
        /// <param name="serverClietnID">The Server client ID. Set to null for access token retrieval.</param>
        /// <returns> If clientId is set to null, returns the current access token; otherwise, returns an
        /// OpenID Connect ID token for the server.</returns>
        public string GetToken(string serverClientID)
        {
            if (serverClientID == null)
            {
                return GetAccessToken();
            }

            // Returns the ID token
            return GetIdToken(serverClientID);
        }

        /// <summary>Gets the access token currently assocuiated with the Unity activity.</summary>
        /// <returns>The OAuth 2.0 access token.</returns>
        public string GetAccessToken()
        {
            string token = null;
            string email = GetEmail() ?? "NULL";
            string scope = "oauth2:https://www.googleapis.com/auth/plus.me";

            using (AndroidJavaClass unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"),
                   googleAuthUtil = new AndroidJavaClass("com.google.android.gms.auth.GoogleAuthUtil"))
            {
                using(AndroidJavaObject currentActivity =
                    unityActivity.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    token = googleAuthUtil.CallStatic<string>("getToken", currentActivity, email, scope);
                }
            }

            return token;
        }

        /// <summary>
        /// Gets the OpenID Connect ID token for authentication with a server backend.
        /// </summary>
        /// <returns>The OpenID Connect ID token.</returns>
        /// <param name="serverClientID">Server client ID from console.developers.google.com or the Play Games
        /// services console.</param>
        public string GetIdToken(string serverClientID)
        {
            string token = null;
            string email = GetEmail() ?? "NULL";
            string scope = "audience:server:client_id:" + serverClientID;

            using (AndroidJavaClass unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"),
                   googleAuthUtil = new AndroidJavaClass("com.google.android.gms.auth.GoogleAuthUtil"))
            {
                using(AndroidJavaObject currentActivity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    token = googleAuthUtil.CallStatic<string>("getToken", currentActivity, email, scope);
                }
            }

            Debug.Log("Token " + token);
            return token;
        }
    }
}
#endif
