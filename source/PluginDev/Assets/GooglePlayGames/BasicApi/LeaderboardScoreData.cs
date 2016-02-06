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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

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

        internal LeaderboardScoreData(string leaderboardId)
        {
            mId = leaderboardId;
        }

        internal LeaderboardScoreData(string leaderboardId, ResponseStatus status)
        {
            mId = leaderboardId;
            mStatus = status;
        }

        public bool Valid
        {
            get
            {
                return mStatus == ResponseStatus.Success ||
                mStatus == ResponseStatus.SuccessWithStale;
            }
        }

        public ResponseStatus Status
        {
            get
            {
                return mStatus;
            }

            internal set
            {
                mStatus = value;
            }
        }

        public ulong ApproximateCount
        {
            get
            {
                return mApproxCount;
            }

            internal set
            {
                mApproxCount  = value;
            }
        }

        public string Title
        {
            get
            {
                return mTitle;
            }

            internal set
            {
                mTitle = value;
            }
        }

        public string Id
        {
            get
            {
                return mId;
            }

            internal set
            {
                mId = value;
            }
        }

        public IScore PlayerScore
        {
            get
            {
                return mPlayerScore;
            }

            internal set
            {
                mPlayerScore = value;
            }
        }

        public IScore[] Scores
        {
            get
            {
                return mScores.ToArray();
            }
        }

        internal int AddScore(PlayGamesScore score)
        {
            mScores.Add(score);
            return mScores.Count;
        }

        public ScorePageToken PrevPageToken
        {
            get
            {
                return mPrevPage;
            }

            internal set
            {
                mPrevPage = value;
            }
        }

        public ScorePageToken NextPageToken
        {
            get
            {
                return mNextPage;
            }

            internal set
            {
                mNextPage = value;
            }
        }

        public override string ToString()
        {
            return string.Format("[LeaderboardScoreData: mId={0}, " +
                " mStatus={1}, mApproxCount={2}, mTitle={3}]",
                mId, mStatus, mApproxCount, mTitle);
        }
    }
}
#endif
