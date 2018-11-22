// <copyright file="NativeVideoClient.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc.
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
namespace GooglePlayGames.Native
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.BasicApi.Video;
    using GooglePlayGames.Native.PInvoke;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal class NativeVideoClient : IVideoClient
    {

        private readonly VideoManager mManager;

        internal NativeVideoClient(VideoManager manager)
        {
            this.mManager = Misc.CheckNotNull(manager);
        }

        public void GetCaptureCapabilities(Action<ResponseStatus, VideoCapabilities> callback)
        {
            Misc.CheckNotNull(callback);
            callback = CallbackUtils.ToOnGameThread(callback);
            mManager.GetCaptureCapabilities(
                response => {
                    var status = ConversionUtils.ConvertResponseStatus(response.GetStatus());

                    if (!response.RequestSucceeded())
                    {
                        callback(status, null);
                    }
                    else
                    {
                        callback(status, FromNativeVideoCapabilities(response.GetData()));
                    }
                });
        }

        private VideoCapabilities FromNativeVideoCapabilities(NativeVideoCapabilities capabilities)
        {
            bool[] captureModes = new bool[mManager.NumCaptureModes];
            captureModes[(int)VideoCaptureMode.File] = capabilities.SupportsCaptureMode(Types.VideoCaptureMode.FILE);
            captureModes[(int)VideoCaptureMode.Stream] = capabilities.SupportsCaptureMode(Types.VideoCaptureMode.STREAM);
            bool[] qualityLevels = new bool[mManager.NumQualityLevels];
            qualityLevels[(int)VideoQualityLevel.SD] = capabilities.SupportsQualityLevel(Types.VideoQualityLevel.SD);
            qualityLevels[(int)VideoQualityLevel.HD] = capabilities.SupportsQualityLevel(Types.VideoQualityLevel.HD);
            qualityLevels[(int)VideoQualityLevel.XHD] = capabilities.SupportsQualityLevel(Types.VideoQualityLevel.XHD);
            qualityLevels[(int)VideoQualityLevel.FullHD] = capabilities.SupportsQualityLevel(Types.VideoQualityLevel.FULLHD);
            return new VideoCapabilities(capabilities.IsCameraSupported(),
                capabilities.IsMicSupported(),
                capabilities.IsWriteStorageSupported(),
                captureModes,
                qualityLevels);
        }

        public void ShowCaptureOverlay()
        {
            mManager.ShowCaptureOverlay();
        }

        public void GetCaptureState(Action<ResponseStatus, VideoCaptureState> callback)
        {
            Misc.CheckNotNull(callback);
            callback = CallbackUtils.ToOnGameThread(callback);
            mManager.GetCaptureState(
                response => {
                    var status = ConversionUtils.ConvertResponseStatus(response.GetStatus());

                    if (!response.RequestSucceeded())
                    {
                        callback(status, null);
                    }
                    else
                    {
                        callback(status, FromNativeVideoCaptureState(response.GetData()));
                    }
                });
        }

        private VideoCaptureState FromNativeVideoCaptureState(NativeVideoCaptureState captureState)
        {
            return new VideoCaptureState(captureState.IsCapturing(),
                ConversionUtils.ConvertNativeVideoCaptureMode(captureState.CaptureMode()),
                ConversionUtils.ConvertNativeVideoQualityLevel(captureState.QualityLevel()),
                captureState.IsOverlayVisible(),
                captureState.IsPaused());
        }

        public void IsCaptureAvailable(VideoCaptureMode captureMode, Action<ResponseStatus, bool> callback)
        {
            Misc.CheckNotNull(callback);
            callback = CallbackUtils.ToOnGameThread(callback);
            mManager.IsCaptureAvailable(ConversionUtils.ConvertVideoCaptureMode(captureMode),
                response => {
                    var status = ConversionUtils.ConvertResponseStatus(response.GetStatus());

                    if (!response.RequestSucceeded())
                    {
                        callback(status, false);
                    }
                    else
                    {
                        callback(status, response.IsCaptureAvailable());
                    }
                });
        }

        public bool IsCaptureSupported()
        {
            return mManager.IsCaptureSupported();
        }

        public void RegisterCaptureOverlayStateChangedListener(CaptureOverlayStateListener listener)
        {
            Misc.CheckNotNull(listener);
            CaptureOverlayStateListenerHelper helper = CaptureOverlayStateListenerHelper.Create()
                .SetOnCaptureOverlayStateChangedCallback(
                    response => {
                        listener.OnCaptureOverlayStateChanged(ConversionUtils.ConvertNativeVideoCaptureOverlayState(response));
                });
            mManager.RegisterCaptureOverlayStateChangedListener(helper);
        }

        public void UnregisterCaptureOverlayStateChangedListener()
        {
            mManager.UnregisterCaptureOverlayStateChangedListener();
        }
    }
}
#endif //UNITY_ANDROID

