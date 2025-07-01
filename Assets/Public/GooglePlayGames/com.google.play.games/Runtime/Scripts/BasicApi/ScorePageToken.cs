// <copyright file="ScorePageToken.cs" company="Google Inc.">
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
    /// <summary>
    /// Enum representing the direction of score page navigation.
    /// </summary>
    public enum ScorePageDirection
    {
        /// <summary>
        /// Represents the forward direction (next page).
        /// </summary>
        Forward = 1,

        /// <summary>
        /// Represents the backward direction (previous page).
        /// </summary>
        Backward = 2,
    }

    /// <summary>
    /// Score page token. This holds the internal token used
    /// to page through the score pages. The id, collection, and
    /// timespan are added as a convenience, and not actually part of the
    /// page token returned from the SDK.
    /// </summary>
    public class ScorePageToken
    {
        private string mId;
        private object mInternalObject;
        private LeaderboardCollection mCollection;
        private LeaderboardTimeSpan mTimespan;
        private ScorePageDirection mDirection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScorePageToken"/> class.
        /// </summary>
        /// <param name="internalObject">The internal object representing the page token.</param>
        /// <param name="id">The leaderboard ID.</param>
        /// <param name="collection">The leaderboard collection type. For example, public or social.</param>
        /// <param name="timespan">The timespan of the leaderboard. For example, daily or all-time.</param>
        /// <param name="direction">The direction of the score page navigation, such as forward or backward.</param>
        internal ScorePageToken(object internalObject, string id,
            LeaderboardCollection collection, LeaderboardTimeSpan timespan,
            ScorePageDirection direction)
        {
            mInternalObject = internalObject;
            mId = id;
            mCollection = collection;
            mTimespan = timespan;
            mDirection = direction;
        }

        /// <summary>
        /// Gets the collection type of the leaderboard. For example, public or social.
        /// </summary>
        public LeaderboardCollection Collection
        {
            get { return mCollection; }
        }

        /// <summary>
        /// Gets the timespan of the leaderboard. For example, daily or all-time.
        /// </summary>
        public LeaderboardTimeSpan TimeSpan
        {
            get { return mTimespan; }
        }

        /// <summary>
        /// Gets the direction of the score page navigation. For example, forward or backward.
        /// </summary>
        public ScorePageDirection Direction
        {
            get { return mDirection; }
        }

        /// <summary>
        /// Gets the leaderboard ID associated with this token.
        /// </summary>
        public string LeaderboardId
        {
            get { return mId; }
        }

        /// <summary>
        /// Gets the internal object representing the page token.
        /// This is an internal implementation detail and should not be accessed directly.
        /// </summary>
        internal object InternalObject
        {
            get { return mInternalObject; }
        }
    }
}
#endif
