// <copyright file="NativeClient.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
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

#if UNITY_ANDROID

namespace GooglePlayGames.Native
{

    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames.Native.PInvoke;
    using GooglePlayGames.OurUtils;
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi.Events;
    using GooglePlayGames.BasicApi.Video;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class NativeClient : IPlayGamesClient
    {

        private readonly IClientImpl clientImpl;

        private enum AuthState
        {
            Unauthenticated,
            Authenticated
        }

        private readonly object GameServicesLock = new object();
        private readonly object AuthStateLock = new object();

        private readonly PlayGamesClientConfiguration mConfiguration;
        private GameServices mServices;
        private volatile NativeTurnBasedMultiplayerClient mTurnBasedClient;
        private volatile NativeRealtimeMultiplayerClient mRealTimeClient;
        private volatile ISavedGameClient mSavedGameClient;
        private volatile IEventsClient mEventsClient;
        private volatile IVideoClient mVideoClient;
        private volatile TokenClient mTokenClient;
        private volatile Action<Invitation, bool> mInvitationDelegate;
        private volatile Player mUser = null;
        private volatile List<Player> mFriends = null;
        private volatile Action<bool, string> mPendingAuthCallbacks;
        private volatile AuthState mAuthState = AuthState.Unauthenticated;
        private volatile uint mAuthGeneration = 0;
        private volatile bool friendsLoading = false;

        internal NativeClient(PlayGamesClientConfiguration configuration,
            IClientImpl clientImpl)
        {
            PlayGamesHelperObject.CreateObject();
            this.mConfiguration = Misc.CheckNotNull(configuration);
            this.clientImpl = clientImpl;
        }

        private GameServices GameServices()
        {
            lock (GameServicesLock)
            {
                return mServices;
            }
        }
        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.Authenticate"/>
        public void Authenticate(Action<bool,string> callback, bool silent)
        {
            lock (AuthStateLock)
            {
                // If the user is already authenticated, just fire the callback, we don't need
                // any additional work.
                if (mAuthState == AuthState.Authenticated)
                {
                    InvokeCallbackOnGameThread(callback, true, null);
                    return;
                }
            }

            // reset friends loading flag
            friendsLoading = false;

            InitializeTokenClient();

            Debug.Log("Starting Auth with token client.");
            mTokenClient.FetchTokens(silent, (int result) => {
                bool succeed = result == 0 /* CommonStatusCodes.SUCCEED */;
                InitializeGameServices();
                if (succeed) {
                    if (callback != null) {
                      mPendingAuthCallbacks += callback;
                    }
                    GameServices().StartAuthorizationUI();
                } else {
                    Action<bool, string> localCallback = callback;
                    if (result == 16 /* CommonStatusCodes.CANCELED */) {
                        InvokeCallbackOnGameThread(localCallback, false, "Authentication canceled");
                    } else if (result == 8 /* CommonStatusCodes.DEVELOPER_ERROR */) {
                        InvokeCallbackOnGameThread(localCallback, false, "Authentication failed - developer error");
                    } else {
                        InvokeCallbackOnGameThread(localCallback, false, "Authentication failed");
                    }
                }
            });
        }

        private static Action<T> AsOnGameThreadCallback<T>(Action<T> callback)
        {
            if (callback == null)
            {
                return delegate
                {
                };
            }

            return result => InvokeCallbackOnGameThread(callback, result);
        }

        private static void InvokeCallbackOnGameThread<T,S>(Action<T,S> callback, T data, S msg)
        {
            if (callback == null)
            {
                return;
            }

            PlayGamesHelperObject.RunOnGameThread(() =>
                {
                    OurUtils.Logger.d("Invoking user callback on game thread");
                    callback(data, msg);
                });
        }

        private static void InvokeCallbackOnGameThread<T>(Action<T> callback, T data)
        {
            if (callback == null)
            {
                return;
            }

            PlayGamesHelperObject.RunOnGameThread(() =>
                {
                    GooglePlayGames.OurUtils.Logger.d("Invoking user callback on game thread");
                    callback(data);
                });
        }

        private void InitializeGameServices()
        {
            lock (GameServicesLock)
            {
                if (mServices != null)
                {
                    return;
                }

                using (var builder = GameServicesBuilder.Create())
                {
                    using (var config = clientImpl.CreatePlatformConfiguration(mConfiguration))
                    {
                        // We need to make sure that the invitation delegate
                        // is registered before the services object is
                        // initialized - otherwise we might miss a callback if
                        // the game was opened because of a user accepting an
                        // invitation through a system notification.
                        RegisterInvitationDelegate(mConfiguration.InvitationDelegate);
                        builder.SetOnAuthFinishedCallback(HandleAuthTransition);
                        builder.SetOnTurnBasedMatchEventCallback(
                            (eventType, matchId, match) =>
                                mTurnBasedClient.HandleMatchEvent(
                                    eventType, matchId, match));
                        builder.SetOnMultiplayerInvitationEventCallback(
                                    HandleInvitation);
                        if (mConfiguration.EnableSavedGames)
                        {
                            builder.EnableSnapshots();
                        }

                        string[] scopes = mConfiguration.Scopes;
                        for (int i = 0; i < scopes.Length; i++) {
                            builder.AddOauthScope(scopes[i]);
                        }

                        if (mConfiguration.IsHidingPopups)
                        {
                            builder.SetShowConnectingPopup(false);
                        }

                        Debug.Log("Building GPG services, implicitly attempts silent auth");
                        mServices = builder.Build(config);
                        mEventsClient = new NativeEventClient(new EventManager(mServices));
                        mVideoClient = new NativeVideoClient(new VideoManager(mServices));
                        mTurnBasedClient =
                        new NativeTurnBasedMultiplayerClient(this, new TurnBasedManager(mServices));

                        mTurnBasedClient.RegisterMatchDelegate(mConfiguration.MatchDelegate);

                        mRealTimeClient =
                        new NativeRealtimeMultiplayerClient(this, new RealtimeManager(mServices));

                        if (mConfiguration.EnableSavedGames)
                        {
                            mSavedGameClient =
                            new NativeSavedGameClient(new SnapshotManager(mServices));
                        }
                        else
                        {
                            mSavedGameClient = new UnsupportedSavedGamesClient(
                                "You must enable saved games before it can be used. " +
                                "See PlayGamesClientConfiguration.Builder.EnableSavedGames.");
                        }

                        InitializeTokenClient();
                    }
                }
            }
        }

        private void InitializeTokenClient() {
            if (mTokenClient != null) {
                return;
            }
            mTokenClient = clientImpl.CreateTokenClient(true);

            if (!GameInfo.WebClientIdInitialized() &&
                (mConfiguration.IsRequestingIdToken || mConfiguration.IsRequestingAuthCode)) {
                OurUtils.Logger.e("Server Auth Code and ID Token require web clientId to configured.");
            }
            string[] scopes = mConfiguration.Scopes;
            // Set the auth flags in the token client.
            mTokenClient.SetWebClientId(GameInfo.WebClientId);
            mTokenClient.SetRequestAuthCode(mConfiguration.IsRequestingAuthCode, mConfiguration.IsForcingRefresh);
            mTokenClient.SetRequestEmail(mConfiguration.IsRequestingEmail);
            mTokenClient.SetRequestIdToken(mConfiguration.IsRequestingIdToken);
            mTokenClient.SetHidePopups(mConfiguration.IsHidingPopups);
            mTokenClient.AddOauthScopes("https://www.googleapis.com/auth/games_lite");
            if (mConfiguration.EnableSavedGames) {
                mTokenClient.AddOauthScopes("https://www.googleapis.com/auth/drive.appdata");
            }
            mTokenClient.AddOauthScopes(scopes);
            mTokenClient.SetAccountName(mConfiguration.AccountName);
        }

        internal void HandleInvitation(Types.MultiplayerEvent eventType, string invitationId,
                                       MultiplayerInvitation invitation)
        {
            // Stash a reference to the invitation handler in case it is updated while we're handling
            // this callback.
            var currentHandler = mInvitationDelegate;

            if (currentHandler == null)
            {
                GooglePlayGames.OurUtils.Logger.d("Received " + eventType + " for invitation "
                    + invitationId + " but no handler was registered.");
                return;
            }

            if (eventType == Types.MultiplayerEvent.REMOVED)
            {
                GooglePlayGames.OurUtils.Logger.d("Ignoring REMOVED for invitation " + invitationId);
                return;
            }

            bool shouldAutolaunch = eventType == Types.MultiplayerEvent.UPDATED_FROM_APP_LAUNCH;

            // Copy the invitation into managed memory.
            Invitation invite = invitation.AsInvitation();
            PlayGamesHelperObject.RunOnGameThread(() =>
                currentHandler(invite, shouldAutolaunch));
        }

        /// <summary>
        /// Gets the user's email.
        /// </summary>
        /// <remarks>The email address returned is selected by the user from the accounts present
        /// on the device. There is no guarantee this uniquely identifies the player.
        /// For unique identification use the id property of the local player.
        /// The user can also choose to not select any email address, meaning it is not
        /// available.</remarks>
        /// <returns>The user email or null if not authenticated or the permission is
        /// not available.</returns>
        public string GetUserEmail()
        {
            if (!this.IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                return null;
            }

            return mTokenClient.GetEmail();
        }

        /// <summary>
        /// Returns an id token, which can be verified server side, if they are logged in.
        /// </summary>
        /// <param name="idTokenCallback"> A callback to be invoked after token is retrieved. Will be passed null value
        /// on failure. </param>
        /// <returns>The identifier token.</returns>
        public string GetIdToken()
        {
            if (!this.IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                return null;
            }
            return mTokenClient.GetIdToken();
        }

        /// <summary>
        /// Asynchronously retrieves the server auth code for this client.
        /// </summary>
        /// <remarks>Note: This function is currently only implemented for Android.</remarks>
        /// <param name="serverClientId">The Client ID.</param>
        /// <param name="callback">Callback for response.</param>
        public string GetServerAuthCode()
        {
            if (!this.IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                return null;
            }
            return mTokenClient.GetAuthCode();
        }

        public void GetAnotherServerAuthCode(bool reAuthenticateIfNeeded,
                                             Action<string> callback)
        {
            mTokenClient.GetAnotherServerAuthCode(reAuthenticateIfNeeded,
                                                  callback);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.IsAuthenticated"/>
        public bool IsAuthenticated()
        {
            lock (AuthStateLock)
            {
                return mAuthState == AuthState.Authenticated;
            }
        }

        public void LoadFriends(Action<bool> callback)
        {

            if (!IsAuthenticated())
            {
                GooglePlayGames.OurUtils.Logger.d("Cannot loadFriends when not authenticated");
                PlayGamesHelperObject.RunOnGameThread(() =>
                    callback(false));
                return;
            }

            // avoid calling excessively
            if (mFriends != null)
            {
                PlayGamesHelperObject.RunOnGameThread(() =>
                    callback(true));
                return;
            }

            mServices.PlayerManager().FetchFriends((status, players) =>
                {
                    if (status == ResponseStatus.Success ||
                        status == ResponseStatus.SuccessWithStale)
                    {
                        mFriends = players;
                        PlayGamesHelperObject.RunOnGameThread(() =>
                            callback(true));
                    }
                    else
                    {
                        mFriends = new List<Player>();
                        GooglePlayGames.OurUtils.Logger.e(
                            "Got " + status + " loading friends");
                        PlayGamesHelperObject.RunOnGameThread(() =>
                            callback(false));
                    }
                });
        }

        public IUserProfile[] GetFriends()
        {
            if (mFriends == null && !friendsLoading)
            {
                GooglePlayGames.OurUtils.Logger.w("Getting friends before they are loaded!!!");
                friendsLoading = true;
                LoadFriends((ok) =>
                    {
                        GooglePlayGames.OurUtils.Logger.d("loading: " + ok + " mFriends = " + mFriends);
                        if (!ok)
                        {
                            GooglePlayGames.OurUtils.Logger.e("Friends list did not load successfully." +
                                "  Disabling loading until re-authenticated");
                        }
                        friendsLoading = !ok;
                    });
            }
            return (mFriends == null) ? new IUserProfile[0] : mFriends.ToArray();
        }

        void MaybeFinishAuthentication()
        {
            Action<bool, string> localCallbacks = null;

            lock (AuthStateLock)
            {
                // Only proceed if both the fetch-self and fetch-achievements callback have
                // completed.
                if (mUser == null)
                {
                    GooglePlayGames.OurUtils.Logger.d("Auth not finished. User=" + mUser);
                    return;
                }

                GooglePlayGames.OurUtils.Logger.d("Auth finished. Proceeding.");
                // Null out the pending callbacks - we will be invoking any pending ones.
                localCallbacks = mPendingAuthCallbacks;
                mPendingAuthCallbacks = null;
                mAuthState = AuthState.Authenticated;
            }

            if (localCallbacks != null)
            {
                GooglePlayGames.OurUtils.Logger.d("Invoking Callbacks: " + localCallbacks);
                InvokeCallbackOnGameThread(localCallbacks, true, null);
            }
        }

        void PopulateUser(uint authGeneration, PlayerManager.FetchSelfResponse response)
        {
            GooglePlayGames.OurUtils.Logger.d("Populating User");

            if (authGeneration != mAuthGeneration)
            {
                GooglePlayGames.OurUtils.Logger.d("Received user callback after signout occurred, ignoring");
                return;
            }

            lock (AuthStateLock)
            {
                if (response.Status() != Status.ResponseStatus.VALID &&
                    response.Status() != Status.ResponseStatus.VALID_BUT_STALE)
                {
                    GooglePlayGames.OurUtils.Logger.e("Error retrieving user, signing out");
                    var localCallbacks = mPendingAuthCallbacks;
                    mPendingAuthCallbacks = null;

                    if (localCallbacks != null)
                    {
                        InvokeCallbackOnGameThread(localCallbacks, false, "Cannot load user profile");
                    }
                    SignOut();
                    return;
                }

                mUser = response.Self().AsPlayer();
                mFriends = null;
            }
            GooglePlayGames.OurUtils.Logger.d("Found User: " + mUser);
            GooglePlayGames.OurUtils.Logger.d("Maybe finish for User");
            MaybeFinishAuthentication();
        }

        private void HandleAuthTransition(Types.AuthOperation operation, Status.AuthStatus status)
        {
            GooglePlayGames.OurUtils.Logger.d("Starting Auth Transition. Op: " + operation + " status: " + status);
            lock (AuthStateLock)
            {
                switch (operation)
                {
                    case Types.AuthOperation.SIGN_IN:
                        if (status == Status.AuthStatus.VALID) {
                            uint currentAuthGeneration = mAuthGeneration;
                            mServices.PlayerManager().FetchSelf(
                                results => PopulateUser(currentAuthGeneration, results));
                        }
                        else {
                            // Auth failed
                            // The initial silent auth failed - take note of that and
                            // notify any pending silent-auth callbacks. If there are
                            // additional non-silent auth callbacks pending, attempt to auth
                            // by popping the Auth UI.
                            mAuthState = AuthState.Unauthenticated;
                            GooglePlayGames.OurUtils.Logger.d(
                                    "AuthState == " + mAuthState +
                                      " calling auth callbacks with failure");

                            // Noisy sign-in failed - report failure.
                            Action<bool, string> localCallbacks = mPendingAuthCallbacks;
                            mPendingAuthCallbacks = null;
                            InvokeCallbackOnGameThread(localCallbacks, false, "Authentication failed");
                        }
                        break;
                    case Types.AuthOperation.SIGN_OUT:
                        ToUnauthenticated();
                        break;
                    default:
                        GooglePlayGames.OurUtils.Logger.e("Unknown AuthOperation " + operation);
                        break;
                }
            }
        }

        private void ToUnauthenticated()
        {
            lock (AuthStateLock)
            {
                mUser = null;
                mFriends = null;
                mAuthState = AuthState.Unauthenticated;
                mTokenClient = clientImpl.CreateTokenClient(true);
                mAuthGeneration++;
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SignOut"/>
        public void SignOut()
        {
            ToUnauthenticated();

            if (GameServices() == null)
            {
                return;
            }

            mTokenClient.Signout();
            GameServices().SignOut();
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetUserId"/>
        public string GetUserId()
        {
            if (mUser == null)
            {
                return null;
            }

            return mUser.id;
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetUserDisplayName"/>
        public string GetUserDisplayName()
        {
            if (mUser == null)
            {
                return null;
            }

            return mUser.userName;
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetUserImageUrl"/>
        public string GetUserImageUrl()
        {
            if (mUser == null)
            {
                return null;
            }

            return mUser.AvatarURL;
        }

        public void SetGravityForPopups(Gravity gravity)
        {
            PlayGamesHelperObject.RunOnGameThread(() => 
             clientImpl.SetGravityForPopups(GetApiClient(), gravity));
        }


        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetPlayerStats"/>
        public void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
#if UNITY_ANDROID
            // Temporary fix to get SpendProbability until the
            // C++ SDK supports it.
            PlayGamesHelperObject.RunOnGameThread(() =>
                clientImpl.GetPlayerStats(GetApiClient(), callback));
#else
            mServices.StatsManager().FetchForPlayer((playerStatsResponse) => {
                // Translate native errors into CommonStatusCodes.
                CommonStatusCodes responseCode =
                    ConversionUtils.ConvertResponseStatusToCommonStatus(playerStatsResponse.Status());
                // Log errors.
                if (responseCode != CommonStatusCodes.Success &&
                responseCode != CommonStatusCodes.SuccessCached)
                {
                    GooglePlayGames.OurUtils.Logger.e("Error loading PlayerStats: " + playerStatsResponse.Status().ToString());
                }
                // Fill in the stats & call the callback.
                if (callback != null)
                {
                    if (playerStatsResponse.PlayerStats() != null)
                    {
                        // Copy the object out of the native interface so
                        // it will not be deleted before the callback is
                        // executed on the UI thread.
                        PlayerStats stats =
                            playerStatsResponse.PlayerStats().AsPlayerStats();
                        PlayGamesHelperObject.RunOnGameThread(() =>
                            callback(responseCode,stats));
                    }
                    else
                    {
                        PlayGamesHelperObject.RunOnGameThread(() =>
                            callback(responseCode, new PlayerStats()));
                    }
                }
            });
#endif
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadUsers"/>
        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            mServices.PlayerManager().FetchList(userIds,
                (nativeUsers) =>
                {
                    IUserProfile[] users = new IUserProfile[nativeUsers.Length];
                    for (int i = 0; i < users.Length; i++)
                    {
                        users[i] = nativeUsers[i].AsPlayer();
                    }
                    PlayGamesHelperObject.RunOnGameThread(() =>
                        callback(users));
                });
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadAchievements"/>
        public void LoadAchievements(Action<Achievement[]> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            if (!IsAuthenticated())
            {
                callback(null);
                return;
            }
            mServices.AchievementManager().FetchAll(
                response => {
                    if (response.Status() != Status.ResponseStatus.VALID &&
                        response.Status() != Status.ResponseStatus.VALID_BUT_STALE)
                    {
                        GooglePlayGames.OurUtils.Logger.e("Error retrieving achievements - check the log for more information. ");
                        callback(null);
                        return;
                    }

                    Achievement[] data = new Achievement[(int)response.Length()];
                    int i = 0;
                    foreach (var achievement in response)
                    {
                        using (achievement)
                        {
                            data[i++] = achievement.AsAchievement();
                        }
                    }
                    callback.Invoke(data);
                });

        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.UnlockAchievement"/>
        public void UnlockAchievement(string achId, Action<bool> callback)
        {
            Misc.CheckNotNull(achId);

            callback = AsOnGameThreadCallback(callback);

            InitializeGameServices();
            GameServices().AchievementManager().Unlock(achId);
            callback(true);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.RevealAchievement"/>
        public void RevealAchievement(string achId, Action<bool> callback)
        {
            Misc.CheckNotNull(achId);

            callback = AsOnGameThreadCallback(callback);

            InitializeGameServices();
            GameServices().AchievementManager().Reveal(achId);
            callback(true);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.IncrementAchievement"/>
        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
            Misc.CheckNotNull(achId);
            callback = AsOnGameThreadCallback(callback);

            InitializeGameServices();

            if (steps < 0)
            {
                GooglePlayGames.OurUtils.Logger.e("Attempted to increment by negative steps");
                callback(false);
                return;
            }

            GameServices().AchievementManager().Increment(achId, Convert.ToUInt32(steps));
            callback(true);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SetStepsAtLeast"/>
        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
            Misc.CheckNotNull(achId);
            callback = AsOnGameThreadCallback(callback);

            InitializeGameServices();

            if (steps < 0)
            {
                GooglePlayGames.OurUtils.Logger.e("Attempted to increment by negative steps");
                callback(false);
                return;
            }

            GameServices().AchievementManager().SetStepsAtLeast(achId, Convert.ToUInt32(steps));
            callback(true);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.ShowAchievementsUI"/>
        public void ShowAchievementsUI(Action<UIStatus> cb)
        {
            if (!IsAuthenticated())
            {
                return;
            }

            var callback = Callbacks.NoopUICallback;
            if (cb != null)
            {
                callback = (result) =>
                {
                    cb.Invoke((UIStatus)result);
                };
            }
            callback = AsOnGameThreadCallback(callback);

            GameServices().AchievementManager().ShowAllUI(callback);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LeaderboardMaxResults"/>
        public int LeaderboardMaxResults()
        {
            return GameServices().LeaderboardManager().LeaderboardMaxResults;
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.ShowLeaderboardUI"/>
        public void ShowLeaderboardUI(string leaderboardId, LeaderboardTimeSpan span,
            Action<UIStatus> cb)
        {
            if (!IsAuthenticated())
            {
                return;
            }

            Action<Status.UIStatus> callback = Callbacks.NoopUICallback;
            if (cb != null)
            {
                callback = (result) =>
                {
                    cb.Invoke((UIStatus)result);
                };
            }
            callback = AsOnGameThreadCallback(callback);
            if (leaderboardId == null)
            {
                GameServices().LeaderboardManager().ShowAllUI(callback);
            }
            else
            {
                GameServices().LeaderboardManager().ShowUI(leaderboardId, span, callback);
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadScores"/>
        public void LoadScores(string leaderboardId, LeaderboardStart start,
            int rowCount, LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            GameServices().LeaderboardManager().LoadLeaderboardData(
                leaderboardId, start, rowCount, collection, timeSpan,
                this.mUser.id, callback
            );
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadMoreScores"/>
        public void LoadMoreScores(ScorePageToken token, int rowCount,
            Action<LeaderboardScoreData> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            GameServices().LeaderboardManager().LoadScorePage(null,
                rowCount, token, callback);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SubmitScore"/>
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            if (!IsAuthenticated())
            {
                callback(false);
            }

            InitializeGameServices();

            if (leaderboardId == null)
            {
                throw new ArgumentNullException("leaderboardId");
            }

            GameServices().LeaderboardManager().SubmitScore(leaderboardId,
                score, null);
            // Score submissions cannot fail.
            callback(true);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SubmitScore"/>
        public void SubmitScore(string leaderboardId, long score, string metadata,
                                Action<bool> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            if (!IsAuthenticated())
            {
                callback(false);
            }

            InitializeGameServices();

            if (leaderboardId == null)
            {
                throw new ArgumentNullException("leaderboardId");
            }

            GameServices().LeaderboardManager().SubmitScore(leaderboardId,
                score, metadata);
            // Score submissions cannot fail.
            callback(true);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetRtmpClient"/>
        public IRealTimeMultiplayerClient GetRtmpClient()
        {
            if (!IsAuthenticated())
            {
                return null;
            }

            lock (GameServicesLock)
            {
                return mRealTimeClient;
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetTbmpClient"/>
        public ITurnBasedMultiplayerClient GetTbmpClient()
        {
            lock (GameServicesLock)
            {
                return mTurnBasedClient;
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetSavedGameClient"/>
        public ISavedGameClient GetSavedGameClient()
        {
            lock (GameServicesLock)
            {
                return mSavedGameClient;
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetEventsClient"/>
        public IEventsClient GetEventsClient()
        {
            lock (GameServicesLock)
            {
                return mEventsClient;
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetVideoClient"/>
        public IVideoClient GetVideoClient()
        {
            lock (GameServicesLock)
            {
                return mVideoClient;
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.RegisterInvitationDelegate"/>
        public void RegisterInvitationDelegate(InvitationReceivedDelegate invitationDelegate)
        {
            if (invitationDelegate == null)
            {
                mInvitationDelegate = null;
            }
            else
            {
                mInvitationDelegate = Callbacks.AsOnGameThreadCallback<Invitation, bool>(
                    (invitation, autoAccept) => invitationDelegate(invitation, autoAccept));
            }
        }

        public IntPtr GetApiClient()
        {
#if UNITY_ANDROID
            IntPtr ptr =
                Cwrapper.InternalHooks.InternalHooks_GetApiClient(mServices.AsHandle());

            return  ptr;
#else
            Debug.Log("GoogleAPIClient is not available on this platform");
            return IntPtr.Zero;
#endif
        }
    }
}
#endif
