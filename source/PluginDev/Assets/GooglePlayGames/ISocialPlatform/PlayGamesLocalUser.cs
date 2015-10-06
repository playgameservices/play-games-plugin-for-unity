// <copyright file="PlayGamesLocalUser.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using System;
    using GooglePlayGames.BasicApi;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Represents the Google Play Games local user.
    /// </summary>
    public class PlayGamesLocalUser : PlayGamesUserProfile, ILocalUser
    {
        internal PlayGamesPlatform mPlatform;

        private string emailAddress;

        private PlayerStats mStats;

        internal PlayGamesLocalUser(PlayGamesPlatform plaf)
            : base("localUser", string.Empty, string.Empty)
        {
            mPlatform = plaf;
            emailAddress = null;
            mStats = null;
        }

        /// <summary>
        /// Authenticates the local user. Equivalent to calling
        /// <see cref="PlayGamesPlatform.Authenticate" />.
        /// </summary>
        public void Authenticate(Action<bool> callback)
        {
            mPlatform.Authenticate(callback);
        }

        /// <summary>
        /// Authenticates the local user. Equivalent to calling
        /// <see cref="PlayGamesPlatform.Authenticate" />.
        /// </summary>
        public void Authenticate(Action<bool> callback, bool silent)
        {
            mPlatform.Authenticate(callback, silent);
        }

        /// <summary>
        /// Loads all friends of the authenticated user.
        /// </summary>
        public void LoadFriends(Action<bool> callback)
        {
            mPlatform.LoadFriends(this, callback);
        }

        /// <summary>
        /// Not implemented. Returns an empty list.
        /// </summary>
        public IUserProfile[] friends
        {
            get
            {
                return mPlatform.GetFriends();
            }
        }

        /// <summary>
        /// Returns whether or not the local user is authenticated to Google Play Games.
        /// </summary>
        /// <returns>
        /// <c>true</c> if authenticated; otherwise, <c>false</c>.
        /// </returns>
        public bool authenticated
        {
            get
            {
                return mPlatform.IsAuthenticated();
            }
        }

        /// <summary>
        /// Not implemented. As safety placeholder, returns true.
        /// </summary>
        public bool underage
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        /// <returns>
        /// The display name of the user.
        /// </returns>
        public new string userName
        {
            get
            {
                string retval = string.Empty;
                if (authenticated)
                {
                    retval = mPlatform.GetUserDisplayName();
                    if (!base.userName.Equals(retval))
                    {
                        ResetIdentity(retval, mPlatform.GetUserId(), mPlatform.GetUserImageUrl());
                    }
                }
                return retval;
            }
        }

        /// <summary>
        /// Gets the user's Google id.
        /// </summary>
        /// <returns>
        /// The user's Google id.
        /// </returns>
        public new string id
        {
            get
            {
                string retval = string.Empty;
                if (authenticated)
                {
                    retval = mPlatform.GetUserId();
                    if (!base.id.Equals(retval))
                    {
                        ResetIdentity(mPlatform.GetUserDisplayName(), retval, mPlatform.GetUserImageUrl());
                    }
                }
                return retval;
            }
        }

        /// <summary>
        /// Gets an id token for the user.
        /// NOTE: This property can only be accessed using the main Unity thread.
        /// </summary>
        /// <returns>
        /// An id token for the user.
        /// </returns>
        public string idToken
        {
            get
            {
                return authenticated ? mPlatform.GetIdToken() : string.Empty;
            }
        }

        /// <summary>
        /// Gets an access token for the user.
        /// NOTE: This property can only be accessed using the main Unity thread.
        /// </summary>
        /// <returns>
        /// An id token for the user.
        /// </returns>
        public string accessToken
        {
            get
            {
                return authenticated ? mPlatform.GetAccessToken() : string.Empty;
            }
        }

        /// <summary>
        /// Returns true (since this is the local user).
        /// </summary>
        public new bool isFriend
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the local user's state. This is always <c>UserState.Online</c> for
        /// the local user.
        /// </summary>
        public new UserState state
        {
            get
            {
                return UserState.Online;
            }
        }


        public new string AvatarURL
        {
            get
            {
                string retval = string.Empty;
                if (authenticated)
                {
                    retval = mPlatform.GetUserImageUrl();
                    if (!base.id.Equals(retval))
                    {
                        ResetIdentity(mPlatform.GetUserDisplayName(),
                            mPlatform.GetUserId(), retval);
                    }
                }
                return retval;
            }
        }

        /// <summary>
        /// Gets the email of the signed in player.  This is only available
        /// if the web client id is added to the setup (which enables additional
        /// permissions for the application).
        /// NOTE: This property can only be accessed using the main Unity thread.
        /// </summary>
        /// <value>The email.</value>
        public string Email
        {
            get
            {
                // treat null as unitialized, empty as no email.  This can
                // happen when the web client is not initialized.
                if (authenticated && emailAddress == null)
                {
                    emailAddress = mPlatform.GetUserEmail();
                    emailAddress = emailAddress != null ? emailAddress : string.Empty;
                }
                return authenticated ? emailAddress : string.Empty;
            }
        }

        /// <summary>
        /// Gets the player's stats.
        /// </summary>
        /// <param name="callback">Callback when they are available.</param>
        public void GetStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            if (mStats == null)
            {
                mPlatform.GetPlayerStats((rc, stats) =>
                    {
                        mStats = stats;
                        callback(rc, stats);
                    });
            }
            else
            {
                // 0 = success
                callback(CommonStatusCodes.Success, mStats);
            }
        }

        /// <summary>
        /// Player stats. See https://developers.google.com/games/services/android/stats
        /// </summary>
        public class PlayerStats
        {
            /// <summary>
            /// The number of in-app purchases.
            /// </summary>
            private int numberOfPurchases;

            /// <summary>
            /// The length of the avg sesson in minutes.
            /// </summary>
            private float avgSessonLength;

            /// <summary>
            /// The days since last played.
            /// </summary>
            private int daysSinceLastPlayed;

            /// <summary>
            /// The number of sessions based on sign-ins.
            /// </summary>
            private int numOfSessions;

            /// <summary>
            /// The approximation of sessions percentile for the player,
            /// given as a decimal value between 0 and 1 (inclusive).
            /// This value indicates how many sessions the current player has
            /// played in comparison to the rest of this game's player base.
            /// Higher numbers indicate that this player has played more sessions.
            /// </summary>
            private float sessPercentile;

            /// <summary>
            /// The approximate spend percentile of the player,
            /// given as a decimal value between 0 and 1 (inclusive). This
            /// value indicates how much the current player has spent in
            /// comparison to the rest of this game's player base. Higher
            /// numbers indicate that this player has spent more.
            /// </summary>
            private float spendPercentile;

            public int NumberOfPurchases
            {
                get;
                set;
            }

            public float AvgSessonLength
            {
                get;
                set;
            }

            public int DaysSinceLastPlayed
            {
                get;
                set;
            }

            public int NumOfSessions
            {
                get;
                set;
            }

            public float SessPercentile
            {
                get;
                set;
            }

            public float SpendPercentile
            {
                get;
                set;
            }
        }
    }
}
