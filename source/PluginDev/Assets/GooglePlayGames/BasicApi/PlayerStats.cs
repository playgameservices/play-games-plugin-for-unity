// <copyright file="PlayerStats.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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
    using System;

    /// <summary>
    /// Player stats. See https://developers.google.com/games/services/android/stats
    /// </summary>
    public class PlayerStats
    {
        private static float UNSET_VALUE = -1.0f;

        /// <summary>
        /// If this PlayerStats object is valid (i.e. successfully retrieved from games services).
        /// </summary>
        /// <remarks>
        /// Note that a PlayerStats with all stats unset may still be valid.
        /// </remarks>
        public bool Valid {
            get;
            set;
        }

        /// <summary>
        /// The number of in-app purchases.
        /// </summary>
        public int NumberOfPurchases {
            get;
            set;
        }

        /// <summary>
        /// The length of the avg sesson in minutes.
        /// </summary>
        public float AvgSessonLength {
            get;
            set;
        }

        /// <summary>
        /// The days since last played.
        /// </summary>
        public int DaysSinceLastPlayed {
            get;
            set;
        }

        /// <summary>
        /// The number of sessions based on sign-ins.
        /// </summary>
        public int NumberOfSessions {
            get;
            set;
        }

        /// <summary>
        /// The approximation of sessions percentile for the player.
        /// </summary>
        /// <remarks>
        /// This value is given as a decimal value between 0 and 1 (inclusive).
        /// It indicates how many sessions the current player has
        /// played in comparison to the rest of this game's player base.
        /// Higher numbers indicate that this player has played more sessions.
        /// A return value less than zero indicates this value is not available.
        /// </remarks>
        public float SessPercentile {
            get;
            set;
        }

        /// <summary>
        /// The approximate spend percentile of the player.
        /// </summary>
        /// <remarks>
        /// This value is given as a decimal value between 0 and 1 (inclusive).
        /// It indicates how much the current player has spent in
        /// comparison to the rest of this game's player base. Higher
        /// numbers indicate that this player has spent more.
        /// A return value less than zero indicates this value is not available.
        /// </remarks>
        public float SpendPercentile {
            get;
            set;
        }

        /// <summary>
        /// The approximate probability of the player not returning to play the game.
        /// </summary>
        /// <remarks>
        /// Higher values indicate that a player is less likely to return.
        /// A return value less than zero indicates this value is not available.
        /// </remarks>
        public float ChurnProbability {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GooglePlayGames.BasicApi.PlayerStats"/> class.
        /// Sets all values to -1.
        /// </summary>
        public PlayerStats() {
            Valid = false;
        }

        /// <summary>
        /// Determines whether this instance has NumberOfPurchases.
        /// </summary>
        /// <returns><c>true</c> if this instance has NumberOfPurchases; otherwise, <c>false</c>.</returns>
        public bool HasNumberOfPurchases()
        {
            return NumberOfPurchases != (int)UNSET_VALUE;
        }

        /// <summary>
        /// Determines whether this instance has AvgSessonLength.
        /// </summary>
        /// <returns><c>true</c> if this instance has AvgSessonLength; otherwise, <c>false</c>.</returns>
        public bool HasAvgSessonLength()
        {
            return AvgSessonLength != UNSET_VALUE;
        }

        /// <summary>
        /// Determines whether this instance has DaysSinceLastPlayed.
        /// </summary>
        /// <returns><c>true</c> if this instance has DaysSinceLastPlayed; otherwise, <c>false</c>.</returns>
        public bool HasDaysSinceLastPlayed()
        {
            return DaysSinceLastPlayed != (int)UNSET_VALUE;
        }

        /// <summary>
        /// Determines whether this instance has NumberOfSessions.
        /// </summary>
        /// <returns><c>true</c> if this instance has NumberOfSessions; otherwise, <c>false</c>.</returns>
        public bool HasNumberOfSessions()
        {
            return NumberOfSessions != (int)UNSET_VALUE;
        }

        /// <summary>
        /// Determines whether this instance has SessPercentile.
        /// </summary>
        /// <returns><c>true</c> if this instance has SessPercentile; otherwise, <c>false</c>.</returns>
        public bool HasSessPercentile()
        {
            return SessPercentile != UNSET_VALUE;
        }

        /// <summary>
        /// Determines whether this instance has SpendPercentile.
        /// </summary>
        /// <returns><c>true</c> if this instance has SpendPercentile; otherwise, <c>false</c>.</returns>
        public bool HasSpendPercentile()
        {
            return SpendPercentile != UNSET_VALUE;
        }

        /// <summary>
        /// Determines whether this instance has ChurnProbability.
        /// </summary>
        /// <returns><c>true</c> if this instance has ChurnProbability; otherwise, <c>false</c>.</returns>
        public bool HasChurnProbability()
        {
            return ChurnProbability != UNSET_VALUE;
        }
    }
}
#endif
