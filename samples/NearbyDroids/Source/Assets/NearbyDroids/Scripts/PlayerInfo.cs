// <copyright file="PlayerInfo.cs" company="Google Inc.">
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
    using UnityEngine.UI;

    public class PlayerInfo : ScriptableObject
    {
        private static Dictionary<string, PlayerInfo> playerScores = new Dictionary<string, PlayerInfo>();

        private GameObject scorePanel;
        private Text scoreText;
        private Text nameText;

        // device id is stably unique to a device.
        private string deviceId;

        // endpoint is the current endpoint for the player.  This can change
        // if there is a network issue or something which causes a reconnection.
        private string endpointId;

        /// <summary>
        ///  the avatar index for this player.
        /// </summary>
        private int charIndex;

        // the score
        private int score = 0;

        public static IEnumerable<PlayerInfo> AllPlayers
        {
            get
            {
                return playerScores.Values;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string DeviceId
        {
            get
            {
                return deviceId;
            }
        }

        public string EndpointId
        {
            get
            {
                return endpointId;
            }
        }

        public int Score
        {
            get
            {
                return score;
            }

            set
            {
                score = value;
            }
        }

        public int AvatarIndex
        {
            get
            {
                return charIndex;
            }
        }

        public static PlayerInfo CreatePlayer(
            GameObject scorePanel,
            string name, 
            string deviceId,
            string endpointId,
            GameObject prefab)
        {
            PlayerInfo info = playerScores.ContainsKey(deviceId) ? playerScores[deviceId] : null;

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<PlayerInfo>();
                playerScores.Add(deviceId, info);
            }
            else
            {
                DestroyObject(info.scorePanel);
            }

            GameObject score = Instantiate(prefab) as GameObject;
            info.scorePanel = score;

            score.transform.SetParent(scorePanel.transform, false);
            score.name = "score_" + name;
            Text[] texts = score.GetComponentsInChildren<Text>();
            foreach (Text t in texts)
            {
                if (t.gameObject.name.EndsWith("name"))
                {
                    info.nameText = t;
                }
                else if (t.gameObject.name.EndsWith("score"))
                {
                    info.scoreText = t;
                }
            }

            info.nameText.text = name;
            info.scoreText.text = "0";
            info.deviceId = deviceId;
            info.endpointId = endpointId;
            info.scorePanel = score;

            LayoutRebuilder.MarkLayoutForRebuild(scorePanel.GetComponent<RectTransform>());

            return info;
        }

        internal static void UpdateScores()
        {
            foreach (PlayerInfo p in playerScores.Values)
            {
                p.scoreText.text = string.Empty + p.score;
            }
        }

        public static int GetScore(string deviceId)
        {
            PlayerInfo p = GetPlayer(deviceId);
            if (p != null)
            {
                return p.score;
            }

            return 0;
        }

        public static int AddScore(string deviceId, int value)
        {
            PlayerInfo p = GetPlayer(deviceId);
            if (p != null)
            {
                p.score += value;
                return p.score;
            }

            return 0;
        }

        public static PlayerInfo GetPlayer(string deviceId)
        {
            return playerScores[deviceId];
        }

        public static void AddPendingPlayer(string deviceId, string endpointId, string name, int charIndex)
        {
            PlayerInfo info = playerScores.ContainsKey(deviceId) ? playerScores[deviceId] : null;

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<PlayerInfo>();
                playerScores.Add(deviceId, info);
            }
            else
            {
                DestroyObject(info.scorePanel);
            }

            info.deviceId = deviceId;
            info.endpointId = endpointId;
            info.name = name;
            info.charIndex = charIndex;
        }
    }
}