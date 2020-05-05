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
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames.OurUtils;
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi.Events;
    using GooglePlayGames.BasicApi.Video;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class AndroidClient : IPlayGamesClient
    {
        private enum AuthState
        {
            Unauthenticated,
            Authenticated
        }

        private readonly object GameServicesLock = new object();
        private readonly object AuthStateLock = new object();

        private readonly PlayGamesClientConfiguration mConfiguration;
        private volatile AndroidTurnBasedMultiplayerClient mTurnBasedClient;
        private volatile IRealTimeMultiplayerClient mRealTimeClient;
        private volatile ISavedGameClient mSavedGameClient;
        private volatile IEventsClient mEventsClient;
        private volatile IVideoClient mVideoClient;
        private volatile AndroidTokenClient mTokenClient;
        private volatile Action<Invitation, bool> mInvitationDelegate;
        private volatile Player mUser = null;
        private volatile AuthState mAuthState = AuthState.Unauthenticated;

        AndroidJavaClass mGamesClass = new AndroidJavaClass("com.google.android.gms.games.Games");
        private static string TasksClassName = "com.google.android.gms.tasks.Tasks";

        private AndroidJavaObject mInvitationCallback = null;

        private readonly int mLeaderboardMaxResults = 25; // can be from 1 to 25

        internal AndroidClient(PlayGamesClientConfiguration configuration)
        {
            PlayGamesHelperObject.CreateObject();
            this.mConfiguration = Misc.CheckNotNull(configuration);
            RegisterInvitationDelegate(configuration.InvitationDelegate);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.Authenticate"/>
        public void Authenticate(bool silent, Action<SignInStatus> callback)
        {
            lock (AuthStateLock)
            {
                // If the user is already authenticated, just fire the callback, we don't need
                // any additional work.
                if (mAuthState == AuthState.Authenticated)
                {
                    Debug.Log("Already authenticated.");
                    InvokeCallbackOnGameThread(callback, SignInStatus.Success);
                    return;
                }
            }

            InitializeTokenClient();

            Debug.Log("Starting Auth with token client.");
            mTokenClient.FetchTokens(silent, (int result) =>
            {
                bool succeed = result == 0 /* CommonStatusCodes.SUCCEED */;
                InitializeGameServices();
                if (succeed)
                {
                    using (var signInTasks = new AndroidJavaObject("java.util.ArrayList"))
                    {
                        if (mInvitationDelegate != null)
                        {
                            mInvitationCallback = new AndroidJavaObject(
                                "com.google.games.bridge.InvitationCallbackProxy",
                                new InvitationCallbackProxy(mInvitationDelegate));
                            using (var invitationsClient = getInvitationsClient())
                            using (var taskRegisterCallback =
                                invitationsClient.Call<AndroidJavaObject>("registerInvitationCallback",
                                    mInvitationCallback))
                            {
                                signInTasks.Call<bool>("add", taskRegisterCallback);
                            }
                        }

                        AndroidJavaObject taskGetPlayer =
                            getPlayersClient().Call<AndroidJavaObject>("getCurrentPlayer");
                        AndroidJavaObject taskGetActivationHint =
                            getGamesClient().Call<AndroidJavaObject>("getActivationHint");
                        AndroidJavaObject taskIsCaptureSupported =
                            getVideosClient().Call<AndroidJavaObject>("isCaptureSupported");

                        if (!mConfiguration.IsHidingPopups)
                        {
                            AndroidJavaObject taskSetViewForPopups;
                            using (var popupView = AndroidHelperFragment.GetDefaultPopupView())
                            {
                                taskSetViewForPopups =
                                    getGamesClient().Call<AndroidJavaObject>("setViewForPopups", popupView);
                            }

                            signInTasks.Call<bool>("add", taskSetViewForPopups);
                        }

                        signInTasks.Call<bool>("add", taskGetPlayer);
                        signInTasks.Call<bool>("add", taskGetActivationHint);
                        signInTasks.Call<bool>("add", taskIsCaptureSupported);

                        using (var tasks = new AndroidJavaClass(TasksClassName))
                        using (var allTask = tasks.CallStatic<AndroidJavaObject>("whenAll", signInTasks))
                        {
                            AndroidTaskUtils.AddOnCompleteListener<AndroidJavaObject>(
                                allTask,
                                completeTask =>
                                {
                                    if (completeTask.Call<bool>("isSuccessful"))
                                    {
                                        using (var resultObject = taskGetPlayer.Call<AndroidJavaObject>("getResult"))
                                        {
                                            mUser = AndroidJavaConverter.ToPlayer(resultObject);
                                        }

                                        var account = mTokenClient.GetAccount();
                                        lock (GameServicesLock)
                                        {
                                            mSavedGameClient = new AndroidSavedGameClient(this, account);
                                            mEventsClient = new AndroidEventsClient(account);
                                            bool isCaptureSupported;
                                            using (var resultObject =
                                                taskIsCaptureSupported.Call<AndroidJavaObject>("getResult"))
                                            {
                                                isCaptureSupported = resultObject.Call<bool>("booleanValue");
                                            }

                                            mVideoClient = new AndroidVideoClient(isCaptureSupported, account);
                                            mRealTimeClient = new AndroidRealTimeMultiplayerClient(this, account);
                                            mTurnBasedClient = new AndroidTurnBasedMultiplayerClient(this, account);
                                            mTurnBasedClient.RegisterMatchDelegate(mConfiguration.MatchDelegate);
                                        }

                                        mAuthState = AuthState.Authenticated;
                                        InvokeCallbackOnGameThread(callback, SignInStatus.Success);
                                        GooglePlayGames.OurUtils.Logger.d("Authentication succeeded");
                                        try
                                        {
                                            using (var activationHint =
                                                taskGetActivationHint.Call<AndroidJavaObject>("getResult"))
                                            {
                                                if (mInvitationDelegate != null)
                                                {
                                                    try
                                                    {
                                                        using (var invitationObject =
                                                            activationHint.Call<AndroidJavaObject>("getParcelable",
                                                                "invitation" /* Multiplayer.EXTRA_INVITATION */))
                                                        {
                                                            Invitation invitation =
                                                                AndroidJavaConverter.ToInvitation(invitationObject);
                                                            mInvitationDelegate(invitation, /* shouldAutoAccept= */
                                                                true);
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        // handle null return
                                                    }
                                                }


                                                if (mTurnBasedClient.MatchDelegate != null)
                                                {
                                                    try
                                                    {
                                                        using (var matchObject =
                                                            activationHint.Call<AndroidJavaObject>("getParcelable",
                                                                "turn_based_match" /* Multiplayer#EXTRA_TURN_BASED_MATCH */)
                                                        )
                                                        {
                                                            TurnBasedMatch turnBasedMatch =
                                                                AndroidJavaConverter.ToTurnBasedMatch(matchObject);
                                                            mTurnBasedClient.MatchDelegate(
                                                                turnBasedMatch, /* shouldAutoLaunch= */ true);
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            // handle null return
                                        }

                                        LoadAchievements(ignore => { });
                                    }
                                    else
                                    {
                                        SignOut();
                                        if (completeTask.Call<bool>("isCanceled"))
                                        {
                                            InvokeCallbackOnGameThread(callback, SignInStatus.Canceled);
                                            return;
                                        }

                                        using (var exception = completeTask.Call<AndroidJavaObject>("getException"))
                                        {
                                            GooglePlayGames.OurUtils.Logger.e(
                                                "Authentication failed - " + exception.Call<string>("toString"));
                                            InvokeCallbackOnGameThread(callback, SignInStatus.InternalError);
                                        }
                                    }
                                }
                            );
                        }
                    }
                }
                else
                {
                    lock (AuthStateLock)
                    {
                        Debug.Log("Returning an error code.");
                        InvokeCallbackOnGameThread(callback, SignInHelper.ToSignInStatus(result));
                    }
                }
            });
        }

        private static Action<T> AsOnGameThreadCallback<T>(Action<T> callback)
        {
            if (callback == null)
            {
                return delegate { };
            }

            return result => InvokeCallbackOnGameThread(callback, result);
        }

        private static void InvokeCallbackOnGameThread(Action callback)
        {
            if (callback == null)
            {
                return;
            }

            PlayGamesHelperObject.RunOnGameThread(() =>
            {
                callback();
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
                callback(data);
            });
        }


        private static Action<T1, T2> AsOnGameThreadCallback<T1, T2>(
            Action<T1, T2> toInvokeOnGameThread)
        {
            return (result1, result2) =>
            {
                if (toInvokeOnGameThread == null)
                {
                    return;
                }

                PlayGamesHelperObject.RunOnGameThread(() => toInvokeOnGameThread(result1, result2));
            };
        }

        private static void InvokeCallbackOnGameThread<T1, T2>(Action<T1, T2> callback, T1 t1, T2 t2)
        {
            if (callback == null)
            {
                return;
            }

            PlayGamesHelperObject.RunOnGameThread(() =>
            {
                callback(t1, t2);
            });
        }

        private void InitializeGameServices()
        {
            if (mTokenClient != null)
            {
                return;
            }

            InitializeTokenClient();
        }

        private void InitializeTokenClient()
        {
            if (mTokenClient != null)
            {
                return;
            }

            mTokenClient = new AndroidTokenClient();

            if (!GameInfo.WebClientIdInitialized() &&
                (mConfiguration.IsRequestingIdToken || mConfiguration.IsRequestingAuthCode))
            {
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
            if (mConfiguration.EnableSavedGames)
            {
                mTokenClient.AddOauthScopes("https://www.googleapis.com/auth/drive.appdata");
            }

            mTokenClient.AddOauthScopes(scopes);
            mTokenClient.SetAccountName(mConfiguration.AccountName);
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
            mTokenClient.GetAnotherServerAuthCode(reAuthenticateIfNeeded, AsOnGameThreadCallback(callback));
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
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            InvokeCallbackOnGameThread(callback, true);
        }

        public IUserProfile[] GetFriends()
        {
            return new IUserProfile[0];
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SignOut"/>
        public void SignOut()
        {
            SignOut( /* uiCallback= */ null);
        }


        public void SignOut(Action uiCallback)
        {
            if (mTokenClient == null)
            {
                InvokeCallbackOnGameThread(uiCallback);
                return;
            }

            if (mInvitationCallback != null)
            {
                using (var invitationsClient = getInvitationsClient())
                using (var task = invitationsClient.Call<AndroidJavaObject>(
                    "unregisterInvitationCallback", mInvitationCallback))
                {
                    AndroidTaskUtils.AddOnCompleteListener<AndroidJavaObject>(
                        task,
                        completedTask =>
                        {
                            mInvitationCallback = null;
                            mTokenClient.Signout();
                            mAuthState = AuthState.Unauthenticated;
                            if (uiCallback != null)
                            {
                                InvokeCallbackOnGameThread(uiCallback);
                            }
                        });
                }
            }
            else
            {
                mTokenClient.Signout();
                mAuthState = AuthState.Unauthenticated;
                if (uiCallback != null)
                {
                    InvokeCallbackOnGameThread(uiCallback);
                }
            }

            PlayGamesHelperObject.RunOnGameThread(() => SignInHelper.SetPromptUiSignIn(true));
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
            if (!IsAuthenticated())
            {
                GooglePlayGames.OurUtils.Logger.d("Cannot call SetGravityForPopups when not authenticated");
            }

            using (var gamesClient = getGamesClient())
            using (gamesClient.Call<AndroidJavaObject>("setGravityForPopups",
                (int) gravity | (int) Gravity.CENTER_HORIZONTAL))
                ;
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetPlayerStats"/>
        public void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            using (var playerStatsClient = getPlayerStatsClient())
            using (var task = playerStatsClient.Call<AndroidJavaObject>("loadPlayerStats", /* forceReload= */ false))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var playerStatsJava = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            int numberOfPurchases = playerStatsJava.Call<int>("getNumberOfPurchases");
                            float avgSessionLength = playerStatsJava.Call<float>("getAverageSessionLength");
                            int daysSinceLastPlayed = playerStatsJava.Call<int>("getDaysSinceLastPlayed");
                            int numberOfSessions = playerStatsJava.Call<int>("getNumberOfSessions");
                            float sessionPercentile = playerStatsJava.Call<float>("getSessionPercentile");
                            float spendPercentile = playerStatsJava.Call<float>("getSpendPercentile");
                            float spendProbability = playerStatsJava.Call<float>("getSpendProbability");
                            float churnProbability = playerStatsJava.Call<float>("getChurnProbability");
                            float highSpenderProbability = playerStatsJava.Call<float>("getHighSpenderProbability");
                            float totalSpendNext28Days = playerStatsJava.Call<float>("getTotalSpendNext28Days");

                            PlayerStats result = new PlayerStats(
                                numberOfPurchases,
                                avgSessionLength,
                                daysSinceLastPlayed,
                                numberOfSessions,
                                sessionPercentile,
                                spendPercentile,
                                spendProbability,
                                churnProbability,
                                highSpenderProbability,
                                totalSpendNext28Days);

                            InvokeCallbackOnGameThread(callback, CommonStatusCodes.Success, result);
                        }
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    e =>
                    {
                        Debug.Log("GetPlayerStats failed: " + e.Call<string>("toString"));
                        var statusCode = IsAuthenticated() ?
                            CommonStatusCodes.InternalError : CommonStatusCodes.SignInRequired;
                        InvokeCallbackOnGameThread(callback, statusCode, new PlayerStats());
                    });
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadUsers"/>
        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, new IUserProfile[0]);
                return;
            }

            using (var playersClient = getPlayersClient())
            {
                object countLock = new object();
                int count = userIds.Length;
                int resultCount = 0;
                IUserProfile[] users = new IUserProfile[count];
                for (int i = 0; i < count; ++i)
                {
                    using (var task = playersClient.Call<AndroidJavaObject>("loadPlayer", userIds[i]))
                    {
                        AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                            task,
                            annotatedData =>
                            {
                                using (var player = annotatedData.Call<AndroidJavaObject>("get"))
                                {
                                    string playerId = player.Call<string>("getPlayerId");
                                    for (int j = 0; j < count; ++j)
                                    {
                                        if (playerId == userIds[j])
                                        {
                                            users[j] = AndroidJavaConverter.ToPlayer(player);
                                            break;
                                        }
                                    }

                                    lock (countLock)
                                    {
                                        ++resultCount;
                                        if (resultCount == count)
                                        {
                                            InvokeCallbackOnGameThread(callback, users);
                                        }
                                    }
                                }
                            });

                        AddOnFailureListenerWithSignOut(
                            task,
                            exception =>
                            {
                                Debug.Log("LoadUsers failed for index " + i + " with: " + exception.Call<string>("toString"));
                                lock (countLock)
                                {
                                    ++resultCount;
                                    if (resultCount == count)
                                    {
                                        InvokeCallbackOnGameThread(callback, users);
                                    }
                                }
                            });
                    }
                }
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadAchievements"/>
        public void LoadAchievements(Action<Achievement[]> callback)
        {
            using (var achievementsClient = getAchievementsClient())
            using (var task = achievementsClient.Call<AndroidJavaObject>("load", /* forceReload= */ false))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var achievementBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            int count = achievementBuffer.Call<int>("getCount");
                            Achievement[] result = new Achievement[count];
                            for (int i = 0; i < count; ++i)
                            {
                                Achievement achievement = new Achievement();
                                using (var javaAchievement = achievementBuffer.Call<AndroidJavaObject>("get", i))
                                {
                                    achievement.Id = javaAchievement.Call<string>("getAchievementId");
                                    achievement.Description = javaAchievement.Call<string>("getDescription");
                                    achievement.Name = javaAchievement.Call<string>("getName");
                                    achievement.Points = javaAchievement.Call<ulong>("getXpValue");

                                    long timestamp = javaAchievement.Call<long>("getLastUpdatedTimestamp");
                                    achievement.LastModifiedTime = AndroidJavaConverter.ToDateTime(timestamp);

                                    achievement.RevealedImageUrl = javaAchievement.Call<string>("getRevealedImageUrl");
                                    achievement.UnlockedImageUrl = javaAchievement.Call<string>("getUnlockedImageUrl");
                                    achievement.IsIncremental =
                                        javaAchievement.Call<int>("getType") == 1 /* TYPE_INCREMENTAL */;
                                    if (achievement.IsIncremental)
                                    {
                                        achievement.CurrentSteps = javaAchievement.Call<int>("getCurrentSteps");
                                        achievement.TotalSteps = javaAchievement.Call<int>("getTotalSteps");
                                    }

                                    int state = javaAchievement.Call<int>("getState");
                                    achievement.IsUnlocked = state == 0 /* STATE_UNLOCKED */;
                                    achievement.IsRevealed = state == 1 /* STATE_REVEALED */;
                                }

                                result[i] = achievement;
                            }

                            achievementBuffer.Call("release");
                            InvokeCallbackOnGameThread(callback, result);
                        }
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    exception =>
                    {
                        Debug.Log("LoadAchievements failed: " + exception.Call<string>("toString"));
                        InvokeCallbackOnGameThread(callback, new Achievement[0]);
                    });
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.UnlockAchievement"/>
        public void UnlockAchievement(string achId, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("unlock", achId);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.RevealAchievement"/>
        public void RevealAchievement(string achId, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("reveal", achId);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.IncrementAchievement"/>
        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("increment", achId, steps);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SetStepsAtLeast"/>
        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("setSteps", achId, steps);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.ShowAchievementsUI"/>
        public void ShowAchievementsUI(Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, UIStatus.NotAuthorized);
                return;
            }

            AndroidHelperFragment.ShowAchievementsUI(GetUiSignOutCallbackOnGameThread(callback));
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LeaderboardMaxResults"/>
        public int LeaderboardMaxResults()
        {
            return mLeaderboardMaxResults;
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.ShowLeaderboardUI"/>
        public void ShowLeaderboardUI(string leaderboardId, LeaderboardTimeSpan span, Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, UIStatus.NotAuthorized);
                return;
            }

            if (leaderboardId == null)
            {
                AndroidHelperFragment.ShowAllLeaderboardsUI(GetUiSignOutCallbackOnGameThread(callback));
            }
            else
            {
                AndroidHelperFragment.ShowLeaderboardUI(leaderboardId, span,
                    GetUiSignOutCallbackOnGameThread(callback));
            }
        }

        private void AddOnFailureListenerWithSignOut(AndroidJavaObject task, Action<AndroidJavaObject> callback)
        {
            AndroidTaskUtils.AddOnFailureListener(
                task,
                exception =>
                {
                    var statusCode = exception.Call<int>("getStatusCode");
                    if (statusCode == /* CommonStatusCodes.SignInRequired */ 4 ||
                        statusCode == /* GamesClientStatusCodes.CLIENT_RECONNECT_REQUIRED */ 26502)
                    {
                        SignOut();
                    }
                    callback(exception);
                });
        }

        private Action<UIStatus> GetUiSignOutCallbackOnGameThread(Action<UIStatus> callback)
        {
            Action<UIStatus> uiCallback = (status) =>
            {
                if (status == UIStatus.NotAuthorized)
                {
                    SignOut(() =>
                    {
                        if (callback != null)
                        {
                            callback(status);
                        }
                    });
                }
                else
                {
                    if (callback != null)
                    {
                        callback(status);
                    }
                }
            };

            return AsOnGameThreadCallback(uiCallback);
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadScores"/>
        public void LoadScores(string leaderboardId, LeaderboardStart start,
            int rowCount, LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback)
        {
            using (var client = getLeaderboardsClient())
            {
                string loadScoresMethod =
                    start == LeaderboardStart.TopScores ? "loadTopScores" : "loadPlayerCenteredScores";
                using (var task = client.Call<AndroidJavaObject>(
                    loadScoresMethod,
                    leaderboardId,
                    AndroidJavaConverter.ToLeaderboardVariantTimeSpan(timeSpan),
                    AndroidJavaConverter.ToLeaderboardVariantCollection(collection),
                    rowCount))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task,
                        annotatedData =>
                        {
                            using (var leaderboardScores = annotatedData.Call<AndroidJavaObject>("get"))
                            {
                                InvokeCallbackOnGameThread(callback, CreateLeaderboardScoreData(
                                    leaderboardId,
                                    collection,
                                    timeSpan,
                                    annotatedData.Call<bool>("isStale")
                                        ? ResponseStatus.SuccessWithStale
                                        : ResponseStatus.Success,
                                    leaderboardScores));
                                leaderboardScores.Call("release");
                            }
                        });

                    AddOnFailureListenerWithSignOut(
                        task,
                        exception =>
                        {
                            Debug.Log("LoadScores failed: " + exception.Call<string>("toString"));
                            InvokeCallbackOnGameThread(callback,
                                new LeaderboardScoreData(leaderboardId, ResponseStatus.InternalError));
                        });
                }
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadMoreScores"/>
        public void LoadMoreScores(ScorePageToken token, int rowCount,
            Action<LeaderboardScoreData> callback)
        {
            using (var client = getLeaderboardsClient())
            using (var task = client.Call<AndroidJavaObject>("loadMoreScores",
                token.InternalObject, rowCount, AndroidJavaConverter.ToPageDirection(token.Direction)))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var leaderboardScores = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            InvokeCallbackOnGameThread(callback, CreateLeaderboardScoreData(
                                token.LeaderboardId,
                                token.Collection,
                                token.TimeSpan,
                                annotatedData.Call<bool>("isStale")
                                    ? ResponseStatus.SuccessWithStale
                                    : ResponseStatus.Success,
                                leaderboardScores));
                            leaderboardScores.Call("release");
                        }
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    exception =>
                    {
                        Debug.Log("LoadMoreScores failed: " + exception.Call<string>("toString"));
                        InvokeCallbackOnGameThread(callback,
                            new LeaderboardScoreData(token.LeaderboardId, ResponseStatus.InternalError));
                    });
            }
        }

        private LeaderboardScoreData CreateLeaderboardScoreData(
            string leaderboardId,
            LeaderboardCollection collection,
            LeaderboardTimeSpan timespan,
            ResponseStatus status,
            AndroidJavaObject leaderboardScoresJava)
        {
            LeaderboardScoreData leaderboardScoreData = new LeaderboardScoreData(leaderboardId, status);
            var scoresBuffer = leaderboardScoresJava.Call<AndroidJavaObject>("getScores");
            int count = scoresBuffer.Call<int>("getCount");
            for (int i = 0; i < count; ++i)
            {
                using (var leaderboardScore = scoresBuffer.Call<AndroidJavaObject>("get", i))
                {
                    long timestamp = leaderboardScore.Call<long>("getTimestampMillis");
                    System.DateTime date = AndroidJavaConverter.ToDateTime(timestamp);

                    ulong rank = (ulong) leaderboardScore.Call<long>("getRank");
                    string scoreHolderId = "";
                    using (var scoreHolder = leaderboardScore.Call<AndroidJavaObject>("getScoreHolder"))
                    {
                        scoreHolderId = scoreHolder.Call<string>("getPlayerId");
                    }

                    ulong score = (ulong) leaderboardScore.Call<long>("getRawScore");
                    string metadata = leaderboardScore.Call<string>("getScoreTag");

                    leaderboardScoreData.AddScore(new PlayGamesScore(date, leaderboardId,
                        rank, scoreHolderId, score, metadata));
                }
            }

            leaderboardScoreData.NextPageToken = new ScorePageToken(scoresBuffer, leaderboardId, collection,
                timespan, ScorePageDirection.Forward);
            leaderboardScoreData.PrevPageToken = new ScorePageToken(scoresBuffer, leaderboardId, collection,
                timespan, ScorePageDirection.Backward);

            using (var leaderboard = leaderboardScoresJava.Call<AndroidJavaObject>("getLeaderboard"))
            using (var variants = leaderboard.Call<AndroidJavaObject>("getVariants"))
            using (var variant = variants.Call<AndroidJavaObject>("get", 0))
            {
                leaderboardScoreData.Title = leaderboard.Call<string>("getDisplayName");
                if (variant.Call<bool>("hasPlayerInfo"))
                {
                    System.DateTime date = AndroidJavaConverter.ToDateTime(0);
                    ulong rank = (ulong) variant.Call<long>("getPlayerRank");
                    ulong score = (ulong) variant.Call<long>("getRawPlayerScore");
                    string metadata = variant.Call<string>("getPlayerScoreTag");
                    leaderboardScoreData.PlayerScore = new PlayGamesScore(date, leaderboardId,
                        rank, mUser.id, score, metadata);
                }

                leaderboardScoreData.ApproximateCount = (ulong) variant.Call<long>("getNumScores");
            }

            return leaderboardScoreData;
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SubmitScore"/>
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
            }

            using (var client = getLeaderboardsClient())
            {
                client.Call("submitScore", leaderboardId, score);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SubmitScore"/>
        public void SubmitScore(string leaderboardId, long score, string metadata,
            Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
            }

            using (var client = getLeaderboardsClient())
            {
                client.Call("submitScore", leaderboardId, score, metadata);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        public void RequestPermissions(string[] scopes, Action<SignInStatus> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            mTokenClient.RequestPermissions(scopes, code =>
            {
                UpdateClients();
                callback(code);
            });
        }

        private void UpdateClients()
        {
            lock (GameServicesLock)
            {
                var account = mTokenClient.GetAccount();
                mSavedGameClient = new AndroidSavedGameClient(this, account);
                mEventsClient = new AndroidEventsClient(account);
                mVideoClient = new AndroidVideoClient(mVideoClient.IsCaptureSupported(), account);
                mRealTimeClient = new AndroidRealTimeMultiplayerClient(this, account);
                mTurnBasedClient = new AndroidTurnBasedMultiplayerClient(this, account);
                mTurnBasedClient.RegisterMatchDelegate(mConfiguration.MatchDelegate);
            }
        }

        /// <summary>Returns whether or not user has given permissions for given scopes.</summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.HasPermissions"/>
        public bool HasPermissions(string[] scopes)
        {
            return mTokenClient.HasPermissions(scopes);
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
                mInvitationDelegate = AsOnGameThreadCallback<Invitation, bool>(
                    (invitation, autoAccept) => invitationDelegate(invitation, autoAccept));
            }
        }

        private class InvitationCallbackProxy : AndroidJavaProxy
        {
            private Action<Invitation, bool> mInvitationDelegate;

            public InvitationCallbackProxy(Action<Invitation, bool> invitationDelegate)
                : base("com/google/games/bridge/InvitationCallbackProxy$Callback")
            {
                mInvitationDelegate = invitationDelegate;
            }

            public void onInvitationReceived(AndroidJavaObject invitation)
            {
                mInvitationDelegate.Invoke(AndroidJavaConverter.ToInvitation(invitation), /* shouldAutoAccept= */
                    false);
            }

            public void onInvitationRemoved(string invitationId)
            {
            }
        }

        private AndroidJavaObject getAchievementsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getAchievementsClient",
                AndroidHelperFragment.GetActivity(), mTokenClient.GetAccount());
        }

        private AndroidJavaObject getGamesClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getGamesClient", AndroidHelperFragment.GetActivity(),
                mTokenClient.GetAccount());
        }

        private AndroidJavaObject getInvitationsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getInvitationsClient",
                AndroidHelperFragment.GetActivity(), mTokenClient.GetAccount());
        }

        private AndroidJavaObject getPlayersClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getPlayersClient", AndroidHelperFragment.GetActivity(),
                mTokenClient.GetAccount());
        }

        private AndroidJavaObject getLeaderboardsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getLeaderboardsClient",
                AndroidHelperFragment.GetActivity(), mTokenClient.GetAccount());
        }

        private AndroidJavaObject getPlayerStatsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getPlayerStatsClient",
                AndroidHelperFragment.GetActivity(), mTokenClient.GetAccount());
        }

        private AndroidJavaObject getVideosClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getVideosClient", AndroidHelperFragment.GetActivity(),
                mTokenClient.GetAccount());
        }
    }
}
#endif
