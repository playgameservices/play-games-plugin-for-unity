// <copyright file="DummyNearbyConnectionClient.cs" company="Google Inc.">
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

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi.Nearby
{
    using UnityEngine;

    /// <summary>
    /// Dummy implementation of INearbyConnectionClient. This class can be used for testing purposes.
    /// It logs messages indicating that its methods have been called.
    /// </summary>
    public class DummyNearbyConnectionClient : INearbyConnectionClient
    {
        /// <summary>
        /// The maximum size of an unreliable message payload.
        /// </summary>
        public int MaxUnreliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxUnreliableMessagePayloadLength;
        }

        /// <summary>
        /// The maximum size of a reliable message payload.
        /// </summary>
        public int MaxReliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxReliableMessagePayloadLength;
        }

        /// <summary>
        /// Logs the message about Reliable call from dummy implementation.
        /// </summary>
        public void SendReliable(System.Collections.Generic.List<string> recipientEndpointIds, byte[] payload)
        {
            OurUtils.Logger.d("SendReliable called from dummy implementation");
        }

        /// <summary>
        /// Logs the message about Unreliable call from dummy implementation.
        /// </summary>
        public void SendUnreliable(System.Collections.Generic.List<string> recipientEndpointIds, byte[] payload)
        {
            OurUtils.Logger.d("SendUnreliable called from dummy implementation");
        }

        /// <summary>
        /// Starts advertising for a service.
        /// </summary>
        public void StartAdvertising(string name, System.Collections.Generic.List<string> appIdentifiers,
            System.TimeSpan? advertisingDuration, System.Action<AdvertisingResult> resultCallback,
            System.Action<ConnectionRequest> connectionRequestCallback)
        {
            AdvertisingResult obj = new AdvertisingResult(ResponseStatus.LicenseCheckFailed, string.Empty);
            resultCallback.Invoke(obj);
        }

        /// <summary>
        /// Logs the message about StopAdvertising call from dummy implementation.
        /// </summary>
        public void StopAdvertising()
        {
            OurUtils.Logger.d("StopAvertising in dummy implementation called");
        }

        /// <summary>
        /// Sends a connection request to the specified endpoint.
        /// </summary>
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
        /// Logs the message about accepts a connection request from the specified endpoint.
        /// </summary>
        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload, IMessageListener listener)
        {
            OurUtils.Logger.d("AcceptConnectionRequest in dummy implementation called");
        }

        /// <summary>
        /// Logs the message about StartDiscovery call from dummy implementation.
        /// </summary>
        public void StartDiscovery(string serviceId, System.TimeSpan? advertisingTimeout, IDiscoveryListener listener)
        {
            OurUtils.Logger.d("StartDiscovery in dummy implementation called");
        }

        /// <summary>
        /// Logs the message about StopDiscovery call from dummy implementation.
        /// </summary>
        public void StopDiscovery(string serviceId)
        {
            OurUtils.Logger.d("StopDiscovery in dummy implementation called");
        }

        /// <summary>
        /// Logs the message about RejectConnectionRequest call from dummy implementation.
        /// </summary>
        public void RejectConnectionRequest(string requestingEndpointId)
        {
            OurUtils.Logger.d("RejectConnectionRequest in dummy implementation called");
        }

        /// <summary>
        /// Logs the message about DisconnectFromEndpoint call from dummy implementation.
        /// </summary>
        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            OurUtils.Logger.d("DisconnectFromEndpoint in dummy implementation called");
        }

        /// <summary>
        /// Logs the message about StopAllConnections call from dummy implementation.
        /// </summary>
        public void StopAllConnections()
        {
            OurUtils.Logger.d("StopAllConnections in dummy implementation called");
        }

        /// <summary>
        /// Returns the local endpoint id string.
        /// </summary>
        public string LocalEndpointId()
        {
            return string.Empty;
        }

        /// <summary>
        /// Returns the local device id string.
        /// </summary>
        public string LocalDeviceId()
        {
            return "DummyDevice";
        }

        /// <summary>
        /// Returns the app bundle id string.
        /// </summary>
        public string GetAppBundleId()
        {
            return "dummy.bundle.id";
        }

        /// <summary>
        /// Returns the service id string.
        /// </summary>
        public string GetServiceId()
        {
            return "dummy.service.id";
        }
    }
}
#endif