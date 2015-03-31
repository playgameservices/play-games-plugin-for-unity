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
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Handles the Multiplayer menus
    /// </summary>
    public class Multiplayer : MonoBehaviour
    {
        public const string RoomNameKey = "roomname";
        public const string PlayerNameKey = "playername";
        public const string CharacterIndexKey = "char";

        public InputField roomName;
        public InputField playerName;
        public ToggleGroup charGroup;
        public Toggle autoConnect;
        public Toggle allowJoinDuringGame;

        private bool joining;
        private string statusMsg;
        public GameObject lobbyPanel;
        public Text statusText;

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
            string defaultRoomName = PlayerPrefs.GetString(RoomNameKey);
            if (defaultRoomName != null)
            {
                roomName.text = defaultRoomName;
            }

            string defaultPlayerName = PlayerPrefs.GetString(PlayerNameKey);
            if (defaultRoomName != null)
            {
                playerName.text = defaultPlayerName;
            }

            int charIndex = PlayerPrefs.GetInt(CharacterIndexKey);
            Toggle[] chars = charGroup.GetComponentsInChildren<Toggle>();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i].isOn = i == charIndex;
            }
        }

        public void StartMultiplayer()
        {
            string room = roomName.text;
            string player = playerName.text;
            Toggle[] chars = charGroup.GetComponentsInChildren<Toggle>();
            int charIndex = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i].isOn)
                {
                    charIndex = i;
                    break;
                }
            }

            // save preferences
            PlayerPrefs.SetString(RoomNameKey, room);
            PlayerPrefs.SetString(PlayerNameKey, player);
            PlayerPrefs.SetInt(CharacterIndexKey, charIndex);
            PlayerPrefs.Save();

            if (joining)
            {
                StartDiscovery(player, charIndex);
            }
            else
            {
                StartAdvertising(room, allowJoinDuringGame.isOn, autoConnect.isOn, player, charIndex);
            }
        }

        internal void Update()
        {
            if (statusText != null)
            {
                statusText.text = statusMsg;
            }
        }

        internal void StartDiscovery(string name, int charIndex)
        {
            statusMsg = "Looking for Games...";

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

        internal void StartAdvertising(string roomName, bool joinDuringGame, bool autoconnect, string name, int charIndex)
        {
            if (!joinDuringGame || !autoconnect)
            {
                statusMsg = "Waiting for players...";
                Debug.Log(statusMsg);
                ShowLobbyPanel();
            }
            else
            {
                Debug.Log("Starting Game!");
                string deviceId = string.Empty;
                string endpointId = string.Empty;

                // start the game, others can join whenever.
                PlayerInfo.AddPendingPlayer(deviceId, endpointId, name, charIndex);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartPlaying();
                }
                else
                {
                    Application.LoadLevel(1);
                }
            }
        }
    }
}