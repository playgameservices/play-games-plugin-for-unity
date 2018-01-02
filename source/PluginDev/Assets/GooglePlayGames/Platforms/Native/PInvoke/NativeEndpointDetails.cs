// <copyright file="NativeEndpointDetails.cs" company="Google Inc.">
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

    internal class NativeEndpointDetails : BaseReferenceHolder
    {
        internal NativeEndpointDetails(IntPtr pointer)
            : base(pointer)
        {
        }

        internal string EndpointId()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_arg, out_size) => C.EndpointDetails_GetEndpointId(SelfPtr(), out_arg, out_size));
        }

        internal string Name()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_arg, out_size) => C.EndpointDetails_GetName(SelfPtr(), out_arg, out_size));
        }

        internal string ServiceId()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_arg, out_size) => C.EndpointDetails_GetServiceId(SelfPtr(), out_arg, out_size));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.EndpointDetails_Dispose(selfPointer);
        }

        internal EndpointDetails ToDetails()
        {
            return new EndpointDetails(EndpointId(), Name(), ServiceId());
        }

        internal static NativeEndpointDetails FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }

            return new NativeEndpointDetails(pointer);
        }

    }
}
#endif // #if (UNITY_ANDROID || UNITY_IPHONE)
