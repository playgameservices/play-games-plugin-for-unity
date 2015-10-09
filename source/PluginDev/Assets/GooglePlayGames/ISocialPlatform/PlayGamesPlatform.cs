// <copyright file="PlayGamesPlatform.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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

namespace GooglePlayGames
{
    using System;
    using UnityEngine.SocialPlatforms;
    using System.Collections.Generic;
    using UnityEngine;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames.BasicApi.Quests;
    using GooglePlayGames.BasicApi.Events;
    using GooglePlayGames.BasicApi.Nearby;

    /// <summary>
    /// Provides access to the Google Play Games platform. This is an implementation of
    /// UnityEngine.SocialPlatforms.ISocialPlatform. Activate this platform by calling
    /// the <see cref="Activate" /> method, then authenticate by calling
    /// the <see cref="Authenticate" /> method. After authentication
    /// completes, you may call the other methods of this class. This is not a complete
    /// implementation of the ISocialPlatform interface. Methods lacking an implementation
    /// or whose behavior is at variance with the standard are noted as such.
    /// </summary>
    public class PlayGamesPlatform : ISocialPlatform
    {
        private static volatile PlayGamesPlatform sInstance = null;

        private readonly PlayGamesClientConfiguration mConfiguration;
        private PlayGamesLocalUser mLocalUser = null;
        private IPlayGamesClient mClient = null;

        private volatile static bool sNearbyInitializePending;
        private volatile static INearbyConnectionClient sNearbyConnectionClient;

        // the default leaderboard we show on ShowLeaderboardUI
        private string mDefaultLbUi = null;

        // achievement/leaderboard ID mapping table
        private Dictionary<string, string> mIdMap = new Dictionary<string, string>();

        private PlayGamesPlatform(PlayGamesClientConfiguration configuration)
        {
            this.mLocalUser = new PlayGamesLocalUser(this);
            this.mConfiguration = configuration;
        }

        internal PlayGamesPlatform(IPlayGamesClient client)
        {
            this.mClient = Misc.CheckNotNull(client);
            this.mLocalUser = new PlayGamesLocalUser(this);
            this.mConfiguration = PlayGamesClientConfiguration.DefaultConfiguration;
        }

        public static void InitializeInstance(PlayGamesClientConfiguration configuration)
        {
            if (sInstance != null)
            {
                Logger.w("PlayGamesPlatform already initialized. Ignoring this call.");
                return;
            }

            sInstance = new PlayGamesPlatform(configuration);
        }

        /// <summary>
        /// Gets the singleton instance of the Play Games platform.
        /// </summary>
        /// <returns>
        /// The instance.
        /// </returns>
        public static PlayGamesPlatform Instance
        {
            get
            {
                if (sInstance == null)
                {
                    Logger.d("Instance was not initialized, using default configuration.");
                    InitializeInstance(PlayGamesClientConfiguration.DefaultConfiguration);
                }

                return sInstance;
            }
        }

        public static void InitializeNearby(Action<INearbyConnectionClient> callback)
        {
            Debug.Log("Calling InitializeNearby!");
            if (sNearbyConnectionClient == null)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                NearbyConnectionClientFactory.Create(client => {
                    Debug.Log("Nearby Client Created!!");
                    sNearbyConnectionClient = client;
                    if (callback != null) {
                        callback.Invoke(client);
                    }
                    else {
                        Debug.Log("Initialize Nearby callback is null");
                    }
                });
#else
                sNearbyConnectionClient = new DummyNearbyConnectionClient();
                if (callback != null)
                {
                    callback.Invoke(sNearbyConnectionClient);
                }

#endif
            }
            else if (callback != null)
            {
                Debug.Log("Nearby Already initialized: calling callback directly");
                callback.Invoke(sNearbyConnectionClient);
            }
            else
            {
                Debug.Log("Nearby Already initialized");
            }
        }

