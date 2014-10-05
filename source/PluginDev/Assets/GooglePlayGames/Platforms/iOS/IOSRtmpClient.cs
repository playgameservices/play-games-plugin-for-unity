/*
 * Copyright (C) 2014 Google Inc.
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

#if UNITY_IPHONE
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.OurUtils;
using UnityEngine;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace GooglePlayGames.IOS {
    public class IOSRtmpClient : IRealTimeMultiplayerClient {

        private delegate void GPGRealTimeRoomStatusChangedCallback(int newStatus);
        private delegate void GPGRealTimeParticipantListChangedCallback(string jsonifiedList);
        private delegate void GPGRealTimeDataReceivedCallback(string participantId, IntPtr dataBuf, int dataSize, bool isReliable);
        private delegate void GPGRealTimeRoomErrorCallback(string localizedError, int errorCode);







        static RealTimeMultiplayerListener sRtmpListener = null;

        static Array sAllParticipants;

        // whether rtmp is currently active (this becomes true when we begin setup, and
        // remains true while the room is active; when we leave the room, this goes back to
        // false.
        static bool sRtmpActive = false;

        static bool sDeliveredRoomConnected = false;

        // accumulated "approximate progress" of the room setup process
        static float mAccumulatedProgress = 0.0f;

        static Dictionary<string, Participant> mAllParticipants = new Dictionary<string, Participant>();
        static Dictionary<string, Participant> mConnectedParticipants = new Dictionary<string, Participant>();


        [DllImport("__Internal")]
        private static extern void GPGSRtmpCreateWithInviteScreen(int minOpponents, int maxOpponents, int variant, 
                                                      GPGRealTimeRoomStatusChangedCallback roomStatusCallback,
                                                                  GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
                                                                  GPGRealTimeDataReceivedCallback dataReceivedCallback,
                                                                  GPGRealTimeRoomErrorCallback roomErrorCallback);

        [DllImport("__Internal")]
        private static extern void GPGSRtmpCreateQuickGame(int minOpponents, int maxOpponents, int variant, 
                                                      GPGRealTimeRoomStatusChangedCallback roomStatusCallback,
                                                           GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
                                                           GPGRealTimeDataReceivedCallback dataReceivedCallback,
                                                           GPGRealTimeRoomErrorCallback roomErrorCallback);

        [DllImport("__Internal")]
        private static extern void GPGSRtmpGetLocalParticipantId(StringBuilder outbuf, int bufSize);

        [DllImport("__Internal")]
        private static extern void GPGSRtmpSendMessage(bool reliable, byte[] data, int dataLen, bool toEveryone, string participantId);

        [DllImport("__Internal")]
        private static extern void GPGSRtmpLeaveRoom();

		[DllImport("__Internal")]
		private static extern void GPGSRtmpDeclineRoomWithId(string roomId);

		[DllImport("__Internal")]
		private static extern void GPGSRtmpAcceptRoomWithId(string invitationId,     
		                                                    GPGRealTimeRoomStatusChangedCallback roomStatusCallback,
		                                                    GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
		                                                    GPGRealTimeDataReceivedCallback dataReceivedCallback,
		                                                    GPGRealTimeRoomErrorCallback roomErrorCallback);

		[DllImport("__Internal")]
		private static extern void GPGSRtmpShowAllInvitations (GPGRealTimeRoomStatusChangedCallback roomStatusCallback,
		                                                      GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
		                                                      GPGRealTimeDataReceivedCallback dataReceivedCallback,
		                                                      GPGRealTimeRoomErrorCallback roomErrorCallback);


        // Due to limitations of Mono on iOS, callbacks invoked from C have to be static,
        // and have to have this MonoPInvokeCallback annotation.
        [MonoPInvokeCallback(typeof(GPGRealTimeRoomStatusChangedCallback))]
        private static void RtmpRoomStatusChangedCallback(int newStatus) {
            Logger.d("IOSRtmpClient.RtmpRoomStatusChanged, newstate=" + newStatus);
            
            switch (newStatus) {
            case 0:
                Logger.d("Room is inviting");
                mAccumulatedProgress = Math.Min(mAccumulatedProgress, 20.0f);
                sRtmpListener.OnRoomSetupProgress(mAccumulatedProgress);
                break;
            case 1:
                Logger.d("Room is connnecting");
                mAccumulatedProgress = Math.Min(mAccumulatedProgress, 40.0f);
                sRtmpListener.OnRoomSetupProgress(mAccumulatedProgress);
                break;
            case 2:
                Logger.d("Room is automatching");
                mAccumulatedProgress = Math.Min(mAccumulatedProgress, 20.0f);
                sRtmpListener.OnRoomSetupProgress(mAccumulatedProgress);
                break;
            case 3:
                Logger.d("Room is active");
                mAccumulatedProgress = Math.Min(mAccumulatedProgress, 100.0f);
                sRtmpListener.OnRoomSetupProgress(mAccumulatedProgress);
                sRtmpListener.OnRoomConnected(true);
                sDeliveredRoomConnected = true;

                break;
            case 4: // Room was deleted
                Logger.d("Room was deleted. Calling onLeftRoom");
                if (sRtmpListener != null) {
                    sRtmpListener.OnLeftRoom();
                }
                Clear();
                break;
            default:
                break;
            }
            
        }


        private static string[] SubtractParticipants(Dictionary<string, Participant> a, Dictionary<string, Participant> b) {
            List<string> result = new List<string>();
            if (a != null) {
                foreach (string participantId in a.Keys) {
                    result.Add(participantId);
                }
            }
            if (b != null) {
                foreach (string participantId in b.Keys) {
                    if (result.Contains(participantId)) {
                        result.Remove(participantId);
                    }
                }
            }
            return result.ToArray();
        }

        private static Participant.ParticipantStatus convertIntFromiOSToParticipantStatus(System.Int64 iOSStatus) {
            switch (iOSStatus) {
            case 0:
                return Participant.ParticipantStatus.Invited;
            case 1:
                return Participant.ParticipantStatus.Joined;
            case 2:
                return Participant.ParticipantStatus.Declined;
            case 3:
                return Participant.ParticipantStatus.Left;
            case 4:
				return Participant.ParticipantStatus.ConnectionEstablished;
            }
            return Participant.ParticipantStatus.Unknown;
        }


        [MonoPInvokeCallback(typeof(GPGRealTimeParticipantListChangedCallback))]
        private static void RtmpParticipantListChangedCallback(string jsonifiedParticipants) {

            mAllParticipants = new Dictionary<string, Participant>();
            Logger.d("IOSRtmpClient.ParticipantListChanged, JSONString=" + jsonifiedParticipants);

            Dictionary<string, Participant> connectedInThisUpdate = new Dictionary<string, Participant>();
            var myArray = Json.Deserialize(jsonifiedParticipants) as List<object>;
            foreach (Dictionary<string, object> nextData in myArray) {
                Player dummyPlayer = new Player((string)nextData["playerName"], (string)nextData["playerId"]);
                Participant nextParticipant = new Participant((string)nextData["displayName"],
                                                              (string)nextData["participantId"],
                                                              convertIntFromiOSToParticipantStatus((System.Int64)nextData["status"]),
                                                              dummyPlayer,
                                                              (System.Int64)nextData["connectedToRoom"] == 1);
                Debug.Log("Participant " + nextParticipant.DisplayName + " id " + 
                          nextParticipant.ParticipantId + " status " + nextParticipant.Status + "  isConnected " + nextParticipant.IsConnectedToRoom);
                mAllParticipants.Add(nextParticipant.ParticipantId, nextParticipant);
                if (nextParticipant.IsConnectedToRoom) {
                    connectedInThisUpdate.Add(nextParticipant.ParticipantId, nextParticipant);
                }
            }

            string[] newlyConnected;
            string[] newlyDisconnected;

            newlyConnected = SubtractParticipants(connectedInThisUpdate, mConnectedParticipants);
            newlyDisconnected = SubtractParticipants(mConnectedParticipants, connectedInThisUpdate);

            foreach (string newlyConnectedId in newlyConnected) {
                Logger.d(newlyConnectedId + " just connected.");
            }

            foreach (string newlyDisonnectedId in newlyDisconnected) {
                Logger.d(newlyDisonnectedId + " just disconnected.");
            }

            mConnectedParticipants = connectedInThisUpdate;
            Logger.d("UpdateRoom: participant list now has " + mConnectedParticipants.Count + 
                     " participants.");


            if (sDeliveredRoomConnected) {
                if (newlyConnected.Length > 0 && sRtmpListener != null) {
                    Logger.d("UpdateRoom: calling OnPeersConnected callback");
                    sRtmpListener.OnPeersConnected(newlyConnected);
                }
                if (newlyDisconnected.Length > 0 && sRtmpListener != null) {
                    Logger.d("UpdateRoom: calling OnPeersDisconnected callback");
                    sRtmpListener.OnPeersDisconnected(newlyDisconnected);
                }
            }

        }

        [MonoPInvokeCallback(typeof(GPGRealTimeDataReceivedCallback))]
        private static void RtmpDataReceivedCallback(string participantId, IntPtr dataBuf, int dataSize, bool isReliable) {
            string prefix = "IOSRtmpClient.RealTimeDataReceivedCallback ";
            Logger.d(prefix + " dataBuf=" + dataBuf + " dataSize=" + dataSize + " reliable=" + isReliable) ;
            byte[] data = null;
            
            if (dataBuf.ToInt32() != 0) {
                data = new byte[dataSize];
                Marshal.Copy(dataBuf, data, 0, dataSize);
                if (sRtmpListener != null) {
                    sRtmpListener.OnRealTimeMessageReceived(isReliable, participantId, data);
                }
            }
        }

        
        [MonoPInvokeCallback(typeof(GPGRealTimeRoomErrorCallback))]
        private static void RtmpRoomErrorCallback(string localizedError, int errorCode) {
            string prefix = "IOSRtmpClient.RealTimeErrorCallback ";
            Logger.e(prefix + "Error with room " + localizedError);
            Clear();
        }

         

        public System.Collections.Generic.List<Participant> GetConnectedParticipants ()
        {
            Debug.Log("Getting connected participants");
            // TODO: Build this at callback time instead of fetch time?
            List<Participant> connectedParticipants = new List<Participant>();
            foreach (Participant nextParticipant in mAllParticipants.Values) {
                Debug.Log("Looking at participant " + nextParticipant.ParticipantId);
                if (nextParticipant.IsConnectedToRoom) {
                    Debug.Log("Participant is connected to room!");
                    connectedParticipants.Add(nextParticipant);
                }
            }
            connectedParticipants.Sort();
            return connectedParticipants;
        }

        public Participant GetParticipant (string participantId)
        {
            return mAllParticipants[participantId];
        }

        public Participant GetSelf ()
        {
            return GetParticipant(GetLocalParticipantId());
        }

        public string GetLocalParticipantId () {
            Logger.d("IOSRtmpClient.GetLocalParticipantId");
            StringBuilder sb = new StringBuilder(512);
            GPGSRtmpGetLocalParticipantId(sb, sb.Capacity);
            Logger.d("Returned user display name: " + sb.ToString());
            return sb.ToString();
        }

		/**
		 * 
		 * Dealing with invitations
		 * 
		 */
		public void DeclineInvitation (string invitationId)
		{
			Logger.d ("iOSRtmpClient.DeclineInvitation id = " + invitationId);
			GPGSRtmpDeclineRoomWithId (invitationId);
		}

		public void AcceptInvitation (string invitationId, RealTimeMultiplayerListener listener)
		{
			Logger.d(string.Format("iOSRtmpClient.AcceptInvitation, id= " + invitationId));

			if (!PrepareToCreateRoom("CreateWithInvitationScreen", listener)) {
				return;
			}
			GPGSRtmpAcceptRoomWithId(invitationId, RtmpRoomStatusChangedCallback, 
			                     RtmpParticipantListChangedCallback, RtmpDataReceivedCallback, RtmpRoomErrorCallback);
		}

		public void AcceptFromInbox (RealTimeMultiplayerListener listener)
		{
			Logger.d(string.Format("iOSRtmpClient.AcceptFromInbox"));
			if (!PrepareToCreateRoom("CreateWithInvitationScreen", listener)) {
				return;
			}
			GPGSRtmpShowAllInvitations (RtmpRoomStatusChangedCallback, 
			                           RtmpParticipantListChangedCallback, RtmpDataReceivedCallback, RtmpRoomErrorCallback);
		}
		


        /**
         * 
         * Passing data
         * 
         */
        public void SendMessageToAll (bool reliable, byte[] data)
        {
            int dataLen = data.Length;
            GPGSRtmpSendMessage(reliable, data, dataLen, true, null);
        }

        
        public void SendMessageToAll (bool reliable, byte[] data, int offset, int length)
        {
            byte[] dataToSend = Misc.GetSubsetBytes(data, offset, length);
            SendMessageToAll(reliable, dataToSend);
        }
        
        public void SendMessage (bool reliable, string participantId, byte[] data)
        {
            int dataLen = data.Length;
            GPGSRtmpSendMessage(reliable, data, dataLen, false, participantId);
        }
        
        public void SendMessage (bool reliable, string participantId, byte[] data, int offset, int length)
        {
            byte[] dataToSend = Misc.GetSubsetBytes(data, offset, length);
            SendMessage(reliable, participantId, dataToSend);
        }



        /**
         * 
         * Creating rooms
         * 
         */

        public void CreateQuickGame(int minOpponents, int maxOpponents, int variant, RealTimeMultiplayerListener listener)
        {
            Logger.d(string.Format("iOSRtmpClient.CreateQuickGame, opponents = {0}-{1}, variant = {2}",
                                   minOpponents, maxOpponents, variant));
            if (!PrepareToCreateRoom("CreateQuickGame", listener)) {
                return;
            }
            GPGSRtmpCreateQuickGame(minOpponents, maxOpponents, variant, RtmpRoomStatusChangedCallback, 
                                    RtmpParticipantListChangedCallback, RtmpDataReceivedCallback, RtmpRoomErrorCallback);
        }
        

        public void CreateWithInvitationScreen(int minOpponents, int maxOppponents, int variant,
                                        RealTimeMultiplayerListener listener) {
            Logger.d(string.Format("iOSRtmpClient.CreateWithInvitationScreen, " +
                                   "opponents={0}-{1}, variant = {2}", minOpponents, maxOppponents, variant));
            if (!PrepareToCreateRoom("CreateWithInvitationScreen", listener)) {
                return;
            }

            GPGSRtmpCreateWithInviteScreen(minOpponents, maxOppponents, variant, RtmpRoomStatusChangedCallback, 
                                           RtmpParticipantListChangedCallback, RtmpDataReceivedCallback, RtmpRoomErrorCallback);
        }

        // prepares to create a room
        private bool PrepareToCreateRoom(string method, RealTimeMultiplayerListener listener) {
            if (sRtmpActive) {
                Logger.e("Cannot call " + method + " while a real-time game is active.");
                if (listener != null) {
                    Logger.d("Notifying listener of failure to create room.");
                    listener.OnRoomConnected(false);
                }
                return false;
            }
            
            mAccumulatedProgress = 0.0f;
            sRtmpListener = listener;
            sRtmpActive = true;
            return true;        
        }

        
        public bool IsRoomConnected ()
        {
            return sDeliveredRoomConnected;
        }



        public void LeaveRoom ()
        {
            GPGSRtmpLeaveRoom();
            Clear();
        }

        public static void Clear() {

            // call the OnLeftRoom() callback if needed
            if (sDeliveredRoomConnected) {
                Logger.d("iOSRtmpClear: looks like we must call the OnLeftRoom() callback.");
                if (sRtmpListener != null) {
                    Logger.d("Calling OnLeftRoom() callback.");
                    sRtmpListener.OnLeftRoom();
                }
            } else {
                Logger.d("iOSRtmpClear: no need to call OnLeftRoom() callback.");
            }

            sDeliveredRoomConnected = false;
            mConnectedParticipants = null;
            mAllParticipants = null;
            sRtmpListener = null;
            sRtmpActive = false;
            mAccumulatedProgress = 0.0f;
            Logger.d("iOSRtmpClear: RTMP cleared.");


        }

    }
}

#endif

