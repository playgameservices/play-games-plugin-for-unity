// <copyright file="PlayGamesLeaderboard.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Represents a Google Play Games leaderboard. The class provides a way to configure and store
    /// data for a specific leaderboard.
    /// Implements Unity's generic <c>ILeaderboard</c> interface.
    /// </summary>
    public class PlayGamesLeaderboard : ILeaderboard
    {
        /// <summary>
        /// The ID of the leaderboard.
        /// </summary>
        private string mId;

        /// <summary>
        /// The user scope for the leaderboard scores. For example, determines if scores are fetched
        /// from all players (Global) or just the user's friends (FriendsOnly).
        /// </summary>
        private UserScope mUserScope;

        /// <summary>
        /// Specifies the start rank and the number of scores to retrieve.
        /// </summary>
        private Range mRange;

        /// <summary>
        /// Filters scores by time period. For example, AllTime, Weekly, Daily.
        /// </summary>
        private TimeScope mTimeScope;

        /// <summary>
        /// An array of user IDs to filter the scores.
        /// </summary>
        private string[] mFilteredUserIds;

        /// <summary>
        /// A boolean flag that is <c>true</c> while the scores are being loaded; otherwise, <c>false</c>.
        /// </summary>
        private bool mLoading;

        /// <summary>
        /// The score of the local user.
        /// </summary>
        private IScore mLocalUserScore;

        /// <summary>
        /// The approximate total number of scores in the leaderboard.
        /// </summary>
        private uint mMaxRange;

        /// <summary>
        /// The list of loaded scores.
        /// </summary>
        private List<PlayGamesScore> mScoreList = new List<PlayGamesScore>();

        /// <summary>
        /// The title of the leaderboard.
        /// </summary>
        private string mTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesLeaderboard"/> class.
        /// </summary>
        /// <param name="id">The leaderboard ID.</param>
        public PlayGamesLeaderboard(string id)
        {
            mId = id;
        }

        #region ILeaderboard implementation

        /// <summary>
        /// Sets a filter to load scores only for a specific set of users.
        /// </summary>
        /// <param name="userIDs">The array of user IDs to filter by.</param>
        public void SetUserFilter(string[] userIDs)
        {
            mFilteredUserIds = userIDs;
        }

        /// <summary>
        /// Initiates the loading of scores from the Google Play Games platform.
        /// </summary>
        /// <param name="callback">A callback that will be invoked with a boolean indicating the success of the operation.</param>
        public void LoadScores(System.Action<bool> callback)
        {
            PlayGamesPlatform.Instance.LoadScores(this, callback);
        }

        /// <summary>
        /// Gets a value indicating whether the leaderboard scores are currently loading.
        /// </summary>
        /// <value><c>true</c> if loading; otherwise, <c>false</c>.</value>
        public bool loading
        {
            get { return mLoading; }
            internal set { mLoading = value; }
        }

        /// <summary>
        /// Gets or sets the leaderboard ID.
        /// </summary>
        /// <value>The leaderboard ID.</value>
        public string id
        {
            get { return mId; }
            set { mId = value; }
        }

        /// <summary>
        /// Gets or sets the user scope for the scores to be loaded.
        /// </summary>
        /// <value>The user scope.</value>
        public UserScope userScope
        {
            get { return mUserScope; }
            set { mUserScope = value; }
        }

        /// <summary>
        /// Gets or sets the rank range for the scores to be loaded.
        /// </summary>
        /// <value>The rank range.</value>
        public Range range
        {
            get { return mRange; }
            set { mRange = value; }
        }

        /// <summary>
        /// Gets or sets the time scope for the scores to be loaded.
        /// </summary>
        /// <value>The time scope.</value>
        public TimeScope timeScope
        {
            get { return mTimeScope; }
            set { mTimeScope = value; }
        }

        /// <summary>
        /// Gets the local user's score on this leaderboard.
        /// </summary>
        /// <value>The local user's score.</value>
        public IScore localUserScore
        {
            get { return mLocalUserScore; }
        }

        /// <summary>
        /// Gets the approximate number of total scores in the leaderboard.
        /// </summary>
        /// <value>The maximum range of scores.</value>
        public uint maxRange
        {
            get { return mMaxRange; }
        }

        /// <summary>
        /// Gets the array of loaded scores.
        /// </summary>
        /// <value>The scores.</value>
        public IScore[] scores
        {
            get
            {
                PlayGamesScore[] arr = new PlayGamesScore[mScoreList.Count];
                mScoreList.CopyTo(arr);
                return arr;
            }
        }

        /// <summary>
        /// Gets the title of the leaderboard.
        /// </summary>
        /// <value>The title.</value>
        public string title
        {
            get { return mTitle; }
        }

        #endregion

        /// <summary>
        /// Populates the leaderboard's properties from a <see cref="LeaderboardScoreData"/> object.
        /// </summary>
        /// <param name="data">The data object containing leaderboard information.</param>
        /// <returns><c>true</c> if the data was valid and applied; otherwise, <c>false</c>.</returns>
        internal bool SetFromData(LeaderboardScoreData data)
        {
            if (data.Valid)
            {
                OurUtils.Logger.d("Setting leaderboard from: " + data);
                SetMaxRange(data.ApproximateCount);
                SetTitle(data.Title);
                SetLocalUserScore((PlayGamesScore) data.PlayerScore);
                foreach (IScore score in data.Scores)
                {
                    AddScore((PlayGamesScore) score);
                }

                mLoading = data.Scores.Length == 0 || HasAllScores();
            }

            return data.Valid;
        }

        /// <summary>
        /// Sets the maximum range (approximate total number of scores).
        /// </summary>
        /// <param name="val">The value for the maximum range.</param>
        internal void SetMaxRange(ulong val)
        {
            mMaxRange = (uint) val;
        }

        /// <summary>
        /// Sets the title of the leaderboard.
        /// </summary>
        /// <param name="value">The title string.</param>
        internal void SetTitle(string value)
        {
            mTitle = value;
        }

        /// <summary>
        /// Sets the local user's score.
        /// </summary>
        /// <param name="score">The local user's score.</param>
        internal void SetLocalUserScore(PlayGamesScore score)
        {
            mLocalUserScore = score;
        }

        /// <summary>
        /// Adds a score to the internal list of scores. If a user filter is active,
        /// the score will only be added if the user ID matches the filter.
        /// </summary>
        /// <param name="score">The score to add.</param>
        /// <returns>The new count of scores in the list.</returns>
        internal int AddScore(PlayGamesScore score)
        {
            if (mFilteredUserIds == null || mFilteredUserIds.Length == 0)
            {
                mScoreList.Add(score);
            }
            else
            {
                foreach (string fid in mFilteredUserIds)
                {
                    if (fid.Equals(score.userID))
                    {
                        mScoreList.Add(score);
                        break;
                    }
                }
            }

            return mScoreList.Count;
        }

        /// <summary>
        /// Gets the number of scores currently loaded.
        /// </summary>
        /// <value>The score count.</value>
        public int ScoreCount
        {
            get { return mScoreList.Count; }
        }

        /// <summary>
        /// Checks if all requested scores have been loaded.
        /// </summary>
        /// <returns><c>true</c> if the number of loaded scores matches the requested range or the total number of scores; otherwise, <c>false</c>.</returns>
        internal bool HasAllScores()
        {
            return mScoreList.Count >= mRange.count || mScoreList.Count >= maxRange;
        }
    }
}
#endif