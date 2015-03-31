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
    using UnityEngine;

    /// <summary>
    /// Main menu events.
    /// </summary>
    public class MainMenuEvents : MonoBehaviour
    {
        public GameObject mainMenuPanel;
        public GameObject multiplayerRoomPanel;
        public GameObject createRoomGroup;

        internal void Awake()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
                multiplayerRoomPanel.SetActive(false);
            }
        }

        public void Play()
        {
            string defaultPlayerName = PlayerPrefs.GetString(Multiplayer.PlayerNameKey);
            if (defaultPlayerName == null)
            {
                defaultPlayerName = "Me";
            }

            int charIndex = PlayerPrefs.GetInt(Multiplayer.CharacterIndexKey, 0);

            PlayerInfo.AddPendingPlayer(string.Empty, string.Empty, defaultPlayerName, charIndex);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartPlaying();
            }
            else
            {
                Application.LoadLevel(1);
            }
        }

        public void MainMenu()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StopPlaying();
            }
            else
            {
                Application.LoadLevel(0);
            }
        }

        public void CreateRoom()
        {
            mainMenuPanel.SetActive(false);
            multiplayerRoomPanel.SetActive(true);
            createRoomGroup.SetActive(true);
            multiplayerRoomPanel.GetComponent<Multiplayer>().Joining = false;
        }

        public void JoinRoom()
        {
            mainMenuPanel.SetActive(false);
            multiplayerRoomPanel.SetActive(true);
            createRoomGroup.SetActive(false);
            multiplayerRoomPanel.GetComponent<Multiplayer>().Joining = true;
        }
    }
}
