// <copyright file="NativeConnectionResponse.cs" company="Google Inc.">
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
    using GooglePlayGames.BasicApi.Nearby;
    using System;
    using System.Runtime.InteropServices;
    using C = GooglePlayGames.Native.Cwrapper.NearbyConnectionTypes;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

internal class NativeConnectionResponse : BaseReferenceHolder
    {
        internal NativeConnectionResponse(IntPtr pointer)
            : base(pointer)
        {
        }

        internal string RemoteEndpointId()
        {
            return PInvokeUtilities.OutParamsToString((out_arg, out_size) =>
            C.ConnectionResponse_GetRemoteEndpointId(SelfPtr(), out_arg, out_size));
        }

        internal C.ConnectionResponse_ResponseCode ResponseCode()
        {
            return C.ConnectionResponse_GetStatus(SelfPtr());
        }

        internal byte[] Payload()
        {
            return PInvokeUtilities.OutParamsToArray<byte>((out_arg, out_size) =>
            C.ConnectionResponse_GetPayload(SelfPtr(), out_arg, out_size));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.ConnectionResponse_Dispose(selfPointer);
        }

        internal ConnectionResponse AsResponse(long localClientId)
        {
            switch (ResponseCode())
            {
                case C.ConnectionResponse_ResponseCode.ACCEPTED:
                    return ConnectionResponse.Accepted(localClientId, RemoteEndpointId(), Payload());
                case C.ConnectionResponse_ResponseCode.ERROR_ENDPOINT_ALREADY_CONNECTED:
                    return ConnectionResponse.AlreadyConnected(localClientId, RemoteEndpointId());
                case C.ConnectionResponse_ResponseCode.REJECTED:
                    return ConnectionResponse.Rejected(localClientId, RemoteEndpointId());
                case C.ConnectionResponse_ResponseCode.ERROR_ENDPOINT_NOT_CONNECTED:
                    return ConnectionResponse.EndpointNotConnected(localClientId, RemoteEndpointId());
                case C.ConnectionResponse_ResponseCode.ERROR_NETWORK_NOT_CONNECTED:
                    return ConnectionResponse.NetworkNotConnected(localClientId, RemoteEndpointId());
                case C.ConnectionResponse_ResponseCode.ERROR_INTERNAL:
                    return ConnectionResponse.InternalError(localClientId, RemoteEndpointId());
                default:
                    throw new InvalidOperationException("Found connection response of unknown type: " +
                        ResponseCode());
            }
        }

        internal static NativeConnectionResponse FromPointer(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            return new NativeConnectionResponse(pointer);
        }
    }
}
#endif // #if UNITY_ANDROID
