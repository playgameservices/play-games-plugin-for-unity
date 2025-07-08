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

#if UNITY_ANDROID

namespace GooglePlayGames
{
    using System;
    using GooglePlayGames.BasicApi;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Represents the Google Play Games local user, providing access to
    /// authentication and user-specific functionality. Implements Unity's
    /// <c>ILocalUser</c> interface.
    /// </summary>
    public class PlayGamesLocalUser : PlayGamesUserProfile, ILocalUser
    {
        /// <summary>
        /// A reference to the active Play Games platform instance.
        /// </summary>
        internal PlayGamesPlatform mPlatform;

        /// <summary>
        /// Cached player stats.
        /// </summary>
        private PlayerStats mStats;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesLocalUser"/> class.
        /// </summary>
        /// <param name="plaf">The platform instance.</param>
        internal PlayGamesLocalUser(PlayGamesPlatform plaf)
            : base("localUser", string.Empty, string.Empty)
        {
            mPlatform = plaf;
            mStats = null;
        }

        /// <summary>
        /// Authenticates the local user. This is equivalent to calling
        /// <see cref="PlayGamesPlatform.Authenticate(Action{SignInStatus})"/>.
        /// </summary>
        /// <param name="callback">A callback to invoke with a boolean indicating success.</param>
        public void Authenticate(Action<bool> callback)
        {
            mPlatform.Authenticate(status => callback(status == SignInStatus.Success));
        }

        /// <summary>
        /// Authenticates the local user with an extended callback that includes the reason for failure.
        /// This is equivalent to calling <see cref="PlayGamesPlatform.Authenticate(Action{SignInStatus})"/>.
        /// </summary>
        /// <param name="callback">
        /// A callback to invoke with a boolean indicating success and a string containing the status.
        /// </param>
        public void Authenticate(Action<bool, string> callback)
        {
            mPlatform.Authenticate(status => callback(status == SignInStatus.Success, status.ToString()));
        }

        /// <summary>
        /// Loads the friends of the authenticated user.
        /// </summary>
        /// <param name="callback">A callback to invoke with a boolean indicating success.</param>
        public void LoadFriends(Action<bool> callback)
        {
            mPlatform.LoadFriends(this, callback);
        }

        /// <summary>
        /// Gets the local user's friends. This will be null until <see cref="LoadFriends"/> completes.
        /// </summary>
        /// <value>An array of the user's friends, or null if not yet loaded.</value>
        public IUserProfile[] friends
        {
            get { return mPlatform.GetFriends(); }
        }

        /// <summary>
        /// Gets a value indicating whether the local user is authenticated to Google Play Games.
        /// </summary>
        /// <value><c>true</c> if authenticated; otherwise, <c>false</c>.</value>
        public bool authenticated
        {
            get { return mPlatform.IsAuthenticated(); }
        }

        /// <summary>
        /// Gets a value indicating whether the user is underage.
        /// </summary>
        /// <value>This is not implemented and returns <c>true</c> as a placeholder.</value>
        public bool underage
        {
            get { return true; }
        }



        /// <summary>
        /// Gets the display name of the local user.
        /// </summary>
        /// <value>The user's display name.</value>
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
        /// Gets the user's Google ID (Player ID).
        /// </summary>
        /// <remarks>
        /// This ID is persistent and uniquely identifies the user across all games.
        /// It is the preferred way to identify a player.
        /// </remarks>
        /// <value>The user's Google ID.</value>
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
        /// Gets a value indicating whether this user is a friend of the local user.
        /// </summary>
        /// <value>Always returns <c>true</c>.</value>
        public new bool isFriend
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the user's state.
        /// </summary>
        /// <value>For the local user, this is always <c>UserState.Online</c>.</value>
        public new UserState state
        {
            get { return UserState.Online; }
        }

        /// <summary>
        /// Gets the URL of the user's avatar image.
        /// </summary>
        /// <value>The avatar image URL.</value>
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
        /// Gets the player's stats from the server.
        /// </summary>
        /// <param name="callback">A callback to be invoked with the status code and the player's stats.
        /// The stats may be cached from a previous call.
        /// </param>
        public void GetStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            if (mStats == null || !mStats.Valid)
            {
                mPlatform.GetPlayerStats((rc, stats) =>
                {
                    mStats = stats;
                    callback(rc, stats);
                });
            }
            else
            {
                // Return cached stats with a success code.
                callback(CommonStatusCodes.Success, mStats);
            }
        }
    }
}
#endif