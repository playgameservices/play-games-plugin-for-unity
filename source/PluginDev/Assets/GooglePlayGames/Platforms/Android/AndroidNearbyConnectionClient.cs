#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Nearby;
    using GooglePlayGames.OurUtils;
    using UnityEngine;
    using UnityEngine.Android;
    using UnityEngine.SocialPlatforms;

    public class AndroidNearbyConnectionClient : INearbyConnectionClient
    {
        private volatile AndroidJavaObject mClient;
        private readonly static string ServiceId = ReadServiceId();

        public AndroidNearbyConnectionClient()
        {
            using (var nearbyClass = new AndroidJavaClass("com.google.android.gms.nearby.Nearby"))
            {
                mClient = nearbyClass.CallStatic<AndroidJavaObject>("getConnectionsClient", AndroidHelperFragment.GetActivity());
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

        }

        public void SendUnreliable(List<string> recipientEndpointIds, byte[] payload)
        {

        }

        public void StartAdvertising(string name, List<string> appIdentifiers,
                              TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
                              Action<ConnectionRequest> connectionRequestCallback)
        {

        }

        public void StopAdvertising()
        {

        }

        public void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
                                   Action<ConnectionResponse> responseCallback, IMessageListener listener)
        {

        }

        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload,
                                     IMessageListener listener)
        {

        }

        // TODO: stop discovery after timeout
        public void StartDiscovery(string serviceId, TimeSpan? advertisingTimeout,
                            IDiscoveryListener listener)
        {
            Misc.CheckNotNull(serviceId, "serviceId");
            Misc.CheckNotNull(listener, "listener");

            AndroidJavaObject endpointDiscoveryCallback = new AndroidJavaObject("com.google.games.bridge.EndpointDiscoveryCallbackProxy", new EndpointDiscoveryCallback(listener));
            AndroidJavaObject discoveryOptions = CreateDiscoveryOptions();

            using (var task = mClient.Call<AndroidJavaObject>("startDiscovery", serviceId, endpointDiscoveryCallback, discoveryOptions))
            {
            }
        }

        private AndroidJavaObject CreateDiscoveryOptions()
        {
            AndroidJavaObject strategy = new AndroidJavaClass("com.google.android.gms.nearby.connection.Strategy").GetStatic<AndroidJavaObject>("P2P_CLUSTER");
            return new AndroidJavaObject("com.google.android.gms.nearby.connection.DiscoveryOptions$Builder").Call<AndroidJavaObject>("setStrategy", strategy).Call<AndroidJavaObject>("build");
        }

        private class EndpointDiscoveryCallback : AndroidJavaProxy
        {
            private IDiscoveryListener mListener;

            public EndpointDiscoveryCallback(IDiscoveryListener listener) : base("com/google/games/bridge/EndpointDiscoveryCallbackProxy$Callback")
            {
                mListener = listener;
            }

            public void onEndpointFound(string endpointId, AndroidJavaObject endpointInfo) {
                mListener.OnEndpointFound(CreateEndPointDetails(endpointId, endpointInfo));
            }

            public void onEndpointLost(string endpointId) {
                mListener.OnEndpointLost(endpointId);
            }

            private EndpointDetails CreateEndPointDetails(string endpointId, AndroidJavaObject endpointInfo) {
                return new EndpointDetails(
                    endpointId,
                    endpointInfo.Call<string>("getEndpointName"),
                    endpointInfo.Call<string>("getServiceId")
                );
            }
        }

        public void StopDiscovery(string serviceId)
        {
            mClient.Call("stopDiscovery");
        }

        public void RejectConnectionRequest(string requestingEndpointId)
        {

        }

        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            mClient.Call("disconnectFromEndpoint", remoteEndpointId);
        }

        public void StopAllConnections()
        {
            mClient.Call("stopAllEndpoints");
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
                AndroidJavaObject pm = activity.Call<AndroidJavaObject>("getPackageManager");
                Debug.Log("package name: " + packageName);
                AndroidJavaObject appInfo = pm.Call<AndroidJavaObject>("getApplicationInfo", packageName, (int)0x00000080);
                AndroidJavaObject bundle = appInfo.Get<AndroidJavaObject>("metaData");
                string sysId = bundle.Call<string>("getString", "com.google.android.gms.nearby.connection.SERVICE_ID");
                Debug.Log("SystemId from Manifest: " + sysId);
                return sysId;
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
