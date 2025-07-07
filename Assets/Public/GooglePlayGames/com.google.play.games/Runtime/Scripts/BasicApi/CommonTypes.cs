// <copyright file="CommonTypes.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi
{
    /// <summary>
    /// A enum describing where game data can be fetched from.
    /// </summary>
    public enum DataSource
    {
        /// <summary>
        /// Allow a read from either a local cache, or the network.
        /// </summary>
        /// <remarks> Values from the cache may be
        /// stale (potentially producing more write conflicts), but reading from cache may still
        /// allow reads to succeed if the device does not have internet access and may complete more
        /// quickly (as the reads can occur locally rather requiring network roundtrips).
        /// </remarks>
        ReadCacheOrNetwork,

        /// <summary>
        /// Only allow reads from network.
        /// </summary>
        /// <remarks> This guarantees any returned values were current at the time
        /// the read succeeded, but prevents reads from succeeding if the network is unavailable for
        /// any reason.
        /// </remarks>
        ReadNetworkOnly
    }

    /// <summary> Native response status codes</summary>
    /// <remarks> These values are returned by the native SDK API.
    /// NOTE: These values are different than the CommonStatusCodes.
    /// </remarks>
    public enum ResponseStatus
    {
        /// <summary>The operation was successful.</summary>
        Success = 1,

        /// <summary>The operation was successful, but the device's cache was used.</summary>
        SuccessWithStale = 2,

        /// <summary>The application is not licensed to the user.</summary>
        LicenseCheckFailed = -1,

        /// <summary>An internal error occurred.</summary>
        InternalError = -2,

        /// <summary>The player is not authorized to perform the operation.</summary>
        NotAuthorized = -3,

        /// <summary>The installed version of Google Play services is out of date.</summary>
        VersionUpdateRequired = -4,

        /// <summary>Timed out while awaiting the result.</summary>
        Timeout = -5,

        ///< summary>
        /// Constant indicating that the developer does not have access to the friends list, but can
        /// call the AskForLoadFriendsResolution API to show a consent dialog.
        ///</summary>
        ResolutionRequired = -6,
    }

    /// <summary> Native response status codes for UI operations.</summary>
    /// <remarks> These values are returned by the native SDK API.
    /// </remarks>
    public enum UIStatus
    {
        /// <summary>The result is valid.</summary>
        Valid = 1,

        /// <summary>An internal error occurred.</summary>
        InternalError = -2,

        /// <summary>The player is not authorized to perform the operation.</summary>
        NotAuthorized = -3,

        /// <summary>The installed version of Google Play services is out of date.</summary>
        VersionUpdateRequired = -4,

        /// <summary>Timed out while awaiting the result.</summary>
        Timeout = -5,

        /// <summary>UI closed by user.</summary>
        UserClosedUI = -6,
        UiBusy = -12,

        /// <summary>An network error occurred.</summary>
        NetworkError = -20,
    }

    /// <summary>Values specifying the start location for fetching scores.</summary>
    public enum LeaderboardStart
    {
        /// <summary>Start fetching scores from the top of the list.</summary>
        TopScores = 1,

        /// <summary>Start fetching relative to the player's score.</summary>
        PlayerCentered = 2,
    }

    /// <summary>Values specifying which leaderboard timespan to use.</summary>
    public enum LeaderboardTimeSpan
    {
        /// <summary>Daily scores.  The day resets at 11:59 PM PST.</summary>
        Daily = 1,

        /// <summary>Weekly scores.  The week resets at 11:59 PM PST on Sunday.</summary>
        Weekly = 2,

        /// <summary>All time scores.</summary>
        AllTime = 3,
    }

    /// <summary>Values specifying which leaderboard collection to use.</summary>
    public enum LeaderboardCollection
    {
        /// <summary>Public leaderboards contain the scores of players who are sharing their gameplay publicly.</summary>
        Public = 1,

        /// <summary>Social leaderboards contain the scores of players in the viewing player's circles.</summary>
        Social = 2,
    }

    /// <summary>Values specifying the visibility status of the friends list.</summary>
    public enum FriendsListVisibilityStatus
    {
        ///< summary>
        /// Constant indicating that currently it's unknown if the friends list is visible to the
        /// game, game can ask for permission from user.
        ///</summary>
        Unknown = 0,

        /// Constant indicating that the friends list is currently visible to the game.
        Visible = 1,

        ///< summary>
        /// Constant indicating that the developer does not have access to the friends list, but can
        /// call the AskForLoadFriendsResolution API to show a consent dialog.
        ///</summary>
        ResolutionRequired = 2,

        ///< summary>
        /// Constant indicating that the friends list is currently unavailable for this user, and it
        /// is not possible to request access at this time, either because the user has permanently
        /// declined or the friends feature is not available to them. In this state, any attempts to
        /// request
        /// access to the friends list will be unsuccessful.
        ///</summary>
        Unavailable = 3,

        /// <summary>An network error occurred.</summary>
        NetworkError = -4,

        /// <summary>The player is not authorized to perform the operation.</summary>
        NotAuthorized = -5,
    }

    /// <summary>Values specifying the status of the friends list.</summary>
    public enum LoadFriendsStatus
    {
        /// <summary>An unknown value to return when loadFriends is not available.</summary>
        Unknown = 0,

        /// <summary>All the friends have been loaded.</summary>
        Completed = 1,

        /// <summary>There are more friends to load.</summary>
        LoadMore = 2,

        /// <summary>
        /// The game doesn't have permission to access the player's friends list. No friends loaded.
        /// </summary>
        ResolutionRequired = -3,

        /// <summary>An internal error occurred.</summary>
        InternalError = -4,

        /// <summary>The player is not authorized to perform the operation.</summary>
        NotAuthorized = -5,

        /// <summary>An network error occurred.</summary>
        NetworkError = -6,
    }

    /// <summary>
    /// Utility class for common types.
    /// This class checks if a response status is a success or not.
    /// </summary>
    public class CommonTypesUtil
    {
        /// <summary>
        /// Checks if a response status is a success or not.
        /// </summary>
        public static bool StatusIsSuccess(ResponseStatus status)
        {
            return ((int) status) > 0;
        }
    }
}
