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

    public class PlayGamesLeaderboard : ILeaderboard
    {
        private string mId;
        private UserScope mUserScope;
        private Range mRange;
        private TimeScope mTimeScope;
        private string[] mFilteredUserIds;
        private bool mLoading;

        private IScore mLocalUserScore;
        private uint mMaxRange;
        private List<PlayGamesScore> mScoreList = new List<PlayGamesScore>();
        private string mTitle;

        public PlayGamesLeaderboard(string id)
        {
            mId = id;
        }

        #region ILeaderboard implementation

        public void SetUserFilter(string[] userIDs)
        {
            mFilteredUserIds = userIDs;
        }

        public void LoadScores(System.Action<bool> callback)
        {
            PlayGamesPlatform.Instance.LoadScores(this, callback);
        }

        public bool loading
        {
            get { return mLoading; }
            internal set { mLoading = value; }
        }

        public string id
        {
            get { return mId; }
            set { mId = value; }
        }

        public UserScope userScope
        {
            get { return mUserScope; }
            set { mUserScope = value; }
        }

        public Range range
        {
            get { return mRange; }
            set { mRange = value; }
        }

        public TimeScope timeScope
        {
            get { return mTimeScope; }
            set { mTimeScope = value; }
        }

        public IScore localUserScore
        {
            get { return mLocalUserScore; }
        }

        public uint maxRange
        {
            get { return mMaxRange; }
        }

        public IScore[] scores
        {
            get
            {
                PlayGamesScore[] arr = new PlayGamesScore[mScoreList.Count];
                mScoreList.CopyTo(arr);
                return arr;
            }
        }

        public string title
        {
            get { return mTitle; }
        }

        #endregion

        internal bool SetFromData(LeaderboardScoreData data)
        {
            if (data.Valid)
            {
                Debug.Log("Setting leaderboard from: " + data);
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

        internal void SetMaxRange(ulong val)
        {
            mMaxRange = (uint) val;
        }

        internal void SetTitle(string value)
        {
            mTitle = value;
        }

        internal void SetLocalUserScore(PlayGamesScore score)
        {
            mLocalUserScore = score;
        }

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
                        return mScoreList.Count;
                    }
                }

                mScoreList.Add(score);
            }

            return mScoreList.Count;
        }

        public int ScoreCount
        {
            get { return mScoreList.Count; }
        }

        internal bool HasAllScores()
        {
            return mScoreList.Count >= mRange.count || mScoreList.Count >= maxRange;
        }
    }
}
#endif