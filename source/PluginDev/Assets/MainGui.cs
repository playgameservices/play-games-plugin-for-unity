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

using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using System;
using GooglePlayGames;

public class MainGui : MonoBehaviour, GooglePlayGames.BasicApi.OnStateLoadedListener {
    const int Margin = 20, Spacing = 10;
    const float FontSizeFactor = 50;
    const int GridCols = 2;
    const int GridRows = 7;

    public GUISkin GuiSkin;

    bool mStandby = false;
    string mStandbyMessage = "";
    string mStatus = "Ready.";

    bool mHadCloudConflict = false;

    Rect CalcGrid(int col, int row) {
        return CalcGrid(col, row, 1, 1);
    }

    Rect CalcGrid(int col, int row, int colcount, int rowcount) {
        int cellW = (Screen.width - 2 * Margin - (GridCols - 1) * Spacing) / GridCols;
        int cellH = (Screen.height - 2 * Margin - (GridRows - 1) * Spacing) / GridRows;
        return new Rect(Margin + col * (cellW + Spacing),
            Margin + row * (cellH + Spacing),
            cellW + (colcount - 1) * (Spacing + cellW),
            cellH + (rowcount - 1) * (Spacing + cellH));
    }

    void ShowStandbyUi() {
        GUI.Label(CalcGrid(0, 2, 2, 1), mStandbyMessage);
    }

    void DrawTitle() {
        GUI.Label(CalcGrid(0, 0, 2, 1), "Play Games Unity Plugin - Smoke Test");
    }

    void DrawStatus() {
        GUI.Label(CalcGrid(0, 6, 2, 1), mStatus);
    }

    void ShowNotAuthUi() {
        DrawTitle();
        DrawStatus();
        if (GUI.Button(CalcGrid(1,1), "Authenticate")) {
            DoAuthenticate();
        }
    }

    void ShowRegularUi() {
        DrawTitle();
        DrawStatus();

        if (GUI.Button(CalcGrid(0,1), "Ach Reveal")) {
            DoAchievementReveal();
        } else if (GUI.Button(CalcGrid(0,2), "Ach Unlock")) {
            DoAchievementUnlock();
        } else if (GUI.Button(CalcGrid(0,3), "Ach Increment")) {
            DoAchievementIncrement();
        } else if (GUI.Button(CalcGrid(0,4), "Ach Show UI")) {
            DoAchievementUI();
        }

        if (GUI.Button(CalcGrid(1,1), "Post Score")) {
            DoPostScore();
        } else if (GUI.Button(CalcGrid(1,2), "LB Show UI")) {
            DoLeaderboardUI();
        } else if (GUI.Button(CalcGrid(1,3), "Cloud Save")) {
            DoCloudSave();
        } else if (GUI.Button(CalcGrid(1,4), "Cloud Load")) {
            DoCloudLoad();
        }

        if (GUI.Button(CalcGrid(0,5), "Reload Scene")) {
            Application.LoadLevel(Application.loadedLevel);
        }
        
        if (GUI.Button(CalcGrid(1,5), "Sign Out")) {
            DoSignOut();
        }
    }

    void ShowEffect(bool success) {
        Camera.main.backgroundColor = success ?
            new Color(0.0f, 0.0f, 0.8f, 1.0f) :
            new Color(0.8f, 0.0f, 0.0f, 1.0f);
    }


    int CalcFontSize() {
        return (int)(Screen.width * FontSizeFactor / 1000.0f);
    }

    // Update is called once per frame
    void OnGUI() {
        GUI.skin = GuiSkin;
        GUI.skin.label.fontSize = CalcFontSize();
        GUI.skin.button.fontSize = CalcFontSize();

        if (mStandby) {
            ShowStandbyUi();
        } else if (Social.localUser.authenticated) {
            ShowRegularUi();
        } else {
            ShowNotAuthUi();
        }
    }

    void SetStandBy(string message) {
        mStandby = true;
        mStandbyMessage = message;
    }

    void EndStandBy() {
        mStandby = false;
    }