        /// <summary>
        /// Gets the nearby connection client.  NOTE: Can be null until the nearby client
        /// is initialized.  Call InitializeNearby to use callback to be notified when initialization
        /// is complete.
        /// </summary>
        /// <value>The nearby.</value>
        public static INearbyConnectionClient Nearby
        {
            get
            {
                if (sNearbyConnectionClient == null && !sNearbyInitializePending)
                {
                    sNearbyInitializePending = true;
                    InitializeNearby(null);
                }

                return sNearbyConnectionClient;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether debug logs are enabled. This property
        /// may be set before calling <see cref="Activate" /> method.
        /// </summary>
        /// <returns>
        /// <c>true</c> if debug log enabled; otherwise, <c>false</c>.
        /// </returns>
        public static bool DebugLogEnabled
        {
            get
            {
                return Logger.DebugLogEnabled;
            }

            set
            {
                Logger.DebugLogEnabled = value;
            }
        }

        /// <summary> Gets the real time multiplayer API object</summary>
        public IRealTimeMultiplayerClient RealTime
        {
            get
            {
                return mClient.GetRtmpClient();
            }
        }

        /// <summary> Gets the turn based multiplayer API object</summary>
        public ITurnBasedMultiplayerClient TurnBased
        {
            get
            {
                return mClient.GetTbmpClient();
            }
        }

        public ISavedGameClient SavedGame
        {
            get
            {
                return mClient.GetSavedGameClient();
            }
        }

        public IEventsClient Events
        {
            get
            {
                return mClient.GetEventsClient();
            }
        }

        public IQuestsClient Quests
        {
            get
            {
                return mClient.GetQuestsClient();
            }
        }

        public IntPtr GetApiClient()
        {
            return mClient.GetApiClient();
        }

        /// <summary>
        /// Activates the Play Games platform as the implementation of Social.Active.
        /// After calling this method, you can call methods on Social.Active. For
        /// example, <c>Social.Active.Authenticate()</c>.
        /// </summary>
        /// <returns>The singleton <see cref="PlayGamesPlatform" /> instance.</returns>
        public static PlayGamesPlatform Activate()
        {
            Logger.d("Activating PlayGamesPlatform.");
            Social.Active = PlayGamesPlatform.Instance;
            Logger.d("PlayGamesPlatform activated: " + Social.Active);
            return PlayGamesPlatform.Instance;
        }

        /// <summary>
        /// Specifies that the ID <c>fromId</c> should be implicitly replaced by <c>toId</c>
        /// on any calls that take a leaderboard or achievement ID. After a mapping is
        /// registered, you can use <c>fromId</c> instead of <c>toId</c> when making a call.
        /// For example, the following two snippets are equivalent:
        ///
        /// <code>
        /// ReportProgress("Cfiwjew894_AQ", 100.0, callback);
        /// </code>
        /// ...is equivalent to:
        /// <code>
        /// AddIdMapping("super-combo", "Cfiwjew894_AQ");
        /// ReportProgress("super-combo", 100.0, callback);
        /// </code>
        ///
        /// </summary>
        /// <param name='fromId'>
        /// The identifier to map.
        /// </param>
        /// <param name='toId'>
        /// The identifier that <c>fromId</c> will be mapped to.
        /// </param>
        public void AddIdMapping(string fromId, string toId)
        {
            mIdMap[fromId] = toId;
        }

        /// <summary>
        /// Authenticate the local user with the Google Play Games service.
        /// </summary>
        /// <param name='callback'>
        /// The callback to call when authentication finishes. It will be called
        /// with <c>true</c> if authentication was successful, <c>false</c>
        /// otherwise.
        /// </param>
        public void Authenticate(Action<bool> callback)
        {
            Authenticate(callback, false);
        }

        /// <summary>
        /// Authenticate the local user with the Google Play Games service.
        /// </summary>
        /// <param name='callback'>
        /// The callback to call when authentication finishes. It will be called
        /// with <c>true</c> if authentication was successful, <c>false</c>
        /// otherwise.
        /// </param>
        /// <param name='silent'>
        /// Indicates whether authentication should be silent. If <c>false</c>,
        /// authentication may show popups and interact with the user to obtain
        /// authorization. If <c>true</c>, there will be no popups or interaction with
        /// the user, and the authentication will fail instead if such interaction
        /// is required. A typical pattern is to try silent authentication on startup
        /// and, if that fails, present the user with a "Sign in" button that then
        /// triggers normal (not silent) authentication.
        /// </param>
        public void Authenticate(Action<bool> callback, bool silent)
        {
            // make a platform-specific Play Games client
            if (mClient == null)
            {
                Logger.d("Creating platform-specific Play Games client.");
                mClient = PlayGamesClientFactory.GetPlatformPlayGamesClient(mConfiguration);
            }

            // authenticate!
            mClient.Authenticate(callback, silent);
        }

        /// <summary>
        ///  Provided for compatibility with ISocialPlatform.
        /// </summary>
        /// <seealso cref="Authenticate(Action&lt;bool&gt;,bool)"/>
        /// <param name="unused">Unused.</param>
        /// <param name="callback">Callback.</param>
        public void Authenticate(ILocalUser unused, Action<bool> callback)
        {
            Authenticate(callback, false);
        }

        /// <summary>
        /// Determines whether the user is authenticated.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAuthenticated()
        {
            return mClient != null && mClient.IsAuthenticated();
        }

        /// <summary>Sign out. After signing out,
        /// Authenticate must be called again to sign back in.
        /// </summary>
        public void SignOut()
        {
            if (mClient != null)
            {
                mClient.SignOut();
            }
        }

        /// <summary>
        /// Loads the user information if available.
        /// <param name="userIds">The user ids to look up</param>
        /// <param name="callback">The callback</param>
        /// </summary>
        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("GetUserId() can only be called after authentication.");
                callback(new IUserProfile[0]);
            }

            mClient.LoadUsers(userIds, callback);
        }

