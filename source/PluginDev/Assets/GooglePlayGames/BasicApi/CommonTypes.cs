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
        /// Allow a read from either a local cache, or the network. Values from the cache may be
        /// stale (potentially producing more write conflicts), but reading from cache may still
        /// allow reads to succeed if the device does not have internet access and may complete more
        /// quickly (as the reads can occur locally rather requiring network roundtrips).
        /// </summary>
        ReadCacheOrNetwork,

        /// <summary>
        /// Only allow reads from network. This guarantees any returned values were current at the time
        /// the read succeeded, but prevents reads from succeeding if the network is unavailable for
        /// any reason.
        /// </summary>
        ReadNetworkOnly
      }

      public enum ResponseStatus
      {
        Success = 1,
        SuccessWithStale = 2,
        LicenseCheckFailed = -1,
        InternalError = -2,
        NotAuthorized = -3,
        VersionUpdateRequired = -4,
        Timeout = -5,
      }

      public enum UIStatus
      {
        Valid = 1,
        InternalError = -2,
        NotAuthorized = -3,
        VersionUpdateRequired = -4,
        Timeout = -5,
        UserClosedUI = -6,
        UiBusy = -12,
        LeftRoom = -18,
      }

      public enum LeaderboardStart
      {
        TopScores = 1,
        PlayerCentered = 2,
      }

      public enum LeaderboardTimeSpan
      {
        Daily = 1,
        Weekly = 2,
        AllTime = 3,
      }

      public enum LeaderboardCollection
      {
        Public = 1,
        Social = 2,
      }
}
