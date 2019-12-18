#if UNITY_ANDROID
#pragma warning disable 0168 // The variable 'var' is declared but never used
#pragma warning disable 0642 // Possible mistaken empty statement

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

        private int mMinPlayersToStart = 0;

        private enum RoomStatus
        {
            NotCreated = -1, // to handle null room case.
            Inviting = 0, // com.google.android.gms.games.multiplayer.realtime.ROOM_STATUS_INVITING
            AutoMatching = 1, // com.google.android.gms.games.multiplayer.realtime.ROOM_STATUS_AUTO_MATCHING
            Connecting = 2, // com.google.android.gms.games.multiplayer.realtime.ROOM_STATUS_CONNECTING
            Active = 3, // com.google.android.gms.games.multiplayer.realtime.ROOM_STATUS_ACTIVE
            Deleted = 4, // com.google.android.gms.games.multiplayer.realtime.ROOM_STATUS_DELETED
        };

        private volatile AndroidClient mAndroidClient;
        private volatile AndroidJavaObject mRtmpClient;
        private volatile AndroidJavaObject mInvitationsClient;

        private AndroidJavaObject mRoom;
        private AndroidJavaObject mRoomConfig;
        private OnGameThreadForwardingListener mListener;
        private Invitation mInvitation;

        public AndroidRealTimeMultiplayerClient(AndroidClient androidClient, AndroidJavaObject account)
        {
            mAndroidClient = androidClient;
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games"))
            {
                mRtmpClient = gamesClass.CallStatic<AndroidJavaObject>("getRealTimeMultiplayerClient",
                    AndroidHelperFragment.GetActivity(), account);
                mInvitationsClient = gamesClass.CallStatic<AndroidJavaObject>("getInvitationsClient",
                    AndroidHelperFragment.GetActivity(), account);
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
            var listenerOnGameThread = new OnGameThreadForwardingListener(listener);
            lock (mSessionLock)
            {
                if (GetRoomStatus() == RoomStatus.Active)
                {
                    OurUtils.Logger.e("Received attempt to create a new room without cleaning up the old one.");
                    listenerOnGameThread.OnRoomConnected(false);
                    return;
                }

                // build room config
                using (var roomConfigClass =
                    new AndroidJavaClass("com.google.android.gms.games.multiplayer.realtime.RoomConfig"))
                using (var roomUpdateCallback = new AndroidJavaObject(
                    "com.google.games.bridge.RoomUpdateCallbackProxy",
                    new RoomUpdateCallbackProxy( /* parent= */ this, listenerOnGameThread)))
                using (var roomConfigBuilder =
                    roomConfigClass.CallStatic<AndroidJavaObject>("builder", roomUpdateCallback))
                {
                    if (variant > 0)
                    {
                        roomConfigBuilder.Call<AndroidJavaObject>("setVariant", (int) variant);
                    }

                    using (var autoMatchCriteria = roomConfigClass.CallStatic<AndroidJavaObject>(
                        "createAutoMatchCriteria", (int) minOpponents,
                        (int) maxOpponents, (long) exclusiveBitMask))
                    using (roomConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria", autoMatchCriteria))
                        ;

                    using (var messageReceivedListener = new AndroidJavaObject(
                        "com.google.games.bridge.RealTimeMessageReceivedListenerProxy",
                        new MessageReceivedListenerProxy(listenerOnGameThread)))
                    using (roomConfigBuilder.Call<AndroidJavaObject>("setOnMessageReceivedListener",
                        messageReceivedListener))
                        ;

                    using (var roomStatusUpdateCallback = new AndroidJavaObject(
                        "com.google.games.bridge.RoomStatusUpdateCallbackProxy",
                        new RoomStatusUpdateCallbackProxy(this, listenerOnGameThread)))
                    using (roomConfigBuilder.Call<AndroidJavaObject>("setRoomStatusUpdateCallback",
                        roomStatusUpdateCallback))
                        ;

                    mRoomConfig = roomConfigBuilder.Call<AndroidJavaObject>("build");
                    mListener = listenerOnGameThread;
                }

                mMinPlayersToStart = (int) minOpponents + 1;

                using (var task = mRtmpClient.Call<AndroidJavaObject>("create", mRoomConfig))
                {
                    AndroidTaskUtils.AddOnFailureListener(
                        task,
                        e =>
                        {
                            listenerOnGameThread.OnRoomConnected(false);
                            CleanSession();
                        });
                }
            }
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            RealTimeMultiplayerListener listener)
        {
            var listenerOnGameThread = new OnGameThreadForwardingListener(listener);
            lock (mSessionLock)
            {
                if (GetRoomStatus() == RoomStatus.Active)
                {
                    OurUtils.Logger.e("Received attempt to create a new room without cleaning up the old one.");
                    listenerOnGameThread.OnRoomConnected(false);
                    return;
                }

                AndroidHelperFragment.ShowRtmpSelectOpponentsUI(minOpponents, maxOpponents,
                    (status, result) =>
                    {
                        if (status == UIStatus.NotAuthorized)
                        {
                            mAndroidClient.SignOut((() =>
                            {
                                listenerOnGameThread.OnRoomConnected(false);
                                CleanSession();
                            }));
                            return;
                        }

                        if (status != UIStatus.Valid)
                        {
                            listenerOnGameThread.OnRoomConnected(false);
                            CleanSession();
                            return;
                        }

                        using (var roomConfigClass =
                            new AndroidJavaClass("com.google.android.gms.games.multiplayer.realtime.RoomConfig"))
                        using (var roomUpdateCallback = new AndroidJavaObject(
                            "com.google.games.bridge.RoomUpdateCallbackProxy",
                            new RoomUpdateCallbackProxy( /* parent= */ this, listenerOnGameThread)))
                        using (var roomConfigBuilder =
                            roomConfigClass.CallStatic<AndroidJavaObject>("builder", roomUpdateCallback))
                        {
                            if (result.MinAutomatchingPlayers > 0)
                            {
                                using (var autoMatchCriteria = roomConfigClass.CallStatic<AndroidJavaObject>(
                                    "createAutoMatchCriteria", result.MinAutomatchingPlayers,
                                    result.MaxAutomatchingPlayers, /* exclusiveBitMask= */ (long) 0))
                                using (roomConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria",
                                    autoMatchCriteria))
                                    ;
                            }

                            if (variant != 0)
                            {
                                using (roomConfigBuilder.Call<AndroidJavaObject>("setVariant", (int) variant)) ;
                            }

                            using (var messageReceivedListener =
                                new AndroidJavaObject(
                                    "com.google.games.bridge.RealTimeMessageReceivedListenerProxy",
                                    new MessageReceivedListenerProxy(listenerOnGameThread)))
                            using (roomConfigBuilder.Call<AndroidJavaObject>("setOnMessageReceivedListener",
                                messageReceivedListener))
                                ;

                            using (var roomStatusUpdateCallback =
                                new AndroidJavaObject("com.google.games.bridge.RoomStatusUpdateCallbackProxy",
                                    new RoomStatusUpdateCallbackProxy(this, listenerOnGameThread)))
                            using (roomConfigBuilder.Call<AndroidJavaObject>("setRoomStatusUpdateCallback",
                                roomStatusUpdateCallback))
                                ;

                            using (roomConfigBuilder.Call<AndroidJavaObject>("addPlayersToInvite",
                                AndroidJavaConverter.ToJavaStringList(result.PlayerIdsToInvite)))
                            {
                                mRoomConfig = roomConfigBuilder.Call<AndroidJavaObject>("build");
                            }

                            mListener = listenerOnGameThread;
                        }

                        // the min number to start is the number of automatched + the number of named invitations +
                        // the local player.
                        mMinPlayersToStart = result.MinAutomatchingPlayers + result.PlayerIdsToInvite.Count + 1;

                        using (var task = mRtmpClient.Call<AndroidJavaObject>("create", mRoomConfig))
                        {
                            AndroidTaskUtils.AddOnFailureListener(
                                task,
                                exception =>
                                {
                                    listenerOnGameThread.OnRoomConnected(false);
                                    CleanSession();
                                });
                        }
                    });
            }
        }

        private float GetPercentComplete()
        {
            // For a player creating a room, RoomStatusUpdateCallbackProxy.onConnectedToRoom is not called until someone
            // else joins the room. This makes the percentage of room setup progress go from 0/mMinPlayersToStart
            // to 2/mMinPlayersToStart. To give a more meaningful percentage until another player joins, we can assume
            // player creating the room, gets connected to the room when it's created.
            var connectedPlayerCount = Math.Max(1, GetConnectedParticipants().Count);
            return Math.Min(100F * connectedPlayerCount / mMinPlayersToStart, 100F);
        }

        public void ShowWaitingRoomUI()
        {
            var roomStatus = GetRoomStatus();
            if (roomStatus != RoomStatus.Connecting && roomStatus != RoomStatus.AutoMatching &&
                roomStatus != RoomStatus.Inviting)
            {
                return;
            }

            AndroidHelperFragment.ShowWaitingRoomUI(mRoom, mMinPlayersToStart, (response, room) =>
            {
                if (response == AndroidHelperFragment.WaitingRoomUIStatus.Valid)
                {
                    mRoom = room;
                    if (GetRoomStatus() == RoomStatus.Active)
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
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var invitationBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            int count = invitationBuffer.Call<int>("getCount");
                            Invitation[] invitations = new Invitation[count];
                            for (int i = 0; i < count; i++)
                            {
                                using (var invitationObject = invitationBuffer.Call<AndroidJavaObject>("get", i))
                                {
                                    invitations[i] = AndroidJavaConverter.ToInvitation(invitationObject);
                                }
                            }

                            callback(invitations);
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => callback(null));
            }
        }

        public void AcceptFromInbox(RealTimeMultiplayerListener listener)
        {
            var listenerOnGameThread = new OnGameThreadForwardingListener(listener);
            lock (mSessionLock)
            {
                if (GetRoomStatus() == RoomStatus.Active)
                {
                    OurUtils.Logger.e("Received attempt to accept invitation without cleaning up " +
                                      "active session.");
                    listenerOnGameThread.OnRoomConnected(false);
                    return;
                }

                AndroidHelperFragment.ShowInvitationInboxUI((status, invitation) =>
                {
                    if (status == UIStatus.NotAuthorized)
                    {
                        mAndroidClient.SignOut((() => listenerOnGameThread.OnRoomConnected(false)));
                        return;
                    }

                    if (status != UIStatus.Valid)
                    {
                        OurUtils.Logger.d("User did not complete invitation screen.");
                        listenerOnGameThread.OnRoomConnected(false);
                        return;
                    }

                    mInvitation = invitation;

                    AcceptInvitation(mInvitation.InvitationId, listener);
                });
            }
        }

        public void AcceptInvitation(string invitationId, RealTimeMultiplayerListener listener)
        {
            var listenerOnGameThread = new OnGameThreadForwardingListener(listener);
            lock (mSessionLock)
            {
                if (GetRoomStatus() == RoomStatus.Active)
                {
                    OurUtils.Logger.e("Received attempt to accept invitation without cleaning up " +
                                      "active session.");
                    listenerOnGameThread.OnRoomConnected(false);
                    return;
                }

                FindInvitation(invitationId, fail => listenerOnGameThread.OnRoomConnected(false),
                    invitation =>
                    {
                        mInvitation = invitation;

                        using (var roomConfigClass =
                            new AndroidJavaClass("com.google.android.gms.games.multiplayer.realtime.RoomConfig"))
                        using (var roomUpdateCallback = new AndroidJavaObject(
                            "com.google.games.bridge.RoomUpdateCallbackProxy",
                            new RoomUpdateCallbackProxy( /* parent= */ this, listenerOnGameThread)))
                        using (var roomConfigBuilder =
                            roomConfigClass.CallStatic<AndroidJavaObject>("builder", roomUpdateCallback))
                        using (var messageReceivedListener =
                            new AndroidJavaObject(
                                "com.google.games.bridge.RealTimeMessageReceivedListenerProxy",
                                new MessageReceivedListenerProxy(listenerOnGameThread)))
                        using (roomConfigBuilder.Call<AndroidJavaObject>("setOnMessageReceivedListener",
                            messageReceivedListener))
                        using (var roomStatusUpdateCallback =
                            new AndroidJavaObject("com.google.games.bridge.RoomStatusUpdateCallbackProxy",
                                new RoomStatusUpdateCallbackProxy(this, listenerOnGameThread)))
                        using (roomConfigBuilder.Call<AndroidJavaObject>("setRoomStatusUpdateCallback",
                            roomStatusUpdateCallback))
                        using (roomConfigBuilder.Call<AndroidJavaObject>("setInvitationIdToAccept", invitationId))
                        {
                            mRoomConfig = roomConfigBuilder.Call<AndroidJavaObject>("build");
                            mListener = listenerOnGameThread;

                            using (var task = mRtmpClient.Call<AndroidJavaObject>("join", mRoomConfig))
                            {
                                AndroidTaskUtils.AddOnFailureListener(
                                    task,
                                    e =>
                                    {
                                        listenerOnGameThread.OnRoomConnected(false);
                                        CleanSession();
                                    });
                            }
                        }
                    });
            }
        }

        public void SendMessageToAll(bool reliable, byte[] data)
        {
            if (reliable)
            {
                List<Participant> participants = GetConnectedParticipants();
                foreach (Participant participant in participants)
                {
                    SendMessage( /* reliable= */ true, participant.ParticipantId, data);
                }

                return;
            }

            var roomStatus = GetRoomStatus();
            if (roomStatus != RoomStatus.Active && roomStatus != RoomStatus.Connecting)
            {
                OurUtils.Logger.d("Sending message is not allowed in this state.");
                return;
            }

            string roomId = mRoom.Call<string>("getRoomId");
            using (mRtmpClient.Call<AndroidJavaObject>("sendUnreliableMessageToOthers", data, roomId)) ;
        }

        public void SendMessageToAll(bool reliable, byte[] data, int offset, int length)
        {
            SendMessageToAll(reliable, Misc.GetSubsetBytes(data, offset, length));
        }

        public void SendMessage(bool reliable, string participantId, byte[] data)
        {
            var roomStatus = GetRoomStatus();
            if (roomStatus != RoomStatus.Active && roomStatus != RoomStatus.Connecting)
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
                using (mRtmpClient.Call<AndroidJavaObject>("sendReliableMessage", data, roomId, participantId,
                    /* callback= */ null))
                    ;
                return;
            }

            using (mRtmpClient.Call<AndroidJavaObject>("sendUnreliableMessage", data, roomId, participantId)) ;
        }

        public void SendMessage(bool reliable, string participantId, byte[] data, int offset, int length)
        {
            SendMessage(reliable, participantId, Misc.GetSubsetBytes(data, offset, length));
        }

        public List<Participant> GetConnectedParticipants()
        {
            List<Participant> result = new List<Participant>();
            foreach (Participant participant in GetParticipantList())
            {
                if (participant.IsConnectedToRoom)
                {
                    result.Add(participant);
                }
            }

            return result;
        }

        private List<Participant> GetParticipantList()
        {
            if (mRoom == null)
            {
                return new List<Participant>();
            }

            List<Participant> participants = new List<Participant>();
            using (var participantsObject = mRoom.Call<AndroidJavaObject>("getParticipants"))
            {
                int size = participantsObject.Call<int>("size");
                for (int i = 0; i < size; i++)
                {
                    using (var participant = participantsObject.Call<AndroidJavaObject>("get", i))
                    {
                        participants.Add(AndroidJavaConverter.ToParticipant(participant));
                    }
                }
            }

            return participants;
        }

        public Participant GetSelf()
        {
            foreach (var participant in GetParticipantList())
            {
                if (participant.Player != null && participant.Player.id.Equals(mAndroidClient.GetUserId()))
                {
                    return participant;
                }
            }

            return null;
        }

        public Participant GetParticipant(string participantId)
        {
            if (GetRoomStatus() != RoomStatus.Active)
            {
                return null;
            }

            try
            {
                using (var participant = mRoom.Call<AndroidJavaObject>("getParticipant", participantId))
                {
                    return AndroidJavaConverter.ToParticipant(participant);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Invitation GetInvitation()
        {
            return mInvitation;
        }

        public void LeaveRoom()
        {
            if (GetRoomStatus() == RoomStatus.NotCreated)
            {
                return;
            }

            using (mRtmpClient.Call<AndroidJavaObject>("leave", mRoomConfig, mRoom.Call<String>("getRoomId"))) ;
            if (mListener != null)
            {
                mListener.OnRoomConnected(false);
            }

            CleanSession();
        }

        public bool IsRoomConnected()
        {
            return GetRoomStatus() == RoomStatus.Active;
        }

        private RoomStatus GetRoomStatus()
        {
            return mRoom != null ? (RoomStatus) mRoom.Call<int>("getStatus") : RoomStatus.NotCreated;
        }

        public void DeclineInvitation(string invitationId)
        {
            FindInvitation(invitationId, fail => { },
                invitation =>
                {
                    using (mRtmpClient.Call<AndroidJavaObject>("declineInvitation", invitationId)) ;
                });
        }

        private void FindInvitation(string invitationId, Action<bool> fail, Action<Invitation> callback)
        {
            using (var task = mInvitationsClient.Call<AndroidJavaObject>("loadInvitations"))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var invitationBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            int count = invitationBuffer.Call<int>("getCount");
                            for (int i = 0; i < count; i++)
                            {
                                Invitation invitation;
                                using (var invitationObject = invitationBuffer.Call<AndroidJavaObject>("get", i))
                                {
                                    invitation = AndroidJavaConverter.ToInvitation(invitationObject);
                                }

                                if (invitation.InvitationId == invitationId)
                                {
                                    callback(invitation);
                                    return;
                                }
                            }

                            OurUtils.Logger.e("Invitation with ID " + invitationId + " couldn't be found");
                            fail(true);
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        OurUtils.Logger.e("Couldn't load invitations.");
                        fail(true);
                    });
            }
        }

        private class RoomStatusUpdateCallbackProxy : AndroidJavaProxy
        {
            private OnGameThreadForwardingListener mListener;
            private AndroidRealTimeMultiplayerClient mParent;

            public RoomStatusUpdateCallbackProxy(AndroidRealTimeMultiplayerClient parent,
                OnGameThreadForwardingListener listener) : base(
                "com/google/games/bridge/RoomStatusUpdateCallbackProxy$Callback")
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
                for (int i = 0; i < size; i++)
                {
                    String participantId = participantIds.Call<String>("get", i);
                    Participant participant =
                        AndroidJavaConverter.ToParticipant(
                            mParent.mRoom.Call<AndroidJavaObject>("getParticipant", participantId));

                    if (participant.Status != Participant.ParticipantStatus.Declined &&
                        participant.Status != Participant.ParticipantStatus.Left)
                    {
                        continue;
                    }

                    mListener.OnParticipantLeft(participant);

                    var roomStatus = mParent.GetRoomStatus();
                    if (roomStatus != RoomStatus.Connecting && roomStatus != RoomStatus.AutoMatching)
                    {
                        mParent.LeaveRoom();
                    }
                }
            }

            public void onConnectedToRoom(AndroidJavaObject room)
            {
                if (mParent.GetRoomStatus() == RoomStatus.Active)
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
                if (mParent.GetRoomStatus() == RoomStatus.Active)
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
                if (mParent.GetRoomStatus() == RoomStatus.Active)
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
                if (mParent.GetRoomStatus() == RoomStatus.Active)
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
                foreach (Participant participant in mParent.GetConnectedParticipants())
                {
                    oldConnectedSet.Add(participant.ParticipantId);
                }

                mParent.mRoom = room;

                HashSet<string> connectedSet = new HashSet<string>();
                foreach (Participant participant in mParent.GetConnectedParticipants())
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
                foreach (string id in oldConnectedSet)
                {
                    if (!connectedSet.Contains(id))
                    {
                        noLongerConnected.Add(id);
                    }
                }

                if (mParent.GetRoomStatus() == RoomStatus.Deleted)
                {
                    OurUtils.Logger.e("Participants disconnected during room setup, failing. " + "Participants were: " +
                                      string.Join(",", noLongerConnected.ToArray()));
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
            private OnGameThreadForwardingListener mListener;

            public MessageReceivedListenerProxy(OnGameThreadForwardingListener listener) : base(
                "com/google/games/bridge/RealTimeMessageReceivedListenerProxy$Callback")
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
            private OnGameThreadForwardingListener mListener;
            private AndroidRealTimeMultiplayerClient mParent;

            public RoomUpdateCallbackProxy(AndroidRealTimeMultiplayerClient parent,
                OnGameThreadForwardingListener listener) : base(
                "com/google/games/bridge/RoomUpdateCallbackProxy$Callback")
            {
                mListener = listener;
                mParent = parent;
            }

            public void onRoomCreated( /* @OnRoomCreatedStatusCodes */ int statusCode, /* @Nullable Room */
                AndroidJavaObject room)
            {
                if (room == null)
                {
                    mListener.OnRoomConnected(false);
                    return;
                }

                mParent.mRoom = room;
                mListener.OnRoomSetupProgress(mParent.GetPercentComplete());
            }

            public void onJoinedRoom( /* @OnJoinedRoomStatusCodes */ int statusCode, /* @Nullable Room */
                AndroidJavaObject room)
            {
                if (room == null)
                {
                    mListener.OnRoomConnected(false);
                    return;
                }

                mParent.mRoom = room;

                int minPlayersToStart = 0;
                using (var autoMatchCriteria = room.Call<AndroidJavaObject>("getAutoMatchCriteria"))
                {
                    if (autoMatchCriteria != null)
                    {
                        minPlayersToStart += autoMatchCriteria.Call<int>("getInt", "min_automatch_players", 0);
                    }
                }

                using (var participantIds = room.Call<AndroidJavaObject>("getParticipantIds"))
                {
                    minPlayersToStart += participantIds.Call<int>("size");
                }

                mParent.mMinPlayersToStart = minPlayersToStart;
            }

            public void onLeftRoom( /* @OnLeftRoomStatusCodes */ int statusCode, /* @Nullable */ string roomId)
            {
                mListener.OnLeftRoom();
                mParent.CleanSession();
            }

            public void onRoomConnected( /* @OnRoomConnectedStatusCodes */ int statusCode, /* @Nullable Room */
                AndroidJavaObject room)
            {
                if (room == null)
                {
                    mListener.OnRoomConnected(false);
                    return;
                }

                mParent.mRoom = room;
                mListener.OnRoomConnected(true);
            }
        }

        /// <summary>
        /// Simple forwarding wrapper that makes sure all callbacks occur on the game thread.
        /// </summary>
        class OnGameThreadForwardingListener
        {
            private readonly RealTimeMultiplayerListener mListener;

            internal OnGameThreadForwardingListener(RealTimeMultiplayerListener listener)
            {
                mListener = Misc.CheckNotNull(listener);
            }

            public void OnRoomSetupProgress(float percent)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnRoomSetupProgress(percent));
            }

            public void OnRoomConnected(bool success)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnRoomConnected(success));
            }

            public void OnLeftRoom()
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnLeftRoom());
            }

            public void OnPeersConnected(string[] participantIds)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnPeersConnected(participantIds));
            }

            public void OnPeersDisconnected(string[] participantIds)
            {
                PlayGamesHelperObject.RunOnGameThread(
                    () => mListener.OnPeersDisconnected(participantIds));
            }

            public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data)
            {
                PlayGamesHelperObject.RunOnGameThread(
                    () => mListener.OnRealTimeMessageReceived(isReliable, senderId, data));
            }

            public void OnParticipantLeft(Participant participant)
            {
                PlayGamesHelperObject.RunOnGameThread(
                    () => mListener.OnParticipantLeft(participant));
            }
        }

        private void CleanSession()
        {
            lock (mSessionLock)
            {
                mRoom = null;
                mRoomConfig = null;
                mListener = null;
                mInvitation = null;
                mMinPlayersToStart = 0;
            }
        }

        private static Action<T> ToOnGameThread<T>(Action<T> toConvert)
        {
            return (val) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val));
        }
    }
}
#endif
