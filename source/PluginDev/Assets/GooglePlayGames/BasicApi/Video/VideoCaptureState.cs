// <copyright file="VideoCaptureState.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi.Video
{
    using System.Collections.Generic;
    using System.Linq;
    using GooglePlayGames.OurUtils;

    /// <summary>
    /// Represents the video recording capabilities.
    /// </summary>
    public class VideoCaptureState
    {
        private bool mIsCapturing;
        private VideoCaptureMode mCaptureMode;
        private VideoQualityLevel mQualityLevel;
        private bool mIsOverlayVisible;
        private bool mIsPaused;

        internal VideoCaptureState(bool isCapturing, VideoCaptureMode captureMode,
            VideoQualityLevel qualityLevel, bool isOverlayVisible, bool isPaused)
        {
            mIsCapturing = isCapturing;
            mCaptureMode = captureMode;
            mQualityLevel = qualityLevel;
            mIsOverlayVisible = isOverlayVisible;
            mIsPaused = isPaused;
        }

        /// <summary>Returns whether the service is currently capturing or not.</summary>
        public bool IsCapturing
        {
            get
            {
                return mIsCapturing;
            }
        }

        /// <summary>Returns the capture mode of the current capture.</summary>
        public VideoCaptureMode CaptureMode
        {
            get
            {
                return mCaptureMode;
            }
        }

        /// <summary>Returns the quality level of the current capture.</summary>
        public VideoQualityLevel QualityLevel
        {
            get
            {
                return mQualityLevel;
            }
        }

        /// <summary>
        /// Returns whether the capture overlay is currently visible or not.
        /// </summary>
        /// <remarks>
        /// This also indicates the capture overlay is being used by the user and background capture will fail.
        /// </remarks>
        public bool IsOverlayVisible
        {
            get
            {
                return mIsOverlayVisible;
            }
        }

        /// <summary>
        /// Returns whether the capture is currently paused or not.
        /// </summary>
        /// <remarks>
        /// Will always be <code>false</code> if <code>IsCapturing</code> if <code>false</code>.
        /// </remarks>
        public bool IsPaused
        {
            get
            {
                return mIsPaused;
            }
        }

        public override string ToString()
        {
            return string.Format("[VideoCaptureState: mIsCapturing={0}, mCaptureMode={1}, mQualityLevel={2}, " +
                "mIsOverlayVisible={3}, mIsPaused={4}]",
                mIsCapturing,
                mCaptureMode.ToString(),
                mQualityLevel.ToString(),
                mIsOverlayVisible,
                mIsPaused);
        }
    }
}