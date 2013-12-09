/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace GooglePlayGames.BasicApi {
    /// <summary>
    /// Set of callbacks for app state (a.k.a. "cloud save") loading and conflict resolution.
    /// </summary>
    public interface OnStateLoadedListener {
        /// <summary>
        /// Load callback. Called when app state data has been loaded from the servers
        /// or from the local cache.
        /// </summary>
        /// <param name="success">Indicates whether the load operation was successful.
        /// If <c>true</c>, the data is available in the <c>data</c> parameter.
        /// If <c>false</c>, the operation failed.</param>
        /// <param name="slot">The app state slot number that was loaded.</param>
        /// <param name="data">The data that was loaded.</para>
        void OnStateLoaded(bool success, int slot, byte[] data);

        /// <summary>
        /// Called when a conflict is detected between local data and the data that
        /// exists on the server. An implementation of this method must compare the two
        /// sets of data and submit a resolved data set to the server by making a call
        /// to the <see cref="GooglePlayGames.PlayGamesPlatform.ResolveState" /> method.
        /// </summary>
        /// <param name='slot'>
        /// The slot number where the conflict occurred.
        /// </param>
        /// <param name='version'>
        /// The version string. Pass this parameter to
        /// <see cref="GooglePlaygames.PlayGamesPlatform.ResolveState" /> when submitting
        /// the resolved state.
        /// </param>
        /// <param name='localData'>
        /// The local data.
        /// </param>
        /// <param name='serverData'>
        /// The server data.
        /// </param>
        byte[] OnStateConflict(int slot, byte[] localData, byte[] serverData);

        // Called to report the result of updating state to the cloud
        void OnStateSaved(bool success, int slot);
    }
}
