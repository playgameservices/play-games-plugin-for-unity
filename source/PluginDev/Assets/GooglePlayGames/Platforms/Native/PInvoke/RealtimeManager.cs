// <copyright file="RealtimeManager.cs" company="Google Inc.">
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


#if UNITY_ANDROID

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Linq;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.BasicApi.Multiplayer;
    using System.Collections.Generic;
    using C = GooglePlayGames.Native.Cwrapper.RealTimeMultiplayerManager;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using MultiplayerStatus = GooglePlayGames.Native.Cwrapper.CommonErrorStatus.MultiplayerStatus;
    using System.Runtime.InteropServices;

    internal class RealtimeManager
    {

        private readonly GameServices mGameServices;

        internal RealtimeManager(GameServices gameServices)
        {
            this.mGameServices = Misc.CheckNotNull(gameServices);
        }

        internal void CreateRoom(RealtimeRoomConfig config, RealTimeEventListenerHelper helper,
                             Action<RealTimeRoomResponse> callback)
        {
            C.RealTimeMultiplayerManager_CreateRealTimeRoom(mGameServices.AsHandle(),
                config.AsPointer(),
                helper.AsPointer(),
                InternalRealTimeRoomCallback,
                ToCallbackPointer(callback));
        }

        internal void ShowPlayerSelectUI(uint minimumPlayers, uint maxiumPlayers,
                                     bool allowAutomatching, Action<PlayerSelectUIResponse> callback)
        {
            C.RealTimeMultiplayerManager_ShowPlayerSelectUI(mGameServices.AsHandle(), minimumPlayers,
                maxiumPlayers, allowAutomatching, InternalPlayerSelectUIcallback,
                Callbacks.ToIntPtr(callback, PlayerSelectUIResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.PlayerSelectUICallback))]
        internal static void InternalPlayerSelectUIcallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "RealtimeManager#PlayerSelectUICallback", Callbacks.Type.Temporary, response, data);
        }

        [AOT.MonoPInvokeCallback(typeof(C.RealTimeRoomCallback))]
        internal static void InternalRealTimeRoomCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "RealtimeManager#InternalRealTimeRoomCallback",
                Callbacks.Type.Temporary, response, data);
        }

        [AOT.MonoPInvokeCallback(typeof(C.RoomInboxUICallback))]
        internal static void InternalRoomInboxUICallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "RealtimeManager#InternalRoomInboxUICallback",
                Callbacks.Type.Temporary, response, data);
        }

        internal void ShowRoomInboxUI(Action<RoomInboxUIResponse> callback)
        {
            C.RealTimeMultiplayerManager_ShowRoomInboxUI(mGameServices.AsHandle(),
                InternalRoomInboxUICallback,
                Callbacks.ToIntPtr(callback, RoomInboxUIResponse.FromPointer));
        }

        internal void ShowWaitingRoomUI(NativeRealTimeRoom room, uint minimumParticipantsBeforeStarting,
                                    Action<WaitingRoomUIResponse> callback)
        {
            Misc.CheckNotNull(room);
            C.RealTimeMultiplayerManager_ShowWaitingRoomUI(mGameServices.AsHandle(),
                room.AsPointer(),
                minimumParticipantsBeforeStarting,
                InternalWaitingRoomUICallback,
                Callbacks.ToIntPtr(callback, WaitingRoomUIResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.WaitingRoomUICallback))]
        internal static void InternalWaitingRoomUICallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "RealtimeManager#InternalWaitingRoomUICallback",
                Callbacks.Type.Temporary, response, data);
        }


        [AOT.MonoPInvokeCallback(typeof(C.FetchInvitationsCallback))]
        internal static void InternalFetchInvitationsCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "RealtimeManager#InternalFetchInvitationsCallback",
                Callbacks.Type.Temporary, response, data);
        }

        internal void FetchInvitations(Action<FetchInvitationsResponse> callback)
        {
            C.RealTimeMultiplayerManager_FetchInvitations(mGameServices.AsHandle(),
                InternalFetchInvitationsCallback,
                Callbacks.ToIntPtr(callback, FetchInvitationsResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.LeaveRoomCallback))]
        internal static void InternalLeaveRoomCallback(Status.ResponseStatus response, IntPtr data)
        {
            Logger.d("Entering internal callback for InternalLeaveRoomCallback");

            Action<Status.ResponseStatus> callback =
                Callbacks.IntPtrToTempCallback<Action<Status.ResponseStatus>>(data);

            if (callback == null)
            {
                return;
            }

            try
            {
                callback(response);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing InternalLeaveRoomCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal void LeaveRoom(NativeRealTimeRoom room, Action<Status.ResponseStatus> callback)
        {
            C.RealTimeMultiplayerManager_LeaveRoom(mGameServices.AsHandle(),
                room.AsPointer(),
                InternalLeaveRoomCallback,
                Callbacks.ToIntPtr(callback));
        }

        internal void AcceptInvitation(MultiplayerInvitation invitation,
                                   RealTimeEventListenerHelper listener,
                                   Action<RealTimeRoomResponse> callback)
        {
            C.RealTimeMultiplayerManager_AcceptInvitation(mGameServices.AsHandle(),
                invitation.AsPointer(), listener.AsPointer(), InternalRealTimeRoomCallback,
                ToCallbackPointer(callback));
        }

        internal void DeclineInvitation(MultiplayerInvitation invitation)
        {
            C.RealTimeMultiplayerManager_DeclineInvitation(mGameServices.AsHandle(),
                invitation.AsPointer());
        }

        internal void SendReliableMessage(NativeRealTimeRoom room, MultiplayerParticipant participant,
                                      byte[] data, Action<Status.MultiplayerStatus> callback)
        {
            C.RealTimeMultiplayerManager_SendReliableMessage(mGameServices.AsHandle(),
                room.AsPointer(), participant.AsPointer(), data, PInvokeUtilities.ArrayToSizeT(data),
                InternalSendReliableMessageCallback, Callbacks.ToIntPtr(callback));
        }

        [AOT.MonoPInvokeCallback(typeof(C.SendReliableMessageCallback))]
        internal static void InternalSendReliableMessageCallback(Status.MultiplayerStatus response,
                                                             IntPtr data)
        {
            Logger.d("Entering internal callback for InternalSendReliableMessageCallback " + response);

            Action<Status.MultiplayerStatus> callback =
                Callbacks.IntPtrToTempCallback<Action<Status.MultiplayerStatus>>(data);

            if (callback == null)
            {
                return;
            }

            try
            {
                callback(response);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing InternalSendReliableMessageCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal void SendUnreliableMessageToAll(NativeRealTimeRoom room, byte[] data)
        {
            C.RealTimeMultiplayerManager_SendUnreliableMessageToOthers(mGameServices.AsHandle(),
                room.AsPointer(), data, PInvokeUtilities.ArrayToSizeT(data));
        }

        internal void SendUnreliableMessageToSpecificParticipants(NativeRealTimeRoom room,
                                                              List<MultiplayerParticipant> recipients, byte[] data)
        {
            C.RealTimeMultiplayerManager_SendUnreliableMessage(
                mGameServices.AsHandle(),
                room.AsPointer(),
                recipients.Select(r => r.AsPointer()).ToArray(),
                new UIntPtr((ulong)recipients.LongCount()),
                data,
                PInvokeUtilities.ArrayToSizeT(data));
        }

        private static IntPtr ToCallbackPointer(Action<RealTimeRoomResponse> callback)
        {
            return Callbacks.ToIntPtr<RealTimeRoomResponse>(
                callback,
                RealTimeRoomResponse.FromPointer
            );
        }

        internal class RealTimeRoomResponse : BaseReferenceHolder
        {
            internal RealTimeRoomResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.MultiplayerStatus ResponseStatus()
            {
                return C.RealTimeMultiplayerManager_RealTimeRoomResponse_GetStatus(SelfPtr());
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            internal NativeRealTimeRoom Room()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                return new NativeRealTimeRoom(
                    C.RealTimeMultiplayerManager_RealTimeRoomResponse_GetRoom(SelfPtr()));
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.RealTimeMultiplayerManager_RealTimeRoomResponse_Dispose(selfPointer);
            }

            internal static RealTimeRoomResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new RealTimeRoomResponse(pointer);
            }
        }

        internal class RoomInboxUIResponse : BaseReferenceHolder
        {
            internal RoomInboxUIResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.UIStatus ResponseStatus()
            {
                return C.RealTimeMultiplayerManager_RoomInboxUIResponse_GetStatus(SelfPtr());
            }

            internal MultiplayerInvitation Invitation()
            {
                if (ResponseStatus() != Status.UIStatus.VALID)
                {
                    return null;
                }

                return new MultiplayerInvitation(
                    C.RealTimeMultiplayerManager_RoomInboxUIResponse_GetInvitation(SelfPtr()));
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.RealTimeMultiplayerManager_RoomInboxUIResponse_Dispose(selfPointer);
            }

            internal static RoomInboxUIResponse FromPointer(IntPtr pointer)
            {
                if (PInvokeUtilities.IsNull(pointer))
                {
                    return null;
                }

                return new RoomInboxUIResponse(pointer);
            }

        }

        internal class WaitingRoomUIResponse : BaseReferenceHolder
        {
            internal WaitingRoomUIResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.UIStatus ResponseStatus()
            {
                return C.RealTimeMultiplayerManager_WaitingRoomUIResponse_GetStatus(SelfPtr());
            }

            internal NativeRealTimeRoom Room()
            {
                if (ResponseStatus() != Status.UIStatus.VALID)
                {
                    return null;
                }

                return new NativeRealTimeRoom(
                    C.RealTimeMultiplayerManager_WaitingRoomUIResponse_GetRoom(SelfPtr()));
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.RealTimeMultiplayerManager_WaitingRoomUIResponse_Dispose(selfPointer);
            }

            internal static WaitingRoomUIResponse FromPointer(IntPtr pointer)
            {
                if (PInvokeUtilities.IsNull(pointer))
                {
                    return null;
                }

                return new WaitingRoomUIResponse(pointer);
            }
        }


        internal class FetchInvitationsResponse : BaseReferenceHolder
        {
            internal FetchInvitationsResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            internal Status.ResponseStatus ResponseStatus()
            {
                return C.RealTimeMultiplayerManager_FetchInvitationsResponse_GetStatus(SelfPtr());
            }

            internal IEnumerable<MultiplayerInvitation> Invitations()
            {
                return PInvokeUtilities.ToEnumerable(
                    C.RealTimeMultiplayerManager_FetchInvitationsResponse_GetInvitations_Length(
                        SelfPtr()),
                    index => new MultiplayerInvitation(
                        C.RealTimeMultiplayerManager_FetchInvitationsResponse_GetInvitations_GetElement(
                            SelfPtr(), index))
                );
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.RealTimeMultiplayerManager_FetchInvitationsResponse_Dispose(selfPointer);
            }

            internal static FetchInvitationsResponse FromPointer(IntPtr pointer)
            {
                if (PInvokeUtilities.IsNull(pointer))
                {
                    return null;
                }

                return new FetchInvitationsResponse(pointer);
            }
        }

    }
}


#endif
