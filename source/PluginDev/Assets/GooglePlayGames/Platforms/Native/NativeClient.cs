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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

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
    using GooglePlayGames.BasicApi.Quests;
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
            Authenticated,
            SilentPending
        }

        private readonly object GameServicesLock = new object();
        private readonly object AuthStateLock = new object();

        private readonly PlayGamesClientConfiguration mConfiguration;
        private GameServices mServices;
        private volatile NativeTurnBasedMultiplayerClient mTurnBasedClient;
        private volatile NativeRealtimeMultiplayerClient mRealTimeClient;
        private volatile ISavedGameClient mSavedGameClient;
        private volatile IEventsClient mEventsClient;
        private volatile IQuestsClient mQuestsClient;
        private volatile TokenClient mTokenClient;
        private volatile Action<Invitation, bool> mInvitationDelegate;
        private volatile Dictionary<String, Achievement> mAchievements = null;
        private volatile Player mUser = null;
        private volatile List<Player> mFriends = null;
        private volatile Action<bool> mPendingAuthCallbacks;
        private volatile Action<bool> mSilentAuthCallbacks;
        private volatile AuthState mAuthState = AuthState.Unauthenticated;
        private volatile uint mAuthGeneration = 0;
        private volatile bool mSilentAuthFailed = false;
        private volatile bool friendsLoading = false;

        private string rationale;

        private int webclientWarningFreq = 100000;
        private int noWebClientIdWarningCount = 0;

        internal NativeClient(PlayGamesClientConfiguration configuration,
            IClientImpl clientImpl)
        {
            PlayGamesHelperObject.CreateObject();
            this.mConfiguration = Misc.CheckNotNull(configuration);
            this.clientImpl = clientImpl;
            this.rationale = configuration.PermissionRationale;
            if (string.IsNullOrEmpty(this.rationale))
            {
                this.rationale = "Select email address to send to this game or hit cancel to not share.";
            }
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
        public void Authenticate(Action<bool> callback, bool silent)
        {
            lock (AuthStateLock)
            {
                // If the user is already authenticated, just fire the callback, we don't need
                // any additional work.
                if (mAuthState == AuthState.Authenticated)
                {
                    InvokeCallbackOnGameThread(callback, true);
                    return;
                }

                // If this is silent auth, and silent auth already failed, there's no point in
                // trying again.
                if (mSilentAuthFailed && silent)
                {
                    InvokeCallbackOnGameThread(callback, false);
                    return;
                }

                // Otherwise, hold the callback for invocation.
                if (callback != null)
                {
                    if (silent)
                    {
                        mSilentAuthCallbacks += callback;
                    }
                    else
                    {
                        mPendingAuthCallbacks += callback;
                    }
                }
            }

            // If game services are uninitialized, creating them will start a silent auth attempt.
            InitializeGameServices();

            // reset friends loading flag
            friendsLoading = false;

            if (!silent)
            {
                GameServices().StartAuthorizationUI();
            }
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
                    using (var config = clientImpl.CreatePlatformConfiguration())
                    {
                        // We need to make sure that the invitation delegate is registered before the
                        // services object is initialized - otherwise we might miss a callback if
                        // the game was opened because of a user accepting an invitation through
                        // a system notification.
                        RegisterInvitationDelegate(mConfiguration.InvitationDelegate);
                        builder.SetOnAuthFinishedCallback(HandleAuthTransition);
                        builder.SetOnTurnBasedMatchEventCallback((eventType, matchId, match) => mTurnBasedClient.HandleMatchEvent(eventType, matchId, match));
                        builder.SetOnMultiplayerInvitationEventCallback(HandleInvitation);
                        if (mConfiguration.EnableSavedGames)
                        {
                            builder.EnableSnapshots();
                        }
                        if (mConfiguration.RequireGooglePlus)
                        {
                            builder.RequireGooglePlus();
                        }
                        string[] scopes = mConfiguration.Scopes;
                        for (int i = 0; i < scopes.Length; i++) {
                            builder.AddOauthScope(scopes[i]);
                        }
                        Debug.Log("Building GPG services, implicitly attempts silent auth");
                        mAuthState = AuthState.SilentPending;
                        mServices = builder.Build(config);
                        mEventsClient = new NativeEventClient(new EventManager(mServices));
                        mQuestsClient = new NativeQuestClient(new QuestManager(mServices));
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

                        mAuthState = AuthState.SilentPending;
                        mTokenClient = clientImpl.CreateTokenClient(
                            (mUser == null) ? null : mUser.id, false);
                    }
                }
            }
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

            mTokenClient.SetRationale(rationale);
            return mTokenClient.GetEmail();
        }

        /// <summary>
        /// Gets the user's email with a callback.
        /// </summary>
        /// <remarks>The email address returned is selected by the user from the accounts present
        /// on the device. There is no guarantee this uniquely identifies the player.
        /// For unique identification use the id property of the local player.
        /// The user can also choose to not select any email address, meaning it is not
        /// available.</remarks>
        /// <param name="callback">The callback with a status code of the request,
        /// and string which is the email. It can be null.</param>
        public void GetUserEmail(Action<CommonStatusCodes, string> callback)
        {
            if (!this.IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                if (callback != null)
                {
                    PlayGamesHelperObject.RunOnGameThread(() =>
                        callback(CommonStatusCodes.SignInRequired, null));
                    return;
                }
            }
            mTokenClient.SetRationale(rationale);
            mTokenClient.GetEmail((status, email) =>
                PlayGamesHelperObject.RunOnGameThread(()=>callback(status,email)));
        }

        /// <summary>Gets the access token currently associated with the Unity activity.</summary>
        /// <returns>The OAuth 2.0 access token.</returns>
        [Obsolete("Use GetServerAuthCode() then exchange it for a token")]
        public string GetAccessToken()
        {
            if (!this.IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                return null;
            }

            if(!GameInfo.WebClientIdInitialized())
            {
                //don't spam the log, only do this every webclientWarningFreq times.
                if (noWebClientIdWarningCount++ % webclientWarningFreq == 0)
                {
                    Debug.LogError("Web client ID has not been set, cannot request access token.");
                    // avoid int overflow
                    noWebClientIdWarningCount = (noWebClientIdWarningCount/ webclientWarningFreq) + 1;
                }
                return null;
            }
            mTokenClient.SetRationale(rationale);
            return mTokenClient.GetAccessToken();
        }

        /// <summary>
        /// Returns an id token, which can be verified server side, if they are logged in.
        /// </summary>
        /// <param name="idTokenCallback"> A callback to be invoked after token is retrieved. Will be passed null value
        /// on failure. </param>
        /// <returns>The identifier token.</returns>
        [Obsolete("Use GetServerAuthCode() then exchange it for a token")]
        public void GetIdToken(Action<string> idTokenCallback)
        {
            if (!this.IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                PlayGamesHelperObject.RunOnGameThread(() =>
                    idTokenCallback(null));
            }

            if(!GameInfo.WebClientIdInitialized())
            {
                //don't spam the log, only do this every webclientWarningFreq times.
                if (noWebClientIdWarningCount++ % webclientWarningFreq == 0)
                {
                    Debug.LogError("Web client ID has not been set, cannot request id token.");
                    // avoid int overflow
                    noWebClientIdWarningCount = (noWebClientIdWarningCount/ webclientWarningFreq) + 1;
                }
                PlayGamesHelperObject.RunOnGameThread(() =>
                    idTokenCallback(null));
            }
            mTokenClient.SetRationale(rationale);
            mTokenClient.GetIdToken(GameInfo.WebClientId,
                AsOnGameThreadCallback(idTokenCallback));
        }

        /// <summary>
        /// Asynchronously retrieves the server auth code for this client.
        /// </summary>
        /// <remarks>Note: This function is currently only implemented for Android.</remarks>
        /// <param name="serverClientId">The Client ID.</param>
        /// <param name="callback">Callback for response.</param>
        public void GetServerAuthCode(string serverClientId, Action<CommonStatusCodes, string> callback)
        {
            mServices.FetchServerAuthCode(serverClientId, (serverAuthCodeResponse) => {
                // Translate native errors into CommonStatusCodes.
                CommonStatusCodes responseCode =
                    ConversionUtils.ConvertResponseStatusToCommonStatus(serverAuthCodeResponse.Status());
                // Log errors.
                if (responseCode != CommonStatusCodes.Success &&
                    responseCode != CommonStatusCodes.SuccessCached)
                {
                    OurUtils.Logger.e("Error loading server auth code: " + serverAuthCodeResponse.Status().ToString());
                }
                // Fill in the code & call the callback.
                if (callback != null)
                {
                    // copy the auth code into managed memory before posting
                    // the callback.
                    string authCode = serverAuthCodeResponse.Code();
                    PlayGamesHelperObject.RunOnGameThread(() =>
                        callback(responseCode, authCode));
                }
            });
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

        private void PopulateAchievements(uint authGeneration,
                                          AchievementManager.FetchAllResponse response)
        {

            if (authGeneration != mAuthGeneration)
            {
                GooglePlayGames.OurUtils.Logger.d("Received achievement callback after signout occurred, ignoring");
                return;
            }

            GooglePlayGames.OurUtils.Logger.d("Populating Achievements, status = " + response.Status());
            lock (AuthStateLock)
            {
                if (response.Status() != Status.ResponseStatus.VALID &&
                    response.Status() != Status.ResponseStatus.VALID_BUT_STALE)
                {
                    GooglePlayGames.OurUtils.Logger.e("Error retrieving achievements - check the log for more information. " +
                        "Failing signin.");
                    var localLoudAuthCallbacks = mPendingAuthCallbacks;
                    mPendingAuthCallbacks = null;

                    if (localLoudAuthCallbacks != null)
                    {
                        InvokeCallbackOnGameThread(localLoudAuthCallbacks, false);
                    }
                    SignOut();
                    return;
                }

                var achievements = new Dictionary<string, Achievement>();
                foreach (var achievement in response)
                {
                    using (achievement)
                    {
                        achievements[achievement.Id()] = achievement.AsAchievement();
                    }
                }
                GooglePlayGames.OurUtils.Logger.d("Found " + achievements.Count + " Achievements");
                mAchievements = achievements;
            }

            GooglePlayGames.OurUtils.Logger.d("Maybe finish for Achievements");
            MaybeFinishAuthentication();
        }

        void MaybeFinishAuthentication()
        {
            Action<bool> localCallbacks = null;

            lock (AuthStateLock)
            {
                // Only proceed if both the fetch-self and fetch-achievements callback have
                // completed.
                if (mUser == null || mAchievements == null)
                {
                    GooglePlayGames.OurUtils.Logger.d("Auth not finished. User=" + mUser + " achievements=" + mAchievements);
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
                InvokeCallbackOnGameThread(localCallbacks, true);
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
                        InvokeCallbackOnGameThread(localCallbacks, false);
                    }
                    SignOut();
                    return;
                }

                mUser = response.Self().AsPlayer();
                mFriends = null;
                mTokenClient = clientImpl.CreateTokenClient(mUser.id, true);
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
                        if (status == Status.AuthStatus.VALID)
                        {
                            // If sign-in succeeded, treat any silent auth callbacks the same way
                            // we would treat loud ones.
                            if (mSilentAuthCallbacks != null)
                            {
                                mPendingAuthCallbacks += mSilentAuthCallbacks;
                                mSilentAuthCallbacks = null;
                            }

                            uint currentAuthGeneration = mAuthGeneration;
                            mServices.AchievementManager().FetchAll(
                                results => PopulateAchievements(currentAuthGeneration, results));
                            mServices.PlayerManager().FetchSelf(
                                results => PopulateUser(currentAuthGeneration, results));
                        }
                        else
                        {
                            // Auth failed
                            if (mAuthState == AuthState.SilentPending)
                            {
                                // The initial silent auth failed - take note of that and
                                // notify any pending silent-auth callbacks. If there are
                                // additional non-silent auth callbacks pending, attempt to auth
                                // by popping the Auth UI.
                                mSilentAuthFailed = true;
                                mAuthState = AuthState.Unauthenticated;
                                var silentCallbacks = mSilentAuthCallbacks;
                                mSilentAuthCallbacks = null;
                                GooglePlayGames.OurUtils.Logger.d(
                                    "Invoking callbacks, AuthState changed " +
                                    "from silentPending to Unauthenticated.");

                                InvokeCallbackOnGameThread(silentCallbacks, false);
                                if (mPendingAuthCallbacks != null)
                                {
                                    GooglePlayGames.OurUtils.Logger.d(
                                        "there are pending auth callbacks - starting AuthUI");
                                    GameServices().StartAuthorizationUI();
                                }
                            }
                            else
                            {
                                GooglePlayGames.OurUtils.Logger.d(
                                        "AuthState == " + mAuthState +
                                          " calling auth callbacks with failure");

                                // make sure we are not paused
                                UnpauseUnityPlayer();

                                // Noisy sign-in failed - report failure.
                                Action<bool> localCallbacks = mPendingAuthCallbacks;
                                mPendingAuthCallbacks = null;
                                InvokeCallbackOnGameThread(localCallbacks, false);
                            }
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

#if UNITY_IOS || UNITY_IPHONE
    [System.Runtime.InteropServices.DllImport("__Internal")]
    internal static extern void UnpauseUnityPlayer();
#else
    private void UnpauseUnityPlayer()
    {
        // don't do anything.
    }
#endif

        private void ToUnauthenticated()
        {
            lock (AuthStateLock)
            {
                mUser = null;
                mFriends = null;
                mAchievements = null;
                mAuthState = AuthState.Unauthenticated;
                mTokenClient = clientImpl.CreateTokenClient(null, true);
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
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetAchievement"/>
        public Achievement GetAchievement(string achId)
        {
            if (mAchievements == null || !mAchievements.ContainsKey(achId))
            {
                return null;
            }

            return mAchievements[achId];
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadAchievements"/>
        public void LoadAchievements(Action<Achievement[]> callback)
        {
            Achievement[] data = new Achievement[mAchievements.Count];
            mAchievements.Values.CopyTo (data, 0);
            PlayGamesHelperObject.RunOnGameThread(() =>
                callback.Invoke (data));
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.UnlockAchievement"/>
        public void UnlockAchievement(string achId, Action<bool> callback)
        {
            UpdateAchievement("Unlock", achId, callback, a => a.IsUnlocked,
                a =>
                {
                    a.IsUnlocked = true;
                    GameServices().AchievementManager().Unlock(achId);
                });
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.RevealAchievement"/>
        public void RevealAchievement(string achId, Action<bool> callback)
        {
            UpdateAchievement("Reveal", achId, callback, a => a.IsRevealed,
                a =>
                {
                    a.IsRevealed = true;
                    GameServices().AchievementManager().Reveal(achId);
                });
        }

        private void UpdateAchievement(string updateType, string achId, Action<bool> callback,
                                       Predicate<Achievement> alreadyDone, Action<Achievement> updateAchievment)
        {
            callback = AsOnGameThreadCallback(callback);

            Misc.CheckNotNull(achId);

            InitializeGameServices();

            var achievement = GetAchievement(achId);

            if (achievement == null)
            {
                GooglePlayGames.OurUtils.Logger.d("Could not " + updateType + ", no achievement with ID " + achId);
                callback(false);
                return;
            }

            if (alreadyDone(achievement))
            {
                GooglePlayGames.OurUtils.Logger.d("Did not need to perform " + updateType + ": " + "on achievement " + achId);
                callback(true);
                return;
            }

            GooglePlayGames.OurUtils.Logger.d("Performing " + updateType + " on " + achId);
            updateAchievment(achievement);

            GameServices().AchievementManager().Fetch(achId, rsp =>
                {
                    if (rsp.Status() == Status.ResponseStatus.VALID)
                    {
                        mAchievements.Remove(achId);
                        mAchievements.Add(achId, rsp.Achievement().AsAchievement());
                        callback(true);
                    }
                    else
                    {
                        GooglePlayGames.OurUtils.Logger.e("Cannot refresh achievement " + achId + ": " +
                            rsp.Status());
                        callback(false);
                    }
                });
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.IncrementAchievement"/>
        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
            Misc.CheckNotNull(achId);
            callback = AsOnGameThreadCallback(callback);

            InitializeGameServices();

            var achievement = GetAchievement(achId);
            if (achievement == null)
            {
                GooglePlayGames.OurUtils.Logger.e("Could not increment, no achievement with ID " + achId);
                callback(false);
                return;
            }

            if (!achievement.IsIncremental)
            {
                GooglePlayGames.OurUtils.Logger.e("Could not increment, achievement with ID " + achId + " was not incremental");
                callback(false);
                return;
            }

            if (steps < 0)
            {
                GooglePlayGames.OurUtils.Logger.e("Attempted to increment by negative steps");
                callback(false);
                return;
            }

            GameServices().AchievementManager().Increment(achId, Convert.ToUInt32(steps));
            GameServices().AchievementManager().Fetch(achId, rsp =>
                {
                    if (rsp.Status() == Status.ResponseStatus.VALID)
                    {
                        mAchievements.Remove(achId);
                        mAchievements.Add(achId, rsp.Achievement().AsAchievement());
                        callback(true);
                    }
                    else
                    {
                        GooglePlayGames.OurUtils.Logger.e("Cannot refresh achievement " + achId + ": " +
                            rsp.Status());
                        callback(false);
                    }
                });
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SetStepsAtLeast"/>
        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
            Misc.CheckNotNull(achId);
            callback = AsOnGameThreadCallback(callback);

            InitializeGameServices();

            var achievement = GetAchievement(achId);
            if (achievement == null)
            {
                GooglePlayGames.OurUtils.Logger.e("Could not increment, no achievement with ID " + achId);
                callback(false);
                return;
            }

            if (!achievement.IsIncremental)
            {
                GooglePlayGames.OurUtils.Logger.e("Could not increment, achievement with ID " +
                    achId + " is not incremental");
                callback(false);
                return;
            }

            if (steps < 0)
            {
                GooglePlayGames.OurUtils.Logger.e("Attempted to increment by negative steps");
                callback(false);
                return;
            }

            GameServices().AchievementManager().SetStepsAtLeast(achId, Convert.ToUInt32(steps));
            GameServices().AchievementManager().Fetch(achId, rsp =>
                {
                    if (rsp.Status() == Status.ResponseStatus.VALID)
                    {
                        mAchievements.Remove(achId);
                        mAchievements.Add(achId, rsp.Achievement().AsAchievement());
                        callback(true);
                    }
                    else
                    {
                        GooglePlayGames.OurUtils.Logger.e("Cannot refresh achievement " + achId + ": " +
                            rsp.Status());
                        callback(false);
                    }
                });
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
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetQuestsClient"/>
        public IQuestsClient GetQuestsClient()
        {
            lock (GameServicesLock)
            {
                return mQuestsClient;
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

        /// <summary>Gets the client OAuth 2.0 bearer token.</summary>
        /// <returns>A string representing the bearer token if the client is valid; otherwise,
        /// returns null.</returns>
        public string GetToken()
        {
            if (mTokenClient != null)
            {
                return mTokenClient.GetAccessToken();
            }
            return null;
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
