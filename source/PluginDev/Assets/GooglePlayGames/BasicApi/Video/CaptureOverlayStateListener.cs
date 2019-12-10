// <copyright file="CaptureOverlayStateListener.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All rights reserved.
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
    /// <summary>
    /// Capture overlay state listener, will be called to notify you of changes
    /// to the state of the capture overlay.
    /// </summary>
    public interface CaptureOverlayStateListener
    {
        /// <summary>
        /// Called when the state of the capture overlay changes.
        /// </summary>
        /// <param name="overlayState">The current capture overlay state.</param>
        void OnCaptureOverlayStateChanged(VideoCaptureOverlayState overlayState);
    }
}