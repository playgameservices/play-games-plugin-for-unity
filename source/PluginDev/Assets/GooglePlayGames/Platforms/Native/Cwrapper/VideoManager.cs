// <copyright file="VideoManager.cs" company="Google Inc.">
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
namespace GooglePlayGames.Native.Cwrapper
{
    using System;
    using System.Runtime.InteropServices;

    internal static class VideoManager
    {
        internal delegate void CaptureCapabilitiesCallback(
        /* from(VideoManager_GetCaptureCapabilitiesResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void CaptureStateCallback(
        /* from(VideoManager_GetCaptureStateResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void IsCaptureAvailableCallback(
        /* from(VideoManager_IsCaptureAvailableResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_GetCaptureCapabilities(
            HandleRef self,
        /* from(VideoManager_CaptureCapabilitiesCallback_t) */CaptureCapabilitiesCallback callback,
        /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_ShowCaptureOverlay(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_GetCaptureState(
            HandleRef self,
        /* from(VideoManager_CaptureStateCallback_t) */CaptureStateCallback callback,
        /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_IsCaptureAvailable(
            HandleRef self,
        /* from(VideoCaptureMode_t) */Types.VideoCaptureMode capture_mode,
        /* from(VideoManager_IsCaptureAvailableCallback_t) */IsCaptureAvailableCallback callback,
        /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool VideoManager_IsCaptureSupported(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_RegisterCaptureOverlayStateChangedListener(
            HandleRef self,
        /* from(CaptureOverlayStateListenerHelper_t) */IntPtr helper);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_UnregisterCaptureOverlayStateChangedListener(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_GetCaptureCapabilitiesResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus VideoManager_GetCaptureCapabilitiesResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(VideoCapabilities_t) */ IntPtr VideoManager_GetCaptureCapabilitiesResponse_GetVideoCapabilities(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_GetCaptureStateResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus VideoManager_GetCaptureStateResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(VideoCaptureState_t) */ IntPtr VideoManager_GetCaptureStateResponse_GetVideoCaptureState(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void VideoManager_IsCaptureAvailableResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus VideoManager_IsCaptureAvailableResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool VideoManager_IsCaptureAvailableResponse_GetIsCaptureAvailable(
            HandleRef self);
    }
}
#endif //UNITY_ANDROID

