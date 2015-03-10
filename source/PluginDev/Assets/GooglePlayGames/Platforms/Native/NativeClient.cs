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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.Native.PInvoke;
using GooglePlayGames.OurUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native {
public class NativeClient : IPlayGamesClient {

    private const string BridgeActivityClass = "com.google.games.bridge.NativeBridgeActivity";
    private const string LaunchBridgeMethod = "launchBridgeIntent";
    private const string LaunchBridgeSignature =
        "(Landroid/app/Activity;Landroid/content/Intent;)V";

    private enum AuthState {
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
    private volatile AppStateClient mAppStateClient;
    private volatile Action<Invitation, bool> mInvitationDelegate;
    private volatile Dictionary<String, Achievement> mAchievements = null;
    private volatile Player mUser = null;
    private volatile Action<bool> mPendingAuthCallbacks;
    private volatile Action<bool> mSilentAuthCallbacks;
    private volatile AuthState mAuthState = AuthState.Unauthenticated;
    private volatile uint mAuthGeneration = 0;
    private volatile bool mSilentAuthFailed = false;

    public NativeClient(PlayGamesClientConfiguration configuration) {
        PlayGamesHelperObject.CreateObject();
        this.mConfiguration = Misc.CheckNotNull(configuration);
    }

    private GameServices GameServices() {
        lock (GameServicesLock) {
            return mServices;
        }
    }

    public void Authenticate(Action<bool> callback, bool silent) {
        lock (AuthStateLock) {
            // If the user is already authenticated, just fire the callback, we don't need
            // any additional work.
            if (mAuthState == AuthState.Authenticated) {
                InvokeCallbackOnGameThread(callback, true);
                return;
            }

            // If this is silent auth, and silent auth already failed, there's no point in
            // trying again.
            if (mSilentAuthFailed && silent) {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            // Otherwise, hold the callback for invocation.
            if (callback != null) {
                if (silent) {
                    mSilentAuthCallbacks += callback;
                } else {
                    mPendingAuthCallbacks += callback;
                }
            }
        }

        // If game services are uninitialized, creating them will start a silent auth attempt.
        InitializeGameServices();

        if (!silent) {
            GameServices().StartAuthorizationUI();
        }
    }

    private static Action<bool> AsOnGameThreadCallback(Action<bool> callback) {
        if (callback == null) {
            return delegate { };
        }

        return result => InvokeCallbackOnGameThread(callback, result);
    }

    private static void InvokeCallbackOnGameThread(Action<bool> callback, bool success) {
        if (callback == null) {
            return;
        }

        PlayGamesHelperObject.RunOnGameThread(() => {
            Logger.d("Invoking user callback on game thread");
            callback(success);
        });
    }

    private void InitializeGameServices() {
        lock (GameServicesLock) {
            if (mServices != null) {
                return;
            }

            using (var builder = GameServicesBuilder.Create()) {
                using (var config = CreatePlatformConfiguration(builder)) {
                    // We need to make sure that the invitation delegate is registered before the
                    // services object is initialized - otherwise we might miss a callback if
                    // the game was opened because of a user accepting an invitation through
                    // a system notification.
                    RegisterInvitationDelegate(mConfiguration.InvitationDelegate);

                    builder.SetOnAuthFinishedCallback(HandleAuthTransition);
                    builder.SetOnTurnBasedMatchEventCallback((eventType, matchId, match)
                        => mTurnBasedClient.HandleMatchEvent(eventType, matchId, match));
                    builder.SetOnMultiplayerInvitationEventCallback(HandleInvitation);
                    if (mConfiguration.EnableSavedGames) {
                        builder.EnableSnapshots();
                    }
                    mServices = builder.Build(config);
                    mTurnBasedClient =
                        new NativeTurnBasedMultiplayerClient(this, new TurnBasedManager(mServices));

                    mTurnBasedClient.RegisterMatchDelegate(mConfiguration.MatchDelegate);

                    mRealTimeClient =
                        new NativeRealtimeMultiplayerClient(this, new RealtimeManager(mServices));

                    if (mConfiguration.EnableSavedGames) {
                        mSavedGameClient =
                            new NativeSavedGameClient(new SnapshotManager(mServices));
                    } else {
                        mSavedGameClient = new UnsupportedSavedGamesClient(
                            "You must enable saved games before it can be used. " +
                            "See PlayGamesClientConfiguration.Builder.EnableSavedGames.");
                    }

                    mAppStateClient = CreateAppStateClient();
                    mAuthState = AuthState.SilentPending;
                }
            }
        }
    }

    private AppStateClient CreateAppStateClient() {
        #if UNITY_ANDROID
        if (mConfiguration.EnableDeprecatedCloudSave) {
            return new AndroidAppStateClient(mServices);
        } else {
            return new UnsupportedAppStateClient(
                "You must explicitly enable cloud save - see " +
                "PlayGamesClientConfiguration.Builder.EnableDeprecatedCloudSave.");
        }
        #else
        return new UnsupportedAppStateClient("App State is not supported on this platform.");
        #endif
    }

    internal void HandleInvitation(Types.MultiplayerEvent eventType, string invitationId,
                                   MultiplayerInvitation invitation) {
        // Stash a reference to the invitation handler in case it is updated while we're handling
        // this callback.
        var currentHandler = mInvitationDelegate;

        if (currentHandler == null) {
            Logger.d("Received " + eventType + " for invitation "
            + invitationId + " but no handler was registered.");
            return;
        }

        if (eventType == Types.MultiplayerEvent.REMOVED) {
            Logger.d("Ignoring REMOVED for invitation " + invitationId);
            return;
        }

        bool shouldAutolaunch = eventType == Types.MultiplayerEvent.UPDATED_FROM_APP_LAUNCH;

        currentHandler(invitation.AsInvitation(), shouldAutolaunch);
    }

    #if UNITY_ANDROID
    internal AndroidJavaObject GetActivity() {
        using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            return jc.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    // Must be launched from the game thread (otherwise the classloader cannot locate the unity
    // java classes we require).
    private void LaunchBridgeIntent(IntPtr bridgedIntent) {
        object[] objectArray = new object[2];
        jvalue[] jArgs = AndroidJNIHelper.CreateJNIArgArray(objectArray);
        try {
            using (var bridgeClass = new AndroidJavaClass(BridgeActivityClass)) {
                using (var currentActivity = GetActivity()) {
                    // Unity no longer supports constructing an AndroidJavaObject using an IntPtr,
                    // so I have to manually munge with JNI here.
                    IntPtr methodId = AndroidJNI.GetStaticMethodID(bridgeClass.GetRawClass(),
                                          LaunchBridgeMethod,
                                          LaunchBridgeSignature);
                    jArgs[0].l = currentActivity.GetRawObject();
                    jArgs[1].l = bridgedIntent;
                    AndroidJNI.CallStaticVoidMethod(bridgeClass.GetRawClass(), methodId, jArgs);
                }
            }
        } finally {
            AndroidJNIHelper.DeleteJNIArgArray(objectArray, jArgs);
        }
    }
    #endif

    PlatformConfiguration CreatePlatformConfiguration(GameServicesBuilder builder) {
        #if UNITY_ANDROID
        var config = AndroidPlatformConfiguration.Create();
        config.EnableAppState();
        using (var activity = GetActivity()) {
            config.SetActivity(activity.GetRawObject());
            config.SetOptionalIntentHandlerForUI((intent) => {
                // Capture a global reference to the intent we are to show. This is required
                // since we are launching the intent from the game thread, and this callback
                // will return before this happens. If we do not hold onto a durable reference,
                // the code calling us will clean up the intent before we have a chance to display
                // it.
                IntPtr intentRef = AndroidJNI.NewGlobalRef(intent);

                PlayGamesHelperObject.RunOnGameThread(() => {
                    try {
                        LaunchBridgeIntent(intentRef);
                    } finally {
                        // Now that we've launched the intent, release the global reference.
                        AndroidJNI.DeleteGlobalRef(intentRef);
                    }
                });
            });
        }

        return config;
        #endif

        #if UNITY_IPHONE
        if (!GameInfo.IosClientIdInitialized()) {
            throw new System.InvalidOperationException("Could not locate the OAuth Client ID, " +
                "provide this by navigating to Google Play Games > iOS Setup");
        }

        var config = IosPlatformConfiguration.Create();
        config.SetClientId(GameInfo.IosClientId);
        return config;
        #endif
    }

    public bool IsAuthenticated() {
        lock (AuthStateLock) {
            return mAuthState == AuthState.Authenticated;
        }
    }

    private void PopulateAchievements(uint authGeneration,
                                      AchievementManager.FetchAllResponse response) {

        if (authGeneration != mAuthGeneration) {
            Logger.d("Received achievement callback after signout occurred, ignoring");
            return;
        }

        Logger.d("Populating Achievements");
        lock (AuthStateLock) {
            if (response.Status() != Status.ResponseStatus.VALID &&
                response.Status() != Status.ResponseStatus.VALID_BUT_STALE) {
                Logger.e("Error retrieving achievements - check the log for more information. " +
                    "Failing signin.");
                var localLoudAuthCallbacks = mPendingAuthCallbacks;
                mPendingAuthCallbacks = null;

                if (localLoudAuthCallbacks != null) {
                    InvokeCallbackOnGameThread(localLoudAuthCallbacks, false);
                }
                SignOut();
                return;
            }

            var achievements = new Dictionary<string, Achievement>();
            foreach (var achievement in response) {
                using (achievement) {
                    achievements[achievement.Id()] = achievement.AsAchievement();
                }
            }
            mAchievements = achievements;
        }

        Logger.d("Maybe finish for Achievements");
        MaybeFinishAuthentication();
    }

    void MaybeFinishAuthentication() {
        Action<bool> localCallbacks = null;

        lock (AuthStateLock) {
            // Only proceed if both the fetch-self and fetch-achievements callback have
            // completed.
            if (mUser == null || mAchievements == null) {
                Logger.d("Auth not finished. User=" + mUser + " achievements=" + mAchievements);
                return;
            }

            Logger.d("Auth finished. Proceeding.");
            // Null out the pending callbacks - we will be invoking any pending ones.
            localCallbacks = mPendingAuthCallbacks;
            mPendingAuthCallbacks = null;
            mAuthState = AuthState.Authenticated;
        }

        if (localCallbacks != null) {
            Logger.d("Invoking Callbacks: " + localCallbacks);
            InvokeCallbackOnGameThread(localCallbacks, true);
        }
    }

    void PopulateUser(uint authGeneration, PlayerManager.FetchSelfResponse response) {
        Logger.d("Populating User");

        if (authGeneration != mAuthGeneration) {
            Logger.d("Received user callback after signout occurred, ignoring");
            return;
        }

        lock (AuthStateLock) {
            if (response.Status() != Status.ResponseStatus.VALID &&
                response.Status() != Status.ResponseStatus.VALID_BUT_STALE) {
                Logger.e("Error retrieving user, signing out");
                var localCallbacks = mPendingAuthCallbacks;
                mPendingAuthCallbacks = null;

                if (localCallbacks != null) {
                    InvokeCallbackOnGameThread(localCallbacks, false);
                }
                SignOut();
                return;
            }

            mUser = response.Self().AsPlayer();
        }
        Logger.d("Found User: " + mUser);
        Logger.d("Maybe finish for User");
        MaybeFinishAuthentication();
    }

    private void HandleAuthTransition(Types.AuthOperation operation, Status.AuthStatus status) {
        Logger.d("Starting Auth Transition. Op: " + operation + " status: " + status);
        lock (AuthStateLock) {
            switch (operation) {
                case Types.AuthOperation.SIGN_IN:
                    if (status == Status.AuthStatus.VALID) {
                        // If sign-in succeeded, treat any silent auth callbacks the same way
                        // we would treat loud ones.
                        if (mSilentAuthCallbacks != null) {
                            mPendingAuthCallbacks += mSilentAuthCallbacks;
                            mSilentAuthCallbacks = null;
                        }

                        uint currentAuthGeneration = mAuthGeneration;
                        mServices.AchievementManager().FetchAll(
                            results => PopulateAchievements(currentAuthGeneration, results));
                        mServices.PlayerManager().FetchSelf(
                            results => PopulateUser(currentAuthGeneration, results));
                    } else {
                        // Auth failed
                        if (mAuthState == AuthState.SilentPending) {
                            // The initial silent auth failed - take note of that and
                            // notify any pending silent-auth callbacks. If there are
                            // additional non-silent auth callbacks pending, attempt to auth
                            // by popping the Auth UI.
                            mSilentAuthFailed = true;
                            mAuthState = AuthState.Unauthenticated;
                            var silentCallbacks = mSilentAuthCallbacks;
                            mSilentAuthCallbacks = null;
                            InvokeCallbackOnGameThread(silentCallbacks, false);
                            if (mPendingAuthCallbacks != null) {
                                GameServices().StartAuthorizationUI();
                            }
                        } else {
                            // Noisy sign-in failed - report failure.
                            Action<bool> localCallbacks = mPendingAuthCallbacks;
                            mPendingAuthCallbacks = null;
                            InvokeCallbackOnGameThread(localCallbacks, false);
                        }
                    }
                    break;
                case Types.AuthOperation.SIGN_OUT:
                    mAuthState = AuthState.Unauthenticated;
                    break;
                default:
                    Logger.e("Unknown AuthOperation " + operation);
                    break;
            }
        }
    }

    private void ToUnauthenticated() {
        lock (AuthStateLock) {
            mUser = null;
            mAchievements = null;
            mAuthState = AuthState.Unauthenticated;
            mAuthGeneration++;
        }
    }

    public void SignOut() {
        ToUnauthenticated();

        if (GameServices() == null) {
            return;
        }

        GameServices().SignOut();
    }

    public string GetUserId() {
        if (mUser == null) {
            return null;
        }

        return mUser.PlayerId;
    }

    public string GetUserDisplayName() {
        if (mUser == null) {
            return null;
        }

        return mUser.DisplayName;
    }

    public string GetUserImageUrl() {
        if (mUser == null) {
          return null;
        }

        return mUser.AvatarURL;
    }

    public Achievement GetAchievement(string achId) {
        if (mAchievements == null || !mAchievements.ContainsKey(achId)) {
            return null;
        }

        return mAchievements[achId];
    }

    public void UnlockAchievement(string achId, Action<bool> callback) {
        UpdateAchievement("Unlock", achId, callback, a => a.IsUnlocked,
            a => {
                a.IsUnlocked = true;
                GameServices().AchievementManager().Unlock(achId);
            });
    }

    public void RevealAchievement(string achId, Action<bool> callback) {
        UpdateAchievement("Reveal", achId, callback, a => a.IsRevealed,
            a => {
                a.IsRevealed = true;
                GameServices().AchievementManager().Reveal(achId);
            });
    }

    private void UpdateAchievement(string updateType, string achId, Action<bool> callback,
                                   Predicate<Achievement> alreadyDone, Action<Achievement> updateAchievment) {
        callback = AsOnGameThreadCallback(callback);

        Misc.CheckNotNull(achId);

        InitializeGameServices();

        var achievement = GetAchievement(achId);

        if (achievement == null) {
            Logger.d("Could not " + updateType + ", no achievement with ID " + achId);
            callback(false);
            return;
        }

        if (alreadyDone(achievement)) {
            Logger.d("Did not need to perform " + updateType + ": " + "on achievement " + achId);
            callback(true);
            return;
        }

        Logger.d("Performing " + updateType + " on " + achId);
        updateAchievment(achievement);
        // The native SDK never fails.
        callback(true);
    }

    public void IncrementAchievement(string achId, int steps, Action<bool> callback) {
        Misc.CheckNotNull(achId);
        callback = AsOnGameThreadCallback(callback);

        InitializeGameServices();

        var achievement = GetAchievement(achId);
        if (achievement == null) {
            Logger.e("Could not increment, no achievement with ID " + achId);
            callback(false);
            return;
        }

        if (!achievement.IsIncremental) {
            Logger.e("Could not increment, achievement with ID " + achId + " was not incremental");
            callback(false);
            return;
        }

        if (steps < 0) {
            Logger.e("Attempted to increment by negative steps");
            callback(false);
            return;
        }

        GameServices().AchievementManager().Increment(achId, Convert.ToUInt32(steps));
        callback(true);
    }

    public void ShowAchievementsUI() {
        if (!IsAuthenticated()) {
            return;
        }

        GameServices().AchievementManager().ShowAllUI(Callbacks.NoopUICallback);
    }

    public void ShowLeaderboardUI(string leaderboardId) {
        if (!IsAuthenticated()) {
            return;
        }

        if (leaderboardId == null) {
            GameServices().LeaderboardManager().ShowAllUI(Callbacks.NoopUICallback);
        } else {
            GameServices().LeaderboardManager().ShowUI(leaderboardId, Callbacks.NoopUICallback);
        }
    }

    public void SubmitScore(string leaderboardId, long score, Action<bool> callback) {
        callback = AsOnGameThreadCallback(callback);
        if (!IsAuthenticated()) {
            callback(false);
        }

        InitializeGameServices();

        if (leaderboardId == null) {
            throw new ArgumentNullException("Leaderboard ID was null");
        }

        GameServices().LeaderboardManager().SubmitScore(leaderboardId, score);
        // Score submissions cannot fail.
        callback(true);
    }

    public void LoadState(int slot, OnStateLoadedListener listener) {
        Misc.CheckNotNull(listener);
        lock (GameServicesLock) {
            if (mAuthState != AuthState.Authenticated) {
                Logger.e("You can only call LoadState after the user has successfully logged in");
                listener.OnStateLoaded(false, slot, null);
            }

            mAppStateClient.LoadState(slot, listener);
        }
    }

    public void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
        Misc.CheckNotNull(listener);

        lock (GameServicesLock) {
            if (mAuthState != AuthState.Authenticated) {
                Logger.e("You can only call UpdateState after the user has successfully logged in");
                listener.OnStateSaved(false, slot);
            }

            mAppStateClient.UpdateState(slot, data, listener);
        }
    }

    public GooglePlayGames.BasicApi.Multiplayer.IRealTimeMultiplayerClient GetRtmpClient() {
        if (!IsAuthenticated()) {
            return null;
        }

        lock (GameServicesLock) {
            return mRealTimeClient;
        }
    }

    public GooglePlayGames.BasicApi.Multiplayer.ITurnBasedMultiplayerClient GetTbmpClient() {
        lock (GameServicesLock) {
            return mTurnBasedClient;
        }
    }

    public GooglePlayGames.BasicApi.SavedGame.ISavedGameClient GetSavedGameClient() {
        lock (GameServicesLock) {
            return mSavedGameClient;
        }
    }

    public void RegisterInvitationDelegate(InvitationReceivedDelegate invitationDelegate) {
        if (invitationDelegate == null) {
            mInvitationDelegate = null;
        } else {
            mInvitationDelegate = Callbacks.AsOnGameThreadCallback<Invitation, bool>(
                (invitation, autoAccept) => invitationDelegate(invitation, autoAccept));
        }
    }
}
}
#endif
