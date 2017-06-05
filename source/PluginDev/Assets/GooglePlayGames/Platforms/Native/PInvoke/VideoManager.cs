// <copyright file="VideoManager.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All Rights Reserved.
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.OurUtils;
    using C = GooglePlayGames.Native.Cwrapper.VideoManager;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using UnityEngine.SocialPlatforms;

    internal class VideoManager
    {

        private readonly GameServices mServices;

        internal VideoManager(GameServices services)
        {
            mServices = Misc.CheckNotNull(services);
        }

        internal int NumCaptureModes
        {
            get
            {
                return 2;
            }
        }

        internal int NumQualityLevels
        {
            get
            {
                return 4;
            }
        }

        internal void GetCaptureCapabilities(Action<GetCaptureCapabilitiesResponse> callback)
        {
            C.VideoManager_GetCaptureCapabilities(
                mServices.AsHandle(),
                InternalCaptureCapabilitiesCallback,
                Callbacks.ToIntPtr<GetCaptureCapabilitiesResponse>(callback, GetCaptureCapabilitiesResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.CaptureCapabilitiesCallback))]
        internal static void InternalCaptureCapabilitiesCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "VideoManager#CaptureCapabilitiesCallback", Callbacks.Type.Temporary, response, data);
        }

        internal void ShowCaptureOverlay()
        {
            C.VideoManager_ShowCaptureOverlay(mServices.AsHandle());
        }

        internal void GetCaptureState(Action<GetCaptureStateResponse> callback)
        {
            C.VideoManager_GetCaptureState(
                mServices.AsHandle(),
                InternalCaptureStateCallback,
                Callbacks.ToIntPtr<GetCaptureStateResponse>(callback, GetCaptureStateResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.CaptureStateCallback))]
        internal static void InternalCaptureStateCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "VideoManager#CaptureStateCallback", Callbacks.Type.Temporary, response, data);
        }

        internal void IsCaptureAvailable(Types.VideoCaptureMode captureMode, Action<IsCaptureAvailableResponse> callback)
        {
            C.VideoManager_IsCaptureAvailable(
                mServices.AsHandle(),
                captureMode,
                InternalIsCaptureAvailableCallback,
                Callbacks.ToIntPtr<IsCaptureAvailableResponse>(callback, IsCaptureAvailableResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.IsCaptureAvailableCallback))]
        internal static void InternalIsCaptureAvailableCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "VideoManager#IsCaptureAvailableCallback", Callbacks.Type.Temporary, response, data);
        }

        internal bool IsCaptureSupported()
        {
            return C.VideoManager_IsCaptureSupported(mServices.AsHandle());
        }

        internal void RegisterCaptureOverlayStateChangedListener(CaptureOverlayStateListenerHelper helper)
        {
            C.VideoManager_RegisterCaptureOverlayStateChangedListener(mServices.AsHandle(), helper.AsPointer());
        }

        internal void UnregisterCaptureOverlayStateChangedListener()
        {
            C.VideoManager_UnregisterCaptureOverlayStateChangedListener(mServices.AsHandle());
        }
    }

    internal class GetCaptureCapabilitiesResponse : BaseReferenceHolder
    {

        internal GetCaptureCapabilitiesResponse(IntPtr selfPointer) : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.VideoManager_GetCaptureCapabilitiesResponse_Dispose(SelfPtr());
        }

        internal Status.ResponseStatus GetStatus()
        {
            return C.VideoManager_GetCaptureCapabilitiesResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded()
        {
            return GetStatus() > 0;
        }

        internal NativeVideoCapabilities GetData()
        {
            return NativeVideoCapabilities.FromPointer(
                C.VideoManager_GetCaptureCapabilitiesResponse_GetVideoCapabilities(SelfPtr()));
        }

        internal static GetCaptureCapabilitiesResponse FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new GetCaptureCapabilitiesResponse(pointer);
        }
    }

    internal class GetCaptureStateResponse : BaseReferenceHolder
    {
        internal GetCaptureStateResponse(IntPtr selfPointer) : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.VideoManager_GetCaptureStateResponse_Dispose(SelfPtr());
        }

        internal NativeVideoCaptureState GetData()
        {
            return NativeVideoCaptureState.FromPointer(
                C.VideoManager_GetCaptureStateResponse_GetVideoCaptureState(SelfPtr()));
        }

    internal Status.ResponseStatus GetStatus()
        {
            return C.VideoManager_GetCaptureStateResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded()
        {
            return GetStatus() > 0;
        }

        internal static GetCaptureStateResponse FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new GetCaptureStateResponse(pointer);
        }
    }

    internal class IsCaptureAvailableResponse : BaseReferenceHolder
    {
        internal IsCaptureAvailableResponse(IntPtr selfPointer) : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.VideoManager_IsCaptureAvailableResponse_Dispose(selfPointer);
        }

    internal Status.ResponseStatus GetStatus()
        {
            return C.VideoManager_IsCaptureAvailableResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded()
        {
            return GetStatus() > 0;
        }

        internal bool IsCaptureAvailable()
        {
            return C.VideoManager_IsCaptureAvailableResponse_GetIsCaptureAvailable(SelfPtr());
        }

        internal static IsCaptureAvailableResponse FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new IsCaptureAvailableResponse(pointer);
        }
    }
}


#endif
