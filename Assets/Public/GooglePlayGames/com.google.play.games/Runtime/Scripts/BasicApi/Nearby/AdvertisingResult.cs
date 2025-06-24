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
    /// Represents the result of an attempt to start advertising for nearby connections.
    /// </summary>
    public struct AdvertisingResult
    {
        private readonly ResponseStatus mStatus;
        private readonly string mLocalEndpointName;

        /// <summary>
        /// Constructs a new <see cref="AdvertisingResult"/>.
        /// </summary>
        /// <param name="status">The result of the advertising attempt.</param>
        /// <param name="localEndpointName">The name of the local endpoint.</param>
        /// <exception cref="System.ArgumentNullException">If <see cref="localEndpointName"/> is null.</exception>
        public AdvertisingResult(ResponseStatus status, string localEndpointName)
        {
            this.mStatus = status;
            this.mLocalEndpointName = Misc.CheckNotNull(localEndpointName);
        }

        /// <summary>
        /// Gets a value indicating whether the advertising operation was successful.
        /// </summary>
        public bool Succeeded
        {
            get { return mStatus == ResponseStatus.Success; }
        }

        /// <summary>
        /// Gets the response status of the advertising operation.
        /// </summary>
        public ResponseStatus Status
        {
            get { return mStatus; }
        }

        /// <summary>
        /// Gets the name of the local endpoint used in the advertising operation.
        /// </summary>
        public string LocalEndpointName
        {
            get { return mLocalEndpointName; }
        }
    }
}