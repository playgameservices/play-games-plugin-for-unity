/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GooglePlayGames.Native.Cwrapper {
internal static class RealTimeEventListenerHelper {
    internal delegate void OnRoomStatusChangedCallback(
         /* from(RealTimeRoom_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void OnRoomConnectedSetChangedCallback(
         /* from(RealTimeRoom_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void OnP2PConnectedCallback(
         /* from(RealTimeRoom_t) */ IntPtr arg0,
         /* from(MultiplayerParticipant_t) */ IntPtr arg1,
         /* from(void *) */ IntPtr arg2);

    internal delegate void OnP2PDisconnectedCallback(
         /* from(RealTimeRoom_t) */ IntPtr arg0,
         /* from(MultiplayerParticipant_t) */ IntPtr arg1,
         /* from(void *) */ IntPtr arg2);

    internal delegate void OnParticipantStatusChangedCallback(
         /* from(RealTimeRoom_t) */ IntPtr arg0,
         /* from(MultiplayerParticipant_t) */ IntPtr arg1,
         /* from(void *) */ IntPtr arg2);

    internal delegate void OnDataReceivedCallback(
         /* from(RealTimeRoom_t) */ IntPtr arg0,
         /* from(MultiplayerParticipant_t) */ IntPtr arg1,
         /* from(uint8_t const *) */ IntPtr arg2,
         /* from(size_t) */ UIntPtr arg3,
        [MarshalAs(UnmanagedType.I1)] /* from(bool) */ bool arg4,
         /* from(void *) */ IntPtr arg5);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeEventListenerHelper_SetOnParticipantStatusChangedCallback(
        HandleRef self,
         /* from(RealTimeEventListenerHelper_OnParticipantStatusChangedCallback_t) */ OnParticipantStatusChangedCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(RealTimeEventListenerHelper_t) */ IntPtr RealTimeEventListenerHelper_Construct();

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeEventListenerHelper_SetOnP2PDisconnectedCallback(
        HandleRef self,
         /* from(RealTimeEventListenerHelper_OnP2PDisconnectedCallback_t) */ OnP2PDisconnectedCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeEventListenerHelper_SetOnDataReceivedCallback(
        HandleRef self,
         /* from(RealTimeEventListenerHelper_OnDataReceivedCallback_t) */ OnDataReceivedCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeEventListenerHelper_SetOnRoomStatusChangedCallback(
        HandleRef self,
         /* from(RealTimeEventListenerHelper_OnRoomStatusChangedCallback_t) */ OnRoomStatusChangedCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeEventListenerHelper_SetOnP2PConnectedCallback(
        HandleRef self,
         /* from(RealTimeEventListenerHelper_OnP2PConnectedCallback_t) */ OnP2PConnectedCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeEventListenerHelper_SetOnRoomConnectedSetChangedCallback(
        HandleRef self,
         /* from(RealTimeEventListenerHelper_OnRoomConnectedSetChangedCallback_t) */ OnRoomConnectedSetChangedCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeEventListenerHelper_Dispose(
        HandleRef self);
}
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
