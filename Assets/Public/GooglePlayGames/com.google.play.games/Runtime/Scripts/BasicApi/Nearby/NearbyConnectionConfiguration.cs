// <copyright file="NearbyConnectionConfiguration.cs" company="Google Inc.">
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
    using GooglePlayGames.OurUtils;

    /// <summary>
    /// Represents the configuration for a Nearby Connections operation.
    /// Includes initialization status and client-specific configuration.
    /// </summary>
    public enum InitializationStatus
    {
        /// <summary>
        /// Indicates that the initialization was successful.
        /// </summary>
        Success,

        /// <summary>
        /// Signifies that a version update is required for nearby connections.
        /// </summary>
        VersionUpdateRequired,

        /// <summary>
        /// Denotes that an internal error occurred during initialization.
        /// </summary>
        InternalError
    }

    /// <summary>
    /// Defines the configuration for establishing a Nearby connection.
    /// This includes parameters like client ID and initialization callback.
    /// </summary>
    public struct NearbyConnectionConfiguration
    {
        /// <summary>
        /// A constant integer representing the maximum payload length for unreliable messages.
        /// </summary>
        public const int MaxUnreliableMessagePayloadLength = 1168;

        /// <summary>
        /// A constant integer representing the maximum payload length for reliable messages.
        /// </summary>
        public const int MaxReliableMessagePayloadLength = 4096;

        private readonly Action<InitializationStatus> mInitializationCallback;
        private readonly long mLocalClientId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearbyConnectionConfiguration"/> struct.
        /// </summary>
        /// <param name="callback">A callback that will be invoked when initialization completes.</param>
        /// <param name="localClientId">The unique identifier for the local client.</param>
        public NearbyConnectionConfiguration(Action<InitializationStatus> callback,
            long localClientId)
        {
            this.mInitializationCallback = Misc.CheckNotNull(callback);
            this.mLocalClientId = localClientId;
        }

        /// <summary>
        /// Gets the unique identifier for the local client.
        /// </summary>
        public long LocalClientId
        {
            get { return mLocalClientId; }
        }

        /// <summary>
        /// Gets the callback to be invoked upon the completion of initialization.
        /// </summary>
        public Action<InitializationStatus> InitializationCallback
        {
            get { return mInitializationCallback; }
        }
    }
}