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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using GooglePlayGames;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Player info for the gamemanager.  This keeps track of the ids and
    /// scores of the players, as well as references to UI elements for the
    /// player.  The PlayerInfo is not passed to other players, but the
    /// PlayerData member object is passed around.
    /// </summary>
    public class PlayerInfo : ScriptableObject
    {
        // serializer for player info - this is needed for the connection request
        // payload.
        private static BinaryFormatter bf = new BinaryFormatter();

        // all users so we can look them up and iterate over them.
        private static Dictionary<string, PlayerInfo> allPlayers =
            new Dictionary<string, PlayerInfo>();

        // UI element references
        private GameObject scorePanel;
        private Text scoreText;
        private Text nameText;

        // flag indicating of the player has moved in this turn.
        private bool moved;

        /// <summary>
        /// The player's name and connection information.
        /// </summary>
        private NearbyPlayer player;

        /// <summary>
        /// Keep the state of the player in a struct so it can be serialized
        /// easily.
        /// </summary>
        private PlayerData dataState;

        public static IEnumerable<PlayerInfo> AllPlayers
        {
            get
            {
                return allPlayers.Values;
            }
        }

        public static PlayerInfo LocalPlayer
        {
            get
            {
                return GetPlayer(NearbyPlayer.LocalDeviceId);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NearbyDroids.PlayerInfo"/> has moved.
        /// </summary>
        /// <value><c>true</c> if moved; otherwise, <c>false</c>.</value>
        public bool Moved
        {
            get
            {
                return moved;
            }

            set
            {
                moved = value;
            }
        }

        public int Score
        {
            get
            {
                return dataState.score;
            }

            set
            {
                dataState.score = value;
            }
        }

        public int AvatarIndex
        {
            get
            {
                return dataState.avatarIndex;
            }
        }

        public string DeviceId
        {
            get
            {
                return player.DeviceId;
            }
        }

        public NearbyPlayer Player
        {
            get
            {
                return player;
            }
        }

        public PlayerData DataState
        {
            get
            {
                return this.dataState;
            }

            set
            {
                dataState = value;
            }
        }

        public GameObject ScorePanel
        {
            get
            {
                return scorePanel;
            }

            set
            {
                scorePanel = value;
            }
        }

        public Text NameText
        {
            get
            {
                return nameText;
            }

            set
            {
                nameText = value;
            }
        }

        public Text ScoreText
        {
            get
            {
                return scoreText;
            }

            set
            {
                scoreText = value;
            }
        }

        public bool IsLocal
        {
            get
            {
                return this.player.IsLocal;
            }
        }

        public byte[] SerializedData
        {
            get
            {
                MemoryStream m = new MemoryStream();
                bf.Serialize(m, dataState);
                m.Flush();
                return m.ToArray();
            }
        }

        internal void SetDataState(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                dataState = new PlayerData();
                dataState.deviceId = NearbyPlayer.LocalDeviceId;
            }
            else
            {
                MemoryStream s = new MemoryStream(data);
                PlayerData d = bf.Deserialize(s) as PlayerData;

                // Note: Could handle versioning conversions here...
                dataState = d;
            }
        }

        public static void ClearAllPlayers()
        {
            allPlayers.Clear();
        }

        public static int GetScore(string deviceId)
        {
            PlayerInfo p = GetPlayer(deviceId);
            if (p != null)
            {
                return p.dataState.score;
            }

            return 0;
        }

        public static int AddScore(string deviceId, int value)
        {
            PlayerInfo p = GetPlayer(deviceId);
            if (p != null)
            {
                p.dataState.score += value;
                return p.dataState.score;
            }

            return 0;
        }

        public static PlayerInfo GetPlayer(string deviceId)
        {
            if (allPlayers.ContainsKey(deviceId))
            {
                return allPlayers[deviceId];
            }
            else
            {
                Debug.Log("Could not find player for deviceId " + deviceId);
            }

            return null;
        }

        public static PlayerInfo AddPendingPlayer(string endpointId, NearbyPlayer player, byte[] data)
        {
            PlayerInfo info;

            if (player != null && player.DeviceId != null && allPlayers.ContainsKey(player.DeviceId))
            {
                info = allPlayers[player.DeviceId];
            }
            else
            {
                info = null;
            }

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<PlayerInfo>();
            }
            else
            {
                DestroyObject(info.scorePanel);
            }

            info.SetDataState(data);
            NearbyPlayer newPlayer = NearbyPlayer.FindByDeviceId(info.dataState.DeviceId);
            if (newPlayer == null)
            {
                newPlayer = new NearbyPlayer (info.dataState.DeviceId, endpointId, info.DataState.Name);
            }
            info.player = newPlayer;
            info.dataState.Name = newPlayer.Name;

            allPlayers.Add(newPlayer.DeviceId, info);

            return info;
        }

        public static PlayerInfo AddPendingPlayer(NearbyPlayer player, int charIndex)
        {
            PlayerInfo info;
            if (player != null && player.DeviceId != null && allPlayers.ContainsKey(player.DeviceId))
            {
                info = allPlayers[player.DeviceId];
            }
            else
            {
                info = null;
            }

            if (info == null)
            {
                info = ScriptableObject.CreateInstance<PlayerInfo>();
                allPlayers.Add(player.DeviceId, info);
            }
            else
            {
                DestroyObject(info.scorePanel);
                info.scorePanel = null;
            }

            info.player = player;
            info.dataState = new PlayerData();
            info.dataState.avatarIndex = charIndex;
            info.dataState.deviceId = player.DeviceId;
            info.dataState.Name = player.Name;
            return info;
        }

        public static void RemovePendingPlayer(string deviceId)
        {
            allPlayers.Remove(deviceId);
        }

        [Serializable]
        public class PlayerData
        {
            internal readonly int CurrentVersion = 1;
            internal int version = 1;
            internal string deviceId;
            internal string name;
            internal int avatarIndex;
            internal int score;

            public string DeviceId
            {
                get
                {
                    return deviceId;
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }

                set
                {
                    name = value;
                }
            }

            public int AvatarIndex
            {
                get
                {
                    return avatarIndex;
                }

                set
                {
                    avatarIndex = value;
                }
            }
        }
    }
}
