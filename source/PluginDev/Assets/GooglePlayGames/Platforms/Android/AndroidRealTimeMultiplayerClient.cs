#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.OurUtils;
    using UnityEngine;

    internal class AndroidRealTimeMultiplayerClient : IRealTimeMultiplayerClient
    {
        private readonly object mSessionLock = new object();

        private const float InitialPercentComplete = 20.0F;

        private static readonly int ROOM_VARIANT_DEFAULT = -1;
        private static readonly int ROOM_STATUS_INVITING = 0;
        private static readonly int ROOM_STATUS_AUTO_MATCHING = 1;
        private static readonly int ROOM_STATUS_CONNECTING = 2;
        private static readonly int ROOM_STATUS_ACTIVE = 3;
        private static readonly int ROOM_STATUS_DELETED = 4;

        private volatile AndroidClient mAndroidClient;
        private volatile AndroidJavaObject mClient;
        private volatile AndroidJavaObject mInvitationsClient;

        private AndroidJavaObject mRoom;
        private AndroidJavaObject mRoomConfig;
        private RealTimeMultiplayerListener mListener;

        private Invitation mInvitation;

        public AndroidRealTimeMultiplayerClient(AndroidClient androidClient, AndroidJavaObject account)
        {
            mAndroidClient = androidClient;
            using(var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games"))
            {
                mClient = gamesClass.CallStatic<AndroidJavaObject>("getRealTimeMultiplayerClient",
                        AndroidHelperFragment.GetActivity(), account);
                mInvitationsClient = gamesClass.CallStatic<AndroidJavaObject>("getInvitationsClient", AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
            RealTimeMultiplayerListener listener) {
            CreateQuickGame(minOpponents, maxOpponents, variant, /* exclusiveBitMask= */ 0, listener);
        }

        public void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitMask,
            RealTimeMultiplayerListener listener) {
            AndroidJavaObject roomUpdateCallback = new AndroidJavaObject("com.google.games.bridge.RoomUpdateCallbackProxy",
                new RoomUpdateCallbackProxy( /* parent= */ this, listener));

            lock (mSessionLock)
            {
                if (GetRoomStatus() == ROOM_STATUS_ACTIVE)
                {
                    OurUtils.Logger.e("Received attempt to create a new room without cleaning up the old one.");
                    return;
                }
                // build room config
                using(var roomConfigClass = new AndroidJavaClass("com.google.android.gms.games.multiplayer.realtime.RoomConfig")) {
                    using(var roomConfigBuilder = roomConfigClass.CallStatic<AndroidJavaObject>("builder", roomUpdateCallback)) {
                        if(variant > 0) {
                            roomConfigBuilder.Call<AndroidJavaObject>("setVariant",(int) variant);
                        }
                        roomConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria",
                            roomConfigClass.CallStatic<AndroidJavaObject>("createAutoMatchCriteria",(int) minOpponents,(int) maxOpponents,(long) exclusiveBitMask));

                        AndroidJavaObject messageReceivedListener = new AndroidJavaObject("com.google.games.bridge.RealTimeMessageReceivedListenerProxy", new MessageReceivedListenerProxy(listener));
                        roomConfigBuilder.Call<AndroidJavaObject>("setOnMessageReceivedListener", messageReceivedListener);

                        AndroidJavaObject roomStatusUpdateCallback = new AndroidJavaObject("com.google.games.bridge.RoomStatusUpdateCallbackProxy", new RoomStatusUpdateCallbackProxy(this, listener));
                        roomConfigBuilder.Call<AndroidJavaObject>("setRoomStatusUpdateCallback", roomStatusUpdateCallback);

                        mRoomConfig = roomConfigBuilder.Call<AndroidJavaObject>("build");
                        mListener = listener;
                    }
                }
                using(var task = mClient.Call<AndroidJavaObject>("create", mRoomConfig))
                {
                    task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                        e => {
                            listener.OnRoomConnected(false);
                            CleanSession();
                        }
                    ));
                }
            }
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
                                        RealTimeMultiplayerListener listener)
        {
            AndroidHelperFragment.InvitePlayerUI(minOpponents, maxOpponents, /* realTime= */ true, (status, result) =>
            {
                if (status != UIStatus.Valid)
                {
                    listener.OnRoomConnected(false);
                    CleanSession();
                    return;
                }

                lock (mSessionLock)
                {
                    using (var roomConfigClass = new AndroidJavaClass ("com.google.android.gms.games.multiplayer.realtime.RoomConfig"))
                    {
                        AndroidJavaObject roomUpdateCallback = new AndroidJavaObject ("com.google.games.bridge.RoomUpdateCallbackProxy",
                                new RoomUpdateCallbackProxy ( /* parent= */ this, listener));

                        using (var roomConfigBuilder = roomConfigClass.CallStatic<AndroidJavaObject> ("builder", roomUpdateCallback)) {

                            if (result.MinAutomatchingPlayers > 0)
                            {
                                var autoMatchCriteria = roomConfigClass.CallStatic<AndroidJavaObject>("createAutoMatchCriteria", result.MinAutomatchingPlayers, result.MaxAutomatchingPlayers, /* exclusiveBitMask= */ (long) 0);
                                roomConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria", autoMatchCriteria);
                            }

                            if (variant != 0)
                            {
                                roomConfigBuilder.Call<AndroidJavaObject>("setVariant", (int) variant);
                            }

                            AndroidJavaObject messageReceivedListener = new AndroidJavaObject ("com.google.games.bridge.RealTimeMessageReceivedListenerProxy", new MessageReceivedListenerProxy (listener));
                            roomConfigBuilder.Call<AndroidJavaObject> ("setOnMessageReceivedListener", messageReceivedListener);

                            AndroidJavaObject roomStatusUpdateCallback = new AndroidJavaObject ("com.google.games.bridge.RoomStatusUpdateCallbackProxy", new RoomStatusUpdateCallbackProxy (this, listener));
                            roomConfigBuilder.Call<AndroidJavaObject> ("setRoomStatusUpdateCallback", roomStatusUpdateCallback);

                            roomConfigBuilder.Call<AndroidJavaObject>("addPlayersToInvite", AndroidJavaConverter.ToJavaStringList(result.PlayerIdsToInvite));
                            mRoomConfig = roomConfigBuilder.Call<AndroidJavaObject>("build");
                            mListener = listener;
                        }
                    }

                    using (var task = mClient.Call<AndroidJavaObject>("create", mRoomConfig))
                    {
                        task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                            exception =>
                            {
                                listener.OnRoomConnected(false);
                                CleanSession();
                            }
                        ));
                    }
                }
            });
        }

        private int GetMinParticipantsToStart()
        {
            int minParticipantsToStart = mRoom.Call<AndroidJavaObject>("getParticipants").Call<int>("size");

            AndroidJavaObject autoMatchCriteria = mRoom.Call<AndroidJavaObject>("getAutoMatchCriteria");
            if (autoMatchCriteria != null)
            {
                minParticipantsToStart = minParticipantsToStart + autoMatchCriteria.Call<int>("getInt", "min_automatch_players", 0);
            }

            if (mInvitation != null)
            {
                minParticipantsToStart = minParticipantsToStart + 1;
            }

            return minParticipantsToStart;
        }

        private float GetPercentComplete()
        {
            int connectedCount = GetConnectedParticipants().Count;
            float percentPerParticipant = (100.0F - InitialPercentComplete) / GetMinParticipantsToStart();
            return InitialPercentComplete + connectedCount * percentPerParticipant;
        }

        public void ShowWaitingRoomUI()
        {
            if (mRoom == null)
            {
                return;
            }

            AndroidHelperFragment.ShowWaitingRoomUI(mRoom, GetMinParticipantsToStart(), (response, room) => {

                if (response == AndroidHelperFragment.WaitingRoomUIStatus.Valid)
                {
                    if (GetRoomStatus() == ROOM_STATUS_ACTIVE)
                    {
                        mListener.OnRoomConnected(true);
                    }
                }
                else if (response == AndroidHelperFragment.WaitingRoomUIStatus.LeftRoom)
                {
                    LeaveRoom();
                }
                else
                {
                    mListener.OnRoomSetupProgress(GetPercentComplete());
                }
            });
        }

        public void GetAllInvitations(Action<Invitation[]> callback)
        {
            callback = ToOnGameThread(callback);
            using (var task = mInvitationsClient.Call<AndroidJavaObject>("loadInvitations"))
            {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    annotatedData => {
                      using (var invitationBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                      {
                        int count = invitationBuffer.Call<int>("getCount");
                        Invitation[] invitations = new Invitation[count];
                        for (int i=0; i<count; i++) {
                          Invitation invitation = AndroidJavaConverter.ToInvitation(invitationBuffer.Call<AndroidJavaObject>("get", i));
                          invitations[i] = invitation;
                        }
                        callback(invitations);
                      }
                    }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(null);
                    }
                ));
            }
        }

        private static Action<T> ToOnGameThread<T>(Action<T> toConvert)
        {
            return (val) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val));
        }

        public void AcceptFromInbox(RealTimeMultiplayerListener listener)
        {
            AndroidHelperFragment.ShowInvitationInboxUI((status, invitation) => {
                if (status != UIStatus.Valid)
                {
                    OurUtils.Logger.d("User did not complete invitation screen.");
                    listener.OnRoomConnected(false);
                    CleanSession();
                    return;
                }

                mInvitation = invitation;

                AcceptInvitation(mInvitation.InvitationId, listener);
            });
        }

        public void AcceptInvitation(string invitationId, RealTimeMultiplayerListener listener)
        {
            lock (mSessionLock)
            {
                int roomStatus = GetRoomStatus();
                if (roomStatus == ROOM_STATUS_ACTIVE)
                {
                    OurUtils.Logger.e("Received attempt to accept invitation without cleaning up " +
                    "active session.");
                    listener.OnRoomConnected(false);
                    CleanSession();
                    return;
                }

                FindInvitation(invitationId, fail => listener.OnRoomConnected(false),
                    invitation => {

                        mInvitation = invitation;
                        // build room config
                        using (var roomConfigClass = new AndroidJavaClass ("com.google.android.gms.games.multiplayer.realtime.RoomConfig"))
                        {
                            AndroidJavaObject roomUpdateCallback = new AndroidJavaObject ("com.google.games.bridge.RoomUpdateCallbackProxy",
                                    new RoomUpdateCallbackProxy ( /* parent= */ this, listener));

                            using (var roomConfigBuilder = roomConfigClass.CallStatic<AndroidJavaObject> ("builder", roomUpdateCallback)) {

                                AndroidJavaObject messageReceivedListener = new AndroidJavaObject ("com.google.games.bridge.RealTimeMessageReceivedListenerProxy", new MessageReceivedListenerProxy (listener));
                                roomConfigBuilder.Call<AndroidJavaObject> ("setOnMessageReceivedListener", messageReceivedListener);

                                AndroidJavaObject roomStatusUpdateCallback = new AndroidJavaObject ("com.google.games.bridge.RoomStatusUpdateCallbackProxy", new RoomStatusUpdateCallbackProxy (this, listener));
                                roomConfigBuilder.Call<AndroidJavaObject> ("setRoomStatusUpdateCallback", roomStatusUpdateCallback);

                                roomConfigBuilder.Call<AndroidJavaObject> ("setInvitationIdToAccept", invitationId);

                                mRoomConfig = roomConfigBuilder.Call<AndroidJavaObject> ("build");
                                mListener = listener;

                                using (var task = mClient.Call<AndroidJavaObject> ("join", mRoomConfig)) {
                                    task.Call<AndroidJavaObject> ("addOnFailureListener", new TaskOnFailedProxy (
                                        e => {
                                            listener.OnRoomConnected (false);
                                            CleanSession();
                                        }
                                    ));
                                }
                            }
                        }
                });
            }
        }

        public void SendMessageToAll(bool reliable, byte[] data)
        {
            if (reliable)
            {
                List<Participant> participants = AndroidJavaConverter.ToParticipantList(mRoom.Call<AndroidJavaObject>("getParticipants"));
                foreach (Participant participant in participants)
                {
                    SendMessage(true, participant.ParticipantId, data);
                }
                return;
            }

            int roomStatus = GetRoomStatus();
            if (roomStatus != ROOM_STATUS_ACTIVE && roomStatus != ROOM_STATUS_CONNECTING)
            {
                OurUtils.Logger.d("Sending message is not allowed in this state.");
                return;
            }

            string roomId = mRoom.Call<string>("getRoomId");
            mClient.Call<AndroidJavaObject>("sendUnreliableMessageToOthers", data, roomId);
        }

        public void SendMessageToAll(bool reliable, byte[] data, int offset, int length)
        {
            SendMessageToAll(reliable, Misc.GetSubsetBytes(data, offset, length));
        }

        public void SendMessage(bool reliable, string participantId, byte[] data)
        {
            int roomStatus = GetRoomStatus();
            if (roomStatus != ROOM_STATUS_ACTIVE && roomStatus != ROOM_STATUS_CONNECTING)
            {
                OurUtils.Logger.d("Sending message is not allowed in this state.");
                return;
            }

            if (GetParticipant(participantId) == null)
            {
                OurUtils.Logger.e("Attempted to send message to unknown participant " + participantId);
                return;
            }

            string roomId = mRoom.Call<string>("getRoomId");
            if (reliable)
            {
                mClient.Call<AndroidJavaObject>("sendReliableMessage", data, roomId, participantId, new AndroidJavaObject("com.google.games.bridge.ReliableMessageSentCallbackProxy", new ReliableMessageSentCallbackProxy(this)));
            }
            else
            {
                mClient.Call<AndroidJavaObject>("sendUnreliableMessage", data, roomId, participantId);
            }
        }

        public void SendMessage(bool reliable, string participantId, byte[] data, int offset, int length)
        {
            SendMessage(reliable, participantId, Misc.GetSubsetBytes(data, offset, length));
        }

        public List<Participant> GetConnectedParticipants()
        {
            int roomStatus = GetRoomStatus();
            if (roomStatus != ROOM_STATUS_ACTIVE && roomStatus != ROOM_STATUS_CONNECTING)
            {
                return new List<Participant>();
            }

            List<Participant> result = new List<Participant>();
            foreach (Participant participant in AndroidJavaConverter.ToParticipantList(mRoom.Call<AndroidJavaObject>("getParticipants")))
            {
                if (participant.IsConnectedToRoom)
                {
                    result.Add(participant);
                }
            }

            return result;
        }

        public Participant GetSelf()
        {
            if (GetRoomStatus() != ROOM_STATUS_ACTIVE)
            {
                return null;
            }
            return GetParticipant(mAndroidClient.GetUserId());
        }

        public Participant GetParticipant(string participantId)
        {
            if (GetRoomStatus() != ROOM_STATUS_ACTIVE)
            {
                return null;
            }
            AndroidJavaObject participant = mRoom.Call<AndroidJavaObject>("getParticipant", participantId);
            return AndroidJavaConverter.ToParticipant(participant);
        }

        public Invitation GetInvitation()
        {
            return mInvitation;
        }

        public void LeaveRoom()
        {
            mInvitation = null;
            if (GetRoomStatus() == ROOM_STATUS_ACTIVE)
            {
                mClient.Call<AndroidJavaObject> ("leave", mRoomConfig, mRoom.Call<String>("getRoomId"));
                return;
            }

            if (mListener != null)
            {
                mListener.OnRoomConnected(false);
                CleanSession();
            }
        }

        public bool IsRoomConnected()
        {
            return GetRoomStatus() == ROOM_STATUS_ACTIVE;
        }

        private int GetRoomStatus()
        {
            return mRoom != null ? mRoom.Call<int>("getStatus") : ROOM_VARIANT_DEFAULT;
        }

        public void DeclineInvitation(string invitationId)
        {
            FindInvitation(invitationId, fail => {}, invitation =>
            {
                mClient.Call<AndroidJavaObject>("declineInvitation", invitationId);
            });
        }

        private void FindInvitation(string invitationId, Action<bool> fail, Action<Invitation> callback)
        {
            using (var task = mInvitationsClient.Call<AndroidJavaObject>("loadInvitations"))
            {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    annotatedData =>
                    {
                        using (var invitationBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            int count = invitationBuffer.Call<int>("getCount");
                            for (int i=0; i<count; i++)
                            {
                                Invitation invitation = AndroidJavaConverter.ToInvitation(invitationBuffer.Call<AndroidJavaObject>("get", i));
                                if (invitation.InvitationId == invitationId)
                                {
                                    callback(invitation);
                                    return;
                                }
                            }
                            OurUtils.Logger.e("Invitation with ID " + invitationId + " couldn't be found");
                            fail(true);
                        }
                    }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception =>
                    {
                        OurUtils.Logger.e("Couldn't load invitations.");
                        fail(true);
                    }
                ));
            }
        }

        private class ReliableMessageSentCallbackProxy: AndroidJavaProxy
        {
            private AndroidRealTimeMultiplayerClient mParent;

            public ReliableMessageSentCallbackProxy(AndroidRealTimeMultiplayerClient parent) : base("com/google/games/bridge/ReliableMessageSentCallbackProxy$Callback")
            {
                mParent = parent;
            }

            public void onRealTimeMessageSent(int statusCode, int tokenId, string recipientParticipantId)
            {

            }
        }

        private class RoomStatusUpdateCallbackProxy : AndroidJavaProxy
        {
            private RealTimeMultiplayerListener mListener;
            private AndroidRealTimeMultiplayerClient mParent;

            public RoomStatusUpdateCallbackProxy(AndroidRealTimeMultiplayerClient parent, RealTimeMultiplayerListener listener) : base("com/google/games/bridge/RoomStatusUpdateCallbackProxy$Callback")
            {
                mListener = listener;
                mParent = parent;
            }

            public void onRoomConnecting(AndroidJavaObject room)
            {
                mParent.mRoom = room;
            }

            public void onRoomAutoMatching(AndroidJavaObject room)
            {
                mParent.mRoom = room;
            }

            public void onPeerInvitedToRoom(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                handleParticipantStatusChanged(room, participantIds);
            }

            public void onPeerDeclined(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                handleParticipantStatusChanged(room, participantIds);
            }

            public void onPeerJoined(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                handleParticipantStatusChanged(room, participantIds);
            }

            public void onPeerLeft(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                handleParticipantStatusChanged(room, participantIds);
            }

            private void handleParticipantStatusChanged(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                mParent.mRoom = room;
                int size = participantIds.Get<int>("size");
                for (int i=0; i<size; i++)
                {
                    String participantId = participantIds.Call<String>("get", i);
                    Participant participant = AndroidJavaConverter.ToParticipant(mParent.mRoom.Call<AndroidJavaObject>("getParticipant", participantId));

                    HashSet<Participant.ParticipantStatus> failedStatus = new HashSet<Participant.ParticipantStatus>
                            {
                                Participant.ParticipantStatus.Declined,
                                Participant.ParticipantStatus.Left
                            };
                    if (!failedStatus.Contains(participant.Status))
                    {
                        continue;
                    }

                    mListener.OnParticipantLeft(participant);

                    if (mParent.GetRoomStatus() != ROOM_STATUS_CONNECTING && mParent.GetRoomStatus() != ROOM_STATUS_AUTO_MATCHING)
                    {
                        mParent.LeaveRoom();
                    }
                }
            }

            public void onConnectedToRoom(AndroidJavaObject room)
            {
                if (mParent.GetRoomStatus() == ROOM_STATUS_ACTIVE)
                {
                    mParent.mRoom = room;
                }
                else
                {
                    handleConnectedSetChanged(room);
                }
            }

            public void onDisconnectedFromRoom(AndroidJavaObject room)
            {
                if (mParent.GetRoomStatus() == ROOM_STATUS_ACTIVE)
                {
                    mParent.mRoom = room;
                }
                else
                {
                    handleConnectedSetChanged(room);
                    mParent.CleanSession();
                }
            }

            public void onPeersConnected(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                if (mParent.GetRoomStatus() == ROOM_STATUS_ACTIVE)
                {
                    mParent.mRoom = room;
                    mParent.mListener.OnPeersConnected(AndroidJavaConverter.ToStringList(participantIds).ToArray());
                }
                else
                {
                    handleConnectedSetChanged(room);
                }
            }

            public void onPeersDisconnected(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                if (mParent.GetRoomStatus() == ROOM_STATUS_ACTIVE)
                {
                    mParent.mRoom = room;
                    mParent.mListener.OnPeersDisconnected(AndroidJavaConverter.ToStringList(participantIds).ToArray());
                }
                else
                {
                    handleConnectedSetChanged(room);
                }
            }

            private void handleConnectedSetChanged(AndroidJavaObject room)
            {
                HashSet<string> oldConnectedSet = new HashSet<string>();
                foreach(Participant participant in mParent.GetConnectedParticipants())
                {
                    oldConnectedSet.Add(participant.ParticipantId);
                }

                mParent.mRoom = room;

                HashSet<string> connectedSet = new HashSet<string>();
                foreach(Participant participant in mParent.GetConnectedParticipants())
                {
                    connectedSet.Add(participant.ParticipantId);
                }

                // If the connected set hasn't actually changed, bail out.
                if (oldConnectedSet.Equals(connectedSet))
                {
                    OurUtils.Logger.w("Received connected set callback with unchanged connected set!");
                    return;
                }

                List<string> noLongerConnected = new List<string>();
                foreach(string id in oldConnectedSet)
                {
                    if(!connectedSet.Contains(id))
                    {
                        noLongerConnected.Add(id);
                    }
                }

                if (mParent.GetRoomStatus() == ROOM_STATUS_DELETED)
                {
                    OurUtils.Logger.e("Participants disconnected during room setup, failing. " + "Participants were: " + string.Join(",", noLongerConnected.ToArray()));
                    mParent.mListener.OnRoomConnected(false);
                    mParent.CleanSession();
                    return;
                }

                mParent.mListener.OnRoomSetupProgress(mParent.GetPercentComplete());
            }

            public void onP2PConnected(string participantId)
            {
            }

            public void onP2PDisconnected(string participantId)
            {
            }
        }

        private class MessageReceivedListenerProxy : AndroidJavaProxy
        {
            private RealTimeMultiplayerListener mListener;

            public MessageReceivedListenerProxy(RealTimeMultiplayerListener listener) : base("com/google/games/bridge/RealTimeMessageReceivedListenerProxy$Callback")
            {
                mListener = listener;
            }

            public void onRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
            {
                mListener.OnRealTimeMessageReceived(isReliable, senderId, data);
            }
        }

        private class RoomUpdateCallbackProxy : AndroidJavaProxy
        {
            private RealTimeMultiplayerListener mListener;
            private AndroidRealTimeMultiplayerClient mParent;

            public RoomUpdateCallbackProxy(AndroidRealTimeMultiplayerClient parent, RealTimeMultiplayerListener listener) : base("com/google/games/bridge/RoomUpdateCallbackProxy$Callback")
            {
                mListener = listener;
                mParent = parent;
            }

            public void onRoomCreated( /* @OnRoomCreatedStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mParent.mRoom = room;
                mListener.OnRoomSetupProgress(mParent.GetPercentComplete());
            }

            public void onJoinedRoom( /* @OnJoinedRoomStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mParent.mRoom = room;
            }

            public void onLeftRoom( /* @OnLeftRoomStatusCodes */ int statusCode, /* @Nullable */ string roomId)
            {
                mListener.OnLeftRoom();
                mParent.CleanSession();
            }

            public void onRoomConnected( /* @OnRoomConnectedStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mListener.OnRoomConnected(true);
            }
        }

        private void CleanSession()
        {
            mRoom = null;
            mRoomConfig = null;
            mListener = null;
            mInvitation = null;
        }
    }
}
#endif
