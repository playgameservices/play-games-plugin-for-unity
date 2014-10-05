/*
 * Copyright (C) 2013 Google Inc.
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

#if UNITY_ANDROID
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.Android {
    public class AndroidClient : IPlayGamesClient {
        GameHelperManager mGHManager = null;
        
        // In what state of the authentication process are we?
        enum AuthState {
            NoAuth, // not authenticated
            AuthPending, // we want to authenticate, but GameHelper is busy
            InProgress, // we are authenticating
            LoadingAchs, // we are signed in and are doing the initial achievement load
            Done // we are authenticated!
        };
        AuthState mAuthState = AuthState.NoAuth;

        // are we trying silent authentication? If so, then we can't show UIs in the process:
        // we have to fail instead
        bool mSilentAuth = false;

        // user's ID and display name (retrieved on sign in)
        string mUserId = null, mUserDisplayName = null;

        // the auth callback that we have to call at the end of the auth process
        System.Action<bool> mAuthCallback = null;

        // the achievements we've loaded
        AchievementBank mAchievementBank = new AchievementBank();

        // Sometimes we have to execute an action on the UI thread involving GamesClient,
        // but we might hit that unlucky moment when GamesClient is still in the process
        // of connecting and can't take API calls. So, if that happens, we queue the
        // actions here and execute when we get onSignInSucceeded or onSignInFailed
        List<Action> mActionsPendingSignIn = new List<Action>();
        
        // Are we currently in the process of signing out?
        private bool mSignOutInProgress = false;

        // Result code for child activities whose result we don't care about
        const int RC_UNUSED = 9999;
        
        // RTMP client and TBMP clients
        AndroidRtmpClient mRtmpClient;
        AndroidTbmpClient mTbmpClient;
        
        // invitation delegate
        InvitationReceivedDelegate mInvitationDelegate = null;
        
        // have we installed an invitation listener?
        bool mRegisteredInvitationListener = false;
        
        // the invitation we received via notification. We store it here while we don't have
        // an invitation delegate to deliver it to; when we get one, we deliver it and forget it.
        Invitation mInvitationFromNotification = null;
        
        public AndroidClient() {
            mRtmpClient = new AndroidRtmpClient(this);
            mTbmpClient = new AndroidTbmpClient(this);
            RunOnUiThread(() => {
                Logger.d("Initializing Android Client.");
                Logger.d("Creating GameHelperManager to manage GameHelper.");
                mGHManager = new GameHelperManager(this);
                
                // make sure that when game stops, we clean up the real time multiplayer room
                mGHManager.AddOnStopDelegate(mRtmpClient.OnStop);
            });
            // now we wait for the result of the initial auth, which will trigger
            // a call to either OnSignInSucceeded or OnSignInFailed
        }

        // called from game thread
        public void Authenticate(System.Action<bool> callback, bool silent) {
            if (mAuthState != AuthState.NoAuth) {
                Logger.w("Authenticate() called while an authentication process was active. " + 
                        mAuthState);
                mAuthCallback = callback;
                return;
            }

            // make sure the helper GameObject is ready (we use it for the auth callback)
            Logger.d("Making sure PlayGamesHelperObject is ready.");
            PlayGamesHelperObject.CreateObject();
            Logger.d("PlayGamesHelperObject created.");

            mSilentAuth = silent;
            Logger.d("AUTH: starting auth process, silent=" + mSilentAuth);
            RunOnUiThread(() => {
                switch (mGHManager.State) {
                    case GameHelperManager.ConnectionState.Connected:
                        Logger.d("AUTH: already connected! Proceeding to achievement load phase.");
                        mAuthCallback = callback;
                        DoInitialAchievementLoad();
                        break;
                    case GameHelperManager.ConnectionState.Connecting:
                        Logger.d("AUTH: connection in progress; auth now pending.");
                        mAuthCallback = callback;
                        mAuthState = AuthState.AuthPending;
                        // we'll do the right thing in OnSignInSucceeded/Failed
                        break;
                    default:
                        mAuthCallback = callback;
                        if (mSilentAuth) {
                            Logger.d("AUTH: not connected and silent=true, so failing.");
                            mAuthState = AuthState.NoAuth;
                            InvokeAuthCallback(false);
                        } else {
                            Logger.d("AUTH: not connected and silent=false, so starting flow.");
                            mAuthState = AuthState.InProgress;
                            mGHManager.BeginUserInitiatedSignIn();
                            // we'll do the right thing in OnSignInSucceeded/Failed
                        }
                        break;
                }
            });
        }

        // call from UI thread only!
        private void DoInitialAchievementLoad() {
            Logger.d("AUTH: Now performing initial achievement load...");
            mAuthState = AuthState.LoadingAchs;            
            mGHManager.CallGmsApiWithResult("games.Games", "Achievements", "load",
                    new OnAchievementsLoadedResultProxy(this), false);
            Logger.d("AUTH: Initial achievement load call made.");
        }
        
        // UI thread
        private void OnAchievementsLoaded(int statusCode, AndroidJavaObject buffer) {
            if (mAuthState == AuthState.LoadingAchs) {
                Logger.d("AUTH: Initial achievement load finished.");

                if (statusCode == JavaConsts.STATUS_OK || 
                        statusCode == JavaConsts.STATUS_STALE_DATA ||
                        statusCode == JavaConsts.STATUS_DEFERRED) {
                    // successful load (either from network or local cache)
                    Logger.d("Processing achievement buffer.");
                    mAchievementBank.ProcessBuffer(buffer);
                    
                    Logger.d("Closing achievement buffer.");
                    buffer.Call("close");
                    
                    Logger.d("AUTH: Auth process complete!");
                    mAuthState = AuthState.Done;
                    InvokeAuthCallback(true);
                    
                    // inform the RTMP client and TBMP clients that sign in suceeded
                    CheckForConnectionExtras();
                    mRtmpClient.OnSignInSucceeded();
                    mTbmpClient.OnSignInSucceeded();
                } else {
                    Logger.w("AUTH: Failed to load achievements, status code " + statusCode);
                    mAuthState = AuthState.NoAuth;
                    InvokeAuthCallback(false);
                }
            } else {
                Logger.w("OnAchievementsLoaded called unexpectedly in auth state " + mAuthState);
            }
        }

        // UI thread
        private void InvokeAuthCallback(bool success) {
            if (mAuthCallback == null) return;
            Logger.d("AUTH: Calling auth callback: success=" + success);
            System.Action<bool> cb = mAuthCallback;
            mAuthCallback = null;
            PlayGamesHelperObject.RunOnGameThread(() => {
                cb.Invoke(success);
            });
        }

        private void RetrieveUserInfo() {
            Logger.d("Attempting to retrieve player info.");
            
            using (AndroidJavaObject playerObj = mGHManager.CallGmsApi<AndroidJavaObject>(
                    "games.Games", "Players", "getCurrentPlayer")) {
        
                if (mUserId == null) {
                    mUserId = playerObj.Call<string>("getPlayerId");
                    Logger.d("Player ID: " + mUserId);
                }
    
                if (mUserDisplayName == null) {                
                    mUserDisplayName = playerObj.Call<string>("getDisplayName");
                    Logger.d("Player display name: " + mUserDisplayName);
                }
            }
        }

        // called (on the UI thread) by GameHelperManager to notify us that sign in succeeded
        internal void OnSignInSucceeded() {
            Logger.d("AndroidClient got OnSignInSucceeded.");
            RetrieveUserInfo();

            if (mAuthState == AuthState.AuthPending || mAuthState == AuthState.InProgress) {
                Logger.d("AUTH: Auth succeeded. Proceeding to achievement loading.");
                DoInitialAchievementLoad();
            } else if (mAuthState == AuthState.LoadingAchs) {
                Logger.w("AUTH: Got OnSignInSucceeded() while in achievement loading phase (unexpected).");
                Logger.w("AUTH: Trying to fix by issuing a new achievement load call.");
                DoInitialAchievementLoad();
            } else {
                // we will hit this case during the normal lifecycle (for example, Activity
                // was brought to the foreground and sign in has succeeded even though
                // we were not in an auth flow).
                Logger.d("Normal lifecycle OnSignInSucceeded received.");
                RunPendingActions();
                
                // check for invitations that may have arrived via notification
                CheckForConnectionExtras();
                
                // inform the RTMP client that sign-in has suceeded
                mRtmpClient.OnSignInSucceeded();
                mTbmpClient.OnSignInSucceeded();
            }
        }

        // called (on the UI thread) by GameHelperManager to notify us that sign in failed
        internal void OnSignInFailed() {
            Logger.d("AndroidClient got OnSignInFailed.");
            if (mAuthState == AuthState.AuthPending) {
                // we have yet to start the auth flow
                if (mSilentAuth) {
                    Logger.d("AUTH: Auth flow was pending, but silent=true, so failing.");
                    mAuthState = AuthState.NoAuth;
                    InvokeAuthCallback(false);
                } else {
                    Logger.d("AUTH: Auth flow was pending and silent=false, so doing noisy auth.");
                    mAuthState = AuthState.InProgress;
                    mGHManager.BeginUserInitiatedSignIn();
                }
            } else if (mAuthState == AuthState.InProgress) {
                // authentication was in progress, but failed: notify callback
                Logger.d("AUTH: FAILED!");
                mAuthState = AuthState.NoAuth;
                InvokeAuthCallback(false);
            } else if (mAuthState == AuthState.LoadingAchs) {
                // we were loading achievements and got disconnected: notify callback
                Logger.d("AUTH: FAILED (while loading achievements).");
                mAuthState = AuthState.NoAuth;
                InvokeAuthCallback(false);
            } else if (mAuthState == AuthState.NoAuth) {
                // we will hit this case during the normal lifecycle (for example, Activity
                // was brought to the foreground and sign in has failed).
                Logger.d("Normal OnSignInFailed received.");
            } else if (mAuthState == AuthState.Done) {
                // we lost authentication (for example, the token might have expired,
                // or the user revoked it)
                Logger.e("Authentication has been lost!");
                mAuthState = AuthState.NoAuth;
            }
        }

        // Runs any actions pending in the mActionsPendingSignIn queue
        private void RunPendingActions() {
            if (mActionsPendingSignIn.Count > 0) {
                Logger.d("Running pending actions on the UI thread.");
                while (mActionsPendingSignIn.Count > 0) {
                    Action a = mActionsPendingSignIn[0];
                    mActionsPendingSignIn.RemoveAt(0);
                    a.Invoke();
                }
                Logger.d("Done running pending actions on the UI thread.");
            } else {
                Logger.d("No pending actions to run on UI thread.");
            }
        }

        // runs on the game thread
        public bool IsAuthenticated() {
            return mAuthState == AuthState.Done && !mSignOutInProgress;
        }
        
        public void SignOut() {
            Logger.d("AndroidClient.SignOut");
            mSignOutInProgress = true;
            RunWhenConnectionStable(() => {
                Logger.d("Calling GHM.SignOut");
                mGHManager.SignOut();
                mAuthState = AuthState.NoAuth;
                mUserId = null;
                mUserDisplayName = null;
                mSignOutInProgress = false;
                Logger.d("Now signed out.");
            });
        }
        
        // Returns the game's Activity
        internal AndroidJavaObject GetActivity() {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                return jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        internal void RunOnUiThread(System.Action action) {
            using (AndroidJavaObject activity = GetActivity()) {
                activity.Call("runOnUiThread", new AndroidJavaRunnable(action));
            }
        }

        private class OnAchievementsLoadedResultProxy : AndroidJavaProxy {
            AndroidClient mOwner;

            internal OnAchievementsLoadedResultProxy(AndroidClient c) :
                    base(JavaConsts.ResultCallbackClass) {
                mOwner = c;
            }

            public void onResult(AndroidJavaObject result) {
                Logger.d("OnAchievementsLoadedResultProxy invoked");
                Logger.d("    result=" + result);
                int statusCode = JavaUtil.GetStatusCode(result);
                AndroidJavaObject achBuffer = JavaUtil.CallNullSafeObjectMethod(result, 
                        "getAchievements");
                mOwner.OnAchievementsLoaded(statusCode, achBuffer);
                if (achBuffer != null) {
                    achBuffer.Dispose();
                }
            }
        }


        // Runs the given action on the UI thread when the state of the GameHelper connection
        // becomes stable (i.e. not in the temporary lapse between Activity startup and
        // connection). So when the action runs, we will either be definitely signed in,
        // or have definitely failed to sign in.
        private void RunWhenConnectionStable(Action a) {
            RunOnUiThread(() => {
                if (mGHManager.Paused || mGHManager.Connecting) {
                    // we're in the middle of establishing a connection, so we'll
                    // have to queue this action to execute once the connection is
                    // established (or fails)
                    Logger.d("Action scheduled for later (connection currently in progress).");
                    mActionsPendingSignIn.Add(a);
                } else {
                    // connection is in a definite state, so we can run it right away
                    a.Invoke();
                }
            });
        }

        internal void CallClientApi(string desc, Action call, Action<bool> callback) {
            Logger.d("Requesting API call: " + desc);
            RunWhenConnectionStable(() => {
                // we got a stable connection state to the games service
                // (either connected or disconnected, but not in progress).
                if (mGHManager.IsConnected()) {
                    // we are connected, so make the API call
                    Logger.d("Connected! Calling API: " + desc);
                    call.Invoke();
                    if (callback != null) {
                        PlayGamesHelperObject.RunOnGameThread(() => {
                            callback.Invoke(true);
                        });
                    }
                } else {
                    // we are not connected, so fail the API call
                    Logger.w("Not connected! Failed to call API :" + desc);
                    if (callback != null) {
                        PlayGamesHelperObject.RunOnGameThread(() => {
                            callback.Invoke(false);
                        });
                    }
                }
            });
        }

        // called from game thread
        public string GetUserId() {
            return mUserId;
        }

        // called from game thread
        public string GetUserDisplayName() {
            return mUserDisplayName;
        }

        // called from game thread
        public void UnlockAchievement(string achId, Action<bool> callback) {
            // if the local cache says it's unlocked, we don't have to do anything
            Logger.d("AndroidClient.UnlockAchievement: " + achId);
            Achievement a = GetAchievement(achId);
            if (a != null && a.IsUnlocked) {
                Logger.d("...was already unlocked, so no-op.");
                if (callback != null) {
                    callback.Invoke(true);
                }
                return;
            }

            CallClientApi("unlock ach " + achId, () => {
                mGHManager.CallGmsApi("games.Games", "Achievements", "unlock", achId);
            }, callback);

            // update local cache
            a = GetAchievement(achId);
            if (a != null) {
                a.IsUnlocked = a.IsRevealed = true;
            }
        }

        // called from game thread
        public void RevealAchievement(string achId, Action<bool> callback) {
            Logger.d("AndroidClient.RevealAchievement: " + achId);
            Achievement a = GetAchievement(achId);
            if (a != null && a.IsRevealed) {
                Logger.d("...was already revealed, so no-op.");
                if (callback != null) {
                    callback.Invoke(true);
                }
                return;
            }

            CallClientApi("reveal ach " + achId, () => {
                mGHManager.CallGmsApi("games.Games", "Achievements", "reveal", achId);
            }, callback);

            // update local cache
            a = GetAchievement(achId);
            if (a != null) {
                a.IsRevealed = true;
            }
        }


        // called from game thread
        public void IncrementAchievement(string achId, int steps, Action<bool> callback) {
            Logger.d("AndroidClient.IncrementAchievement: " + achId + ", steps " + steps);
            
            CallClientApi("increment ach " + achId, () => {
                mGHManager.CallGmsApi("games.Games", "Achievements", "increment",
                        achId, steps);
            }, callback);
            
            // update local cache
            Achievement a = GetAchievement(achId);
            if (a != null) {
                a.CurrentSteps += steps;
                if (a.CurrentSteps >= a.TotalSteps) {
                    a.CurrentSteps = a.TotalSteps;
                }
            }
        }

        // called from game thread
        public List<Achievement> GetAchievements() {
            return mAchievementBank.GetAchievements();
        }

        // called from game thread
        public Achievement GetAchievement(string achId) {
            return mAchievementBank.GetAchievement(achId);
        }
        
        // called from game thread
        public void ShowAchievementsUI() {
            Logger.d("AndroidClient.ShowAchievementsUI.");
            CallClientApi("show achievements ui", () => {
                using (AndroidJavaObject intent = mGHManager.CallGmsApi<AndroidJavaObject>(
                        "games.Games", "Achievements", "getAchievementsIntent")) {
                    using (AndroidJavaObject activity = GetActivity()) {
                        Logger.d("About to show achievements UI with intent " + intent +
                            ", activity " + activity);
                        if (intent != null && activity != null) {
                            activity.Call("startActivityForResult", intent, RC_UNUSED);
                        }
                    }
                }
            }, null);
        }
        
        private AndroidJavaObject GetLeaderboardIntent(string lbId) {
            return (lbId == null) ?
                mGHManager.CallGmsApi<AndroidJavaObject>(
                    "games.Games", "Leaderboards", "getAllLeaderboardsIntent") :
                mGHManager.CallGmsApi<AndroidJavaObject>(
                    "games.Games", "Leaderboards", "getLeaderboardIntent", lbId);
        }

        // called from game thread
        public void ShowLeaderboardUI(string lbId) {
            Logger.d("AndroidClient.ShowLeaderboardUI, lb=" + (lbId == null ? "(all)" : lbId));
            CallClientApi("show LB ui", () => {
                using (AndroidJavaObject intent = GetLeaderboardIntent(lbId)) {
                    using (AndroidJavaObject activity = GetActivity()) {
                        Logger.d("About to show LB UI with intent " + intent +
                            ", activity " + activity);
                        if (intent != null && activity != null) {
                            activity.Call("startActivityForResult", intent, RC_UNUSED);
                        }
                    }
                }
            }, null);
        }

        // called from game thread
        public void SubmitScore(string lbId, long score, Action<bool> callback) {
            Logger.d("AndroidClient.SubmitScore, lb=" + lbId + ", score=" + score);
            CallClientApi("submit score " + score + ", lb " + lbId, () => {
                mGHManager.CallGmsApi("games.Games", "Leaderboards", 
                        "submitScore", lbId, score);
            }, callback);
        }

        // called from game thread
        public void LoadState(int slot, OnStateLoadedListener listener) {
            Logger.d("AndroidClient.LoadState, slot=" + slot);
            CallClientApi("load state slot=" + slot, () => {
                OnStateResultProxy proxy = new OnStateResultProxy(this, listener);
                mGHManager.CallGmsApiWithResult("appstate.AppStateManager", null, "load", 
                        proxy, slot);
            }, null);
        }

        // called from game thread. This is ONLY called internally (OnStateLoadedProxy
        // calls this). This is not part of the IPlayGamesClient interface.
        internal void ResolveState(int slot, string resolvedVersion, byte[] resolvedData,
                OnStateLoadedListener listener) {
            Logger.d(string.Format("AndroidClient.ResolveState, slot={0}, ver={1}, " +
                "data={2}", slot, resolvedVersion, resolvedData));
            CallClientApi("resolve state slot=" + slot, () => {
                mGHManager.CallGmsApiWithResult("appstate.AppStateManager", null, "resolve",
                new OnStateResultProxy(this, listener), slot, resolvedVersion, resolvedData);
            }, null);
        }

        // called from game thread
        public void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
            Logger.d(string.Format("AndroidClient.UpdateState, slot={0}, data={1}",
                slot, Logger.describe(data)));
            CallClientApi("update state, slot=" + slot, () => {
                mGHManager.CallGmsApi("appstate.AppStateManager", null, "update", slot, data);
            }, null);

            // On Android, cloud writes always succeeds (because, in the worst case,
            // data gets cached locally to send to the cloud later)
            listener.OnStateSaved(true, slot);
        }

        // called from game thread
        public void SetCloudCacheEncrypter(BufferEncrypter encrypter) {
            Logger.d("Ignoring cloud cache encrypter (not used in Android)");
            // Not necessary in Android (since the library takes care of storing
            // data locally)
        }
        
        // called from game thread
        public void RegisterInvitationDelegate(InvitationReceivedDelegate deleg) {
            Logger.d("AndroidClient.RegisterInvitationDelegate");
            if (deleg == null) {
                Logger.w("AndroidClient.RegisterInvitationDelegate called w/ null argument.");
                return;
            }
            mInvitationDelegate = deleg;
            
            // install invitation listener, if we don't have one yet
            if (!mRegisteredInvitationListener) {
                Logger.d("Registering an invitation listener.");
                RegisterInvitationListener();
            }
            
            if (mInvitationFromNotification != null) {
                Logger.d("Delivering pending invitation from notification now.");
                Invitation inv = mInvitationFromNotification;
                mInvitationFromNotification = null;
                PlayGamesHelperObject.RunOnGameThread(() => {
                    if (mInvitationDelegate != null) {
                        mInvitationDelegate.Invoke(inv, true);
                    }
                });
            }
        }
        
        // called from game thread
        public Invitation GetInvitationFromNotification() {
            Logger.d("AndroidClient.GetInvitationFromNotification");
            Logger.d("Returning invitation: " + ((mInvitationFromNotification == null) ?
                     "(null)" : mInvitationFromNotification.ToString()));
            return mInvitationFromNotification;
        }
        
        // called from game thread
        public bool HasInvitationFromNotification() {
            bool has = mInvitationFromNotification != null;
            Logger.d("AndroidClient.HasInvitationFromNotification, returning " + has);
            return has;
        }
        
        
        private void RegisterInvitationListener() {
            Logger.d("AndroidClient.RegisterInvitationListener");
            CallClientApi("register invitation listener", () => {
                mGHManager.CallGmsApi("games.Games", "Invitations",
                        "registerInvitationListener", new OnInvitationReceivedProxy(this));
            }, null);
            mRegisteredInvitationListener = true;
        }
        
        public IRealTimeMultiplayerClient GetRtmpClient() {
            return mRtmpClient;
        }
        
        public ITurnBasedMultiplayerClient GetTbmpClient() {
            return mTbmpClient;
        }

        internal GameHelperManager GHManager {
            get {
                return mGHManager;
            }
        }
        
        internal void ClearInvitationIfFromNotification(string invitationId) {
            Logger.d("AndroidClient.ClearInvitationIfFromNotification: " + invitationId);
            if (mInvitationFromNotification != null && 
                mInvitationFromNotification.InvitationId.Equals(invitationId)) {
                Logger.d("Clearing invitation from notification: " + invitationId);
                mInvitationFromNotification = null;
            }
        }
        
        private void CheckForConnectionExtras() {
            // check to see if we have a pending invitation in our gamehelper
            Logger.d("AndroidClient: CheckInvitationFromNotification.");
            Logger.d("AndroidClient: looking for invitation in our GameHelper.");
            Invitation invFromNotif = null;
            AndroidJavaObject invObj = mGHManager.GetInvitation();
            AndroidJavaObject matchObj = mGHManager.GetTurnBasedMatch();
            
            mGHManager.ClearInvitationAndTurnBasedMatch();
            
            if (invObj != null) {
                Logger.d("Found invitation in GameHelper. Converting.");
                invFromNotif = ConvertInvitation(invObj);
                Logger.d("Found invitation in our GameHelper: " + invFromNotif);
            } else {
                Logger.d("No invitation in our GameHelper. Trying SignInHelperManager.");
                AndroidJavaClass cls = JavaUtil.GetClass(JavaConsts.SignInHelperManagerClass);
                using (AndroidJavaObject inst = cls.CallStatic<AndroidJavaObject>("getInstance")) {
                    if (inst.Call<bool>("hasInvitation")) {
                        invFromNotif = ConvertInvitation(inst.Call<AndroidJavaObject>("getInvitation"));
                        Logger.d("Found invitation in SignInHelperManager: " + invFromNotif);
                        inst.Call("forgetInvitation");
                    } else {
                        Logger.d("No invitation in SignInHelperManager either.");
                    }
                }
            }
            
            TurnBasedMatch match = null;
            if (matchObj != null) {
                Logger.d("Found match in GameHelper. Converting.");
                match = JavaUtil.ConvertMatch(mUserId, matchObj);
                Logger.d("Match from GameHelper: " + match);
            } else {
                Logger.d("No match in our GameHelper. Trying SignInHelperManager.");
                AndroidJavaClass cls = JavaUtil.GetClass(JavaConsts.SignInHelperManagerClass);
                using (AndroidJavaObject inst = cls.CallStatic<AndroidJavaObject>("getInstance")) {
                    if (inst.Call<bool>("hasTurnBasedMatch")) {
                        match = JavaUtil.ConvertMatch(mUserId, 
                                inst.Call<AndroidJavaObject>("getTurnBasedMatch"));
                        Logger.d("Found match in SignInHelperManager: " + match);
                        inst.Call("forgetTurnBasedMatch");
                    } else {
                        Logger.d("No match in SignInHelperManager either.");
                    }
                }
            }
            
            // if we got an invitation from the notification, invoke the delegate
            if (invFromNotif != null) {
                if (mInvitationDelegate != null) {
                    Logger.d("Invoking invitation received delegate to deal with invitation " + 
                             " from notification.");
                    PlayGamesHelperObject.RunOnGameThread(() => {
                        if (mInvitationDelegate != null) {
                            mInvitationDelegate.Invoke(invFromNotif, true);
                        }
                    });
                } else {
                    Logger.d("No delegate to handle invitation from notification; queueing.");
                    mInvitationFromNotification = invFromNotif;
                }
            }
            
            // if we got a turn-based match, hand it over to the TBMP client who will know
            // better what to do with it
            if (match != null) {
                mTbmpClient.HandleMatchFromNotification(match);
            }
        }
        
        private Invitation ConvertInvitation(AndroidJavaObject invObj) {
            Logger.d("Converting Android invitation to our Invitation object.");
            string invitationId = invObj.Call<string>("getInvitationId");
            int invType = invObj.Call<int>("getInvitationType");
            Participant inviter;
            using (AndroidJavaObject inviterObj = invObj.Call<AndroidJavaObject>("getInviter")) {
                inviter = JavaUtil.ConvertParticipant(inviterObj);
            }
            int variant = invObj.Call<int>("getVariant");
            Invitation.InvType type;
            
            switch (invType) {
            case JavaConsts.INVITATION_TYPE_REAL_TIME:
                type = Invitation.InvType.RealTime;
                break;
            case JavaConsts.INVITATION_TYPE_TURN_BASED:
                type = Invitation.InvType.TurnBased;
                break;
            default:
                Logger.e("Unknown invitation type " + invType);
                type = Invitation.InvType.Unknown;
                break;
            }
            
            Invitation result = new Invitation(type, invitationId, inviter, variant);
            Logger.d("Converted invitation: " + result.ToString());
            return result;
        }
        
        private void OnInvitationReceived(AndroidJavaObject invitationObj) {
            Logger.d("AndroidClient.OnInvitationReceived. Converting invitation...");
            Invitation inv = ConvertInvitation(invitationObj);
            Logger.d("Invitation: " + inv.ToString());
            
            if (mInvitationDelegate != null) {
                Logger.d("Delivering invitation to invitation received delegate.");
                PlayGamesHelperObject.RunOnGameThread(() => {
                    if (mInvitationDelegate != null) {
                        mInvitationDelegate.Invoke(inv, false);
                    }
                });
            } else {
                Logger.w("AndroidClient.OnInvitationReceived discarding invitation because " + 
                        " delegate is null.");
            }
        }
        
        private void OnInvitationRemoved(string invitationId) {
            Logger.d("AndroidClient.OnInvitationRemoved: " + invitationId);
            ClearInvitationIfFromNotification(invitationId);
        }
        
        private class OnInvitationReceivedProxy : AndroidJavaProxy {
            AndroidClient mOwner;
            
            internal OnInvitationReceivedProxy(AndroidClient owner)
                    : base(JavaConsts.OnInvitationReceivedListenerClass) {
                mOwner = owner;
            }
            
            public void onInvitationReceived(AndroidJavaObject invitationObj) {
                Logger.d("OnInvitationReceivedProxy.onInvitationReceived");
                mOwner.OnInvitationReceived(invitationObj);
            }
            
            public void onInvitationRemoved(string invitationId) {
                Logger.d("OnInvitationReceivedProxy.onInvitationRemoved");
                mOwner.OnInvitationRemoved(invitationId);
            }
        }
        
        public string PlayerId {
            get {
                return mUserId;
            }
        }
    }
}
#endif
