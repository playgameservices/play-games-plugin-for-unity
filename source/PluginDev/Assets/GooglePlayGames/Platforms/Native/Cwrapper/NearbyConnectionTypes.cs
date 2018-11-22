// <copyright file="NearbyConnectionTypes.cs" company="Google Inc.">
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
namespace GooglePlayGames.Native.Cwrapper
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class NearbyConnectionTypes
    {
        internal enum ConnectionResponse_ResponseCode
        {
            ACCEPTED = 1,
            REJECTED = 2,
            ERROR_INTERNAL = -1,
            ERROR_NETWORK_NOT_CONNECTED = -2,
            ERROR_ENDPOINT_ALREADY_CONNECTED = -3,
            ERROR_ENDPOINT_NOT_CONNECTED = -4
        }


        internal delegate void ConnectionRequestCallback(
        /* from(int64_t) */ long arg0,
        /* from(ConnectionRequest_t) */ IntPtr arg1,
        /* from(void *) */ IntPtr arg2);

        internal delegate void StartAdvertisingCallback(
        /* from(int64_t) */ long arg0,
        /* from(StartAdvertisingResult_t) */ IntPtr arg1,
        /* from(void *) */ IntPtr arg2);

        internal delegate void ConnectionResponseCallback(
        /* from(int64_t) */ long arg0,
        /* from(ConnectionResponse_t) */ IntPtr arg1,
        /* from(void *) */ IntPtr arg2);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void AppIdentifier_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr AppIdentifier_GetIdentifier(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void StartAdvertisingResult_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern /* from(uint32_t) */ int StartAdvertisingResult_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr StartAdvertisingResult_GetLocalEndpointName(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void EndpointDetails_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr EndpointDetails_GetEndpointId(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr EndpointDetails_GetName(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr EndpointDetails_GetServiceId(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void ConnectionRequest_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr ConnectionRequest_GetRemoteEndpointId(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr ConnectionRequest_GetRemoteEndpointName(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr ConnectionRequest_GetPayload(
            HandleRef self,
            [In, Out] /* from(uint8_t *) */ byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void ConnectionResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr ConnectionResponse_GetRemoteEndpointId(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ConnectionResponse_ResponseCode_t) */ NearbyConnectionTypes.ConnectionResponse_ResponseCode ConnectionResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr ConnectionResponse_GetPayload(
            HandleRef self,
            [In, Out] /* from(uint8_t *) */ byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);
    }
}
#endif //UNITY_ANDROID

