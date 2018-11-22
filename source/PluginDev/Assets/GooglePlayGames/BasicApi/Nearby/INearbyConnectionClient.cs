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

    public interface INearbyConnectionClient
    {

        int MaxUnreliableMessagePayloadLength();

        int MaxReliableMessagePayloadLength();

        void SendReliable(List<string> recipientEndpointIds, byte[] payload);

        void SendUnreliable(List<string> recipientEndpointIds, byte[] payload);

        void StartAdvertising(string name, List<string> appIdentifiers,
                              TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
                              Action<ConnectionRequest> connectionRequestCallback);

        void StopAdvertising();

        void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
                                   Action<ConnectionResponse> responseCallback, IMessageListener listener);

        void AcceptConnectionRequest(string remoteEndpointId, byte[] payload,
                                     IMessageListener listener);

        void StartDiscovery(string serviceId, TimeSpan? advertisingTimeout,
                            IDiscoveryListener listener);

        void StopDiscovery(string serviceId);

        void RejectConnectionRequest(string requestingEndpointId);

        void DisconnectFromEndpoint(string remoteEndpointId);

        void StopAllConnections();

        string GetAppBundleId();

        string GetServiceId();
    }
#endif

    public interface IMessageListener
    {
        void OnMessageReceived(string remoteEndpointId, byte[] data,
                       bool isReliableMessage);

        void OnRemoteEndpointDisconnected(string remoteEndpointId);
    }

    public interface IDiscoveryListener
    {
        void OnEndpointFound(EndpointDetails discoveredEndpoint);

        void OnEndpointLost(string lostEndpointId);
    }
}

