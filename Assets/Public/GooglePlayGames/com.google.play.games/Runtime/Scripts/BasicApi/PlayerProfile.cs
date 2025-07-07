// <copyright file="Player.cs" company="Google Inc.">
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

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi {
  /// <summary>
  /// Represents a player, a real-world person (tied to a Games account).
  /// </summary>
  public class PlayerProfile : PlayGamesUserProfile {
    /// <summary>
    /// Constructor for PlayerProfile.
    /// </summary>
    /// <param name="displayName">The display name of the player.</param>
    /// <param name="playerId">The player ID of the player.</param>
    /// <param name="avatarUrl">The URL of the player's avatar.</param>
    /// <param name="isFriend">Whether the player is a friend of the current player.</param>
    internal PlayerProfile(string displayName, string playerId, string avatarUrl, bool isFriend)
        : base(displayName, playerId, avatarUrl, isFriend) {}
  }
}
#endif
