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
using GooglePlayGames.BasicApi.SavedGame;
using System;
using GooglePlayGames.BasicApi;

public class GameManager {
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

    // keep track of saving or loading during callbacks.
    private bool mSaving;

    // auto save
    private string mAutoSaveName;

    private Texture2D mScreenImage;
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
        mAutoSaveName = "Autosaved";
    }

    void ReportAllProgress() {
        FlushAchievements();
        UnlockProgressBasedAchievements();
        PostToLeaderboard();
        AutoSave();
    }

    public void RestartLevel() {
        AutoSave();
        ReportAllProgress();
        Application.LoadLevel("GameplayScene");
    }

    public void FinishLevelAndGoToNext(int score, int stars) {
        mProgress.SetLevelProgress(mLevel, score, stars);
        AutoSave();
        ReportAllProgress();
        if (mLevel < GameConsts.MaxLevel) {
            mLevel++;
            Application.LoadLevel("GameplayScene");
        } else {
            Application.LoadLevel("FinaleScene");
        }
    }

    public void QuitToMenu() {
        AutoSave();
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

    public void CaptureScreenshot() {
        mScreenImage = new Texture2D(Screen.width, Screen.height);
        mScreenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        mScreenImage.Apply();
        Debug.Log ("Captured screen: " + mScreenImage);
    }

    public GameProgress Progress {
        get {
            return mProgress;
        }
    }

    public void SaveProgress() {

        mProgress.SaveToDisk();
        SaveToCloud(null);

    }

    public void AutoSave() {
        if(mProgress.Dirty) {
            mProgress.SaveToDisk();
            SaveToCloud (mAutoSaveName);
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


    public void Authenticate() {
        if (Authenticated || mAuthenticating) {
            Debug.LogWarning("Ignoring repeated call to Authenticate().");
            return;
        }

        // Enable/disable logs on the PlayGamesPlatform
        PlayGamesPlatform.DebugLogEnabled = GameConsts.PlayGamesDebugLogsEnabled;

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .EnableSavedGames()
                .Build();
        PlayGamesPlatform.InitializeInstance(config);

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
                Debug.Log("Login successful!");
            } else {
                // no need to show error message (error messages are shown automatically
                // by plugin)
                Debug.LogWarning("Failed to sign in with Google Play Games.");
            }
        });
    }

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

    public void LoadFromCloud() {
        // Cloud save is not in ISocialPlatform, it's a Play Games extension,
        // so we have to break the abstraction and use PlayGamesPlatform:
        Debug.Log("Loading game progress from the cloud.");
        mSaving = false;
        ((PlayGamesPlatform)Social.Active).SavedGame.ShowSelectSavedGameUI("Select saved game to load",
                                                                           4,false,false,SavedGameSelected);
    }

    void SaveToCloud(string filename) {
        if (Authenticated) {
            // Cloud save is not in ISocialPlatform, it's a Play Games extension,
            // so we have to break the abstraction and use PlayGamesPlatform:
            Debug.Log("Saving progress to the cloud...");
            mSaving = true;
            if(filename == null) {
                ((PlayGamesPlatform)Social.Active).SavedGame.ShowSelectSavedGameUI("Save game progress",
                                                                               4,true,true,SavedGameSelected);
            } else {
                // save to named file
                ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution(filename,
                                                                                         DataSource.ReadCacheOrNetwork,
                                                                                         ConflictResolutionStrategy.UseLongestPlaytime,
                                                                                         SavedGameOpened);
            }
        }
    }

    public void SavedGameSelected(SelectUIStatus status, ISavedGameMetadata game) {

        if (status == SelectUIStatus.SavedGameSelected) {
            string filename = game.Filename;
            Debug.Log("opening saved game:  " + game);
            if(mSaving && (filename == null || filename.Length == 0)) {
                filename = "save" + DateTime.Now.ToBinary();
            }
            //open the data.
            ((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution(filename,
                                                                                             DataSource.ReadCacheOrNetwork,
                                                                                             ConflictResolutionStrategy.UseLongestPlaytime,
                                                                                             SavedGameOpened);
        } else {
            Debug.LogWarning("Error selecting save game: " + status);
        }

    }

    public void SavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if(status == SavedGameRequestStatus.Success) {
            if( mSaving) {
                if(mScreenImage == null) {
                    CaptureScreenshot();
                }
                byte[] pngData = (mScreenImage!=null) ?mScreenImage.EncodeToPNG():null;
                Debug.Log("Saving to " + game);
                byte[] data = mProgress.ToBytes();
                TimeSpan playedTime = mProgress.TotalPlayingTime;
                SavedGameMetadataUpdate.Builder builder =  new 
                    SavedGameMetadataUpdate.Builder()
                        .WithUpdatedPlayedTime(playedTime)
                        .WithUpdatedDescription("Saved Game at " + DateTime.Now);

                if (pngData != null) {
                    Debug.Log("Save image of len " + pngData.Length);
                    builder = builder.WithUpdatedPngCoverImage(pngData);
                }
                else {
                    Debug.Log ("No image avail");
                }
                SavedGameMetadataUpdate updatedMetadata  = builder.Build();
                ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(game,updatedMetadata,data,SavedGameWritten);
            } else {
                mAutoSaveName = game.Filename;
                ((PlayGamesPlatform)Social.Active).SavedGame.ReadBinaryData(game,SavedGameLoaded);
            }
        } else {
            Debug.LogWarning("Error opening game: " + status);
        }
    }

    public void SavedGameLoaded(SavedGameRequestStatus status, byte[] data) {
        if (status == SavedGameRequestStatus.Success) {
            Debug.Log("SaveGameLoaded, success=" + status);
            ProcessCloudData(data);
        } else {
            Debug.LogWarning("Error reading game: " + status);
        }
    }

    public void SavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if(status == SavedGameRequestStatus.Success) {
            Debug.Log ("Game " + game.Description + " written");
        } else {
            Debug.LogWarning("Error saving game: " + status);
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
