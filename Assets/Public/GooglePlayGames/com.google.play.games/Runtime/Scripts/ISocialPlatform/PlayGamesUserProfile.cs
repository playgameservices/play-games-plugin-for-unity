// <copyright file="PlayGamesUserProfile.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
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
    using System.Collections;
    using GooglePlayGames.OurUtils;
    using UnityEngine;
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.Networking;
#endif
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Represents a Google Play Games user profile. Implements the Unity's <c>IUserProfile</c>
    /// interface and is used as a base class for <see cref="PlayGamesLocalUser" />.
    /// </summary>
    public class PlayGamesUserProfile : IUserProfile
    {
        /// <summary>
        /// The user's display name.
        /// </summary>
        private string mDisplayName;

        /// <summary>
        /// The user's unique player ID.
        /// </summary>
        private string mPlayerId;

        /// <summary>
        /// The URL for the user's avatar image.
        /// </summary>
        private string mAvatarUrl;

        /// <summary>
        /// A flag indicating if this user is a friend of the local user.
        /// </summary>
        private bool mIsFriend;

        /// <summary>
        /// A flag to prevent multiple concurrent image loading coroutines.
        /// </summary>
        private volatile bool mImageLoading = false;

        /// <summary>
        /// The cached user avatar image.
        /// </summary>
        private Texture2D mImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesUserProfile"/> class.
        /// </summary>
        /// <param name="displayName">The user's display name.</param>
        /// <param name="playerId">The user's player ID.</param>
        /// <param name="avatarUrl">The URL of the user's avatar.</param>
        internal PlayGamesUserProfile(string displayName, string playerId,
            string avatarUrl)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            setAvatarUrl(avatarUrl);
            mImageLoading = false;
            mIsFriend = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayGamesUserProfile"/> class.
        /// </summary>
        /// <param name="displayName">The user's display name.</param>
        /// <param name="playerId">The user's player ID.</param>
        /// <param name="avatarUrl">The URL of the user's avatar.</param>
        /// <param name="isFriend">A flag indicating if the user is a friend.</param>
        internal PlayGamesUserProfile(string displayName, string playerId, string avatarUrl,
            bool isFriend)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            mAvatarUrl = avatarUrl;
            mImageLoading = false;
            mIsFriend = isFriend;
        }

        /// <summary>
        /// Resets the user's identity with new information. If the avatar URL has changed,
        /// the old image is discarded.
        /// </summary>
        /// <param name="displayName">The new display name.</param>
        /// <param name="playerId">The new player ID.</param>
        /// <param name="avatarUrl">The new avatar URL.</param>
        protected void ResetIdentity(string displayName, string playerId,
            string avatarUrl)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            mIsFriend = false;
            if (mAvatarUrl != avatarUrl)
            {
                mImage = null;
                setAvatarUrl(avatarUrl);
            }

            mImageLoading = false;
        }

        #region IUserProfile implementation

        /// <summary>
        /// Gets the user's display name.
        /// </summary>
        /// <value>The name of the user.</value>
        public string userName
        {
            get { return mDisplayName; }
        }

        /// <summary>
        /// Gets the user's unique player ID.
        /// </summary>
        /// <value>The player ID.</value>
        public string id
        {
            get { return mPlayerId; }
        }

        /// <summary>
        /// Gets the user's game-specific identifier. In this implementation, it is the same as the player ID.
        /// </summary>
        public string gameId
        {
            get { return mPlayerId; }
        }

        /// <summary>
        /// Gets a value indicating whether this user is a friend of the local user.
        /// </summary>
        /// <value><c>true</c> if this user is a friend; otherwise, <c>false</c>.</value>
        public bool isFriend
        {
            get { return mIsFriend; }
        }

        /// <summary>
        /// Gets the user's current state. In this implementation, it always returns 'Online'.
        /// </summary>
        public UserState state
        {
            get { return UserState.Online; }
        }

        /// <summary>
        /// Gets the user's avatar image as a <see cref="Texture2D"/>.
        /// The image is loaded asynchronously. Returns null until the image has been loaded.
        /// </summary>
        /// <value>The user's avatar image.</value>
        public Texture2D image
        {
            get
            {
                if (!mImageLoading && mImage == null && !string.IsNullOrEmpty(AvatarURL))
                {
                    OurUtils.Logger.d("Starting to load image: " + AvatarURL);
                    mImageLoading = true;
                    PlayGamesHelperObject.RunCoroutine(LoadImage());
                }

                return mImage;
            }
        }

        #endregion

        /// <summary>
        /// Gets the URL of the user's avatar.
        /// </summary>
        public string AvatarURL
        {
            get { return mAvatarUrl; }
        }

        /// <summary>
        /// An enumerator that asynchronously loads the user's avatar image from the <see cref="AvatarURL"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> to be used with a coroutine.</returns>
        internal IEnumerator LoadImage()
        {
            // The URL can be null if the user does not have an avatar configured.
            if (!string.IsNullOrEmpty(AvatarURL))
            {
#if UNITY_2017_2_OR_NEWER
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(AvatarURL);
                www.SendWebRequest();
#else
                WWW www = new WWW(AvatarURL);
#endif
                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.error == null)
                {
#if UNITY_2017_2_OR_NEWER
                    this.mImage = DownloadHandlerTexture.GetContent(www);
#else
                    this.mImage = www.texture;
#endif
                }
                else
                {
                    mImage = Texture2D.blackTexture;
                    OurUtils.Logger.e("Error downloading image: " + www.error);
                }

                mImageLoading = false;
            }
            else
            {
                OurUtils.Logger.e("No URL found.");
                mImage = Texture2D.blackTexture;
                mImageLoading = false;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="PlayGamesUserProfile"/>.
        /// Equality is based on the player ID.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="PlayGamesUserProfile"/>.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            PlayGamesUserProfile other = obj as PlayGamesUserProfile;
            if (other == null)
            {
                return false;
            }

            return StringComparer.Ordinal.Equals(mPlayerId, other.mPlayerId);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="PlayGamesUserProfile"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
        public override int GetHashCode()
        {
            return typeof(PlayGamesUserProfile).GetHashCode() ^ mPlayerId.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="PlayGamesUserProfile"/>.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return string.Format("[Player: '{0}' (id {1})]", mDisplayName, mPlayerId);
        }

        /// <summary>
        /// Sets the avatar URL, ensuring it uses HTTPS.
        /// </summary>
        /// <param name="avatarUrl">The avatar URL to set.</param>
        private void setAvatarUrl(string avatarUrl)
        {
            mAvatarUrl = avatarUrl;
            if (!string.IsNullOrEmpty(mAvatarUrl) && !mAvatarUrl.StartsWith("https") && mAvatarUrl.StartsWith("http"))
            {
                mAvatarUrl = mAvatarUrl.Insert(4, "s");
            }
        }
    }
}
#endif