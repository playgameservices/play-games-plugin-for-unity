// <copyright file="LocalGameManager.cs" company="Google Inc.">
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
    using System.Collections;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi.Nearby;
    using UnityEngine;

    /// <summary>
    /// Local game manager. This manages standalone and hosting
    /// multiplayer games.  As such, it creates the scene layout and
    /// keeps track of player scores and overall game state.
    /// </summary>
    public class LocalGameManager : IGameManager
    {
        // true if the enemies are currently moving
        private bool enemiesMoving;

        // the outer gamemanager "owner"
        private GameManager owner;

        // the timer for all the players to take a turn
        private float playerTurnTimer = 0f;

        // the interval for the player to make a move
        private float playerTurnInterval = 2f;

        // Dictionary to keep track of enemies.  We use a dictionary so
        // we can quickly lookup the enemy and sync there state with others.
        private Dictionary<string, EnemyController> enemies;

        public LocalGameManager(GameManager owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Gets or Sets the players' turn to move.  Every player gets to move on each turn.
        /// If a player does not move within 2 seconds, their turn is done by
        /// standing still.
        /// </summary>
        /// <value><c>true</c> if the players are still moving.</value>
        public bool PlayersTurn
        {
            get
            {
                bool allMoved = true;
                foreach (PlayerInfo p in PlayerInfo.AllPlayers)
                {
                    allMoved = allMoved && p.Moved;
                }

                // if there are still players that need to move
                // and time has not run out!
                return !allMoved && playerTurnTimer < playerTurnInterval;
            }

            set
            {
                // figure out the current state of the players turn
                // so we can notify others if the state changes.
                bool playersTurn = playerTurnTimer < playerTurnInterval;
                bool allMoved = true;

                // if it is now the players' turn, reset the timer.
                if (value)
                {
                    playerTurnTimer = 0f;
                }
                else
                {
                    // otherwise max it out
                    playerTurnTimer = playerTurnInterval;
                }

                // check the players for moving, and also
                // reset the moved flag if the new value is true.
                foreach (PlayerInfo p in PlayerInfo.AllPlayers)
                {
                    allMoved = allMoved && p.Moved;
                    if (value)
                    {
                        p.Moved = false;
                    }
                }

                // if it is true and that is different than from before,
                // send out the notification.
                if (value && (allMoved || !playersTurn))
                {
                    owner.SendPlayerTurnChanged();
                }
            }
        }

        /// <summary>
        /// Initialize this instance.
        /// </summary>
        public void Initialize()
        {
            // Assign enemies to a new List of Enemy objects.
            enemies = new Dictionary<string, EnemyController>();
        }

        /// <summary>
        /// Initializes the level.
        /// </summary>
        public void InitializeLevel()
        {
            // always clear out the working lists
            enemies.Clear();

            // Add one to our level number.
            owner.Level = owner.Level + 1;

            // Call InitGame to initialize our level.
            InitLocalGame();

            if (owner.Room != null)
            {
                // keep the room open if needed.
                if (owner.Room.AlwaysOpen)
                {
                    owner.Room.WaitForPlayers(OnPlayerConnecting);
                }
                else
                {
                    Debug.Log("Room is closed - don't accept other players");
                }

                // Send to all the players the new state
                owner.SendToAll(
                    owner.GetGameStateData(
                        GameManager.GameStateData.Phase.Initializing));
            }
            else
            {
                // single player mode
                Debug.Log("Initializing Single player game");
            }
        }

        /// <summary>
        /// Adds the enemy to list. so we can keep track of enemies that
        /// need to move.
        /// </summary>
        /// <param name="enemy">Enemy.</param>
        public void AddEnemyToList(EnemyController enemy)
        {
            enemy.BroadcastMovement = true;
            this.enemies.Add(enemy.gameObject.name, enemy);
        }

        /// <summary>
        /// New player is connecting to the game
        /// </summary>
        /// <param name="player">Player.</param>
        /// <param name="data">Data.</param>
        internal void OnPlayerConnecting(string endpointId, byte[] data)
        {
            PlayerInfo p = PlayerInfo.AddPendingPlayer(endpointId, null, data);

            GameObject obj = owner.LevelManager.CreatePlayer(
                                 p.AvatarIndex,
                                 p.DeviceId);

            PlayerController ctl = obj.GetComponent<PlayerController>();
            if (ctl != null)
            {
                ctl.Player = p;
                ctl.BroadcastMovement = true;

                // force the  movement message so the remote player moves to the
                // assigned, random position.
                ItemState state = new ItemState();
                state.Enabled = obj.activeSelf;
                state.Name = obj.name;
                state.Position = obj.transform.position;
                state.PrefabIndex = p.AvatarIndex;
                state.Rotation = obj.transform.rotation;
                state.TileSetName = ItemState.PlayerTileSet;
                owner.LevelData.Add(state);
            }

            owner.CreatePlayerScorePanel(p);
        }

        /// <summary>
        /// Called from the Update() event
        /// </summary>
        public void OnUpdate()
        {
            // keep track of player turn time
            playerTurnTimer += Time.deltaTime;

            // Check that playersTurn or enemiesMoving or doingSetup are not currently true.
            if (PlayersTurn || enemiesMoving)
            {
                // If any of these are true, return and do not start MoveEnemies.
                return;
            }

            // Only move the enemies if it is a local game.
            if (owner.Room == null || owner.Room.IsLocal)
            {
                // check for ending of level
                if (enemies.Count == 0)
                {
                    GameObject exit = owner.ShowExit();
                    owner.OnObjectChanged(exit.GetComponent<Shareable>());
                }

                // Start moving enemies.
                owner.StartCoroutine(MoveEnemies());
            }
        }

        // Coroutine to move enemies in sequence.
        internal IEnumerator MoveEnemies()
        {
            // While enemiesMoving is true player is unable to move.
            enemiesMoving = true;

            // Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(owner.turnDelay);

            // If there are no enemies spawned (IE in first level):
            if (enemies.Count == 0)
            {
                // Wait for turnDelay seconds between moves, replaces
                // delay caused by enemies moving when there are none.
                yield return new WaitForSeconds(owner.turnDelay);
            }

            List<string> dead = new List<string>();

            // Loop through List of Enemy objects.
            foreach (string key in enemies.Keys)
            {
                if (enemies[key] != null)
                {
                    if (enemies[key].enabled)
                    {
                        // Move this enemy
                        enemies[key].MoveEnemy();

                        // Wait for Enemy's moveTime before moving next Enemy,
                        yield return new WaitForSeconds(enemies[key].moveTime);
                    }
                    else
                    {
                        // the enemy is dead - make sure the game object is
                        // is still there (a destroyed game object will have
                        // disabled scripts) and pass along the change to
                        // the other players.
                        if (enemies[key].gameObject != null)
                        {
                            enemies[key].gameObject.SetActive(false);
                            dead.Add(key);
                            owner.OnObjectChanged(
                                enemies[key].gameObject.GetComponent<Shareable>());
                        }
                    }
                }
                else
                {
                    Debug.Log("Enemy is null for key " + key + " ?!?!");
                }
            }

            // clean up enemies that died.
            foreach (string key in dead)
            {
                enemies.Remove(key);
            }

            // Once Enemies are done moving, set playersTurn to true so player can move.
            PlayersTurn = true;

            // Enemies are done moving, set enemiesMoving to false.
            enemiesMoving = false;
        }

        // Initializes the game for each level.
        internal void InitLocalGame()
        {
            Debug.Log("Initializing LocalGame!");

            // Clear any Enemy objects in our List to prepare for next level.
            // Call the SetupScene, pass it current level number.
            owner.LevelData.Clear();
            owner.LevelData.AddRange(owner.LevelManager.SetupScene(owner.Level));

            // ther should be a pending player for the local player
            foreach (PlayerInfo p in PlayerInfo.AllPlayers)
            {
                GameObject obj =
                    owner.LevelManager.CreatePlayer(p.AvatarIndex, p.DeviceId);
                PlayerController ctl = obj.GetComponent<PlayerController>();
                if (ctl != null)
                {
                    ctl.Player = p;
                    ctl.BroadcastMovement = true;
                }
                else
                {
                    Debug.LogWarning("PlayerController is null?? for " + p.Player.Name);
                }

                owner.GameState.LevelData.Add(obj.GetComponent<Shareable>().State);
            }
        }

        public void OnNewGameState(GameManager.GameStateData newState, List<ItemState> changes)
        {
            if (newState != null)
            {
                // we don't do anything with game state we receive since we
                // are the local game manager which is the source of truth for
                // the game state.
                Debug.Log("Got new game state: " + newState);
            }

            if (changes != null && changes.Count > 0)
            {
                Debug.Log("Got New changes from Remote! with " + changes.Count);

                // Only change the players and scores
                foreach (ItemState item in changes)
                {
                    Debug.Log("Processing  ----> " + item.Name);
                    GameObject obj = owner.LevelManager.Find(item.Name);
                    if (obj != null)
                    {
                        PlayerController ctl = obj.GetComponent<PlayerController>();

                        // only move players - enemies are managed locally.
                        if (ctl != null && !ctl.Player.IsLocal)
                        {
                            ctl.MoveTo(item.Position);
                        }
                    }
                    else if (item.Name.StartsWith(GameManager.ScoreChangedItemName))
                    {
                        PlayerInfo p = PlayerInfo.GetPlayer(item.TileSetName);
                        if (p != null)
                        {
                            p.Score = (p.Score > item.PrefabIndex) ? p.Score : item.PrefabIndex;
                        }
                    }
                }
            }
        }
    }
}
