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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.BasicApi.Nearby
{
    using System;
    using GooglePlayGames.OurUtils;

    public enum InitializationStatus
    {
        Success,
        VersionUpdateRequired,
        InternalError
    }

    public struct NearbyConnectionConfiguration
    {
        public const int MaxUnreliableMessagePayloadLength = 1168;
        public const int MaxReliableMessagePayloadLength = 4096;

        private readonly Action<InitializationStatus> mInitializationCallback;
        private readonly long mLocalClientId;

        public NearbyConnectionConfiguration(Action<InitializationStatus> callback,
                                             long localClientId)
        {
            this.mInitializationCallback = Misc.CheckNotNull(callback);
            this.mLocalClientId = localClientId;
        }

        public long LocalClientId
        {
            get
            {
                return mLocalClientId;
            }
        }

        public Action<InitializationStatus> InitializationCallback
        {
            get
            {
                return mInitializationCallback;
            }
        }
    }
}
#endif
