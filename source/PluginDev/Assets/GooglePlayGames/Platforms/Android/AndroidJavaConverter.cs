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
    using Com.Google.Android.Gms.Common.Api;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames.BasicApi.Multiplayer;
    using OurUtils;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    internal class AndroidJavaConverter
    {
        // Convert to LeaderboardVariant.java#TimeSpan
        internal static int ToLeaderboardVariantTimeSpan(LeaderboardTimeSpan span) 
        {
            switch(span)
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
            switch(collection)
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
            switch(direction)
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
            switch(invitationTypeJava)
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
            switch(participantStatusJava)
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
    }
}
#endif
