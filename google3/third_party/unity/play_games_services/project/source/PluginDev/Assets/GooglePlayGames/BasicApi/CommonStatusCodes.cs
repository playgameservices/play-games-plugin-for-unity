// <copyright file="CommonStatusCodes.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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
    /// <summary>
    /// Common status codes.
    /// See https://developers.google.com/android/reference/com/google/android/gms/common/api/CommonStatusCodes
    /// </summary>
    public enum CommonStatusCodes
    {
        /// <summary>The operation was successful, but the device's cache was used.</summary>
        SuccessCached = -1,

        /// <summary>The operation was successful.</summary>
        Success = 0,

        /// <summary>Google Play services is missing on this device.</summary>
        ServiceMissing = 1,

        /// <summary>The installed version of Google Play services is out of date.</summary>
        ServiceVersionUpdateRequired = 2,

        /// <summary>The installed version of Google Play services has been disabled on this device.</summary>
        ServiceDisabled = 3,

        /// <summary>The client attempted to connect to the service but the user is not signed in.</summary>
        SignInRequired = 4,

        /// <summary>The client attempted to connect to the service with an invalid account name specified.</summary>
        InvalidAccount = 5,

        /// <summary>Completing the operation requires some form of resolution.</summary>
        ResolutionRequired = 6,

        /// <summary>A network error occurred.</summary>
        NetworkError = 7,

        /// <summary>An internal error occurred.</summary>
        InternalError = 8,

        /// <summary>The version of the Google Play services installed on this device is not authentic.</summary>
        ServiceInvalid = 9,

        /// <summary>The application is misconfigured.</summary>
        DeveloperError = 10,

        /// <summary>The application is not licensed to the user.</summary>
        LicenseCheckFailed = 11,

        /// <summary>The operation failed with no more detailed information.</summary>
        Error = 13,

        /// <summary>A blocking call was interrupted while waiting and did not run to completion.</summary>
        Interrupted = 14,

        /// <summary>Timed out while awaiting the result.</summary>
        Timeout = 15,

        /// <summary>The result was canceled either due to client disconnect or cancel().</summary>
        Canceled = 16,

        /// <summary>The client attempted to call a method from an API that failed to connect.</summary>
        ApiNotConnected = 17,

        /// <summary>Invalid credentials were provided.</summary>
        AuthApiInvalidCredentials = 3000,

        /// <summary>Access is forbidden.</summary>
        AuthApiAccessForbidden = 3001,

        /// <summary>Error related to the client.</summary>
        AuthApiClientError = 3002,

        /// <summary>Error related to the server.</summary>
        AuthApiServerError = 3003,

        /// <summary>Error related to token.</summary>
        AuthTokenError = 3004,

        /// <summary>Error related to auth URL resolution.</summary>
        AuthUrlResolution = 3005
    }
}
#endif