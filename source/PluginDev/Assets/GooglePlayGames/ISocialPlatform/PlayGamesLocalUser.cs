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
    using UnityEngine.SocialPlatforms;
    using UnityEngine;

    /// <summary>
    /// Represents the Google Play Games local user.
    /// </summary>
    public class PlayGamesLocalUser : PlayGamesUserProfile, ILocalUser
    {
        internal PlayGamesPlatform mPlatform;

        private WWW mAvatarUrl;
        private Texture2D mImage;

        internal PlayGamesLocalUser(PlayGamesPlatform plaf)
        {
            mPlatform = plaf;
            mAvatarUrl = null;
            mImage = null;

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
        /// Not implemented. Calls the callback with <c>false</c>.
        /// </summary>
        public void LoadFriends(Action<bool> callback)
        {
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Not implemented. Returns an empty list.
        /// </summary>
        public IUserProfile[] friends
        {
            get
            {
                return new IUserProfile[0];
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
                return authenticated ? mPlatform.GetUserDisplayName() : string.Empty;
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
                return authenticated ? mPlatform.GetUserId() : string.Empty;
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

        /// <summary>
        /// Loads the local user's image from the url.  Loading urls
        /// is asynchronous so the return from this call is fast,
        /// the image is returned once it is loaded.  null is returned
        /// up to that point.
        /// </summary>
        private Texture2D LoadImage()
        {
            string url = mPlatform.GetUserImageUrl();

            // the url can be null if the user does not have an
            // avatar configured.
            if (!string.IsNullOrEmpty(url))
            {
                if (mAvatarUrl == null || mAvatarUrl.url != url)
                {
                    mAvatarUrl = new WWW(url);
                    mImage = null;
                }

                if (mImage != null) {
                    return mImage;
                }

                if (mAvatarUrl.isDone)
                {
                    mImage =  mAvatarUrl.texture;
                    return mImage;
                }
            }

            // if there is no url, always return null.
            return null;
        }

        /// <summary>
        /// Gets the display image of the user.
        /// </summary>
        /// <returns>
        /// null if the user has no avatar, or it has not loaded yet.
        /// </returns>
        public new Texture2D image
        {
            get
            {
                return LoadImage();
            }
        }
    }
}
