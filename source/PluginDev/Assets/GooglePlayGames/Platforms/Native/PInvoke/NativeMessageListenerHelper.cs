// <copyright file="NativeMessageListenerHelper.cs" company="Google Inc.">
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
    using C = GooglePlayGames.Native.Cwrapper.MessageListenerHelper;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    internal class NativeMessageListenerHelper : BaseReferenceHolder
    {
        internal delegate void OnMessageReceived(long localClientId,string remoteEndpointId,
        byte[] data,bool isReliable);

        internal NativeMessageListenerHelper()
            : base(C.MessageListenerHelper_Construct())
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.MessageListenerHelper_Dispose(selfPointer);
        }

        internal void SetOnMessageReceivedCallback(OnMessageReceived callback)
        {
            C.MessageListenerHelper_SetOnMessageReceivedCallback(
                SelfPtr(),
                InternalOnMessageReceivedCallback,
                Callbacks.ToIntPtr(callback)
            );
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnMessageReceivedCallback))]
        private static void InternalOnMessageReceivedCallback(long id, string name, IntPtr data,
                                                              UIntPtr dataLength, bool isReliable, IntPtr userData)
        {
            var userCallback = Callbacks.IntPtrToPermanentCallback<OnMessageReceived>(userData);

            if (userCallback == null)
            {
                return;
            }

            try
            {
                userCallback(id, name, Callbacks.IntPtrAndSizeToByteArray(data, dataLength),
                    isReliable);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing " +
                    "NativeMessageListenerHelper#InternalOnMessageReceivedCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal void SetOnDisconnectedCallback(Action<long, string> callback)
        {
            C.MessageListenerHelper_SetOnDisconnectedCallback(
                SelfPtr(),
                InternalOnDisconnectedCallback,
                Callbacks.ToIntPtr(callback)
            );
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnDisconnectedCallback))]
        private static void InternalOnDisconnectedCallback(long id, string lostEndpointId,
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
                    "NativeMessageListenerHelper#InternalOnDisconnectedCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

    }
}
#endif // #if (UNITY_ANDROID)
