// <copyright file="AuthResponse.cs" company="Google Inc.">
// Copyright (C) 2025 Google Inc.
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
namespace GooglePlayGames.BasicApi
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the response received from Play Games Services when requesting a server-side OAuth 2.0
    /// authorization code for the signed-in player.
    /// </summary>
    public class AuthResponse
    {
        private readonly string _authCode;
        private readonly List<AuthScope> _grantedScopes;

        /// <summary>
        /// Constructs an <c>AuthResponse</c> with the provided granted scopes and authentication code.
        /// </summary>
        /// <param name="authCode">The authentication code.</param>
        /// <param name="grantedScopes">A list of <c>AuthScope</c> objects representing the granted scopes.</param>
        /// <exception cref="ArgumentNullException">If <c>grantedScopes</c> is null.</exception>
        public AuthResponse(string authCode, List<AuthScope> grantedScopes)
        {
            if (grantedScopes == null)
            {
                throw new ArgumentNullException(nameof(grantedScopes), "Granted scopes list cannot be null");
            }

            _authCode = authCode;
            _grantedScopes = grantedScopes;
        }


        /// <summary>
        /// Gets the list of <c>AuthScope</c> permissions that the user has granted.
        /// </summary>
        /// <remarks>
        /// A list of the <c>AuthScope</c> permissions the user explicitly granted consent for (or
        /// previously approved). The list will be empty if the user declines consent and none of the
        /// requested <c>AuthScope</c> were previously granted.
        /// </remarks>
        /// <returns>A <c>List</c> of <c>AuthScope</c> objects, representing the granted permissions.</returns>
        public List<AuthScope> GetGrantedScopes()
        {
            return _grantedScopes;
        }

        /// <summary>
        /// Gets the OAuth 2.0 authorization code.
        /// </summary>
        /// <remarks>
        /// This code is a short-lived credential that should be sent securely to your server to be
        /// exchanged for an access token and conditionally a refresh token.
        /// </remarks>
        /// <returns>A string containing the OAuth 2.0 authorization code.</returns>
        public string GetAuthCode()
        {
            return _authCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (AuthResponse)obj;
            return _grantedScopes.Equals(other._grantedScopes) && _authCode == other._authCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_grantedScopes, _authCode);
        }

        public override string ToString()
        {
            string grantedScopesText = _grantedScopes.Count > 0 ? string.Join(", ", _grantedScopes.ToArray()) : "[]";
            return $"AuthResponse {{ grantedScopes = {grantedScopesText}, authCode = {_authCode} }}";
        }
    }
}
#endif