        /// <summary>
        /// Returns the user's Google ID.
        /// </summary>
        /// <returns>
        /// The user's Google ID. No guarantees are made as to the meaning or format of
        /// this identifier except that it is unique to the user who is signed in.
        /// </returns>
        public string GetUserId()
        {
            if (!IsAuthenticated())
            {
                Logger.e("GetUserId() can only be called after authentication.");
                return "0";
            }

            return mClient.GetUserId();
        }

        /// <summary>
        /// Returns an id token for the user.
        /// </summary>
        /// <returns>
        /// An id token for the user.
        /// </returns>
        public string GetIdToken()
        {
            if (mClient != null)
            {
                return mClient.GetIdToken();
            }

            return null;
        }

        /// <summary>
        /// Returns an id token for the user.
        /// </summary>
        /// <returns>
        /// An id token for the user.
        /// </returns>
        public string GetAccessToken()
        {
            if (mClient != null)
            {
                return mClient.GetAccessToken();
            }

            return null;
        }

        /// <summary>
        /// Gets the email of the current user.
        /// This requires additional configuration of permissions in order
        /// to work.
        /// </summary>
        /// <returns>The user email.</returns>
        public string GetUserEmail()
        {
            if (mClient != null)
            {
                return mClient.GetUserEmail();
            }

            return null;
        }

        public void GetPlayerStats(Action<CommonStatusCodes, PlayGamesLocalUser.PlayerStats> callback)
        {
            if (mClient != null && mClient.IsAuthenticated())
            {
                mClient.GetPlayerStats(callback);
            }
            else
            {
                Logger.e("GetPlayerStats can only be called after authentication.");

                callback(CommonStatusCodes.SignInRequired, null);
            }
        }

        /// <summary>
        /// Returns the achievement corresponding to the passed achievement identifier.
        /// </summary>
        /// <returns>
        /// The achievement corresponding to the identifer. <code>null</code> if no such
        /// achievement is found or if the user is not authenticated.
        /// </returns>
        /// <param name="achievementId">
        /// The identifier of the achievement.
        /// </param>
        public Achievement GetAchievement(string achievementId)
        {
            if (!IsAuthenticated())
            {
                Logger.e("GetAchievement can only be called after authentication.");
                return null;
            }

            return mClient.GetAchievement(achievementId);
        }

