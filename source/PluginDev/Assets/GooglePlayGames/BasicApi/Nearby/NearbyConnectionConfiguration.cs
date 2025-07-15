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
    /// Represents the status of the Nearby Connections initialization process.
    /// </summary>
    /// <remarks>
    /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public enum InitializationStatus
    {
        /// <summary>
        /// Initialization was successful.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        Success,

        /// <summary>
        /// Initialization failed because the service version is out of date.
        /// An update is required.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        VersionUpdateRequired,

        /// <summary>
        /// Initialization failed due to an internal error.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        InternalError
    }

    /// <summary>
    /// Configuration for the Nearby Connections service. This is an immutable struct.
    /// </summary>
    /// <remarks>
    /// @deprecated This struct will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public struct NearbyConnectionConfiguration
    {
        /// <summary>
        /// The maximum length of a message payload for unreliable messages, in bytes.
        /// </summary>
        /// <remarks>
        /// @deprecated This constant will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public const int MaxUnreliableMessagePayloadLength = 1168;

        /// <summary>
        /// The maximum length of a message payload for reliable messages, in bytes.
        /// </summary>
        /// <remarks>
        /// @deprecated This constant will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public const int MaxReliableMessagePayloadLength = 4096;

        private readonly Action<InitializationStatus> mInitializationCallback;
        private readonly long mLocalClientId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearbyConnectionConfiguration"/> struct.
        /// </summary>
        /// <param name="callback">The callback to invoke with the result of the initialization.</param>
        /// <param name="localClientId">The ID of the local client.</param>
        /// <remarks>
        /// @deprecated This constructor will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public NearbyConnectionConfiguration(Action<InitializationStatus> callback,
            long localClientId)
        {
            this.mInitializationCallback = Misc.CheckNotNull(callback);
            this.mLocalClientId = localClientId;
        }

        /// <summary>
        /// Gets the ID of the local client.
        /// </summary>
        /// <value>The local client ID.</value>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public long LocalClientId
        {
            get { return mLocalClientId; }
        }

        /// <summary>
        /// Gets the callback to be invoked upon completion of initialization.
        /// </summary>
        /// <value>The initialization callback.</value>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public Action<InitializationStatus> InitializationCallback
        {
            get { return mInitializationCallback; }
        }
    }
}