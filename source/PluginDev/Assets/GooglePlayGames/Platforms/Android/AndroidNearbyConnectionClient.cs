#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Nearby;
    using GooglePlayGames.OurUtils;
    using UnityEngine;

    public class AndroidNearbyConnectionClient : INearbyConnectionClient
    {
        private volatile AndroidJavaObject mClient;
        private readonly static long NearbyClientId = 0L;
        private readonly static int ApplicationInfoFlags = 0x00000080;
        private readonly static string ServiceId = ReadServiceId();
        protected IMessageListener mAdvertisingMessageListener;

        public AndroidNearbyConnectionClient()
        {
            PlayGamesHelperObject.CreateObject();
            NearbyHelperObject.CreateObject(this);
            using (var nearbyClass = new AndroidJavaClass("com.google.android.gms.nearby.Nearby"))
            {
                mClient = nearbyClass.CallStatic<AndroidJavaObject>("getConnectionsClient",
                    AndroidHelperFragment.GetActivity());
            }
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
            InternalSend(recipientEndpointIds, payload);
        }

        public void SendUnreliable(List<string> recipientEndpointIds, byte[] payload)
        {
            InternalSend(recipientEndpointIds, payload);
        }

        private void InternalSend(List<string> recipientEndpointIds, byte[] payload)
        {
            Misc.CheckNotNull(recipientEndpointIds);
            Misc.CheckNotNull(payload);

            using (var payloadClass = new AndroidJavaClass("com.google.android.gms.nearby.connection.Payload"))
            using (var payloadObject = payloadClass.CallStatic<AndroidJavaObject>("fromBytes", payload))
            using (var task = mClient.Call<AndroidJavaObject>("sendPayload",
                AndroidJavaConverter.ToJavaStringList(recipientEndpointIds),
                payloadObject))
                ;
        }

        public void StartAdvertising(string name, List<string> appIdentifiers,
            TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
            Action<ConnectionRequest> connectionRequestCallback)
        {
            Misc.CheckNotNull(resultCallback, "resultCallback");
            Misc.CheckNotNull(connectionRequestCallback, "connectionRequestCallback");

            if (advertisingDuration.HasValue && advertisingDuration.Value.Ticks < 0)
            {
                throw new InvalidOperationException("advertisingDuration must be positive");
            }

            connectionRequestCallback = ToOnGameThread(connectionRequestCallback);
            resultCallback = ToOnGameThread(resultCallback);

            AdvertisingConnectionLifecycleCallbackProxy callbackProxy =
                new AdvertisingConnectionLifecycleCallbackProxy(resultCallback, connectionRequestCallback, this);
            using (var connectionLifecycleCallback =
                new AndroidJavaObject("com.google.games.bridge.ConnectionLifecycleCallbackProxy", callbackProxy))
            using (var advertisingOptions = CreateAdvertisingOptions())
            using (var task = mClient.Call<AndroidJavaObject>("startAdvertising", name, GetServiceId(),
                connectionLifecycleCallback, advertisingOptions))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    v => NearbyHelperObject.StartAdvertisingTimer(advertisingDuration)
                );
            }
        }

        private AndroidJavaObject CreateAdvertisingOptions()
        {
            using (var strategy = new AndroidJavaClass("com.google.android.gms.nearby.connection.Strategy")
                .GetStatic<AndroidJavaObject>("P2P_CLUSTER"))
            using (var builder =
                new AndroidJavaObject("com.google.android.gms.nearby.connection.AdvertisingOptions$Builder"))
            using (builder.Call<AndroidJavaObject>("setStrategy", strategy))
            {
                return builder.Call<AndroidJavaObject>("build");
            }
        }

        private class AdvertisingConnectionLifecycleCallbackProxy : AndroidJavaProxy
        {
            private Action<AdvertisingResult> mResultCallback;
            private Action<ConnectionRequest> mConnectionRequestCallback;
            private AndroidNearbyConnectionClient mClient;
            private string mLocalEndpointName;

            public AdvertisingConnectionLifecycleCallbackProxy(Action<AdvertisingResult> resultCallback,
                Action<ConnectionRequest> connectionRequestCallback, AndroidNearbyConnectionClient client) : base(
                "com/google/games/bridge/ConnectionLifecycleCallbackProxy$Callback")
            {
                mResultCallback = resultCallback;
                mConnectionRequestCallback = connectionRequestCallback;
                mClient = client;
            }

            public void onConnectionInitiated(string endpointId, AndroidJavaObject connectionInfo)
            {
                mLocalEndpointName = connectionInfo.Call<string>("getEndpointName");
                mConnectionRequestCallback(new ConnectionRequest(endpointId, mLocalEndpointName, mClient.GetServiceId(),
                    new byte[0]));
            }

            public void onConnectionResult(string endpointId, AndroidJavaObject connectionResolution)
            {
                int statusCode;
                using (var status = connectionResolution.Call<AndroidJavaObject>("getStatus"))
                {
                    statusCode = status.Call<int>("getStatusCode");
                }

                if (statusCode == 0) // STATUS_OK
                {
                    mResultCallback(new AdvertisingResult(ResponseStatus.Success, mLocalEndpointName));
                    return;
                }

                if (statusCode == 8001) // STATUS_ALREADY_ADVERTISING
                {
                    mResultCallback(new AdvertisingResult(ResponseStatus.NotAuthorized, mLocalEndpointName));
                    return;
                }

                mResultCallback(new AdvertisingResult(ResponseStatus.InternalError, mLocalEndpointName));
            }

            public void onDisconnected(string endpointId)
            {
                if (mClient.mAdvertisingMessageListener != null)
                {
                    mClient.mAdvertisingMessageListener.OnRemoteEndpointDisconnected(endpointId);
                }
            }
        }

        public void StopAdvertising()
        {
            mClient.Call("stopAdvertising");
            mAdvertisingMessageListener = null;
        }

        public void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
            Action<ConnectionResponse> responseCallback, IMessageListener listener)
        {
            Misc.CheckNotNull(listener, "listener");
            var listenerOnGameThread = new OnGameThreadMessageListener(listener);
            DiscoveringConnectionLifecycleCallback cb =
                new DiscoveringConnectionLifecycleCallback(responseCallback, listenerOnGameThread, mClient);
            using (var connectionLifecycleCallback =
                new AndroidJavaObject("com.google.games.bridge.ConnectionLifecycleCallbackProxy", cb))
            using (mClient.Call<AndroidJavaObject>("requestConnection", name, remoteEndpointId,
                connectionLifecycleCallback))
                ;
        }

        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload, IMessageListener listener)
        {
            Misc.CheckNotNull(listener, "listener");
            mAdvertisingMessageListener = new OnGameThreadMessageListener(listener);

            using (var payloadCallback = new AndroidJavaObject("com.google.games.bridge.PayloadCallbackProxy",
                new PayloadCallback(listener)))
            using (mClient.Call<AndroidJavaObject>("acceptConnection", remoteEndpointId, payloadCallback))
                ;
        }

        private class PayloadCallback : AndroidJavaProxy
        {
            private IMessageListener mListener;

            public PayloadCallback(IMessageListener listener) : base(
                "com/google/games/bridge/PayloadCallbackProxy$Callback")
            {
                mListener = listener;
            }

            public void onPayloadReceived(String endpointId, AndroidJavaObject payload)
            {
                if (payload.Call<int>("getType") != 1) // 1 for BYTES
                {
                    return;
                }

                mListener.OnMessageReceived(endpointId, payload.Call<byte[]>("asBytes"), /* isReliableMessage */ true);
            }
        }

        public void StartDiscovery(string serviceId, TimeSpan? advertisingDuration,
            IDiscoveryListener listener)
        {
            Misc.CheckNotNull(serviceId, "serviceId");
            Misc.CheckNotNull(listener, "listener");

            var listenerOnGameThread = new OnGameThreadDiscoveryListener(listener);

            if (advertisingDuration.HasValue && advertisingDuration.Value.Ticks < 0)
            {
                throw new InvalidOperationException("advertisingDuration must be positive");
            }

            using (var endpointDiscoveryCallback = new AndroidJavaObject(
                "com.google.games.bridge.EndpointDiscoveryCallbackProxy",
                new EndpointDiscoveryCallback(listenerOnGameThread)))
            using (var discoveryOptions = CreateDiscoveryOptions())
            using (var task = mClient.Call<AndroidJavaObject>("startDiscovery", serviceId, endpointDiscoveryCallback,
                discoveryOptions))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    v => NearbyHelperObject.StartDiscoveryTimer(advertisingDuration)
                );
            }
        }

        private class DiscoveringConnectionLifecycleCallback : AndroidJavaProxy
        {
            private Action<ConnectionResponse> mResponseCallback;
            private IMessageListener mListener;
            private AndroidJavaObject mClient;
            private string mLocalEndpointName;

            public DiscoveringConnectionLifecycleCallback(Action<ConnectionResponse> responseCallback,
                IMessageListener listener, AndroidJavaObject client) : base(
                "com/google/games/bridge/ConnectionLifecycleCallbackProxy$Callback")
            {
                mResponseCallback = responseCallback;
                mListener = listener;
                mClient = client;
            }

            public void onConnectionInitiated(string endpointId, AndroidJavaObject connectionInfo)
            {
                using (var payloadCallback = new AndroidJavaObject("com.google.games.bridge.PayloadCallbackProxy",
                    new PayloadCallback(mListener)))
                using (mClient.Call<AndroidJavaObject>("acceptConnection", endpointId, payloadCallback))
                    ;
            }

            public void onConnectionResult(string endpointId, AndroidJavaObject connectionResolution)
            {
                int statusCode;
                using (var status = connectionResolution.Call<AndroidJavaObject>("getStatus"))
                {
                    statusCode = status.Call<int>("getStatusCode");
                }

                if (statusCode == 0) // STATUS_OK
                {
                    mResponseCallback(ConnectionResponse.Accepted(NearbyClientId, endpointId, new byte[0]));
                    return;
                }

                if (statusCode == 8002) // STATUS_ALREADY_DISCOVERING
                {
                    mResponseCallback(ConnectionResponse.AlreadyConnected(NearbyClientId, endpointId));
                    return;
                }

                mResponseCallback(ConnectionResponse.Rejected(NearbyClientId, endpointId));
            }

            public void onDisconnected(string endpointId)
            {
                mListener.OnRemoteEndpointDisconnected(endpointId);
            }
        }

        private AndroidJavaObject CreateDiscoveryOptions()
        {
            using (var strategy =
                new AndroidJavaClass("com.google.android.gms.nearby.connection.Strategy").GetStatic<AndroidJavaObject>(
                    "P2P_CLUSTER"))
            using (var builder =
                new AndroidJavaObject("com.google.android.gms.nearby.connection.DiscoveryOptions$Builder"))
            using (builder.Call<AndroidJavaObject>("setStrategy", strategy))
            {
                return builder.Call<AndroidJavaObject>("build");
            }
        }

        private class EndpointDiscoveryCallback : AndroidJavaProxy
        {
            private IDiscoveryListener mListener;

            public EndpointDiscoveryCallback(IDiscoveryListener listener) : base(
                "com/google/games/bridge/EndpointDiscoveryCallbackProxy$Callback")
            {
                mListener = listener;
            }

            public void onEndpointFound(string endpointId, AndroidJavaObject endpointInfo)
            {
                mListener.OnEndpointFound(CreateEndPointDetails(endpointId, endpointInfo));
            }

            public void onEndpointLost(string endpointId)
            {
                mListener.OnEndpointLost(endpointId);
            }

            private EndpointDetails CreateEndPointDetails(string endpointId, AndroidJavaObject endpointInfo)
            {
                return new EndpointDetails(
                    endpointId,
                    endpointInfo.Call<string>("getEndpointName"),
                    endpointInfo.Call<string>("getServiceId")
                );
            }
        }

        private class OnGameThreadMessageListener : IMessageListener
        {
            private readonly IMessageListener mListener;

            public OnGameThreadMessageListener(IMessageListener listener)
            {
                mListener = Misc.CheckNotNull(listener);
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

        private class OnGameThreadDiscoveryListener : IDiscoveryListener
        {
            private readonly IDiscoveryListener mListener;

            public OnGameThreadDiscoveryListener(IDiscoveryListener listener)
            {
                mListener = listener;
            }

            public void OnEndpointFound(EndpointDetails discoveredEndpoint)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnEndpointFound(discoveredEndpoint));
            }

            public void OnEndpointLost(string lostEndpointId)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnEndpointLost(lostEndpointId));
            }
        }

        public void StopDiscovery(string serviceId)
        {
            mClient.Call("stopDiscovery");
        }

        public void RejectConnectionRequest(string requestingEndpointId)
        {
            Misc.CheckNotNull(requestingEndpointId, "requestingEndpointId");
            using (var task = mClient.Call<AndroidJavaObject>("rejectConnection", requestingEndpointId)) ;
        }

        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            mClient.Call("disconnectFromEndpoint", remoteEndpointId);
        }

        public void StopAllConnections()
        {
            mClient.Call("stopAllEndpoints");
            mAdvertisingMessageListener = null;
        }

        public string GetAppBundleId()
        {
            using (var activity = AndroidHelperFragment.GetActivity())
            {
                return activity.Call<string>("getPackageName");
            }
        }

        public string GetServiceId()
        {
            return ServiceId;
        }

        private static string ReadServiceId()
        {
            using (var activity = AndroidHelperFragment.GetActivity())
            {
                string packageName = activity.Call<string>("getPackageName");
                using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
                using (var appInfo =
                    pm.Call<AndroidJavaObject>("getApplicationInfo", packageName, ApplicationInfoFlags))
                using (var bundle = appInfo.Get<AndroidJavaObject>("metaData"))
                {
                    string sysId = bundle.Call<string>("getString",
                        "com.google.android.gms.nearby.connection.SERVICE_ID");
                    Debug.Log("SystemId from Manifest: " + sysId);
                    return sysId;
                }
            }
        }

        private static Action<T> ToOnGameThread<T>(Action<T> toConvert)
        {
            return (val) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val));
        }

        private static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert)
        {
            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }
    }
}
#endif