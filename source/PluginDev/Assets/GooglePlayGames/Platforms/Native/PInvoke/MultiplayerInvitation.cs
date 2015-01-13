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

using C = GooglePlayGames.Native.Cwrapper.MultiplayerInvitation;
using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.Native.PInvoke {
internal class MultiplayerInvitation : BaseReferenceHolder {
    internal MultiplayerInvitation(IntPtr selfPointer) : base(selfPointer) {
    }

    internal MultiplayerParticipant Inviter() {
        MultiplayerParticipant participant =
            new MultiplayerParticipant(C.MultiplayerInvitation_InvitingParticipant(SelfPtr()));

        if (!participant.Valid()) {
            participant.Dispose();
            return null;
        }

        return participant;
    }

    internal uint Variant() {
        return C.MultiplayerInvitation_Variant(SelfPtr());
    }

    internal Types.MultiplayerInvitationType Type() {
        return C.MultiplayerInvitation_Type(SelfPtr());
    }

    internal string Id() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, size) => C.MultiplayerInvitation_Id(SelfPtr(), out_string, size)
        );
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.MultiplayerInvitation_Dispose(selfPointer);
    }

    private static Invitation.InvType ToInvType(Types.MultiplayerInvitationType invitationType) {
        switch (invitationType) {
            case Types.MultiplayerInvitationType.REAL_TIME:
                return Invitation.InvType.RealTime;
            case Types.MultiplayerInvitationType.TURN_BASED:
                return Invitation.InvType.TurnBased;
            default:
                Logger.d("Found unknown invitation type: " + invitationType);
                return Invitation.InvType.Unknown;
        }
    }

    internal Invitation AsInvitation() {
        var type = ToInvType(Type());
        var invitationId = Id();
        int variant = (int)Variant();
        Participant inviter;

        using (var nativeInviter = Inviter()) {
            inviter = nativeInviter == null ? null : nativeInviter.AsParticipant();
        }

        return new Invitation(type, invitationId, inviter, variant);
    }

    internal static MultiplayerInvitation FromPointer(IntPtr selfPointer) {
        if (PInvokeUtilities.IsNull(selfPointer)) {
            return null;
        }

        return new MultiplayerInvitation(selfPointer);
    }
}
}

#endif
