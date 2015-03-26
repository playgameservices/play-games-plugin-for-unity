// <copyright file="NativeStartAdvertisingResult.cs" company="Google Inc.">
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
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Nearby;
    using System;
    using System.Runtime.InteropServices;
    using C = GooglePlayGames.Native.Cwrapper.NearbyConnectionTypes;

    internal class NativeStartAdvertisingResult : BaseReferenceHolder
    {
        internal NativeStartAdvertisingResult(IntPtr pointer)
            : base(pointer)
        {
        }

        internal int GetStatus()
        {
            return C.StartAdvertisingResult_GetStatus(SelfPtr());
        }


        internal string LocalEndpointName()
        {
            return PInvokeUtilities.OutParamsToString((out_arg, out_size) =>
            C.StartAdvertisingResult_GetLocalEndpointName(SelfPtr(), out_arg, out_size));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.StartAdvertisingResult_Dispose(selfPointer);
        }

        internal AdvertisingResult AsResult()
        {
            return new AdvertisingResult(
                (ResponseStatus)
                ResponseStatus.ToObject(
                    typeof(ResponseStatus),
                    GetStatus()), LocalEndpointName());
        }

        internal static NativeStartAdvertisingResult FromPointer(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            return new NativeStartAdvertisingResult(pointer);
        }
    }
}
#endif // #if UNITY_ANDROID 
