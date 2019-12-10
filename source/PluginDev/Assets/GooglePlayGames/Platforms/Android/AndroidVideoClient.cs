#if UNITY_ANDROID
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Video;
    using GooglePlayGames.OurUtils;
    using UnityEngine;

    internal class AndroidVideoClient : IVideoClient
    {
        private volatile AndroidJavaObject mVideosClient;
        private bool mIsCaptureSupported;
        private OnCaptureOverlayStateListenerProxy mOnCaptureOverlayStateListenerProxy = null;

        public AndroidVideoClient(bool isCaptureSupported, AndroidJavaObject account)
        {
            mIsCaptureSupported = isCaptureSupported;
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games"))
            {
                mVideosClient = gamesClass.CallStatic<AndroidJavaObject>("getVideosClient",
                    AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void GetCaptureCapabilities(Action<ResponseStatus, VideoCapabilities> callback)
        {
            callback = ToOnGameThread(callback);
            using (var task = mVideosClient.Call<AndroidJavaObject>("getCaptureCapabilities"))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    videoCapabilities => callback(ResponseStatus.Success, CreateVideoCapabilities(videoCapabilities)));

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => callback(ResponseStatus.InternalError, null));
            }
        }

        public void ShowCaptureOverlay()
        {
            AndroidHelperFragment.ShowCaptureOverlayUI();
        }

        public void GetCaptureState(Action<ResponseStatus, VideoCaptureState> callback)
        {
            callback = ToOnGameThread(callback);
            using (var task = mVideosClient.Call<AndroidJavaObject>("getCaptureState"))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    captureState =>
                        callback(ResponseStatus.Success, CreateVideoCaptureState(captureState)));

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => callback(ResponseStatus.InternalError, null));
            }
        }

        public void IsCaptureAvailable(VideoCaptureMode captureMode, Action<ResponseStatus, bool> callback)
        {
            callback = ToOnGameThread(callback);
            using (var task =
                mVideosClient.Call<AndroidJavaObject>("isCaptureAvailable", ToVideoCaptureMode(captureMode)))
            {
                AndroidTaskUtils.AddOnSuccessListener<bool>(
                    task,
                    isCaptureAvailable => callback(ResponseStatus.Success, isCaptureAvailable));

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => callback(ResponseStatus.InternalError, false));
            }
        }

        public bool IsCaptureSupported()
        {
            return mIsCaptureSupported;
        }

        public void RegisterCaptureOverlayStateChangedListener(CaptureOverlayStateListener listener)
        {
            if (mOnCaptureOverlayStateListenerProxy != null)
            {
                UnregisterCaptureOverlayStateChangedListener();
            }

            mOnCaptureOverlayStateListenerProxy = new OnCaptureOverlayStateListenerProxy(listener);
            using (mVideosClient.Call<AndroidJavaObject>("registerOnCaptureOverlayStateChangedListener",
                mOnCaptureOverlayStateListenerProxy)) ;
        }

        public void UnregisterCaptureOverlayStateChangedListener()
        {
            if (mOnCaptureOverlayStateListenerProxy != null)
            {
                using (mVideosClient.Call<AndroidJavaObject>("unregisterOnCaptureOverlayStateChangedListener",
                    mOnCaptureOverlayStateListenerProxy)) ;

                mOnCaptureOverlayStateListenerProxy = null;
            }
        }

        private class OnCaptureOverlayStateListenerProxy : AndroidJavaProxy
        {
            private CaptureOverlayStateListener mListener;

            public OnCaptureOverlayStateListenerProxy(CaptureOverlayStateListener listener)
                : base("com/google/android/gms/games/VideosClient$OnCaptureOverlayStateListener")
            {
                mListener = listener;
            }

            public void onCaptureOverlayStateChanged(int overlayState)
            {
                PlayGamesHelperObject.RunOnGameThread(() =>
                    mListener.OnCaptureOverlayStateChanged(FromVideoCaptureOverlayState(overlayState))
                );
            }

            private static VideoCaptureOverlayState FromVideoCaptureOverlayState(int overlayState)
            {
                switch (overlayState)
                {
                    case 1: // CAPTURE_OVERLAY_STATE_SHOWN
                        return VideoCaptureOverlayState.Shown;
                    case 2: // CAPTURE_OVERLAY_STATE_CAPTURE_STARTED
                        return VideoCaptureOverlayState.Started;
                    case 3: // CAPTURE_OVERLAY_STATE_CAPTURE_STOPPED
                        return VideoCaptureOverlayState.Stopped;
                    case 4: // CAPTURE_OVERLAY_STATE_DISMISSED
                        return VideoCaptureOverlayState.Dismissed;
                    default:
                        return VideoCaptureOverlayState.Unknown;
                }
            }
        }

        private static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert)
        {
            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }

        private static VideoQualityLevel FromVideoQualityLevel(int captureQualityJava)
        {
            switch (captureQualityJava)
            {
                case 0: // QUALITY_LEVEL_SD
                    return VideoQualityLevel.SD;
                case 1: // QUALITY_LEVEL_HD
                    return VideoQualityLevel.HD;
                case 2: // QUALITY_LEVEL_XHD
                    return VideoQualityLevel.XHD;
                case 3: // QUALITY_LEVEL_FULLHD
                    return VideoQualityLevel.FullHD;
                default:
                    return VideoQualityLevel.Unknown;
            }
        }

        private static VideoCaptureMode FromVideoCaptureMode(int captureMode)
        {
            switch (captureMode)
            {
                case 0: // CAPTURE_MODE_FILE
                    return VideoCaptureMode.File;
                case 1: // CAPTURE_MODE_STREAM
                    return VideoCaptureMode.Stream;
                default:
                    return VideoCaptureMode.Unknown;
            }
        }

        private static int ToVideoCaptureMode(VideoCaptureMode captureMode)
        {
            switch (captureMode)
            {
                case VideoCaptureMode.File:
                    return 0; // CAPTURE_MODE_FILE
                case VideoCaptureMode.Stream:
                    return 1; // CAPTURE_MODE_STREAM
                default:
                    return -1; // CAPTURE_MODE_UNKNOWN
            }
        }

        private static VideoCaptureState CreateVideoCaptureState(AndroidJavaObject videoCaptureState)
        {
            bool isCapturing = videoCaptureState.Call<bool>("isCapturing");
            VideoCaptureMode captureMode = FromVideoCaptureMode(videoCaptureState.Call<int>("getCaptureMode"));
            VideoQualityLevel qualityLevel = FromVideoQualityLevel(videoCaptureState.Call<int>("getCaptureQuality"));
            bool isOverlayVisible = videoCaptureState.Call<bool>("isOverlayVisible");
            bool isPaused = videoCaptureState.Call<bool>("isPaused");

            return new VideoCaptureState(isCapturing, captureMode,
                qualityLevel, isOverlayVisible, isPaused);
        }

        private static VideoCapabilities CreateVideoCapabilities(AndroidJavaObject videoCapabilities)
        {
            bool isCameraSupported = videoCapabilities.Call<bool>("isCameraSupported");
            bool isMicSupported = videoCapabilities.Call<bool>("isMicSupported");
            bool isWriteStorageSupported = videoCapabilities.Call<bool>("isWriteStorageSupported");
            bool[] captureModesSupported = videoCapabilities.Call<bool[]>("getSupportedCaptureModes");
            bool[] qualityLevelsSupported = videoCapabilities.Call<bool[]>("getSupportedQualityLevels");

            return new VideoCapabilities(isCameraSupported, isMicSupported, isWriteStorageSupported,
                captureModesSupported, qualityLevelsSupported);
        }
    }
}
#endif
