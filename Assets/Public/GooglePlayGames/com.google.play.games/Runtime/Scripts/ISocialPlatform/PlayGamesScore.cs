// <copyright file="PlayGamesScore.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using System;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Represents a score on a Google Play Games leaderboard. Implements the Unity
    /// <c>IScore</c> interface.
    /// </summary>
    public class PlayGamesScore : IScore
    {
        /// <summary>
        /// The ID of the leaderboard this score belongs to.
        /// </summary>
        private string mLbId = null;

        /// <summary>
        /// The numerical value of the score.
        /// </summary>
        private long mValue = 0;

        /// <summary>
        /// The rank of this score on the leaderboard.
        /// </summary>
        private ulong mRank = 0;

        /// <summary>
        /// The ID of the player who achieved this score.
        /// </summary>
        private string mPlayerId = string.Empty;

        /// <summary>
        /// The metadata associated with this score (also known as a score tag).
        /// </summary>
        private string mMetadata = string.Empty;

        /// <summary>
        /// The date and time when the score was achieved.
        /// </summary>
        private DateTime mDate = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesScore"/> class.
        /// </summary>
        /// <param name="date">The date the score was achieved.</param>
        /// <param name="leaderboardId">The leaderboard ID.</param>
        /// <param name="rank">The rank of the score.</param>
        /// <param name="playerId">The player's ID.</param>
        /// <param name="value">The numerical score value.</param>
        /// <param name="metadata">The metadata (score tag) associated with the score.</param>
        internal PlayGamesScore(DateTime date, string leaderboardId,
            ulong rank, string playerId, ulong value, string metadata)
        {
            this.mDate = date;
            mLbId = leaderboardID;
            this.mRank = rank;
            this.mPlayerId = playerId;
            this.mValue = (long) value;
            this.mMetadata = metadata;
        }

        /// <summary>
        /// Reports this score to the Google Play Games services. This is equivalent
        /// to calling <see cref="PlayGamesPlatform.ReportScore" />.
        /// </summary>
        /// <param name="callback">A callback to be invoked with a boolean indicating the success of the operation.</param>
        public void ReportScore(Action<bool> callback)
        {
            PlayGamesPlatform.Instance.ReportScore(mValue, mLbId, mMetadata, callback);
        }

        /// <summary>
        /// Gets or sets the ID of the leaderboard this score is for.
        /// </summary>
        public string leaderboardID
        {
            get { return mLbId; }
            set { mLbId = value; }
        }

        /// <summary>
        /// Gets or sets the score value.
        /// </summary>
        public long value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        /// <summary>
        /// Gets the date and time this score was achieved.
        /// </summary>
        public DateTime date
        {
            get { return mDate; }
        }

        /// <summary>
        /// Gets the score value as a formatted string.
        /// </summary>
        public string formattedValue
        {
            get { return mValue.ToString(); }
        }

        /// <summary>
        /// Gets the ID of the user who achieved this score.
        /// </summary>
        public string userID
        {
            get { return mPlayerId; }
        }

        /// <summary>
        /// Gets the rank of this score in the leaderboard.
        /// </summary>
        public int rank
        {
            get { return (int) mRank; }
        }

        /// <summary>
        /// Gets the metadata associated with this score (also known as a score tag).
        /// </summary>
        public string metaData
        {
            get { return mMetadata; }
        }
    }
}
#endif