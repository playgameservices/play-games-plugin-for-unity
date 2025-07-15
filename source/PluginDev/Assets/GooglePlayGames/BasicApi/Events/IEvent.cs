// <copyright file="IEvent.cs" company="Google Inc.">
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

using System;

namespace GooglePlayGames.BasicApi.Events
{
    public enum EventVisibility
    {
        Hidden = 1,
        Revealed = 2,
    }

    /// <summary>
    /// @deprecated This interface will be removed in the future in favor of Unity Games V2 Plugin.
    /// Data object representing an Event. <see cref="Native.PInvoke.EventManager"/> for more.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Gets the ID of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        string Id { get; }

        /// <summary>
        /// Gets the name of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the description of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        string Description { get; }

        /// <summary>
        /// Gets the URL of the image for the event. Empty if there is no image.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        string ImageUrl { get; }

        /// <summary>
        /// Gets the current count for this event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        ulong CurrentCount { get; }

        /// <summary>
        /// Gets the visibility of the event.
        /// </summary>
        /// <remarks>
        /// @deprecated This property will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        EventVisibility Visibility { get; }
    }
}
