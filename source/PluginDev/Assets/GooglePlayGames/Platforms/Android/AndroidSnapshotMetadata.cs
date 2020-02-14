#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using UnityEngine;

    internal class AndroidSnapshotMetadata : ISavedGameMetadata
    {
        private AndroidJavaObject mJavaSnapshot;
        private AndroidJavaObject mJavaMetadata;
        private AndroidJavaObject mJavaContents;

        public AndroidSnapshotMetadata(AndroidJavaObject javaSnapshot)
        {
            mJavaSnapshot = javaSnapshot;
            mJavaMetadata = javaSnapshot.Call<AndroidJavaObject>("getMetadata");
            mJavaContents = javaSnapshot.Call<AndroidJavaObject>("getSnapshotContents");
        }

        public AndroidSnapshotMetadata(AndroidJavaObject javaMetadata, AndroidJavaObject javaContents)
        {
            mJavaSnapshot = null;
            mJavaMetadata = javaMetadata;
            mJavaContents = javaContents;
        }

        public AndroidJavaObject JavaSnapshot
        {
            get { return mJavaSnapshot; }
        }

        public AndroidJavaObject JavaMetadata
        {
            get { return mJavaMetadata; }
        }

        public AndroidJavaObject JavaContents
        {
            get { return mJavaContents; }
        }

        public bool IsOpen
        {
            get
            {
                if (mJavaContents == null)
                {
                    return false;
                }

                return !mJavaContents.Call<bool>("isClosed");
            }
        }

        public string Filename
        {
            get { return mJavaMetadata.Call<string>("getUniqueName"); }
        }

        public string Description
        {
            get { return mJavaMetadata.Call<string>("getDescription"); }
        }

        public string CoverImageURL
        {
            get { return mJavaMetadata.Call<string>("getCoverImageUrl"); }
        }

        public TimeSpan TotalTimePlayed
        {
            get { return TimeSpan.FromMilliseconds(mJavaMetadata.Call<long>("getPlayedTime")); }
        }

        public DateTime LastModifiedTimestamp
        {
            get
            {
                long timestamp = mJavaMetadata.Call<long>("getLastModifiedTimestamp");
                System.DateTime lastModifiedTime = AndroidJavaConverter.ToDateTime(timestamp);
                return lastModifiedTime;
            }
        }
    }
}
#endif
