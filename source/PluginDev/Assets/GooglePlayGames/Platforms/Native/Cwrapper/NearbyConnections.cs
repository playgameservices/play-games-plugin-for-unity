// <copyright file="NearbyConnections.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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

namespace GooglePlayGames.Native.Cwrapper {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class NearbyConnections {
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_StartDiscovery(
            HandleRef self,
            /* from(char const *) */ string service_id,
            /* from(int64_t) */ long duration,
            /* from(EndpointDiscoveryListenerHelper_t) */ IntPtr helper);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_RejectConnectionRequest(
            HandleRef self,
            /* from(char const *) */ string remote_endpoint_id);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_Disconnect(
            HandleRef self,
            /* from(char const *) */ string remote_endpoint_id);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_SendUnreliableMessage(
            HandleRef self,
            /* from(char const *) */ string remote_endpoint_id,
            /* from(uint8_t const *) */ byte[] payload,
            /* from(size_t) */ UIntPtr payload_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr NearbyConnections_GetLocalDeviceId(
            HandleRef self,
            /* from(char *) */ StringBuilder out_arg,
            /* from(size_t) */ UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_StopAdvertising(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr NearbyConnections_GetLocalEndpointId(
            HandleRef self,
            /* from(char *) */ StringBuilder out_arg,
            /* from(size_t) */ UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_SendReliableMessage(
            HandleRef self,
            /* from(char const *) */ string remote_endpoint_id,
            /* from(uint8_t const *) */ byte[] payload,
            /* from(size_t) */ UIntPtr payload_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_StopDiscovery(
            HandleRef self,
            /* from(char const *) */ string service_id);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_SendConnectionRequest(
            HandleRef self,
            /* from(char const *) */ string name,
            /* from(char const *) */ string remote_endpoint_id,
            /* from(uint8_t const *) */ byte[] payload,
            /* from(size_t) */ UIntPtr payload_size,
            /* from(ConnectionResponseCallback_t) */ NearbyConnectionTypes.ConnectionResponseCallback callback,
            /* from(void *) */ IntPtr callback_arg,
            /* from(MessageListenerHelper_t) */ IntPtr helper);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_StartAdvertising(
            HandleRef self,
            /* from(char const *) */ string name,
            /* from(AppIdentifier_t const *) */ IntPtr[] app_identifiers,
            /* from(size_t) */ UIntPtr app_identifiers_size,
            /* from(int64_t) */ long duration,
            /* from(StartAdvertisingCallback_t) */ NearbyConnectionTypes.StartAdvertisingCallback start_advertising_callback,
            /* from(void *) */ IntPtr start_advertising_callback_arg,
            /* from(ConnectionRequestCallback_t) */ NearbyConnectionTypes.ConnectionRequestCallback request_callback,
            /* from(void *) */ IntPtr request_callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_Stop(HandleRef self);
    
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void NearbyConnections_AcceptConnectionRequest(
            HandleRef self,
            /* from(char const *) */ string remote_endpoint_id,
            /* from(uint8_t const *) */ byte[] payload,
            /* from(size_t) */ UIntPtr payload_size,
            /* from(MessageListenerHelper_t) */ IntPtr helper);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
