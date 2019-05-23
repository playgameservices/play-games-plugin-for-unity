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
        private volatile AndroidJavaObject mClient;
        private volatile AndroidJavaObject mInvitationsClient;
        private AndroidJavaObject mRoomConfig;
        private AndroidJavaObject mRoom;

        public AndroidRealTimeMultiplayerClient(AndroidJavaObject account)
        {
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
            using(var task = mClient.Call<AndroidJavaObject>("create", mRoomConfig)) {
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    e => {
                        listener.OnRoomConnected(false);
                    }
                ));
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
                      using (var matchesResponse = annotatedData.Call<AndroidJavaObject>("get"))
                      {
                        AndroidJavaObject invitationsBuffer = matchesResponse.Call<AndroidJavaObject>("getInvitations");
                        int count = invitationsBuffer.Call<int>("getCount");
                        Invitation[] invitations = new Invitation[count];
                        for (int i=0; i<count; i++) {
                          Invitation invitation = AndroidJavaConverter.ToInvitation(invitationsBuffer.Call<AndroidJavaObject>("get", (int) i));
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
            // Task<Void> join(@NonNull RoomConfig config)
        }

        public void SendMessageToAll(bool reliable, byte[] data)
        {
            // Task<Void> sendUnreliableMessageToOthers(@NonNull byte[] messageData, @NonNull String roomId)
        }

        public void SendMessageToAll(bool reliable, byte[] data, int offset, int length)
        {
            // Task<Void> sendUnreliableMessageToOthers(@NonNull byte[] messageData, @NonNull String roomId)
        }

        public void SendMessage(bool reliable, string participantId, byte[] data)
        {
            // Task<Integer> sendReliableMessage(@NonNull byte[] messageData, @NonNull String roomId, @NonNull String recipientParticipantId, @Nullable ReliableMessageSentCallback callback)
            // Task<Void> sendUnreliableMessage(@NonNull byte[] messageData, @NonNull String roomId, @NonNull String recipientParticipantId)
        }

        public void SendMessage(bool reliable, string participantId, byte[] data, int offset, int length)
        {
            // Task<Integer> sendReliableMessage(@NonNull byte[] messageData, @NonNull String roomId, @NonNull String recipientParticipantId, @Nullable ReliableMessageSentCallback callback)
            // Task<Void> sendUnreliableMessage(@NonNull byte[] messageData, @NonNull String roomId, @NonNull String recipientParticipantId)
        }

        public List<Participant> GetConnectedParticipants()
        {
            return null;
        }

        public Participant GetSelf()
        {
            return null;
        }

        public Participant GetParticipant(string participantId)
        {
            return null;
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
            return false;
        }

        public void DeclineInvitation(string invitationId)
        {
            // Task<Void> declineInvitation(@NonNull String invitationId)
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
