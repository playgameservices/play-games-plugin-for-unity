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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;

public class GameManager : GooglePlayGames.BasicApi.OnStateLoadedListener {
    private static GameManager sInstance = new GameManager();
    private int mLevel = 0;

    private GameProgress mProgress;

    private bool mAuthenticating = false;
    private string mAuthProgressMessage = Strings.SigningIn;

    // list of achievements we know we have unlocked (to avoid making repeated calls to the API)
    private Dictionary<string,bool> mUnlockedAchievements = new Dictionary<string, bool>();

    // achievement increments we are accumulating locally, waiting to send to the games API
    private Dictionary<string,int> mPendingIncrements = new Dictionary<string, int>();

    // what is the highest score we have posted to the leaderboard?
    private int mHighestPostedScore = 0;

    // cloud save callbacks
    private GooglePlayGames.BasicApi.OnStateLoadedListener mAppStateListener;

    public static GameManager Instance {
        get {
            return sInstance;
        }
    }

    public int Level {
        get {
            return mLevel;
        }
    }

    private GameManager() {
        mProgress = GameProgress.LoadFromDisk();
    }

    void ReportAllProgress() {
        FlushAchievements();
        UnlockProgressBasedAchievements();
        PostToLeaderboard();
        SaveToCloud();
    }

    public void RestartLevel() {
        SaveProgress();
        ReportAllProgress();
        Application.LoadLevel("GameplayScene");
    }

    public void FinishLevelAndGoToNext(int score, int stars) {
        mProgress.SetLevelProgress(mLevel, score, stars);
        SaveProgress();
        ReportAllProgress();
        if (mLevel < GameConsts.MaxLevel) {
            mLevel++;
            Application.LoadLevel("GameplayScene");
        } else {
            Application.LoadLevel("FinaleScene");
        }
    }

    public void QuitToMenu() {
        SaveProgress();
        ReportAllProgress();
        Application.LoadLevel("MenuScene");
    }

    public void GoToLevel(int level) {
        ReportAllProgress();
        mLevel = level;
        Application.LoadLevel("GameplayScene");
    }

    public bool HasNextLevel() {
        return mLevel < GameConsts.MaxLevel;
    }

    public GameProgress Progress {
        get {
            return mProgress;
        }
    }

    public void SaveProgress() {
        if (mProgress.Dirty) {
            mProgress.SaveToDisk();
            SaveToCloud();
        }
    }

    private void UnlockProgressBasedAchievements() {
        int totalStars = mProgress.TotalStars;
        int i;
        for (i = 0; i < GameIds.Achievements.ForTotalStars.Length; i++) {
            int starsRequired = GameIds.Achievements.TotalStarsRequired[i];
            if (totalStars >= starsRequired) {
                UnlockAchievement(GameIds.Achievements.ForTotalStars[i]);
            }
        }

        if (mProgress.AreAllLevelsCleared()) {
            UnlockAchievement(GameIds.Achievements.ClearAllLevels);
        }
    }

    public void UnlockAchievement(string achId) {
        if (Authenticated && !mUnlockedAchievements.ContainsKey(achId)) {
            Social.ReportProgress(achId, 100.0f, (bool success) => {});
            mUnlockedAchievements[achId] = true;
        }
    }

    public void IncrementAchievement(string achId, int steps) {
        if (mPendingIncrements.ContainsKey(achId)) {
            steps += mPendingIncrements[achId];
        }
        mPendingIncrements[achId] = steps;
    }

    public void FlushAchievements() {
        if (Authenticated) {
            foreach (string ach in mPendingIncrements.Keys) {
                // incrementing achievements by a delta is a feature
                // that's specific to the Play Games API and not part of the
                // ISocialPlatform spec, so we have to break the abstraction and
                // use the PlayGamesPlatform rather than ISocialPlatform
                PlayGamesPlatform p = (PlayGamesPlatform) Social.Active;
                p.IncrementAchievement(ach, mPendingIncrements[ach], (bool success) => {});
            }
            mPendingIncrements.Clear();
        }
    }

