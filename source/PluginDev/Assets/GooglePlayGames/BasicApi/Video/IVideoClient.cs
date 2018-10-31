// <copyright file="IVideoClient.cs" company="Google Inc.">
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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for interacting with the video recording API.
    /// </summary>
    /// <remarks>
    /// All callbacks in this interface must be invoked on the game thread.
    /// </remarks>
    public interface IVideoClient
    {

        /// <summary>
        /// Fetches the video capabilities of the service.
        /// </summary>
        /// <remarks>Includes whether the mic or front-facing camera are supported,
        /// if the service can write to external storage, and what capture modes
        /// and quality levels are available.
        /// </remarks>
        /// <param name="callback">The callback for the results. The passed capabilities will be non-null if
        /// and only if the request succeeded. This callback will be invoked on the game thread.</param>
        void GetCaptureCapabilities(Action<ResponseStatus, VideoCapabilities> callback);

        /// <summary>
        /// Launches the video capture overlay.
        /// </summary>
        void ShowCaptureOverlay();

        /// <summary>
        /// Fetches the current state of the capture service.
        /// </summary>
        /// <remarks>
        /// This will inform about whether the capture overlay is visible,
        /// if the overlay is actively being used to capture, and much more.
        /// See <see cref="VideoCaptureState"/> for more details.
        /// </remarks>
        /// <param name="callback">The callback for the results. The passed capture state will be non-null if
        /// and only if the request succeeded. This callback will be invoked on the game thread.</param>
        void GetCaptureState(Action<ResponseStatus, VideoCaptureState> callback);

        /// <summary>
        /// Fetches if the capture service is already in use or not.
        /// </summary>
        /// <remarks>
        /// Use this call to check if a start capture api call will return ErrorVideoAlreadyCapturing.
        /// If this returns true, then its safe to start capturing.
        ///
        /// Do not use this call to check if capture is supported, instead use
        /// <see cref="IsCaptureSupported"/> or <see cref="GetCaptureCapabilities"/>.
        /// </remarks>
        /// <param name="captureMode"></param>
        /// <param name="callback">The callback for the results.
        /// This callback will be invoked on the game thread.</param>
        void IsCaptureAvailable(VideoCaptureMode captureMode, Action<ResponseStatus, bool> callback);

        /// <summary>
        /// Synchronous simple check to determine if the device supports capture.
        /// </summary>
        /// <returns>If video capture is supported on this device.</returns>
        bool IsCaptureSupported();

        /// <summary>
        /// Register a listener to listen for changes to the video capture overlay state.
        /// </summary>
        /// <remarks>
        /// Note that only one overlay state listener may be active at a time.
        /// Calling this method while another overlay state listener was previously
        /// registered will replace the original listener with the new one.
        /// </remarks>
        /// <param name="listener"></param>
        void RegisterCaptureOverlayStateChangedListener(CaptureOverlayStateListener listener);

        /// <summary>
        /// Unregisters this client's overlay state update listener, if any.
        /// </summary>
        void UnregisterCaptureOverlayStateChangedListener();
    }
}
