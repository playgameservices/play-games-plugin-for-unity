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
    using UnityEngine;

    internal class AndroidTokenClient: TokenClient
    {
        public static AndroidJavaObject GetActivity()
        {
            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        /// <summary>Gets the Google API client Java object.</summary>
        /// <returns>The API client associated with the current Unity app.</returns>
        /// <param name="serverClientID">The OAuth 2.0 client ID for a backend server.</param>
        public AndroidJavaObject GetApiClient(bool getServerAuthCode = false,
                                              string serverClientID = null)
        {
            Debug.Log("Calling GetApiClient....");
            using (var currentActivity = GetActivity())
            {
                using (AndroidJavaClass jc_plus = new AndroidJavaClass("com.google.android.gms.plus.Plus"))
                {
                    using (AndroidJavaObject jc_builder = new AndroidJavaObject("com.google.android.gms.common.api.GoogleApiClient$Builder", currentActivity))
                    {
                        jc_builder.Call<AndroidJavaObject>("addApi", jc_plus.GetStatic<AndroidJavaObject>("API"));
                        jc_builder.Call<AndroidJavaObject>("addScope", jc_plus.GetStatic<AndroidJavaObject>("SCOPE_PLUS_LOGIN"));
                        if (getServerAuthCode)
                        {
                            jc_builder.Call<AndroidJavaObject>("requestServerAuthCode", serverClientID, jc_builder);
                        }
                        AndroidJavaObject client = jc_builder.Call<AndroidJavaObject>("build");
                        client.Call("connect");

                        // limit spinning to 100, to minimize blocking when not
                        // working as expected.
                        // TODO: Make this a callback.
                        int ct = 100;
                        while ((!client.Call<bool>("isConnected")) && (ct-- != 0))
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                        Debug.Log("Done GetApiClient is " + client);
                        return client;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the account name of the currently signed-in user to later use for token retrieval.
        /// </summary>
        /// <remarks>Currently only used internally to encourage using the unique player ID instead.</remarks>
        /// <returns>The current user's Google account name.</returns>
        private string GetAccountName()
        {
            string accountName;
            using (AndroidJavaClass plusService = new AndroidJavaClass("com.google.android.gms.plus.Plus"))
            {
                using (AndroidJavaObject accountService = plusService.GetStatic<AndroidJavaObject>("AccountApi"))
                {
                    using (var apiClient = GetApiClient())
                    {
                        accountName = accountService.Call<string>("getAccountName", apiClient);
                    }
                }
            }
            return accountName;
        }

        /// <summary>Gets the current user's email.</summary>
        /// <returns>A string representing the email.</returns>
        public string GetEmail()
        {
            return GetAccountName();
        }

        /// <summary>Gets the authZ token for server authorization.</summary>
        /// <param name="serverClientID">The client ID for the server that will exchange the one-time code.</param>
        /// <returns> An authorization code upon success.</returns>
        public string GetAuthorizationCode(string serverClientID)
        {
            throw new NotImplementedException();
        }

        /// <summary>Gets the access token currently associated with the Unity activity.</summary>
        /// <returns>The OAuth 2.0 access token.</returns>
        public string GetAccessToken()
        {
            string token = null;
            string accountName = GetAccountName() ?? "NULL";
            string scope = "oauth2:https://www.googleapis.com/auth/plus.me";

            using (var googleAuthUtil = new AndroidJavaClass("com.google.android.gms.auth.GoogleAuthUtil"))
            {
                token = googleAuthUtil.CallStatic<string>("getToken", GetActivity(), accountName, scope);
            }

            Debug.Log("Access Token " + token);
            return token;
        }

        /// <summary>Gets the OpenID Connect ID token for authentication with a server backend.</summary>
        /// <returns>The OpenID Connect ID token.</returns>
        /// <param name="serverClientID">Server client ID from console.developers.google.com or the Play Games
        /// services console.</param>
        public string GetIdToken(string serverClientID)
        {
            string token = null;
            string accountName = GetAccountName() ?? "NULL";
            string scope = "audience:server:client_id:" + serverClientID;

            using (AndroidJavaClass unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"),
                   googleAuthUtil = new AndroidJavaClass("com.google.android.gms.auth.GoogleAuthUtil"))
            {
                using (AndroidJavaObject currentActivity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    token = googleAuthUtil.CallStatic<string>("getToken", currentActivity, accountName, scope);
                }
            }

            Debug.Log("ID Token " + token);
            return token;
        }
    }
}
#endif