    void DoAuthenticate() {
        SetStandBy("Authenticating...");

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        Social.localUser.Authenticate((bool success) => {
            EndStandBy();
            if (success) {
                mStatus = "Authenticated. Hello, " + Social.localUser.userName + " (" +
                    Social.localUser.id + ")";
            } else {
                mStatus = "*** Failed to authenticate.";
            }
            ShowEffect(success);
        });
    }
    
    void DoSignOut() {
        ((PlayGamesPlatform) Social.Active).SignOut();
        mStatus = "Signing out.";
    }

    void DoAchievementReveal() {
        SetStandBy("Revealing achievement...");
        Social.ReportProgress(Settings.AchievementToReveal, 0.0f, (bool success) => {
            EndStandBy();
            mStatus = success ? "Revealed successfully." : "*** Failed to reveal ach.";
            ShowEffect(success);
        });
    }

    void DoAchievementUnlock() {
        SetStandBy("Unlocking achievement...");
        Social.ReportProgress(Settings.AchievementToUnlock, 100.0f, (bool success) => {
            EndStandBy();
            mStatus = success ? "Unlocked successfully." : "*** Failed to unlock ach.";
            ShowEffect(success);
        });
    }

    void DoAchievementIncrement() {
        PlayGamesPlatform p = (PlayGamesPlatform) Social.Active;

        SetStandBy("Incrementing achievement...");
        p.IncrementAchievement(Settings.AchievementToIncrement, 1,(bool success) => {
            EndStandBy();
            mStatus = success ? "Incremented successfully." : "*** Failed to increment ach.";
            ShowEffect(success);
        });

    }

    long GenScore() {
        return (long)(DateTime.Today.Subtract(new DateTime(2013, 1, 1, 0, 0, 0)).TotalSeconds);
    }

    void DoPostScore() {
        long score = GenScore();
        SetStandBy("Posting score: " + score);
        Social.ReportScore(score, Settings.Leaderboard, (bool success) => {
            EndStandBy();
            mStatus = success ? "Successfully reported score " + score :
                "*** Failed to report score " + score;
            ShowEffect(success);
        });
    }

    void DoLeaderboardUI() {
        Social.ShowLeaderboardUI();
        ShowEffect(true);
    }

    void DoAchievementUI() {
        Social.ShowAchievementsUI();
        ShowEffect(true);
    }

    char RandCharFrom(string s) {
        int i = UnityEngine.Random.Range(0, s.Length);
        i = i < 0 ? 0 : i >= s.Length ? s.Length - 1 : i;
        return s[i];
    }

    string GenString() {
        string x = "";
        int syl = UnityEngine.Random.Range(4, 7);
        while (x.Length < syl) {
            x += RandCharFrom("bcdfghjklmnpqrstvwxyz");
            x += RandCharFrom("aeiou");
            if (UnityEngine.Random.Range(0,10) > 7) {
                x += RandCharFrom("nsr");
            }
        }
        return x;
    }

    void DoCloudSave() {
        string word = GenString();

        SetStandBy("Saving string to cloud: " + word);
        PlayGamesPlatform p = (PlayGamesPlatform) Social.Active;
        p.UpdateState(0, System.Text.ASCIIEncoding.Default.GetBytes(word), this);
        EndStandBy();
        mStatus = "Saved string to cloud: " + word;
        ShowEffect(true);
    }


    public void OnStateLoaded(bool success, int slot, byte[] data) {
        EndStandBy();
        if (success) {
            mStatus = "Loaded from cloud: " + System.Text.ASCIIEncoding.Default.GetString(data);
        } else {
            mStatus = "*** Failed to load from cloud.";
        }

        mStatus += ". conflict=" + (mHadCloudConflict ? "yes" : "no");
        ShowEffect(success);
    }

    public byte[] OnStateConflict(int slot, byte[] local, byte[] server) {
        mHadCloudConflict = true;
        return local;
    }

    public void OnStateSaved(bool success, int slot) {
        mStatus = "Cloud save " + (success ? "successful" : "failed");
        ShowEffect(success);
    }

    void DoCloudLoad() {
        mHadCloudConflict = false;
        SetStandBy("Loading from cloud...");
        ((PlayGamesPlatform) Social.Active).LoadState(0, this);
    }
}