        /// <summary>
        /// Returns the user's display name.
        /// </summary>
        /// <returns>
        /// The user display name (e.g. "Bruno Oliveira")
        /// </returns>
        public string GetUserDisplayName()
        {
            if (!IsAuthenticated())
            {
                Logger.e("GetUserDisplayName can only be called after authentication.");
                return string.Empty;
            }

            return mClient.GetUserDisplayName();
        }

        /// <summary>
        /// Returns the user's avatar URL if they have one.
        /// </summary>
        /// <returns>
        /// The URL, or <code>null</code> if the user is not authenticated or does not have
        /// an avatar.
        /// </returns>
        public string GetUserImageUrl()
        {
            if (!IsAuthenticated())
            {
                Logger.e("GetUserImageUrl can only be called after authentication.");
                return null;
            }

            return mClient.GetUserImageUrl();
        }

        /// <summary>
        /// Reports the progress of an achievement (reveal, unlock or increment). This method attempts
        /// to implement the expected behavior of ISocialPlatform.ReportProgress as closely as possible,
        /// as described below. Although this method works with incremental achievements for compatibility
        /// purposes, calling this method for incremental achievements is not recommended,
        /// since the Play Games API exposes incremental achievements in a very different way
        /// than the interface presented by ISocialPlatform.ReportProgress. The implementation of this
        /// method for incremental achievements attempts to produce the correct result, but may be
        /// imprecise. If possible, call <see cref="IncrementAchievement" /> instead.
        /// </summary>
        /// <param name='achievementID'>
        /// The ID of the achievement to unlock, reveal or increment. This can be a raw Google Play
        /// Games achievement ID (alphanumeric string), or an alias that was previously configured
        /// by a call to <see cref="AddIdMapping" />.
        /// </param>
        /// <param name='progress'>
        /// Progress of the achievement. If the achievement is standard (not incremental), then
        /// a progress of 0.0 will reveal the achievement and 100.0 will unlock it. Behavior of other
        /// values is undefined. If the achievement is incremental, then this value is interpreted
        /// as the total percentage of the achievement's progress that the player should have
        /// as a result of this call (regardless of the progress they had before). So if the
        /// player's previous progress was 30% and this call specifies 50.0, the new progress will
        /// be 50% (not 80%).
        /// </param>
        /// <param name='callback'>
        /// Callback that will be called to report the result of the operation: <c>true</c> on
        /// success, <c>false</c> otherwise.
        /// </param>
        public void ReportProgress(string achievementID, double progress, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("ReportProgress can only be called after authentication.");
                if (callback != null)
                {
                    callback.Invoke(false);
                }

                return;
            }

            // map ID, if it's in the dictionary
            Logger.d("ReportProgress, " + achievementID + ", " + progress);
            achievementID = MapId(achievementID);

            // if progress is 0.0, we just want to reveal it
            if (progress < 0.000001)
            {
                Logger.d("Progress 0.00 interpreted as request to reveal.");
                mClient.RevealAchievement(achievementID, callback);
                return;
            }

            // figure out if it's a standard or incremental achievement
            bool isIncremental = false;
            int curSteps = 0, totalSteps = 0;
            Achievement ach = mClient.GetAchievement(achievementID);
            if (ach == null)
            {
                Logger.w("Unable to locate achievement " + achievementID);
                Logger.w("As a quick fix, assuming it's standard.");
                isIncremental = false;
            }
            else
            {
                isIncremental = ach.IsIncremental;
                curSteps = ach.CurrentSteps;
                totalSteps = ach.TotalSteps;
                Logger.d("Achievement is " + (isIncremental ? "INCREMENTAL" : "STANDARD"));
                if (isIncremental)
                {
                    Logger.d("Current steps: " + curSteps + "/" + totalSteps);
                }
            }

