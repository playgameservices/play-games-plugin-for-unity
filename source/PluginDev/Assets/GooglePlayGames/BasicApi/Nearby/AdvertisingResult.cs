// <copyright file="AdvertisingResult.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi.Nearby
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.OurUtils;

    /// <summary>
    /// Represents the result of an advertising operation in the Nearby API.
    /// </summary>
    /// <remarks>
    /// @deprecated This struct will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public struct AdvertisingResult
    {
        private readonly ResponseStatus mStatus;
        private readonly string mLocalEndpointName;

        /// <summary>
        /// Initializes a new instance of the AdvertisingResult struct.
        /// </summary>
        /// <param name="status">The response status of the advertising operation.</param>
        /// <param name="localEndpointName">The local endpoint name.</param>
        /// <remarks>
        /// @deprecated This constructor will be removed in the future. We recommend that you migrate to the Play Games Services Unity Plugin (v2).
        /// </remarks>
        public AdvertisingResult(ResponseStatus status, string localEndpointName)
        {
            this.mStatus = status;
            this.mLocalEndpointName = Misc.CheckNotNull(localEndpointName);
        }

        /// <summary>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// Gets a value indicating whether the advertising operation succeeded.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public bool Succeeded
        {
            get { return mStatus == ResponseStatus.Success; }
        }

        /// <summary>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// Gets the status of the advertising operation.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public ResponseStatus Status
        {
            get { return mStatus; }
        }

        /// <summary>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// Gets the local endpoint name of the advertising operation.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string LocalEndpointName
        {
            get { return mLocalEndpointName; }
        }
    }
}
