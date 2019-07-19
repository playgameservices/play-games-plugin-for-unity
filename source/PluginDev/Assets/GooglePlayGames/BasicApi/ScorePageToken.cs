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
    public enum ScorePageDirection
    {
        Forward = 1,
        Backward = 2,
    }

    /// <summary>
    /// Score page token. This holds the internal token used
    /// to page through the score pages.  The id, collection, and
    /// timespan are added as a convience, and not actually part of the
    /// page token returned from the SDK.
    /// </summary>
    public class ScorePageToken
    {
        private string mId;
        private object mInternalObject;
        private LeaderboardCollection mCollection;
        private LeaderboardTimeSpan mTimespan;
        private ScorePageDirection mDirection;

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

        public LeaderboardCollection Collection
        {
            get { return mCollection; }
        }

        public LeaderboardTimeSpan TimeSpan
        {
            get { return mTimespan; }
        }

        public ScorePageDirection Direction
        {
            get { return mDirection; }
        }

        public string LeaderboardId
        {
            get { return mId; }
        }

        internal object InternalObject
        {
            get { return mInternalObject; }
        }
    }
}
#endif