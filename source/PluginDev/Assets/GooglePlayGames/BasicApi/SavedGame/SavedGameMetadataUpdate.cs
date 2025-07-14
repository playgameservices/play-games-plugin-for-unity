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
    /// <remarks>
    /// @deprecated This struct will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
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

        /// <summary>
        /// Returns true if the description was updated.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public bool IsDescriptionUpdated
        {
            get { return mDescriptionUpdated; }
        }

        /// <summary>
        /// Returns the new description.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string UpdatedDescription
        {
            get { return mNewDescription; }
        }

        /// <summary>
        /// Returns true if the cover image was updated.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public bool IsCoverImageUpdated
        {
            get { return mCoverImageUpdated; }
        }

        public byte[] UpdatedPngCoverImage
        {
            get { return mNewPngCoverImage; }
        }

        /// <summary>
        /// Returns true if the played time was updated.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public bool IsPlayedTimeUpdated
        {
            get { return mNewPlayedTime.HasValue; }
        }

        /// <summary>
        /// Returns the new played time.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public TimeSpan? UpdatedPlayedTime
        {
            get { return mNewPlayedTime; }
        }

        /// <summary>
        /// Builder for <see cref="SavedGameMetadataUpdate"/>.
        /// </summary>
        /// <remarks>
        /// @deprecated This struct will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public struct Builder
        {
            internal bool mDescriptionUpdated;
            internal string mNewDescription;
            internal bool mCoverImageUpdated;
            internal byte[] mNewPngCoverImage;
            internal TimeSpan? mNewPlayedTime;

            /// <summary>
            /// Updates the description of the saved game.
            /// </summary>
            /// <remarks>
            /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            public Builder WithUpdatedDescription(string description)
            {
                mNewDescription = Misc.CheckNotNull(description);
                mDescriptionUpdated = true;
                return this;
            }

            /// <summary>
            /// Updates the cover image of the saved game.
            /// </summary>
            /// <remarks>
            /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            public Builder WithUpdatedPngCoverImage(byte[] newPngCoverImage)
            {
                mCoverImageUpdated = true;
                mNewPngCoverImage = newPngCoverImage;
                return this;
            }

            /// <summary>
            /// Updates the played time of the saved game.
            /// </summary>
            /// <remarks>
            /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
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

            /// <summary>
            /// Builds the <see cref="SavedGameMetadataUpdate"/>.
            /// </summary>
            /// <remarks>
            /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
            /// </remarks>
            public SavedGameMetadataUpdate Build()
            {
                return new SavedGameMetadataUpdate(this);
            }
        }
    }
}