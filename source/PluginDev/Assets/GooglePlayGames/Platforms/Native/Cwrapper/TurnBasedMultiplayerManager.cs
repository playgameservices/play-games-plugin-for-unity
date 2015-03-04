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
#if (UNITY_ANDROID || UNITY_IPHONE)
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GooglePlayGames.Native.Cwrapper {
internal static class TurnBasedMultiplayerManager {
    internal delegate void TurnBasedMatchCallback(
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void MultiplayerStatusCallback(
         /* from(MultiplayerStatus_t) */ CommonErrorStatus.MultiplayerStatus arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void TurnBasedMatchesCallback(
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchesResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void MatchInboxUICallback(
         /* from(TurnBasedMultiplayerManager_MatchInboxUIResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void PlayerSelectUICallback(
         /* from(TurnBasedMultiplayerManager_PlayerSelectUIResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_ShowPlayerSelectUI(
        HandleRef self,
         /* from(uint32_t) */ uint minimum_players,
         /* from(uint32_t) */ uint maximum_players,
        [MarshalAs(UnmanagedType.I1)] /* from(bool) */ bool allow_automatch,
         /* from(TurnBasedMultiplayerManager_PlayerSelectUICallback_t) */ PlayerSelectUICallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_CancelMatch(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match,
         /* from(TurnBasedMultiplayerManager_MultiplayerStatusCallback_t) */ MultiplayerStatusCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_DismissMatch(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_ShowMatchInboxUI(
        HandleRef self,
         /* from(TurnBasedMultiplayerManager_MatchInboxUICallback_t) */ MatchInboxUICallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_SynchronizeData(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_Rematch(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchCallback_t) */ TurnBasedMatchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_DismissInvitation(
        HandleRef self,
         /* from(MultiplayerInvitation_t) */ IntPtr invitation);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_FetchMatch(
        HandleRef self,
         /* from(char const *) */ string match_id,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchCallback_t) */ TurnBasedMatchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_DeclineInvitation(
        HandleRef self,
         /* from(MultiplayerInvitation_t) */ IntPtr invitation);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_FinishMatchDuringMyTurn(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match,
         /* from(uint8_t const *) */ byte[] match_data,
         /* from(size_t) */ UIntPtr match_data_size,
         /* from(ParticipantResults_t) */ IntPtr results,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchCallback_t) */ TurnBasedMatchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_FetchMatches(
        HandleRef self,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchesCallback_t) */ TurnBasedMatchesCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_CreateTurnBasedMatch(
        HandleRef self,
         /* from(TurnBasedMatchConfig_t) */ IntPtr config,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchCallback_t) */ TurnBasedMatchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_AcceptInvitation(
        HandleRef self,
         /* from(MultiplayerInvitation_t) */ IntPtr invitation,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchCallback_t) */ TurnBasedMatchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_TakeMyTurn(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match,
         /* from(uint8_t const *) */ byte[] match_data,
         /* from(size_t) */ UIntPtr match_data_size,
         /* from(ParticipantResults_t) */ IntPtr results,
         /* from(MultiplayerParticipant_t) */ IntPtr next_participant,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchCallback_t) */ TurnBasedMatchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_ConfirmPendingCompletion(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match,
         /* from(TurnBasedMultiplayerManager_TurnBasedMatchCallback_t) */ TurnBasedMatchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_LeaveMatchDuringMyTurn(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match,
         /* from(MultiplayerParticipant_t) */ IntPtr next_participant,
         /* from(TurnBasedMultiplayerManager_MultiplayerStatusCallback_t) */ MultiplayerStatusCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_LeaveMatchDuringTheirTurn(
        HandleRef self,
         /* from(TurnBasedMatch_t) */ IntPtr match,
         /* from(TurnBasedMultiplayerManager_MultiplayerStatusCallback_t) */ MultiplayerStatusCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_TurnBasedMatchResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(MultiplayerStatus_t) */ CommonErrorStatus.MultiplayerStatus TurnBasedMultiplayerManager_TurnBasedMatchResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(TurnBasedMatch_t) */ IntPtr TurnBasedMultiplayerManager_TurnBasedMatchResponse_GetMatch(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_TurnBasedMatchesResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(MultiplayerStatus_t) */ CommonErrorStatus.MultiplayerStatus TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetInvitations_Length(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(MultiplayerInvitation_t) */ IntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetInvitations_GetElement(
        HandleRef self,
         /* from(size_t) */ UIntPtr index);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetMyTurnMatches_Length(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(TurnBasedMatch_t) */ IntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetMyTurnMatches_GetElement(
        HandleRef self,
         /* from(size_t) */ UIntPtr index);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetTheirTurnMatches_Length(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(TurnBasedMatch_t) */ IntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetTheirTurnMatches_GetElement(
        HandleRef self,
         /* from(size_t) */ UIntPtr index);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetCompletedMatches_Length(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(TurnBasedMatch_t) */ IntPtr TurnBasedMultiplayerManager_TurnBasedMatchesResponse_GetCompletedMatches_GetElement(
        HandleRef self,
         /* from(size_t) */ UIntPtr index);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_MatchInboxUIResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(UIStatus_t) */ CommonErrorStatus.UIStatus TurnBasedMultiplayerManager_MatchInboxUIResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(TurnBasedMatch_t) */ IntPtr TurnBasedMultiplayerManager_MatchInboxUIResponse_GetMatch(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void TurnBasedMultiplayerManager_PlayerSelectUIResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(UIStatus_t) */ CommonErrorStatus.UIStatus TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetPlayerIds_Length(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetPlayerIds_GetElement(
        HandleRef self,
         /* from(size_t) */ UIntPtr index,
         /* from(char *) */ StringBuilder out_arg,
         /* from(size_t) */ UIntPtr out_size);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(uint32_t) */ uint TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetMinimumAutomatchingPlayers(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(uint32_t) */ uint TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetMaximumAutomatchingPlayers(
        HandleRef self);
}
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
