// <copyright file="NearbyPlayer.cs" company="Google Inc.">
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
    using UnityEngine;

    /// <summary>
    /// Represents a nearby player or address of a room.
    /// </summary>
    public class NearbyPlayer
    {
        // keep a dictionary of all the players so we can look
        // them up when needed by device ID.
        private static Dictionary<string, NearbyPlayer> allPlayers =
            new Dictionary<string, NearbyPlayer>();

        // device id is stably unique to a device.
        private string deviceId;

        // endpoint is the current endpoint for the player.  This can change
        // if there is a network issue or something which causes a reconnection.
        private string endpointId;

        // the human friendly name of the player/room.
        private string name;

    private static string localDeviceId;
    public static string LocalDeviceId {
        get {
            localDeviceId = PlayerPrefs.GetString ("_NearbyLocalDeviceID_", null);
            if (string.IsNullOrEmpty(localDeviceId)) {
                localDeviceId = System.Guid.NewGuid ().ToString();
                PlayerPrefs.SetString ("_NearbyLocalDeviceID_", localDeviceId);
                PlayerPrefs.Save ();
            }
            return localDeviceId;
        }
    }

        /// <summary>
        /// Initializes a new instance of the <see cref="NearbyDroids.NearbyPlayer"/> class.
        /// which is local to the game.
        /// </summary>
        /// <param name="name">Name.</param>
        public NearbyPlayer(string name)
        {
            if (PlayGamesPlatform.Nearby == null)
            {
                Debug.Log("Whoa!!! Nearby is null!");
            }

            this.name = name;
            this.deviceId = LocalDeviceId;
            this.endpointId = "_LOCAL_ENDPOINT_";

            allPlayers[deviceId] = this;

            Debug.Log("Creating local player " + name + "@" + deviceId + ":" + endpointId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NearbyDroids.NearbyPlayer"/> class.
        /// Creates a remote player.
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        /// <param name="endpointId">Endpoint identifier.</param>
        /// <param name="name">Name.</param>
        public NearbyPlayer(string deviceId, string endpointId, string name)
        {
            Debug.Log("Creating player " + name + "@" + deviceId + ":" + endpointId);
            this.name = name;
            this.deviceId = deviceId;
            this.endpointId = endpointId;

            if (deviceId != null)
            {
                allPlayers [deviceId] = this;
            }
            else
            {
                Debug.Log ("Skipping registering player: " + endpointId + " (" + name + ") with null device ID");
            }
        }

        public string DeviceId
        {
            get
            {
                return this.deviceId;
            }
        }

        public string EndpointId
        {
            get
            {
                return this.endpointId;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is local.
        /// </summary>
        /// <value><c>true</c> if this instance is local; otherwise, <c>false</c>.</value>
        public bool IsLocal
        {
            get
            {
                return DeviceId == NearbyPlayer.LocalDeviceId;
            }
        }

        /// <summary>
        /// Finds the by endpoint identifier.
        /// </summary>
        /// <returns>The by endpoint identifier.</returns>
        /// <param name="key">Key.</param>
        public static NearbyPlayer FindByEndpointId(string key)
        {
            foreach (NearbyPlayer p in allPlayers.Values)
            {
                if (p.EndpointId == key)
                {
                    return p;
                }
            }
            return null;
        }

        public static NearbyPlayer FindByDeviceId(string key)
        {
            if (allPlayers.ContainsKey (key))
            {
                return allPlayers [key];
            }
            return null;
        }
    }
}
