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
internal static class RealTimeMultiplayerManager {
    internal delegate void RealTimeRoomCallback(
         /* from(RealTimeMultiplayerManager_RealTimeRoomResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void LeaveRoomCallback(
         /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void SendReliableMessageCallback(
         /* from(MultiplayerStatus_t) */ CommonErrorStatus.MultiplayerStatus arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void RoomInboxUICallback(
         /* from(RealTimeMultiplayerManager_RoomInboxUIResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void PlayerSelectUICallback(
         /* from(RealTimeMultiplayerManager_PlayerSelectUIResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void WaitingRoomUICallback(
         /* from(RealTimeMultiplayerManager_WaitingRoomUIResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void FetchInvitationsCallback(
         /* from(RealTimeMultiplayerManager_FetchInvitationsResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_CreateRealTimeRoom(
        HandleRef self,
         /* from(RealTimeRoomConfig_t) */ IntPtr config,
         /* from(RealTimeEventListenerHelper_t) */ IntPtr helper,
         /* from(RealTimeMultiplayerManager_RealTimeRoomCallback_t) */ RealTimeRoomCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_LeaveRoom(
        HandleRef self,
         /* from(RealTimeRoom_t) */ IntPtr room,
         /* from(RealTimeMultiplayerManager_LeaveRoomCallback_t) */ LeaveRoomCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_SendUnreliableMessage(
        HandleRef self,
         /* from(RealTimeRoom_t) */ IntPtr room,
         /* from(MultiplayerParticipant_t const *) */ IntPtr[] participants,
         /* from(size_t) */ UIntPtr participants_size,
         /* from(uint8_t const *) */ byte[] data,
         /* from(size_t) */ UIntPtr data_size);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_ShowWaitingRoomUI(
        HandleRef self,
         /* from(RealTimeRoom_t) */ IntPtr room,
         /* from(uint32_t) */ uint min_participants_to_start,
         /* from(RealTimeMultiplayerManager_WaitingRoomUICallback_t) */ WaitingRoomUICallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_ShowPlayerSelectUI(
        HandleRef self,
         /* from(uint32_t) */ uint minimum_players,
         /* from(uint32_t) */ uint maximum_players,
        [MarshalAs(UnmanagedType.I1)] /* from(bool) */ bool allow_automatch,
         /* from(RealTimeMultiplayerManager_PlayerSelectUICallback_t) */ PlayerSelectUICallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_DismissInvitation(
        HandleRef self,
         /* from(MultiplayerInvitation_t) */ IntPtr invitation);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_DeclineInvitation(
        HandleRef self,
         /* from(MultiplayerInvitation_t) */ IntPtr invitation);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_SendReliableMessage(
        HandleRef self,
         /* from(RealTimeRoom_t) */ IntPtr room,
         /* from(MultiplayerParticipant_t) */ IntPtr participant,
         /* from(uint8_t const *) */ byte[] data,
         /* from(size_t) */ UIntPtr data_size,
         /* from(RealTimeMultiplayerManager_SendReliableMessageCallback_t) */ SendReliableMessageCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_AcceptInvitation(
        HandleRef self,
         /* from(MultiplayerInvitation_t) */ IntPtr invitation,
         /* from(RealTimeEventListenerHelper_t) */ IntPtr helper,
         /* from(RealTimeMultiplayerManager_RealTimeRoomCallback_t) */ RealTimeRoomCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_FetchInvitations(
        HandleRef self,
         /* from(RealTimeMultiplayerManager_FetchInvitationsCallback_t) */ FetchInvitationsCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_SendUnreliableMessageToOthers(
        HandleRef self,
         /* from(RealTimeRoom_t) */ IntPtr room,
         /* from(uint8_t const *) */ byte[] data,
         /* from(size_t) */ UIntPtr data_size);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_ShowRoomInboxUI(
        HandleRef self,
         /* from(RealTimeMultiplayerManager_RoomInboxUICallback_t) */ RoomInboxUICallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_RealTimeRoomResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(MultiplayerStatus_t) */ CommonErrorStatus.MultiplayerStatus RealTimeMultiplayerManager_RealTimeRoomResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(RealTimeRoom_t) */ IntPtr RealTimeMultiplayerManager_RealTimeRoomResponse_GetRoom(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_RoomInboxUIResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(UIStatus_t) */ CommonErrorStatus.UIStatus RealTimeMultiplayerManager_RoomInboxUIResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(MultiplayerInvitation_t) */ IntPtr RealTimeMultiplayerManager_RoomInboxUIResponse_GetInvitation(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_WaitingRoomUIResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(UIStatus_t) */ CommonErrorStatus.UIStatus RealTimeMultiplayerManager_WaitingRoomUIResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(RealTimeRoom_t) */ IntPtr RealTimeMultiplayerManager_WaitingRoomUIResponse_GetRoom(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void RealTimeMultiplayerManager_FetchInvitationsResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus RealTimeMultiplayerManager_FetchInvitationsResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr RealTimeMultiplayerManager_FetchInvitationsResponse_GetInvitations_Length(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(MultiplayerInvitation_t) */ IntPtr RealTimeMultiplayerManager_FetchInvitationsResponse_GetInvitations_GetElement(
        HandleRef self,
         /* from(size_t) */ UIntPtr index);
}
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
