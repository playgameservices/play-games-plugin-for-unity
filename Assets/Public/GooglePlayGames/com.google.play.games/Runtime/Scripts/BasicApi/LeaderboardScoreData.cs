// <copyright file="LeaderboardScoreData.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
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

namespace GooglePlayGames.BasicApi
{
    using System.Collections.Generic;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Leaderboard score data. This is the callback data
    /// when loading leaderboard scores.  There are several SDK
    /// API calls needed to be made to collect all the required data,
    /// so this class is used to simplify the response.
    /// </summary>
    public class LeaderboardScoreData
    {
        private string mId;
        private ResponseStatus mStatus;
        private ulong mApproxCount;
        private string mTitle;
        private IScore mPlayerScore;
        private ScorePageToken mPrevPage;
        private ScorePageToken mNextPage;
        private List<PlayGamesScore> mScores = new List<PlayGamesScore>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardScoreData"/> class.
        /// </summary>
        /// <param name="leaderboardId">The identifier of the leaderboard.</param>
        internal LeaderboardScoreData(string leaderboardId)
        {
            mId = leaderboardId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardScoreData"/> class with a specified status.
        /// </summary>
        /// <param name="leaderboardId">The identifier of the leaderboard.</param>
        /// <param name="status">The response status of the leaderboard data.</param>
        internal LeaderboardScoreData(string leaderboardId, ResponseStatus status)
        {
            mId = leaderboardId;
            mStatus = status;
        }

        /// <summary>
        /// Gets a value indicating whether the leaderboard data is valid.
        /// </summary>
        public bool Valid
        {
            get
            {
                return mStatus == ResponseStatus.Success ||
                       mStatus == ResponseStatus.SuccessWithStale;
            }
        }

        /// <summary>
        /// Gets or sets the status of the leaderboard data response.
        /// </summary>
        public ResponseStatus Status
        {
            get { return mStatus; }

            internal set { mStatus = value; }
        }

        /// <summary>
        /// Gets or sets the approximate count of scores in the leaderboard.
        /// </summary>
        public ulong ApproximateCount
        {
            get { return mApproxCount; }

            internal set { mApproxCount = value; }
        }

        /// <summary>
        /// Gets or sets the title of the leaderboard.
        /// </summary>
        public string Title
        {
            get { return mTitle; }

            internal set { mTitle = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier of the leaderboard.
        /// </summary>
        public string Id
        {
            get { return mId; }

            internal set { mId = value; }
        }

        /// <summary>
        /// Gets or sets the player's score in the leaderboard.
        /// </summary>
        public IScore PlayerScore
        {
            get { return mPlayerScore; }

            internal set { mPlayerScore = value; }
        }

        /// <summary>
        /// Gets an array of the scores in the leaderboard.
        /// </summary>
        public IScore[] Scores
        {
            get { return mScores.ToArray(); }
        }

        /// <summary>
        /// Adds a score to the leaderboard data.
        /// </summary>
        /// <param name="score">The score to add.</param>
        /// <returns>The count of scores after the addition.</returns>
        internal int AddScore(PlayGamesScore score)
        {
            mScores.Add(score);
            return mScores.Count;
        }

        /// <summary>
        /// Gets or sets the token for the previous page of scores.
        /// </summary>
        public ScorePageToken PrevPageToken
        {
            get { return mPrevPage; }

            internal set { mPrevPage = value; }
        }

        /// <summary>
        /// Gets or sets the token for the next page of scores.
        /// </summary>
        public ScorePageToken NextPageToken
        {
            get { return mNextPage; }

            internal set { mNextPage = value; }
        }

        /// <summary>
        /// Returns a string representation of the leaderboard score data.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("[LeaderboardScoreData: mId={0}, " +
                                 " mStatus={1}, mApproxCount={2}, mTitle={3}]",
                mId, mStatus, mApproxCount, mTitle);
        }
    }
}
#endif
