// <copyright file="Achievement.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi
{
    using System;

    /// <summary>
    /// Achievement.
    /// Represents an achievement that can be unlocked at once or incrementally.
    /// </summary>
    public class Achievement
    {
        static readonly DateTime UnixEpoch =
                new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private string mId = string.Empty;
        private bool mIsIncremental = false;
        private bool mIsRevealed = false;
        private bool mIsUnlocked = false;
        private int mCurrentSteps = 0;
        private int mTotalSteps = 0;
        private string mDescription = string.Empty;
        private string mName = string.Empty;
        private long mLastModifiedTime = 0;
        private ulong mPoints;
        private string mRevealedImageUrl;
        private string mUnlockedImageUrl;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="GooglePlayGames.BasicApi.Achievement"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="GooglePlayGames.BasicApi.Achievement"/>.</returns>
        public override string ToString()
        {
            return string.Format(
                "[Achievement] id={0}, name={1}, desc={2}, type={3}, revealed={4}, unlocked={5}, steps={6}/{7}",
                mId, mName, mDescription, mIsIncremental ? "INCREMENTAL" : "STANDARD",
                mIsRevealed, mIsUnlocked, mCurrentSteps, mTotalSteps);
        }

        public Achievement()
        {
        }

        public bool IsIncremental
        {
            get
            {
                return mIsIncremental;
            }

            set
            {
                mIsIncremental = value;
            }
        }

        public int CurrentSteps
        {
            get
            {
                return mCurrentSteps;
            }

            set
            {
                mCurrentSteps = value;
            }
        }

        public int TotalSteps
        {
            get
            {
                return mTotalSteps;
            }

            set
            {
                mTotalSteps = value;
            }
        }

        public bool IsUnlocked
        {
            get
            {
                return mIsUnlocked;
            }

            set
            {
               mIsUnlocked = value;
            }
        }

        public bool IsRevealed
        {
            get
            {
                return mIsRevealed;
            }

            set
            {
                mIsRevealed = value;
            }
        }

        public string Id
        {
            get
            {
                return mId;
            }

            set
            {
                mId = value;
            }
        }

        public string Description
        {
            get
            {
                return this.mDescription;
            }

            set
            {
                mDescription = value;
            }
        }

        public string Name
        {
            get
            {
                return this.mName;
            }

            set
            {
                mName = value;
            }
        }

        public DateTime LastModifiedTime
        {
            get
            {
                return UnixEpoch.AddMilliseconds(mLastModifiedTime);
            }

            set
            {
                TimeSpan ts = value - UnixEpoch;
                mLastModifiedTime = (long)ts.TotalMilliseconds;
            }
        }

        public ulong Points
        {
            get
            {
                return mPoints;
            }

            set
            {
                mPoints = value;
            }
        }

        public string RevealedImageUrl
        {
            get
            {
                return mRevealedImageUrl;
            }

            set
            {
                mRevealedImageUrl = value;
            }
        }

        public string UnlockedImageUrl
        {
            get
            {
                return mUnlockedImageUrl;
            }

            set
            {
                mUnlockedImageUrl = value;
            }
        }
    }
}
