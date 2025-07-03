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
    /// Represents a Google Play Games user profile. In the current implementation,
    /// this is only used as a base class of <see cref="PlayGamesLocalUser" />
    /// and should not be used directly.
    /// </summary>
    public class PlayGamesUserProfile : IUserProfile
    {
        private string mDisplayName;
        private string mPlayerId;
        private string mAvatarUrl;
        private bool mIsFriend;

        private volatile bool mImageLoading = false;
        private Texture2D mImage;

        internal PlayGamesUserProfile(string displayName, string playerId,
            string avatarUrl)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            setAvatarUrl(avatarUrl);
            mImageLoading = false;
            mIsFriend = false;
        }

        internal PlayGamesUserProfile(string displayName, string playerId, string avatarUrl,
            bool isFriend)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            mAvatarUrl = avatarUrl;
            mImageLoading = false;
            mIsFriend = isFriend;
        }

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

        public string userName
        {
            get { return mDisplayName; }
        }

        public string id
        {
            get { return mPlayerId; }
        }

        public string gameId
        {
            get { return mPlayerId; }
        }

        public bool isFriend
        {
            get { return mIsFriend; }
        }

        public UserState state
        {
            get { return UserState.Online; }
        }

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

        public string AvatarURL
        {
            get { return mAvatarUrl; }
        }

        /// <summary>
        /// Loads the local user's image from the url.  Loading urls
        /// is asynchronous so the return from this call is fast,
        /// the image is returned once it is loaded.  null is returned
        /// up to that point.
        /// </summary>
        internal IEnumerator LoadImage()
        {
            // the url can be null if the user does not have an
            // avatar configured.
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

        public override int GetHashCode()
        {
            return typeof(PlayGamesUserProfile).GetHashCode() ^ mPlayerId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[Player: '{0}' (id {1})]", mDisplayName, mPlayerId);
        }

        private void setAvatarUrl(string avatarUrl)
        {
            mAvatarUrl = avatarUrl;
            if (!string.IsNullOrEmpty(avatarUrl) && !avatarUrl.StartsWith("https") && avatarUrl.StartsWith("http"))
            {
                mAvatarUrl = avatarUrl.Insert(4, "s");
            }
        }
    }
}
#endif
