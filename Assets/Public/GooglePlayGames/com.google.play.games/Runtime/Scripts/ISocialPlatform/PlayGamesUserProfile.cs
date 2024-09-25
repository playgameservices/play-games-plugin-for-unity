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

    using UnityEngine;

    using GooglePlayGames.OurUtils;

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

        internal PlayGamesUserProfile(string displayName, string playerId,string avatarUrl) : this(displayName,playerId,avatarUrl,false)
        {
        }

        internal PlayGamesUserProfile(string displayName, string playerId, string avatarUrl,bool isFriend)
        {
            mDisplayName = displayName;
            mPlayerId = playerId;
            mAvatarUrl = avatarUrl;
            mImageLoading = false;
            mIsFriend = isFriend;
        }

        protected void ResetIdentity(string displayName, string playerId,string avatarUrl)
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

                    mImage = new Texture2D(96,96);

                    using(var currentActivity = Android.AndroidHelperFragment.GetActivity())
                    {
                        currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                            using(var currentActivity = Android.AndroidHelperFragment.GetActivity())
                            using(var ImageManager = new AndroidJavaClass("com.google.android.gms.common.images.ImageManager"))
                            using(var imageManager = ImageManager.CallStatic<AndroidJavaObject>("create",currentActivity))
                            using(var Uri = new AndroidJavaClass("android.net.Uri"))
                            using(var uri = Uri.CallStatic<AndroidJavaObject>("parse",AvatarURL))
                            {
                                imageManager.Call("loadImage",new OnLoadImageListener((result) => {
                                    PlayGamesHelperObject.RunOnGameThread(() => {
                                        var (uri,drawable,isRequestedDrawable) = result;

                                        using(var CompressFormat = new AndroidJavaClass("android.graphics.Bitmap$CompressFormat"))
                                        using(var format = CompressFormat.GetStatic<AndroidJavaObject>("PNG"))
                                        using(var outputStream = new AndroidJavaObject("java.io.ByteArrayOutputStream"))
                                        using(var bitmap = drawable.Call<AndroidJavaObject>("getBitmap"))
                                        {
                                            mImage.Reinitialize(bitmap.Call<int>("getWidth"),bitmap.Call<int>("getHeight"));

                                            bitmap.Call<bool>("compress",format,100,outputStream);
                                            var data = outputStream.Call<sbyte[]>("toByteArray");

                                            mImage.LoadImage(System.Runtime.InteropServices.MemoryMarshal.Cast<sbyte,byte>(data).ToArray());
                                        }

                                        drawable?.Dispose();
                                        uri?.Dispose();
                                    });
                                }),uri);
                            }
                        }));
                    }
                }

                return mImage;
            }
        }

        #endregion

        public string AvatarURL
        {
            get { return mAvatarUrl; }
        }

        class OnLoadImageListener : AndroidJavaProxy
        {
            private Action<(AndroidJavaObject uri, AndroidJavaObject drawable, bool isRequestedDrawable)> mAction;
            public OnLoadImageListener(Action<(AndroidJavaObject uri, AndroidJavaObject drawable, bool isRequestedDrawable)> action) : base("com.google.android.gms.common.images.ImageManager$OnImageLoadedListener")
            {
                mAction = action;
            }

            public void onImageLoaded(AndroidJavaObject uri, AndroidJavaObject drawable, bool isRequestedDrawable)
            {
                mAction((uri,drawable,isRequestedDrawable));
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
            if (!avatarUrl.StartsWith("https") && avatarUrl.StartsWith("http"))
            {
                mAvatarUrl = avatarUrl.Insert(4, "s");
            }
        }
    }
}
#endif
