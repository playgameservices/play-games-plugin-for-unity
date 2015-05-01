// <copyright file="IGameMangerImpl.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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

namespace NearbyDroids
{
    using System.Collections.Generic;

    /// <summary>
    /// game manger implementation interface.  This is used
    /// to define the interactions between the game manager and the
    /// various implementations.
    /// </summary>
    internal interface IGameManager
    {
        /// <summary>
        /// Initialize this instance.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Initializes the level.
        /// </summary>
        void InitializeLevel();

        /// <summary>
        /// Called from the Update() event
        /// </summary>
        void OnUpdate();

        /// <summary>
        /// Adds the enemy to list.
        /// </summary>
        /// <param name="enemy">Enemy.</param>
        void AddEnemyToList(EnemyController enemy);

        /// <summary>
        /// Raises the new game state event.
        /// </summary>
        /// <param name="newState">New state.</param>
        /// <param name="changes">Changes.</param>
        void OnNewGameState(GameManager.GameStateData newState, List<ItemState> changes);
    }
}
