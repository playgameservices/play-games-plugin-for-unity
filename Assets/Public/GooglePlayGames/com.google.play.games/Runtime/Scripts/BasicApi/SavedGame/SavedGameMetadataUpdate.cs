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

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedGameMetadataUpdate"/> struct using the specified builder.
        /// </summary>
        /// <param name="builder">The builder used to initialize the saved game metadata update.</param>
        private SavedGameMetadataUpdate(Builder builder)
        {
            mDescriptionUpdated = builder.mDescriptionUpdated;
            mNewDescription = builder.mNewDescription;
            mCoverImageUpdated = builder.mCoverImageUpdated;
            mNewPngCoverImage = builder.mNewPngCoverImage;
            mNewPlayedTime = builder.mNewPlayedTime;
        }

        /// <summary>
        /// Gets whether the description has been updated in the metadata.
        /// </summary>
        public bool IsDescriptionUpdated
        {
            get { return mDescriptionUpdated; }
        }

        /// <summary>
        /// Gets the updated description for the saved game, if it has been changed.
        /// </summary>
        public string UpdatedDescription
        {
            get { return mNewDescription; }
        }

        /// <summary>
        /// Gets whether the cover image has been updated in the metadata.
        /// </summary>
        public bool IsCoverImageUpdated
        {
            get { return mCoverImageUpdated; }
        }

        /// <summary>
        /// Gets the updated PNG cover image, if it has been changed.
        /// </summary>
        public byte[] UpdatedPngCoverImage
        {
            get { return mNewPngCoverImage; }
        }

        /// <summary>
        /// Gets whether the played time has been updated in the metadata.
        /// </summary>
        public bool IsPlayedTimeUpdated
        {
            get { return mNewPlayedTime.HasValue; }
        }

        /// <summary>
        /// Gets the updated played time, if it has been changed.
        /// </summary>
        public TimeSpan? UpdatedPlayedTime
        {
            get { return mNewPlayedTime; }
        }

        /// <summary>
        /// A builder for constructing instances of <see cref="SavedGameMetadataUpdate"/>.
        /// </summary>
        public struct Builder
        {
            internal bool mDescriptionUpdated;
            internal string mNewDescription;
            internal bool mCoverImageUpdated;
            internal byte[] mNewPngCoverImage;
            internal TimeSpan? mNewPlayedTime;

            /// <summary>
            /// Sets the description to be updated in the saved game metadata.
            /// </summary>
            /// <param name="description">The new description to set.</param>
            /// <returns>The builder with the updated description.</returns>
            public Builder WithUpdatedDescription(string description)
            {
                mNewDescription = Misc.CheckNotNull(description);
                mDescriptionUpdated = true;
                return this;
            }

            /// <summary>
            /// Sets the PNG cover image to be updated in the saved game metadata.
            /// </summary>
            /// <param name="newPngCoverImage">The new PNG image data for the cover image.</param>
            /// <returns>The builder with the updated cover image.</returns>
            public Builder WithUpdatedPngCoverImage(byte[] newPngCoverImage)
            {
                mCoverImageUpdated = true;
                mNewPngCoverImage = newPngCoverImage;
                return this;
            }

            /// <summary>
            /// Sets the played time to be updated in the saved game metadata.
            /// </summary>
            /// <param name="newPlayedTime">The new played time to set.</param>
            /// <returns>The builder with the updated played time.</returns>
            /// <exception cref="InvalidOperationException">Thrown if the played time exceeds the maximum allowed value.</exception>
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
            /// Builds a new <see cref="SavedGameMetadataUpdate"/> instance with the configured updates.
            /// </summary>
            /// <returns>A new instance of <see cref="SavedGameMetadataUpdate"/>.</returns>
            public SavedGameMetadataUpdate Build()
            {
                return new SavedGameMetadataUpdate(this);
            }
        }
    }
}