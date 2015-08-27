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


        /// <summary>Gets the current user's email.</summary>
        /// <returns>A string representing the email.</returns>
        public string GetEmail()
        {
            return _GooglePlayGetUserEmail();;
        }


        /// <summary>Gets the authZ token for server authorization.</summary>
        /// <param name="serverClietnID">The client ID for the server that will exchange the one-time code.</param>
        /// <returns> An authorization code upon success.</returns>
        public string GetAuthorizationCode(string serverClientID)
        {
            throw new NotImplementedException();
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
        /// <returns>The OpenID Connect ID token.</returns>
        /// <param name="serverClientID">Server client ID from console.developers.google.com or the Play Games
        /// services console.</param>
        public string GetIdToken(string serverClientID)
        {
            return _GooglePlayGetIdToken();
        }
    }
}
#endif