            // do the right thing depending on the achievement type
            if (isIncremental)
            {
                // increment it to the target percentage (approximate)
                Logger.d("Progress " + progress +
                    " interpreted as incremental target (approximate).");
                if (progress >= 0.0 && progress <= 1.0)
                {
                    // in a previous version, incremental progress was reported by using the range [0-1]
                    Logger.w("Progress " + progress + " is less than or equal to 1. You might be trying to use values in the range of [0,1], while values are expected to be within the range [0,100]. If you are using the latter, you can safely ignore this message.");
                }

                int targetSteps = (int)((progress / 100) * totalSteps);
                int numSteps = targetSteps - curSteps;
                Logger.d("Target steps: " + targetSteps + ", cur steps:" + curSteps);
                Logger.d("Steps to increment: " + numSteps);
                if (numSteps > 0)
                {
                    mClient.IncrementAchievement(achievementID, numSteps, callback);
                }
            }
            else if (progress >= 100)
            {
                // unlock it!
                Logger.d("Progress " + progress + " interpreted as UNLOCK.");
                mClient.UnlockAchievement(achievementID, callback);
            }
            else
            {
                // not enough to unlock
                Logger.d("Progress " + progress + " not enough to unlock non-incremental achievement.");
            }
        }

        /// <summary>
        /// Increments an achievement. This is a Play Games extension of the ISocialPlatform API.
        /// </summary>
        /// <param name='achievementID'>
        /// The ID of the achievement to increment. This can be a raw Google Play
        /// Games achievement ID (alphanumeric string), or an alias that was previously configured
        /// by a call to <see cref="AddIdMapping" />.
        /// </param>
        /// <param name='steps'>
        /// The number of steps to increment the achievement by.
        /// </param>
        /// <param name='callback'>
        /// The callback to call to report the success or failure of the operation. The callback
        /// will be called with <c>true</c> to indicate success or <c>false</c> for failure.
        /// </param>
        public void IncrementAchievement(string achievementID, int steps, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("IncrementAchievement can only be called after authentication.");
                if (callback != null)
                {
                    callback.Invoke(false);
                }

                return;
            }

            // map ID, if it's in the dictionary
            Logger.d("IncrementAchievement: " + achievementID + ", steps " + steps);
            achievementID = MapId(achievementID);
            mClient.IncrementAchievement(achievementID, steps, callback);
        }

        /// <summary>
        /// Set an achievement to have at least the given number of steps completed.
        /// Calling this method while the achievement already has more steps than
        /// the provided value is a no-op. Once the achievement reaches the
        /// maximum number of steps, the achievement is automatically unlocked,
        /// and any further mutation operations are ignored.
        /// </summary>
        /// <param name='achievementID'>
        /// The ID of the achievement to increment. This can be a raw Google Play
        /// Games achievement ID (alphanumeric string), or an alias that was previously configured
        /// by a call to <see cref="AddIdMapping" />.
        /// </param>
        /// <param name='steps'>
        /// The number of steps to increment the achievement by.
        /// </param>
        /// <param name='callback'>
        /// The callback to call to report the success or failure of the operation. The callback
        /// will be called with <c>true</c> to indicate success or <c>false</c> for failure.
        /// </param>
        public void SetStepsAtLeast(string achievementID, int steps, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("SetStepsAtLeast can only be called after authentication.");
                if (callback != null)
                {
                    callback.Invoke(false);
                }

                return;
            }

            // map ID, if it's in the dictionary
            Logger.d("SetStepsAtLeast: " + achievementID + ", steps " + steps);
            achievementID = MapId(achievementID);
            mClient.SetStepsAtLeast(achievementID, steps, callback);
        }

        /// <summary>
        /// Loads the Achievement descriptions.
        /// <param name="callback">The callback to receive the descriptions</param>
        /// </summary>
        public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("LoadAchievementDescriptions can only be called after authentication.");
                callback.Invoke(null);
            }

            mClient.LoadAchievements(ach =>
                {
                    IAchievementDescription[] data = new IAchievementDescription[ach.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = new PlayGamesAchievement(ach[i]);
                    }

                    callback.Invoke(data);
                });
        }

        /// <summary>
        /// Loads the achievement state for the current user.
        /// <param name="callback">The callback to receive the achievements</param>
        /// </summary>
        public void LoadAchievements(Action<IAchievement[]> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("LoadAchievements can only be called after authentication.");
                callback.Invoke(null);
            }

            mClient.LoadAchievements(ach =>
                {
                    IAchievement[] data = new IAchievement[ach.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = new PlayGamesAchievement(ach[i]);
                    }

                    callback.Invoke(data);
                });
        }

        /// <summary>
        /// Creates an achievement object which may be subsequently used to report an
        /// achievement.
        /// </summary>
        /// <returns>
        /// The achievement object.
        /// </returns>
        public IAchievement CreateAchievement()
        {
            return new PlayGamesAchievement();
        }

        /// <summary>
        /// Reports a score to a leaderboard.
        /// </summary>
        /// <param name='score'>
        /// The score to report.
        /// </param>
        /// <param name='board'>
        /// The ID of the leaderboard on which the score is to be posted. This may be a raw
        /// Google Play Games leaderboard ID or an alias configured through a call to
        /// <see cref="AddIdMapping" />.
        /// </param>
        /// <param name='callback'>
        /// The callback to call to report the success or failure of the operation. The callback
        /// will be called with <c>true</c> to indicate success or <c>false</c> for failure.
        /// </param>
        public void ReportScore(long score, string board, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("ReportScore can only be called after authentication.");
                if (callback != null)
                {
                    callback.Invoke(false);
                }

                return;
            }

            Logger.d("ReportScore: score=" + score + ", board=" + board);
            string lbId = MapId(board);
            mClient.SubmitScore(lbId, score, callback);
        }

        /// <summary>
        /// Submits the score for the currently signed-in player
        /// to the leaderboard associated with a specific id
        /// and metadata (such as something the player did to earn the score).
        /// </summary>
        /// <param name="score">Score.</param>
        /// <param name="board">leaderboard id.</param>
        /// <param name="metadata">metadata about the score.</param>
        /// <param name="callback">Callback upon completion.</param>
        public void ReportScore(long score, string board, string metadata, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("ReportScore can only be called after authentication.");
                if (callback != null)
                {
                    callback.Invoke(false);
                }

                return;
            }

            Logger.d("ReportScore: score=" + score + ", board=" + board +
                " metadata=" + metadata);
            string lbId = MapId(board);
            mClient.SubmitScore(lbId, score, metadata, callback);
        }

        /// <summary>
        /// Loads the scores relative the player.  This returns the 25
        /// (which is the max results returned by the SDK per call) scores
        /// that are around the player's score on the Social, all time leaderboard.
        /// Use the overloaded methods which are specific to GPGS to modify these
        /// parameters.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard Id</param>
        /// <param name="callback">Callback.</param>
        public void LoadScores(string leaderboardId, Action<IScore[]> callback)
        {
            LoadScores(leaderboardId, LeaderboardStart.PlayerCentered,
                mClient.LeaderboardMaxResults(),
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                (scoreData) => callback(scoreData.Scores));
        }

        /// <summary>
        /// Loads the scores using the provided parameters.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard identifier.</param>
        /// <param name="start">Start either top scores, or player centered.</param>
        /// <param name="rowCount">Row count. the number of rows to return.</param>
        /// <param name="collection">Collection. social or public</param>
        /// <param name="timeSpan">Time span. daily, weekly, all-time</param>
        /// <param name="callback">Callback.</param>
        public void LoadScores(string leaderboardId, LeaderboardStart start,
            int rowCount, LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("LoadScores can only be called after authentication.");
                callback(new LeaderboardScoreData(leaderboardId,
                    ResponseStatus.NotAuthorized));
                return;
            }

            mClient.LoadScores(leaderboardId, start,
                rowCount, collection, timeSpan, callback);
        }

        public void LoadMoreScores(ScorePageToken token, int rowCount,
            Action<LeaderboardScoreData> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("LoadMoreScores can only be called after authentication.");
                callback(new LeaderboardScoreData(token.LeaderboardId,
                    ResponseStatus.NotAuthorized));
                return;
            }

            mClient.LoadMoreScores(token, rowCount, callback);
        }

        /// <summary>
        /// Returns a leaderboard object that can be configured to
        /// load scores.
        /// </summary>
        public ILeaderboard CreateLeaderboard()
        {
            return new PlayGamesLeaderboard(mDefaultLbUi);
        }

        /// <summary>
        /// Shows the standard Google Play Games achievements user interface,
        /// which allows the player to browse their achievements.
        /// </summary>
        public void ShowAchievementsUI()
        {
            ShowAchievementsUI(null);
        }

        /// <summary>
        /// Shows the standard Google Play Games achievements user interface,
        /// which allows the player to browse their achievements.
        /// </summary>
        /// <param name="callback">If non-null, the callback is invoked when
        /// the achievement UI is dismissed</param>
        public void ShowAchievementsUI(Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("ShowAchievementsUI can only be called after authentication.");
                return;
            }

            Logger.d("ShowAchievementsUI callback is "  + callback);
            mClient.ShowAchievementsUI(callback);
        }

        /// <summary>
        /// Shows the standard Google Play Games leaderboards user interface,
        /// which allows the player to browse their leaderboards. If you have
        /// configured a specific leaderboard as the default through a call to
        /// <see cref="SetDefaultLeaderboardForUi" />, the UI will show that
        /// specific leaderboard only. Otherwise, a list of all the leaderboards
        /// will be shown.
        /// </summary>
        public void ShowLeaderboardUI()
        {
            Logger.d("ShowLeaderboardUI with default ID");
            ShowLeaderboardUI(MapId(mDefaultLbUi), null);
        }

        /// <summary>
        /// Shows the standard Google Play Games leaderboard UI for the given
        /// leaderboard.
        /// </summary>
        /// <param name='lbId'>
        /// The ID of the leaderboard to display. This may be a raw
        /// Google Play Games leaderboard ID or an alias configured through a call to
        /// <see cref="AddIdMapping" />.
        /// </param>
        public void ShowLeaderboardUI(string lbId)
        {
           if (lbId != null)
            {
                lbId = MapId(lbId);
            }

            mClient.ShowLeaderboardUI(lbId, LeaderboardTimeSpan.AllTime, null);
        }

        /// <summary>
        /// Shows the leaderboard UI and calls the specified callback upon
        /// completion.
        /// </summary>
        /// <param name="lbId">leaderboard ID, can be null meaning all leaderboards.</param>
        /// <param name="callback">Callback to call.  If null, nothing is called.</param>
        public void ShowLeaderboardUI(string lbId, Action<UIStatus> callback)
        {
            ShowLeaderboardUI(lbId, LeaderboardTimeSpan.AllTime, callback);
        }
        /// <summary>
        /// Shows the leaderboard UI and calls the specified callback upon
        /// completion.
        /// </summary>
        /// <param name="lbId">leaderboard ID, can be null meaning all leaderboards.</param>
        /// <param name="span">Timespan to display scores in the leaderboard.</param>
        /// <param name="callback">Callback to call.  If null, nothing is called.</param>
        public void ShowLeaderboardUI(string lbId, LeaderboardTimeSpan span,
            Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("ShowLeaderboardUI can only be called after authentication.");
                callback(UIStatus.NotAuthorized);
                return;
            }

            Logger.d("ShowLeaderboardUI, lbId=" + lbId + " callback is " + callback);
            mClient.ShowLeaderboardUI(lbId, span, callback);
        }

        /// <summary>
        /// Sets the default leaderboard for the leaderboard UI. After calling this
        /// method, a call to <see cref="ShowLeaderboardUI" /> will show only the specified
        /// leaderboard instead of showing the list of all leaderboards.
        /// </summary>
        /// <param name='lbid'>
        /// The ID of the leaderboard to display on the default UI. This may be a raw
        /// Google Play Games leaderboard ID or an alias configured through a call to
        /// <see cref="AddIdMapping" />.
        /// </param>
        public void SetDefaultLeaderboardForUI(string lbid)
        {
            Logger.d("SetDefaultLeaderboardForUI: " + lbid);
            if (lbid != null)
            {
                lbid = MapId(lbid);
            }

            mDefaultLbUi = lbid;
        }

       /// <summary>
       /// Loads the friends that also play this game.  See loadConnectedPlayers.
       /// </summary>
       /// <param name="callback">Callback.</param>
        public void LoadFriends(ILocalUser user, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("LoadScores can only be called after authentication.");
                if (callback != null)
                {
                    callback(false);
                }
            }

            mClient.LoadFriends(callback);
        }

        /// <summary>
        /// Loads the leaderboard based on the constraints in the leaderboard
        /// object.
        /// <param name="board">The leaderboard object.  This is created by
        /// calling CreateLeaderboard(), and then initialized appropriately.</param>
        /// <param name="callback">callback, returning boolean for success</param>
        /// </summary>
        public void LoadScores(ILeaderboard board, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                Logger.e("LoadScores can only be called after authentication.");
                if (callback != null)
                {
                    callback(false);
                }
            }

            LeaderboardTimeSpan timeSpan;
            switch (board.timeScope)
            {
                case TimeScope.AllTime:
                    timeSpan = LeaderboardTimeSpan.AllTime;
                    break;
                case TimeScope.Week:
                    timeSpan = LeaderboardTimeSpan.Weekly;
                    break;
                case TimeScope.Today:
                    timeSpan = LeaderboardTimeSpan.Daily;
                    break;
                default:
                    timeSpan = LeaderboardTimeSpan.AllTime;
                    break;
            }

            ((PlayGamesLeaderboard)board).loading = true;
            Logger.d("LoadScores, board=" + board + " callback is " + callback);
            mClient.LoadScores(
                board.id,
                LeaderboardStart.PlayerCentered,
                board.range.count > 0 ? board.range.count : mClient.LeaderboardMaxResults(),
                board.userScope == UserScope.FriendsOnly ? LeaderboardCollection.Social : LeaderboardCollection.Public,
                timeSpan,
                (scoreData) => HandleLoadingScores(
                    (PlayGamesLeaderboard)board, scoreData, callback));
        }

        internal void HandleLoadingScores(PlayGamesLeaderboard board,
            LeaderboardScoreData scoreData, Action<bool> callback)
        {
            bool ok = board.SetFromData(scoreData);
            if (ok && !board.HasAllScores() && scoreData.NextPageToken != null)
            {
                int rowCount = board.range.count - board.ScoreCount;

                // need to load more scores
                mClient.LoadMoreScores(scoreData.NextPageToken, rowCount,
                    (nextScoreData) =>
                    HandleLoadingScores(board, nextScoreData, callback));
            }
            else
            {
                callback(ok);
            }
        }

        /// <summary>
        /// Check if the leaderboard is currently loading.
        /// <param name="board">The leaderboard of interest.</param>
        /// <returns>true if loading.</returns>
        /// </summary>
        public bool GetLoading(ILeaderboard board)
        {
            return board != null && board.loading;
        }

        /// <summary>
        /// Gets the local user.
        /// </summary>
        /// <returns>
        /// The local user.
        /// </returns>
        public ILocalUser localUser
        {
            get
            {
                return mLocalUser;
            }
        }

        /// <summary>
        /// Register an invitation delegate to be
        /// notified when a multiplayer invitation arrives
        /// </summary>
        public void RegisterInvitationDelegate(BasicApi.InvitationReceivedDelegate deleg)
        {
            mClient.RegisterInvitationDelegate(deleg);
        }

        private string MapId(string id)
        {
            if (id == null)
            {
                return null;
            }

            if (mIdMap.ContainsKey(id))
            {
                string result = mIdMap[id];
                Logger.d("Mapping alias " + id + " to ID " + result);
                return result;
            }

            return id;
        }

        internal IUserProfile[] GetFriends()
        {
            if (!IsAuthenticated())
            {
                Logger.d("Cannot get friends when not authenticated!");
                return new IUserProfile[0];
            }

            return mClient.GetFriends();
        }

        /// <summary>
        /// Retrieves a bearer token associated with the current account.
        /// </summary>
        /// <returns>A bearer token for authorized requests.</returns>
        public string GetToken()
        {
            return mClient.GetToken();
        }
    }
}
