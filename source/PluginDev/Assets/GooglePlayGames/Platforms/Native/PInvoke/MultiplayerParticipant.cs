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
using GooglePlayGames.Native.PInvoke;
using System.Runtime.InteropServices;
using GooglePlayGames.OurUtils;
using System.Collections.Generic;
using GooglePlayGames.Native.Cwrapper;

using C = GooglePlayGames.Native.Cwrapper.MultiplayerParticipant;
using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.Native.PInvoke {
internal class MultiplayerParticipant : BaseReferenceHolder {

    private static readonly
        Dictionary<Types.ParticipantStatus, Participant.ParticipantStatus> StatusConversion =
            new Dictionary<Types.ParticipantStatus, Participant.ParticipantStatus> {
        {Types.ParticipantStatus.INVITED, Participant.ParticipantStatus.Invited},
        {Types.ParticipantStatus.JOINED, Participant.ParticipantStatus.Joined},
        {Types.ParticipantStatus.DECLINED, Participant.ParticipantStatus.Declined},
        {Types.ParticipantStatus.LEFT, Participant.ParticipantStatus.Left},
        {Types.ParticipantStatus.NOT_INVITED_YET, Participant.ParticipantStatus.NotInvitedYet},
        {Types.ParticipantStatus.FINISHED, Participant.ParticipantStatus.Finished},
        {Types.ParticipantStatus.UNRESPONSIVE, Participant.ParticipantStatus.Unresponsive},
    };

    internal MultiplayerParticipant(IntPtr selfPointer) : base(selfPointer) {
    }

    internal Types.ParticipantStatus Status() {
        return C.MultiplayerParticipant_Status(SelfPtr());
    }

    internal bool IsConnectedToRoom() {
        return C.MultiplayerParticipant_IsConnectedToRoom(SelfPtr());
    }

    internal string DisplayName() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, size) => C.MultiplayerParticipant_DisplayName(SelfPtr(), out_string, size)
        );
    }

    internal NativePlayer Player() {
        if (!C.MultiplayerParticipant_HasPlayer(SelfPtr())) {
            return null;
        }

        return new NativePlayer(C.MultiplayerParticipant_Player(SelfPtr()));
    }

    internal string Id() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, size) => C.MultiplayerParticipant_Id(SelfPtr(), out_string, size));
    }

    internal bool Valid() {
        return C.MultiplayerParticipant_Valid(SelfPtr());
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.MultiplayerParticipant_Dispose(selfPointer);
    }

    internal Participant AsParticipant() {
        NativePlayer nativePlayer = Player();

        return new Participant(
            DisplayName(),
            Id(),
            StatusConversion[Status()],
            nativePlayer == null ? null : nativePlayer.AsPlayer(),
            IsConnectedToRoom()
        );
    }

    internal static MultiplayerParticipant FromPointer(IntPtr pointer) {
        if (PInvokeUtilities.IsNull(pointer)) {
            return null;
        }

        return new MultiplayerParticipant(pointer);
    }

    internal static MultiplayerParticipant AutomatchingSentinel() {
        return new MultiplayerParticipant(Sentinels.Sentinels_AutomatchingParticipant());
    }
}
}

#endif
