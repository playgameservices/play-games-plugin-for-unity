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

namespace GooglePlayGames.BasicApi.Multiplayer
{
    /// <summary>
    /// Represents a player. A player is different from a participant! The participant is
    /// an entity that takes part in a particular match; a Player is a real-world person
    /// (tied to a Games account). The player exists across matches, the Participant
    /// only exists in the context of a particular match.
    /// </summary>
    public class Player : PlayGamesUserProfile
    {
        internal Player(string displayName, string playerId, string avatarUrl)
            : base(displayName, playerId, avatarUrl)
        {
        }
    }
}
#endif