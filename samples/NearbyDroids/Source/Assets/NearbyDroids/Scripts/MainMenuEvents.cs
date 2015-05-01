// <copyright file="MainMenuEvents.cs" company="Google Inc.">
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
    using GooglePlayGames.BasicApi.Nearby;
    using UnityEngine;

    /// <summary>
    /// Main menu events.
    /// </summary>
    public class MainMenuEvents : MonoBehaviour
    {
        // references to other parts of the UI
        public GameObject mainMenuPanel;
        public GameObject multiplayerRoomPanel;
        public GameObject createRoomGroup;

        internal void Awake()
        {
            // show the main menu at start
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
                multiplayerRoomPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Starts single player game play.
        /// </summary>
        public void Play()
        {
            // read player preferences for the name and avatar.
            string defaultPlayerName = PlayerPrefs.GetString(Multiplayer.PlayerNameKey);
            if (defaultPlayerName == null)
            {
                defaultPlayerName = "Me";
            }

            int avatarIndex = PlayerPrefs.GetInt(Multiplayer.AvatarIndexKey, 0);

            PlayerInfo.AddPendingPlayer(
                new NearbyPlayer(defaultPlayerName), avatarIndex);
            
            GameManager.Instance.StartPlaying(GameManager.GameType.SinglePlayer);
        }

        /// <summary>
        /// Shows the main menu
        /// </summary>
        public void MainMenu()
        {
            GameManager.Instance.StopPlaying();
        }

        /// <summary>
        /// Creates the room for a multi player game.
        /// </summary>
        public void CreateRoom()
        {
            mainMenuPanel.SetActive(false);
            multiplayerRoomPanel.SetActive(true);
            createRoomGroup.SetActive(true);
            multiplayerRoomPanel.GetComponent<Multiplayer>().Joining = false;
        }

        /// <summary>
        /// Joins the room of a multi player game.
        /// </summary>
        public void JoinRoom()
        {
            mainMenuPanel.SetActive(false);
            multiplayerRoomPanel.SetActive(true);
            createRoomGroup.SetActive(false);
            multiplayerRoomPanel.GetComponent<Multiplayer>().Joining = true;
        }
    }
}
