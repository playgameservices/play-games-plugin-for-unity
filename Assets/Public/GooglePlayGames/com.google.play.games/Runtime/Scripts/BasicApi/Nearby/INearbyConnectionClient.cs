// <copyright file="INearbyConnectionClient.cs" company="Google Inc.">
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

    // move this inside IMessageListener and IDiscoveryListener are always declared.
#if UNITY_ANDROID

    /// <summary>
    /// Interface for managing connections and communications between devices using Nearby Connections.
    /// </summary>
    public interface INearbyConnectionClient
    {
        /// <summary>
        /// Gets the maximum length of an unreliable message payload.
        /// </summary>
        /// <returns>Maximum length of an unreliable message payload.</returns>
        int MaxUnreliableMessagePayloadLength();

        /// <summary>
        /// Gets the maximum length of a reliable message payload.
        /// </summary>
        /// <returns>Maximum length of a reliable message payload.</returns>
        int MaxReliableMessagePayloadLength();

        /// <summary>
        /// Sends a reliable message to a list of recipients.
        /// </summary>
        /// <param name="recipientEndpointIds">List of recipient endpoint IDs.</param>
        /// <param name="payload">The message payload to send.</param>
        void SendReliable(List<string> recipientEndpointIds, byte[] payload);

        /// <summary>
        /// Sends an unreliable message to a list of recipients.
        /// </summary>
        /// <param name="recipientEndpointIds">List of recipient endpoint IDs.</param>
        /// <param name="payload">The message payload to send.</param>
        void SendUnreliable(List<string> recipientEndpointIds, byte[] payload);

        /// <summary>
        /// Starts advertising the local device to nearby devices.
        /// </summary>
        /// <param name="name">The name to advertise.</param>
        /// <param name="appIdentifiers">List of application identifiers.</param>
        /// <param name="advertisingDuration">Optional advertising duration.</param>
        /// <param name="resultCallback">Callback for advertising result.</param>
        /// <param name="connectionRequestCallback">Callback for incoming connection requests.</param>
        void StartAdvertising(string name, List<string> appIdentifiers,
            TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
            Action<ConnectionRequest> connectionRequestCallback);

        /// <summary>
        /// Stops advertising the local device to nearby devices.
        /// </summary>
        void StopAdvertising();

        /// <summary>
        /// Sends a connection request to a remote endpoint.
        /// </summary>
        /// <param name="name">The name of the local device.</param>
        /// <param name="remoteEndpointId">The ID of the remote endpoint.</param>
        /// <param name="payload">The connection request payload.</param>
        /// <param name="responseCallback">Callback for the connection response.</param>
        /// <param name="listener">Listener for message events.</param>
        void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
            Action<ConnectionResponse> responseCallback, IMessageListener listener);

        /// <summary>
        /// Accepts a connection request from a remote endpoint.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the remote endpoint.</param>
        /// <param name="payload">The connection acceptance payload.</param>
        /// <param name="listener">Listener for message events.</param>
        void AcceptConnectionRequest(string remoteEndpointId, byte[] payload,
            IMessageListener listener);

        /// <summary>
        /// Starts discovering nearby endpoints for a specific service.
        /// </summary>
        /// <param name="serviceId">The service ID to discover.</param>
        /// <param name="advertisingTimeout">Optional timeout for advertising discovery.</param>
        /// <param name="listener">Listener for discovery events.</param>
        void StartDiscovery(string serviceId, TimeSpan? advertisingTimeout,
            IDiscoveryListener listener);

        /// <summary>
        /// Stops discovering endpoints for a specific service.
        /// </summary>
        /// <param name="serviceId">The service ID to stop discovering.</param>
        void StopDiscovery(string serviceId);

        /// <summary>
        /// Rejects a connection request from a remote endpoint.
        /// </summary>
        /// <param name="requestingEndpointId">The ID of the endpoint that sent the request.</param>
        void RejectConnectionRequest(string requestingEndpointId);

        /// <summary>
        /// Disconnects from a remote endpoint.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the remote endpoint to disconnect from.</param>
        void DisconnectFromEndpoint(string remoteEndpointId);

        /// <summary>
        /// Stops all connections to nearby endpoints.
        /// </summary>
        void StopAllConnections();

        /// <summary>
        /// Gets the app bundle ID.
        /// </summary>
        /// <returns>The app bundle ID.</returns>
        string GetAppBundleId();

        /// <summary>
        /// Gets the service ID used for discovery and connection.
        /// </summary>
        /// <returns>The service ID.</returns>
        string GetServiceId();
    }
#endif

    /// <summary>
    /// Interface for receiving messages and notifications about remote endpoints.
    /// </summary>
    public interface IMessageListener
    {
        /// <summary>
        /// Called when a message is received from a remote endpoint.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the remote endpoint.</param>
        /// <param name="data">The data of the received message.</param>
        /// <param name="isReliableMessage">Indicates whether the message is reliable.</param>
        void OnMessageReceived(string remoteEndpointId, byte[] data,
            bool isReliableMessage);

        /// <summary>
        /// Called when a remote endpoint has disconnected.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the disconnected endpoint.</param>
        void OnRemoteEndpointDisconnected(string remoteEndpointId);
    }

    /// <summary>
    /// Interface for receiving notifications about discovered endpoints.
    /// </summary>
    public interface IDiscoveryListener
    {
        /// <summary>
        /// Called when an endpoint is found during discovery.
        /// </summary>
        /// <param name="discoveredEndpoint">The details of the discovered endpoint.</param>
        void OnEndpointFound(EndpointDetails discoveredEndpoint);

        /// <summary>
        /// Called when an endpoint is lost during discovery.
        /// </summary>
        /// <param name="lostEndpointId">The ID of the lost endpoint.</param>
        void OnEndpointLost(string lostEndpointId);
    }
}
