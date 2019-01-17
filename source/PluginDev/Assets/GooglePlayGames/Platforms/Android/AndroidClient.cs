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

namespace GooglePlayGames.Android
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
        private volatile ITurnBasedMultiplayerClient mTurnBasedClient;
        private volatile IRealTimeMultiplayerClient mRealTimeClient;
        private volatile ISavedGameClient mSavedGameClient;
        private volatile IEventsClient mEventsClient;
        private volatile IVideoClient mVideoClient;
        private volatile AndroidTokenClient mTokenClient;
        private volatile Action<Invitation, bool> mInvitationDelegate;
        private volatile Dictionary<String, Achievement> mAchievements = null;
        private volatile Player mUser = null;
        private volatile List<Player> mFriends = null;
        private volatile AuthState mAuthState = AuthState.Unauthenticated;
        private volatile uint mAuthGeneration = 0;
        private volatile bool friendsLoading = false;

        AndroidJavaClass mGamesClient = new AndroidJavaClass("com/google/android/gms/games/Games");
        AndroidJavaObject mPlayersClient;

        internal AndroidClient(PlayGamesClientConfiguration configuration)
        {
            PlayGamesHelperObject.CreateObject();
            this.mConfiguration = Misc.CheckNotNull(configuration);
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
                    mPlayersClient = mGamesClient.CallStatic<AndroidJavaObject>("getPlayersClient", AndroidTokenClient.GetActivity(), mTokenClient.GetAccount());
                    AndroidJavaObject task = mPlayersClient.Call<AndroidJavaObject>("getCurrentPlayer");
                    task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy(
                        player => {
                            string displayName = player.Call<String>("getDisplayName");
                            string playerId = player.Call<String>("getPlayerId");
                            string avatarUrl = player.Call<String>("getIconImageUrl");
                            mUser = new Player(displayName, playerId, avatarUrl);
                            lock (AuthStateLock)
                            {
                                mAuthState = AuthState.Authenticated;
                                InvokeCallbackOnGameThread(callback, true, "Authentication succeed");
                            }    
                        }
                    ));
                    task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                        exception => {
                            lock (AuthStateLock)
                            {
                                InvokeCallbackOnGameThread(callback, false, "Authentication failed");
                            }
                        }
                    ));
                } else {
                    lock (AuthStateLock)
                    {
                        if (result == 16 /* CommonStatusCodes.CANCELED */) {
                            InvokeCallbackOnGameThread(callback, false, "Authentication canceled");
                        } else if (result == 8 /* CommonStatusCodes.DEVELOPER_ERROR */) {
                            InvokeCallbackOnGameThread(callback, false, "Authentication failed - developer error");
                        } else {
                            InvokeCallbackOnGameThread(callback, false, "Authentication failed");
                        }
                    }
                }
            });
        }

        private class TaskOnSuccessProxy : AndroidJavaProxy
        {
            private Action<AndroidJavaObject> mCallback;

            public TaskOnSuccessProxy(Action<AndroidJavaObject> callback) 
            : base("com/google/android/gms/tasks/OnSuccessListener")
            {
                mCallback = callback;
            }

            public void onSuccess(AndroidJavaObject result)
            {
                mCallback(result);
            }
        }

        private class TaskOnFailedProxy : AndroidJavaProxy
        {
            private Action<AndroidJavaObject> mCallback;

            public TaskOnFailedProxy(Action<AndroidJavaObject> callback) 
            : base("com/google/android/gms/tasks/OnFailureListener")
            {
                mCallback = callback;
            }

            public void onFailure(AndroidJavaObject exception)
            {
                mCallback(exception);
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
            if (mTokenClient != null) {
                return;
            }
            InitializeTokenClient();
        }

        private void InitializeTokenClient() {
            if (mTokenClient != null) {
                return;
            }
            mTokenClient = new AndroidTokenClient();

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

        }

        public IUserProfile[] GetFriends()
        {
            return new IUserProfile[0];
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SignOut"/>
        public void SignOut()
        {
            if (mTokenClient == null)
            {
                return;
            }

            mTokenClient.Signout();
            mAuthState = AuthState.Unauthenticated;
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
        }


        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetPlayerStats"/>
        public void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            // Temporary fix to get SpendProbability until the
            // C++ SDK supports it.
//            PlayGamesHelperObject.RunOnGameThread(() =>
                //clientImpl.GetPlayerStats(GetApiClient(), callback));
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadUsers"/>
        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
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
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.RevealAchievement"/>
        public void RevealAchievement(string achId, Action<bool> callback)
        {
        }

        private void UpdateAchievement(string updateType, string achId, Action<bool> callback,
                                       Predicate<Achievement> alreadyDone, Action<Achievement> updateAchievment)
        {
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.IncrementAchievement"/>
        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SetStepsAtLeast"/>
        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.ShowAchievementsUI"/>
        public void ShowAchievementsUI(Action<UIStatus> cb)
        {
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LeaderboardMaxResults"/>
        public int LeaderboardMaxResults()
        {
            return 0;
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
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadScores"/>
        public void LoadScores(string leaderboardId, LeaderboardStart start,
            int rowCount, LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback)
        {
        }

        ///<summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadMoreScores"/>
        public void LoadMoreScores(ScorePageToken token, int rowCount,
            Action<LeaderboardScoreData> callback)
        {
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
            return  IntPtr.Zero;
        }
    }
}
#endif
