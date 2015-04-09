// <copyright file="NativeNearbyConnectionsClient.cs" company="Google Inc.">
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

// Android only feature
#if (UNITY_ANDROID)

namespace GooglePlayGames.Native
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Nearby;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.Native.PInvoke;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal class NativeNearbyConnectionsClient : INearbyConnectionClient
    {

        private readonly NearbyConnectionsManager mManager;

        internal NativeNearbyConnectionsClient(NearbyConnectionsManager manager)
        {
            this.mManager = Misc.CheckNotNull(manager);
        }

        public int MaxUnreliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxUnreliableMessagePayloadLength;
        }

        public int MaxReliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxReliableMessagePayloadLength;
        }

        public void SendReliable(List<string> recipientEndpointIds, byte[] payload)
        {
            InternalSend(recipientEndpointIds, payload, true);
        }

        public void SendUnreliable(List<string> recipientEndpointIds, byte[] payload)
        {
            InternalSend(recipientEndpointIds, payload, false);
        }

        private void InternalSend(List<string> recipientEndpointIds, byte[] payload, bool isReliable)
        {
            if (recipientEndpointIds == null)
            {
                throw new ArgumentNullException("recipientEndpointIds");
            }

            if (payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            if (recipientEndpointIds.Contains(null))
            {
                throw new InvalidOperationException("Cannot send a message to a null recipient");
            }

            if (recipientEndpointIds.Count == 0)
            {
                Logger.w("Attempted to send a reliable message with no recipients");
                return;
            }

            if (isReliable)
            {
                if (payload.Length > MaxReliableMessagePayloadLength())
                {
                    throw new InvalidOperationException("cannot send more than "
                        + MaxReliableMessagePayloadLength() + " bytes");
                }
            }
            else
            {
                if (payload.Length > MaxUnreliableMessagePayloadLength())
                {
                    throw new InvalidOperationException("cannot send more than "
                        + MaxUnreliableMessagePayloadLength() + " bytes");
                }
            }

            foreach (var recipient in recipientEndpointIds)
            {
                if (isReliable)
                {
                    mManager.SendReliable(recipient, payload);
                }
                else
                {
                    mManager.SendUnreliable(recipient, payload);
                }
            }
        }

        public void StartAdvertising(string name, List<string> appIdentifiers,
                                     TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
                                     Action<ConnectionRequest> requestCallback)
        {
            Misc.CheckNotNull(appIdentifiers, "appIdentifiers");
            Misc.CheckNotNull(resultCallback, "resultCallback");
            Misc.CheckNotNull(requestCallback, "connectionRequestCallback");

            if (advertisingDuration.HasValue && advertisingDuration.Value.Ticks < 0)
            {
                throw new InvalidOperationException("advertisingDuration must be positive");
            }

            resultCallback = Callbacks.AsOnGameThreadCallback<AdvertisingResult>(resultCallback);
            requestCallback = Callbacks.AsOnGameThreadCallback<ConnectionRequest>(requestCallback);

            mManager.StartAdvertising(
                name,
                appIdentifiers.Select<string, NativeAppIdentifier>(NativeAppIdentifier.FromString)
            .ToList(),
                ToTimeoutMillis(advertisingDuration),
                (localClientId, result) => resultCallback(result.AsResult()),
                (localClientId, request) => requestCallback(request.AsRequest()));
        }

        private static long ToTimeoutMillis(TimeSpan? span)
        {
            // If the duration is present, use its milliseconds. Otherwise use "0" indicating no
            // timeout.
            return span.HasValue ? PInvokeUtilities.ToMilliseconds(span.Value) : 0L;
        }

        public void StopAdvertising()
        {
            mManager.StopAdvertising();
        }

        public void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
                                          Action<ConnectionResponse> responseCallback, IMessageListener listener)
        {
            Misc.CheckNotNull(remoteEndpointId, "remoteEndpointId");
            Misc.CheckNotNull(payload, "payload");
            Misc.CheckNotNull(responseCallback, "responseCallback");
            Misc.CheckNotNull(listener, "listener");

            responseCallback = Callbacks.AsOnGameThreadCallback(responseCallback);

            using (var nativeListener = ToMessageListener(listener))
            {
                mManager.SendConnectionRequest(
                    name,
                    remoteEndpointId,
                    payload,
                    (localClientId, response) => responseCallback(response.AsResponse(localClientId)),
                    nativeListener);
            }
        }

        private static NativeMessageListenerHelper ToMessageListener(IMessageListener listener)
        {
            listener = new OnGameThreadMessageListener(listener);

            var helper = new NativeMessageListenerHelper();
            helper.SetOnMessageReceivedCallback(
                (localClientId, endpointId, data, isReliable) => listener.OnMessageReceived(
                    endpointId, data, isReliable));
            helper.SetOnDisconnectedCallback((localClientId, endpointId) =>
            listener.OnRemoteEndpointDisconnected(endpointId));

            return helper;
        }

        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload,
                                            IMessageListener listener)
        {
            Misc.CheckNotNull(remoteEndpointId, "remoteEndpointId");
            Misc.CheckNotNull(payload, "payload");
            Misc.CheckNotNull(listener, "listener");

            Logger.d("Calling AcceptConncectionRequest");
            mManager.AcceptConnectionRequest(remoteEndpointId, payload, ToMessageListener(listener));
            Logger.d("Called!");
        }

        public void StartDiscovery(string serviceId, TimeSpan? advertisingTimeout,
                                   IDiscoveryListener listener)
        {
            Misc.CheckNotNull(serviceId, "serviceId");
            Misc.CheckNotNull(listener, "listener");

            using (var nativeListener = ToDiscoveryListener(listener))
            {
                mManager.StartDiscovery(serviceId, ToTimeoutMillis(advertisingTimeout),
                    nativeListener);
            }
        }

        private static NativeEndpointDiscoveryListenerHelper ToDiscoveryListener(
            IDiscoveryListener listener)
        {
            listener = new OnGameThreadDiscoveryListener(listener);
            var helper = new NativeEndpointDiscoveryListenerHelper();

            helper.SetOnEndpointFound((localClientId, endpoint) =>
            listener.OnEndpointFound(endpoint.ToDetails()));
            helper.SetOnEndpointLostCallback((localClientId, lostEndpointId) =>
            listener.OnEndpointLost(lostEndpointId));

            return helper;
        }

        public void StopDiscovery(string serviceId)
        {
            Misc.CheckNotNull(serviceId, "serviceId");
            mManager.StopDiscovery(serviceId);
        }

        public void RejectConnectionRequest(string requestingEndpointId)
        {
            Misc.CheckNotNull(requestingEndpointId, "requestingEndpointId");
            mManager.RejectConnectionRequest(requestingEndpointId);
        }

        
        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            mManager.DisconnectFromEndpoint(remoteEndpointId);
        }

        public void StopAllConnections()
        {
            mManager.StopAllConnections();
        }

        public string LocalEndpointId()
        {
            return mManager.LocalEndpointId();
        }

        public string LocalDeviceId()
        {
            return mManager.LocalDeviceId();
        }

        public string GetAppBundleId()
        {
            return mManager.AppBundleId;
        }

        public string GetServiceId()
        {
            return NearbyConnectionsManager.ServiceId;
        }

        protected class OnGameThreadMessageListener : IMessageListener
        {

            private readonly IMessageListener mListener;

            public OnGameThreadMessageListener(IMessageListener listener)
            {
                this.mListener = Misc.CheckNotNull(listener);
            }

            public void OnMessageReceived(string remoteEndpointId, byte[] data,
                                          bool isReliableMessage)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnMessageReceived(
                        remoteEndpointId, data, isReliableMessage));
            }

            public void OnRemoteEndpointDisconnected(string remoteEndpointId)
            {
                PlayGamesHelperObject.RunOnGameThread(
                    () => mListener.OnRemoteEndpointDisconnected(remoteEndpointId));
            }
        }

        protected class OnGameThreadDiscoveryListener : IDiscoveryListener
        {
            private readonly IDiscoveryListener mListener;

            public OnGameThreadDiscoveryListener(IDiscoveryListener listener)
            {
                this.mListener = Misc.CheckNotNull(listener);
            }

            public void OnEndpointFound(EndpointDetails discoveredEndpoint)
            {
                PlayGamesHelperObject.RunOnGameThread(
                    () => mListener.OnEndpointFound(discoveredEndpoint));
            }

            public void OnEndpointLost(string lostEndpointId)
            {
                PlayGamesHelperObject.RunOnGameThread(
                    () => mListener.OnEndpointLost(lostEndpointId));
            }

        }
    }
}

#endif // #if (UNITY_ANDROID || UNITY_IPHONE)
