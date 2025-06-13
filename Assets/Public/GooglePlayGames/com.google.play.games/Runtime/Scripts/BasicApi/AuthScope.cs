// <copyright file="AuthScope.cs" company="Google Inc.">
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
    using System.Linq;

    /// <summary>
    /// Represents type-safe constants for the specific OAuth 2.0 authorization scopes used when
    /// requesting server-side access to Play Games Services web APIs.
    /// </summary>
    public enum AuthScope
    {
        /// <summary>
        /// See your primary Google Account email address.
        /// </summary>
        EMAIL,

        /// <summary>
        /// See your personal info, including any personal info you've made publicly available.
        /// </summary>
        PROFILE,

        /// <summary>
        /// Associate you with your personal info on Google.
        /// </summary>
        OPEN_ID
    }

    /// <summary>
    /// Extensions for the AuthScope enum.
    /// <remarks>
    /// These extensions are used to converting between the AuthScope enum and its string
    /// representation.
    /// </remarks>
    /// </summary>
    public static class AuthScopeExtensions
    {
        /// <summary>
        /// A map of AuthScope string values to their enum representations.
        /// </summary>
        private static readonly Dictionary<string, AuthScope> _stringToEnumMap =
            new Dictionary<string, AuthScope>
            {
                { "EMAIL", AuthScope.EMAIL },
                { "PROFILE", AuthScope.PROFILE },
                { "OPEN_ID", AuthScope.OPEN_ID }
            };

        /// <summary>
        /// A map of AuthScope enum values to their string representations.
        /// </summary>
        private static readonly Dictionary<AuthScope, string> _enumToStringMap =
           _stringToEnumMap.ToDictionary(pair => pair.Value, pair => pair.Key);

        /// <summary>
        /// Returns the standard string representation of this OAuth 2.0 scope.
        /// </summary>
        /// <param name="authScope">The AuthScope enum value.</param>
        /// <returns>The string value used to represent this scope.</returns>
        /// <exception cref="ArgumentException">If the provided AuthScope is not valid.</exception>
        public static string GetValue(this AuthScope authScope)
        {
          if (!_enumToStringMap.ContainsKey(authScope))
          {
              throw new ArgumentException($"Invalid AuthScope: {authScope}");
          }
          return _enumToStringMap[authScope];
        }

        /// <summary>
        /// Returns the AuthScope enum value corresponding to the provided string.
        /// </summary>
        /// <param name="value">The string value used to represent the scope.</param>
        /// <returns>The AuthScope enum value corresponding to the provided string.</returns>
        /// <exception cref="ArgumentException">If the provided string is not a valid AuthScope.</exception>
        public static AuthScope FromValue(string value)
        {
            if (!_stringToEnumMap.ContainsKey(value))
            {
                throw new ArgumentException($"Invalid AuthScope: {value}");
            }
            return _stringToEnumMap[value];
        }
    }
}
#endif