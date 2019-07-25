// <copyright file="MatchOutcome.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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

namespace GooglePlayGames.BasicApi.Multiplayer
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the outcome of a match (who won, who lost...).
    /// </summary>
    public class MatchOutcome
    {
        public enum ParticipantResult
        {
            Unset = -1,
            None = 0,
            Win = 1,
            Loss = 2,
            Tie = 3
        }

        public const uint PlacementUnset = 0;

        private List<string> mParticipantIds = new List<string>();
        private Dictionary<string, uint> mPlacements = new Dictionary<string, uint>();

        private Dictionary<string, ParticipantResult> mResults =
            new Dictionary<string, ParticipantResult>();

        public MatchOutcome()
        {
        }

        /// <summary>
        /// Sets the result for a given participant. A result is whether they won,
        /// lost or tied and, optionally, their placement (1st, 2nd, ...).
        /// </summary>
        /// <param name="participantId">Participant identifier.</param>
        /// <param name="result">Result. May be Win, Loss, Tie or None.</param>
        /// <param name="placement">Placement. Use 0 for unset, 1 for 1st, 2 for 2nd, etc.</param>
        public void SetParticipantResult(string participantId, ParticipantResult result,
            uint placement)
        {
            if (!mParticipantIds.Contains(participantId))
            {
                mParticipantIds.Add(participantId);
            }

            mPlacements[participantId] = placement;
            mResults[participantId] = result;
        }

        /// <summary>
        /// <see cref="SetParticipantResult(string,ParticipantResult,uint)"/>
        /// </summary>
        public void SetParticipantResult(string participantId, ParticipantResult result)
        {
            SetParticipantResult(participantId, result, PlacementUnset);
        }

        /// <summary>
        /// <see cref="SetParticipantResult(string,ParticipantResult,uint)"/>
        /// </summary>
        public void SetParticipantResult(string participantId, uint placement)
        {
            SetParticipantResult(participantId, ParticipantResult.Unset, placement);
        }

        /// <summary>
        /// Returns a list of the participant IDs in this match outcome.
        /// </summary>
        /// <value>The participant ids.</value>
        public List<string> ParticipantIds
        {
            get { return mParticipantIds; }
        }

        /// Returns the result for the given participant ID.
        public ParticipantResult GetResultFor(string participantId)
        {
            return mResults.ContainsKey(participantId) ? mResults[participantId] : ParticipantResult.Unset;
        }

        /// Returns the placement for the given participant ID.
        public uint GetPlacementFor(string participantId)
        {
            return mPlacements.ContainsKey(participantId) ? mPlacements[participantId] : PlacementUnset;
        }

        public override string ToString()
        {
            string s = "[MatchOutcome";
            foreach (string pid in mParticipantIds)
            {
                s += string.Format(" {0}->({1},{2})", pid,
                    GetResultFor(pid), GetPlacementFor(pid));
            }

            return s + "]";
        }
    }
}
#endif