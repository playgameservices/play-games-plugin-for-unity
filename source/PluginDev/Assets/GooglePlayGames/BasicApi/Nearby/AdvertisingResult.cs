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

    public struct AdvertisingResult
    {
        private readonly ResponseStatus mStatus;
        private readonly string mLocalEndpointName;

        public AdvertisingResult(ResponseStatus status, string localEndpointName)
        {
            this.mStatus = status;
            this.mLocalEndpointName = Misc.CheckNotNull(localEndpointName);
        }

        public bool Succeeded
        {
            get
            {
                return mStatus == ResponseStatus.Success;
            }
        }

        public ResponseStatus Status
        {
            get
            {
                return mStatus;
            }
        }

        public string LocalEndpointName
        {
            get
            {
                return mLocalEndpointName;
            }
        }
    }
}