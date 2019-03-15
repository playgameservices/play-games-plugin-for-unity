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
        private AndroidJavaObject mRoomConfig;
        private AndroidJavaObject mRoom;

        public AndroidRealTimeMultiplayerClient(AndroidJavaObject account) 
        {
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games")) 
            {
                mClient = gamesClass.CallStatic<AndroidJavaObject>("getRealTimeMultiplayerClient", AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
                            RealTimeMultiplayerListener listener)
        {
            CreateQuickGame(minOpponents, maxOpponents, variant, /* exclusiveBitMask= */ 0, listener);
        }

        public void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitMask,
            RealTimeMultiplayerListener listener)
        {
            AndroidJavaObject roomUpdateCallback = new AndroidJavaObject("com.google.games.bridge.RoomUpdateCallbackProxy",
                new RoomUpdateCallbackProxy(/* parent= */this, listener));
            AndroidJavaObject realTimeListener = null; //listener
            // build room config
            using (var roomConfigClass = new AndroidJavaClass("com.google.android.gms.games.multiplayer.realtime.RoomConfig"))
            {
                using (var roomConfigBuilder = roomConfigClass.CallStatic<AndroidJavaObject>("builder", roomUpdateCallback))
                {
                    roomConfigBuilder.Call<AndroidJavaObject>("setVariant", (int)variant);
                    roomConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria",
                        roomConfigBuilder.CallStatic<AndroidJavaObject>("createAutoMatchCriteria", (int)minOpponents, (int)maxOpponents, (long)exclusiveBitMask));
                    mRoomConfig = roomConfigBuilder.Call<AndroidJavaObject>("build");
                }
            }
            using (var task = mClient.Call<AndroidJavaObject>("create", mRoomConfig))
            {
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
            // Task<AnnotatedData<InvitationBuffer>> InvitationsClient.loadInvitations()
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

        private class RoomUpdateCallbackProxy : AndroidJavaProxy
        {
            private RealTimeMultiplayerListener mListener;
            private AndroidRealTimeMultiplayerClient mParent;
            
            public RoomUpdateCallbackProxy(AndroidRealTimeMultiplayerClient parent, RealTimeMultiplayerListener listener) 
                : base("com/google/games/bridge/RoomUpdateCallbackProxy$Callback")
            {
                mListener = listener;
                mParent = parent;
            }

            public void onRoomCreated(/* @OnRoomCreatedStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mParent.mRoom = room;
            }

            public void onJoinedRoom(/* @OnJoinedRoomStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mParent.mRoom = room;
            }

            public void onLeftRoom(/* @OnLeftRoomStatusCodes */ int statusCode, /* @Nullable */ string roomId)
            {
                mListener.OnLeftRoom();
            }

            public void onRoomConnected(/* @OnRoomConnectedStatusCodes */ int statusCode, /* @Nullable Room */ AndroidJavaObject room)
            {
                mListener.OnRoomConnected(true);
            }
        }
    }
}
#endif

