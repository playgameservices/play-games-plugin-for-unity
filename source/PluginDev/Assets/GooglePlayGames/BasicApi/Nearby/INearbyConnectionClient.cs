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

#if UNITY_ANDROID
    /// <summary>
    /// Defines the interface for a client that handles Nearby Connections operations.
    /// </summary>
    /// <remarks>
    /// @deprecated This interface will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public interface INearbyConnectionClient
    {
        /// <summary>
        /// Gets the maximum size of a message payload for unreliable messages.
        /// </summary>
        /// <returns>The maximum unreliable message payload length in bytes.</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        int MaxUnreliableMessagePayloadLength();

        /// <summary>
        /// Gets the maximum size of a message payload for reliable messages.
        /// </summary>
        /// <returns>The maximum reliable message payload length in bytes.</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        int MaxReliableMessagePayloadLength();

        /// <summary>
        /// Sends a reliable message to a list of recipients.
        /// </summary>
        /// <param name="recipientEndpointIds">A list of endpoint IDs to which the message should be sent.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        /// <param name="payload">The data to be sent.</param>
        void SendReliable(List<string> recipientEndpointIds, byte[] payload);

        /// <summary>
        /// Sends an unreliable message to a list of recipients.
        /// </summary>
        /// <param name="recipientEndpointIds">A list of endpoint IDs to which the message should be sent.</param>
        /// <param name="payload">The data to be sent.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void SendUnreliable(List<string> recipientEndpointIds, byte[] payload);

        /// <summary>
        /// Starts advertising the local device so other devices can discover and connect to it.
        /// </summary>
        /// <param name="name">The name to advertise for this device. If null, the device name is used.</param>
        /// <param name="appIdentifiers">The identifiers for the app, used to distinguish it from others.</param>
        /// <param name="advertisingDuration">The duration for which to advertise. If null, advertises indefinitely.</param>
        /// <param name="resultCallback">A callback invoked with the result of the advertising attempt.</param>
        /// <param name="connectionRequestCallback">A callback for handling incoming connection requests.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void StartAdvertising(string name, List<string> appIdentifiers,
            TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
            Action<ConnectionRequest> connectionRequestCallback);

        /// <summary>
        /// Stops advertising the local device.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void StopAdvertising();

        /// <summary>
        /// Sends a connection request to a remote endpoint.
        /// </summary>
        /// <param name="name">The name of the local player.</param>
        /// <param name="remoteEndpointId">The ID of the endpoint to which the connection request is sent.</param>
        /// <param name="payload">A payload to send with the connection request.</param>
        /// <param name="responseCallback">A callback to receive the response to the connection request.</param>
        /// <param name="listener">A message listener for this connection.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
            Action<ConnectionResponse> responseCallback, IMessageListener listener);

        /// <summary>
        /// Accepts a connection request from a remote endpoint.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the endpoint whose connection request is being accepted.</param>
        /// <param name="payload">A payload to send back to the requesting endpoint.</param>
        /// <param name="listener">A message listener for this connection.</param>
        void AcceptConnectionRequest(string remoteEndpointId, byte[] payload,
            IMessageListener listener);

        /// <summary>
        /// Starts discovering remote endpoints that are advertising for the same service ID.
        /// </summary>
        /// <param name="serviceId">The ID of the service to discover.</param>
        /// <param name="advertisingTimeout">The duration for which to discover. If null, discovers indefinitely.</param>
        /// <param name="listener">A listener for discovery events.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void StartDiscovery(string serviceId, TimeSpan? advertisingTimeout,
            IDiscoveryListener listener);

        /// <summary>
        /// Stops discovering remote endpoints.
        /// </summary>
        /// <param name="serviceId">The ID of the service for which to stop discovery.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void StopDiscovery(string serviceId);

        /// <summary>
        /// Rejects a connection request from a remote endpoint.
        /// </summary>
        /// <param name="requestingEndpointId">The ID of the endpoint whose connection request is being rejected.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void RejectConnectionRequest(string requestingEndpointId);

        /// <summary>
        /// Disconnects from a remote endpoint.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the remote endpoint from which to disconnect.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void DisconnectFromEndpoint(string remoteEndpointId);

        /// <summary>
        /// Disconnects from all connected endpoints and stops all advertising and discovery.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void StopAllConnections();

        /// <summary>
        /// Gets the application's bundle ID.
        /// </summary>
        /// <returns>The app bundle ID.</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        string GetAppBundleId();

        /// <summary>
        /// Gets the service ID used for discovery.
        /// </summary>
        /// <returns>The service ID.</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        string GetServiceId();
    }
#endif

    /// <summary>
    /// A listener for receiving messages and disconnection events from a remote endpoint.
    /// </summary>
    /// <remarks>
    /// @deprecated This interface will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public interface IMessageListener
    {
        /// <summary>
        /// Called when a message is received from a remote endpoint.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the endpoint that sent the message.</param>
        /// <param name="data">The message data.</param>
        /// <param name="isReliableMessage">True if the message was sent reliably, false otherwise.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void OnMessageReceived(string remoteEndpointId, byte[] data,
            bool isReliableMessage);

        /// <summary>
        /// Called when a remote endpoint is disconnected.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the disconnected endpoint.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void OnRemoteEndpointDisconnected(string remoteEndpointId);
    }

    /// <summary>
    /// A listener for discovering remote endpoints.
    /// </summary>
    /// <remarks>
    /// @deprecated This interface will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public interface IDiscoveryListener
    {
        /// <summary>
        /// Called when a remote endpoint is found.
        /// </summary>
        /// <param name="discoveredEndpoint">The details of the discovered endpoint.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void OnEndpointFound(EndpointDetails discoveredEndpoint);

        /// <summary>
        /// Called when a previously discovered endpoint is no longer available.
        /// </summary>
        /// <param name="lostEndpointId">The ID of the lost endpoint.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void OnEndpointLost(string lostEndpointId);
    }
}