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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using GooglePlayGames;
    using GooglePlayGames.BasicApi.Nearby;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;
    /// <summary>
    /// Game manager manages the game play and players.
    /// Heavily inspired by http://unity3d.com/learn/tutorials/projects/2d-roguelike
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // constants used to share game state with remote players.
        internal const string PlayerTurnFlagName = "_flag.PlayerTurn";
        internal const string ScoreChangedItemName = "_score_";

        // formatter used to serialize the data for sending to remote players.
        private static BinaryFormatter bf = new BinaryFormatter();

        // frequency in seconds to share data with remote players.
        // This can be changed to adjust the performance of the game.
        // .2 seconds is 5 times a second, since the enemies move in .1 seconds
        // per turn, there can be some blockiness in the updates, but for a
        // couple humans playing is seems to be fast enough.
        // You could also investigate being event driven and only sending updates
        // when there is one, but the trick is to make sure all related objects
        // (such as an enemy moving and a player moving) are done in the same turn.
        private static float sharingInterval = 0.2f;

        // Static instance of GameManager which allows it to be accessed by any other script.
        private static GameManager instance = null;

        // Multiplayer room
        private NearbyRoom room;

        // state of connection if we are remote and joining another player.
        private bool connected;

        // Store a reference to the level manager.
        private LevelManager levelmanager;

        // current gamestate.  if there is no room or it is local, this is
        // updated by this class.  If there is a remote room, the game state is
        // sent from the host of the room.
        private GameStateData gameState = new GameStateData();

        // changes that have happened locally and need to be shared.
        // A dictionary is used to keep only the latest state for each object.
        // the order of changes does not matter, the dictionary is sent as a
        // set of changes and processed at the same time.
        private Dictionary<string, ItemState> changesToShare =
            new Dictionary<string, ItemState>();

        // implementation of the game manager that is specific for remote or
        // local play.
        private IGameManager managerImpl;

        // the type of the game manager.
        private GameType gameType;

        // Time to wait before starting level, in seconds.
        public float levelStartDelay = 2f;

        // delay in seconds for the enemy turn.
        public float turnDelay = 0.1f;

        // the timer for keeping track of the time since sending changes
        // to other players.
        private float sharingTimer = 0f;

        // Prefab for the score labels and text
        public GameObject scorePrefab;

        // Reference to the panel that should be the parent of the scores
        private GameObject scorePanel;

        public enum GameType
        {
            /// <summary>single player game</summary>
            SinglePlayer,

            /// <summary>The multiplayer local - hosting the game.</summary>
            MultiplayerLocal,

            /// <summary>The multiplayer remote.</summary>
            MultiplayerRemote
        }

        /// <summary>
        /// Gets the instance of the game manager.
        /// </summary>
        /// <value>The instance.</value>
        public static GameManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get
            {
                return gameState.Level;
            }

            set
            {
                gameState.Level = value;
            }
        }

        /// <summary>
        /// Gets or sets the state of the game.
        /// </summary>
        /// <value>The state of the game.</value>
        public GameStateData GameState
        {
            get
            {
                return gameState;
            }

            set
            {
                gameState = value;
            }
        }

        /// <summary>
        /// Gets the level data. the level data is the state of the prefabs
        /// that make up the level.
        /// </summary>
        /// <value>The level data.</value>
        public List<ItemState> LevelData
        {
            get
            {
                return gameState.LevelData;
            }
        }

        /// <summary>
        /// Gets the level manager.
        /// </summary>
        /// <value>The level manager.</value>
        public LevelManager LevelManager
        {
            get
            {
                return levelmanager;
            }
        }

        /// <summary>
        /// Gets the level text object this is found in the scene.
        /// </summary>
        /// <value>The level text.</value>
        private Text LevelText
        {
            get
            {
                return GameObject.Find("level_message").GetComponent<Text>();
            }
        }

        /// <summary>
        /// Gets the game over text. this is found in the scene.
        /// </summary>
        /// <value>The game over text.</value>
        private Text GameOverText
        {
            get
            {
                return GameObject.Find("game_over_text").GetComponent<Text>();
            }
        }

        /// <summary>
        /// Gets or sets the nearby multiplayer room.  This can be null for
        /// single player games.
        /// </summary>
        /// <value>The room.</value>
        public NearbyRoom Room
        {
            get
            {
                return room;
            }

            set
            {
                room = value;
            }
        }

        /// <summary>
        /// Runs as local manager.
        /// </summary>
        public void RunAsLocalManager()
        {
            managerImpl = new LocalGameManager(this);
            managerImpl.Initialize();
        }

        /// <summary>
        /// Runs as remote manager.
        /// </summary>
        public void RunAsRemoteManager()
        {
            Debug.Log("Intializing managerImpl as Remote");
            managerImpl = new RemoteGameManager(this);
            managerImpl.Initialize();
        }

        /// <summary>
        /// Creates the player score panel. This creates a score entry
        /// on the UI for the given player.
        /// </summary>
        /// <param name="playerInfo">Player info.</param>
        internal void CreatePlayerScorePanel(PlayerInfo playerInfo)
        {
            if (scorePanel == null)
            {
                scorePanel = GameObject.Find("Scores");
            }

            if (scorePanel == null)
            {
                Debug.LogError("Score Panel cannot be null!");
                return;
            }

            // use a deterministic name for the panel.
            string panelName = "score" + playerInfo.DeviceId;

            // look for it in the scene.
            GameObject panel = GameObject.Find(panelName);

            // if not there, create it and make it a child object of the 
            // score panel.
            if (panel == null)
            {
                panel = Instantiate(scorePrefab) as GameObject;
                panel.transform.SetParent(scorePanel.transform, false);
                panel.name = panelName;

                // associate it with the player info.
                // if there is an old panel there already, destroy it
                if (playerInfo.ScorePanel != null)
                {
                    DestroyObject(playerInfo.ScorePanel);
                }

                playerInfo.ScorePanel = panel;
                playerInfo.ScoreText = null;
                playerInfo.NameText = null;

                // mark the UI for re-laying out the list of scores.
                LayoutRebuilder.MarkLayoutForRebuild(scorePanel.GetComponent<RectTransform>());
            }
            else
            {
                if (playerInfo.ScorePanel != panel)
                {
                    DestroyObject(playerInfo.ScorePanel);
                }

                playerInfo.ScorePanel = panel;
                playerInfo.ScoreText = null;
                playerInfo.NameText = null;
            }

            // Set the text UI objects for easy access.
            if (playerInfo.ScoreText == null || playerInfo.NameText == null)
            {
                Text[] texts = panel.GetComponentsInChildren<Text>();
                if (texts == null)
                {
                    Debug.Log("Found no text components?!?!?!");
                }

                foreach (Text t in texts)
                {
                    if (t.gameObject.name.EndsWith("name"))
                    {
                        playerInfo.NameText = t;
                    }
                    else if (t.gameObject.name.EndsWith("score"))
                    {
                        playerInfo.ScoreText = t;
                    }
                }
            }

            if (playerInfo.NameText == null)
            {
                Debug.Log("Did not find name text field?");
            }

            if (playerInfo.ScoreText == null)
            {
                Debug.Log("Did not find score text field?");
            }

            playerInfo.NameText.text = playerInfo.Player.Name;
            playerInfo.ScoreText.text = Convert.ToString(playerInfo.Score);
        }

        /// <summary>
        /// Sends the player turn changed. This is used let remote players
        /// know it is time to move.
        /// </summary>
        internal void SendPlayerTurnChanged()
        {
            OnObjectChanged(PlayerTurnFlagName, true);
        }

        /// <summary>
        /// GameOver is called when the player dies.
        /// </summary>
        /// <param name="txt">The text in the game over message</param>
        public void GameOver(string txt)
        {
            Text t = GameOverText;

            t.text = "Game Over\n" + txt;

            // Enable black background image gameObject.
            t.enabled = true;
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

            // Get a component reference to the attached BoardManager script
            levelmanager = GetComponent<LevelManager>();
        }

        /// <summary>
        /// Starts the playing.
        /// This entails creating a new game state, initializing the
        /// type of game manager needed and loading the game scene (level 1)
        /// </summary>
        /// <param name="type">Type of game</param>
        public void StartPlaying(GameType type)
        {
            Debug.Log("Let the " + type + " game begin!");

            gameState = new GameStateData();

            this.gameType = type;

            // Run as a local manager or remote
            if (gameType == GameType.MultiplayerLocal ||
                gameType == GameType.SinglePlayer)
            {
                RunAsLocalManager();
            }
            else
            {
                RunAsRemoteManager();
            }

            // 1 is the game, 0 is the main menu
            SceneManager.LoadScene(1);
        }

        /// <summary>
        /// Stops the playing.  This cleans up the internal data structures
        /// and stops the nearby communications.
        /// </summary>
        public void StopPlaying()
        {
            levelmanager.DestroyBoard();
            if (Room != null)
            {
                NearbyRoom.StopAll();
                Room = null;
                PlayerInfo.ClearAllPlayers();
                gameState.LevelData.Clear();
            }

            // 0 is the main menu scene.
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Starts the next level.  Remote managers
        /// don't do anything, they wait for the signal
        /// from the multiplayer local manager.
        /// </summary>
        public void StartNextLevel()
        {
            if (gameType == GameType.MultiplayerLocal ||
                gameType == GameType.SinglePlayer)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        /// <summary>
        /// Refreshs the players.  This is used to 
        /// create any instances of the players at the start of the level
        /// and adds their score to the score list.
        /// </summary>
        internal void RefreshPlayers()
        {
            foreach (PlayerInfo player in PlayerInfo.AllPlayers)
            {
                Debug.Log("Got the info player: " + player.Player.Name);

                // 10 is the min score.
                if (player.Score == 0)
                {
                    player.Score = 10;
                    OnScoreChanged(player.DeviceId);
                }

                // Find the player object.
                GameObject playerObj = LevelManager.GetPlayerObject(player.DeviceId);

                if (playerObj != null)
                {
                    PlayerController ctl = playerObj.GetComponent<PlayerController>();
                    if (ctl != null)
                    {
                        // initialize the player info in the controller
                        ctl.Player = ctl.Player ?? player;

                        // set the flag controlling sending player updates
                        // to the other game participants.  This is
                        // done if the player is local, or if we are hosting
                        // the game room.
                        ctl.BroadcastMovement = player.IsLocal ||
                        gameType == GameType.MultiplayerLocal;
                    }
                    else
                    {
                        Debug.Log("Can't find playercontroller??!?!?");
                    }
                }
                else
                {
                    Debug.Log("Cannot find gameobject for " + player.DeviceId);
                }
            }
        }
            
        // This is called each time a scene is loaded.
        internal void OnLevelWasLoaded(int sceneIndex)
        {
            // scene index 1 is the game, 0 is the menu.
            // this object is not destroyed on scene load, 
            // so we need to make sure we don't try to initialize the
            // gameplay on the menu.
            if (sceneIndex == 1)
            {
                if (Room != null)
                {
                    Debug.Log("Setting Room level callbacks");
                    Room.MessageHandler = OnMessageReceived;
                    Room.PlayerHandler = OnPlayerChanged;
                    if (gameType == GameType.MultiplayerLocal)
                    {
                        Room.ConnectionData = 
                        () =>
                        {
                            return GetGameStateData(GameStateData.Phase.Connecting);
                        };
                    }
                }

                // if remote and not connected, then connect to the room.
                if (gameType == GameType.MultiplayerRemote && !connected)
                {
                    PlayerInfo localPlayer = PlayerInfo.LocalPlayer;
                    Room.JoinRoom(
                        localPlayer.Player,
                        localPlayer.SerializedData,
                        OnRoomJoined);
                }

                managerImpl.InitializeLevel();
                scorePanel = GameObject.Find("Scores");
                RefreshPlayers();

                if (LevelText != null)
                {
                    LevelText.text = "Level " + Level;
                }
            }
        }

        /// <summary>
        /// Called when the nearby connection response from
        /// sending Remote connection request to the room advertiser.
        /// </summary>
        /// <param name="response">Response.</param>
        internal void OnRoomJoined(ConnectionResponse response)
        {
            Debug.Log("OnRoomJoined Called status: " + response.ResponseStatus);
            if (response.ResponseStatus == ConnectionResponse.Status.Accepted)
            {
                // if we are connected, stop looking for rooms.
                NearbyRoom.StopRoomDiscovery();

                // the first payload is sent with the response so we can initialize
                // the game scene.
                OnMessageReceived(Room.Address, response.Payload);
                connected = true;
            }
            else if (response.ResponseStatus == ConnectionResponse.Status.ErrorAlreadyConnected)
            {
                // cleanup the old connection and join again.
                Room.Disconnect();
                PlayerInfo localPlayer = PlayerInfo.LocalPlayer;
                Room.JoinRoom(
                    localPlayer.Player,
                    localPlayer.SerializedData,
                    OnRoomJoined);
            }
            else
            {
                GameOver("Error joining room: " + response.ResponseStatus);
            }
        }

        /// <summary>
        /// Helper to send the change of a boolean property to the remote players.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="flag">If set to <c>true</c> flag.</param>
        public void OnObjectChanged(string property, bool flag)
        {
            ItemState state = new ItemState();
            state.Name = property;
            state.Enabled = flag;
            state.TileSetName = null;
            changesToShare[state.Name] = state;
        }

        /// <summary>
        /// Raises the score changed event.
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        public void OnScoreChanged(string deviceId)
        {
            int score = PlayerInfo.GetScore(deviceId);
            ItemState state = new ItemState();
            state.Name = GameManager.ScoreChangedItemName + deviceId;
            state.PrefabIndex = score;
            state.TileSetName = deviceId;
            changesToShare[state.Name] = state;
        }

        /// <summary>
        /// Raises the object changed event.
        /// </summary>
        /// <param name="o">O.</param>
        public void OnObjectChanged(Shareable o)
        {
            // send the object change to other players
            if (o != null)
            {
                o.UpdateState();
                changesToShare[o.State.Name] = o.State;
            }
            else
            {
                Debug.Log("Trying to record null object change?");
            }
        }

        /// <summary>
        /// Notifies the changes.
        /// </summary>
        public void NotifyChanges()
        {
            if (Room != null)
            {
                if (changesToShare.Count != 0)
                {
                    foreach (ItemState s in changesToShare.Values)
                    {
                        Debug.Log("-->" + s.Name + " at " + s.Position);
                    }

                    Debug.Log("Sending " + changesToShare.Count + " changes!");
                    SendToAll(GetGameStateData(GameStateData.Phase.Updating));
                    changesToShare.Clear();
                }
            }
            else
            {
                Debug.Log("Not a multiplayer game, notification not sent.");
            }
        }

        internal void Update()
        {
            // if we are on the main menu, don't update.
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                return;
            }

            if (managerImpl != null)
            {
                managerImpl.OnUpdate();
            }
            else
            {
                Debug.Log("Skipping managerImpl.OnUpdate, impl is null");
            }

            UpdateScores();

            // if not multiplayer, skip the notification.
            if (gameType != GameType.SinglePlayer)
            {
                if (sharingTimer >= sharingInterval)
                {
                    NotifyChanges();
                    sharingTimer = 0;
                }
                else
                {
                    sharingTimer += Time.deltaTime;
                }
            }
        }

        internal void UpdateScores()
        {
            bool allDead = true;
            foreach (PlayerInfo p in PlayerInfo.AllPlayers)
            {
                if (p.Score >= 0)
                {
                    allDead = false;
                }

                // creates the score panel objects if needed.
                CreatePlayerScorePanel(p);

                if (p.ScoreText != null)
                {
                    p.ScoreText.text = Convert.ToString(p.Score);
                }
                else
                {
                    Debug.Log("Score prefab not set for " + p.Player.Name);
                }
            }

            if (allDead)
            {
                GameOver("Everyone is Zapped!");
            }
        }

        /// <summary>
        /// Shows the exit portal game object.
        /// </summary>
        /// <returns>The exit.</returns>
        public GameObject ShowExit()
        {
            return levelmanager.ShowExit();
        }

        /// <summary>
        /// Adds the enemy to list.
        /// </summary>
        /// <param name="enemy">Enemy.</param>
        public void AddEnemyToList(EnemyController enemy)
        {
            managerImpl.AddEnemyToList(enemy);
        }

        /// <summary>
        /// Gets the state of the game serialized so it can be sent to others.
        /// </summary>
        /// <param name="phase">The phase of games state to generate.</param>
        /// <returns>The game state.</returns>
        internal byte[] GetGameStateData(GameStateData.Phase phase)
        {
            gameState.SerialNumber = gameState.SerialNumber + 1;
            gameState.CurrentPhase = phase;
            Debug.Log("Serializing Game state: " + phase + " " + gameState.SerialNumber);
            gameState.Players.Clear();
            foreach (PlayerInfo p in PlayerInfo.AllPlayers)
            {
                gameState.Players.Add(p.DataState);
                if (phase == GameStateData.Phase.Initializing ||
                    phase == GameStateData.Phase.Connecting)
                {
                    GameObject playerObj = levelmanager.GetPlayerObject(p.DeviceId);
                    if (playerObj != null)
                    {
                        OnObjectChanged(playerObj.GetComponent<Shareable>());
                    }
                }
            }

            GameDataMessage msg = new GameDataMessage();
            if (gameType == GameType.MultiplayerLocal)
            {
                msg.GameState = gameState;
            }

            msg.Changes = new List<ItemState>();
            msg.Changes.AddRange(changesToShare.Values);

            MemoryStream m = new MemoryStream();
            bf.Serialize(m, msg); 
            m.Flush();
            return m.ToArray();
        }

        /// <summary>
        /// Sends to all connected players (eventually).
        /// If this is the room "host" meaning every other player
        /// is connected to this one, then we send messages to all
        /// players.
        /// If this is a remote player (relative to the room), only
        /// the room is sent the message.
        /// </summary>
        /// <param name="message">Message.</param>
        public void SendToAll(byte[] message)
        {
            List<string> endpoints = new List<string>();

            if (Room.IsLocal)
            {
                foreach (PlayerInfo p in PlayerInfo.AllPlayers)
                {
                    if (p.Player != null &&
                        p.Player.DeviceId != NearbyPlayer.LocalDeviceId)
                    {
                        endpoints.Add(p.Player.EndpointId);
                    }
                }
            }
            else
            {
                endpoints.Add(Room.EndpointId);
            }

            if (endpoints.Count > 0)
            {
                PlayGamesPlatform.Nearby.SendReliable(endpoints, message);
            }
            else
            {
                Debug.Log("No Remote endpoints for getting updates!");
            }
        }

        /// <summary>
        /// Updates the game state from data by deserializing it into the 
        /// message object.
        /// </summary>
        /// <param name="data">Data.</param>
        internal void UpdateGameStateFromData(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            GameDataMessage msg = bf.Deserialize(ms) as GameDataMessage;
            Debug.Log("Received new GameState!( " + msg.Changes.Count + ")");
            managerImpl.OnNewGameState(msg.GameState, msg.Changes);
        }

        /// <summary>
        /// Raises the player changed event.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <param name="present">If set to <c>true</c> present.</param>
        internal void OnPlayerChanged(NearbyPlayer player, bool present)
        {
            Debug.Log("Player " + player.Name + " " + (string)(present ? "Arrived" : "Left"));
            if (player.DeviceId == Room.Address.DeviceId)
            {
                connected = false;
            }
        }

        /// <summary>
        /// Raises the message received event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="data">Data.</param>
        internal void OnMessageReceived(NearbyPlayer sender, byte[] data)
        {
            UpdateGameStateFromData(data);
        }

        /// <summary>
        /// Game data message.  This is serialized and sent
        /// to all the remote players.  It is a simple 
        /// class that contains references to the game state object
        /// and a list of changed items.
        /// </summary>
        [Serializable]
        public class GameDataMessage
        {
            private GameStateData gameState;

            private List<ItemState> changes;

            /// <summary>
            /// Gets or sets the state of the game. This is used
            /// for the player scores and the scene layout.
            /// </summary>
            /// <value>The state of the game.</value>
            public GameStateData GameState
            {
                get
                {
                    return gameState;
                }

                set
                {
                    gameState = value;
                }
            }

            /// <summary>
            /// Gets or sets the changes. These are movements,
            /// de-activations or other changes at runtime.
            /// </summary>
            /// <value>The changes.</value>
            public List<ItemState> Changes
            {
                get
                {
                    return changes;
                }

                set
                {
                    changes = value;
                }
            }
        }

        /// <summary>
        /// Game state data incorporating the scene and player data
        /// </summary>
        [Serializable]
        public class GameStateData
        {
            private long serialnumber;
            private Phase phase;
            private int level = 0;
            private List<ItemState> levelData;
            private List<PlayerInfo.PlayerData> players;

            internal GameStateData()
            {
                levelData = new List<ItemState>();
                players = new List<PlayerInfo.PlayerData>();
            }

            /// <summary>
            /// Phase of game play.  This can be used to figure out if
            /// the state is just an update (and somewhat incomplete)
            /// or for connecting/initialization.
            /// </summary>
            internal enum Phase
            {
                /// <summary>
                /// Connecting phase, data sent back to a connection request.
                /// </summary>
                Connecting,

                /// <summary>
                /// Initializing a new level
                /// </summary>
                Initializing,

                /// <summary>
                /// Update during a level
                /// </summary>
                Updating
            }

            /// <summary>
            /// Gets or sets the level that is currently being played.
            /// </summary>
            /// <value>The level.</value>
            internal int Level
            {
                get
                {
                    return level;
                }

                set
                {
                    level = value;
                }
            }

            /// <summary>
            /// Gets or sets the current phase of this game state instance.
            /// </summary>
            /// <value>The current phase.</value>
            internal Phase CurrentPhase
            {
                get
                {
                    return phase;
                }

                set
                {
                    phase = value;
                }
            }

            /// <summary>
            /// Gets or sets the serial number. Useful in debugging,
            /// it lets the developer correlate messages between the different
            /// players in the room.
            /// </summary>
            /// <value>The serial number.</value>
            internal long SerialNumber
            {
                get
                {
                    return serialnumber;
                }

                set
                {
                    serialnumber = value;
                }
            }

            /// <summary>
            /// Gets the level data.  This is used by the level manager
            /// to build the scene.
            /// </summary>
            /// <value>The level data.</value>
            internal List<ItemState> LevelData
            {
                get
                {
                    return levelData;
                }
            }

            /// <summary>
            /// Gets the players.
            /// </summary>
            /// <value>The players.</value>
            internal List<PlayerInfo.PlayerData> Players
            {
                get
                {
                    return players;
                }
            }
        }
    }
}
