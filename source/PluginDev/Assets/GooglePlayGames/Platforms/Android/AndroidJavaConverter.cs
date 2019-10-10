// <copyright file="AndroidTokenClient.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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
//  limitations under the License.
// </copyright>

#if UNITY_ANDROID
namespace GooglePlayGames.Android
{
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames.BasicApi.Multiplayer;
    using OurUtils;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    internal class AndroidJavaConverter
    {
        internal static System.DateTime ToDateTime(long milliseconds)
        {
            System.DateTime result = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            result.AddMilliseconds(milliseconds);
            return result;
        }

        // Convert to LeaderboardVariant.java#TimeSpan
        internal static int ToLeaderboardVariantTimeSpan(LeaderboardTimeSpan span)
        {
            switch (span)
            {
                case LeaderboardTimeSpan.Daily:
                    return 0 /* TIME_SPAN_DAILY */;
                case LeaderboardTimeSpan.Weekly:
                    return 1 /* TIME_SPAN_WEEKLY */;
                case LeaderboardTimeSpan.AllTime:
                default:
                    return 2 /* TIME_SPAN_ALL_TIME */;
            }
        }

        // Convert to LeaderboardVariant.java#Collection
        internal static int ToLeaderboardVariantCollection(LeaderboardCollection collection)
        {
            switch (collection)
            {
                case LeaderboardCollection.Social:
                    return 1 /* COLLECTION_SOCIAL */;
                case LeaderboardCollection.Public:
                default:
                    return 0 /* COLLECTION_PUBLIC */;
            }
        }

        // Convert to PageDirection.java#Direction
        internal static int ToPageDirection(ScorePageDirection direction)
        {
            switch (direction)
            {
                case ScorePageDirection.Forward:
                    return 0 /* NEXT */;
                case ScorePageDirection.Backward:
                    return 1 /* PREV */;
                default:
                    return -1 /* NONE */;
            }
        }

        internal static Invitation.InvType FromInvitationType(int invitationTypeJava)
        {
            switch (invitationTypeJava)
            {
                case 0: // INVITATION_TYPE_REAL_TIME
                    return Invitation.InvType.RealTime;
                case 1: // INVITATION_TYPE_TURN_BASED
                    return Invitation.InvType.TurnBased;
                default:
                    return Invitation.InvType.Unknown;
            }
        }

        internal static Participant.ParticipantStatus FromParticipantStatus(int participantStatusJava)
        {
            switch (participantStatusJava)
            {
                case 0: // STATUS_NOT_INVITED_YET
                    return Participant.ParticipantStatus.NotInvitedYet;
                case 1: // STATUS_INVITED
                    return Participant.ParticipantStatus.Invited;
                case 2: // STATUS_JOINED
                    return Participant.ParticipantStatus.Joined;
                case 3: // STATUS_DECLINED
                    return Participant.ParticipantStatus.Declined;
                case 4: // STATUS_LEFT
                    return Participant.ParticipantStatus.Left;
                case 5: // STATUS_FINISHED
                    return Participant.ParticipantStatus.Finished;
                case 6: // STATUS_UNRESPONSIVE
                    return Participant.ParticipantStatus.Unresponsive;
                default:
                    return Participant.ParticipantStatus.Unknown;
            }
        }

        internal static Participant ToParticipant(AndroidJavaObject participant)
        {
            string displayName = participant.Call<string>("getDisplayName");
            string participantId = participant.Call<string>("getParticipantId");
            Participant.ParticipantStatus status =
                AndroidJavaConverter.FromParticipantStatus(participant.Call<int>("getStatus"));
            bool connectedToRoom = participant.Call<bool>("isConnectedToRoom");
            Player player = null;
            try
            {
                using (var playerObject = participant.Call<AndroidJavaObject>("getPlayer"))
                {
                    player = ToPlayer(playerObject);
                }
            }
            catch (Exception)
            {
                // Unity throws exception for returned null
            }

            return new Participant(displayName, participantId, status, player, connectedToRoom);
        }

        internal static Player ToPlayer(AndroidJavaObject player)
        {
            if (player == null)
            {
                return null;
            }

            string displayName = player.Call<String>("getDisplayName");
            string playerId = player.Call<String>("getPlayerId");
            string avatarUrl = player.Call<String>("getIconImageUrl");
            return new Player(displayName, playerId, avatarUrl);
        }

        internal static Invitation ToInvitation(AndroidJavaObject invitation)
        {
            string invitationId = invitation.Call<string>("getInvitationId");
            int invitationType = invitation.Call<int>("getInvitationType");
            int variant = invitation.Call<int>("getVariant");
            long creationTimestamp = invitation.Call<long>("getCreationTimestamp");
            System.DateTime creationTime = AndroidJavaConverter.ToDateTime(creationTimestamp);
            using (var participant = invitation.Call<AndroidJavaObject>("getInviter"))
            {
                return new Invitation(
                    AndroidJavaConverter.FromInvitationType(invitationType),
                    invitationId,
                    AndroidJavaConverter.ToParticipant(participant),
                    variant,
                    creationTime);
            }
        }

        internal static TurnBasedMatch ToTurnBasedMatch(AndroidJavaObject turnBasedMatch)
        {
            if (turnBasedMatch == null)
            {
                return null;
            }

            string matchId = turnBasedMatch.Call<string>("getMatchId");
            byte[] data = turnBasedMatch.Call<byte[]>("getData");
            bool canRematch = turnBasedMatch.Call<bool>("canRematch");
            uint availableAutomatchSlots = (uint) turnBasedMatch.Call<int>("getAvailableAutoMatchSlots");
            string selfParticipantId = turnBasedMatch.Call<string>("getCreatorId");
            List<Participant> participants = ToParticipantList(turnBasedMatch);
            string pendingParticipantId = turnBasedMatch.Call<string>("getPendingParticipantId");
            TurnBasedMatch.MatchStatus turnStatus = ToMatchStatus(turnBasedMatch.Call<int>("getStatus"));
            TurnBasedMatch.MatchTurnStatus matchStatus =
                AndroidJavaConverter.ToMatchTurnStatus(turnBasedMatch.Call<int>("getTurnStatus"));
            uint variant = (uint) turnBasedMatch.Call<int>("getVariant");
            uint version = (uint) turnBasedMatch.Call<int>("getVersion");
            DateTime creationTime = AndroidJavaConverter.ToDateTime(turnBasedMatch.Call<long>("getCreationTimestamp"));
            DateTime lastUpdateTime =
                AndroidJavaConverter.ToDateTime(turnBasedMatch.Call<long>("getLastUpdatedTimestamp"));

            return new TurnBasedMatch(matchId, data, canRematch, selfParticipantId, participants,
                availableAutomatchSlots, pendingParticipantId, matchStatus, turnStatus, variant, version, creationTime,
                lastUpdateTime);
        }

        internal static List<Participant> ToParticipantList(AndroidJavaObject turnBasedMatch)
        {
            using (var participantsObject = turnBasedMatch.Call<AndroidJavaObject>("getParticipantIds"))
            {
                List<Participant> participants = new List<Participant>();
                int size = participantsObject.Call<int>("size");

                for (int i = 0; i < size; i++)
                {
                    string participantId = participantsObject.Call<string>("get", i);
                    using (var participantObject =
                        turnBasedMatch.Call<AndroidJavaObject>("getParticipant", participantId))
                    {
                        participants.Add(AndroidJavaConverter.ToParticipant(participantObject));
                    }
                }

                return participants;
            }
        }

        internal static List<string> ToStringList(AndroidJavaObject stringList)
        {
            if (stringList == null)
            {
                return new List<string>();
            }

            int size = stringList.Call<int>("size");
            List<string> converted = new List<string>(size);

            for (int i = 0; i < size; i++)
            {
                converted.Add(stringList.Call<string>("get", i));
            }

            return converted;
        }

        // from C#: List<string> to Java: ArrayList<String>
        internal static AndroidJavaObject ToJavaStringList(List<string> list)
        {
            AndroidJavaObject converted = new AndroidJavaObject("java.util.ArrayList");
            for (int i = 0; i < list.Count; i++)
            {
                converted.Call<bool>("add", list[i]);
            }

            return converted;
        }

        internal static TurnBasedMatch.MatchStatus ToMatchStatus(int matchStatus)
        {
            switch (matchStatus)
            {
                case 0: // MATCH_STATUS_AUTO_MATCHING
                    return TurnBasedMatch.MatchStatus.AutoMatching;
                case 1: // MATCH_STATUS_ACTIVE
                    return TurnBasedMatch.MatchStatus.Active;
                case 2: // MATCH_STATUS_COMPLETE
                    return TurnBasedMatch.MatchStatus.Complete;
                case 3: // MATCH_STATUS_EXPIRED
                    return TurnBasedMatch.MatchStatus.Expired;
                case 4: // MATCH_STATUS_CANCELED
                    return TurnBasedMatch.MatchStatus.Cancelled;
                case 5: // MATCH_STATUS_DELETED
                    return TurnBasedMatch.MatchStatus.Deleted;
                default:
                    return TurnBasedMatch.MatchStatus.Unknown;
            }
        }

        internal static TurnBasedMatch.MatchTurnStatus ToMatchTurnStatus(int matchTurnStatus)
        {
            switch (matchTurnStatus)
            {
                case 0: // MATCH_TURN_STATUS_INVITED
                    return TurnBasedMatch.MatchTurnStatus.Invited;
                case 1: // MATCH_TURN_STATUS_MY_TURN
                    return TurnBasedMatch.MatchTurnStatus.MyTurn;
                case 2: // MATCH_TURN_STATUS_THEIR_TURN
                    return TurnBasedMatch.MatchTurnStatus.TheirTurn;
                case 3: // MATCH_TURN_STATUS_COMPLETE
                    return TurnBasedMatch.MatchTurnStatus.Complete;
                default:
                    return TurnBasedMatch.MatchTurnStatus.Unknown;
            }
        }
    }
}
#endif