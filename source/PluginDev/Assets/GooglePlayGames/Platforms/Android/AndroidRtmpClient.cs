/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.Android {
    internal class AndroidRtmpClient : IRealTimeMultiplayerClient {
        AndroidClient mClient = null;
        
        AndroidJavaObject mRoom = null;
        RealTimeMultiplayerListener mRtmpListener = null;
        
        // whether rtmp is currently active (this becomes true when we begin setup, and
        // remains true while the room is active; when we leave the room, this goes back to
        // false.
        bool mRtmpActive = false;
        
        // whether we are invoking an external UI (which might cause us to get stopped)
        // we must know this because we will know to keep the reference to the Rtmp Listener
        // so that when we come back from that UI, we can continue the room setup
        bool mLaunchedExternalActivity = false;
        
        // whether we delivered OnRoomConnected callback; amongst other things, this
        // means that we also have to deliver the OnLeftRoom callback when leaving the room
        bool mDeliveredRoomConnected = false;
        
        // whether we have a pending request to leave the room (this will happen if the
        // developer calls LeaveRoom() when we are in a state where we can't service that
        // request immediately)
        bool mLeaveRoomRequested = false;
        
        // variant requested, 0 for ANY
        int mVariant = 0;
            
        // we use this lock when we access the lists of participants, or mSelf,
        // from either the UI thread or the game thread.
        object mParticipantListsLock = new object();
        List<Participant> mConnectedParticipants = new List<Participant>();
        List<Participant> mAllParticipants = new List<Participant>();
        Participant mSelf = null;
        
        // accumulated "approximate progress" of the room setup process
        float mAccumulatedProgress = 0.0f;
        float mLastReportedProgress = 0.0f;
        
        public AndroidRtmpClient(AndroidClient client) {
            mClient = client;
        }
        
        // called from game thread
        public void CreateQuickGame(int minOpponents, int maxOpponents, int variant,
                                        RealTimeMultiplayerListener listener) {
            Logger.d(string.Format("AndroidRtmpClient.CreateQuickGame, opponents={0}-{1}, " + 
                                   "variant={2}", minOpponents, maxOpponents, variant));
                                   
            if (!PrepareToCreateRoom("CreateQuickGame", listener)) {
                return;
            }
            
            mRtmpListener = listener;
            mVariant = variant;
            mClient.CallClientApi("rtmp create quick game", () => {
                AndroidJavaClass rtmpUtil = JavaUtil.GetClass(JavaConsts.SupportRtmpUtilsClass);
                rtmpUtil.CallStatic("createQuickGame", mClient.GHManager.GetApiClient(),
                                    minOpponents, maxOpponents, variant,
                                    new RoomUpdateProxy(this), new RoomStatusUpdateProxy(this),
                                    new RealTimeMessageReceivedProxy(this));
            }, (bool success) => {
                if (!success) {
                    FailRoomSetup("Failed to create game because GoogleApiClient was disconnected");
                }
            });
        }
        
        // called from game thread
        public void CreateWithInvitationScreen(int minOpponents, int maxOpponents, int variant,
                RealTimeMultiplayerListener listener) {
            Logger.d(string.Format("AndroidRtmpClient.CreateWithInvitationScreen, " + 
                    "opponents={0}-{1}, variant={2}", minOpponents, maxOpponents, variant));
            
            if (!PrepareToCreateRoom("CreateWithInvitationScreen", listener)) {
                return;
            }
            
            mRtmpListener = listener;
            mVariant = variant;
            mClient.CallClientApi("rtmp create with invitation screen", () => {
                AndroidJavaClass klass = JavaUtil.GetClass(
                        JavaConsts.SupportSelectOpponentsHelperActivity);
                mLaunchedExternalActivity = true;
                klass.CallStatic("launch", true, mClient.GetActivity(),
                        new SelectOpponentsProxy(this), Logger.DebugLogEnabled,
                        minOpponents, maxOpponents);
            }, (bool success) => {
                if (!success) {
                    FailRoomSetup("Failed to create game because GoogleApiClient was disconnected");
                }
            });
        }
        
        // called from game thread
        public void AcceptFromInbox(RealTimeMultiplayerListener listener) {
            Logger.d("AndroidRtmpClient.AcceptFromInbox.");
            if (!PrepareToCreateRoom("AcceptFromInbox", listener)) {
                return;
            }
            
            mRtmpListener = listener;
            mClient.CallClientApi("rtmp accept with inbox screen", () => {
                AndroidJavaClass klass = JavaUtil.GetClass(
                    JavaConsts.SupportInvitationInboxHelperActivity);
                mLaunchedExternalActivity = true;
                klass.CallStatic("launch", true, mClient.GetActivity(),
                                 new InvitationInboxProxy(this), Logger.DebugLogEnabled);
            }, (bool success) => {
                if (!success) {
                    FailRoomSetup("Failed to accept from inbox because GoogleApiClient was disconnected");
                }
            });
        }
        
        // called from the game thread
        public void AcceptInvitation(string invitationId, RealTimeMultiplayerListener listener) {
            Logger.d("AndroidRtmpClient.AcceptInvitation " + invitationId);
            if (!PrepareToCreateRoom("AcceptInvitation", listener)) {
                return;
            }
            
            mRtmpListener = listener;
            mClient.ClearInvitationIfFromNotification(invitationId);
            mClient.CallClientApi("rtmp accept invitation", () => {
                Logger.d("Accepting invite via support lib.");
                AndroidJavaClass rtmpUtil = JavaUtil.GetClass(JavaConsts.SupportRtmpUtilsClass);
                rtmpUtil.CallStatic("accept", mClient.GHManager.GetApiClient(), invitationId,
                                    new RoomUpdateProxy(this), new RoomStatusUpdateProxy(this),
                                    new RealTimeMessageReceivedProxy(this));
            }, (bool success) => {
                if (!success) {
                    FailRoomSetup("Failed to accept invitation because GoogleApiClient was disconnected");
                }
            });
        }
        
        // called from game thread
        public void SendMessage(bool reliable, string participantId, byte[] data) {
            SendMessage(reliable, participantId, data, 0, data.Length);
        }
        
        // called from game thread
        public void SendMessageToAll(bool reliable, byte[] data) {
            SendMessage(reliable, null, data, 0, data.Length);
        }
        
        // called from game thread
        public void SendMessageToAll(bool reliable, byte[] data, int offset, int length) {
            SendMessage(reliable, null, data, offset, length);
        }
        
        // called from game thread
        public void SendMessage(bool reliable, string participantId, byte[] data,
                                    int offset, int length) {
            Logger.d(string.Format("AndroidRtmpClient.SendMessage, reliable={0}, " + 
                                   "participantId={1}, data[]={2} bytes, offset={3}, length={4}",
                                   reliable, participantId, data.Length, offset, length));
                                   
            if (!CheckConnectedRoom("SendMessage")) {
                return;
            }
            
            if (mSelf != null && mSelf.ParticipantId.Equals(participantId)) {
                Logger.d("Ignoring request to send message to self, " + participantId);
                return;
            }
            
            // Since we don't yet have API support for buffer/offset/length, convert
            // to a regular byte[] buffer:
            byte[] dataToSend = Misc.GetSubsetBytes(data, offset, length);
            
            if (participantId == null) {
                // this means "send to all"
                List<Participant> participants = GetConnectedParticipants();
                foreach (Participant p in participants) {
                    if (p.ParticipantId != null && !p.Equals(mSelf)) {
                        SendMessage(reliable, p.ParticipantId, dataToSend, 0, dataToSend.Length);
                    }
                }
                return;
            }
            
            mClient.CallClientApi("send message to " + participantId, () => {
                if (mRoom != null) {
                    string roomId = mRoom.Call<string>("getRoomId");
                    if (reliable) {
                        mClient.GHManager.CallGmsApi<int>("games.Games", "RealTimeMultiplayer",
                            "sendReliableMessage", null, dataToSend, roomId, participantId);
                    } else {
                        mClient.GHManager.CallGmsApi<int>("games.Games", "RealTimeMultiplayer",
                            "sendUnreliableMessage", dataToSend, roomId, participantId);
                    }
                } else {
                    Logger.w("Not sending message because real-time room was torn down.");
                }
            }, null);
        }
        
        
        // called from game thread
        public List<Participant> GetConnectedParticipants() {
            Logger.d("AndroidRtmpClient.GetConnectedParticipants");
            
            if (!CheckConnectedRoom("GetConnectedParticipants")) {
                return null;
            }
            
            List<Participant> participants;
            // lock it because this list is assigned to by the UI thread
            lock (mParticipantListsLock) {
                participants = mConnectedParticipants;
            }
            // Note: it's fine to return a reference to our internal list, because
            // we use it as an immutable list. When we get an update, we create a new list to
            // replace it. So the caller may just as well hold on to our copy.
            return participants;
        }
        
        // called from game thread
        public Participant GetParticipant(string id) {
            Logger.d("AndroidRtmpClient.GetParticipant: " + id);
            
            if (!CheckConnectedRoom("GetParticipant")) {
                return null;
            }
            
            List<Participant> allParticipants;
            lock (mParticipantListsLock) {
                allParticipants = mAllParticipants;
            }
            
            if (allParticipants == null) {
                Logger.e("RtmpGetParticipant called without a valid room!");
                return null;
            }
            
            foreach (Participant p in allParticipants) {
                if (p.ParticipantId.Equals(id)) {
                    return p;
                }
            }
            
            Logger.e("Participant not found in room! id: " + id);
            return null;
        }
        
        // called from game thread
        public Participant GetSelf() {
            Logger.d("AndroidRtmpClient.GetSelf");
            
            if (!CheckConnectedRoom("GetSelf")) {
                return null;
            }
            
            Participant self;
            lock (mParticipantListsLock) {
                self = mSelf;
            }
            
            if (self == null) {
                Logger.e("Call to RtmpGetSelf() can only be made when in a room. Returning null.");
            }
            return self;
        }
        
        // called from game thread
        public void LeaveRoom() {
            Logger.d("AndroidRtmpClient.LeaveRoom");
            
            // if we are setting up a room but haven't got the room yet, we can't
            // leave the room now, we have to defer it to a later time when we have the room
            if (mRtmpActive && mRoom == null) {
                Logger.w("AndroidRtmpClient.LeaveRoom: waiting for room; deferring leave request.");
                mLeaveRoomRequested = true;
            } else {
            	mClient.CallClientApi("leave room", () => {
                	Clear("LeaveRoom called");
            	}, null);
            }
        }
        
        // called from UI thread, on onStop
        public void OnStop() {
            // if we launched an external activity (like the "select opponents" UI) as part of
            // the process, we should NOT clear our RTMP state on OnStop because we will get
            // OnStop when that Activity launches.
            if (mLaunchedExternalActivity) {
                Logger.d("OnStop: EXTERNAL ACTIVITY is pending, so not clearing RTMP.");
            } else {
                Clear("leaving room because game is stopping.");
            }
        }
        
        // called from the game thread
        public bool IsRoomConnected() {
            return mRoom != null && mDeliveredRoomConnected;
        }
        
        // called from the game thread
        public void DeclineInvitation(string invitationId) {
            Logger.d("AndroidRtmpClient.DeclineInvitation " + invitationId);
            
            mClient.ClearInvitationIfFromNotification(invitationId);
            mClient.CallClientApi("rtmp decline invitation", () => {
                mClient.GHManager.CallGmsApi("games.Games", "RealTimeMultiplayer",
                        "declineInvitation", invitationId);
            }, (bool success) => {
                if (!success) {
                    Logger.w("Failed to decline invitation. GoogleApiClient was disconnected");
                }
            });
        }
        
               
        // prepares to create a room
        private bool PrepareToCreateRoom(string method, RealTimeMultiplayerListener listener) {
            if (mRtmpActive) {
                Logger.e("Cannot call " + method + " while a real-time game is active.");
                if (listener != null) {
                    Logger.d("Notifying listener of failure to create room.");
                    listener.OnRoomConnected(false);
                }
                return false;
            }
            
            mAccumulatedProgress = 0.0f;
            mLastReportedProgress = 0.0f;
            mRtmpListener = listener;
            mRtmpActive = true;
            return true;        
        }
        
        // checks that the room is connected, and warn otherwise
        private bool CheckConnectedRoom(string method) {
            if (mRoom == null || !mDeliveredRoomConnected) {
                Logger.e("Method " + method + " called without a connected room. " +
                    "You must create or join a room AND wait until you get the " + 
                    "OnRoomConnected(true) callback.");
                return false;
            }
            return true;
        }
        
        // called from UI thread
        private void Clear(string reason) {
            Logger.d("RtmpClear: clearing RTMP (reason: " + reason + ").");
            
            // leave the room, if we have one
            if (mRoom != null) {
                Logger.d("RtmpClear: Room still active, so leaving room.");
                string roomId = mRoom.Call<string>("getRoomId");
                Logger.d("RtmpClear: room id to leave is " + roomId);
                
                // TODO: we are not specifying the callback from this API call to get
                // notified of when we *actually* leave the room. Perhaps we should do that,
                // in order to prevent the case where the developer tries to create a room
                // too soon after leaving the previous room, resulting in errors.
                mClient.GHManager.CallGmsApi("games.Games" , "RealTimeMultiplayer", "leave",
                                      new NoopProxy(JavaConsts.RoomUpdateListenerClass), roomId);
                Logger.d("RtmpClear: left room.");
                mRoom = null;
            } else {
                Logger.d("RtmpClear: no room active.");
            }
            
            // call the OnLeftRoom() callback if needed
            if (mDeliveredRoomConnected) {
                Logger.d("RtmpClear: looks like we must call the OnLeftRoom() callback.");
                RealTimeMultiplayerListener listener = mRtmpListener;
                if (listener != null) {
                    Logger.d("Calling OnLeftRoom() callback.");
                    PlayGamesHelperObject.RunOnGameThread(() => {
                        listener.OnLeftRoom();
                    });
                }
            } else {
                Logger.d("RtmpClear: no need to call OnLeftRoom() callback.");
            }
            
            mLeaveRoomRequested = false;
            mDeliveredRoomConnected = false;
            mRoom = null;
            mConnectedParticipants = null;
            mAllParticipants = null;
            mSelf = null;            
            mRtmpListener = null;
            mVariant = 0;
            mRtmpActive = false;
            mAccumulatedProgress = 0.0f;
            mLastReportedProgress = 0.0f;
            mLaunchedExternalActivity = false;
            Logger.d("RtmpClear: RTMP cleared.");
        }
        
        
        
        private string[] SubtractParticipants(List<Participant> a, List<Participant> b) {
            List<string> result = new List<string>();
            if (a != null) {
                foreach (Participant p in a) {
                    result.Add(p.ParticipantId);
                }
            }
            if (b != null) {
                foreach (Participant p in b) {
                    if (result.Contains(p.ParticipantId)) {
                        result.Remove(p.ParticipantId);
                    }
                }
            }
            return result.ToArray();
        }
        
        // called from UI thread
        private void UpdateRoom() {
            List<AndroidJavaObject> toDispose = new List<AndroidJavaObject>();
            Logger.d("UpdateRoom: Updating our cached data about the room.");
            
            string roomId = mRoom.Call<string>("getRoomId");
            Logger.d("UpdateRoom: room id: " + roomId);
            
            Logger.d("UpdateRoom: querying for my player ID.");
            string playerId = mClient.GHManager.CallGmsApi<string>("games.Games", "Players",
                "getCurrentPlayerId");
            Logger.d("UpdateRoom: my player ID is: " + playerId);
            Logger.d("UpdateRoom: querying for my participant ID in the room.");
            string myPartId = mRoom.Call<string>("getParticipantId", playerId);
            Logger.d("UpdateRoom: my participant ID is: " + myPartId);
            
            AndroidJavaObject participantIds = mRoom.Call<AndroidJavaObject>("getParticipantIds");
            toDispose.Add(participantIds);
            int participantCount = participantIds.Call<int>("size");
            Logger.d("UpdateRoom: # participants: " + participantCount);
            
            List<Participant> connectedParticipants = new List<Participant>();
            List<Participant> allParticipants = new List<Participant>();
            
            mSelf = null;
            for (int i = 0; i < participantCount; i++) {
                Logger.d("UpdateRoom: querying participant #" + i);
                string thisId = participantIds.Call<string>("get", i);
                Logger.d("UpdateRoom: participant #" + i + " has id: " + thisId);
                AndroidJavaObject thisPart = mRoom.Call<AndroidJavaObject>("getParticipant", thisId);
                toDispose.Add(thisPart);
                
                Participant p = JavaUtil.ConvertParticipant(thisPart);
                allParticipants.Add(p);
                
                if (p.ParticipantId.Equals(myPartId)) {
                    Logger.d("Participant is SELF.");
                    mSelf = p;
                }
                
                if (p.IsConnectedToRoom) {
                    connectedParticipants.Add(p);
                }
            }
            
            if (mSelf == null) {
                Logger.e("List of room participants did not include self, " + 
                        " participant id: " + myPartId + ", player id: " + playerId);
                // stopgap:
                mSelf = new Participant("?", myPartId, Participant.ParticipantStatus.Unknown, 
                            new Player("?", playerId), false);
            }
            
            connectedParticipants.Sort();
            allParticipants.Sort();
            
            string[] newlyConnected;
            string[] newlyDisconnected;
            
            // lock the list because it's read by the game thread
            lock (mParticipantListsLock) {
                newlyConnected = SubtractParticipants(connectedParticipants, mConnectedParticipants);
                newlyDisconnected = SubtractParticipants(mConnectedParticipants, connectedParticipants);
            
                // IMPORTANT: we treat mConnectedParticipants as an immutable list; we give
                // away references to it to the callers of our API, so anyone out there might
                // be holding a reference to it and reading it from any thread.
                // This is why, instead of modifying it in place, we assign a NEW list to it.
                mConnectedParticipants = connectedParticipants;
                mAllParticipants = allParticipants;
                Logger.d("UpdateRoom: participant list now has " + mConnectedParticipants.Count + 
                         " participants.");
            }
            
            // cleanup
            Logger.d("UpdateRoom: cleanup.");
            foreach (AndroidJavaObject obj in toDispose) {
                obj.Dispose();
            }
            
            Logger.d("UpdateRoom: newly connected participants: " + newlyConnected.Length);
            Logger.d("UpdateRoom: newly disconnected participants: " + newlyDisconnected.Length);
            
            // only deliver peers connected/disconnected events if have delivered OnRoomConnected
            if (mDeliveredRoomConnected) {
                if (newlyConnected.Length > 0 && mRtmpListener != null) {
                    Logger.d("UpdateRoom: calling OnPeersConnected callback");
                    mRtmpListener.OnPeersConnected(newlyConnected);
                }
                if (newlyDisconnected.Length > 0 && mRtmpListener != null) {
                    Logger.d("UpdateRoom: calling OnPeersDisconnected callback");
                    mRtmpListener.OnPeersDisconnected(newlyDisconnected);
                }
            }
            
            // did the developer request to leave the room?
            if (mLeaveRoomRequested) {
                Clear("deferred leave-room request");
            }
            
            // is it time to report progress during room setup?
            if (!mDeliveredRoomConnected) {
                DeliverRoomSetupProgressUpdate();
            }
        }
        
        // called from UI thread
        private void FailRoomSetup(string reason) {
            Logger.d("Failing room setup: " + reason);
            RealTimeMultiplayerListener listener = mRtmpListener;
            
            Clear("Room setup failed: " + reason);
            
            if (listener != null) {
                Logger.d("Invoking callback OnRoomConnected(false) to signal failure.");
                PlayGamesHelperObject.RunOnGameThread(() => {
                    listener.OnRoomConnected(false);
                });
            }
        }
        
        private bool CheckRtmpActive(string method) {
            if (!mRtmpActive) {
                Logger.d("Got call to " + method + " with RTMP inactive. Ignoring.");
                return false;
            }
            return true;
        }
        
        private void OnJoinedRoom(int statusCode, AndroidJavaObject room) {
            Logger.d("AndroidClient.OnJoinedRoom, status " + statusCode);
            if (!CheckRtmpActive("OnJoinedRoom")) {
                return;
            }
            
            mRoom = room;
            mAccumulatedProgress += 20.0f;
            
            if (statusCode != 0) {
                FailRoomSetup("OnJoinedRoom error code " + statusCode);
            }
        }
        
        private void OnLeftRoom(int statusCode, AndroidJavaObject room) {
            Logger.d("AndroidClient.OnLeftRoom, status " + statusCode);
            if (!CheckRtmpActive("OnLeftRoom")) {
                return;
            }
            
            Clear("Got OnLeftRoom " + statusCode);
        }
        
        private void OnRoomConnected(int statusCode, AndroidJavaObject room) {
            Logger.d("AndroidClient.OnRoomConnected, status " + statusCode);
            
            if (!CheckRtmpActive("OnRoomConnected")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
            if (statusCode != 0) {
                FailRoomSetup("OnRoomConnected error code " + statusCode);
            } else {
                Logger.d("AndroidClient.OnRoomConnected: room setup succeeded!");
                RealTimeMultiplayerListener listener = mRtmpListener;
                if (listener != null) {
                    Logger.d("Invoking callback OnRoomConnected(true) to report success.");
                    PlayGamesHelperObject.RunOnGameThread(() => {
                        mDeliveredRoomConnected = true;
                        listener.OnRoomConnected(true);
                    });
                }
            }
        }
        
        private void OnRoomCreated(int statusCode, AndroidJavaObject room) {
            Logger.d("AndroidClient.OnRoomCreated, status " + statusCode);
            
            if (!CheckRtmpActive("OnRoomCreated")) {
                return;
            }
            
            mRoom = room;
            mAccumulatedProgress += 20.0f;
            
            if (statusCode != 0) {
                FailRoomSetup("OnRoomCreated error code " + statusCode);
            }
            UpdateRoom();
        }
        
        private void OnConnectedToRoom(AndroidJavaObject room) {
            Logger.d("AndroidClient.OnConnectedToRoom");
            
            if (!CheckRtmpActive("OnConnectedToRoom")) {
                return;
            }
            
            mAccumulatedProgress += 10.0f;
            mRoom = room;
            UpdateRoom();
        }
        
        private void OnDisconnectedFromRoom(AndroidJavaObject room) {
            Logger.d("AndroidClient.OnDisconnectedFromRoom");
            
            if (!CheckRtmpActive("OnDisconnectedFromRoom")) {
                return;
            }
            
            Clear("Got OnDisconnectedFromRoom");
        }
        
        private void OnP2PConnected(string participantId) {
            Logger.d("AndroidClient.OnP2PConnected: " + participantId);
            
            if (!CheckRtmpActive("OnP2PConnected")) {
                return;
            }
            
            UpdateRoom();
        }
        
        private void OnP2PDisconnected(string participantId) {
            Logger.d("AndroidClient.OnP2PDisconnected: " + participantId);
            
            if (!CheckRtmpActive("OnP2PDisconnected")) {
                return;
            }
            
            UpdateRoom();
        }
        
        private void OnPeerDeclined(AndroidJavaObject room, AndroidJavaObject participantIds) {
            Logger.d("AndroidClient.OnPeerDeclined");
            
            if (!CheckRtmpActive("OnPeerDeclined")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
            
            // In the current API implementation, if a peer declines, the room will never
            // subsequently get an onRoomConnected, so this match is doomed to failure.
            if (!mDeliveredRoomConnected) {
				FailRoomSetup("OnPeerDeclined received during setup");
            }
        }
        
        private void OnPeerInvitedToRoom(AndroidJavaObject room, AndroidJavaObject participantIds) {
            Logger.d("AndroidClient.OnPeerInvitedToRoom");
            
            if (!CheckRtmpActive("OnPeerInvitedToRoom")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
        }
        
        private void OnPeerJoined(AndroidJavaObject room, AndroidJavaObject participantIds) {
            Logger.d("AndroidClient.OnPeerJoined");
            
            if (!CheckRtmpActive("OnPeerJoined")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
        }
        
        private void OnPeerLeft(AndroidJavaObject room, AndroidJavaObject participantIds) {
            Logger.d("AndroidClient.OnPeerLeft");
            
            if (!CheckRtmpActive("OnPeerLeft")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
            
			// In the current API implementation, if a peer leaves, the room will never
			// subsequently get an onRoomConnected, so this match is doomed to failure.
			if (!mDeliveredRoomConnected) {
				FailRoomSetup("OnPeerLeft received during setup");
			}
        }
        
        private void OnPeersConnected(AndroidJavaObject room, AndroidJavaObject participantIds) {
            Logger.d("AndroidClient.OnPeersConnected");
            
            if (!CheckRtmpActive("OnPeersConnected")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
        }
        
        private void OnPeersDisconnected(AndroidJavaObject room, AndroidJavaObject participantIds) {
            Logger.d("AndroidClient.OnPeersDisconnected.");
            
            if (!CheckRtmpActive("OnPeersDisconnected")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
        }
        
        private void OnRoomAutoMatching(AndroidJavaObject room) {
            Logger.d("AndroidClient.OnRoomAutoMatching");
            
            if (!CheckRtmpActive("OnRoomAutomatching")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
        }
        
        private void OnRoomConnecting(AndroidJavaObject room) {
            Logger.d("AndroidClient.OnRoomConnecting.");
            
            if (!CheckRtmpActive("OnRoomConnecting")) {
                return;
            }
            
            mRoom = room;
            UpdateRoom();
        }
        
        private void OnRealTimeMessageReceived(AndroidJavaObject message) {
            Logger.d("AndroidClient.OnRealTimeMessageReceived.");
            
            if (!CheckRtmpActive("OnRealTimeMessageReceived")) {
                return;
            }
            
            RealTimeMultiplayerListener listener = mRtmpListener;
            if (listener != null) {
                byte[] messageData;
                using (AndroidJavaObject messageBytes = message.Call<AndroidJavaObject>("getMessageData")) {
                    messageData = JavaUtil.ConvertByteArray(messageBytes);
                }
                bool isReliable = message.Call<bool>("isReliable");
                string senderId = message.Call<string>("getSenderParticipantId");
                
                PlayGamesHelperObject.RunOnGameThread(() => {
                    listener.OnRealTimeMessageReceived(isReliable, senderId, messageData);
                });
            }
            message.Dispose();
        }
        
        private void OnSelectOpponentsResult(bool success, AndroidJavaObject opponents, 
                bool hasAutoMatch, AndroidJavaObject autoMatchCriteria) {
            Logger.d("AndroidRtmpClient.OnSelectOpponentsResult, success=" + success);
            
            if (!CheckRtmpActive("OnSelectOpponentsResult")) {
                return;
            }
            
            // we now do not have an external Activity that we launched
            mLaunchedExternalActivity = false;
            
            if (!success) {
                Logger.w("Room setup failed because select-opponents UI failed.");
                FailRoomSetup("Select opponents UI failed.");
                return;
            }
            
            // at this point, we have to create the room -- but we have to make sure that
            // our GoogleApiClient is connected before we do that. It might NOT be connected
            // right now because we just came back from calling an external Activity.
            // So we use CallClientApi to make sure we are only doing this at the right time:
            mClient.CallClientApi("creating room w/ select-opponents result", () => {
                Logger.d("Creating room via support lib's RtmpUtil.");
                AndroidJavaClass rtmpUtil = JavaUtil.GetClass(JavaConsts.SupportRtmpUtilsClass);
                rtmpUtil.CallStatic("create", mClient.GHManager.GetApiClient(),
                                opponents, mVariant, hasAutoMatch ? autoMatchCriteria : null,
                                new RoomUpdateProxy(this), new RoomStatusUpdateProxy(this),
                                new RealTimeMessageReceivedProxy(this));
            }, (bool ok) => {
                if (!ok) {
                    FailRoomSetup("GoogleApiClient lost connection");
                }
            });
        }
        
        private void OnInvitationInboxResult(bool success, string invitationId) {
            Logger.d("AndroidRtmpClient.OnInvitationInboxResult, " + 
                    "success=" + success + ", invitationId=" + invitationId);
                    
            if (!CheckRtmpActive("OnInvitationInboxResult")) {
                return;
            }
                    
            // we now do not have an external Activity that we launched
            mLaunchedExternalActivity = false;
            
            if (!success || invitationId == null || invitationId.Length == 0) {
                Logger.w("Failed to setup room because invitation inbox UI failed.");
                FailRoomSetup("Invitation inbox UI failed.");
                return;
            }
            
            mClient.ClearInvitationIfFromNotification(invitationId);
            
            // we use CallClientApi instead of calling the API directly because we need
            // to make sure that we call it when the GoogleApiClient is connected, which is
            // not necessarily true at this point (we just came back from an external
            // activity)
            mClient.CallClientApi("accept invite from inbox", () => {
                Logger.d("Accepting invite from inbox via support lib.");
                AndroidJavaClass rtmpUtil = JavaUtil.GetClass(JavaConsts.SupportRtmpUtilsClass);
                rtmpUtil.CallStatic("accept", mClient.GHManager.GetApiClient(), invitationId,
                                    new RoomUpdateProxy(this), new RoomStatusUpdateProxy(this),
                                    new RealTimeMessageReceivedProxy(this));
            }, (bool ok) => {
                if (!ok) {
                    FailRoomSetup("GoogleApiClient lost connection.");
                }
            });
        }
        
        private void DeliverRoomSetupProgressUpdate() {
            Logger.d("AndroidRtmpClient: DeliverRoomSetupProgressUpdate");
            if (!mRtmpActive || mRoom == null || mDeliveredRoomConnected) {
                // no need to deliver progress
                return;
            }
            
            float progress = CalcRoomSetupPercentage();
            
            if (progress < mLastReportedProgress) {
                progress = mLastReportedProgress;
            } else {
                mLastReportedProgress = progress;
            }
            
            Logger.d("room setup progress: " + progress + "%");
            if (mRtmpListener != null) {
                Logger.d("Delivering progress to callback.");
                PlayGamesHelperObject.RunOnGameThread(() => {
                    mRtmpListener.OnRoomSetupProgress(progress);
                });
            }
        }
        
        private float CalcRoomSetupPercentage() {
            if (!mRtmpActive || mRoom == null) {
                return 0.0f;
            }
            if (mDeliveredRoomConnected) {
                return 100.0f;
            }
            float progress = mAccumulatedProgress;
            if (progress > 50.0f) {
                progress = 50.0f;
            }
            
            float remaining = 100.0f - progress;
            
            int all = mAllParticipants == null ? 0 : mAllParticipants.Count;
            int connected = mConnectedParticipants == null ? 0 : mConnectedParticipants.Count;
            
            if (all == 0) {
                return progress;
            } else {
                return progress + remaining * ((float)connected / all);
            }
        }
        
        private class RoomUpdateProxy : AndroidJavaProxy {
            AndroidRtmpClient mOwner;
            
            internal RoomUpdateProxy(AndroidRtmpClient owner) 
                    : base(JavaConsts.RoomUpdateListenerClass) {
                mOwner = owner;        
            }
            
            public void onJoinedRoom(int statusCode, AndroidJavaObject room) {
                mOwner.OnJoinedRoom(statusCode, room);
            }
            
            public void onLeftRoom(int statusCode, AndroidJavaObject room) {
                mOwner.OnLeftRoom(statusCode, room);
            }
            
            public void onRoomConnected(int statusCode, AndroidJavaObject room) {
                mOwner.OnRoomConnected(statusCode, room);
            }
            
            public void onRoomCreated(int statusCode, AndroidJavaObject room) {
                mOwner.OnRoomCreated(statusCode, room);
            }
        }
        
        private class RoomStatusUpdateProxy : AndroidJavaProxy {
            AndroidRtmpClient mOwner;
            
            internal RoomStatusUpdateProxy(AndroidRtmpClient owner) 
            : base(JavaConsts.RoomStatusUpdateListenerClass) {
                mOwner = owner;
            }
            
            public void onConnectedToRoom(AndroidJavaObject room) {
                mOwner.OnConnectedToRoom(room);
            }
            
            public void onDisconnectedFromRoom(AndroidJavaObject room) {
                mOwner.OnDisconnectedFromRoom(room);
            }
            
            public void onP2PConnected(string participantId) {
                mOwner.OnP2PConnected(participantId);
            }
            
            public void onP2PDisconnected(string participantId) {
                mOwner.OnP2PDisconnected(participantId);
            }
            
            public void onPeerDeclined(AndroidJavaObject room, AndroidJavaObject participantIds) {
                mOwner.OnPeerDeclined(room, participantIds);
            }
            
            public void onPeerInvitedToRoom(AndroidJavaObject room, AndroidJavaObject participantIds) {
                mOwner.OnPeerInvitedToRoom(room, participantIds);
            }
            
            public void onPeerJoined(AndroidJavaObject room, AndroidJavaObject participantIds) {
                mOwner.OnPeerJoined(room, participantIds);
            }
            
            public void onPeerLeft(AndroidJavaObject room, AndroidJavaObject participantIds) {
                mOwner.OnPeerLeft(room, participantIds);
            }
            
            public void onPeersConnected(AndroidJavaObject room, AndroidJavaObject participantIds) {
                mOwner.OnPeersConnected(room, participantIds);
            }
            
            public void onPeersDisconnected(AndroidJavaObject room, AndroidJavaObject participantIds) {
                mOwner.OnPeersDisconnected(room, participantIds);
            }
            
            public void onRoomAutoMatching(AndroidJavaObject room) {
                mOwner.OnRoomAutoMatching(room);
            }
            
            public void onRoomConnecting(AndroidJavaObject room) {
                mOwner.OnRoomConnecting(room);
            }
        }
        
        private class RealTimeMessageReceivedProxy : AndroidJavaProxy {
            AndroidRtmpClient mOwner;
            
            internal RealTimeMessageReceivedProxy(AndroidRtmpClient owner) 
                    : base(JavaConsts.RealTimeMessageReceivedListenerClass) {
                mOwner = owner;
            }
            
            public void onRealTimeMessageReceived(AndroidJavaObject message) {
                mOwner.OnRealTimeMessageReceived(message);
            }
        }
        
        private class SelectOpponentsProxy : AndroidJavaProxy {
            AndroidRtmpClient mOwner;
            
            internal SelectOpponentsProxy(AndroidRtmpClient owner) 
                    : base(JavaConsts.SupportSelectOpponentsHelperActivityListener) {
                mOwner = owner;
            }
            
            public void onSelectOpponentsResult(bool success, AndroidJavaObject opponents,
                    bool hasAutoMatch, AndroidJavaObject autoMatchCriteria) {
                mOwner.OnSelectOpponentsResult(success, opponents, hasAutoMatch, autoMatchCriteria);
            }
        }
        
        internal void OnSignInSucceeded() {
            // nothing for now
        }
        
        private class InvitationInboxProxy : AndroidJavaProxy {
            AndroidRtmpClient mOwner;
            
            internal InvitationInboxProxy(AndroidRtmpClient owner)
                    : base(JavaConsts.SupportInvitationInboxHelperActivityListener) {
                mOwner = owner;
            }
            
            public void onInvitationInboxResult(bool success, string invitationId) {
                mOwner.OnInvitationInboxResult(success, invitationId);
            }
            
            public void onTurnBasedMatch(AndroidJavaObject match) {
                Logger.e("Bug: RTMP proxy got onTurnBasedMatch(). Shouldn't happen. Ignoring.");
            }
        }
        
        
    }
}

#endif
