// <copyright file="MultiplayerInvitation.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
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
    using System.Runtime.InteropServices;
    using GooglePlayGames.OurUtils;
    using C = GooglePlayGames.Native.Cwrapper.MultiplayerInvitation;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using GooglePlayGames.BasicApi.Multiplayer;

    internal class MultiplayerInvitation : BaseReferenceHolder
    {
        internal MultiplayerInvitation(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        internal MultiplayerParticipant Inviter()
        {
            MultiplayerParticipant participant =
                new MultiplayerParticipant(C.MultiplayerInvitation_InvitingParticipant(SelfPtr()));

            if (!participant.Valid())
            {
                participant.Dispose();
                return null;
            }

            return participant;
        }

        internal uint Variant()
        {
            return C.MultiplayerInvitation_Variant(SelfPtr());
        }

        internal ulong CreationTime()
        {
            return C.MultiplayerInvitation_CreationTime(SelfPtr());
        }

        internal Types.MultiplayerInvitationType Type()
        {
            return C.MultiplayerInvitation_Type(SelfPtr());
        }

        internal string Id()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_string, size) => C.MultiplayerInvitation_Id(SelfPtr(), out_string, size)
            );
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.MultiplayerInvitation_Dispose(selfPointer);
        }

        internal uint AutomatchingSlots()
        {
            return C.MultiplayerInvitation_AutomatchingSlotsAvailable(SelfPtr());
        }

        internal uint ParticipantCount()
        {
            return C.MultiplayerInvitation_Participants_Length(SelfPtr()).ToUInt32();
        }

        private static Invitation.InvType ToInvType(Types.MultiplayerInvitationType invitationType)
        {
            switch (invitationType)
            {
                case Types.MultiplayerInvitationType.REAL_TIME:
                    return Invitation.InvType.RealTime;
                case Types.MultiplayerInvitationType.TURN_BASED:
                    return Invitation.InvType.TurnBased;
                default:
                    Logger.d("Found unknown invitation type: " + invitationType);
                    return Invitation.InvType.Unknown;
            }
        }

        internal Invitation AsInvitation()
        {
            var type = ToInvType(Type());
            var invitationId = Id();
            int variant = (int)Variant();
            long creationTime = (long)CreationTime();
            
            Participant inviter;

            using (var nativeInviter = Inviter())
            {
                inviter = nativeInviter == null ? null : nativeInviter.AsParticipant();
            }

            return new Invitation(type, invitationId, inviter, variant, creationTime);
        }

        internal static MultiplayerInvitation FromPointer(IntPtr selfPointer)
        {
            if (PInvokeUtilities.IsNull(selfPointer))
            {
                return null;
            }

            return new MultiplayerInvitation(selfPointer);
        }
    }
}

#endif
