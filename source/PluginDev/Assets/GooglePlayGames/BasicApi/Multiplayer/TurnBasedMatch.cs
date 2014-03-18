/*
 * Copyright (C) 2013 Google Inc.
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

using System;
using System.Collections.Generic;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.Multiplayer {
    /// <summary>
    /// Represents a turn-based match.
    /// </summary>
    public class TurnBasedMatch {
        public enum MatchStatus { Active, AutoMatching, Cancelled, Complete, Expired, Unknown, Deleted};
        public enum MatchTurnStatus { Complete, Invited, MyTurn, TheirTurn, Unknown };

        private string mMatchId;
        private byte[] mData;
        private bool mCanRematch;
        private int mAvailableAutomatchSlots;
        private string mSelfParticipantId;
        private List<Participant> mParticipants;
        private string mPendingParticipantId;
        private MatchTurnStatus mTurnStatus;
        private MatchStatus mMatchStatus;
        private int mVariant;

        internal TurnBasedMatch(string matchId, byte[] data, bool canRematch,
                string selfParticipantId, List<Participant> participants,
                int availableAutomatchSlots,
                string pendingParticipantId, MatchTurnStatus turnStatus, MatchStatus matchStatus,
                int variant) {

            mMatchId = matchId;
            mData = data;
            mCanRematch = canRematch;
            mSelfParticipantId = selfParticipantId;
            mParticipants = participants;
            
            // participant list is always sorted!
            mParticipants.Sort();
            
            mAvailableAutomatchSlots = availableAutomatchSlots;
            mPendingParticipantId = pendingParticipantId;
            mTurnStatus = turnStatus;
            mMatchStatus = matchStatus;
            mVariant = variant;
        }

        /// Match ID.
        public string MatchId {
            get {
                return mMatchId;
            }
        }

        /// The data associated with the match. The meaning of this data is defined by the game.
        public byte[] Data {
            get {
                return mData;
            }
        }

        /// If true, this match can be rematched.
        public bool CanRematch {
            get {
                return mCanRematch;
            }
        }

        /// The participant ID that represents the current player.
        public string SelfParticipantId {
            get {
                return mSelfParticipantId;
            }
        }

        /// The participant that represents the current player in the match.
        public Participant Self {
            get {
                return GetParticipant(mSelfParticipantId);
            }
        }

        /// Gets a participant by ID. Returns null if not found.
        public Participant GetParticipant(string participantId) {
            foreach (Participant p in mParticipants) {
                if (p.ParticipantId.Equals(participantId)) {
                    return p;
                }
            }
            Logger.w("Participant not found in turn-based match: " + participantId);
            return null;
        }

        /// Returns the list of participants. Guaranteed to be sorted by participant ID.
        public List<Participant> Participants {
            get {
                return mParticipants;
            }
        }

        /// Returns the pending participant ID (whose turn it is).
        public string PendingParticipantId {
            get {
                return mPendingParticipantId;
            }
        }

        /// Returns the pending participant (whose turn it is).
        public Participant PendingParticipant {
            get {
                return mPendingParticipantId == null ? null :
                        GetParticipant(mPendingParticipantId);
            }
        }

        /// Returns the turn status (whether it's my turn).
        public MatchTurnStatus TurnStatus {
            get {
                return mTurnStatus;
            }
        }

        /// Returns the status of the match.
        public MatchStatus Status {
            get {
                return mMatchStatus;
            }
        }

        /// Returns the match variant being played. 0 for default.
        public int Variant {
            get {
                return mVariant;
            }
        }

        // Returns how many automatch slots are still open in the match.
        public int AvailableAutomatchSlots {
            get {
                return mAvailableAutomatchSlots;
            }
        }

        public override string ToString() {
            return string.Format("[TurnBasedMatch: mMatchId={0}, mData={1}, mCanRematch={2}, " +
                "mSelfParticipantId={3}, mParticipants={4}, mPendingParticipantId={5}, " +
                "mTurnStatus={6}, mMatchStatus={7}, mVariant={8}]", mMatchId, mData, mCanRematch,
                mSelfParticipantId, mParticipants, mPendingParticipantId, mTurnStatus,
                mMatchStatus, mVariant);
        }
    }
}

