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
#if (UNITY_ANDROID || UNITY_IPHONE)
using System;
using System.Linq;
using System.Collections.Generic;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.OurUtils;
using GooglePlayGames.Native.PInvoke;


using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native {
public class NativeRealtimeMultiplayerClient : IRealTimeMultiplayerClient {
    private readonly object mSessionLock = new object();
    private readonly NativeClient mNativeClient;
    private readonly RealtimeManager mRealtimeManager;
    private volatile RoomSession mCurrentSession;

    internal NativeRealtimeMultiplayerClient(NativeClient nativeClient, RealtimeManager manager) {
        mNativeClient = Misc.CheckNotNull(nativeClient);
        mRealtimeManager = Misc.CheckNotNull(manager);
        mCurrentSession = GetTerminatedSession();
    }

    private RoomSession GetTerminatedSession() {
        var terminatedRoom = new RoomSession(mRealtimeManager, new NoopListener());
        terminatedRoom.EnterState(new ShutdownState(terminatedRoom));
        return terminatedRoom;
    }

    public void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
        RealTimeMultiplayerListener listener) {
        lock (mSessionLock) {
            var newSession = new RoomSession(mRealtimeManager, listener);
            if (mCurrentSession.IsActive()) {
                Logger.e("Received attempt to create a new room without cleaning up the old one.");
                newSession.LeaveRoom();
                return;
            }

            mCurrentSession = newSession;

            // We're holding the session lock, so no other threads could have torn down the session
            // in the meantime.

            using (var configBuilder = RealtimeRoomConfigBuilder.Create()) {
                var config = configBuilder.SetMinimumAutomatchingPlayers(minOpponents)
                    .SetMaximumAutomatchingPlayers(maxOpponents)
                    .SetVariant(variant)
                    .Build();

                using (config) {
                    using (var helper = HelperForSession(newSession)) {
                        newSession.StartRoomCreation(mNativeClient.GetUserId(),
                            () => mRealtimeManager.CreateRoom(config, helper,
                                newSession.HandleRoomResponse)
                        );
                    }
                }
            }
        }
    }

    private static RealTimeEventListenerHelper HelperForSession(RoomSession session) {
        return RealTimeEventListenerHelper.Create()
            .SetOnDataReceivedCallback((room, participant, data, isReliable) =>
                    session.OnDataReceived(room, participant, data, isReliable))
            .SetOnParticipantStatusChangedCallback((room, participant) =>
                    session.OnParticipantStatusChanged(room, participant))
            .SetOnRoomConnectedSetChangedCallback((room) =>
                    session.OnConnectedSetChanged(room))
            .SetOnRoomStatusChangedCallback((room) =>
                    session.OnRoomStatusChanged(room));
    }

    public void CreateWithInvitationScreen(uint minOpponents, uint maxOppponents, uint variant,
        RealTimeMultiplayerListener listener) {
        lock (mSessionLock) {
            var newRoom = new RoomSession(mRealtimeManager, listener);

            if (mCurrentSession.IsActive()) {
                Logger.e("Received attempt to create a new room without cleaning up the old one.");
                newRoom.LeaveRoom();
                return;
            }

            // The user attempted to create a room via the invitation screen, this is now the new
            // current room.
            mCurrentSession = newRoom;

            mRealtimeManager.ShowPlayerSelectUI(minOpponents, maxOppponents, true,
                response => {
                    if (response.Status() != Status.UIStatus.VALID) {
                        Logger.d("User did not complete invitation screen.");
                        newRoom.LeaveRoom();
                        return;
                    }

                    using (var configBuilder = RealtimeRoomConfigBuilder.Create()) {
                        configBuilder.SetVariant(variant);
                        configBuilder.PopulateFromUIResponse(response);
                        using (var config = configBuilder.Build()) {
                            using (var helper = HelperForSession(newRoom)) {
                                newRoom.StartRoomCreation(mNativeClient.GetUserId(),
                                    () => mRealtimeManager.CreateRoom(config, helper,
                                        newRoom.HandleRoomResponse));
                            }
                        }

                    }
                });
        }
    }

    public void AcceptFromInbox(RealTimeMultiplayerListener listener) {
        lock (mSessionLock) {
            var newRoom = new RoomSession(mRealtimeManager, listener);
            if (mCurrentSession.IsActive()) {
                Logger.e("Received attempt to accept invitation without cleaning up " +
                    "active session.");
                newRoom.LeaveRoom();
                return;
            }

            // The user accepted an invitation from the inbox, this is now the current room.
            mCurrentSession = newRoom;

            mRealtimeManager.ShowRoomInboxUI(
                response => {
                    if (response.ResponseStatus() != Status.UIStatus.VALID) {
                        Logger.d("User did not complete invitation screen.");
                        newRoom.LeaveRoom();
                        return;
                    }

                    // We are not cleaning up the invitation here to workaround a bug in the
                    // C++ SDK where it holds a reference to un-owned memory rather than making a
                    // copy. This is cleaned up after the callback comes back instead.
                    var invitation = response.Invitation();

                    using (var helper = HelperForSession(newRoom)) {
                        Logger.d("About to accept invitation " + invitation.Id());
                        newRoom.StartRoomCreation(mNativeClient.GetUserId(),
                            () => mRealtimeManager.AcceptInvitation(invitation, helper,
                                acceptResponse => {
                                    // Clean up the invitation here (see above comment).
                                    using (invitation) {
                                        newRoom.HandleRoomResponse(acceptResponse);
                                    }
                                }));
                    }
                });
        }
    }

    public void AcceptInvitation(string invitationId, RealTimeMultiplayerListener listener) {
        lock (mSessionLock) {
            var newRoom = new RoomSession(mRealtimeManager, listener);
            if (mCurrentSession.IsActive()) {
                Logger.e("Received attempt to accept invitation without cleaning up " +
                "active session.");
                newRoom.LeaveRoom();
                return;
            }

            mCurrentSession = newRoom;

            mRealtimeManager.FetchInvitations(response => {
                if (!response.RequestSucceeded()) {
                    Logger.e("Couldn't load invitations.");
                    newRoom.LeaveRoom();
                    return;
                }

                foreach (var invitation in response.Invitations()) {
                    using(invitation) {
                        if (invitation.Id().Equals(invitationId)) {
                            using (var helper = HelperForSession(newRoom)) {
                                newRoom.StartRoomCreation(mNativeClient.GetUserId(),
                                    () => mRealtimeManager.AcceptInvitation(
                                        invitation, helper, newRoom.HandleRoomResponse));
                                return;
                            }
                        }
                    }
                }

                Logger.e("Room creation failed since we could not find invitation with ID "
                    + invitationId);
                newRoom.LeaveRoom();
            });
        }
    }

    public void LeaveRoom() {
        mCurrentSession.LeaveRoom();
    }

    public void SendMessageToAll(bool reliable, byte[] data) {
        mCurrentSession.SendMessageToAll(reliable, data);
    }

    public void SendMessageToAll(bool reliable, byte[] data, int offset, int length) {
        mCurrentSession.SendMessageToAll(reliable, data, offset, length);
    }

    public void SendMessage(bool reliable, string participantId, byte[] data) {
        mCurrentSession.SendMessage(reliable, participantId, data);
    }

    public void SendMessage(bool reliable, string participantId, byte[] data, int offset,
        int length) {
        mCurrentSession.SendMessage(reliable, participantId, data, offset, length);
    }

    public List<Participant> GetConnectedParticipants() {
        return mCurrentSession.GetConnectedParticipants();
    }

    public Participant GetSelf() {
        return mCurrentSession.GetSelf();
    }

    public Participant GetParticipant(string participantId) {
        return mCurrentSession.GetParticipant(participantId);
    }

    public bool IsRoomConnected() {
        return mCurrentSession.IsRoomConnected();
    }

    public void DeclineInvitation(string invitationId) {
        mRealtimeManager.FetchInvitations(response => {
            if (!response.RequestSucceeded()) {
                Logger.e("Couldn't load invitations.");
                return;
            }

            foreach (var invitation in response.Invitations()) {
                using(invitation) {
                    if (invitation.Id().Equals(invitationId)) {
                        mRealtimeManager.DeclineInvitation(invitation);
                    }
                }
            }
        });
    }

    /// <summary>
    /// A stub implementation of the RealTimeMultiplayerListener API. Used so that we can guarantee
    /// that we will never have a null reference to a listener object.
    /// </summary>
    private class NoopListener : RealTimeMultiplayerListener {
        public void OnRoomSetupProgress(float percent) { }
        public void OnRoomConnected(bool success) { }
        public void OnLeftRoom() { }
        public void OnPeersConnected(string[] participantIds) { }
        public void OnPeersDisconnected(string[] participantIds) { }
        public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data) { }
    }

    /// <summary>A class that encapsulates the state machine required to map the native callbacks to the
    /// corresponding callbacks in Unity. This session exposes an API that mirrors all the commands
    /// that can be issued to the RealtimeMultiplayerClient and directs these to the current state
    /// of the state machine which performs the actual logic.
    ///
    /// <para>All methods that can transitively update the state of the statemachine must be guarded with
    /// the lifecycle lock to ensure a consistent user-facing view of the state of the session.</para>
    ///
    /// <para>Note that this class maintains the invariant that mState is never null.</para>
    /// <para>See the doc for the individual states for details on state transitions and note that
    /// all states assume that all lifecycle methods will be invoked while the containing
    /// RoomSession is holding the lifecycle lock.</para>
    /// </summary>
    private class RoomSession {
        private readonly object mLifecycleLock = new object();
        private readonly OnGameThreadForwardingListener mListener;
        private readonly RealtimeManager mManager;

        private volatile string mCurrentPlayerId;
        private volatile State mState;
        private volatile bool mStillPreRoomCreation;

        internal RoomSession(RealtimeManager manager, RealTimeMultiplayerListener listener) {
            mManager = Misc.CheckNotNull(manager);
            mListener = new OnGameThreadForwardingListener(listener);

            EnterState(new BeforeRoomCreateStartedState(this));
            mStillPreRoomCreation = true;
        }

        internal RealtimeManager Manager() {
            return mManager;
        }

        internal bool IsActive() {
            return mState.IsActive();
        }

        internal string SelfPlayerId() {
            return mCurrentPlayerId;
        }

        internal OnGameThreadForwardingListener OnGameThreadListener() {
            return mListener;
        }

        /**
         * Lifecycle methods - these might cause state transitions, and thus require us to hold a
         * lock while they're executing to prevent any externally visible inconsistent state (e.g.
         * receiving any callbacks after we've left a room).
         */

        internal void EnterState(State handler) {
            lock (mLifecycleLock) {
                mState = Misc.CheckNotNull(handler);
                Logger.d("Entering state: " + handler.GetType().Name);
                mState.OnStateEntered();
            }
        }

        internal void LeaveRoom() {
            lock (mLifecycleLock) {
                mState.LeaveRoom();
            }
        }

        /// <summary>
        /// Starts the room creation provided the session is still in a state that allows room
        /// creation (i.e. it hasn't been torn down).
        /// </summary>
        /// <param name="currentPlayerId">The current player identifier.</param>
        /// <param name="createRoom">The action that will begin creating the room.</param>
        internal void StartRoomCreation(string currentPlayerId, Action createRoom) {
            lock (mLifecycleLock) {
                if (!mStillPreRoomCreation) {
                    Logger.e("Room creation started more than once, this shouldn't happen!");
                    return;
                }

                if (!mState.IsActive()) {
                    Logger.w("Received an attempt to create a room after the session was already " +
                        "torn down!");
                    return;
                }

                mCurrentPlayerId = Misc.CheckNotNull(currentPlayerId);
                mStillPreRoomCreation = false;
                EnterState(new RoomCreationPendingState(this));
                createRoom.Invoke();
            }
        }

        internal void OnRoomStatusChanged(NativeRealTimeRoom room) {
            lock (mLifecycleLock) {
                mState.OnRoomStatusChanged(room);
            }
        }

        internal void OnConnectedSetChanged(NativeRealTimeRoom room) {
            lock (mLifecycleLock) {
                mState.OnConnectedSetChanged(room);
            }
        }

        internal void OnParticipantStatusChanged(NativeRealTimeRoom room,
            MultiplayerParticipant participant) {
            lock (mLifecycleLock) {
                mState.OnParticipantStatusChanged(room, participant);
            }
        }

        internal void HandleRoomResponse(RealtimeManager.RealTimeRoomResponse response) {
            lock (mLifecycleLock) {
                mState.HandleRoomResponse(response);
            }
        }


        /**
         * Non-Lifecycle methods - these cannot cause state transitions, and thus we do not need to
         * hold any locks. We rely on only accessing volatile fields to ensure consistency instead.
         */
        internal void OnDataReceived(NativeRealTimeRoom room, MultiplayerParticipant sender,
            byte[] data, bool isReliable) {
            mState.OnDataReceived(room, sender, data, isReliable);
        }

        internal void SendMessageToAll(bool reliable, byte[] data) {
            SendMessageToAll(reliable, data, 0, data.Length);
        }

        internal void SendMessageToAll(bool reliable, byte[] data, int offset, int length) {
            mState.SendToAll(data, offset, length, reliable);
        }

        internal void SendMessage(bool reliable, string participantId, byte[] data) {
            SendMessage(reliable, participantId, data, 0, data.Length);
        }

        internal void SendMessage(bool reliable, string participantId, byte[] data, int offset,
                                int length) {
            mState.SendToSpecificRecipient(participantId, data, offset, length, reliable);
        }

        internal List<Participant> GetConnectedParticipants() {
            return mState.GetConnectedParticipants();
        }

        internal virtual Participant GetSelf() {
            return mState.GetSelf();
        }

        internal virtual Participant GetParticipant(string participantId) {
            return mState.GetParticipant(participantId);
        }

        internal virtual bool IsRoomConnected() {
            return mState.IsRoomConnected();
        }
    }

    private static T WithDefault<T>(T presented, T defaultValue) where T : class {
        return presented != null ? presented : defaultValue;
    }

    /// <summary>
    /// Simple forwarding wrapper that makes sure all callbacks occur on the game thread.
    /// </summary>
    class OnGameThreadForwardingListener {

        private readonly RealTimeMultiplayerListener mListener;

        internal OnGameThreadForwardingListener(RealTimeMultiplayerListener listener) {
            mListener = Misc.CheckNotNull(listener);
        }

        public void RoomSetupProgress(float percent) {
            PlayGamesHelperObject.RunOnGameThread(() => mListener.OnRoomSetupProgress(percent));
        }

        public void RoomConnected(bool success) {
            PlayGamesHelperObject.RunOnGameThread(() => mListener.OnRoomConnected(success));
        }

        public void LeftRoom() {
            PlayGamesHelperObject.RunOnGameThread(() => mListener.OnLeftRoom());
        }

        public void PeersConnected(string[] participantIds) {
            PlayGamesHelperObject.RunOnGameThread(() => mListener.OnPeersConnected(participantIds));
        }

        public void PeersDisconnected(string[] participantIds) {
            PlayGamesHelperObject.RunOnGameThread(
                () => mListener.OnPeersDisconnected(participantIds));
        }

        public void RealTimeMessageReceived(bool isReliable, string senderId, byte[] data) {
            PlayGamesHelperObject.RunOnGameThread(
                () => mListener.OnRealTimeMessageReceived(isReliable, senderId, data));
        }
    }

    /// <summary>
    /// A base state implementation. All methods do nothing or return stub values. States that
    /// require specific behavior must override the corresponding methods.
    /// </summary>
    internal abstract class State {
        internal virtual void HandleRoomResponse(RealtimeManager.RealTimeRoomResponse response) {
            Logger.d(this.GetType().Name + ".HandleRoomResponse: Defaulting to no-op.");
        }

        internal virtual bool IsActive() {
            Logger.d(this.GetType().Name + ".IsNonPreemptable: Is preemptable by default.");
            return true;
        }

        internal virtual void LeaveRoom() {
            Logger.d(this.GetType().Name + ".LeaveRoom: Defaulting to no-op.");
        }

        internal virtual void OnStateEntered() {
            Logger.d(this.GetType().Name + ".OnStateEntered: Defaulting to no-op.");
        }

        internal virtual void OnRoomStatusChanged(NativeRealTimeRoom room) {
            Logger.d(this.GetType().Name + ".OnRoomStatusChanged: Defaulting to no-op.");
        }

        internal virtual void OnConnectedSetChanged(NativeRealTimeRoom room) {
            Logger.d(this.GetType().Name + ".OnConnectedSetChanged: Defaulting to no-op.");
        }

        internal virtual void OnParticipantStatusChanged(NativeRealTimeRoom room,
            MultiplayerParticipant participant) {
            Logger.d(this.GetType().Name + ".OnParticipantStatusChanged: Defaulting to no-op.");
        }

        internal virtual void OnDataReceived(NativeRealTimeRoom room, MultiplayerParticipant sender,
            byte[] data, bool isReliable) {
            Logger.d(this.GetType().Name + ".OnDataReceived: Defaulting to no-op.");
        }

        internal virtual void SendToSpecificRecipient(
            string recipientId, byte[] data, int offset, int length, bool isReliable) {
            Logger.d(this.GetType().Name + ".SendToSpecificRecipient: Defaulting to no-op.");
        }

        internal virtual void SendToAll(byte[] data, int offset, int length, bool isReliable) {
            Logger.d(this.GetType().Name + ".SendToApp: Defaulting to no-op.");
        }

        internal virtual List<Participant> GetConnectedParticipants() {
            Logger.d(this.GetType().Name + ".GetConnectedParticipants: Returning empty connected" +
                " participants");
            return new List<Participant>();
        }

        internal virtual Participant GetSelf() {
            Logger.d(this.GetType().Name + ".GetSelf: Returning null self.");
            return null;
        }

        internal virtual Participant GetParticipant(string participantId) {
            Logger.d(this.GetType().Name + ".GetSelf: Returning null participant.");
            return null;
        }

        internal virtual bool IsRoomConnected() {
            Logger.d(this.GetType().Name + ".IsRoomConnected: Returning room not connected.");
            return false;
        }
    }

    /// <summary>
    /// A base class for all states where message passing is enabled (i.e. the Active and
    /// Connecting states).
    /// </summary>
    private abstract class MessagingEnabledState : State {
        protected readonly RoomSession mSession;
        protected NativeRealTimeRoom mRoom;
        protected Dictionary<string, MultiplayerParticipant> mNativeParticipants;
        protected Dictionary<string, Participant> mParticipants;

        internal MessagingEnabledState(RoomSession session, NativeRealTimeRoom room) {
            mSession = Misc.CheckNotNull(session);
            UpdateCurrentRoom(room);
        }

        internal void UpdateCurrentRoom(NativeRealTimeRoom room) {
            if (mRoom != null) {
                mRoom.Dispose();
            }
            mRoom = Misc.CheckNotNull(room);
            mNativeParticipants = mRoom.Participants().ToDictionary(p => p.Id());
            mParticipants = mNativeParticipants.Values
                .Select(p => p.AsParticipant())
                .ToDictionary(p => p.ParticipantId);
        }

        internal sealed override void OnRoomStatusChanged(NativeRealTimeRoom room) {
            HandleRoomStatusChanged(room);
            UpdateCurrentRoom(room);
        }

        internal virtual void HandleRoomStatusChanged(NativeRealTimeRoom room) {
            // noop
        }

        internal sealed override void OnConnectedSetChanged(NativeRealTimeRoom room) {
            HandleConnectedSetChanged(room);
            UpdateCurrentRoom(room);
        }

        internal virtual void HandleConnectedSetChanged(NativeRealTimeRoom room) {
            // noop
        }

        internal sealed override void OnParticipantStatusChanged(NativeRealTimeRoom room,
            MultiplayerParticipant participant) {
            HandleParticipantStatusChanged(room, participant);
            UpdateCurrentRoom(room);
        }

        internal virtual void HandleParticipantStatusChanged(NativeRealTimeRoom room,
            MultiplayerParticipant participant) {
            // noop
        }

        internal sealed override List<Participant> GetConnectedParticipants() {
            var connectedParticipants = mParticipants.Values
                .Where(p => p.IsConnectedToRoom)
                .ToList();

            connectedParticipants.Sort();

            return connectedParticipants;
        }

        internal override void SendToSpecificRecipient(
            string recipientId, byte[] data, int offset, int length, bool isReliable) {
            if (!mNativeParticipants.ContainsKey(recipientId)) {
                Logger.e("Attempted to send message to unknown participant " + recipientId);
                return;
            }

            if (isReliable) {
                mSession.Manager().SendReliableMessage(mRoom, mNativeParticipants[recipientId],
                    Misc.GetSubsetBytes(data, offset, length), null);
            } else {
                mSession.Manager().SendUnreliableMessageToSpecificParticipants(mRoom,
                    new List<MultiplayerParticipant> {mNativeParticipants[recipientId]},
                    Misc.GetSubsetBytes(data, offset, length));
            }
        }

        internal override void SendToAll(byte[] data, int offset,
            int length, bool isReliable) {
            var trimmed = Misc.GetSubsetBytes(data, offset, length);

            if (isReliable) {
                foreach (var participantId in mNativeParticipants.Keys) {
                    SendToSpecificRecipient(participantId, trimmed, 0, trimmed.Length, true);
                }
            } else {
                mSession.Manager().SendUnreliableMessageToAll(mRoom, trimmed);
            }
        }

        internal override void OnDataReceived(NativeRealTimeRoom room,
            MultiplayerParticipant sender, byte[] data, bool isReliable) {
            mSession.OnGameThreadListener().RealTimeMessageReceived(isReliable, sender.Id(), data);
        }
    }

    /// <summary>The state of the session before we have initiated room creation. This is necessary
    /// in cases where we have to do additional callbacks to look up information before the room
    /// can be created (e.g. finding the invitation corresponding to an ID).
    ///
    /// <para>This is the initial state for all sessions. In the event of an error before room
    /// creation states, this state will immediately transition to Shutdown (as there is nothing
    /// to clean up). Unlike other states, transitions out of this state are determined externally
    /// by the enclosing room (which knows when we can begin room creation).</para>
    /// </summary>
    class BeforeRoomCreateStartedState : State {
        private readonly RoomSession mContainingSession;

        internal BeforeRoomCreateStartedState(RoomSession session) {
            mContainingSession = Misc.CheckNotNull(session);
        }

        internal override void LeaveRoom() {
            Logger.d("Session was torn down before room was created.");
            mContainingSession.OnGameThreadListener().RoomConnected(false);
            mContainingSession.EnterState(new ShutdownState(mContainingSession));
        }
    }

    /// <summary>The state we were have issued a room creation request. Normally this state
    /// immediately transitions into the connecting state where we begin creating the mesh network.
    ///
    /// <para>This state can transition to 3 other states: Connecting, Aborting room, or shutdown.
    /// If room creation proceeds normally and there are no intervening calls to leave room, we
    /// transition into 'Connecting'. If the user tears down the session before room creation
    /// completes, we transition into 'Aborting Room', and if the room creation fails we transition
    /// immediately to 'Shutdown'.</para>
    /// </summary>
    class RoomCreationPendingState : State {
        private readonly RoomSession mContainingSession;

        internal RoomCreationPendingState(RoomSession session) {
            mContainingSession = Misc.CheckNotNull(session);
        }

        internal override void HandleRoomResponse(RealtimeManager.RealTimeRoomResponse response) {
            if (!response.RequestSucceeded()) {
                mContainingSession.EnterState(new ShutdownState(mContainingSession));
                mContainingSession.OnGameThreadListener().RoomConnected(false);
                return;
            }

            mContainingSession.EnterState(new ConnectingState(response.Room(), mContainingSession));
        }

        internal override bool IsActive() {
            // The client must explicitly leave before cleaning up a room that is being created.
            return true;
        }

        internal override void LeaveRoom() {
            Logger.d("Received request to leave room during room creation, aborting creation.");
            mContainingSession.EnterState(new AbortingRoomCreationState(mContainingSession));
        }

    }

    /// <summary>A state indicating we're in the process of creating a fully connected mesh network
    /// between all multiplayer clients.
    ///
    /// <para>We can transition into 2 states from 'Connecting': 'Active' and 'Leaving room'.
    /// If we are able to create a mesh network from all participants, we move into 'Active'.
    /// If any participant fails to create the mesh or the user asks to leave the room, we
    /// transition into 'Leave Room'.</para>
    /// </summary>
    class ConnectingState : MessagingEnabledState {
        private const float InitialPercentComplete = 20.0F;
        private static readonly HashSet<Types.ParticipantStatus> FailedStatuses =
            new HashSet<Types.ParticipantStatus> {
            Types.ParticipantStatus.DECLINED,
            Types.ParticipantStatus.LEFT,
        };

        private HashSet<string> mConnectedParticipants = new HashSet<string>();
        private float mPercentComplete = InitialPercentComplete;
        private float mPercentPerParticipant;

        internal ConnectingState(NativeRealTimeRoom room, RoomSession session)
            : base(session, room) {
            mPercentPerParticipant =
                (100.0f - InitialPercentComplete) / (float)room.ParticipantCount();
        }

        internal override void OnStateEntered() {
            mSession.OnGameThreadListener().RoomSetupProgress(mPercentComplete);
        }

        internal override void HandleConnectedSetChanged(NativeRealTimeRoom room) {
            HashSet<string> newConnectedSet = new HashSet<string>();

            foreach (var participant in room.Participants()) {
                using (participant) {
                    if (participant.IsConnectedToRoom()) {
                        newConnectedSet.Add(participant.Id());
                    }
                }
            }

            // If the connected set hasn't actually changed, bail out.
            if (mConnectedParticipants.Equals(newConnectedSet)) {
                Logger.w("Received connected set callback with unchanged connected set!");
                return;
            }

            var noLongerConnected = mConnectedParticipants.Except(newConnectedSet);

            // Check whether a participant that was in the connected set has left it.
            // If so, we will never reach a fully connected state, and should fail room
            // creation.
            if (noLongerConnected.Any()) {
                Logger.e("Participants disconnected during room setup, failing. " +
                    "Participants were: " + string.Join(",", noLongerConnected.ToArray()));
                LeaveRoom();
                return;
            }

            var newlyConnected = newConnectedSet.Except(mConnectedParticipants);

            Logger.d("New participants connected: " +
                string.Join(",", newlyConnected.ToArray()));

            // If we're fully connected, transition to the Active state and signal the client.
            if (newConnectedSet.Count() == room.ParticipantCount()) {
                Logger.d("Fully connected! Transitioning to active state.");
                mSession.EnterState(new ActiveState(room, mSession));
                mSession.OnGameThreadListener().RoomConnected(true);
                return;
            }

            // Otherwise, we're not fully there. Increment the progress by the appropriate
            // amount and inform the client.
            mPercentComplete += mPercentPerParticipant * (float)newlyConnected.Count();
            mConnectedParticipants = newConnectedSet;
            mSession.OnGameThreadListener().RoomSetupProgress(mPercentComplete);
        }

        internal override void HandleParticipantStatusChanged(NativeRealTimeRoom room,
            MultiplayerParticipant participant) {
            if (!FailedStatuses.Contains(participant.Status())) {
                return;
            }

            Logger.e(string.Format("Participant {0} changed to status {1}, room will never be" +
                "fully connected.", participant.Id(), participant.Status()));
            LeaveRoom();
        }

        internal override void LeaveRoom() {
            mSession.EnterState(new LeavingRoom(mSession, mRoom,
                () => mSession.OnGameThreadListener().RoomConnected(false)));
        }
    }


    /// <summary>The active state, i.e. we have created a full mesh network and have informed the user.
    /// <para>The only transition out of 'Active' is into 'Leaving Room'. This occurs either when
    /// we are informed that the user has been unexpectedly disconnected, or when the user
    /// explicitly asks to leave.</para>
    /// </summary>
    class ActiveState : MessagingEnabledState {
        internal ActiveState(NativeRealTimeRoom room, RoomSession session) : base(session, room) {
        }

        internal override void OnStateEntered() {
            if (GetSelf() == null) {
                Logger.e("Room reached active state with unknown participant for the player");
                LeaveRoom();
            }
        }

        internal override bool IsRoomConnected() {
            return true;
        }

        internal override Participant GetParticipant(string participantId) {
            if (!mParticipants.ContainsKey(participantId)) {
                Logger.e("Attempted to retrieve unknown participant " + participantId);
                return null;
            }

            return mParticipants[participantId];
        }

        internal override Participant GetSelf() {
            foreach (var participant in mParticipants.Values) {
                if (participant.Player != null
                    && participant.Player.PlayerId.Equals(mSession.SelfPlayerId())) {
                    return participant;
                }
            }

            return null;
        }

        internal override void HandleConnectedSetChanged(NativeRealTimeRoom room) {
            List<string> newlyConnected = new List<string>();
            List<string> newlyLeft = new List<string>();

            var updatedParticipants = room.Participants().ToDictionary(p => p.Id());

            foreach (var participantId in mNativeParticipants.Keys) {
                var freshParticipant = updatedParticipants[participantId];
                var staleParticipant = mNativeParticipants[participantId];

                if (staleParticipant.IsConnectedToRoom() && !freshParticipant.IsConnectedToRoom()) {
                    newlyLeft.Add(participantId);
                }

                if (!staleParticipant.IsConnectedToRoom() && freshParticipant.IsConnectedToRoom()) {
                    newlyConnected.Add(participantId);
                }
            }

            // Update the cached participants to reflect the new statuses by cleaning up the old
            // ones and then updating the new values.
            foreach (var participant in mNativeParticipants.Values) {
                participant.Dispose();
            }

            mNativeParticipants = updatedParticipants;
            mParticipants = mNativeParticipants.Values
                .Select(p => p.AsParticipant())
                .ToDictionary(p => p.ParticipantId);

            Logger.d("Updated participant statuses: " +
                string.Join(",", mParticipants.Values.Select(p => p.ToString()).ToArray()));

            // Check whether the current player was disconnected from the room.
            if (newlyLeft.Contains(GetSelf().ParticipantId)) {
                Logger.w("Player was disconnected from the multiplayer session.");
            }

            // Strip out the participant ID of the local player - this player is not a "peer".
            var selfId = GetSelf().ParticipantId;
            newlyConnected = newlyConnected.Where(peerId => !peerId.Equals(selfId)).ToList();
            newlyLeft = newlyLeft.Where(peerId => !peerId.Equals(selfId)).ToList();

            // Otherwise inform the client about changes in room participants, screening out
            // results about the local player.
            if (newlyConnected.Count > 0) {
                newlyConnected.Sort();
                mSession.OnGameThreadListener()
                    .PeersConnected(newlyConnected.Where(peer => !peer.Equals(selfId)).ToArray());
            }

            if (newlyLeft.Count > 0) {
                newlyLeft.Sort();
                mSession.OnGameThreadListener()
                    .PeersDisconnected(newlyLeft.Where(peer => !peer.Equals(selfId)).ToArray());
            }
        }

        internal override void LeaveRoom() {
            mSession.EnterState(new LeavingRoom(mSession, mRoom,
                () => mSession.OnGameThreadListener().LeftRoom()));
        }
    }

    /// <summary>
    /// A terminal state. Once this state is reached the session is considered dead and can be
    /// safely disposed of.
    /// </summary>
    class ShutdownState : State {
        private readonly RoomSession mSession;

        internal ShutdownState(RoomSession session) {
            mSession = Misc.CheckNotNull(session);
        }

        internal override bool IsActive() {
            return false;
        }

        internal override void LeaveRoom() {
            mSession.OnGameThreadListener().LeftRoom();
        }
    }

    /// <summary>
    /// A state indicating the we're in the process of leaving a room. Sessions that enter this
    /// state immediately transition into 'Shutdown' after issuing a request to leave the room.
    /// </summary>
    class LeavingRoom : State {
        private readonly RoomSession mSession;
        private readonly NativeRealTimeRoom mRoomToLeave;
        private readonly Action mLeavingCompleteCallback;

        internal LeavingRoom(RoomSession session, NativeRealTimeRoom room,
            Action leavingCompleteCallback) {
            mSession = Misc.CheckNotNull(session);
            mRoomToLeave = Misc.CheckNotNull(room);
            mLeavingCompleteCallback = Misc.CheckNotNull(leavingCompleteCallback);
        }

        internal override bool IsActive() {
            return false;
        }

        internal override void OnStateEntered() {
            mSession.Manager().LeaveRoom(mRoomToLeave, (status) => mLeavingCompleteCallback());
            mSession.EnterState(new ShutdownState(mSession));
        }
    }

    /// <summary>The state indicating that we were in the process of creating a room, but that the
    /// user quit before this room was sucessfully created. This state is implemented such that any
    /// room response that we receive will result in the room being left, and the session
    /// transitioning to a terminal state.
    ///
    /// <para>This state transitions into 'Shutdown' (if the room creation failed) or 'Leaving room'
    /// (if room creation succeeded).</para>
    /// </summary>
    class AbortingRoomCreationState : State {
        private readonly RoomSession mSession;

        internal AbortingRoomCreationState(RoomSession session) {
            mSession = Misc.CheckNotNull(session);
        }

        internal override bool IsActive() {
            return false;
        }

        internal override void HandleRoomResponse(RealtimeManager.RealTimeRoomResponse response) {
            // If the room creation didn't succeed, we have nothing left to do, just bail out
            // and alert the user callback.
            if (!response.RequestSucceeded()) {
                mSession.EnterState(new ShutdownState(mSession));
                mSession.OnGameThreadListener().RoomConnected(false);
                return;
            }

            // We just created a room which we're not going to use. Clean up and notify the user
            // when we're done.
            mSession.EnterState(new LeavingRoom(mSession, response.Room(),
                () => mSession.OnGameThreadListener().RoomConnected(false)));
        }
    }
}
}
#endif
