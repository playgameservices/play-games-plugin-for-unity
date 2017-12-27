// <copyright file="Multiplayer.cs" company="Google Inc.">
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
    using GooglePlayGames;
    using GooglePlayGames.BasicApi.Nearby;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Handles the Multiplayer menus
    /// </summary>
    public class Multiplayer : MonoBehaviour
    {
        public const string RoomNameKey = "roomname";
        public const string PlayerNameKey = "playername";
        public const string AvatarIndexKey = "char";

        // References to input controls on the UI
        public InputField roomNameField;
        public InputField playerNameField;
        public ToggleGroup charGroup;
        public Toggle autoConnect;
        public Toggle allowJoinDuringGame;
        public Text statusText;

        // reference to the lobby panel used when a searching for
        // rooms or players.
        public GameObject lobbyPanel;

        // reference to where list the rooms or players found
        public GameObject lobbyListArea;

        // prefab used as a list item.
        public GameObject itemChoicePrefab;

        // true if we are joining a room vs. hosting it.
        private bool joining;

        // status message to display
        private string statusMsg;

        // discovered rooms
        private Dictionary<string, GameObject> discoveredRooms;

        // local player info
        private string localPlayerName;
        private int localAvatarIndex;

        public bool Joining
        {
            get
            {
                return joining;
            }

            set
            {
                joining = value;
            }
        }

        internal void Awake()
        {
            // Heavy handed, but reset the nearby connections state.
            PlayGamesPlatform.InitializeNearby((client) =>
                {
                    Debug.Log("Nearby connections initialized");
                    client.StopAllConnections();
                });
    
            // initialize saved preferences.
            string defaultRoomName = PlayerPrefs.GetString(RoomNameKey);
            if (defaultRoomName != null && roomNameField != null)
            {
                roomNameField.text = defaultRoomName;
            }

            string defaultPlayerName = PlayerPrefs.GetString(PlayerNameKey);
            if (defaultRoomName != null && playerNameField != null)
            {
                playerNameField.text = defaultPlayerName;
            }

            int charIndex = PlayerPrefs.GetInt(AvatarIndexKey);
            Toggle[] chars = charGroup.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i].isOn = i == charIndex;
            }

            discoveredRooms = new Dictionary<string, GameObject>();
        }

        /// <summary>
        /// Starts the multiplayer game either as a host or joining,
        /// based on the joining flag.
        /// </summary>
        public void StartMultiplayer()
        {
            string room = roomNameField.text;
            localPlayerName = playerNameField.text;
            Toggle[] chars = charGroup.GetComponentsInChildren<Toggle>();
            localAvatarIndex = 0;

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].isOn)
                {
                    localAvatarIndex = i;
                    break;
                }
            }

            // save preferences
            PlayerPrefs.SetString(RoomNameKey, room);
            PlayerPrefs.SetString(PlayerNameKey, localPlayerName);
            PlayerPrefs.SetInt(AvatarIndexKey, localAvatarIndex);
            PlayerPrefs.Save();

            if (joining)
            {
                StartDiscovery();
            }
            else
            {
                CreateRoom(room, allowJoinDuringGame.isOn, autoConnect.isOn);
            }
        }

        /// <summary>
        /// called by unity when this object is disabled.
        /// </summary>
        internal void OnDisable()
        {
            HideLobbyPanel();
        }

        internal void Update()
        {
            if (statusText != null)
            {
                statusText.text = statusMsg;
            }
        }

        /// <summary>
        /// Starts the discovery of rooms.
        /// </summary>
        internal void StartDiscovery()
        {
            statusMsg = "Looking for Games...";
            PlayerInfo.AddPendingPlayer(
                new NearbyPlayer(localPlayerName), localAvatarIndex);
            NearbyRoom.FindRooms(OnRoomFound);
            ShowLobbyPanel();
        }

        internal void ShowLobbyPanel()
        {
            lobbyPanel.SetActive(true);
        }

        internal void HideLobbyPanel()
        {
            lobbyPanel.SetActive(false);
        }

        public void CancelMultiplayer()
        {
            GameManager.Instance.Room = null;
            NearbyRoom.StopAll();
            GameManager.Instance.StopPlaying();
        }

        /// <summary>
        /// Creates the room.
        /// </summary>
        /// <param name="roomName">Room name to use</param>
        /// <param name="joinDuringGame">If set to <c>true</c> players can join 
        /// after the game has started.</param>
        /// <param name="autoconnect">If set to <c>true</c> players are automatically
        /// connected to the room, otherwise they need to be explicitly accepted.</param>
        internal void CreateRoom(
            string roomName,
            bool joinDuringGame,
            bool autoconnect)
        {
            NearbyRoom room = NearbyRoom.CreateRoom(roomName);
            room.AutoJoin = autoconnect;
            room.AlwaysOpen = joinDuringGame;

            // Store off the room in with the players. The game manager
            // will pick up the room and start processing messages.
            GameManager.Instance.Room = room;

            // if you can join in progress and it is autoconnect, setup the 
            // room and start it, no need to wait.
            if (joinDuringGame && autoconnect)
            {
                Debug.Log("Starting Game!");

                PlayerInfo.AddPendingPlayer(
                    new NearbyPlayer(localPlayerName),
                    localAvatarIndex);
                
                GameManager.Instance.StartPlaying(GameManager.GameType.MultiplayerLocal);
            }
            else
            {
                statusMsg = "Waiting for players...";
                GameManager.Instance.Room.WaitForPlayers(OnPlayerFound);
                Debug.Log(statusMsg);
                ShowLobbyPanel();
            }
        }

        /// <summary>
        /// Enters the room from the lobby.
        /// </summary>
        public void EnterRoom()
        {
            if (joining)
            {
                // get the room that we should join.
                Toggle[] rooms = lobbyListArea.GetComponentsInChildren<Toggle>();
                foreach (Toggle t in rooms)
                {
                    if (t.isOn)
                    {
                        GameManager.Instance.Room = NearbyRoom.LookupRoomByEndpoint(t.name);
                        break;
                    }
                }

                if (GameManager.Instance.Room != null)
                {
                    GameManager.Instance.StartPlaying(GameManager.GameType.MultiplayerRemote);
                }
                else
                {
                    statusMsg = "You must select a room to start!";
                }

                return;
            }
            else
            {
                if (!GameManager.Instance.Room.AutoJoin)
                {
                    // loop over players and remove unchecked players
                    Toggle[] players = lobbyListArea.GetComponentsInChildren<Toggle>();
                    Debug.Log("Checking " + players.Length + " players");
                    foreach (Toggle t in players)
                    {
                        if (!t.isOn)
                        {
                            Debug.Log("Removing " + t.gameObject.name);
                            PlayerInfo.RemovePendingPlayer(t.gameObject.name);
                        }
                        else
                        {
                            Debug.Log("Accepting " + t.gameObject.name);
                            PlayerInfo p = PlayerInfo.GetPlayer(t.gameObject.name);
                            if (p != null)
                            {
                                GameManager.Instance.Room.AcceptRequest(p.Player.EndpointId);
                            }
                            else
                            {
                                Debug.LogError("Cannot find NearbyPlayer for " +
                                    t.gameObject.name);
                            }
                        }
                    }
                }

                if (!GameManager.Instance.Room.AlwaysOpen)
                {
                    GameManager.Instance.Room.CloseRoom();
                }
            
                Debug.Log("Starting Game!");

                GameManager.Instance.StartPlaying(GameManager.GameType.MultiplayerLocal);
            }
        }

        /// <summary>
        /// Called when a player is requesting to join a room.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <param name="data">Data.</param>
        internal void OnPlayerFound(string endpointId, byte[] data)
        {
            PlayerInfo player = PlayerInfo.AddPendingPlayer (endpointId, null, data);
            GameObject obj = Instantiate(itemChoicePrefab) as GameObject;
            obj.transform.SetParent(lobbyListArea.transform, false);
            obj.GetComponentInChildren<Text> ().text = player.Player.Name;
            Toggle t = obj.GetComponentInChildren<Toggle>();
            t.gameObject.name = player.DeviceId;
            t.isOn = true;
        }

        /// <summary>
        /// called when a room is discovered.
        /// </summary>
        /// <param name="room">Room.</param>
        /// <param name="available">If set to <c>true</c> available.</param>
        internal void OnRoomFound(NearbyRoom room, bool available)
        {
            if (available)
            {
                GameObject obj = Instantiate(itemChoicePrefab) as GameObject;
                obj.transform.SetParent(lobbyListArea.transform, false);
                obj.GetComponentInChildren<Text>().text = room.Name;
                Toggle t = obj.GetComponentInChildren<Toggle>();
                t.gameObject.name = room.EndpointId;
                t.group = lobbyPanel.GetComponent<ToggleGroup>();
                Debug.Log("Room found: " + room.Name);
                discoveredRooms.Add(room.EndpointId, obj);
            }
            else
            {
                if (discoveredRooms.ContainsKey(room.EndpointId))
                {
                    GameObject obj = discoveredRooms[room.EndpointId];
                    DestroyObject(obj);
                    discoveredRooms.Remove(room.EndpointId);
                }
            }
        }
    }
}
