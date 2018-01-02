// <copyright file="EndpointDetails.cs" company="Google Inc.">
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
    using GooglePlayGames.OurUtils;

    public struct EndpointDetails
    {
        private readonly string mEndpointId;
        private readonly string mName;
        private readonly string mServiceId;

        public EndpointDetails(string endpointId, string name, string serviceId)
        {
            this.mEndpointId = Misc.CheckNotNull(endpointId);
            this.mName = Misc.CheckNotNull(name);
            this.mServiceId = Misc.CheckNotNull(serviceId);
        }

        public string EndpointId
        {
            get
            {
                return mEndpointId;
            }
        }

        public string Name
        {
            get
            {
                return mName;
            }
        }

        public string ServiceId
        {
            get
            {
                return mServiceId;
            }
        }
    }
}
