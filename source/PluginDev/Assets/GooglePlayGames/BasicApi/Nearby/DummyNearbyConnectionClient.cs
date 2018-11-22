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

    public class DummyNearbyConnectionClient : INearbyConnectionClient
    {


        public int MaxUnreliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxUnreliableMessagePayloadLength;
        }

        public int MaxReliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxReliableMessagePayloadLength;
        }

        public void SendReliable(System.Collections.Generic.List<string> recipientEndpointIds, byte[] payload)
        {
            Debug.LogError("SendReliable called from dummy implementation");
        }

        public void SendUnreliable(System.Collections.Generic.List<string> recipientEndpointIds, byte[] payload)
        {
            Debug.LogError("SendUnreliable called from dummy implementation");
        }

        public void StartAdvertising(string name, System.Collections.Generic.List<string> appIdentifiers, System.TimeSpan? advertisingDuration, System.Action<AdvertisingResult> resultCallback, System.Action<ConnectionRequest> connectionRequestCallback)
        {
            AdvertisingResult obj = new AdvertisingResult(ResponseStatus.LicenseCheckFailed, string.Empty);
            resultCallback.Invoke(obj);
        }

        public void StopAdvertising()
        {
            Debug.LogError("StopAvertising in dummy implementation called");
        }

        public void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload, System.Action<ConnectionResponse> responseCallback, IMessageListener listener)
        {
            Debug.LogError("SendConnectionRequest called from dummy implementation");

            if (responseCallback != null)
            {
                ConnectionResponse obj = ConnectionResponse.Rejected(0, string.Empty);
                responseCallback.Invoke(obj);
            }
        }

        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload, IMessageListener listener)
        {
            Debug.LogError("AcceptConnectionRequest in dummy implementation called");
        }

        public void StartDiscovery(string serviceId, System.TimeSpan? advertisingTimeout, IDiscoveryListener listener)
        {
            Debug.LogError("StartDiscovery in dummy implementation called");
        }

        public void StopDiscovery(string serviceId)
        {
            Debug.LogError("StopDiscovery in dummy implementation called");
        }

        public void RejectConnectionRequest(string requestingEndpointId)
        {
            Debug.LogError("RejectConnectionRequest in dummy implementation called");
        }

        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            Debug.LogError("DisconnectFromEndpoint in dummy implementation called");
        }

        public void StopAllConnections()
        {
            Debug.LogError("StopAllConnections in dummy implementation called");
        }

        public string LocalEndpointId()
        {
            return string.Empty;
        }

        public string LocalDeviceId()
        {
            return "DummyDevice";
        }

        public string GetAppBundleId()
        {
            return "dummy.bundle.id";
        }

        public string GetServiceId()
        {
            return "dummy.service.id";
        }
    }
}
#endif
