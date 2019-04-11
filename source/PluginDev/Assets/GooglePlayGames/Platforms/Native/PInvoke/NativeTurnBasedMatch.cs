// <copyright file="NativeTurnBasedMatch.cs" company="Google Inc.">
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
    using System.Collections.Generic;
    using C = GooglePlayGames.Native.Cwrapper.TurnBasedMatch;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using TBM = GooglePlayGames.BasicApi.Multiplayer.TurnBasedMatch;
    using GooglePlayGames.BasicApi.Multiplayer;

    internal class NativeTurnBasedMatch : BaseReferenceHolder
    {

        internal NativeTurnBasedMatch(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        internal uint AvailableAutomatchSlots()
        {
            return C.TurnBasedMatch_AutomatchingSlotsAvailable(SelfPtr());
        }

        internal ulong CreationTime()
        {
            return C.TurnBasedMatch_CreationTime(SelfPtr());
        }

        internal ulong LastUpdateTime()
        {
            return C.TurnBasedMatch_LastUpdateTime(SelfPtr());
        }

        internal IEnumerable<MultiplayerParticipant> Participants()
        {
            return PInvokeUtilities.ToEnumerable(
                C.TurnBasedMatch_Participants_Length(SelfPtr()),
                (index) => new MultiplayerParticipant(
                    C.TurnBasedMatch_Participants_GetElement(SelfPtr(), index)));
        }

        internal uint Version()
        {
            return C.TurnBasedMatch_Version(SelfPtr());
        }

        internal uint Variant()
        {
            return C.TurnBasedMatch_Variant(SelfPtr());
        }

        internal ParticipantResults Results()
        {
            return new ParticipantResults(C.TurnBasedMatch_ParticipantResults(SelfPtr()));
        }

        internal MultiplayerParticipant ParticipantWithId(string participantId)
        {
            foreach (var participant in Participants())
            {
                if (participant.Id().Equals(participantId))
                {
                    return participant;
                }

                // If this isn't the participant we're looking for, clean it up lest we leak
                // memory.
                participant.Dispose();
            }

            return null;
        }

        internal MultiplayerParticipant PendingParticipant()
        {
            var participant = new MultiplayerParticipant(
                              C.TurnBasedMatch_PendingParticipant(SelfPtr()));

            if (!participant.Valid())
            {
                participant.Dispose();
                return null;
            }

            return participant;
        }

        internal Types.MatchStatus MatchStatus()
        {
            return C.TurnBasedMatch_Status(SelfPtr());
        }

        internal string Description()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_string, size) => C.TurnBasedMatch_Description(SelfPtr(), out_string, size));
        }

        internal bool HasRematchId()
        {
            string rematchId = RematchId();
            return !string.IsNullOrEmpty(rematchId) && !rematchId.Equals("(null)");
        }

        internal string RematchId()
        {
            // There is a bug in C++ for android that always returns true for
            // HasRematchId - so commenting out this optimization until it is
            // fixed.
            //if (!HasRematchId())
            //{
            //    Logger.d("Returning NUll for rematch id since it does not have one");
            //    return null;
            //}

            return PInvokeUtilities.OutParamsToString(
                (out_string, size) => C.TurnBasedMatch_RematchId(SelfPtr(), out_string, size));
        }

        internal byte[] Data()
        {
            if (!C.TurnBasedMatch_HasData(SelfPtr()))
            {
                Logger.d("Match has no data.");
                return null;
            }

            return PInvokeUtilities.OutParamsToArray<byte>(
                (bytes, size) => C.TurnBasedMatch_Data(SelfPtr(), bytes, size));
        }

        internal string Id()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_string, size) => C.TurnBasedMatch_Id(SelfPtr(), out_string, size));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.TurnBasedMatch_Dispose(selfPointer);
        }

        internal TBM AsTurnBasedMatch(string selfPlayerId)
        {
            List<Participant> participants = new List<Participant>();
            string selfParticipantId = null;
            string pendingParticipantId = null;

            using (var pending = PendingParticipant())
            {
                if (pending != null)
                {
                    pendingParticipantId = pending.Id();
                }
            }

            foreach (var participant in Participants())
            {
                using (participant)
                {
                    using (var player = participant.Player())
                    {
                        if (player != null && player.Id().Equals(selfPlayerId))
                        {
                            selfParticipantId = participant.Id();
                        }
                    }

                    participants.Add(participant.AsParticipant());
                }
            }

            // A match can be rematched if it's complete, and it hasn't already been rematched.
            bool canRematch = MatchStatus() == Types.MatchStatus.COMPLETED && !HasRematchId();

            return new GooglePlayGames.BasicApi.Multiplayer.TurnBasedMatch(
                Id(),
                Data(),
                canRematch,
                selfParticipantId,
                participants,
                AvailableAutomatchSlots(),
                pendingParticipantId,
                ToTurnStatus(MatchStatus()),
                ToMatchStatus(pendingParticipantId, MatchStatus()),
                Variant(),
                Version(),
                CreationTime(),
                LastUpdateTime()
            );
        }

        private static TBM.MatchTurnStatus ToTurnStatus(Types.MatchStatus status)
        {
            switch (status)
            {
                case Types.MatchStatus.CANCELED:
                    return TBM.MatchTurnStatus.Complete;
                case Types.MatchStatus.COMPLETED:
                    return TBM.MatchTurnStatus.Complete;
                case Types.MatchStatus.EXPIRED:
                    return TBM.MatchTurnStatus.Complete;
                case Types.MatchStatus.INVITED:
                    return TBM.MatchTurnStatus.Invited;
                case Types.MatchStatus.MY_TURN:
                    return TBM.MatchTurnStatus.MyTurn;
                case Types.MatchStatus.PENDING_COMPLETION:
                    return TBM.MatchTurnStatus.Complete;
                case Types.MatchStatus.THEIR_TURN:
                    return TBM.MatchTurnStatus.TheirTurn;
                default:
                    return TBM.MatchTurnStatus.Unknown;
            }
        }

        private static TBM.MatchStatus ToMatchStatus(
            string pendingParticipantId, Types.MatchStatus status)
        {
            switch (status)
            {
                case Types.MatchStatus.CANCELED:
                    return TBM.MatchStatus.Cancelled;
                case Types.MatchStatus.COMPLETED:
                    return TBM.MatchStatus.Complete;
                case Types.MatchStatus.EXPIRED:
                    return TBM.MatchStatus.Expired;
                case Types.MatchStatus.INVITED:
                    return TBM.MatchStatus.Active;
                case Types.MatchStatus.MY_TURN:
                    return TBM.MatchStatus.Active;
                case Types.MatchStatus.PENDING_COMPLETION:
                    return TBM.MatchStatus.Complete;
                case Types.MatchStatus.THEIR_TURN:
                // If it's their turn, but we don't have a valid pending participant, it's because
                // we're still automatching against that participant.
                    return pendingParticipantId == null
                    ? TBM.MatchStatus.AutoMatching
                    : TBM.MatchStatus.Active;
                default:
                    return TBM.MatchStatus.Unknown;
            }
        }

        internal static NativeTurnBasedMatch FromPointer(IntPtr selfPointer)
        {
            if (PInvokeUtilities.IsNull(selfPointer))
            {
                return null;
            }

            return new NativeTurnBasedMatch(selfPointer);
        }
    }
}

#endif
