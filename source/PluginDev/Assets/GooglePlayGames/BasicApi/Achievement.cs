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

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi
{
    using System;

    /// <summary>Data interface for retrieving achievement information.</summary>
    /// <remarks>
    /// There are 3 states an achievement can be in:
    /// <para>
    ///    Hidden - indicating the name and description of the achievement is
    ///     not visible to the player.
    /// </para><para>
    ///    Revealed - indicating the name and description of the achievement is
    ///     visible to the player.
    ///    Unlocked - indicating the player has unlocked, or achieved, the achievment.
    /// </para><para>
    /// Achievements has two types, standard which is unlocked in one step,
    /// and incremental, which require multiple steps to unlock.
    /// </para>
    /// @deprecated This class will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
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
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public override string ToString()
        {
            return string.Format(
                "[Achievement] id={0}, name={1}, desc={2}, type={3}, revealed={4}, unlocked={5}, steps={6}/{7}",
                mId, mName, mDescription, mIsIncremental ? "INCREMENTAL" : "STANDARD",
                mIsRevealed, mIsUnlocked, mCurrentSteps, mTotalSteps);
        }

        /// <summary>
        /// Constructs a new <see cref="GooglePlayGames.BasicApi.Achievement"/>.
        /// </summary>
        /// <remarks>
        /// @deprecated This constructor will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public Achievement()
        {
        }

        /// <summary>
        /// Indicates whether this achievement is incremental.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public bool IsIncremental
        {
            get { return mIsIncremental; }

            set { mIsIncremental = value; }
        }

        /// <summary>
        /// The number of steps the user has gone towards unlocking this achievement.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public int CurrentSteps
        {
            get { return mCurrentSteps; }

            set { mCurrentSteps = value; }
        }

        /// <summary>
        /// The total number of steps needed to unlock this achievement.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public int TotalSteps
        {
            get { return mTotalSteps; }

            set { mTotalSteps = value; }
        }

        /// <summary>
        /// Indicates whether the achievement is unlocked or not.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public bool IsUnlocked
        {
            get { return mIsUnlocked; }

            set { mIsUnlocked = value; }
        }

        /// <summary>
        /// Indicates whether the achievement is revealed or not (hidden).
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public bool IsRevealed
        {
            get { return mIsRevealed; }

            set { mIsRevealed = value; }
        }

        /// <summary>
        /// The ID string of this achievement.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string Id
        {
            get { return mId; }

            set { mId = value; }
        }

        /// <summary>
        /// The description of this achievement.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string Description
        {
            get { return this.mDescription; }

            set { mDescription = value; }
        }

        /// <summary>
        /// The name of this achievement.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string Name
        {
            get { return this.mName; }

            set { mName = value; }
        }

        /// <summary>
        /// The date and time the state of the achievement was modified.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// The value is invalid (-1 long) if the achievement state has
        /// never been updated.
        /// </remarks>
        public DateTime LastModifiedTime
        {
            get { return UnixEpoch.AddMilliseconds(mLastModifiedTime); }

            set
            {
                TimeSpan ts = value - UnixEpoch;
                mLastModifiedTime = (long) ts.TotalMilliseconds;
            }
        }

        /// <summary>
        /// The number of experience points earned for unlocking this Achievement.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public ulong Points
        {
            get { return mPoints; }

            set { mPoints = value; }
        }

        /// <summary>
        /// The URL to the image to display when the achievement is revealed.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string RevealedImageUrl
        {
            get { return mRevealedImageUrl; }

            set { mRevealedImageUrl = value; }
        }

        /// <summary>
        /// The URL to the image to display when the achievement is unlocked.
        /// </summary>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        public string UnlockedImageUrl
        {
            get { return mUnlockedImageUrl; }

            set { mUnlockedImageUrl = value; }
        }
    }
}
#endif