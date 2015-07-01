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

namespace GooglePlayGames
{
    using System;
    using GooglePlayGames.BasicApi;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    internal delegate void ReportProgress(string id, double progress, Action<bool> callback);

    /// <summary>
    /// Represents a Google Play Games achievement. It can be used to report an achievement
    /// to the API, offering identical functionality as <see cref="PlayGamesPlatform.ReportProgress" />.
    /// </summary>
    internal class PlayGamesAchievement : IAchievement, IAchievementDescription
    {
        private readonly ReportProgress mProgressCallback;
        private string mId = string.Empty;
        private double mPercentComplete = 0.0f;
        private bool mCompleted = false;
        private bool mHidden = false;
        private DateTime mLastModifiedTime = new DateTime (1970, 1, 1, 0, 0, 0, 0);
        private string mTitle = string.Empty;
        private string mRevealedImageUrl = string.Empty;
        private string mUnlockedImageUrl = string.Empty;
        private WWW mImageFetcher = null;
        private Texture2D mImage = null;
        private string mDescription = string.Empty;
        private ulong mPoints = 0;

        internal PlayGamesAchievement()
            : this(PlayGamesPlatform.Instance.ReportProgress)
        {
        }

        internal PlayGamesAchievement(ReportProgress progressCallback)
        {
            mProgressCallback = progressCallback;
        }

        internal PlayGamesAchievement(Achievement ach) : this()
        {
            this.mId = ach.Id;
            this.mPercentComplete = (double)ach.CurrentSteps / (double) ach.TotalSteps;
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
        /// Reveals, unlocks or increment achievement. Call after setting
        /// <see cref="id" /> and <see cref="percentCompleted" />. Equivalent to calling
        /// <see cref="PlayGamesPlatform.ReportProgress" />.
        /// </summary>
        public void ReportProgress(Action<bool> callback)
        {
            mProgressCallback.Invoke(mId, mPercentComplete, callback);
        }

        /// <summary>
        /// Loads the local user's image from the url.  Loading urls
        /// is asynchronous so the return from this call is fast,
        /// the image is returned once it is loaded.  null is returned
        /// up to that point.
        /// </summary>
        private Texture2D LoadImage()
        {
            if (hidden) {
                // return null, we dont have images for hidden achievements.
                return null;
            }
            string url = completed ? mUnlockedImageUrl : mRevealedImageUrl;

            // the url can be null if the image is not configured.
            if (!string.IsNullOrEmpty(url))
            {
                if (mImageFetcher == null || mImageFetcher.url != url)
                {
                    mImageFetcher = new WWW(url);
                    mImage = null;
                }

                // if we have the texture, just return, this avoids excessive
                // memory usage calling www.texture repeatedly.
                if (mImage != null)
                {
                    return mImage;
                }

                if (mImageFetcher.isDone)
                {
                    mImage = mImageFetcher.texture;
                    return mImage;
                }
            }

            // if there is no url, always return null.
            return null;
        }


        /// <summary>
        /// Gets or sets the id of this achievement.
        /// </summary>
        /// <returns>
        /// The identifier.
        /// </returns>
        public string id
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

        /// <summary>
        /// Gets or sets the percent completed.
        /// </summary>
        /// <returns>
        /// The percent completed.
        /// </returns>
        public double percentCompleted
        {
            get
            {
                return mPercentComplete;
            }

            set
            {
                mPercentComplete = value;
            }
        }

        public bool completed
        {
            get
            {
                return this.mCompleted;
            }
        }

        /// <summary>
        /// Not implemented. Always returns false.
        /// </summary>
        public bool hidden
        {
            get
            {
                return this.mHidden;
            }
        }

        public DateTime lastReportedDate
        {
            get
            {
                return mLastModifiedTime;
            }
        }

        public String title
        {
            get
            {
                return mTitle;
            }
        }
        public Texture2D image
        {
            get
            {
                return LoadImage();
            }
        }

        public string achievedDescription
        {
            get
            {
                return mDescription;
            }
        }

        public string unachievedDescription
        {
            get
            {
                return mDescription;
            }
        }

        public int points
        {
            get
            {
                return (int) mPoints;
            }
        }

    }
}
