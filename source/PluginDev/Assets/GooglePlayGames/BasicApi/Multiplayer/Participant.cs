// <copyright file="Participant.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.BasicApi.Multiplayer
{
    using System;

    /// <summary>
    /// Represents a participant in a real-time or turn-based match. Note the difference
    /// between a Player and a Participant! A Player is a real-world person with a name
    /// and a Google ID. A Participant is an entity that participates in a real-time
    /// or turn-based match; it may be tied to a Player or not. Particularly, Participant
    /// without Players represent the anonymous participants in an automatch game.
    /// </summary>
    public class Participant : IComparable<Participant>
    {
        public enum ParticipantStatus
        {
            NotInvitedYet,
            Invited,
            Joined,
            Declined,
            Left,
            Finished,
            Unresponsive,
            Unknown
        }

        private string mDisplayName = string.Empty;
        private string mParticipantId = string.Empty;
        private ParticipantStatus mStatus = ParticipantStatus.Unknown;
        private Player mPlayer = null;
        private bool mIsConnectedToRoom = false;

        /// Gets the participant's display name.
        public string DisplayName
        {
            get
            {
                return mDisplayName;
            }
        }

        /// <summary>
        /// Gets the participant identifier. Important: everyone in a particular match
        /// agrees on what is the participant ID for each participant; however, participant
        /// IDs are not meaningful outside of the particular match where they are used.
        /// If the same user plays two subsequent matches, their Participant Id will likely
        /// be different in the two matches.
        /// </summary>
        /// <value>The participant identifier.</value>
        public string ParticipantId
        {
            get
            {
                return mParticipantId;
            }
        }

        /// Gets the participant's status (invited, joined, declined, left, finished, ...)
        public ParticipantStatus Status
        {
            get
            {
                return mStatus;
            }
        }

        /// <summary>
        /// Gets the player that corresponds to this participant. If this is an anonymous
        /// participant, this will be null.
        /// </summary>
        /// <value>The player, or null if this is an anonymous participant.</value>
        public Player Player
        {
            get
            {
                return mPlayer;
            }
        }

        /// <summary>
        /// Returns whether the participant is connected to the real time room. This has no
        /// meaning in turn-based matches.
        /// </summary>
        public bool IsConnectedToRoom
        {
            get
            {
                return mIsConnectedToRoom;
            }
        }

        /// Returns whether or not this is an automatch participant.
        public bool IsAutomatch
        {
            get
            {
                return mPlayer == null;
            }
        }

        internal Participant(string displayName, string participantId,
                             ParticipantStatus status, Player player, bool connectedToRoom)
        {
            mDisplayName = displayName;
            mParticipantId = participantId;
            mStatus = status;
            mPlayer = player;
            mIsConnectedToRoom = connectedToRoom;
        }

        public override string ToString()
        {
            return string.Format("[Participant: '{0}' (id {1}), status={2}, " +
                "player={3}, connected={4}]", mDisplayName, mParticipantId, mStatus.ToString(),
                mPlayer == null ? "NULL" : mPlayer.ToString(), mIsConnectedToRoom);
        }

        public int CompareTo(Participant other)
        {
            return mParticipantId.CompareTo(other.mParticipantId);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(Participant))
            {
                return false;
            }

            Participant other = (Participant)obj;
            return mParticipantId.Equals(other.mParticipantId);
        }

        public override int GetHashCode()
        {
            return mParticipantId != null ? mParticipantId.GetHashCode() : 0;
        }
    }
}
#endif
