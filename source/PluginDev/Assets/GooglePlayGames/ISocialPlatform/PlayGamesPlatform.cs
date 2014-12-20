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

using System;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.BasicApi.SavedGame;

namespace GooglePlayGames {
/// <summary>
/// Provides access to the Google Play Games platform. This is an implementation of
/// UnityEngine.SocialPlatforms.ISocialPlatform. Activate this platform by calling
/// the <see cref="Activate" /> method, then authenticate by calling
/// the <see cref="Authenticate" /> method. After authentication
/// completes, you may call the other methods of this class. This is not a complete
/// implementation of the ISocialPlatform interface. Methods lacking an implementation
/// or whose behavior is at variance with the standard are noted as such.
/// </summary>
public class PlayGamesPlatform : ISocialPlatform {
    private static volatile PlayGamesPlatform sInstance = null;

    private readonly PlayGamesClientConfiguration mConfiguration;
    private PlayGamesLocalUser mLocalUser = null;
    private IPlayGamesClient mClient = null;

    // the default leaderboard we show on ShowLeaderboardUI
    private string mDefaultLbUi = null;

    // achievement/leaderboard ID mapping table
    private Dictionary<string, string> mIdMap = new Dictionary<string, string>();

    private PlayGamesPlatform(PlayGamesClientConfiguration configuration) {
        this.mLocalUser = new PlayGamesLocalUser(this);
        this.mConfiguration = configuration;
    }

    internal PlayGamesPlatform(IPlayGamesClient client) {
        this.mClient = Misc.CheckNotNull(client);
        this.mLocalUser = new PlayGamesLocalUser(this);
        this.mConfiguration = PlayGamesClientConfiguration.DefaultConfiguration;
    }

