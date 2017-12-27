// <copyright file="NearbyRoom.cs" company="Google Inc.">
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
    using GooglePlayGames;
    using GooglePlayGames.BasicApi.Nearby;
    using UnityEngine;

    /// <summary>
    /// Nearby room is the logical room that is advertised
    /// to nearby players which then can discover the room and connect.
    /// It forms the hub of a hub and spoke model of multi-station communication.
    /// </summary>
    public class NearbyRoom : IMessageListener
    {
        // instance of class to implement discovery result interface.
        private static RoomListener roomListener = new RoomListener();

        // all the rooms so we can look them up when needed by endpoint ID
        // we don't know the device id of the room.
        private static Dictionary<string, NearbyRoom> knownRooms =
            new Dictionary<string, NearbyRoom>();

        // address of this room.
        private NearbyPlayer address;

        // true if connection requests should be automatically accepted.
        private bool autoJoin;

        // true if the room should always be discoverable.
        private bool alwaysOpen;

        // true if the room is local to this game.
        private bool local;

        /// <summary>
        /// The connection data builder. used to create the payload
        /// sent to the remote player when accepting the connection
        /// request.
        /// </summary>
        private ConnectionDataBuilder connectionDataBuilder;

        /// <summary>
        /// The message handler for when messages are received.
        /// </summary>
        private volatile MessageReceiver messageHandler;

        /// <summary>
        /// The player handler used to notify when players arrive or leave
        /// the room.
        /// </summary>
        private volatile PlayerEventHandler playerHandler;

        /// <summary>
        /// The player found callback. called when players are connected.
        /// </summary>
        private volatile Action<string, byte[]> playerFoundCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearbyDroids.NearbyRoom"/> class.
        /// This is a local room.
        /// </summary>
        /// <param name="name">Name - the local room name</param>
        internal NearbyRoom(string name)
        {
            this.address = new NearbyPlayer(name);
            local = true;
            knownRooms[address.EndpointId] = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NearbyDroids.NearbyRoom"/> class.
        /// This is a remote room.
        /// </summary>
        /// <param name="deviceId">Device identifier of the remote room</param>
        /// <param name="endpointId">Endpoint identifier of the remote room</param>
        /// <param name="name">Name of the remote room</param>
        internal NearbyRoom(string deviceId, string endpointId, string name)
        {
            this.address = new NearbyPlayer(deviceId, endpointId, name);
            local = false;
            knownRooms[endpointId] = this;
        }

        public delegate byte[] ConnectionDataBuilder();

        public delegate void MessageReceiver(NearbyPlayer sender, byte[] data);

        public delegate void PlayerEventHandler(NearbyPlayer player, bool present);

        public string Name
        {
            get
            {
                return address.Name;
            }
        }

        public bool AutoJoin
        {
            get
            {
                return autoJoin;
            }

            set
            {
                autoJoin = value;
            }
        }

        public bool AlwaysOpen
        {
            get
            {
                return alwaysOpen;
            }

            set
            {
                alwaysOpen = value;
            }
        }

        public string EndpointId
        {
            get
            {
                return address.EndpointId;
            }
        }

        public NearbyPlayer Address
        {
            get
            {
                return address;
            }
        }

        public bool IsLocal
        {
            get
            {
                return local;
            }
        }

        public ConnectionDataBuilder ConnectionData
        {
            get
            {
                return connectionDataBuilder;
            }

            set
            {
                connectionDataBuilder = value;
            }
        }

        public MessageReceiver MessageHandler
        {
            get
            {
                return messageHandler;
            }

            set
            {
                messageHandler = value;
            }
        }

        public PlayerEventHandler PlayerHandler
        {
            get
            {
                return playerHandler;
            }

            set
            {
                playerHandler = value;
            }
        }

        /// <summary>
        /// Creates a local room.
        /// </summary>
        /// <returns>The room.</returns>
        /// <param name="roomName">Room name.</param>
        public static NearbyRoom CreateRoom(string roomName)
        {
            return new NearbyRoom(roomName);
        }

        /// <summary>
        /// Lookups the room based on the endpoint id.
        /// </summary>
        /// <returns>The room.</returns>
        /// <param name="deviceId">Device identifier.</param>
        public static NearbyRoom LookupRoomByDeviceId(string deviceId)
        {
            foreach (NearbyRoom r in knownRooms.Values)
            {
                if (r.Address.DeviceId == deviceId)
                {
                    return r;
                }
            }
            return null;
        }

        public static NearbyRoom LookupRoomByEndpoint(string endpointId)
        {
            if (knownRooms.ContainsKey(endpointId))
            {
                return knownRooms[endpointId];
            }

            return null;
        }

        /// <summary>
        /// Finds the rooms by starting the discovery of nearby connection
        /// endpoints.
        /// </summary>
        /// <param name="callback">Callback for when a room is found.</param>
        public static void FindRooms(Action<NearbyRoom, bool> callback)
        {
            roomListener.RoomDiscoveredCallback = callback;
            Debug.Log("Calling StartDiscovery");
            PlayGamesPlatform.Nearby.StartDiscovery(
                PlayGamesPlatform.Nearby.GetServiceId(),
                TimeSpan.FromSeconds(0),
                roomListener);
        }

        /// <summary>
        /// Stops the room discovery.
        /// </summary>
        public static void StopRoomDiscovery()
        {
            PlayGamesPlatform.Nearby.StopDiscovery(
                PlayGamesPlatform.Nearby.GetServiceId());
        }

        /// <summary>
        /// Stops all nearby connection activity.
        /// </summary>
        public static void StopAll()
        {
            PlayGamesPlatform.Nearby.StopAllConnections();
            StopRoomDiscovery();
            Dictionary<string, NearbyRoom>.KeyCollection keys = knownRooms.Keys;
            foreach (string k in keys)
            {
                knownRooms[k].CloseRoom();
            }

            knownRooms.Clear();
        }

        /// <summary>
        /// Waits for players. the callback is called when a player
        /// sends a connection request.
        /// This also starts advertising the room via nearby connections.
        /// </summary>
        /// <param name="callback">Callback for when a player sends a connection request.</param>
        public void WaitForPlayers(Action<string, byte[]> callback)
        {
            playerFoundCallback = callback;
            OpenRoom();
        }

        /// <summary>
        /// Opens the room. which starts advertising.
        /// </summary>
        public void OpenRoom()
        {
            Debug.Log("Advertising Room: " + Name);
            List<string> appIdentifiers = new List<string>();

            appIdentifiers.Add(PlayGamesPlatform.Nearby.GetAppBundleId());
            PlayGamesPlatform.Nearby.StartAdvertising(
                Name,
                appIdentifiers,
                TimeSpan.FromSeconds(0),
                OnAdvertisingResult,
                OnConnectionRequest);
            Debug.Log("Advertising: " + PlayGamesPlatform.Nearby.GetAppBundleId());
        }

        /// <summary>
        /// Closes the room. which stops advertising.
        /// </summary>
        public void CloseRoom()
        {
            Debug.Log("Closing Room " + Name);
            PlayGamesPlatform.Nearby.StopAdvertising();
            playerFoundCallback = null;
        }

        /// <summary>
        /// Joins the room.  This is called by a remote player to
        /// join a room.  This sends the nearby connection request message
        /// to the remote room.
        /// </summary>
        /// <param name="localPlayer">the Local player.</param>
        /// <param name="playerData">the serialized Player data to include in the request.</param>
        /// <param name="callback">Callback from the remote room accepting or rejecting the request.</param>
        public void JoinRoom(
            NearbyPlayer localPlayer,
            byte[] playerData,
            Action<ConnectionResponse> callback)
        {
            PlayGamesPlatform.Nearby.SendConnectionRequest(
                localPlayer.Name,
                address.EndpointId,
                playerData,
                callback,
                this);
        }

        /// <summary>
        /// Disconnects from this room.
        /// </summary>
        public void Disconnect()
        {
            PlayGamesPlatform.Nearby.DisconnectFromEndpoint(address.EndpointId);
        }

        /// <summary>
        /// Raises the advertising result event.
        /// </summary>
        /// <param name="result">Result of the call.</param>
        internal void OnAdvertisingResult(AdvertisingResult result)
        {
            Debug.Log("OnAdvertisingResult: " + result);
        }

        /// <summary>
        /// Raises the connection request event.
        /// </summary>
        /// <param name="request">Request sent to join the room.</param>
        internal void OnConnectionRequest(ConnectionRequest request)
        {
            if (playerFoundCallback != null)
            {
                playerFoundCallback.Invoke(request.RemoteEndpoint.EndpointId, request.Payload);
            }

            if (AutoJoin)
            {
                Debug.Log("Automatically connecting to " + request.RemoteEndpoint);
                AcceptRequest(request.RemoteEndpoint.EndpointId);
            }
        }

        /// <summary>
        /// Accepts the request.
        /// </summary>
        /// <param name="player">Player to accept the request from.</param>
        public void AcceptRequest(string endpointId)
        {
            NearbyPlayer player = NearbyPlayer.FindByEndpointId (endpointId);
            PlayGamesPlatform.Nearby.AcceptConnectionRequest(
                player.EndpointId,
                ConnectionData(),
                this);
            playerHandler(player, true);
        }

        #region IMessageListener implementation

        /// <summary>
        /// Raises the message received event.
        /// </summary>
        /// <param name="remoteEndpointId">Remote endpoint identifier.</param>
        /// <param name="data">Data payload of the message.</param>
        /// <param name="isReliableMessage">If set to <c>true</c> is reliable message.</param>
        public void OnMessageReceived(string remoteEndpointId, byte[] data, bool isReliableMessage)
        {
            Debug.Log("RECEIVED Message from " + remoteEndpointId);
            NearbyPlayer sender = NearbyPlayer.FindByEndpointId(remoteEndpointId);
            if (messageHandler != null)
            {
                messageHandler(sender, data);
            }
            else
            {
                Debug.Log("Messagehandler not set, ignoring!");
            }
        }

        /// <summary>
        /// Raises the remote endpoint disconnected event.
        /// </summary>
        /// <param name="remoteEndpointId">Remote endpoint identifier.</param>
        public void OnRemoteEndpointDisconnected(string remoteEndpointId)
        {
            NearbyPlayer player = NearbyPlayer.FindByEndpointId(remoteEndpointId);
            playerHandler(player, false);
        }

        #endregion

        /// <summary>
        /// Room listener. helper class implementing the discover listener interface.
        /// </summary>
        internal class RoomListener : IDiscoveryListener
        {
            private Action<NearbyRoom, bool> roomDiscoveredCallback;

            public Action<NearbyRoom, bool> RoomDiscoveredCallback
            {
                get
                {
                    return this.roomDiscoveredCallback;
                }

                set
                {
                    roomDiscoveredCallback = value;
                }
            }

            #region IDiscoveryListener implementation

            public void OnEndpointFound(EndpointDetails discoveredEndpoint)
            {
                Debug.Log("Found Endpoint!");
                NearbyRoom room = new NearbyRoom(
                                      null,
                                      discoveredEndpoint.EndpointId,
                                      discoveredEndpoint.Name);
                if (roomDiscoveredCallback != null)
                {
                    Debug.Log("Invoking roomCallback.");
                    roomDiscoveredCallback.Invoke(room, true);
                }
                else
                {
                    Debug.Log("No roomCallback configured.");
                }
            }

            public void OnEndpointLost(string lostEndpointId)
            {
                Debug.Log("Endpoint lost: " + lostEndpointId);
                if (roomDiscoveredCallback != null)
                {
                    NearbyRoom room = NearbyRoom.LookupRoomByEndpoint(lostEndpointId);
                    if (room != null)
                    {
                        roomDiscoveredCallback.Invoke(room, false);
                    }
                }
            }

            #endregion
        }
    }
}
