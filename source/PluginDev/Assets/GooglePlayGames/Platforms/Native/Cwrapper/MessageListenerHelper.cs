// <copyright file="MessageListenerHelper.cs" company="Google Inc.">
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

    internal static class MessageListenerHelper {
        internal delegate void OnMessageReceivedCallback(
         /* from(int64_t) */ long arg0,
         /* from(char const *) */ string arg1,
         /* from(uint8_t const *) */ IntPtr arg2,
         /* from(size_t) */ UIntPtr arg3,
        [MarshalAs(UnmanagedType.I1)] /* from(bool) */ bool arg4,
         /* from(void *) */ IntPtr arg5);

        internal delegate void OnDisconnectedCallback(
         /* from(int64_t) */ long arg0,
         /* from(char const *) */ string arg1,
         /* from(void *) */ IntPtr arg2);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void MessageListenerHelper_SetOnMessageReceivedCallback(
            HandleRef self,
            /* from(MessageListenerHelper_OnMessageReceivedCallback_t) */ OnMessageReceivedCallback callback,
            /* from(void *) */ IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void MessageListenerHelper_SetOnDisconnectedCallback(
            HandleRef self,
            /* from(MessageListenerHelper_OnDisconnectedCallback_t) */ OnDisconnectedCallback callback,
            /* from(void *) */ IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(MessageListenerHelper_t) */ IntPtr MessageListenerHelper_Construct();

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void MessageListenerHelper_Dispose(HandleRef self);
    }
}
#endif //UNITY_ANDROID