    #if UNITY_ANDROID || UNITY_IOS
    public void Authenticate() {
        if (Authenticated || mAuthenticating) {
            Debug.LogWarning("Ignoring repeated call to Authenticate().");
            return;
        }

        // Enable/disable logs on the PlayGamesPlatform
        PlayGamesPlatform.DebugLogEnabled = GameConsts.PlayGamesDebugLogsEnabled;

        // Activate the Play Games platform. This will make it the default
        // implementation of Social.Active
        PlayGamesPlatform.Activate();

        // Set the default leaderboard for the leaderboards UI
        ((PlayGamesPlatform) Social.Active).SetDefaultLeaderboardForUI(GameIds.LeaderboardId);

        // Sign in to Google Play Games
        mAuthenticating = true;
        Social.localUser.Authenticate((bool success) => {
            mAuthenticating = false;
            if (success) {
                // if we signed in successfully, load data from cloud
                LoadFromCloud();
            } else {
                // no need to show error message (error messages are shown automatically
                // by plugin)
                Debug.LogWarning("Failed to sign in with Google Play Games.");
            }
        });
    }
    #else
    public void Authenticate() {
        mAuthenticating = false;
    }
    #endif

    void ProcessCloudData(byte[] cloudData) {
        if (cloudData == null) {
            Debug.Log("No data saved to the cloud yet...");
            return;
        }
        Debug.Log("Decoding cloud data from bytes.");
        GameProgress progress = GameProgress.FromBytes(cloudData);
        Debug.Log("Merging with existing game progress.");
        mProgress.MergeWith(progress);
    }

    void LoadFromCloud() {
        // Cloud save is not in ISocialPlatform, it's a Play Games extension,
        // so we have to break the abstraction and use PlayGamesPlatform:
        #if UNITY_ANDROID
        Debug.Log("Loading game progress from the cloud.");
        ((PlayGamesPlatform) Social.Active).LoadState(0, this);
        #else
        // app state is not supported on ios, log the error and carry on
        Debug.LogError("LoadState is not supported on iOS");
        #endif
    }

    void SaveToCloud() {
        if (Authenticated) {
            // Cloud save is not in ISocialPlatform, it's a Play Games extension,
            // so we have to break the abstraction and use PlayGamesPlatform:
            #if UNITY_ANDROID
            Debug.Log("Saving progress to the cloud...");
            ((PlayGamesPlatform) Social.Active).UpdateState(0, mProgress.ToBytes(), this);
            #else
             // app state is not supported on ios, log the error and carry on
             Debug.LogError("SaveState is not supported on iOS");
            #endif
        }
    }

    // Data was successfully loaded from the cloud
    public void OnStateLoaded(bool success, int slot, byte[] data) {
        Debug.Log("Cloud load callback, success=" + success);
        if (success) {
            ProcessCloudData(data);
        } else {
            Debug.LogWarning("Failed to load from cloud. Network problems?");
        }

        // regardless of success, this is the end of the auth process
        mAuthenticating = false;

        // report any progress we have to report
        ReportAllProgress();
    }

    // Conflict in cloud data occurred
    public byte[] OnStateConflict(int slot, byte[] local, byte[] server) {
        Debug.Log("Conflict callback called. Resolving conflict.");

        // decode byte arrays into game progress and merge them
        GameProgress localProgress = local == null ?
            new GameProgress() : GameProgress.FromBytes(local);
        GameProgress serverProgress = server == null ?
            new GameProgress() : GameProgress.FromBytes(server);
        localProgress.MergeWith(serverProgress);

        // resolve conflict
        return localProgress.ToBytes();
    }

    public void OnStateSaved(bool success, int slot) {
        if (!success) {
            Debug.LogWarning("Failed to save state to the cloud.");

            // try to save later:
            mProgress.Dirty = true;
        }
    }

    public bool Authenticating {
        get {
            return mAuthenticating;
        }
    }

    public bool Authenticated {
        get {
            return Social.Active.localUser.authenticated;
        }
    }

    public void SignOut() {
        ((PlayGamesPlatform) Social.Active).SignOut();
    }

    public string AuthProgressMessage {
        get {
            return mAuthProgressMessage;
        }
    }

    public void ShowLeaderboardUI() {
        if (Authenticated) {
            Social.ShowLeaderboardUI();
        }
    }

    public void ShowAchievementsUI() {
        if (Authenticated) {
            Social.ShowAchievementsUI();
        }
    }

    public void PostToLeaderboard() {
        int score = mProgress.TotalScore;
        if (Authenticated && score > mHighestPostedScore) {
            // post score to the leaderboard
            Social.ReportScore(score, GameIds.LeaderboardId, (bool success) => {});
            mHighestPostedScore = score;
        } else {
            Debug.LogWarning ("Not reporting score, auth = " + Authenticated + " " +
                               score + " <= " + mHighestPostedScore);
        }
    }
}
