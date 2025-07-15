// <copyright file="ConnectionRequest.cs" company="Google Inc.">
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
    /// Represents a connection request for a Nearby API operation.
    /// </summary>
    /// <remarks>
    /// @deprecated This struct will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public struct ConnectionRequest
    {
        private readonly EndpointDetails mRemoteEndpoint;
        private readonly byte[] mPayload;

        /// <summary>
        /// Initializes a new instance of the ConnectionRequest struct.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the remote endpoint.</param>
        /// <param name="remoteEndpointName">The name of the remote endpoint.</param>
        /// <param name="serviceId">The service ID for the connection.</param>
        /// <param name="payload">The payload data associated with the request.</param>
        /// <remarks>
        /// @deprecated This constructor will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public ConnectionRequest(string remoteEndpointId,
            string remoteEndpointName, string serviceId, byte[] payload)
        {
            Logger.d("Constructing ConnectionRequest");
            mRemoteEndpoint = new EndpointDetails(remoteEndpointId, remoteEndpointName, serviceId);
            this.mPayload = Misc.CheckNotNull(payload);
        }

        /// <summary>
        /// Gets the remote endpoint details for the connection request.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public EndpointDetails RemoteEndpoint
        {
            get { return mRemoteEndpoint; }
        }

        /// <summary>
        /// Gets the payload data associated with the connection request.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public byte[] Payload
        {
            get { return mPayload; }
        }
    }
}