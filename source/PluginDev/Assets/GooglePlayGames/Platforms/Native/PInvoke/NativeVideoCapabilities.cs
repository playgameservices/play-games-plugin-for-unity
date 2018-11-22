// <copyright file="NativeVideoCapabilities.cs" company="Google Inc.">
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


#if UNITY_ANDROID

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using GooglePlayGames.Native.Cwrapper;


    internal class NativeVideoCapabilities : BaseReferenceHolder {

        internal NativeVideoCapabilities(IntPtr selfPtr) :base(selfPtr)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            VideoCapabilities.VideoCapabilities_Dispose(selfPointer);
        }

        internal bool IsCameraSupported()
        {
            return VideoCapabilities.VideoCapabilities_IsCameraSupported(SelfPtr());
        }

        internal bool IsMicSupported()
        {
            return VideoCapabilities.VideoCapabilities_IsMicSupported(SelfPtr());
        }

        internal bool IsWriteStorageSupported()
        {
            return VideoCapabilities.VideoCapabilities_IsWriteStorageSupported(SelfPtr());
        }

        internal bool SupportsCaptureMode(Types.VideoCaptureMode captureMode)
        {
            return VideoCapabilities.VideoCapabilities_SupportsCaptureMode(SelfPtr(), captureMode);
        }

        internal bool SupportsQualityLevel(Types.VideoQualityLevel qualityLevel)
        {
            return VideoCapabilities.VideoCapabilities_SupportsQualityLevel(SelfPtr(), qualityLevel);
        }

        internal static NativeVideoCapabilities FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new NativeVideoCapabilities(pointer);
        }
    }

}

#endif
