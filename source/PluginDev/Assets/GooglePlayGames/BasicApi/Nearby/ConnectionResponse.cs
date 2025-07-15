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
    /// Represents the response to a connection request in the Nearby API.
    /// </summary>
    /// <remarks>
    /// @deprecated This struct will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public struct ConnectionResponse
    {
        private static readonly byte[] EmptyPayload = new byte[0];

        /// <summary>
        /// Represents the status of a connection response.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public enum Status
        {
            /// <summary>
            /// The connection request was accepted.
            /// </summary>
            /// <remarks>
            /// @deprecated This enum value will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            Accepted,

            /// <summary>
            /// The connection request was rejected.
            /// </summary>
            /// <remarks>
            /// @deprecated This enum value will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            Rejected,

            /// <summary>
            /// An internal error occurred.
            /// </summary>
            /// <remarks>
            /// @deprecated This enum value will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            ErrorInternal,

            /// <summary>
            /// The network is not connected.
            /// </summary>
            /// <remarks>
            /// @deprecated This enum value will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            ErrorNetworkNotConnected,

            /// <summary>
            /// The endpoint is not connected.
            /// </summary>
            /// <remarks>
            /// @deprecated This enum value will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            ErrorEndpointNotConnected,

            /// <summary>
            /// The endpoints are already connected.
            /// </summary>
            /// <remarks>
            /// @deprecated This enum value will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            ErrorAlreadyConnected
        }

        private readonly long mLocalClientId;
        private readonly string mRemoteEndpointId;
        private readonly Status mResponseStatus;
        private readonly byte[] mPayload;

        /// <summary>
        /// Initializes a new instance of the ConnectionResponse struct.
        /// </summary>
        /// <param name="localClientId">The local client ID.</param>
        /// <param name="remoteEndpointId">The ID of the remote endpoint.</param>
        /// <param name="code">The status code of the response.</param>
        /// <param name="payload">The payload data associated with the response.</param>
        /// <remarks>
        /// @deprecated This constructor will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        private ConnectionResponse(long localClientId, string remoteEndpointId, Status code,
            byte[] payload)
        {
            this.mLocalClientId = localClientId;
            this.mRemoteEndpointId = Misc.CheckNotNull(remoteEndpointId);
            this.mResponseStatus = code;
            this.mPayload = Misc.CheckNotNull(payload);
        }

        /// <summary>
        /// Gets the local client ID.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public long LocalClientId
        {
            get { return mLocalClientId; }
        }

        /// <summary>
        /// Gets the remote endpoint ID.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string RemoteEndpointId
        {
            get { return mRemoteEndpointId; }
        }

        /// <summary>
        /// Gets the response status of the connection.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public Status ResponseStatus
        {
            get { return mResponseStatus; }
        }

        /// <summary>
        /// Gets the payload data associated with the connection response.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public byte[] Payload
        {
            get { return mPayload; }
        }

        /// <summary>
        /// Returns a connection response indicating rejection.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public static ConnectionResponse Rejected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.Rejected,
                EmptyPayload);
        }

        /// <summary>
        /// Returns a connection response indicating that the network is not connected.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public static ConnectionResponse NetworkNotConnected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorNetworkNotConnected,
                EmptyPayload);
        }

        /// <summary>
        /// Returns a connection response indicating an internal error.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public static ConnectionResponse InternalError(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorInternal,
                EmptyPayload);
        }

        /// <summary>
        /// Returns a connection response indicating that the endpoint is not connected.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public static ConnectionResponse EndpointNotConnected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorEndpointNotConnected,
                EmptyPayload);
        }

        /// <summary>
        /// Returns a connection response indicating that the connection was accepted.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public static ConnectionResponse Accepted(long localClientId, string remoteEndpointId,
            byte[] payload)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.Accepted,
                payload);
        }

        /// <summary>
        /// Returns a connection response indicating that the endpoints are already connected.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public static ConnectionResponse AlreadyConnected(long localClientId,
            string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId,
                Status.ErrorAlreadyConnected,
                EmptyPayload);
        }
    }
}