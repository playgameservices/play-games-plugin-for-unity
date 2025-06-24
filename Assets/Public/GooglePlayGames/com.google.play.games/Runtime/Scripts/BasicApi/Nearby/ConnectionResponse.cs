// <copyright file="ConnectionResponse.cs" company="Google Inc.">
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
    /// Represents a response to a connection request, including status, payload, and identifying information.
    /// </summary>
    public struct ConnectionResponse
    {
        private static readonly byte[] EmptyPayload = new byte[0];

        /// <summary>
        /// Status codes representing the outcome of a connection request.
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// Indicates that the connection was accepted.
            /// </summary>
            Accepted,

            /// <summary>
            /// Indicates that the connection was rejected.
            /// </summary>
            Rejected,

            /// <summary>
            /// Indicates that an internal error occurred.
            /// </summary>
            ErrorInternal,
 
            /// <summary>
            /// Indicates that the device is not connected to a network.
            /// </summary>
            ErrorNetworkNotConnected,

            /// <summary>
            /// Indicates that the remote endpoint is not connected.
            /// </summary>
            ErrorEndpointNotConnected,

            /// <summary>
            /// Indicates that the endpoints are already connected.
            /// </summary>
            ErrorAlreadyConnected
        }

        private readonly long mLocalClientId;
        private readonly string mRemoteEndpointId;
        private readonly Status mResponseStatus;
        private readonly byte[] mPayload;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionResponse"/> struct.
        /// </summary>
        /// <param name="localClientId">The ID of the local client.</param>
        /// <param name="remoteEndpointId">The ID of the remote endpoint.</param>
        /// <param name="code">The status of the connection response.</param>
        /// <param name="payload">The payload data included with the response.</param>
        private ConnectionResponse(long localClientId, string remoteEndpointId, Status code,
            byte[] payload)
        {
            this.mLocalClientId = localClientId;
            this.mRemoteEndpointId = Misc.CheckNotNull(remoteEndpointId);
            this.mResponseStatus = code;
            this.mPayload = Misc.CheckNotNull(payload);
        }

        /// <summary>
        /// Gets the ID of the local client.
        /// </summary>
        public long LocalClientId
        {
            get { return mLocalClientId; }
        }

        /// <summary>
        /// Gets the ID of the remote endpoint responding to the connection request.
        /// </summary>
        public string RemoteEndpointId
        {
            get { return mRemoteEndpointId; }
        }

        /// <summary>
        /// Gets the status of the connection response.
        /// </summary>
        public Status ResponseStatus
        {
            get { return mResponseStatus; }
        }

        /// <summary>
        /// Gets the payload sent with the connection response.
        /// </summary>
        public byte[] Payload
        {
            get { return mPayload; }
        }

        /// <summary>
        /// Creates a response indicating the connection was rejected.
        /// </summary>
        public static ConnectionResponse Rejected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.Rejected,
                EmptyPayload);
        }

        /// <summary>
        /// Creates a response indicating the device is not connected to a network.
        /// </summary>
        public static ConnectionResponse NetworkNotConnected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorNetworkNotConnected,
                EmptyPayload);
        }

        /// <summary>
        /// Creates a response indicating an internal error occurred.
        /// </summary>
        public static ConnectionResponse InternalError(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorInternal,
                EmptyPayload);
        }

        /// <summary>
        /// Creates a response indicating the remote endpoint is not connected.
        /// </summary>
        public static ConnectionResponse EndpointNotConnected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorEndpointNotConnected,
                EmptyPayload);
        }

        /// <summary>
        /// Creates a response indicating the connection was accepted with a payload.
        /// </summary>
        public static ConnectionResponse Accepted(long localClientId, string remoteEndpointId,
            byte[] payload)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.Accepted,
                payload);
        }

        /// <summary>
        /// Creates a response indicating the endpoints are already connected.
        /// </summary>
        public static ConnectionResponse AlreadyConnected(long localClientId,
            string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId,
                Status.ErrorAlreadyConnected,
                EmptyPayload);
        }
    }
}