    public static void InitializeInstance(PlayGamesClientConfiguration configuration) {
        if (sInstance != null) {
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
    public static PlayGamesPlatform Instance {
        get {
            if (sInstance == null) {
                Logger.d("Instance was not initialized, using default configuration.");
                InitializeInstance(PlayGamesClientConfiguration.DefaultConfiguration);
            }
            return sInstance;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether debug logs are enabled. This property
    /// may be set before calling <see cref="Activate" /> method.
    /// </summary>
    /// <returns>
    /// <c>true</c> if debug log enabled; otherwise, <c>false</c>.
    /// </returns>
    public static bool DebugLogEnabled {
        get {
            return Logger.DebugLogEnabled;
        }
        set {
            Logger.DebugLogEnabled = value;
        }
    }

    /// Gets the real time multiplayer API object
    public IRealTimeMultiplayerClient RealTime {
        get {
            return mClient.GetRtmpClient();
        }
    }

    /// Gets the turn based multiplayer API object
    public ITurnBasedMultiplayerClient TurnBased {
        get {
            return mClient.GetTbmpClient();
        }
    }

    public ISavedGameClient SavedGame {
        get {
            return mClient.GetSavedGameClient();
        }
    }

    /// <summary>
    /// Activates the Play Games platform as the implementation of Social.Active.
    /// After calling this method, you can call methods on Social.Active. For
    /// example, <c>Social.Active.Authenticate()</c>.
    /// </summary>
    /// <returns>The singleton <see cref="PlayGamesPlatform" /> instance.</returns>
    public static PlayGamesPlatform Activate() {
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
    public void AddIdMapping(string fromId, string toId) {
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
    public void Authenticate(Action<bool> callback) {
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
    public void Authenticate(Action<bool> callback, bool silent) {
        // make a platform-specific Play Games client
        if (mClient == null) {
            Logger.d("Creating platform-specific Play Games client.");
            mClient = PlayGamesClientFactory.GetPlatformPlayGamesClient(mConfiguration);
        }

        // authenticate!
        mClient.Authenticate(callback, silent);
    }

    /// <summary>
    /// Same as <see cref="Authenticate(Action<bool>,bool)"/>. Provided for compatibility
    /// with ISocialPlatform.
    /// </summary>
    /// <param name="unused">Unused.</param>
    /// <param name="callback">Callback.</param>
    public void Authenticate(ILocalUser unused, Action<bool> callback) {
        Authenticate(callback, false);
    }

    /// <summary>
    /// Determines whether the user is authenticated.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
    /// </returns>
    public bool IsAuthenticated() {
        return mClient != null && mClient.IsAuthenticated();
    }

    /// Sign out. After signing out, Authenticate must be called again to sign back in.
    public void SignOut() {
        if (mClient != null) {
            mClient.SignOut();
        }
    }

    /// <summary>
    /// Not implemented yet. Calls the callback with an empty list.
    /// </summary>
    public void LoadUsers(string[] userIDs, Action<IUserProfile[]> callback) {
        Logger.w("PlayGamesPlatform.LoadUsers is not implemented.");
        if (callback != null) {
            callback.Invoke(new IUserProfile[0]);
        }
    }

    /// <summary>
    /// Returns the user's Google ID.
    /// </summary>
    /// <returns>
    /// The user's Google ID. No guarantees are made as to the meaning or format of
    /// this identifier except that it is unique to the user who is signed in.
    /// </returns>
    public string GetUserId() {
        if (!IsAuthenticated()) {
            Logger.e("GetUserId() can only be called after authentication.");
            return "0";
        }
        return mClient.GetUserId();
    }

    /// <summary>
    /// Returns the user's display name.
    /// </summary>
    /// <returns>
    /// The user display name (e.g. "Bruno Oliveira")
    /// </returns>
    public string GetUserDisplayName() {
        if (!IsAuthenticated()) {
            Logger.e("GetUserDisplayName can only be called after authentication.");
            return "";
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
    public string GetUserImageUrl() {
        if (!IsAuthenticated()) {
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
    public void ReportProgress(string achievementID, double progress, Action<bool> callback) {
        if (!IsAuthenticated()) {
            Logger.e("ReportProgress can only be called after authentication.");
            if (callback != null) {
                callback.Invoke(false);
            }
            return;
        }

        // map ID, if it's in the dictionary
        Logger.d("ReportProgress, " + achievementID + ", " + progress);
        achievementID = MapId(achievementID);

        // if progress is 0.0, we just want to reveal it
        if (progress < 0.000001) {
            Logger.d("Progress 0.00 interpreted as request to reveal.");
            mClient.RevealAchievement(achievementID, callback);
            return;
        }

        // figure out if it's a standard or incremental achievement
        bool isIncremental = false;
        int curSteps = 0, totalSteps = 0;
        Achievement ach = mClient.GetAchievement(achievementID);
        if (ach == null) {
            Logger.w("Unable to locate achievement " + achievementID);
            Logger.w("As a quick fix, assuming it's standard.");
            isIncremental = false;
        } else {
            isIncremental = ach.IsIncremental;
            curSteps = ach.CurrentSteps;
            totalSteps = ach.TotalSteps;
            Logger.d("Achievement is " + (isIncremental ? "INCREMENTAL" : "STANDARD"));
            if (isIncremental) {
                Logger.d("Current steps: " + curSteps + "/" + totalSteps);
            }
        }

        // do the right thing depending on the achievement type
        if (isIncremental) {
            // increment it to the target percentage (approximate)
            Logger.d("Progress " + progress +
            " interpreted as incremental target (approximate).");
            int targetSteps = (int)(progress * totalSteps);
            int numSteps = targetSteps - curSteps;
            Logger.d("Target steps: " + targetSteps + ", cur steps:" + curSteps);
            Logger.d("Steps to increment: " + numSteps);
            if (numSteps > 0) {
                mClient.IncrementAchievement(achievementID, numSteps, callback);
            }
        } else {
            // unlock it!
            Logger.d("Progress " + progress + " interpreted as UNLOCK.");
            mClient.UnlockAchievement(achievementID, callback);
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
    public void IncrementAchievement(string achievementID, int steps, Action<bool> callback) {
        if (!IsAuthenticated()) {
            Logger.e("IncrementAchievement can only be called after authentication.");
            if (callback != null) {
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
    /// Not implemented yet. Calls the callback with an empty list.
    /// </summary>
    public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback) {
        Logger.w("PlayGamesPlatform.LoadAchievementDescriptions is not implemented.");
        if (callback != null) {
            callback.Invoke(new IAchievementDescription[0]);
        }
    }

    /// <summary>
    /// Not implemented yet. Calls the callback with an empty list.
    /// </summary>
    public void LoadAchievements(Action<IAchievement[]> callback) {
        Logger.w("PlayGamesPlatform.LoadAchievements is not implemented.");
        if (callback != null) {
            callback.Invoke(new IAchievement[0]);
        }
    }

    /// <summary>
    /// Creates an achievement object which may be subsequently used to report an
    /// achievement.
    /// </summary>
    /// <returns>
    /// The achievement object.
    /// </returns>
    public IAchievement CreateAchievement() {
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
    public void ReportScore(long score, string board, Action<bool> callback) {
        if (!IsAuthenticated()) {
            Logger.e("ReportScore can only be called after authentication.");
            if (callback != null) {
                callback.Invoke(false);
            }
            return;
        }

        Logger.d("ReportScore: score=" + score + ", board=" + board);
        string lbId = MapId(board);
        mClient.SubmitScore(lbId, score, callback);
    }

    /// <summary>
    /// Not implemented yet. Calls the callback with an empty list.
    /// </summary>
    public void LoadScores(string leaderboardID, Action<IScore[]> callback) {
        Logger.w("PlayGamesPlatform.LoadScores not implemented.");
        if (callback != null) {
            callback.Invoke(new IScore[0]);
        }
    }

    /// <summary>
    /// Not implemented yet. Returns null;
    /// </summary>
    public ILeaderboard CreateLeaderboard() {
        Logger.w("PlayGamesPlatform.CreateLeaderboard not implemented. Returning null.");
        return null;
    }

    /// <summary>
    /// Shows the standard Google Play Games achievements user interface,
    /// which allows the player to browse their achievements.
    /// </summary>
    public void ShowAchievementsUI() {
        if (!IsAuthenticated()) {
            Logger.e("ShowAchievementsUI can only be called after authentication.");
            return;
        }

        Logger.d("ShowAchievementsUI");
        mClient.ShowAchievementsUI();
    }

    /// <summary>
    /// Shows the standard Google Play Games leaderboards user interface,
    /// which allows the player to browse their leaderboards. If you have
    /// configured a specific leaderboard as the default through a call to
    /// <see cref="SetDefaultLeaderboardForUi" />, the UI will show that
    /// specific leaderboard only. Otherwise, a list of all the leaderboards
    /// will be shown.
    /// </summary>
    public void ShowLeaderboardUI() {
        if (!IsAuthenticated()) {
            Logger.e("ShowLeaderboardUI can only be called after authentication.");
            return;
        }
        Logger.d("ShowLeaderboardUI");
        mClient.ShowLeaderboardUI(MapId(mDefaultLbUi));
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
    public void ShowLeaderboardUI(string lbId) {
        if (!IsAuthenticated()) {
            Logger.e("ShowLeaderboardUI can only be called after authentication.");
            return;
        }
        Logger.d("ShowLeaderboardUI, lbId=" + lbId);
        if (lbId != null) {
            lbId = MapId(lbId);
        }
        mClient.ShowLeaderboardUI(lbId);
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
    public void SetDefaultLeaderboardForUI(string lbid) {
        Logger.d("SetDefaultLeaderboardForUI: " + lbid);
        if (lbid != null) {
            lbid = MapId(lbid);
        }
        mDefaultLbUi = lbid;
    }

    /// <summary>
    /// Not implemented yet. Calls the callback with <c>false</c>.
    /// </summary>
    public void LoadFriends(ILocalUser user, Action<bool> callback) {
        Logger.w("PlayGamesPlatform.LoadFriends not implemented.");
        if (callback != null) {
            callback.Invoke(false);
        }
    }

    /// <summary>
    /// Not implemented yet. Calls the callback with <c>false</c>.
    /// </summary>
    public void LoadScores(ILeaderboard board, Action<bool> callback) {
        Logger.w("PlayGamesPlatform.LoadScores not implemented.");
        if (callback != null) {
            callback.Invoke(false);
        }
    }

    /// <summary>
    /// Not implemented yet. Returns false.
    /// </summary>
    public bool GetLoading(ILeaderboard board) {
        return false;
    }

    /// <summary>
    /// Loads app state (cloud save) data from the server.
    /// </summary>
    /// <param name='slot'>
    /// The app state slot number. The exact number of slots and their size can be seen
    /// in the Google Play Games documentation. Slot 0 is always available, and is at
    /// least 128K long.
    /// </param>
    /// <param name='callbacks'>
    /// The callbacks to call when the state is loaded, or when a conflict occurs.
    /// </param>
    public void LoadState(int slot, OnStateLoadedListener listener) {
        if (!IsAuthenticated()) {
            Logger.e("LoadState can only be called after authentication.");
            if (listener != null) {
                listener.OnStateLoaded(false, slot, null);
            }
            return;
        }
        mClient.LoadState(slot, listener);
    }

    /// <summary>
    /// Writes app state (cloud save) data to the server.
    /// </summary>
    /// <param name='slot'>
    /// The app state slot number. The exact number of slots and their size can be seen
    /// in the Google Play Games documentation. Slot 0 is always available, and is at
    /// least 128K long.
    /// </param>
    /// <param name='data'>
    /// The data to write.
    /// </param>
    public void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
        if (!IsAuthenticated()) {
            Logger.e("UpdateState can only be called after authentication.");
            if (listener != null) {
                listener.OnStateSaved(false, slot);
            }
            return;
        }
        mClient.UpdateState(slot, data, listener);
    }

    /// <summary>
    /// Gets the local user.
    /// </summary>
    /// <returns>
    /// The local user.
    /// </returns>
    public ILocalUser localUser {
        get {
            return mLocalUser;
        }
    }

    /// Register an invitation delegate to be notified when a multiplayer invitation arrives
    public void RegisterInvitationDelegate(BasicApi.InvitationReceivedDelegate deleg) {
        mClient.RegisterInvitationDelegate(deleg);
    }

    private string MapId(string id) {
        if (id == null) {
            return null;
        }
        if (mIdMap.ContainsKey(id)) {
            string result = mIdMap[id];
            Logger.d("Mapping alias " + id + " to ID " + result);
            return result;
        }
        return id;
    }
}
}

