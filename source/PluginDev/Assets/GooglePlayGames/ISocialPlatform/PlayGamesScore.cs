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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames
{
    using System;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Represents a Google Play Games score that can be sent to a leaderboard.
    /// </summary>
    public class PlayGamesScore : IScore
    {
        private string mLbId = null;
        private long mValue = 0;
        private ulong mRank = 0;
        private string mPlayerId = string.Empty;
        private string mMetadata = string.Empty;

        private DateTime mDate = new DateTime(1970, 1, 1, 0, 0, 0);

        internal PlayGamesScore(DateTime date, string leaderboardId,
            ulong rank, string playerId, ulong value, string metadata)
        {
            this.mDate = date;
            mLbId = leaderboardID;
            this.mRank = rank;
            this.mPlayerId = playerId;
            this.mValue = (long)value;
            this.mMetadata = metadata;
        }

        /// <summary>
        /// Reports the score. Equivalent to <see cref="PlayGamesPlatform.ReportScore" />.
        /// </summary>
        public void ReportScore(Action<bool> callback)
        {
            PlayGamesPlatform.Instance.ReportScore(mValue, mLbId, mMetadata, callback);
        }

        /// <summary>
        /// Gets or sets the leaderboard id.
        /// </summary>
        /// <returns>
        /// The leaderboard id.
        /// </returns>
        public string leaderboardID
        {
            get
            {
                return mLbId;
            }

            set
            {
                mLbId = value;
            }
        }

        /// <summary>
        /// Gets or sets the score value.
        /// </summary>
        /// <returns>
        /// The value.
        /// </returns>
        public long value
        {
            get
            {
                return mValue;
            }

            set
            {
                mValue = value;
            }
        }

        /// <summary>
        /// Not implemented. Returns Jan 01, 1970, 00:00:00
        /// </summary>
        public DateTime date
        {
            get
            {
                return mDate;
            }
        }

        /// <summary>
        /// Not implemented. Returns the value converted to a string, unformatted.
        /// </summary>
        public string formattedValue
        {
            get
            {
                return mValue.ToString();
            }
        }

        /// <summary>
        /// Not implemented. Returns the empty string.
        /// </summary>
        public string userID
        {
            get
            {
                return mPlayerId;
            }
        }

        /// <summary>
        /// Not implemented. Returns 1.
        /// </summary>
        public int rank
        {
            get
            {
                return (int)mRank;
            }
        }

        /// <summary>
        /// Gets the metaData (scoreTag).
        /// </summary>
        /// <returns>
        /// The metaData.
        /// </returns>
        public string metaData
        {
            get 
            {
                return mMetadata;
            }
        }
    }
}
#endif
