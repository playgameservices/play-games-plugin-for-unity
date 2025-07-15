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

    /// <summary>
    /// Represents the details of a remote endpoint discovered in the network.
    /// This is an immutable value-type.
    /// </summary>
    /// <remarks>
    /// @deprecated This struct will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public struct EndpointDetails
    {
        private readonly string mEndpointId;
        private readonly string mName;
        private readonly string mServiceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointDetails"/> struct.
        /// </summary>
        /// <param name="endpointId">The unique identifier of the endpoint.</param>
        /// <param name="name">The human-readable name of the endpoint.</param>
        /// <param name="serviceId">The identifier of the service the endpoint is advertising.</param>
        /// <remarks>
        /// @deprecated This constructor will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public EndpointDetails(string endpointId, string name, string serviceId)
        {
            this.mEndpointId = Misc.CheckNotNull(endpointId);
            this.mName = Misc.CheckNotNull(name);
            this.mServiceId = Misc.CheckNotNull(serviceId);
        }

        /// <summary>
        /// Gets the unique ID for the endpoint.
        /// </summary>
        /// <value>The endpoint's unique ID.</value>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string EndpointId
        {
            get { return mEndpointId; }
        }

        /// <summary>
        /// Gets the human-readable name of the endpoint.
        /// </summary>
        /// <value>The endpoint's name.</value>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string Name
        {
            get { return mName; }
        }

        /// <summary>
        /// Gets the service ID the endpoint is advertising.
        /// </summary>
        /// <value>The service ID.</value>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string ServiceId
        {
            get { return mServiceId; }
        }
    }
}