// <copyright file="NearbyConnectionsManager.cs" company="Google Inc.">
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

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using C = GooglePlayGames.Native.Cwrapper.NearbyConnections;
    using N = GooglePlayGames.Native.Cwrapper.NearbyConnectionTypes;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using UnityEngine;

    internal class NearbyConnectionsManager : BaseReferenceHolder
    {

        private readonly static string sServiceId = ReadServiceId();

        internal NearbyConnectionsManager(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.NearbyConnections_Dispose(selfPointer);
        }

        internal void SendUnreliable(string remoteEndpointId, byte[] payload)
        {
            C.NearbyConnections_SendUnreliableMessage(SelfPtr(), remoteEndpointId,
                payload, new UIntPtr((ulong)payload.Length));
        }

        internal void SendReliable(string remoteEndpointId, byte[] payload)
        {
            C.NearbyConnections_SendReliableMessage(SelfPtr(), remoteEndpointId,
                payload, new UIntPtr((ulong)payload.Length));
        }

        internal void StartAdvertising(string name,
                                   List<NativeAppIdentifier> appIds,
                                   long advertisingDuration,
                                   Action<long, NativeStartAdvertisingResult> advertisingCallback,
                                   Action<long, NativeConnectionRequest> connectionRequestCallback)
        {
            C.NearbyConnections_StartAdvertising(
                SelfPtr(),
                name,
                appIds.Select(id => id.AsPointer()).ToArray(),
                new UIntPtr((ulong)appIds.Count),
                advertisingDuration,
                InternalStartAdvertisingCallback,
                Callbacks.ToIntPtr<long, NativeStartAdvertisingResult>(advertisingCallback,
                    NativeStartAdvertisingResult.FromPointer),
                InternalConnectionRequestCallback,
                Callbacks.ToIntPtr<long, NativeConnectionRequest>(connectionRequestCallback,
                    NativeConnectionRequest.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(N.StartAdvertisingCallback))]
        private static void InternalStartAdvertisingCallback(long id, IntPtr result,
                                                         IntPtr userData)
        {
            Callbacks.PerformInternalCallback<long>(
                "NearbyConnectionsManager#InternalStartAdvertisingCallback", Callbacks.Type.Permanent,
                id, result, userData);
        }

        [AOT.MonoPInvokeCallback(typeof(N.ConnectionRequestCallback))]
        private static void InternalConnectionRequestCallback(long id, IntPtr result,
                                                          IntPtr userData)
        {
            Callbacks.PerformInternalCallback<long>(
                "NearbyConnectionsManager#InternalConnectionRequestCallback", Callbacks.Type.Permanent,
                id, result, userData);
        }

        internal void StopAdvertising()
        {
            C.NearbyConnections_StopAdvertising(SelfPtr());
        }

        internal void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
                                        Action<long, NativeConnectionResponse> callback, NativeMessageListenerHelper listener)
        {
            C.NearbyConnections_SendConnectionRequest(
                SelfPtr(),
                name,
                remoteEndpointId,
                payload,
                new UIntPtr((ulong)payload.Length),
                InternalConnectResponseCallback,
                Callbacks.ToIntPtr<long, NativeConnectionResponse>(
                    callback, NativeConnectionResponse.FromPointer),
                listener.AsPointer());
        }

        [AOT.MonoPInvokeCallback(typeof(N.ConnectionResponseCallback))]
        private static void InternalConnectResponseCallback(long localClientId, IntPtr response,
                                                        IntPtr userData)
        {
            Callbacks.PerformInternalCallback<long>(
                "NearbyConnectionManager#InternalConnectResponseCallback", Callbacks.Type.Temporary,
                localClientId, response, userData);
        }

        internal void AcceptConnectionRequest(string remoteEndpointId, byte[] payload,
                                          NativeMessageListenerHelper listener)
        {
            C.NearbyConnections_AcceptConnectionRequest(SelfPtr(), remoteEndpointId, payload,
                new UIntPtr((ulong)payload.Length), listener.AsPointer());
        }

        internal void DisconnectFromEndpoint(string remoteEndpointId)
        {
            C.NearbyConnections_Disconnect(SelfPtr(), remoteEndpointId);
        }

        internal void StopAllConnections()
        {
            C.NearbyConnections_Stop(SelfPtr());
        }

        internal void StartDiscovery(string serviceId, long duration,
                                 NativeEndpointDiscoveryListenerHelper listener)
        {
            C.NearbyConnections_StartDiscovery(SelfPtr(), serviceId, duration, listener.AsPointer());
        }

        internal void StopDiscovery(string serviceId)
        {
            C.NearbyConnections_StopDiscovery(SelfPtr(), serviceId);
        }

        internal void RejectConnectionRequest(string remoteEndpointId)
        {
            C.NearbyConnections_RejectConnectionRequest(SelfPtr(), remoteEndpointId);
        }

        internal void Shutdown()
        {
            C.NearbyConnections_Stop(SelfPtr());
        }

        public string AppBundleId
        {
            get
            {

#if UNITY_ANDROID
                using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                    return activity.Call<string>("getPackageName");
                   
                }

#else
				return "";
#endif
            }
        }

        internal static string ReadServiceId()
        {
            Debug.Log("Initializing ServiceId property!!!!");

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var activity = jc.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string packageName = activity.Call<string>("getPackageName");
                    AndroidJavaObject pm = activity.Call<AndroidJavaObject>("getPackageManager");
                    AndroidJavaObject appInfo = pm.Call<AndroidJavaObject>("getApplicationInfo", packageName, (int)0x00000080);
                    AndroidJavaObject bundle = appInfo.Get<AndroidJavaObject>("metaData");
                    string sysId = bundle.Call<string>("getString", "com.google.android.gms.nearby.connection.SERVICE_ID");
                    Debug.Log("SystemId from Manifest: " + sysId);
                    return sysId;
                }
            }

#else
            return null;
#endif
        }


        public static string ServiceId
        {
            get
            {
                return sServiceId;
            }
        }



    }
}
#endif // #if UNITY_ANDROID
