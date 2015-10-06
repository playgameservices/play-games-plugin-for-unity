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

namespace GooglePlayGames.BasicApi
{
    /// <summary>
    /// Common status codes.
    /// See https://developers.google.com/android/reference/com/google/android/gms/common/api/CommonStatusCodes
    /// </summary>
    public enum CommonStatusCodes
    {
        SuccessCached = -1,
        Success = 0,
        ServiceMissing = 1,
        ServiceVersionUpdateRequired = 2,
        ServiceDisabled = 3,
        SignInRequired = 4,
        InvalidAccount = 5,
        ResolutionRequired = 6,
        NetworkError = 7,
        InternalError = 8,
        ServiceInvalid = 9,
        DeveloperError = 10,
        LicenseCheckFailed = 11,
        Error = 13,
        Interrupted = 14,
        Timeout = 15,
        Canceled = 16,
        ApiNotConnected = 17,
        AuthApiInvalidCredentials = 3000,
        AuthApiAccessForbidden = 3001,
        AuthApiClientError = 3002,
        AuthApiServerError = 3003,
        AuthTokenError = 3004,
        AuthUrlResolution = 3005
    }

}
