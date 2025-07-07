// <copyright file="PlayGamesAchievement.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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
    using System;
    using GooglePlayGames.BasicApi;
    using UnityEngine;
#if UNITY_2017_1_OR_NEWER
    using UnityEngine.Networking;
#endif
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Delegate for reporting achievement progress.
    /// </summary>
    /// <param name="id">The achievement ID.</param>
    /// <param name="progress">The progress of the achievement (a value between 0.0 and 100.0).</param>
    /// <param name="callback">A callback to be invoked with a boolean indicating success.</param>
    internal delegate void ReportProgress(string id, double progress, Action<bool> callback);

    /// <summary>
    /// Represents a Google Play Games achievement. It can be used to report an achievement
    /// to the API, offering identical functionality as <see cref="PlayGamesPlatform.ReportProgress" />.
    /// Implements both the <see cref="IAchievement"/> and <see cref="IAchievementDescription"/> interfaces.
    /// </summary>
    internal class PlayGamesAchievement : IAchievement, IAchievementDescription
    {
        /// <summary>
        /// The callback for reporting progress.
        /// </summary>
        private readonly ReportProgress mProgressCallback;

        /// <summary>
        /// The achievement's ID.
        /// </summary>
        private string mId = string.Empty;

        /// <summary>
        /// A flag indicating if the achievement is incremental.
        /// </summary>
        private bool mIsIncremental = false;

        /// <summary>
        /// The current steps completed for an incremental achievement.
        /// </summary>
        private int mCurrentSteps = 0;

        /// <summary>
        /// The total steps required for an incremental achievement.
        /// </summary>
        private int mTotalSteps = 0;

        /// <summary>
        /// The percentage of completion.
        /// </summary>
        private double mPercentComplete = 0.0;

        /// <summary>
        /// A flag indicating if the achievement is completed (unlocked).
        /// </summary>
        private bool mCompleted = false;

        /// <summary>
        /// A flag indicating if the achievement is hidden.
        /// </summary>
        private bool mHidden = false;

        /// <summary>
        /// The last time the achievement was modified.
        /// </summary>
        private DateTime mLastModifiedTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// The title of the achievement.
        /// </summary>
        private string mTitle = string.Empty;

        /// <summary>
        /// The URL for the revealed (locked) achievement image.
        /// </summary>
        private string mRevealedImageUrl = string.Empty;

        /// <summary>
        /// The URL for the unlocked achievement image.
        /// </summary>
        private string mUnlockedImageUrl = string.Empty;
#if UNITY_2017_1_OR_NEWER
        /// <summary>
        /// The web request used to fetch the achievement image.
        /// </summary>
        private UnityWebRequest mImageFetcher = null;
#else
        /// <summary>
        /// The web request used to fetch the achievement image.
        /// </summary>
        private WWW mImageFetcher = null;
#endif
        /// <summary>
        /// The downloaded achievement image as a Texture2D.
        /// </summary>
        private Texture2D mImage = null;

        /// <summary>
        /// The description of the achievement.
        /// </summary>
        private string mDescription = string.Empty;

        /// <summary>
        /// The points awarded for unlocking the achievement.
        /// </summary>
        private ulong mPoints = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesAchievement"/> class.
        /// Uses the default progress reporting mechanism from <see cref="PlayGamesPlatform"/>.
        /// </summary>
        internal PlayGamesAchievement()
            : this(PlayGamesPlatform.Instance.ReportProgress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesAchievement"/> class with a custom progress callback.
        /// </summary>
        /// <param name="progressCallback">The callback to use for reporting progress.</param>
        internal PlayGamesAchievement(ReportProgress progressCallback)
        {
            mProgressCallback = progressCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesAchievement"/> class from a <see cref="BasicApi.Achievement"/> object.
        /// </summary>
        /// <param name="ach">The achievement data from the Basic API.</param>
        internal PlayGamesAchievement(Achievement ach) : this()
        {
            this.mId = ach.Id;
            this.mIsIncremental = ach.IsIncremental;
            this.mCurrentSteps = ach.CurrentSteps;
            this.mTotalSteps = ach.TotalSteps;
            if (ach.IsIncremental)
            {
                if (ach.TotalSteps > 0)
                {
                    this.mPercentComplete =
                        ((double) ach.CurrentSteps / (double) ach.TotalSteps) * 100.0;
                }
                else
                {
                    this.mPercentComplete = 0.0;
                }
            }
            else
            {
                this.mPercentComplete = ach.IsUnlocked ? 100.0 : 0.0;
            }

            this.mCompleted = ach.IsUnlocked;
            this.mHidden = !ach.IsRevealed;
            this.mLastModifiedTime = ach.LastModifiedTime;
            this.mTitle = ach.Name;
            this.mDescription = ach.Description;
            this.mPoints = ach.Points;
            this.mRevealedImageUrl = ach.RevealedImageUrl;
            this.mUnlockedImageUrl = ach.UnlockedImageUrl;
        }

        /// <summary>
        /// Reveals, unlocks or increments the achievement.
        /// </summary>
        /// <remarks>
        /// This method is equivalent to calling <see cref="PlayGamesPlatform.ReportProgress" />.
        /// The <see cref="id"/> and <see cref="percentCompleted"/> properties should be set before calling this method.
        /// </remarks>
        /// <param name="callback">An action to be invoked with a value indicating whether the operation was successful.</param>
        public void ReportProgress(Action<bool> callback)
        {
            mProgressCallback.Invoke(mId, mPercentComplete, callback);
        }

        /// <summary>
        /// Asynchronously loads the achievement's image from its URL.
        /// </summary>
        /// <returns>The <see cref="Texture2D"/> image once loaded; otherwise, <c>null</c>.</returns>
        private Texture2D LoadImage()
        {
            if (hidden)
            {
                // Return null, as hidden achievements do not have images.
                return null;
            }

            string url = completed ? mUnlockedImageUrl : mRevealedImageUrl;

            // The URL can be null if no image is configured.
            if (!string.IsNullOrEmpty(url))
            {
                if (mImageFetcher == null || mImageFetcher.url != url)
                {
#if UNITY_2017_1_OR_NEWER
                    mImageFetcher = UnityWebRequestTexture.GetTexture(url);
#else
                    mImageFetcher = new WWW(url);
#endif
                    mImage = null;
                }

                // If we already have the texture, return it to avoid repeated downloads.
                if (mImage != null)
                {
                    return mImage;
                }

                if (mImageFetcher.isDone)
                {
#if UNITY_2017_1_OR_NEWER
                    mImage = DownloadHandlerTexture.GetContent(mImageFetcher);
#else
                    mImage = mImageFetcher.texture;
#endif
                    return mImage;
                }
            }

            // If there is no URL or the download is not complete, return null.
            return null;
        }


        /// <summary>
        /// Gets or sets the ID of this achievement.
        /// </summary>
        /// <value>The achievement ID.</value>
        public string id
        {
            get { return mId; }
            set { mId = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this achievement is incremental.
        /// </summary>
        /// <remarks>This value is set by <see cref="PlayGamesPlatform.LoadAchievements"/>.</remarks>
        /// <value><c>true</c> if incremental; otherwise, <c>false</c>.</value>
        public bool isIncremental
        {
            get { return mIsIncremental; }
        }

        /// <summary>
        /// Gets the current number of steps completed for this achievement.
        /// </summary>
        /// <remarks>This value is only defined for incremental achievements and is set by <see cref="PlayGamesPlatform.LoadAchievements"/>.</remarks>
        /// <value>The current steps.</value>
        public int currentSteps
        {
            get { return mCurrentSteps; }
        }

        /// <summary>
        /// Gets the total number of steps for this achievement.
        /// </summary>
        /// <remarks>This value is only defined for incremental achievements and is set by <see cref="PlayGamesPlatform.LoadAchievements"/>.</remarks>
        /// <value>The total steps.</value>
        public int totalSteps
        {
            get { return mTotalSteps; }
        }

        /// <summary>
        /// Gets or sets the completion percentage of this achievement.
        /// </summary>
        /// <value>The percent completed (from 0.0 to 100.0).</value>
        public double percentCompleted
        {
            get { return mPercentComplete; }
            set { mPercentComplete = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this achievement is completed (unlocked).
        /// </summary>
        /// <remarks>This value is set by <see cref="PlayGamesPlatform.LoadAchievements"/>.</remarks>
        /// <value><c>true</c> if completed; otherwise, <c>false</c>.</value>
        public bool completed
        {
            get { return this.mCompleted; }
        }

        /// <summary>
        /// Gets a value indicating whether this achievement is hidden.
        /// </summary>
        /// <remarks>This value is set by <see cref="PlayGamesPlatform.LoadAchievements"/>.</remarks>
        /// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
        public bool hidden
        {
            get { return this.mHidden; }
        }

        /// <summary>
        /// Gets the date and time this achievement was last reported.
        /// </summary>
        /// <value>The last reported date.</value>
        public DateTime lastReportedDate
        {
            get { return mLastModifiedTime; }
        }

        /// <summary>
        /// Gets the title of the achievement.
        /// </summary>
        /// <value>The title.</value>
        public String title
        {
            get { return mTitle; }
        }

        /// <summary>
        /// Gets the image for the achievement, loading it asynchronously if necessary.
        /// </summary>
        /// <value>The achievement image as a <see cref="Texture2D"/>.</value>
        public Texture2D image
        {
            get { return LoadImage(); }
        }

        /// <summary>
        /// Gets the description for the achieved state.
        /// </summary>
        /// <value>The achieved description.</value>
        public string achievedDescription
        {
            get { return mDescription; }
        }

        /// <summary>
        /// Gets the description for the unachieved state.
        /// </summary>
        /// <value>The unachieved description.</value>
        public string unachievedDescription
        {
            get { return mDescription; }
        }

        /// <summary>
        /// Gets the point value of the achievement.
        /// </summary>
        /// <value>The points.</value>
        public int points
        {
            get { return (int) mPoints; }
        }
    }
}
#endif