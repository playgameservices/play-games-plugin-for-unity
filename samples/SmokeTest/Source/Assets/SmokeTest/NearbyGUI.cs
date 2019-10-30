// <copyright file="NearbyGUI.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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

namespace SmokeTest
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames;
    using GooglePlayGames.BasicApi.Nearby;
    using UnityEngine;
#if UNITY_ANDROID && UNITY_2019
    using UnityEngine.Android;
#endif

    public class NearbyGUI : MonoBehaviour, IDiscoveryListener, IMessageListener
    {
        // calculate a nice grid for the layout.
        private const int GridCols = 3;
        private const int GridRows = 10;
        private const float FontSizeFactor = 35;

        private const int Spacing = 4;
        private const int Margin = 8;

        private readonly int timeoutMillis = 60000;

        // link back to main gui
        private MainGui mOwner;

        // endpoints discovered and/or connected
        private Dictionary<string, EndpointHolder> mEndpoints;

        // scroll position for the endpoints
        private Vector2 mEndpointsViewVector;

        // hash set of known endpoints this allows the clean up of
        // endpoints that were connected to vs. discovered.
        private HashSet<string> mKnownEndpoints;

        // message received from others
        private List<string> mMessageLog;

        // and the scroll position
        private Vector2 mMessageViewVector;

        // status string for nearby
        private string mNearbyStatus;

        // timers to keep track of discovery and advertising.
        private float mAdvertisingRemaining;
        private float mDiscoveryRemaining;

        // Constructed by the main gui
        internal NearbyGUI(MainGui owner)
        {
            mOwner = owner;
            mEndpoints = new Dictionary<string, EndpointHolder>();
            mEndpointsViewVector = new Vector2();
            mMessageLog = new List<string>();
            mKnownEndpoints = new HashSet<string>();
            
#if UNITY_ANDROID && UNITY_2019
            Permission.RequestUserPermission(Permission.FineLocation);
            Permission.RequestUserPermission(Permission.CoarseLocation);
#endif
        }

        internal enum EndpointState
        {
            DISCOVERED,
            REQUESTED,
            REJECTED,
            CONNECTED,
            DISCONNECTED,
            ERROR
        }

        public Rect CalcGrid(int col, int row)
        {
            return CalcGrid(col, row, 1, 1);
        }

        Rect CalcGrid(int col, int row, int colcount, int rowcount)
        {
            int cellW = (Screen.width - 2 * Margin - (GridCols - 1) * Spacing) / GridCols;
            int cellH = (Screen.height - 2 * Margin - (GridRows - 1) * Spacing) / GridRows;
            return new Rect(Margin + col * (cellW + Spacing),
                Margin + row * (cellH + Spacing),
                cellW + (colcount - 1) * (Spacing + cellW),
                cellH + (rowcount - 1) * (Spacing + cellH));
        }

        public void OnEndpointFound(EndpointDetails discoveredEndpoint)
        {
            Debug.Log("OnEndpointFound");
            mNearbyStatus = "OnEndpointFound" + discoveredEndpoint.Name +
                            " " + discoveredEndpoint.EndpointId;
            EndpointHolder holder = new EndpointHolder();
            holder.Endpoint = discoveredEndpoint;
            holder.State = EndpointState.DISCOVERED;
            mEndpoints.Remove(discoveredEndpoint.EndpointId);
            mEndpoints.Add(discoveredEndpoint.EndpointId, holder);
            mKnownEndpoints.Add(discoveredEndpoint.EndpointId);
        }

        public void OnEndpointLost(string lostEndpointId)
        {
            // Endpoint lost can be called when the remote calls stop advertising.  This happens even
            // when the connection is established.
            mNearbyStatus = "OnEndpointLost: " + lostEndpointId;
            EndpointHolder ep = mEndpoints[lostEndpointId];
            if (ep != null && ep.State != EndpointState.CONNECTED)
            {
                mEndpoints.Remove(lostEndpointId);
                mKnownEndpoints.Remove(lostEndpointId);
            }
        }

        public void OnMessageReceived(string remoteEndpointId, byte[] data, bool reliableMessage)
        {
            string msg = System.Text.Encoding.UTF8.GetString(data);
            mMessageLog.Add("From: " + remoteEndpointId + ": " + msg);
        }

        public void OnRemoteEndpointDisconnected(string remoteEndpointId)
        {
            mNearbyStatus = "OnRemoteEndpointDisconnected: " + remoteEndpointId;
            EndpointHolder ep = mEndpoints[remoteEndpointId];
            ep.State = EndpointState.DISCONNECTED;

            //OnRemoteEndpointDisconnected is called when the connection is closed.
            // If we are the "discovery" side of the conversation,
            // keep the endpoint in the list, so we can re-connect.
            if (mKnownEndpoints.Contains(remoteEndpointId))
            {
                mEndpoints.Remove(remoteEndpointId);
            }
        }

        // Update is called once per frame
        internal void OnGUI()
        {
            INearbyConnectionClient client = PlayGamesPlatform.Nearby;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.touches[0];

                // TODO: handle the endpoint scroll vs. the message scroll.
                if (touch.phase == TouchPhase.Moved)
                {
                    mMessageViewVector.y += touch.deltaPosition.y;
                }
            }

            // count down the timers
            if (mAdvertisingRemaining > 0)
            {
                mAdvertisingRemaining -= Time.deltaTime;
            }

            if (mDiscoveryRemaining > 0)
            {
                mDiscoveryRemaining -= Time.deltaTime;
            }

            mOwner.DrawTitle("Nearby Connections");

            if (client == null)
            {
                mOwner.Status = "Nearby client is null!";
                mOwner.DrawStatus();
                if (GUI.Button(CalcGrid(1, 4), "Back"))
                {
                    mOwner.SetUI(MainGui.Ui.Main);
                }

                return;
            }

            string topStatus = "Nearby: " + client.GetServiceId();
            string advertButton;
            bool advertising = mAdvertisingRemaining > 0;
            string discoveryButton;
            bool discovering = mDiscoveryRemaining > 0;

            if (advertising)
            {
                topStatus += string.Format(" Advertising({0})", (int) mAdvertisingRemaining);
                advertButton = "Stop\nAdvertising";
            }
            else
            {
                advertButton = "Start\nAdvertising";
            }

            if (discovering)
            {
                topStatus += string.Format(" Disovering({0})", (int) mDiscoveryRemaining);
                discoveryButton = "Stop\nDiscovery";
            }
            else
            {
                discoveryButton = "Start\nDiscovery";
            }

            if (GUI.Button(CalcGrid(0, 1), advertButton))
            {
                if (!advertising)
                {
                    // always call stop to make sure it is clear before calling start.
                    client.StopAdvertising();

                    // use a name of null to use the default name
                    string nearbyName = null;
                    List<string> nearbyAppsIds = new List<string>();
                    Debug.Log("Advertising: " + client.GetAppBundleId());
                    nearbyAppsIds.Add(client.GetAppBundleId());
                    TimeSpan advertisingTimeSpan = TimeSpan.FromMilliseconds(timeoutMillis);
                    mAdvertisingRemaining = timeoutMillis / 1000f;
                    client.StartAdvertising(
                        nearbyName,
                        nearbyAppsIds,
                        advertisingTimeSpan,
                        OnAdvertisingResult,
                        OnConnectionRequest);
                }
                else
                {
                    client.StopAdvertising();
                    mNearbyStatus = "Advertising stopped";
                    mAdvertisingRemaining = 0f;
                }
            }
            else if (GUI.Button(CalcGrid(1, 1), discoveryButton))
            {
                string nearbyServiceId = client.GetServiceId();
                client.StopDiscovery(nearbyServiceId);

                if (!discovering)
                {
                    TimeSpan advertisingTimeSpan = TimeSpan.FromMilliseconds(timeoutMillis);
                    mDiscoveryRemaining = timeoutMillis / 1000f;
                    client.StartDiscovery(nearbyServiceId, advertisingTimeSpan, this);
                    mNearbyStatus = "Discovery started for " + nearbyServiceId;
                }
                else
                {
                    mNearbyStatus = "Discovery stopped";
                    mDiscoveryRemaining = 0f;
                }
            }
            else if (GUI.Button(CalcGrid(2, 1), "Stop All"))
            {
                client.StopAllConnections();
                mEndpoints.Clear();
                mKnownEndpoints.Clear();
                mMessageLog.Clear();
                mNearbyStatus = "Stopped all connections";
                mAdvertisingRemaining = 0;
                mDiscoveryRemaining = 0;
            }
            else if (GUI.Button(CalcGrid(0, 2), "Send All\nReliable"))
            {
                List<string> dest = new List<string>();
                foreach (EndpointHolder ep in mEndpoints.Values)
                {
                    if (ep.State == EndpointState.CONNECTED)
                    {
                        dest.Add(ep.Endpoint.EndpointId);
                    }
                }

                string msg = "Reliable from " + client.GetServiceId() + " " +
                             ((int) Time.realtimeSinceStartup);
                client.SendReliable(dest, System.Text.Encoding.UTF8.GetBytes(msg));
            }
            else if (GUI.Button(CalcGrid(1, 2), "Send All\nUnreliable"))
            {
                List<string> dest = new List<string>();
                foreach (EndpointHolder ep in mEndpoints.Values)
                {
                    if (ep.State == EndpointState.CONNECTED)
                    {
                        dest.Add(ep.Endpoint.EndpointId);
                    }
                }

                string msg = "Unreliable from " + client.GetServiceId() + " " +
                             ((int) Time.realtimeSinceStartup);
                client.SendUnreliable(dest, System.Text.Encoding.UTF8.GetBytes(msg));
            }
            else if (GUI.Button(CalcGrid(2, 2), "Back"))
            {
                mOwner.SetUI(MainGui.Ui.Main);
            }

            // Discovered endpoints
            mEndpointsViewVector = GUI.BeginScrollView(
                CalcGrid(0, 3, 3, 2),
                mEndpointsViewVector,
                CalcGrid(0, 0, 3, 1 + mEndpoints.Count));
            if (mEndpoints.Count == 0)
            {
                GUI.Label(CalcGrid(0, 0), "No Endpoints Discovered");
            }
            else
            {
                mOwner.Status = "Found " + mEndpoints.Count + " endpoints";
            }

            int index = 0;
            GUIStyle style = mOwner.GuiSkin.GetStyle("box");
            List<string> keysToRemove = new List<string>();
            foreach (EndpointHolder endpt in mEndpoints.Values)
            {
                if (index % 2 == 0)
                {
                    GUI.BeginGroup(CalcGrid(0, index * 2, 3, 2), style);
                }
                else
                {
                    GUI.BeginGroup(CalcGrid(0, index * 2, 3, 2));
                }

                GUI.Label(CalcGrid(0, 0), endpt.ToString());
                if (endpt.State == EndpointState.DISCOVERED ||
                    endpt.State == EndpointState.DISCONNECTED ||
                    endpt.State == EndpointState.REJECTED)
                {
                    if (GUI.Button(CalcGrid(1, 0), "Connect"))
                    {
                        string name = null;
                        byte[] payload = new byte[0];
                        client.SendConnectionRequest(
                            name,
                            endpt.Endpoint.EndpointId,
                            payload,
                            OnConnectionResponse,
                            this);
                    }
                }

                if (endpt.State == EndpointState.REQUESTED)
                {
                    if (GUI.Button(CalcGrid(1, 1), "Accept"))
                    {
                        client.AcceptConnectionRequest(endpt.Endpoint.EndpointId, new byte[0], this);
                        endpt.State = EndpointState.CONNECTED;
                    }
                    else if (GUI.Button(CalcGrid(2, 1), "Reject"))
                    {
                        client.RejectConnectionRequest(endpt.Endpoint.EndpointId);
                        keysToRemove.Add(endpt.Endpoint.EndpointId);
                    }
                }

                if (endpt.State == EndpointState.CONNECTED)
                {
                    if (GUI.Button(CalcGrid(1, 0), "Disconnect"))
                    {
                        client.DisconnectFromEndpoint(endpt.Endpoint.EndpointId);
                        endpt.State = EndpointState.DISCONNECTED;
                    }

                    if (GUI.Button(CalcGrid(1, 1), "Send\nReliable"))
                    {
                        List<string> dest = new List<string>();
                        dest.Add(endpt.Endpoint.EndpointId);
                        string msg = "Reliable from " + client.GetServiceId() + " " +
                                     ((int) Time.realtimeSinceStartup);
                        client.SendReliable(dest, System.Text.Encoding.UTF8.GetBytes(msg));
                    }

                    if (GUI.Button(CalcGrid(2, 1), "Send\nUnreliable"))
                    {
                        // client.SendReliable
                        List<string> dest = new List<string>();
                        dest.Add(endpt.Endpoint.EndpointId);
                        string msg = "Unreliable from " + client.GetServiceId() + " " +
                                     ((int) Time.realtimeSinceStartup);
                        client.SendReliable(dest, System.Text.Encoding.UTF8.GetBytes(msg));
                    }
                }

                GUI.EndGroup();
                index++;
            }

            GUI.EndScrollView();

            foreach (string key in keysToRemove)
            {
                mEndpoints.Remove(key);
            }

            mMessageViewVector = GUI.BeginScrollView(
                CalcGrid(0, 6, 3, 2),
                mMessageViewVector,
                CalcGrid(0, 0, 3, 1 + mMessageLog.Count));
            if (mMessageLog.Count == 0)
            {
                GUI.Label(CalcGrid(0, 0), "No Messages");
            }

            for (int i = 0; i < mMessageLog.Count; i++)
            {
                GUI.Label(CalcGrid(0, i, 3, 1), mMessageLog[i]);
            }

            GUI.EndScrollView();
            if (mNearbyStatus != null)
            {
                GUI.Label(CalcGrid(0, 8, 3, 2), mNearbyStatus);
            }
        }

        internal void OnConnectionResponse(ConnectionResponse response)
        {
            Debug.Log("OnConnection Response : " + response.ResponseStatus);
            mNearbyStatus = "OnConnectionResponse: " + response.ResponseStatus;
            switch (response.ResponseStatus)
            {
                case ConnectionResponse.Status.Accepted:
                    mEndpoints[response.RemoteEndpointId].State = EndpointState.CONNECTED;
                    break;
                case ConnectionResponse.Status.Rejected:
                    mEndpoints[response.RemoteEndpointId].State = EndpointState.REJECTED;
                    break;
                case ConnectionResponse.Status.ErrorAlreadyConnected:
                    // it is an error, but we can treat it like connected.
                    mEndpoints[response.RemoteEndpointId].State = EndpointState.CONNECTED;
                    break;
                case ConnectionResponse.Status.ErrorEndpointNotConnected:
                    mEndpoints[response.RemoteEndpointId].State = EndpointState.ERROR;
                    break;
                case ConnectionResponse.Status.ErrorInternal:
                    mEndpoints[response.RemoteEndpointId].State = EndpointState.ERROR;
                    break;
                case ConnectionResponse.Status.ErrorNetworkNotConnected:
                    mEndpoints[response.RemoteEndpointId].State = EndpointState.ERROR;
                    break;
                default:
                    Debug.LogError("Unknown or unsupported status: " + response.ResponseStatus);
                    if (mEndpoints.ContainsKey(response.RemoteEndpointId))
                    {
                        mEndpoints[response.RemoteEndpointId].State = EndpointState.ERROR;
                    }

                    break;
            }
        }

        internal void OnAdvertisingResult(AdvertisingResult result)
        {
            mNearbyStatus = "AdvertisingResult: success:" + result.Succeeded +
                            " " + result.LocalEndpointName;
        }

        internal void OnConnectionRequest(ConnectionRequest request)
        {
            if (request.RemoteEndpoint.Name != null)
            {
                mNearbyStatus = "OnConnectionRequest: " + request.RemoteEndpoint.Name;
            }

            EndpointHolder holder = new EndpointHolder();
            holder.Endpoint = request.RemoteEndpoint;
            holder.State = EndpointState.REQUESTED;
            mEndpoints.Remove(request.RemoteEndpoint.EndpointId);
            mEndpoints.Add(request.RemoteEndpoint.EndpointId, holder);
            if (request.Payload != null)
            {
                mMessageLog.Add(mNearbyStatus + ": " + request.Payload);
            }
        }

        internal class EndpointHolder
        {
            private EndpointDetails mEndpoint;
            private EndpointState mState;

            public EndpointDetails Endpoint
            {
                get { return this.mEndpoint; }

                set { this.mEndpoint = value; }
            }

            public EndpointState State
            {
                get { return this.mState; }

                set { this.mState = value; }
            }

            public override string ToString()
            {
                return string.Format("{0}({1})", this.Endpoint.Name, this.State);
            }
        }
    }
}
