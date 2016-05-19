// <copyright file="RemoteGameManager.cs" company="Google Inc.">
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
    using UnityEngine;
    using UnityEngine.SceneManagement;
    /// <summary>
    /// Remote game manager. used to manage the game state for a player
    /// that is not hosting the room.
    /// </summary>
    public class RemoteGameManager : IGameManager
    {
        // the outer game manager.
        private GameManager owner;

        public RemoteGameManager(GameManager owner)
        {
            this.owner = owner;
        }

        public void Initialize()
        {
            // nothing
        }

        public void AddEnemyToList(EnemyController enemy)
        {
            // nothing
        }

        /// <summary>
        /// Initializes the level.
        /// </summary>
        public void InitializeLevel()
        {
            // copy the state from the room to the game
            Debug.Log("OnLevelLoaded - initialize the Game!");
            InitGame(owner.GameState);
        }

        public void OnUpdate()
        {
            // nothing to do...
        }

        // Initializes the game for each level.
        internal void InitGame(GameManager.GameStateData data)
        {
            if (data != null)
            {
                Debug.Log("Initializing using " + data.CurrentPhase + ": " +
                    data.SerialNumber);

                // if we are moving to the next level of the game, reload!
                if (owner.Level != data.Level)
                {
                    Debug.Log("Going from level " + owner.Level + " to " +
                        data.Level);
                    owner.GameState = data;
                    owner.LevelManager.DestroyBoard();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    return;
                }

                // Call the SetupScene function of the BoardManager script,
                // pass it current level number.
                owner.LevelManager.SetupScene(data.Level, data.LevelData);

                // associate each player with the player state received.
                foreach (PlayerInfo.PlayerData p in data.Players)
                {
                    PlayerInfo player = PlayerInfo.GetPlayer(p.DeviceId);
                    if (player != null)
                    {
                        player.DataState.AvatarIndex = p.AvatarIndex;
                        player.DataState.Name = p.Name;
                        player.Score = p.score;
                    }
                    else
                    {
                        // the endpoint is not known for players that are not local
                        // so make one up that is unique.
                        player = PlayerInfo.AddPendingPlayer(
                            new NearbyPlayer(
                                p.DeviceId,
                                "unknown" + p.DeviceId,
                                p.Name),
                            p.AvatarIndex);
                    }

                    owner.CreatePlayerScorePanel(player);
                }
            }
        }

        /// <summary>
        /// Processes the changes received from the room.
        /// </summary>
        /// <param name="changes">Changes in item state to proces.</param>
        internal void ProcessChanges(List<ItemState> changes)
        {
            Debug.Log("Processing " + changes.Count + " changes!");
            foreach (ItemState item in changes)
            {
                Debug.Log("---> " + item.Name);
                GameObject obj = owner.LevelManager.Find(item.Name);
                if (obj != null)
                {
                    // use the moving object so we get smooth movement.
                    MovingObject moving = obj.GetComponent<MovingObject>();

                    if (moving != null && !moving.BroadcastMovement)
                    {
                        moving.MoveTo(item.Position);
                    }
                    else
                    {
                        obj.transform.position = item.Position;
                    }

                    obj.transform.rotation = item.Rotation;

                    // handle enemies in a clever way to get the animation.
                    EnemyController enemy = obj.GetComponent<EnemyController>();
                    if (enemy != null && obj.activeSelf && obj.activeSelf != item.Enabled)
                    {
                        enemy.Explode();
                    }
                    else
                    {
                        obj.SetActive(item.Enabled);
                    }
                }
                else if (item.Name == GameManager.PlayerTurnFlagName)
                {
                    // toggle the turn flag.
                    if (item.Enabled)
                    {
                        foreach (PlayerInfo p in PlayerInfo.AllPlayers)
                        {
                            if (p.IsLocal)
                            {
                                p.Moved = false;
                            }
                        }
                    }
                }
                else if (item.Name.StartsWith(GameManager.ScoreChangedItemName))
                {
                    // update the scores
                    PlayerInfo p = PlayerInfo.GetPlayer(item.TileSetName);
                    if (p != null)
                    {
                        p.Score = (p.Score > item.PrefabIndex) ? p.Score : item.PrefabIndex;
                    }
                }
                else
                {
                    // create new objects that showed up 
                    // since the scene was initialized.
                    Debug.Log("Creating " + item.Name);
                    GameObject newObj = owner.LevelManager.CreateItem(item);
                    if (newObj.GetComponent<PlayerController>() != null)
                    {
                        string devId = newObj.name.Replace(ItemState.PlayerTileSet, string.Empty);
                        PlayerInfo p = PlayerInfo.GetPlayer(devId);
                        if (p != null)
                        {
                            if (newObj.GetComponent<PlayerController>().Player == null)
                            {
                                newObj.GetComponent<PlayerController>().Player = p;
                                newObj.GetComponent<PlayerController>().BroadcastMovement = p.IsLocal;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raises the new game state event.
        /// </summary>
        /// <param name="newState">New state of the game.</param>
        /// <param name="changes">Changes received from the room</param>
        public void OnNewGameState(
            GameManager.GameStateData newState,
            List<ItemState> changes)
        {
            if (newState != null)
            {
                InitGame(newState);

                // record the game state if we are starting out or on a new
                // level.
                if (newState.CurrentPhase == GameManager.GameStateData.Phase.Connecting ||
                    newState.CurrentPhase == GameManager.GameStateData.Phase.Initializing)
                {
                    owner.GameState = newState;
                }
            }

            if (changes != null)
            {
                ProcessChanges(changes);
            }
        }
    }
}
