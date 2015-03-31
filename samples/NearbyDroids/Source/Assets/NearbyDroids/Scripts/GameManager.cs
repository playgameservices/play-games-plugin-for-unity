// <copyright file="GameManager.cs" company="Google Inc.">
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
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Game manager manages the game play and players.
    /// Heavily inspired by http://unity3d.com/learn/tutorials/projects/2d-roguelike
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Static instance of GameManager which allows it to be accessed by any other script.
        private static GameManager instance = null;

        // Store a reference to our BoardManager which will set up the level.
        private LevelManager levelmanager;

        private int level = 0;

        // Time to wait before starting level, in seconds.
        public float levelStartDelay = 2f;

        public float turnDelay = 0.1f;

        private bool playersTurn = true;

        // Prefab for the score labels and text
        public GameObject scorePrefab;

        // Reference to the panel that should be the parent of the scores
        private GameObject scorePanel;

        // array of player avatars prefabs.
        public GameObject[] playerAvatars;

        // List to keep track of enemies.
        private List<EnemyController> enemies;
        private bool enemiesMoving;

        public static GameManager Instance
        {
            get
            {
                return instance;
            }
        }

        public bool PlayersTurn
        {
            get
            {
                return this.playersTurn;
            }

            set
            {
                playersTurn = value;
            }
        }

        private Text LevelText
        {
            get
            {
                return GameObject.Find("level_message").GetComponent<Text>();
            }
        }

        /// <summary>
        /// GameOver is called when the player dies.
        /// </summary>
        public void GameOver()
        {
            Text t = LevelText;

            t.text = "Game Over";

            // Enable black background image gameObject.
            t.enabled = true;

            // Disable this GameManager.
            enabled = false;
        }

        // Awake is always called before any Start functions
        internal void Awake()
        {
            // Check if instance already exists
            if (instance == null)
            {
                // if not, set instance to this
                instance = this;
            }
            else if (instance != this)
            { 
                // If instance already exists and it's not this:
                // Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);   
            }

            // Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            // Assign enemies to a new List of Enemy objects.
            enemies = new List<EnemyController>();

            // Get a component reference to the attached BoardManager script
            levelmanager = GetComponent<LevelManager>();
        }

        public void StartPlaying()
        {
            Debug.Log("Let the games begin!");

            level = 0;

            // 1 is the game, 0 is the main menu
            Application.LoadLevel(1);
        }

        public void StopPlaying()
        {
            levelmanager.DestroyBoard();
            Application.LoadLevel(0);
        }

        /// <summary>
        /// Refreshs the players.  This is used to 
        /// create any instances of the players at the start of the level
        /// and adds their score to the score list.
        /// </summary>
        internal void RefreshPlayers()
        {
            foreach (PlayerInfo p in PlayerInfo.AllPlayers)
            {
                PlayerInfo player = PlayerInfo.CreatePlayer(scorePanel, p.Name, p.DeviceId, p.EndpointId, scorePrefab);
                if (player.Score == 0)
                {
                    player.Score = 10;
                }

                Vector3 pos = levelmanager.RandomPosition();
                Instantiate(playerAvatars[player.AvatarIndex % playerAvatars.Length], pos, Quaternion.identity);
            }
        }
            
        // This is called each time a scene is loaded.
        internal void OnLevelWasLoaded(int index)
        {
            if (index == 1)
            {
                // Add one to our level number.
                level++;

                // Call InitGame to initialize our level.
                InitGame();
                    
                scorePanel = GameObject.Find("Scores");
                RefreshPlayers();
            }
        }

        // Initializes the game for each level.
        internal void InitGame()
        {
            // While doingSetup is true the player can't move, prevent player from moving while title card is up.
            // doingSetup = true;

            // Clear any Enemy objects in our List to prepare for next level.
            enemies.Clear();

            // Call the SetupScene function of the BoardManager script, pass it current level number.
            levelmanager.SetupScene(level);

            LevelText.text = "Level " + level;
        }

        internal void Update()
        {
            PlayerInfo.UpdateScores();

            // Check that playersTurn or enemiesMoving or doingSetup are not currently true.
            if (playersTurn || enemiesMoving)
            {
                // If any of these are true, return and do not start MoveEnemies.
                return;
            }

            // check for ending of level
            if (enemies.Count == 0)
            {
                levelmanager.ShowExit();
            }

            // Start moving enemies.
            StartCoroutine(MoveEnemies());
        }

        // Coroutine to move enemies in sequence.
        internal IEnumerator MoveEnemies()
        {
            // While enemiesMoving is true player is unable to move.
            enemiesMoving = true;

            // Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(turnDelay);

            // If there are no enemies spawned (IE in first level):
            if (enemies.Count == 0)
            {
                // Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                yield return new WaitForSeconds(turnDelay);
            }

            List<EnemyController> dead = new List<EnemyController>();

            // Loop through List of Enemy objects.
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].enabled)
                {
                    // Call the MoveEnemy function of Enemy at index i in the enemies List.
                    enemies[i].MoveEnemy();

                    // Wait for Enemy's moveTime before moving next Enemy, 
                    yield return new WaitForSeconds(enemies[i].moveTime);
                }
                else
                {
                    enemies[i].gameObject.SetActive(false);
                    dead.Add(enemies[i]);
                }
            }

            foreach (EnemyController e in dead)
            {
                enemies.Remove(e);
            }
          
            // Once Enemies are done moving, set playersTurn to true so player can move.
            playersTurn = true;

            // Enemies are done moving, set enemiesMoving to false.
            enemiesMoving = false;
        }

        public void AddEnemyToList(EnemyController enemy)
        {
            enemies.Add(enemy);
        }
    }
}