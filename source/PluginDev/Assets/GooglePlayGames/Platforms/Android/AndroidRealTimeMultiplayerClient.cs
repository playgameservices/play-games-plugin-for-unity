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
        private static readonly int ROOM_VARIANT_DEFAULT = -1;
        private static readonly int ROOM_STATUS_INVITING = 0;
        private static readonly int ROOM_STATUS_AUTO_MATCHING = 1;
        private static readonly int ROOM_STATUS_CONNECTING = 2;
        private static readonly int ROOM_STATUS_ACTIVE = 3;
        private static readonly int ROOM_STATUS_DELETED = 4;

        private volatile AndroidClient mAndroidClient;
        private volatile AndroidJavaObject mClient;
        private volatile AndroidJavaObject mInvitationsClient;

        private AndroidJavaObject mRoomConfig;
        private AndroidJavaObject mRoom;

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
                    listener.OnRoomConnected(false);
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
                    }
                }
                using(var task = mClient.Call<AndroidJavaObject>("create", mRoomConfig))
                {
                    task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                        e => {
                            listener.OnRoomConnected(false);
                        }
                    ));
                }
            }
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOppponents, uint variant,
                                        RealTimeMultiplayerListener listener)
        {
            // Task<Intent> getSelectOpponentsIntent(@IntRange(from = 1) int minPlayers, @IntRange(from = 1) int maxPlayers, boolean allowAutomatch)
        }

        public void ShowWaitingRoomUI()
        {
            // Task<Intent> getWaitingRoomIntent(@NonNull Room room, @IntRange(from = 0) int minParticipantsToStart)
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
            // Task<Intent> getInvitationInboxIntent()
        }

        public void AcceptInvitation(string invitationId, RealTimeMultiplayerListener listener)
        {
            lock (mSessionLock)
            {
                int roomStatus = GetRoomStatus();
                if (roomStatus == ROOM_STATUS_ACTIVE || roomStatus == ROOM_STATUS_CONNECTING)
                {
                    OurUtils.Logger.e("Received attempt to accept invitation without cleaning up " +
                    "active session.");
                    listener.OnRoomConnected(false);
                    return;
                }

                FindInvitation(invitationId, fail => listener.OnRoomConnected(false),
                    invitation => {
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

                                using (var task = mClient.Call<AndroidJavaObject> ("join", mRoomConfig)) {
                                    task.Call<AndroidJavaObject> ("addOnFailureListener", new TaskOnFailedProxy (
                                        e => listener.OnRoomConnected (false)
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
            return AndroidJavaConverter.ToParticipantList(mRoom.Call<AndroidJavaObject>("getParticipants"));
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
            return null;
        }

        public void LeaveRoom()
        {
            // Task<Void> leave(@NonNull RoomConfig config, @NonNull String roomId)
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
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onPeerDeclined(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onPeerJoined(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onPeerLeft(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onConnectedToRoom(AndroidJavaObject room)
            {
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onDisconnectedFromRoom(AndroidJavaObject room)
            {
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onPeersConnected(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onPeersDisconnected(AndroidJavaObject room, AndroidJavaObject participantIds)
            {
                mParent.mRoom = room;
                // do we need to add something here?
            }

            public void onP2PConnected(string participantId)
            {
                // not sure what to do
            }

            public void onP2PDisconnected(string participantId)
            {
                // not sure what to do
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
                mListener.OnRoomConnected(true);
            }

            public void onJoinedRoom( /* @OnJoinedRoomStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mParent.mRoom = room;
            }

            public void onLeftRoom( /* @OnLeftRoomStatusCodes */ int statusCode, /* @Nullable */ string roomId)
            {
                mListener.OnLeftRoom();
            }

            public void onRoomConnected( /* @OnRoomConnectedStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mListener.OnRoomConnected(true);
            }
        }
    }
}
#endif
