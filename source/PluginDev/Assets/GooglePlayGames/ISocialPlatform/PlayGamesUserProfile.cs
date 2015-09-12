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

namespace GooglePlayGames
{
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Represents a Google Play Games user profile. In the current implementation,
    /// this is only used as a base class of <see cref="PlayGamesLocalUser" />
    /// and should not be used directly.
    /// </summary>
    public class PlayGamesUserProfile : IUserProfile
    {
        private  string mDisplayName;
        private  string mPlayerId;
        private  string mAvatarUrl;

        private WWW wwwImage;
        private Texture2D mImage;

        internal PlayGamesUserProfile(string displayName, string playerId,
            string avatarUrl)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            mAvatarUrl = avatarUrl;
        }

        protected void ResetIdentity(string displayName, string playerId,
            string avatarUrl)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            mAvatarUrl = avatarUrl;
        }

        #region IUserProfile implementation

        public string userName
        {
            get
            {
                return mDisplayName;
            }
        }

        public string id
        {
            get
            {
                return mPlayerId;
            }
        }

        public bool isFriend
        {
            get
            {
                return true;
            }
        }

        public UserState state
        {
            get
            {
                return UserState.Online;
            }
        }

        public Texture2D image
        {
            get
            {
                return LoadImage();
            }
        }

        #endregion

        public string AvatarURL
        {
            get
            {
                return mAvatarUrl;
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
            // the url can be null if the user does not have an
            // avatar configured.
            if (!string.IsNullOrEmpty(AvatarURL))
            {
                if (wwwImage == null || wwwImage.url != AvatarURL)
                {
                    wwwImage = new WWW(AvatarURL);
                    mImage = null;
                }

                if (mImage != null) {
                    return mImage;
                }

                if (wwwImage.isDone)
                {
                    mImage =  wwwImage.texture;
                    return mImage;
                }
            }

            // if there is no url, always return null.
            return null;
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

            if (!typeof(object).IsSubclassOf(typeof(PlayGamesUserProfile)))
            {
                return false;
            }

            return mPlayerId.Equals(((PlayGamesUserProfile)obj).mPlayerId);
        }

        public override int GetHashCode()
        {
            return typeof(PlayGamesUserProfile).GetHashCode() ^ mPlayerId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[Player: '{0}' (id {1})]", mDisplayName, mPlayerId);
        }
    }
}
