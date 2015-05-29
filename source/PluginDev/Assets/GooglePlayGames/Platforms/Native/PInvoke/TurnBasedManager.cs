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
using GooglePlayGames.Native.PInvoke;
using System.Runtime.InteropServices;
using GooglePlayGames.OurUtils;
using System.Collections.Generic;
using GooglePlayGames.Native.Cwrapper;

using C = GooglePlayGames.Native.Cwrapper.TurnBasedMultiplayerManager;
using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
using MultiplayerStatus = GooglePlayGames.Native.Cwrapper.CommonErrorStatus.MultiplayerStatus;

namespace GooglePlayGames.Native.PInvoke {
internal class TurnBasedManager {
    private readonly GameServices mGameServices;

    internal TurnBasedManager(GameServices services) {
        this.mGameServices = services;
    }

    internal delegate void TurnBasedMatchCallback(TurnBasedMatchResponse response);

    internal void GetMatch(string matchId, Action<TurnBasedMatchResponse> callback) {
        C.TurnBasedMultiplayerManager_FetchMatch(mGameServices.AsHandle(),
            matchId, InternalTurnBasedMatchCallback,
            ToCallbackPointer(callback));
    }

    [AOT.MonoPInvokeCallback(typeof(C.TurnBasedMatchCallback))]
    internal static void InternalTurnBasedMatchCallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "TurnBasedManager#InternalTurnBasedMatchCallback",
            Callbacks.Type.Temporary, response, data);
    }

    internal void CreateMatch(TurnBasedMatchConfig config,
        Action<TurnBasedMatchResponse> callback) {
        C.TurnBasedMultiplayerManager_CreateTurnBasedMatch(mGameServices.AsHandle(),
            config.AsPointer(), InternalTurnBasedMatchCallback,
            ToCallbackPointer(callback));
    }

    internal void ShowPlayerSelectUI(uint minimumPlayers, uint maxiumPlayers,
        bool allowAutomatching, Action<PlayerSelectUIResponse> callback) {
        C.TurnBasedMultiplayerManager_ShowPlayerSelectUI(mGameServices.AsHandle(), minimumPlayers,
            maxiumPlayers, allowAutomatching, InternalPlayerSelectUIcallback,
            Callbacks.ToIntPtr(callback, PlayerSelectUIResponse.FromPointer));

    }

    [AOT.MonoPInvokeCallback(typeof(C.PlayerSelectUICallback))]
    internal static void InternalPlayerSelectUIcallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "TurnBasedManager#PlayerSelectUICallback", Callbacks.Type.Temporary, response, data);
    }

    internal void GetAllTurnbasedMatches(Action<TurnBasedMatchesResponse> callback) {
        C.TurnBasedMultiplayerManager_FetchMatches(mGameServices.AsHandle(),
            InternalTurnBasedMatchesCallback,
            Callbacks.ToIntPtr<TurnBasedMatchesResponse>(
                callback, TurnBasedMatchesResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.TurnBasedMatchesCallback))]
    internal static void InternalTurnBasedMatchesCallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "TurnBasedManager#TurnBasedMatchesCallback", Callbacks.Type.Temporary, response, data);
    }

    internal void AcceptInvitation(MultiplayerInvitation invitation,
        Action<TurnBasedMatchResponse> callback) {
        Logger.d("Accepting invitation: " + invitation.AsPointer().ToInt64());
        C.TurnBasedMultiplayerManager_AcceptInvitation(mGameServices.AsHandle(),
            invitation.AsPointer(), InternalTurnBasedMatchCallback, ToCallbackPointer(callback));
    }

    internal void DeclineInvitation(MultiplayerInvitation invitation) {
        C.TurnBasedMultiplayerManager_DeclineInvitation(mGameServices.AsHandle(),
            invitation.AsPointer());
    }

    internal void TakeTurn(NativeTurnBasedMatch match, byte[] data,
        MultiplayerParticipant nextParticipant, Action<TurnBasedMatchResponse> callback) {
        C.TurnBasedMultiplayerManager_TakeMyTurn(
            mGameServices.AsHandle(),
            match.AsPointer(),
            data,
            new UIntPtr((uint)data.Length),
            // Just pass along the old results. Technically the API allows updates here, but
            // we never need them.
            match.Results().AsPointer(),
            nextParticipant.AsPointer(),
            InternalTurnBasedMatchCallback,
            ToCallbackPointer(callback));
    }

    [AOT.MonoPInvokeCallback(typeof(C.MatchInboxUICallback))]
    internal static void InternalMatchInboxUICallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "TurnBasedManager#MatchInboxUICallback", Callbacks.Type.Temporary, response, data);
    }

    internal void ShowInboxUI(Action<MatchInboxUIResponse> callback) {
        C.TurnBasedMultiplayerManager_ShowMatchInboxUI(mGameServices.AsHandle(),
            InternalMatchInboxUICallback,
            Callbacks.ToIntPtr<MatchInboxUIResponse>(callback, MatchInboxUIResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.MultiplayerStatusCallback))]
    internal static void InternalMultiplayerStatusCallback(MultiplayerStatus status, IntPtr data) {
        Logger.d("InternalMultiplayerStatusCallback: " + status);
        var callback = Callbacks.IntPtrToTempCallback<Action<MultiplayerStatus>>(data);

        try {
            callback(status);
        } catch (Exception e) {
            Logger.e("Error encountered executing InternalMultiplayerStatusCallback. " +
                "Smothering to avoid passing exception into Native: " + e);
        }
    }

    internal void LeaveDuringMyTurn(NativeTurnBasedMatch match,
        MultiplayerParticipant nextParticipant, Action<MultiplayerStatus> callback) {
        C.TurnBasedMultiplayerManager_LeaveMatchDuringMyTurn(
            mGameServices.AsHandle(),
            match.AsPointer(),
            nextParticipant.AsPointer(),
            InternalMultiplayerStatusCallback,
            Callbacks.ToIntPtr(callback)
        );
    }

    internal void FinishMatchDuringMyTurn(NativeTurnBasedMatch match, byte[] data,
        ParticipantResults results, Action<TurnBasedMatchResponse> callback) {
        C.TurnBasedMultiplayerManager_FinishMatchDuringMyTurn(
            mGameServices.AsHandle(),
            match.AsPointer(),
            data,
            new UIntPtr((uint)data.Length),
            results.AsPointer(),
            InternalTurnBasedMatchCallback,
            ToCallbackPointer(callback)
        );
    }

    internal void ConfirmPendingCompletion(NativeTurnBasedMatch match,
        Action<TurnBasedMatchResponse> callback) {
        C.TurnBasedMultiplayerManager_ConfirmPendingCompletion(
            mGameServices.AsHandle(),
            match.AsPointer(),
            InternalTurnBasedMatchCallback,
            ToCallbackPointer(callback));
    }

    internal void LeaveMatchDuringTheirTurn(NativeTurnBasedMatch match,
        Action<MultiplayerStatus> callback) {
        C.TurnBasedMultiplayerManager_LeaveMatchDuringTheirTurn(
            mGameServices.AsHandle(),
            match.AsPointer(),
            InternalMultiplayerStatusCallback,
            Callbacks.ToIntPtr(callback));
    }

    internal void CancelMatch(NativeTurnBasedMatch match,
        Action<MultiplayerStatus> callback) {
        C.TurnBasedMultiplayerManager_CancelMatch(
            mGameServices.AsHandle(),
            match.AsPointer(),
            InternalMultiplayerStatusCallback,
            Callbacks.ToIntPtr(callback));
    }

    internal void Rematch(NativeTurnBasedMatch match,
        Action<TurnBasedMatchResponse> callback) {
        C.TurnBasedMultiplayerManager_Rematch(
            mGameServices.AsHandle(),
            match.AsPointer(),
            InternalTurnBasedMatchCallback,
            ToCallbackPointer(callback));
    }

    private static IntPtr ToCallbackPointer(Action<TurnBasedMatchResponse> callback) {
        return Callbacks.ToIntPtr<TurnBasedMatchResponse>(
            callback,
            TurnBasedMatchResponse.FromPointer
        );
    }

    internal class MatchInboxUIResponse : BaseReferenceHolder {
        internal MatchInboxUIResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        internal CommonErrorStatus.UIStatus UiStatus() {
            return C.TurnBasedMultiplayerManager_MatchInboxUIResponse_GetStatus(SelfPtr());
        }

        internal NativeTurnBasedMatch Match() {
            if (UiStatus() != CommonErrorStatus.UIStatus.VALID) {
                return null;
            }

            return new NativeTurnBasedMatch(
                C.TurnBasedMultiplayerManager_MatchInboxUIResponse_GetMatch(SelfPtr()));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.TurnBasedMultiplayerManager_MatchInboxUIResponse_Dispose(selfPointer);
        }

        internal static MatchInboxUIResponse FromPointer(IntPtr pointer) {
            if (pointer.Equals(IntPtr.Zero)) {
                return null;
            }

            return new MatchInboxUIResponse(pointer);
        }
    }

    internal class TurnBasedMatchResponse : BaseReferenceHolder {
        internal TurnBasedMatchResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        internal CommonErrorStatus.MultiplayerStatus ResponseStatus() {
            return C.TurnBasedMultiplayerManager_TurnBasedMatchResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded() {
            return ResponseStatus() > 0;
        }

        internal NativeTurnBasedMatch Match() {
            if (!RequestSucceeded()) {
                return null;
            }

            return new NativeTurnBasedMatch(
                C.TurnBasedMultiplayerManager_TurnBasedMatchResponse_GetMatch(SelfPtr()));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.TurnBasedMultiplayerManager_TurnBasedMatchResponse_Dispose(selfPointer);
        }

        internal static TurnBasedMatchResponse FromPointer(IntPtr pointer) {
            if (pointer.Equals(IntPtr.Zero)) {
                return null;
            }

            return new TurnBasedMatchResponse(pointer);
        }
    }

    internal class TurnBasedMatchesResponse : BaseReferenceHolder {
        internal TurnBasedMatchesResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.TurnBasedMultiplayerManager_TurnBasedMatchesResponse_Dispose(SelfPtr());
        }

        internal CommonErrorStatus.MultiplayerStatus Status() {
            return C.TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetStatus(SelfPtr());
        }

        internal IEnumerable<MultiplayerInvitation> Invitations() {
            return PInvokeUtilities.ToEnumerable(
                C.TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetInvitations_Length(SelfPtr()),
                index => new MultiplayerInvitation(C.TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetInvitations_GetElement(SelfPtr(), index)));
        }

        internal static TurnBasedMatchesResponse FromPointer(IntPtr pointer) {
            if (PInvokeUtilities.IsNull(pointer)) {
                return null;
            }

            return new TurnBasedMatchesResponse(pointer);
        }
    }
}
}


#endif
