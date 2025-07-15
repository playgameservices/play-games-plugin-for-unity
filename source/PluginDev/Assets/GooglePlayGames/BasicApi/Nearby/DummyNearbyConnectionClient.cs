// <copyright file="DummyNearbyConnectionClient.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi.Nearby
{
    using UnityEngine;

    /// <summary>
    /// A dummy implementation of <see cref="INearbyConnectionClient"/>. This implementation
    /// does nothing but log debug messages and return default or error values. It is used
    /// as a fallback when the real Nearby Connections service is unavailable.
    /// </summary>
    /// <remarks>
    /// @deprecated This class will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public class DummyNearbyConnectionClient : INearbyConnectionClient
    {
        /// <summary>
        /// Returns the maximum length of a message payload for unreliable messages.
        /// </summary>
        /// <returns>The maximum unreliable message payload length.</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public int MaxUnreliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxUnreliableMessagePayloadLength;
        }

        /// <summary>
        /// Returns the maximum length of a message payload for reliable messages.
        /// </summary>
        /// <returns>The maximum reliable message payload length.</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public int MaxReliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxReliableMessagePayloadLength;
        }

        /// <summary>
        /// Dummy implementation for sending a reliable message. It only logs a debug message.
        /// </summary>
        /// <param name="recipientEndpointIds">A list of endpoint IDs to which the message should be sent.</param>
        /// <param name="payload">The data to be sent.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void SendReliable(System.Collections.Generic.List<string> recipientEndpointIds, byte[] payload)
        {
            OurUtils.Logger.d("SendReliable called from dummy implementation");
        }

        /// <summary>
        /// Dummy implementation for sending an unreliable message. It only logs a debug message.
        /// </summary>
        /// <param name="recipientEndpointIds">A list of endpoint IDs to which the message should be sent.</param>
        /// <param name="payload">The data to be sent.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void SendUnreliable(System.Collections.Generic.List<string> recipientEndpointIds, byte[] payload)
        {
            OurUtils.Logger.d("SendUnreliable called from dummy implementation");
        }

        /// <summary>
        /// Dummy implementation for starting advertising. It immediately invokes the result callback
        /// with a <see cref="ResponseStatus.LicenseCheckFailed"/> status.
        /// </summary>
        /// <param name="name">The name to advertise for this device. If null, the device name is used.</param>
        /// <param name="appIdentifiers">The identifiers for the app. This is used to distinguish apps from each other.</param>
        /// <param name="advertisingDuration">The duration for which to advertise. If null, advertises indefinitely.</param>
        /// <param name="resultCallback">A callback to be invoked with the result of the advertising attempt.</param>
        /// <param name="connectionRequestCallback">A callback for incoming connection requests.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void StartAdvertising(string name, System.Collections.Generic.List<string> appIdentifiers,
            System.TimeSpan? advertisingDuration, System.Action<AdvertisingResult> resultCallback,
            System.Action<ConnectionRequest> connectionRequestCallback)
        {
            AdvertisingResult obj = new AdvertisingResult(ResponseStatus.LicenseCheckFailed, string.Empty);
            resultCallback.Invoke(obj);
        }

        /// <summary>
        /// Dummy implementation for stopping advertising. It only logs a debug message.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void StopAdvertising()
        {
            OurUtils.Logger.d("StopAvertising in dummy implementation called");
        }

        /// <summary>
        /// Dummy implementation for sending a connection request. It immediately invokes the response
        /// callback with a rejected status.
        /// </summary>
        /// <param name="name">The name of the local player.</param>
        /// <param name="remoteEndpointId">The ID of the endpoint to which the connection request is sent.</param>
        /// <param name="payload">A payload to send with the connection request.</param>
        /// <param name="responseCallback">A callback to receive the response to the connection request.</param>
        /// <param name="listener">A message listener for this connection.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
            System.Action<ConnectionResponse> responseCallback, IMessageListener listener)
        {
            OurUtils.Logger.d("SendConnectionRequest called from dummy implementation");

            if (responseCallback != null)
            {
                ConnectionResponse obj = ConnectionResponse.Rejected(0, string.Empty);
                responseCallback.Invoke(obj);
            }
        }

        /// <summary>
        /// Dummy implementation for accepting a connection request. It only logs a debug message.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the endpoint whose connection request is being accepted.</param>
        /// <param name="payload">A payload to send back to the requesting endpoint.</param>
        /// <param name="listener">A message listener for this connection.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload, IMessageListener listener)
        {
            OurUtils.Logger.d("AcceptConnectionRequest in dummy implementation called");
        }

        /// <summary>
        /// Dummy implementation for starting discovery. It only logs a debug message.
        /// </summary>
        /// <param name="serviceId">The ID of the service to discover.</param>
        /// <param name="advertisingTimeout">The duration for which to discover. If null, discovers indefinitely.</param>
        /// <param name="listener">A listener for discovery events.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void StartDiscovery(string serviceId, System.TimeSpan? advertisingTimeout, IDiscoveryListener listener)
        {
            OurUtils.Logger.d("StartDiscovery in dummy implementation called");
        }

        /// <summary>
        /// Dummy implementation for stopping discovery. It only logs a debug message.
        /// </summary>
        /// <param name="serviceId">The ID of the service for which to stop discovery.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void StopDiscovery(string serviceId)
        {
            OurUtils.Logger.d("StopDiscovery in dummy implementation called");
        }

        /// <summary>
        /// Dummy implementation for rejecting a connection request. It only logs a debug message.
        /// </summary>
        /// <param name="requestingEndpointId">The ID of the endpoint whose connection request is being rejected.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void RejectConnectionRequest(string requestingEndpointId)
        {
            OurUtils.Logger.d("RejectConnectionRequest in dummy implementation called");
        }

        /// <summary>
        /// Dummy implementation for disconnecting from an endpoint. It only logs a debug message.
        /// </summary>
        /// <param name="remoteEndpointId">The ID of the remote endpoint from which to disconnect.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            OurUtils.Logger.d("DisconnectFromEndpoint in dummy implementation called");
        }

        /// <summary>
        /// Dummy implementation for stopping all connections. It only logs a debug message.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public void StopAllConnections()
        {
            OurUtils.Logger.d("StopAllConnections in dummy implementation called");
        }

        /// <summary>
        /// Returns a dummy local endpoint ID.
        /// </summary>
        /// <returns>An empty string.</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string LocalEndpointId()
        {
            return string.Empty;
        }

        /// <summary>
        /// Returns a dummy local device ID.
        /// </summary>
        /// <returns>The string "DummyDevice".</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string LocalDeviceId()
        {
            return "DummyDevice";
        }

        /// <summary>
        /// Returns a dummy app bundle ID.
        /// </summary>
        /// <returns>The string "dummy.bundle.id".</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string GetAppBundleId()
        {
            return "dummy.bundle.id";
        }

        /// <summary>
        /// Returns a dummy service ID.
        /// </summary>
        /// <returns>The string "dummy.service.id".</returns>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string GetServiceId()
        {
            return "dummy.service.id";
        }
    }
}
#endif