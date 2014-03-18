/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using UnityEngine.SocialPlatforms;

namespace GooglePlayGames {
    /// <summary>
    /// Represents the Google Play Games local user.
    /// </summary>
    public class PlayGamesLocalUser : PlayGamesUserProfile, ILocalUser {
        PlayGamesPlatform mPlatform;

        internal PlayGamesLocalUser(PlayGamesPlatform plaf) {
            mPlatform = plaf;
        }

        /// <summary>
        /// Authenticates the local user. Equivalent to calling
        /// <see cref="PlayGamesPlatform.Authenticate" />.
        /// </summary>
        public void Authenticate(Action<bool> callback) {
            mPlatform.Authenticate(callback);
        }

        /// <summary>
        /// Authenticates the local user. Equivalent to calling
        /// <see cref="PlayGamesPlatform.Authenticate" />.
        /// </summary>
        public void Authenticate(Action<bool> callback, bool silent) {
            mPlatform.Authenticate(callback, silent);
        }

        /// <summary>
        /// Not implemented. Calls the callback with <c>false</c>.
        /// </summary>
        public void LoadFriends (Action<bool> callback) {
            if (callback != null) {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Not implemented. Returns an empty list.
        /// </summary>
        public IUserProfile[] friends {
            get {
                return new IUserProfile[0];
            }
        }

        /// <summary>
        /// Returns whether or not the local user is authenticated to Google Play Games.
        /// </summary>
        /// <returns>
        /// <c>true</c> if authenticated; otherwise, <c>false</c>.
        /// </returns>
        public bool authenticated {
            get {
                return mPlatform.IsAuthenticated();
            }
        }

        /// <summary>
        /// Not implemented. As safety placeholder, returns true.
        /// </summary>
        public bool underage {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        /// <returns>
        /// The display name of the user.
        /// </returns>
        public new string userName {
            get {
                return authenticated ? mPlatform.GetUserDisplayName() : "";
            }
        }

        /// <summary>
        /// Gets the user's Google id.
        /// </summary>
        /// <returns>
        /// The user's Google id.
        /// </returns>
        public new string id {
            get {
                return authenticated ? mPlatform.GetUserId() : "";
            }
        }

        /// <summary>
        /// Returns true (since this is the local user).
        /// </summary>
        public new bool isFriend {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the local user's state. This is always <c>UserState.Online</c> for
        /// the local user.
        /// </summary>
        public new UserState state {
            get {
                return UserState.Online;
            }
        }
    }
}

