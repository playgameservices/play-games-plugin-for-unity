// <copyright file="SavedGameMetadataUpdate.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi.SavedGame
{
    using System;
    using GooglePlayGames.OurUtils;

    /// <summary>
    /// A struct representing the mutation of saved game metadata. Fields can either have a new value
    /// or be untouched (in which case the corresponding field in the saved game metadata will be
    /// untouched). Instances must be built using <see cref="SavedGameMetadataUpdate.Builder"/>
    /// and once created, these instances are immutable and threadsafe.
    /// </summary>
    public struct SavedGameMetadataUpdate
    {
        private readonly bool mDescriptionUpdated;
        private readonly string mNewDescription;
        private readonly bool mCoverImageUpdated;
        private readonly byte[] mNewPngCoverImage;
        private readonly TimeSpan? mNewPlayedTime;

        private SavedGameMetadataUpdate(Builder builder)
        {
            mDescriptionUpdated = builder.mDescriptionUpdated;
            mNewDescription = builder.mNewDescription;
            mCoverImageUpdated = builder.mCoverImageUpdated;
            mNewPngCoverImage = builder.mNewPngCoverImage;
            mNewPlayedTime = builder.mNewPlayedTime;
        }

        public bool IsDescriptionUpdated
        {
            get
            {
                return mDescriptionUpdated;
            }
        }

        public string UpdatedDescription
        {
            get
            {
                return mNewDescription;
            }
        }

        public bool IsCoverImageUpdated
        {
            get
            {
                return mCoverImageUpdated;
            }
        }

        public byte[] UpdatedPngCoverImage
        {
            get
            {
                return mNewPngCoverImage;
            }
        }

        public bool IsPlayedTimeUpdated
        {
            get
            {
                return mNewPlayedTime.HasValue;
            }
        }

        public TimeSpan? UpdatedPlayedTime
        {
            get
            {
                return mNewPlayedTime;
            }
        }

        public struct Builder
        {
            internal bool mDescriptionUpdated;
            internal string mNewDescription;
            internal bool mCoverImageUpdated;
            internal byte[] mNewPngCoverImage;
            internal TimeSpan? mNewPlayedTime;

            public Builder WithUpdatedDescription(string description)
            {
                mNewDescription = Misc.CheckNotNull(description);
                mDescriptionUpdated = true;
                return this;
            }

            public Builder WithUpdatedPngCoverImage(byte[] newPngCoverImage)
            {
                mCoverImageUpdated = true;
                mNewPngCoverImage = newPngCoverImage;
                return this;
            }

            public Builder WithUpdatedPlayedTime(TimeSpan newPlayedTime)
            {
                if (newPlayedTime.TotalMilliseconds > ulong.MaxValue)
                {
                    throw new InvalidOperationException("Timespans longer than ulong.MaxValue " +
                        "milliseconds are not allowed");
                }

                mNewPlayedTime = newPlayedTime;
                return this;
            }

            public SavedGameMetadataUpdate Build()
            {
                return new SavedGameMetadataUpdate(this);
            }
        }
    }
}
