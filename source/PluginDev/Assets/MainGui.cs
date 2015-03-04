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
using UnityEngine.SocialPlatforms;
using System;
using System.Linq;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.OurUtils;
using GooglePlayGames.BasicApi;

public class MainGui : MonoBehaviour, GooglePlayGames.BasicApi.OnStateLoadedListener,
        RealTimeMultiplayerListener {
    const int Margin = 20, Spacing = 10;
    const float FontSizeFactor = 35;
    const int GridCols = 2;
    const int GridRows = 9;

    private static readonly PlayGamesClientConfiguration ClientConfiguration =
        new PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .EnableDeprecatedCloudSave()
            .Build();

    // which UI are we showing?
    enum Ui {
        Main,
        Multiplayer,
        Rtmp,
        SavedGame,
        EditSavedGameName,
        WriteSavedGame,
        ResolveSaveConflict,
        Tbmp,
        TbmpMatch
    };

    Ui mUi = Ui.Main;

    public GUISkin GuiSkin;

    bool mStandby = false;
    string mStandbyMessage = "";
    string mStatus = "Ready.";
    string mLastInvitationId = null;

    string mSavedGameFilename = "default_name";
    ISavedGameMetadata mCurrentSavedGame = null;
    string mSavedGameFileContent = "";
    IConflictResolver mConflictResolver = null;
    ISavedGameMetadata mConflictOriginal = null;
    string mConflictOriginalData = null;
    ISavedGameMetadata mConflictUnmerged = null;
    string mConflictUnmergedData = null;

    string mLastLocalSave = null;

    string mConflictLocalVersion = null;
    string mConflictServerVersion = null;
    bool mHadCloudConflict = false;

    TurnBasedMatch mMatch = null;

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

    void DrawTitle(string title) {
        GUI.Label(CalcGrid(0, 0, 2, 1), title == null ?
            "Play Games Unity Plugin - Smoke Test" : title);
    }

    void DrawStatus() {
        GUI.Label(CalcGrid(0, 7, 2, 2), mStatus);
    }

    void ShowNotAuthUi() {
        DrawTitle(null);
        DrawStatus();
        if (GUI.Button(CalcGrid(1,1), "Authenticate")) {
            DoAuthenticate();
        }
    }

    void ShowRegularUi() {
        DrawTitle(null);
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

        if (GUI.Button(CalcGrid(0,5), "Multiplayer")) {
            mUi = Ui.Multiplayer;
        }

        if (GUI.Button(CalcGrid(0, 6), "Saved Game")) {
            mUi = Ui.SavedGame;
        }

        if (GUI.Button(CalcGrid(1,5), "Sign Out")) {
            DoSignOut();
        }
    }

    void ShowMultiplayerUi() {
        DrawTitle("MULTIPLAYER");
        DrawStatus();

        if (GUI.Button(CalcGrid(0,1), "RTMP")) {
            mUi = Ui.Rtmp;
        } else if (GUI.Button(CalcGrid(1,1), "TBMP")) {
            mUi = Ui.Tbmp;
        } else if (GUI.Button(CalcGrid(1,5), "Back")) {
            mUi = Ui.Main;
        }
    }

    void ShowEditSavedGameName() {
        DrawTitle("EDIT SAVED GAME FILENAME");
        DrawStatus();

        mSavedGameFilename = GUI.TextArea(CalcGrid(0, 1), mSavedGameFilename);

        if (GUI.Button(CalcGrid(1, 7), "Back")) {
            mUi = Ui.SavedGame;
        }
    }

    void ShowResolveConflict() {
        DrawTitle("RESOLVE SAVE GAME CONFLICT");
        DrawStatus();

        if (mConflictResolver == null) {
            mStatus = "No pending conflict";
            mUi = Ui.SavedGame;
            return;
        }

        GUI.Label(CalcGrid(0, 1, 2, 2),
            "Original: " + mConflictOriginal.Filename + ":" + mConflictOriginal.Description + "\n" +
            "Data: " + mConflictOriginalData);

        GUI.Label(CalcGrid(0, 2, 2, 2),
            "Unmerged: " + mConflictUnmerged.Filename + ":" + mConflictUnmerged.Description + "\n" +
            "Data: " + mConflictUnmergedData);

        if (GUI.Button(CalcGrid(0, 3), "Use Original")) {
            mConflictResolver.ChooseMetadata(mConflictOriginal);
            SetStandBy("Choosing original, retrying open");
            mUi = Ui.SavedGame;
        } else if (GUI.Button(CalcGrid(1, 3), "Use Unmerged")) {
            mConflictResolver.ChooseMetadata(mConflictUnmerged);
            SetStandBy("Choosing unmerged, retrying open");
            mUi = Ui.SavedGame;
        }

        if (GUI.Button(CalcGrid(1, 7), "Back")) {
            mUi = Ui.SavedGame;
        }
    }

    void ShowWriteSavedGame() {
        DrawTitle("WRITE SAVED GAME");
        DrawStatus();

        mSavedGameFileContent = GUI.TextArea(CalcGrid(0, 1), mSavedGameFileContent);

        if (mCurrentSavedGame == null || !mCurrentSavedGame.IsOpen) {
            mStatus = "No opened saved game selected.";
            mUi = Ui.SavedGame;
            return;
        }

        var update = new SavedGameMetadataUpdate.Builder()
            .WithUpdatedDescription("Saved at " + DateTime.Now.ToString())
            .WithUpdatedPlayedTime(mCurrentSavedGame.TotalTimePlayed.Add(TimeSpan.FromHours(1)))
            .Build();

        if (GUI.Button(CalcGrid(0, 7), "Write")) {
            SetStandBy("Writing update");
            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(
                mCurrentSavedGame,
                update,
                System.Text.ASCIIEncoding.Default.GetBytes(mSavedGameFileContent),
                (status, updated) => {
                    mStatus = "Write status was: " + status;
                    mUi = Ui.SavedGame;
                    EndStandBy();
                });
            mCurrentSavedGame = null;
        } else if (GUI.Button(CalcGrid(1, 7), "Cancel")) {
            mUi = Ui.SavedGame;
        }
    }

    void OpenSavedGame(ConflictResolutionStrategy strategy) {
        SetStandBy("Opening using strategy: " + strategy);
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            mSavedGameFilename,
            DataSource.ReadNetworkOnly,
            strategy,
            (status, openedFile) => {
                mStatus = "Open status for file " + mSavedGameFilename + ": " + status + "\n";
                if (openedFile != null) {
                    mStatus += "Successfully opened file: " + openedFile.ToString();
                    Logger.d("Opened file: " + openedFile.ToString());
                    mCurrentSavedGame = openedFile;
                }
                EndStandBy();
            });
    }

    void DoReadSavedGame() {
        if (mCurrentSavedGame == null) {
            ShowEffect(false);
            mStatus = "No save game selected";
            return;
        }

        if (!mCurrentSavedGame.IsOpen) {
            ShowEffect(false);
            mStatus = "Current saved game is not open. Open it first.";
            return;
        }

        SetStandBy("Reading file: " + mSavedGameFilename);
        var openedFile = mSavedGameFilename;
        PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(mCurrentSavedGame,
            (status, binaryData) => {
                mStatus = "Reading file " + openedFile + ", status: " + status + "\n";

                if (binaryData != null) {
                    var stringContent = System.Text.ASCIIEncoding.Default.GetString(binaryData);
                    mStatus += "File content: " + stringContent;
                    mSavedGameFileContent = stringContent;
                } else {
                    mSavedGameFileContent = "";
                }
                EndStandBy();
            });
    }

    void DoShowSavedGameUI() {
        SetStandBy("Showing saved game UI");
        PlayGamesPlatform.Instance.SavedGame.ShowSelectSavedGameUI(
            "Saved Game UI", 10, false, false,
            (status, savedGame) => {
                mStatus = "UI Status: " + status;
                if (savedGame != null) {
                    mStatus +=
                        "Retrieved saved game with description: " + savedGame.Description;
                    mCurrentSavedGame = savedGame;
                }
                EndStandBy();
            });
    }

    void DoOpenManual() {
        SetStandBy("Manual opening file: " + mSavedGameFilename);
        PlayGamesPlatform.Instance.SavedGame.OpenWithManualConflictResolution(
            mSavedGameFilename,
            DataSource.ReadNetworkOnly,
            true,
            (resolver, original, originalData, unmerged, unmergedData) => {
                Logger.d("Entering conflict callback");
                mConflictResolver = resolver;
                mConflictOriginal = original;
                mConflictOriginalData = System.Text.ASCIIEncoding.Default.GetString(originalData);
                mConflictUnmerged = unmerged;
                mConflictUnmergedData = System.Text.ASCIIEncoding.Default.GetString(unmergedData);
                mUi = Ui.ResolveSaveConflict;
                EndStandBy();
                Logger.d("Encountered manual open conflict.");
            },
            (status, openedFile) => {
                mStatus = "Open status for file " + mSavedGameFilename + ": " + status + "\n";
                if (openedFile != null) {
                    mStatus += "Successfully opened file: " + openedFile.ToString();
                    Logger.d("Opened file: " + openedFile.ToString());
                    mCurrentSavedGame = openedFile;
                }
                EndStandBy();
            }
        );
    }

    void DoFetchAll() {
        SetStandBy("Fetching All Saved Games");
        PlayGamesPlatform.Instance.SavedGame.FetchAllSavedGames(
            DataSource.ReadNetworkOnly,
            (status, savedGames) => {
                mStatus = "Fetch All Status: " + status + "\n";
                mStatus += "Saved Games: [" +
                    string.Join(",", savedGames.Select(g => g.Filename).ToArray()) + "]";
                savedGames.ForEach(g => Logger.d("Retrieved save game: " + g.ToString()));
                EndStandBy();
            });
    }

    void ShowSavedGameUi() {
        DrawTitle("SAVED GAME - Using file: " + mSavedGameFilename);
        DrawStatus();

        if (GUI.Button(CalcGrid(0, 1), "Show UI")) {
            DoShowSavedGameUI();
        } else if (GUI.Button(CalcGrid(1, 1), "Open Manual")) {
            DoOpenManual();
        } else if (GUI.Button(CalcGrid(0, 2), "Open Keep Original")) {
            OpenSavedGame(ConflictResolutionStrategy.UseOriginal);
        } else if (GUI.Button(CalcGrid(1, 2), "Open Keep Unmerged")) {
            OpenSavedGame(ConflictResolutionStrategy.UseUnmerged);
        } else if (GUI.Button(CalcGrid(0, 3), "Read")) {
            DoReadSavedGame();
        } else if (GUI.Button(CalcGrid(1, 3), "Write")) {
            mUi = Ui.WriteSavedGame;
        } else if (GUI.Button(CalcGrid(0, 4), "Fetch All")) {
            DoFetchAll();
        } else if (GUI.Button(CalcGrid(1, 4), "Edit Filename")) {
            mUi = Ui.EditSavedGameName;
        } else if (GUI.Button(CalcGrid(1, 6), "Back")) {
            mUi = Ui.Main;
        }
    }

    void ShowRtmpUi() {
        DrawTitle("REAL-TIME MULTIPLAYER");
        DrawStatus();

        if (GUI.Button(CalcGrid(0,1), "Quick Game 2p")) {
            DoQuickGame(2);
        } else if (GUI.Button(CalcGrid(0,2), "Create Game")) {
            DoCreateGame();
        } else if (GUI.Button(CalcGrid(0,3), "From Inbox")) {
            DoAcceptFromInbox();
        } else if (GUI.Button(CalcGrid(1,1), "Broadcast msg")) {
            DoBroadcastMessage();
        } else if (GUI.Button(CalcGrid(0,4), "Send msg")) {
            DoSendMessage();
        } else if (GUI.Button(CalcGrid(1,2), "Who Is Here")) {
            DoListParticipants();
        } else if (GUI.Button(CalcGrid(1,3), "Accept incoming")) {
            DoAcceptIncoming();
        } else if (GUI.Button(CalcGrid(1,4), "Decline incoming")) {
            DoDeclineIncoming();
        } else if (GUI.Button(CalcGrid(0,5), "Leave Room")) {
            DoLeaveRoom();
        } else if (GUI.Button(CalcGrid(1,5), "Back")) {
            mUi = Ui.Multiplayer;
        }
    }

    void ShowTbmpUi() {
        DrawTitle("TURN-BASED MULTIPLAYER");
        DrawStatus();

        if (GUI.Button(CalcGrid(0,1), "Quick Game 2p")) {
            DoTbmpQuickGame();
        } else if (GUI.Button(CalcGrid(0,2), "Create Game")) {
            DoTbmpCreateGame();
        } else if (GUI.Button(CalcGrid(0,3), "View all Matches")) {
            DoTbmpAcceptFromInbox();
        } else if (GUI.Button(CalcGrid(1,1), "Accept incoming")) {
            DoTbmpAcceptIncoming();
        } else if (GUI.Button(CalcGrid(1,2), "Decline incoming")) {
            DoTbmpDeclineIncoming();
        } else if (GUI.Button(CalcGrid(1,3), "Match...")) {
            if (mMatch == null) {
                mStatus = "No match active.";
            } else {
                mUi = Ui.TbmpMatch;
            }
        } else if (GUI.Button(CalcGrid(1,5), "Back")) {
            mUi = Ui.Multiplayer;
        }
    }

    void ShowTbmpMatchUi() {
        DrawTitle("TURN-BASED MULTIPLAYER MATCH\n" + GetMatchSummary());
        DrawStatus();

        if (GUI.Button(CalcGrid(0,1), "Match Data")) {
            DoTbmpShowMatchData();
        } else if (GUI.Button(CalcGrid(0,2), "Take Turn")) {
            DoTbmpTakeTurn();
        } else if (GUI.Button(CalcGrid(0,3), "Finish")) {
            DoTbmpFinish();
        } else if (GUI.Button(CalcGrid(0,4), "Ack Finish")) {
            DoTbmpAckFinish();
        } else if (GUI.Button(CalcGrid(0,5), "Max Data Size")) {
            mStatus = PlayGamesPlatform.Instance.TurnBased.GetMaxMatchDataSize() + " bytes";
        } else if (GUI.Button(CalcGrid(1,1), "Leave")) {
            DoTbmpLeave();
        } else if (GUI.Button(CalcGrid(1,2), "Leave During Turn")) {
            DoTbmpLeaveDuringTurn();
        } else if (GUI.Button(CalcGrid(1,3), "Cancel")) {
            DoTbmpCancel();
        } else if (GUI.Button(CalcGrid(1,4), "Rematch")) {
            DoTbmpRematch();
        } else if (GUI.Button(CalcGrid(1,5), "Back")) {
            mUi = Ui.Tbmp;
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
        GUI.skin.textArea.fontSize = CalcFontSize();

        if (mStandby) {
            ShowStandbyUi();
        } else if (Social.localUser.authenticated) {
            switch (mUi) {
                case Ui.Rtmp:
                    ShowRtmpUi();
                    break;
                case Ui.Multiplayer:
                    ShowMultiplayerUi();
                    break;
                case Ui.EditSavedGameName:
                    ShowEditSavedGameName();
                    break;
                case Ui.WriteSavedGame:
                    ShowWriteSavedGame();
                    break;
                case Ui.SavedGame:
                    ShowSavedGameUi();
                    break;
                case Ui.ResolveSaveConflict:
                    ShowResolveConflict();
                    break;
                case Ui.Tbmp:
                    ShowTbmpUi();
                    break;
                case Ui.TbmpMatch:
                    ShowTbmpMatchUi();
                    break;
                default:
                    ShowRegularUi();
                    break;
            }
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
        PlayGamesPlatform.InitializeInstance(ClientConfiguration);
        PlayGamesPlatform.Activate();
        Social.localUser.Authenticate((bool success) => {
            EndStandBy();
            if (success) {
                mStatus = "Authenticated. Hello, " + Social.localUser.userName + " (" +
                    Social.localUser.id + ")";

                // register delegates
                PlayGamesPlatform.Instance.RegisterInvitationDelegate(OnInvitationReceived);
                if (PlayGamesPlatform.Instance.TurnBased != null) {
                    PlayGamesPlatform.Instance.TurnBased.RegisterMatchDelegate(
                        OnMatchFromNotification);
                }
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
        mLastLocalSave = word;
        Logger.d("Saved string: " + word);
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

        if (mHadCloudConflict) {
            mStatus += string.Format(" local={0}, server={1}", mConflictLocalVersion,
                mConflictServerVersion);
        }

        ShowEffect(success);
    }

    public byte[] OnStateConflict(int slot, byte[] local, byte[] server) {
        mHadCloudConflict = true;

        mConflictLocalVersion = System.Text.ASCIIEncoding.Default.GetString(local);
        mConflictServerVersion = System.Text.ASCIIEncoding.Default.GetString(server);

        Logger.d(string.Format("Found conflict! local:{0}, server:{1}",
            mConflictLocalVersion,
            mConflictServerVersion
        ));
        return local;
    }

    public void OnStateSaved(bool success, int slot) {
        mStatus = "Cloud save " + (success ? "successful" : "failed") + " word: " + mLastLocalSave;
        ShowEffect(success);
    }

    void DoCloudLoad() {
        mHadCloudConflict = false;
        SetStandBy("Loading from cloud...");
        ((PlayGamesPlatform) Social.Active).LoadState(0, this);
    }

    void DoQuickGame(uint players) {
        uint opponents = players - 1;
        SetStandBy("Starting quick game " + players + " players...");
        PlayGamesPlatform.Instance.RealTime.CreateQuickGame(opponents, opponents,
                0, this);
    }

    void DoCreateGame() {
        SetStandBy("Creating game...");
        PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen(1, 3, 0, this);
    }


    void DoAcceptFromInbox() {
        SetStandBy("Showing inbox...");
        PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(this);
    }

    void DoAcceptIncoming() {
        if (mLastInvitationId == null) {
            mStatus = "No incoming invitation!";
            return;
        }
        SetStandBy("Accepting invitation...");
        PlayGamesPlatform.Instance.RealTime.AcceptInvitation(mLastInvitationId, this);
    }

    void DoDeclineIncoming() {
        if (mLastInvitationId == null) {
            mStatus = "No incoming invitation!";
            return;
        }
        PlayGamesPlatform.Instance.RealTime.DeclineInvitation(mLastInvitationId);
        mStatus = "Declined incoming invitation.";
    }

    void DoBroadcastMessage() {
        string word = GenString();

        bool isReliable = UnityEngine.Random.Range(0, 2) == 0;

        PlayGamesPlatform.Instance.RealTime.SendMessageToAll(isReliable,
                System.Text.ASCIIEncoding.Default.GetBytes(word));
        mStatus = "Sent message: " + word;
    }

    void DoSendMessage() {
        string word = GenString();
        var connected = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();
        var self = PlayGamesPlatform.Instance.RealTime.GetSelf();

        var nonSelf = connected.Where(p => !p.Equals(self)).ToList();

        bool isReliable = UnityEngine.Random.Range(0, 2) == 0;
        var recipient = nonSelf[UnityEngine.Random.Range(0, nonSelf.Count)];

        PlayGamesPlatform.Instance.RealTime.SendMessage(isReliable,
            recipient.ParticipantId,
            System.Text.ASCIIEncoding.Default.GetBytes(word));
        mStatus = string.Format("Sent message: {0}, reliable: {1}, recipient: {2}",
            word, isReliable, recipient.ParticipantId);
    }

    void DoLeaveRoom() {
        mStatus = "Requested to leave room.";
        PlayGamesPlatform.Instance.RealTime.LeaveRoom();
    }

    public void OnRoomConnected(bool success) {
        ShowEffect(success);
        mStatus = success ? "Room connected!" : "Room setup failed!";
        EndStandBy();
    }

    public void OnLeftRoom() {
        mStatus = "Left room.";
    }

    public void OnPeersConnected(string[] participantIds) {
        mStatus = "Peers connected: ";
        foreach (string pid in participantIds) {
            Participant p = PlayGamesPlatform.Instance.RealTime.GetParticipant(pid);
            if (p != null) {
                mStatus += pid + "(" + p.DisplayName + ") ";
            } else {
                mStatus += pid + "(NULL) ";
            }
        }
    }

    public void OnPeersDisconnected(string[] participantIds) {
        mStatus = "Peers disconnected: ";
        foreach (string pid in participantIds) {
            Participant p = PlayGamesPlatform.Instance.RealTime.GetParticipant(pid);
            if (p != null) {
                mStatus += pid + "(" + p.DisplayName + ") ";
            } else {
                mStatus += pid + "(NULL) ";
            }
        }
    }

    public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data) {
        mStatus = string.Format("Got message. Reliable:{0} From:{1} Data: {2}",
            isReliable, senderId,
            System.Text.ASCIIEncoding.Default.GetString(data));
    }

    public void OnRoomSetupProgress(float progress) {
        SetStandBy("Setting up room (" + ((int)progress) + "%)");
    }

    void DoListParticipants() {
        List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();
        if (participants == null) {
            mStatus = "Participants: (null list)";
            return;
        }
        mStatus = string.Format("{0} participants.", participants.Count);
        Participant self = PlayGamesPlatform.Instance.RealTime.GetSelf();
        foreach (Participant p in participants) {
            if (self.ParticipantId.Equals(p.ParticipantId)) {
                mStatus += "*";
            }
            mStatus += p.DisplayName + "(" + p.ParticipantId + ") ";
            Debug.Log(">>> participant: " + p.ToString());
        }
    }

    void OnInvitationReceived(Invitation invitation, bool fromNotification) {
        string inviterName = invitation.Inviter != null ? invitation.Inviter.DisplayName : "(null)";
        mStatus = "!!! Got invitation " + (fromNotification ? " (from notification):" : ":") +
            " from " + inviterName + ", id " + invitation.InvitationId;
        mLastInvitationId = invitation.InvitationId;
    }

    void OnMatchFromNotification(TurnBasedMatch match, bool fromNotification) {
        if (fromNotification) {
            mUi = Ui.TbmpMatch;
            mMatch = match;
            mStatus = "Got match from notification! " + match;
        } else {
            mStatus = "Got match a update not from notification.";
        }
    }

    void DoTbmpQuickGame() {
        SetStandBy("Creating TBMP quick match...");
        PlayGamesPlatform.Instance.TurnBased.CreateQuickMatch(1, 1, 0,
                (bool success, TurnBasedMatch match) => {
            ShowEffect(success);
            EndStandBy();
            mMatch = match;
            mStatus = success ? "Match created" : "Match creation failed";
            if (success) {
                mUi = Ui.TbmpMatch;
            }
        });
    }

    void DoTbmpCreateGame() {
        SetStandBy("Creating TBMP match...");
        PlayGamesPlatform.Instance.TurnBased.CreateWithInvitationScreen(1, 7, 0,
                (bool success, TurnBasedMatch match) => {
            ShowEffect(success);
            EndStandBy();
            mMatch = match;
            mStatus = success ? "Match created" : "Match creation failed";
            if (success) {
                mUi = Ui.TbmpMatch;
            }
        });
    }

    void DoTbmpAcceptFromInbox() {
        SetStandBy("Accepting TBMP from inbox...");
        PlayGamesPlatform.Instance.TurnBased.AcceptFromInbox((bool success, TurnBasedMatch match) => {
            ShowEffect(success);
            EndStandBy();
            mMatch = match;
            mStatus = success ? "Successfully accepted from inbox!" : "Failed to accept from inbox";
            if (success) {
                mUi = Ui.TbmpMatch;
            }
        });
    }

    void DoTbmpAcceptIncoming() {
        if (mLastInvitationId == null) {
            mStatus = "No incoming invitation received from listener.";
            return;
        }
        SetStandBy("Accepting TBMP invitation...");
        PlayGamesPlatform.Instance.TurnBased.AcceptInvitation(mLastInvitationId,
                (bool success, TurnBasedMatch match) => {
            ShowEffect(success);
            EndStandBy();
            mMatch = match;
            mStatus = success ? "Successfully accepted invitation!" :
                "Failed to accept invitation";
            if (success) {
                mUi = Ui.TbmpMatch;
            }
        });
    }

    void DoTbmpDeclineIncoming() {
        if (mLastInvitationId == null) {
            mStatus = "No incoming invitation received from listener.";
            return;
        }
        PlayGamesPlatform.Instance.TurnBased.DeclineInvitation(mLastInvitationId);
        mLastInvitationId = null;
        mStatus = "Declined invitation.";
    }

    string GetMatchSummary() {
        string summary = "";

        if (mMatch == null) {
            return "(null)";
        }

        string data = "(null)";
        if (mMatch.Data != null) {
            data = System.Text.ASCIIEncoding.Default.GetString(mMatch.Data);
        }

        summary = "Match: [" + data + "], S:" + mMatch.Status + ", T:" + mMatch.TurnStatus + "\n";
        summary += "With: ";
        foreach (Participant p in mMatch.Participants) {
            summary += " " + p.DisplayName;
        }
        summary += " and " + mMatch.AvailableAutomatchSlots + " pending automatch";
        return summary;
    }

    void DoTbmpShowMatchData() {
        if (mMatch == null) {
            mStatus = "No match is active!";
            return;
        }

        mStatus = mMatch.ToString();
    }

    // figure out who is next to play
    string GetNextToPlay(TurnBasedMatch match) {
        if (mMatch.AvailableAutomatchSlots > 0) {
            // next to play is an automatch player
            return null;
        }

        // WARNING: The following code for determining "who is next" MUST NOT BE USED
        // in a production game. It is here for debug purposes only. This code will
        // not take into account the order in which the first round (while there were
        // automatch slots open) was played, and will always produce round-robin next
        // participants based on the participant ID, which might make the second (and
        // subsequent) rounds have a different play order than the first round, which is,
        // for most games, a very bad experience.
        //
        // In your production game, consider storing the play order in the match data
        // to help determine who plays next.

        // what is my index in the list of participants?
        int myIndex = -1;
        List<Participant> participants = mMatch.Participants;

        for (int i = 0; i < participants.Count; i++) {
            Participant p = participants[i];
            if (p.ParticipantId.Equals(mMatch.SelfParticipantId)) {
                myIndex = i;
                break;
            }
        }

        GooglePlayGames.OurUtils.Logger.d("My index = " + myIndex);

        // who is the next participant in the Joined state?
        for (int j = 1; j <= participants.Count; j++) {
            Participant p = participants[(myIndex + j) % participants.Count];
            if (p.Status == Participant.ParticipantStatus.Joined ||
                p.Status == Participant.ParticipantStatus.NotInvitedYet) {
                GooglePlayGames.OurUtils.Logger.d("Using index = " + (myIndex + j) % participants.Count);
                return p.ParticipantId;
            }
        }

        Debug.LogError("*** ERROR: Failed to get next participant to play. No one available.");
        return null;
    }

    void DoTbmpTakeTurn() {
        if (mMatch == null) {
            mStatus = "No match is active.";
            return;
        }
        if (mMatch.TurnStatus != TurnBasedMatch.MatchTurnStatus.MyTurn) {
            mStatus = "Not my turn.";
            return;
        }

        SetStandBy("Taking turn...");
        PlayGamesPlatform.Instance.TurnBased.TakeTurn(mMatch,
                System.Text.ASCIIEncoding.Default.GetBytes(GenString()),
                GetNextToPlay(mMatch),
                (bool success) => {
            EndStandBy();
            ShowEffect(success);
            mStatus = success ? "Successfully took turn." : "Failed to take turn.";
            if (success) {
                mMatch = null;
                mUi = Ui.Tbmp;
            }
        });
    }

    void DoTbmpFinish() {
        if (mMatch == null) {
            mStatus = "No match is active.";
            return;
        }
        if (mMatch.TurnStatus != TurnBasedMatch.MatchTurnStatus.MyTurn) {
            mStatus = "Not my turn.";
            return;
        }

        // I win; every one else loses
        MatchOutcome outcome = new MatchOutcome();
        foreach (Participant p in mMatch.Participants) {
            if (p.ParticipantId.Equals(mMatch.SelfParticipantId)) {
                outcome.SetParticipantResult(p.ParticipantId,
                    MatchOutcome.ParticipantResult.Win, 1);
            } else {
                outcome.SetParticipantResult(p.ParticipantId,
                    MatchOutcome.ParticipantResult.Loss, 2);
            }
        }

        SetStandBy("Finishing match...");
        PlayGamesPlatform.Instance.TurnBased.Finish(mMatch,
                System.Text.ASCIIEncoding.Default.GetBytes("the end!"),
                outcome, (bool success) => {

            EndStandBy();
            ShowEffect(success);
            mStatus = success ? "Successfully finished match." : "Failed to finish match.";
            if (success) {
                mMatch = null;
                mUi = Ui.Tbmp;
            }
        });
    }

    void DoTbmpAckFinish() {
        if (mMatch == null) {
            mStatus = "No match is active.";
            return;
        }
        if (mMatch.Status != TurnBasedMatch.MatchStatus.Complete) {
            mStatus = "Match is not complete";
            return;
        }

        SetStandBy("Ack'ing finished match");
        PlayGamesPlatform.Instance.TurnBased.AcknowledgeFinished(mMatch, (bool success) => {
            EndStandBy();
            ShowEffect(success);
            mStatus = success ? "Successfully ack'ed finish." : "Failed to ack finish.";
            if (success) {
                mMatch = null;
                mUi = Ui.Tbmp;
            }
        });
    }

    void DoTbmpLeave() {
        if (mMatch == null) {
            mStatus = "No match is active.";
            return;
        }
        if (mMatch.TurnStatus == TurnBasedMatch.MatchTurnStatus.MyTurn) {
            mStatus = "It's my turn; use 'Leave During Turn'.";
            return;
        }

        SetStandBy("Leaving match...");
        PlayGamesPlatform.Instance.TurnBased.Leave(mMatch, (bool success) => {
            EndStandBy();
            ShowEffect(success);
            mStatus = success ? "Successfully left match." : "Failed to leave match.";
            if (success) {
                mMatch = null;
                mUi = Ui.Tbmp;
            }
        });
    }

    void DoTbmpLeaveDuringTurn() {
        if (mMatch == null) {
            mStatus = "No match is active.";
            return;
        }
        if (mMatch.TurnStatus != TurnBasedMatch.MatchTurnStatus.MyTurn) {
            mStatus = "It's not my turn.";
            return;
        }

        SetStandBy("Leaving match during turn...");
        PlayGamesPlatform.Instance.TurnBased.LeaveDuringTurn(mMatch, GetNextToPlay(mMatch),
                (bool success) => {
            EndStandBy();
            ShowEffect(success);
            mStatus = success ? "Successfully left match during turn." :
                "Failed to leave match during turn.";
            if (success) {
                mMatch = null;
                mUi = Ui.Tbmp;
            }
        });
    }

    void DoTbmpCancel() {
        if (mMatch == null) {
            mStatus = "No match is active.";
            return;
        }
        if (mMatch.Status != TurnBasedMatch.MatchStatus.Active) {
            mStatus = "Match is not active.";
            return;
        }

        SetStandBy("Cancelling match...");
        PlayGamesPlatform.Instance.TurnBased.Cancel(mMatch, (bool success) => {
            EndStandBy();
            ShowEffect(success);
            mStatus = success ? "Successfully cancelled match." : "Failed to cancel match.";
            if (success) {
                mMatch = null;
                mUi = Ui.Tbmp;
            }
        });
    }

    void DoTbmpRematch() {
        if (mMatch == null) {
            mStatus = "No match is active.";
            return;
        }
        if (!mMatch.CanRematch) {
            mStatus = "Match can't be rematched.";
            return;
        }

        SetStandBy("Rematching match...");
        PlayGamesPlatform.Instance.TurnBased.Rematch(mMatch,
                (bool success, TurnBasedMatch match) => {
            EndStandBy();
            ShowEffect(success);
            mMatch = match;
            mStatus = success ? "Successfully rematched." : "Failed to rematch.";
            if (success) {
                // if we succeed, it will be our turn, so go to the appropriate UI
                mUi = Ui.TbmpMatch;
            }
        });
    }
}
