// <copyright file="ConversionUtils.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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


namespace GooglePlayGames.Native
{
    using System;
    using GooglePlayGames.BasicApi;
    using UnityEngine;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal static class ConversionUtils
    {

        internal static ResponseStatus ConvertResponseStatus(Status.ResponseStatus status)
        {
            switch (status)
            {
                case Status.ResponseStatus.VALID:
                    return ResponseStatus.Success;
                case Status.ResponseStatus.VALID_BUT_STALE:
                    return ResponseStatus.SuccessWithStale;
                case Status.ResponseStatus.ERROR_INTERNAL:
                    return ResponseStatus.InternalError;
                case Status.ResponseStatus.ERROR_LICENSE_CHECK_FAILED:
                    return ResponseStatus.LicenseCheckFailed;
                case Status.ResponseStatus.ERROR_NOT_AUTHORIZED:
                    return ResponseStatus.NotAuthorized;
                case Status.ResponseStatus.ERROR_TIMEOUT:
                    return ResponseStatus.Timeout;
                case Status.ResponseStatus.ERROR_VERSION_UPDATE_REQUIRED:
                    return ResponseStatus.VersionUpdateRequired;
                default:
                    throw new InvalidOperationException("Unknown status: " + status);
            }
        }

        internal static CommonStatusCodes ConvertResponseStatusToCommonStatus(Status.ResponseStatus status)
        {
            switch (status) {
            case Status.ResponseStatus.VALID:
                return CommonStatusCodes.Success;
            case Status.ResponseStatus.VALID_BUT_STALE:
                return CommonStatusCodes.SuccessCached;
            case Status.ResponseStatus.ERROR_INTERNAL:
                return CommonStatusCodes.InternalError;
            case Status.ResponseStatus.ERROR_LICENSE_CHECK_FAILED:
                return CommonStatusCodes.LicenseCheckFailed;
            case Status.ResponseStatus.ERROR_NOT_AUTHORIZED:
                return CommonStatusCodes.AuthApiAccessForbidden;
            case Status.ResponseStatus.ERROR_TIMEOUT:
                return CommonStatusCodes.Timeout;
            case Status.ResponseStatus.ERROR_VERSION_UPDATE_REQUIRED:
                return CommonStatusCodes.ServiceVersionUpdateRequired;
            default:
                Debug.LogWarning("Unknown ResponseStatus: " + status +
                    ", defaulting to CommonStatusCodes.Error");
                return CommonStatusCodes.Error;
            }
        }

        internal static UIStatus ConvertUIStatus(Status.UIStatus status)
        {
            switch (status)
            {
                case Status.UIStatus.VALID:
                    return UIStatus.Valid;
                case Status.UIStatus.ERROR_INTERNAL:
                    return UIStatus.InternalError;
                case Status.UIStatus.ERROR_NOT_AUTHORIZED:
                    return UIStatus.NotAuthorized;
                case Status.UIStatus.ERROR_TIMEOUT:
                    return UIStatus.Timeout;
                case Status.UIStatus.ERROR_VERSION_UPDATE_REQUIRED:
                    return UIStatus.VersionUpdateRequired;
                case Status.UIStatus.ERROR_CANCELED:
                    return UIStatus.UserClosedUI;
                case Status.UIStatus.ERROR_UI_BUSY:
                    return UIStatus.UiBusy;
                default:
                    throw new InvalidOperationException("Unknown status: " + status);
            }
        }

        internal static Types.DataSource AsDataSource(DataSource source)
        {
            switch (source)
            {
                case DataSource.ReadCacheOrNetwork:
                    return Types.DataSource.CACHE_OR_NETWORK;
                case DataSource.ReadNetworkOnly:
                    return Types.DataSource.NETWORK_ONLY;
                default:
                    throw new InvalidOperationException("Found unhandled DataSource: " + source);
            }
        }

        internal static Types.VideoCaptureMode ConvertVideoCaptureMode(VideoCaptureMode captureMode)
        {
            switch (captureMode)
            {
                case VideoCaptureMode.File:
                    return Types.VideoCaptureMode.FILE;
                case VideoCaptureMode.Stream:
                    return Types.VideoCaptureMode.STREAM;
                case VideoCaptureMode.Unknown:
                    return Types.VideoCaptureMode.UNKNOWN;
                default:
                    Debug.LogWarning("Unknown VideoCaptureMode: " + captureMode +
                        ", defaulting to Types.VideoCaptureMode.UNKNOWN.");
                    return Types.VideoCaptureMode.UNKNOWN;
            }
        }

        internal static VideoCaptureMode ConvertNativeVideoCaptureMode(Types.VideoCaptureMode nativeCaptureMode)
        {
            switch (nativeCaptureMode)
            {
                case Types.VideoCaptureMode.FILE:
                    return VideoCaptureMode.File;
                case Types.VideoCaptureMode.STREAM:
                    return VideoCaptureMode.Stream;
                case Types.VideoCaptureMode.UNKNOWN:
                    return VideoCaptureMode.Unknown;
                default:
                    Debug.LogWarning("Unknown Types.VideoCaptureMode: " + nativeCaptureMode +
                        ", defaulting to VideoCaptureMode.Unknown.");
                    return VideoCaptureMode.Unknown;
            }
        }

        internal static VideoQualityLevel ConvertNativeVideoQualityLevel(Types.VideoQualityLevel nativeQualityLevel)
        {
            switch (nativeQualityLevel)
            {
                case Types.VideoQualityLevel.SD:
                    return VideoQualityLevel.SD;
                case Types.VideoQualityLevel.HD:
                    return VideoQualityLevel.HD;
                case Types.VideoQualityLevel.XHD:
                    return VideoQualityLevel.XHD;
                case Types.VideoQualityLevel.FULLHD:
                    return VideoQualityLevel.FullHD;
                case Types.VideoQualityLevel.UNKNOWN:
                    return VideoQualityLevel.Unknown;
                default:
                    Debug.LogWarning("Unknown Types.VideoQualityLevel: " + nativeQualityLevel +
                        ", defaulting to VideoQualityLevel.Unknown.");
                    return VideoQualityLevel.Unknown;
            }
        }

        internal static VideoCaptureOverlayState ConvertNativeVideoCaptureOverlayState(Types.VideoCaptureOverlayState nativeOverlayState)
        {
            switch (nativeOverlayState)
            {
                case Types.VideoCaptureOverlayState.DISMISSED:
                    return VideoCaptureOverlayState.Dismissed;
                case Types.VideoCaptureOverlayState.SHOWN:
                    return VideoCaptureOverlayState.Shown;
                case Types.VideoCaptureOverlayState.STARTED:
                    return VideoCaptureOverlayState.Started;
                case Types.VideoCaptureOverlayState.STOPPED:
                    return VideoCaptureOverlayState.Stopped;
                case Types.VideoCaptureOverlayState.UNKNOWN:
                    return VideoCaptureOverlayState.Unknown;
                default:
                    Debug.LogWarning("Unknown Types.VideoCaptureOverlayState: " + nativeOverlayState +
                        ", defaulting to VideoCaptureOverlayState.Unknown.");
                    return VideoCaptureOverlayState.Unknown;
            }
        }

    }
}
#endif // #if (UNITY_ANDROID || UNITY_IPHONE)
