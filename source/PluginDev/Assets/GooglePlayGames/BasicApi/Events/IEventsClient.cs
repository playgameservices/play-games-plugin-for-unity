// <copyright file="IEventsClient.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi.Events
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for interacting with events.
    ///
    /// <para>See online <a href="https://developers.google.com/games/services/common/concepts/events">
    /// documentation for Events</a> for more information.</para>
    ///
    /// All callbacks in this interface must be invoked on the game thread.
    /// </summary>
    public interface IEventsClient
    {
        /// <summary>
        /// Fetches all events defined for this game.
        /// </summary>
        /// <param name="source">The source of the event (i.e. whether we can return stale cached
        /// values).</param>
        /// <param name="callback">A callback for the results of the request. The passed list will only
        /// be non-empty if the request succeeded. This callback will be invoked on the game thread.
        /// </param>
        void FetchAllEvents(DataSource source, Action<ResponseStatus, List<IEvent>> callback);

        /// <summary>
        /// Fetchs the event with the specified ID.
        /// </summary>
        /// <param name="source">The source of the event (i.e. whether we can return stale cached
        /// values).</param>
        /// <param name="eventId">The ID of the event.</param>
        /// <param name="callback">A callback for the result of the event. If the request failed, the
        /// passed event will be null. This callback will be invoked on the game thread.</param>
        void FetchEvent(DataSource source, string eventId, Action<ResponseStatus, IEvent> callback);

        /// <summary>
        /// Increments the indicated event.
        /// </summary>
        /// <param name="eventId">The ID of the event to increment.</param>
        /// <param name="stepsToIncrement">The number of steps to increment by.</param>
        void IncrementEvent(string eventId, uint stepsToIncrement);
    }
}
#endif
