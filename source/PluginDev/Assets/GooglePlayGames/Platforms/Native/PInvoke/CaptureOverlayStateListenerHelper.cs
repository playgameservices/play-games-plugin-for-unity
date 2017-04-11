// <copyright file="CaptureOverlayStateListenerHelper.cs" company="Google Inc.">
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
    using GooglePlayGames.OurUtils;
    using C = GooglePlayGames.Native.Cwrapper.CaptureOverlayStateListenerHelper;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal class CaptureOverlayStateListenerHelper : BaseReferenceHolder
    {

        internal CaptureOverlayStateListenerHelper(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.CaptureOverlayStateListenerHelper_Dispose(selfPointer);
        }

        internal CaptureOverlayStateListenerHelper SetOnCaptureOverlayStateChangedCallback(
            Action<Types.VideoCaptureOverlayState> callback)
        {
            C.CaptureOverlayStateListenerHelper_SetOnCaptureOverlayStateChangedCallback(SelfPtr(),
                InternalOnCaptureOverlayStateChangedCallback,
                Callbacks.ToIntPtr(callback));
            return this;
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnCaptureOverlayStateChangedCallback))]
        internal static void InternalOnCaptureOverlayStateChangedCallback(Types.VideoCaptureOverlayState response, IntPtr data)
        {
            var callback = Callbacks.IntPtrToPermanentCallback<Action<Types.VideoCaptureOverlayState>>(data);

            try
            {
                callback(response);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing InternalOnCaptureOverlayStateChangedCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal static CaptureOverlayStateListenerHelper Create()
        {
            return new CaptureOverlayStateListenerHelper(C.CaptureOverlayStateListenerHelper_Construct());
        }
    }
}

#endif
