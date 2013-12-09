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

namespace GooglePlayGames.Android {
    public class AndroidClient : IPlayGamesClient {
        GameHelperManager mGameHelperManager = null;

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
        // actions here and execute when we get onSignInSucceeded or onSignInFailed,.
        List<Action> mActionsPendingSignIn = new List<Action>();

        // Result code for child activities whose result we don't care about
        const int RC_UNUSED = 9999;

        public AndroidClient() {
            RunOnUiThread(() => {
                Logger.d("Initializing Android Client.");
                Logger.d("Creating GameHelperManager to manage GameHelper.");
                mGameHelperManager = new GameHelperManager(this);
                Logger.d("GameHelper manager is set up.");
            });
            // now we wait for the result of the silent auth, which will trigger
            // a call to either OnSignInSucceeded or OnSignInFailed
        }

        // called from game thread
        public void Authenticate(System.Action<bool> callback, bool silent) {
            if (mAuthState != AuthState.NoAuth) {
                Logger.w("Authenticate() called while an authentication process was active. " + mAuthState);
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
                switch (mGameHelperManager.State) {
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
                            mGameHelperManager.BeginUserInitiatedSignIn();
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
            mGameHelperManager.GetGamesClient().Call("loadAchievements",
                new OnAchievementsLoadedListenerProxy(this), false);
            Logger.d("AUTH: Initial achievement load call made.");
        }

        // UI thread
        private void OnAchievementsLoaded(int statusCode, AndroidJavaObject buffer) {
            if (mAuthState == AuthState.LoadingAchs) {
                Logger.d("AUTH: Initial achievement load finished.");

                if (statusCode == JavaConsts.STATUS_OK || statusCode == JavaConsts.STATUS_STALE_DATA) {
                    // successful load (either from network or local cache)
                    Logger.d("Processing buffer.");
                    mAchievementBank.ProcessBuffer(buffer);
                    Logger.d("AUTH: Auth process complete!");
                    mAuthState = AuthState.Done;
                    InvokeAuthCallback(true);
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
            AndroidJavaObject gamesClient = mGameHelperManager.GetGamesClient();

            if (mUserId == null) {
                Logger.d("Attempting to retrieve player ID.");
                mUserId = gamesClient.Call<string>("getCurrentPlayerId");
                Logger.d("Player ID: " + mUserId);
            }

            if (mUserDisplayName == null) {
                Logger.d("Attempting to retrieve display name.");
                AndroidJavaObject playerObj = gamesClient.Call<AndroidJavaObject>("getCurrentPlayer");
                if (playerObj != null) {
                    mUserDisplayName = playerObj.Call<string>("getDisplayName");
                    Logger.d("Player display name: " + mUserDisplayName);
                } else {
                    Logger.w("Warning: GamesClient.getCurrentPlayer returned null.");
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
                    mGameHelperManager.BeginUserInitiatedSignIn();
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
            return mAuthState == AuthState.Done;
        }
        
        public void SignOut() {
            Logger.d("AndroidClient.SignOut");
            RunWhenConnectionStable(() => {
                Logger.d("Calling GHM.SignOut");
                mGameHelperManager.SignOut();
                mAuthState = AuthState.NoAuth;
                Logger.d("Now signed out.");
            });
        }

        // Returns the game's Activity
        internal AndroidJavaObject GetActivity() {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (jc == null) {
                throw new System.Exception("Could not get class com.unity3d.player.UnityPlayer.");
            }
            AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null) {
                throw new System.Exception("Could not get class current activity from UnityPlayer.");
            }
            return activity;
        }

        internal void RunOnUiThread(System.Action action) {
            GetActivity().Call("runOnUiThread", new AndroidJavaRunnable(action));
        }

        private class OnAchievementsLoadedListenerProxy : AndroidJavaProxy {
            AndroidClient mOwner;

            internal OnAchievementsLoadedListenerProxy(AndroidClient c) :
                    base("com.google.android.gms.games.achievement.OnAchievementsLoadedListener") {
                mOwner = c;
            }

            public void onAchievementsLoaded (int statusCode, AndroidJavaObject buffer) {
                mOwner.OnAchievementsLoaded(statusCode, buffer);
            }
        }


        // Runs the given action on the UI thread when the state of the GameHelper connection
        // becomes stable (i.e. not in the temporary lapse between Activity startup and
        // connection). So when the action runs, we will either be definitely signed in,
        // or have definitely failed to sign in.
        private void RunWhenConnectionStable(Action a) {
            RunOnUiThread(() => {
                if (mGameHelperManager.State == GameHelperManager.ConnectionState.Connecting) {
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

        private enum ClientApi { Games, AppState };
        private void CallGamesClientApi(string desc,
                Action<AndroidJavaObject> call, Action<bool> callback) {
            CallClientApi(ClientApi.Games, desc, call, callback);
        }

        private void CallAppStateClientApi(string desc,
                Action<AndroidJavaObject> call, Action<bool> callback) {
            CallClientApi(ClientApi.AppState, desc, call, callback);
        }

        private void CallClientApi(ClientApi api, string desc,
                Action<AndroidJavaObject> call, Action<bool> callback) {
            Logger.d("Requesting API call: " + desc);
            RunWhenConnectionStable(() => {
                // we got a stable connection state to the games service
                // (either connected or disconnected, but not in progress).
                if (mGameHelperManager.IsConnected()) {
                    // we are connected, so make the API call
                    Logger.d("Connected! Calling API: " + desc);
                    call.Invoke(api == ClientApi.Games ?
                            mGameHelperManager.GetGamesClient() :
                            mGameHelperManager.GetAppStateClient());
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

            CallGamesClientApi("unlock ach " + achId, (AndroidJavaObject c) => {
                c.Call("unlockAchievement", achId);
            },    callback);

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

            CallGamesClientApi("reveal ach " + achId, (AndroidJavaObject c) => {
                c.Call("revealAchievement", achId);
            },    callback);

            // update local cache
            a = GetAchievement(achId);
            if (a != null) {
                a.IsRevealed = true;
            }
        }


        // called from game thread
        public void IncrementAchievement(string achId, int steps, Action<bool> callback) {
            Logger.d("AndroidClient.IncrementAchievement: " + achId + ", steps " + steps);
            CallGamesClientApi("increment ach " + achId + ", " + steps, (AndroidJavaObject c) => {
                c.Call("incrementAchievement", achId, steps);
            },    callback);

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
            CallGamesClientApi("show achievements ui", (AndroidJavaObject c) => {
                AndroidJavaObject intent = c.Call<AndroidJavaObject>("getAchievementsIntent");
                AndroidJavaObject activity = GetActivity();
                Logger.d("About to show achievements UI with intent " + intent +
                    ", activity " + activity);
                if (intent != null && activity != null) {
                    activity.Call("startActivityForResult", intent, RC_UNUSED);
                }
            }, null);
        }

        // called from game thread
        public void ShowLeaderboardUI(string lbId) {
            Logger.d("AndroidClient.ShowLeaderboardUI, lb=" + (lbId == null ? "(all)" : lbId));
            CallGamesClientApi("show LB ui", (AndroidJavaObject c) => {
                AndroidJavaObject intent = (lbId == null) ?
                        c.Call<AndroidJavaObject>("getAllLeaderboardsIntent") :
                        c.Call<AndroidJavaObject>("getLeaderboardIntent", lbId);
                AndroidJavaObject activity = GetActivity();
                Logger.d("About to show LB UI with intent " + intent +
                    ", activity " + activity);
                if (intent != null && activity != null) {
                    activity.Call("startActivityForResult", intent, RC_UNUSED);
                }
            }, null);
        }

        // called from game thread
        public void SubmitScore(string lbId, long score, Action<bool> callback) {
            Logger.d("AndroidClient.SubmitScore, lb=" + lbId + ", score=" + score);
            CallGamesClientApi("submit score " + score + ", lb " + lbId, (AndroidJavaObject c) => {
                c.Call("submitScore", lbId, score);
            }, callback);
        }

        // called from game thread
        public void LoadState(int slot, OnStateLoadedListener listener) {
            Logger.d("AndroidClient.LoadState, slot=" + slot);
            CallAppStateClientApi("load state slot=" + slot, (AndroidJavaObject c) => {
                OnStateLoadedProxy proxy = new OnStateLoadedProxy(this, listener);
                c.Call("loadState", proxy, slot);
            }, null);
        }

        // called from game thread. This is ONLY called internally (OnStateLoadedProxy
        // calls this). This is not part of the IPlayGamesClient interface.
        internal void ResolveState(int slot, string resolvedVersion, byte[] resolvedData,
                OnStateLoadedListener listener) {
            Logger.d(string.Format("AndroidClient.ResolveState, slot={0}, ver={1}, " +
                "data={2}", slot, resolvedVersion, resolvedData));
            CallAppStateClientApi("resolve state slot=" + slot, (AndroidJavaObject c) => {
                c.Call("resolveState", new OnStateLoadedProxy(this, listener), slot,
                    resolvedVersion, resolvedData);
            }, null);
        }

        // called from game thread
        public void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
            Logger.d(string.Format("AndroidClient.UpdateState, slot={0}, data={1}",
                slot, Logger.describe(data)));
            CallAppStateClientApi("update state, slot=" + slot, (AndroidJavaObject c) => {
                c.Call("updateState", slot, data);
            }, null);

            // On Android, cloud writes always succeed (because, at worst, they get cached
            // locally to send to the cloud later)
            listener.OnStateSaved(true, slot);
        }

        public void SetCloudCacheEncrypter(BufferEncrypter encrypter) {
            Logger.d("Ignoring cloud cache encrypter (not used in Android)");
            // Not necessary in Android (since the library takes care of storing
            // data locally)
        }
    }
}

#endif
