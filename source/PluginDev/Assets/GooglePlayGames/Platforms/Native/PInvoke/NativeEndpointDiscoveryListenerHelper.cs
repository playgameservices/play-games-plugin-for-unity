// <copyright file="NativeEndpointDiscoveryListenerHelper.cs" company="Google Inc.">
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
    using GooglePlayGames.OurUtils;
    using System;
    using System.Runtime.InteropServices;
    using C = GooglePlayGames.Native.Cwrapper.EndpointDiscoveryListenerHelper;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    internal class NativeEndpointDiscoveryListenerHelper : BaseReferenceHolder
    {
        internal NativeEndpointDiscoveryListenerHelper()
            : base(C.EndpointDiscoveryListenerHelper_Construct())
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.EndpointDiscoveryListenerHelper_Dispose(selfPointer);
        }

        internal void SetOnEndpointFound(Action<long, NativeEndpointDetails> callback)
        {
            C.EndpointDiscoveryListenerHelper_SetOnEndpointFoundCallback(
                SelfPtr(),
                InternalOnEndpointFoundCallback,
                Callbacks.ToIntPtr<long, NativeEndpointDetails>(
                    callback, NativeEndpointDetails.FromPointer)
            );
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnEndpointFoundCallback))]
        private static void InternalOnEndpointFoundCallback(long id, IntPtr data, IntPtr userData)
        {

            Callbacks.PerformInternalCallback<long>(
                "NativeEndpointDiscoveryListenerHelper#InternalOnEndpointFoundCallback",
                // make this Perm since the callback can happen multiple times.
                Callbacks.Type.Permanent, id, data, userData);
        }

        internal void SetOnEndpointLostCallback(Action<long, string> callback)
        {
            C.EndpointDiscoveryListenerHelper_SetOnEndpointLostCallback(
                SelfPtr(),
                InternalOnEndpointLostCallback,
                Callbacks.ToIntPtr(callback)
            );
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnEndpointLostCallback))]
        private static void InternalOnEndpointLostCallback(long id, string lostEndpointId,
                                                           IntPtr userData)
        {

            var userCallback = Callbacks.IntPtrToPermanentCallback<Action<long, string>>(userData);

            if (userCallback == null)
            {
                return;
            }

            try
            {
                userCallback(id, lostEndpointId);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing " +
                    "NativeEndpointDiscoveryListenerHelper#InternalOnEndpointLostCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

    }
}
#endif // #if UNITY_ANDROID