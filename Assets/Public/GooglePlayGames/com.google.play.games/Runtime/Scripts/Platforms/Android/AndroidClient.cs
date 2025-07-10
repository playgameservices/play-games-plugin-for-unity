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
    using GooglePlayGames.BasicApi.Events;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames.OurUtils;
    using System;
    using System.Linq;
    using System.Collections.Generic;
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
        private const string PlayGamesSdkClassName = "com.google.android.gms.games.PlayGamesSdk";

        private volatile ISavedGameClient mSavedGameClient;
        private volatile IEventsClient mEventsClient;
        private volatile PlayGamesUserProfile mUser = null;
        private volatile AuthState mAuthState = AuthState.Unauthenticated;
        private IUserProfile[] mFriends = new IUserProfile[0];
        private LoadFriendsStatus mLastLoadFriendsStatus = LoadFriendsStatus.Unknown;

        AndroidJavaClass mGamesClass = new AndroidJavaClass("com.google.android.gms.games.PlayGames");
        private static string TasksClassName = "com.google.android.gms.tasks.Tasks";

        private AndroidJavaObject mFriendsResolutionException = null;

        private readonly int mLeaderboardMaxResults = 25; // can be from 1 to 25

        private readonly int mFriendsMaxResults = 200; // the maximum load friends page size

        internal AndroidClient()
        {
            PlayGamesHelperObject.CreateObject();
            InitializeSdk();
        }

        private static void InitializeSdk() {
            using (var playGamesSdkClass = new AndroidJavaClass(PlayGamesSdkClassName)) {
                playGamesSdkClass.CallStatic("initialize", AndroidHelperFragment.GetActivity());
            }
        }

        public void Authenticate(Action<SignInStatus> callback)
        {
            Authenticate( /* isAutoSignIn= */ true, callback);
        }

        public void ManuallyAuthenticate(Action<SignInStatus> callback)
        {
            Authenticate( /* isAutoSignIn= */ false, callback);
        }

        private void Authenticate(bool isAutoSignIn, Action<SignInStatus> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            lock (AuthStateLock)
            {
                // If the user is already authenticated, just fire the callback, we don't need
                // any additional work.
                if (isAutoSignIn && mAuthState == AuthState.Authenticated)
                {
                    OurUtils.Logger.d("Already authenticated.");
                    InvokeCallbackOnGameThread(callback, SignInStatus.Success);
                    return;
                }
            }

            string methodName = isAutoSignIn ? "isAuthenticated" : "signIn";

            OurUtils.Logger.d("Starting Auth using the method " + methodName);
            using (var client = getGamesSignInClient())
            using (var task = client.Call<AndroidJavaObject>(methodName))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(authenticationResult => {
                    bool isAuthenticated = authenticationResult.Call<bool>("isAuthenticated");
                    if (!isAuthenticated)
                    {
                        lock (AuthStateLock)
                        {
                            OurUtils.Logger.e("Not authenticated");
                            callback(SignInStatus.Canceled);
                            return;
                        }
                    }

                    using (var client = getPlayersClient())
                    using (client.Call<AndroidJavaObject>("getCurrentPlayer").AddOnSuccessListener((AndroidJavaObject resultObject) => {
                        mUser = AndroidJavaConverter.ToPlayer(resultObject);

                        lock (GameServicesLock)
                        {
                            mSavedGameClient = new AndroidSavedGameClient(this);
                            mEventsClient = new AndroidEventsClient();
                        }

                        mAuthState = AuthState.Authenticated;
                        InvokeCallbackOnGameThread(callback, SignInStatus.Success);
                        OurUtils.Logger.d("Authentication succeeded");
                        LoadAchievements(ignore => { });
                    }).AddOnFailureListener((exception) => {
                        OurUtils.Logger.e("GetCurrentPlayer failed - " + exception.Call<string>("toString"));
                        callback(SignInStatus.InternalError);
                    }).AddOnCanceledListener(() => {
                        callback(SignInStatus.Canceled);
                    }));
                }).AddOnFailureListener(exception => {
                    OurUtils.Logger.e("Authentication failed - " + exception.Call<string>("toString"));
                    callback(SignInStatus.InternalError);
                }).AddOnCanceledListener(() => {
                    callback(SignInStatus.Canceled);
                });
            }
        }

        static string appID = null;
        public static string AppId
        {
            get
            {
                if(appID == null)
                    appID = AndroidHelperFragment.CallPackageMetaData<string>("GET_META_DATA","getString","com.google.android.gms.games.APP_ID") ?? "";

                return appID;
            }
        }

        static string webClientId = null;
        public static string WebClientId
        {
            get
            {
                if(webClientId == null)
                    webClientId = AndroidHelperFragment.CallPackageMetaData<string>("GET_META_DATA","getString","com.google.android.gms.games.WEB_CLIENT_ID") ?? "";

                return webClientId;
            }
        }

        public void RequestServerSideAccess(bool forceRefreshToken, Action<string> callback)
        {
            callback = AsOnGameThreadCallback(callback);

            if (!GameInfo.WebClientIdInitialized())
            {
                throw new InvalidOperationException("Requesting server side access requires web " +
                                                    "client id to be configured.");
            }

            using (var client = getGamesSignInClient())
            using (var task = client.Call<AndroidJavaObject>("requestServerSideAccess",
                GameInfo.WebClientId, forceRefreshToken))
            {
                AndroidTaskUtils.AddOnSuccessListener<string>(
                    task,
                    authCode => callback(authCode)
                );

                AndroidTaskUtils.AddOnFailureListener(task, exception =>
                {
                    OurUtils.Logger.e("Requesting server side access task failed - " +
                                      exception.Call<string>("toString"));
                    callback(null);
                });
            }
        }

        public void RequestServerSideAccess(bool forceRefreshToken, List<AuthScope> scopes, Action<AuthResponse> callback)
        {
            callback = AsOnGameThreadCallback(callback);

            if (string.IsNullOrEmpty(WebClientId) || !WebClientId.StartsWith(AppId) || !WebClientId.EndsWith(".googleusercontent.com"))
                throw new InvalidOperationException("Requesting server side access requires web client id to be configured.");

            if (scopes == null)
            {
                throw new ArgumentException("At least one scope must be provided.");
            }

            var javaScopesList = new AndroidJavaObject("java.util.ArrayList");

            foreach (var scope in scopes)
            {
                javaScopesList.Call<bool>("add", getJavaScopeEnum(scope));
            }

            using (var client = getGamesSignInClient())
            using (var task = client.Call<AndroidJavaObject>("requestServerSideAccess", WebClientId, forceRefreshToken))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(authCode => callback(ToAuthResponse(authCode))).AddOnFailureListener((exception) => {
                    OurUtils.Logger.e("Requesting server side access task failed - " + exception.Call<string>("toString"));
                    callback(new AuthResponse(null, new List<AuthScope>())); // Return empty response on failure
                });
            }
        }

        private AuthResponse ToAuthResponse(AndroidJavaObject result)
        {
            string authCode = result.Call<string>("getAuthCode");
            var grantedScopesObject = result.Call<AndroidJavaObject>("getGrantedScopes");

            var grantedScopesList = new List<AuthScope>();
            if (grantedScopesObject != null)
            {
                int size = grantedScopesObject.Call<int>("size");
                for (int i = 0; i < size; i++)
                {
                    var javaScopeEnum = grantedScopesObject.Call<AndroidJavaObject>("get", i);
                    string javaScopeName = javaScopeEnum.Call<string>("name");
                    if (Enum.TryParse(javaScopeName, out AuthScope grantedScope))
                    {
                        grantedScopesList.Add(grantedScope);
                    }
                    else
                    {
                        OurUtils.Logger.w($"Unrecognized scope {javaScopeName} returned from java side.");
                    }
                }
            }
            AuthResponse authResponse = new AuthResponse(authCode, grantedScopesList);
            return authResponse;
        }

        private AndroidJavaObject getJavaScopeEnum(AuthScope scope)
        {
            String AuthScopeClassName = "com.google.android.gms.games.gamessignin.AuthScope";
            var javaAuthScopeClass = new AndroidJavaClass(AuthScopeClassName);
            return javaAuthScopeClass.CallStatic<AndroidJavaObject>("valueOf", scope.GetValue());
        }


        public void RequestRecallAccessToken(Action<RecallAccess> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            using (var client = getRecallClient())
            using (var task = client.Call<AndroidJavaObject>("requestRecallAccess"))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    recallAccess => {
                        var sessionId = recallAccess.Call<string>("getSessionId");
                        callback(new RecallAccess(sessionId));
                    }
                );

                AndroidTaskUtils.AddOnFailureListener(task, exception =>
                {
                    OurUtils.Logger.e("Requesting Recall access task failed - " +
                                      exception.Call<string>("toString"));
                    callback(null);
                });
            }
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

            PlayGamesHelperObject.RunOnGameThread(() => { callback(); });
        }

        private static void InvokeCallbackOnGameThread<T>(Action<T> callback, T data)
        {
            if (callback == null)
            {
                return;
            }

            PlayGamesHelperObject.RunOnGameThread(() => { callback(data); });
        }


        private static Action<T1, T2> AsOnGameThreadCallback<T1, T2>(Action<T1, T2> toInvokeOnGameThread)
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

            PlayGamesHelperObject.RunOnGameThread(() => { callback(t1, t2); });
        }

        public bool IsAuthenticated()
        {
            lock (AuthStateLock)
            {
                return mAuthState == AuthState.Authenticated;
            }
        }

        public void LoadFriends(Action<bool> callback)
        {
            LoadAllFriends(mFriendsMaxResults, /* forceReload= */ false, /* loadMore= */ false, callback);
        }

        private void LoadAllFriends(int pageSize, bool forceReload, bool loadMore, Action<bool> callback)
        {
            LoadFriendsPaginated(pageSize, loadMore, forceReload, result => {
                mLastLoadFriendsStatus = result;
                switch (result)
                {
                    case LoadFriendsStatus.Completed:
                        InvokeCallbackOnGameThread(callback, true);
                        break;
                    case LoadFriendsStatus.LoadMore:
                        // There are more friends to load.
                        LoadAllFriends(pageSize, /* forceReload= */ false, /* loadMore= */ true, callback);
                        break;
                    case LoadFriendsStatus.ResolutionRequired:
                    case LoadFriendsStatus.InternalError:
                    case LoadFriendsStatus.NotAuthorized:
                        InvokeCallbackOnGameThread(callback, false);
                        break;
                    default:
                        GooglePlayGames.OurUtils.Logger.d("There was an error when loading friends." + result);
                        InvokeCallbackOnGameThread(callback, false);
                        break;
                }
            });
        }

        public void LoadFriends(int pageSize, bool forceReload, Action<LoadFriendsStatus> callback)
        {
            LoadFriendsPaginated(pageSize, /* isLoadMore= */ false, /* forceReload= */ forceReload,
                callback);
        }

        public void LoadMoreFriends(int pageSize, Action<LoadFriendsStatus> callback)
        {
            LoadFriendsPaginated(pageSize, /* isLoadMore= */ true, /* forceReload= */ false,
                callback);
        }

        private void LoadFriendsPaginated(int pageSize, bool isLoadMore, bool forceReload, Action<LoadFriendsStatus> callback)
        {
            mFriendsResolutionException = null;
            using (var playersClient = getPlayersClient())
            using (var task = isLoadMore
                ? playersClient.Call<AndroidJavaObject>("loadMoreFriends", pageSize)
                : playersClient.Call<AndroidJavaObject>("loadFriends", pageSize, forceReload))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
                    using (var playersBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                    {
                        AndroidJavaObject metadata = playersBuffer.Call<AndroidJavaObject>("getMetadata");
                        var areMoreFriendsToLoad = metadata != null &&
                                                    metadata.Call<AndroidJavaObject>("getString",
                                                        "next_page_token") != null;
                        mFriends = AndroidJavaConverter.playersBufferToArray(playersBuffer);
                        mLastLoadFriendsStatus = areMoreFriendsToLoad
                            ? LoadFriendsStatus.LoadMore
                            : LoadFriendsStatus.Completed;
                        InvokeCallbackOnGameThread(callback, mLastLoadFriendsStatus);
                    }
                }).AddOnFailureListener(exception => {
                    AndroidHelperFragment.IsResolutionRequired(exception, resolutionRequired =>
                    {
                        if (resolutionRequired)
                        {
                            mFriendsResolutionException =
                                exception.Call<AndroidJavaObject>("getResolution");
                            mLastLoadFriendsStatus = LoadFriendsStatus.ResolutionRequired;
                            mFriends = new IUserProfile[0];
                            InvokeCallbackOnGameThread(callback, LoadFriendsStatus.ResolutionRequired);
                        }
                        else
                        {
                            mFriendsResolutionException = null;

                            if (IsApiException(exception))
                            {
                                var statusCode = exception.Call<int>("getStatusCode");
                                if (statusCode == /* GamesClientStatusCodes.NETWORK_ERROR_NO_DATA */ 26504)
                                {
                                    mLastLoadFriendsStatus = LoadFriendsStatus.NetworkError;
                                    InvokeCallbackOnGameThread(callback, LoadFriendsStatus.NetworkError);
                                    return;
                                }
                            }

                            mLastLoadFriendsStatus = LoadFriendsStatus.InternalError;
                            OurUtils.Logger.e("LoadFriends failed: " + exception.Call<string>("toString"));
                            InvokeCallbackOnGameThread(callback, LoadFriendsStatus.InternalError);
                        }
                    });
                    return;
                });
            }
        }

        private static bool IsApiException(AndroidJavaObject exception) {
            var exceptionClassName = exception.Call<AndroidJavaObject>("getClass").Call<String>("getName");
            return exceptionClassName == "com.google.android.gms.common.api.ApiException";
        }

        public LoadFriendsStatus GetLastLoadFriendsStatus()
        {
            return mLastLoadFriendsStatus;
        }

        public void AskForLoadFriendsResolution(Action<UIStatus> callback)
        {
            if (mFriendsResolutionException == null)
            {
                GooglePlayGames.OurUtils.Logger.d("The developer asked for access to the friends " +
                                                  "list but there is no intent to trigger the UI. This may be because the user " +
                                                  "has granted access already or the game has not called loadFriends() before.");
                using (var playersClient = getPlayersClient())
                using (var task = playersClient.Call<AndroidJavaObject>("loadFriends", /* pageSize= */ 1, /* forceReload= */ false))
                {
                    task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
                        InvokeCallbackOnGameThread(callback, UIStatus.Valid);
                    }).AddOnFailureListener(exception => {
                        AndroidHelperFragment.IsResolutionRequired(exception, resolutionRequired =>
                        {
                            if (resolutionRequired)
                            {
                                mFriendsResolutionException =
                                    exception.Call<AndroidJavaObject>("getResolution");
                                AndroidHelperFragment.AskForLoadFriendsResolution(
                                    mFriendsResolutionException, AsOnGameThreadCallback(callback));
                                return;
                            }

                            if (IsApiException(exception))
                            {
                                var statusCode = exception.Call<int>("getStatusCode");
                                if (statusCode ==
                                    /* GamesClientStatusCodes.NETWORK_ERROR_NO_DATA */ 26504)
                                {
                                    InvokeCallbackOnGameThread(callback, UIStatus.NetworkError);
                                    return;
                                }
                            }

                            OurUtils.Logger.e("LoadFriends failed: " + exception.Call<string>("toString"));
                            InvokeCallbackOnGameThread(callback, UIStatus.InternalError);
                        });
                    });
                }
            }
            else
            {
                AndroidHelperFragment.AskForLoadFriendsResolution(mFriendsResolutionException,AsOnGameThreadCallback(callback));
            }
        }

        public void ShowCompareProfileWithAlternativeNameHintsUI(string playerId,
            string otherPlayerInGameName,
            string currentPlayerInGameName,
            Action<UIStatus> callback)
        {
            AndroidHelperFragment.ShowCompareProfileWithAlternativeNameHintsUI(
                playerId, otherPlayerInGameName, currentPlayerInGameName,
                AsOnGameThreadCallback(callback));
        }

        public void GetFriendsListVisibility(bool forceReload, Action<FriendsListVisibilityStatus> callback)
        {
            using (var playersClient = getPlayersClient())
            using (var task = playersClient.Call<AndroidJavaObject>("getCurrentPlayer", forceReload))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
                    AndroidJavaObject currentPlayerInfo =
                        annotatedData.Call<AndroidJavaObject>("get").Call<AndroidJavaObject>(
                            "getCurrentPlayerInfo");
                    int playerListVisibility =
                        currentPlayerInfo.Call<int>("getFriendsListVisibilityStatus");
                    InvokeCallbackOnGameThread(callback,
                        AndroidJavaConverter.ToFriendsListVisibilityStatus(playerListVisibility));
                }).AddOnFailureListener(exception => {
                    InvokeCallbackOnGameThread(callback, FriendsListVisibilityStatus.NetworkError);
                });
            }
        }

        public IUserProfile[] GetFriends()
        {
            return mFriends;
        }

        public string GetUserId()
        {
            if (mUser == null)
            {
                return null;
            }

            return mUser.id;
        }

        public string GetUserDisplayName()
        {
            if (mUser == null)
            {
                return null;
            }

            return mUser.userName;
        }

        public string GetUserImageUrl()
        {
            if (mUser == null)
            {
                return null;
            }

            return mUser.AvatarURL;
        }

        public void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            using (var playerStatsClient = getPlayerStatsClient())
            using (var task = playerStatsClient.Call<AndroidJavaObject>("loadPlayerStats", /* forceReload= */ false))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
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
                }).AddOnFailureListener(exception => {
                    OurUtils.Logger.e("GetPlayerStats failed: " + exception.Call<string>("toString"));
                    var statusCode = IsAuthenticated()
                        ? CommonStatusCodes.InternalError
                        : CommonStatusCodes.SignInRequired;
                    InvokeCallbackOnGameThread(callback, statusCode, new PlayerStats());
                });
            }
        }

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
                        task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
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
                        }).AddOnFailureListener(exception => {
                            OurUtils.Logger.e("LoadUsers failed for index " + i +
                                              " with: " + exception.Call<string>("toString"));
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

        public void LoadAchievements(Action<Achievement[]> callback)
        {
            using (var achievementsClient = getAchievementsClient())
            using (var task = achievementsClient.Call<AndroidJavaObject>("load", /* forceReload= */ false))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
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
                }).AddOnFailureListener(exception => {
                    OurUtils.Logger.e("LoadAchievements failed: " + exception.Call<string>("toString"));
                    InvokeCallbackOnGameThread(callback, new Achievement[0]);
                });
            }
        }

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

        public void ShowAchievementsUI(Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, UIStatus.NotAuthorized);
                return;
            }

            AndroidHelperFragment.ShowAchievementsUI(AsOnGameThreadCallback(callback));
        }

        public int LeaderboardMaxResults()
        {
            return mLeaderboardMaxResults;
        }

        public void ShowLeaderboardUI(string leaderboardId, LeaderboardTimeSpan span, Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, UIStatus.NotAuthorized);
                return;
            }

            if (leaderboardId == null)
            {
                AndroidHelperFragment.ShowAllLeaderboardsUI(AsOnGameThreadCallback(callback));
            }
            else
            {
                AndroidHelperFragment.ShowLeaderboardUI(leaderboardId, span,
                    AsOnGameThreadCallback(callback));
            }
        }

        public void LoadScores(string leaderboardId, LeaderboardStart start,
            int rowCount, LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback)
        {
            using (var client = getLeaderboardsClient())
            using (var task = client.Call<AndroidJavaObject>(
                start == LeaderboardStart.TopScores ? "loadTopScores" : "loadPlayerCenteredScores",
                leaderboardId,
                AndroidJavaConverter.ToLeaderboardVariantTimeSpan(timeSpan),
                AndroidJavaConverter.ToLeaderboardVariantCollection(collection),
                rowCount))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
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
                }).AddOnFailureListener(exception => {
                    AndroidHelperFragment.IsResolutionRequired(exception, resolutionRequired => {
                        if (resolutionRequired)
                        {
                            mFriendsResolutionException = exception.Call<AndroidJavaObject>("getResolution");
                            InvokeCallbackOnGameThread(
                                callback, new LeaderboardScoreData(leaderboardId,ResponseStatus.ResolutionRequired));
                        }
                        else
                        {
                            mFriendsResolutionException = null;
                        }
                    });

                    OurUtils.Logger.e("LoadScores failed: " + exception.Call<string>("toString"));
                    InvokeCallbackOnGameThread(callback, new LeaderboardScoreData(leaderboardId,ResponseStatus.InternalError));
                });
            }
        }

        public void LoadMoreScores(ScorePageToken token, int rowCount, Action<LeaderboardScoreData> callback)
        {
            using (var client = getLeaderboardsClient())
            using (var task = client.Call<AndroidJavaObject>("loadMoreScores",
                token.InternalObject, rowCount, AndroidJavaConverter.ToPageDirection(token.Direction)))
            {
                task.AddOnSuccessListener<AndroidJavaObject>(annotatedData => {
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
                }).AddOnFailureListener(exception => {
                    AndroidHelperFragment.IsResolutionRequired(exception, resolutionRequired => {
                        if (resolutionRequired)
                        {
                            mFriendsResolutionException =
                                exception.Call<AndroidJavaObject>("getResolution");
                            InvokeCallbackOnGameThread(
                                callback, new LeaderboardScoreData(token.LeaderboardId,
                                    ResponseStatus.ResolutionRequired));
                        }
                        else
                        {
                            mFriendsResolutionException = null;
                        }
                    });
                    OurUtils.Logger.e("LoadMoreScores failed: " + exception.Call<string>("toString"));
                    InvokeCallbackOnGameThread(
                        callback, new LeaderboardScoreData(token.LeaderboardId,
                            ResponseStatus.InternalError));
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

        public void SubmitScore(string leaderboardId, long score, string metadata, Action<bool> callback)
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

        public ISavedGameClient GetSavedGameClient()
        {
            lock (GameServicesLock)
            {
                return mSavedGameClient;
            }
        }

        public IEventsClient GetEventsClient()
        {
            lock (GameServicesLock)
            {
                return mEventsClient;
            }
        }

        private AndroidJavaObject getAchievementsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getAchievementsClient",
                AndroidHelperFragment.GetActivity());
        }

        private AndroidJavaObject getPlayersClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getPlayersClient", AndroidHelperFragment.GetActivity());
        }

        private AndroidJavaObject getLeaderboardsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getLeaderboardsClient",
                AndroidHelperFragment.GetActivity());
        }

        private AndroidJavaObject getPlayerStatsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getPlayerStatsClient",
                AndroidHelperFragment.GetActivity());
        }

        private AndroidJavaObject getGamesSignInClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getGamesSignInClient",
                AndroidHelperFragment.GetActivity());
        }

        private AndroidJavaObject getRecallClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getRecallClient",
                AndroidHelperFragment.GetActivity());
        }
    }
}
#endif