﻿/*
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
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;

public class MainGui : MonoBehaviour, GooglePlayGames.BasicApi.OnStateLoadedListener,
        RealTimeMultiplayerListener {
    const int Margin = 20, Spacing = 10;
    const float FontSizeFactor = 35;
    const int GridCols = 2;
    const int GridRows = 8;

    // which UI are we showing?
    enum Ui { Main, Multiplayer, Rtmp, Tbmp, TbmpMatch };
    Ui mUi = Ui.Main;

    public GUISkin GuiSkin;

    bool mStandby = false;
    string mStandbyMessage = "";
    string mStatus = "Ready.";
    string mLastInvitationId = null;

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
        GUI.Label(CalcGrid(0, 6, 2, 2), mStatus);
    }

    void ShowNotAuthUi() {
        DrawTitle(null);
        DrawStatus();
        if (GUI.Button(CalcGrid(1, 1), "Authenticate")) {
            DoAuthenticate(false);
        } else if (GUI.Button(CalcGrid(1, 2), "Silent Auth")) {
            DoAuthenticate(true);
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

    void ShowRtmpUi() {
        DrawTitle("REAL-TIME MULTIPLAYER");
        DrawStatus();

        if (GUI.Button(CalcGrid(0,1), "Quick Game 2p")) {
            DoQuickGame(2);
        } else if (GUI.Button(CalcGrid(0,2), "Create Game")) {
            DoCreateGame();
        } else if (GUI.Button(CalcGrid(0,3), "From Inbox")) {
            DoAcceptFromInbox();
        } else if (GUI.Button(CalcGrid(1,1), "Send msg")) {
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

    void DoAuthenticate(bool silent) {
        if (!silent) {
            SetStandBy("Authenticating...");
        }

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate((bool success) => {
            EndStandBy();
            if (success) {
                mStatus = "Authenticated. Hello, " + Social.localUser.userName + " (" +
                    Social.localUser.id + ")";

                // register delegates
                PlayGamesPlatform.Instance.RegisterInvitationDelegate(OnInvitationReceived);
                PlayGamesPlatform.Instance.TurnBased.RegisterMatchDelegate(OnMatchFromNotification);
            } else {
                mStatus = "*** Failed to authenticate.";
            }
            ShowEffect(success);
        }, silent);
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

    void DoQuickGame(int players) {
        int opponents = players - 1;
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

    void DoSendMessage() {
        string word = GenString();
        PlayGamesPlatform.Instance.RealTime.SendMessageToAll(true,
                System.Text.ASCIIEncoding.Default.GetBytes(word));
        mStatus = "Sent message: " + word;
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
        mStatus = "Got message: " + System.Text.ASCIIEncoding.Default.GetString(data);
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
        
        // who is the next participant in the Joined state?
        for (int j = 0; j < participants.Count; j++) {
            Participant p = participants[(myIndex + j) % participants.Count];
            if (p.Status == Participant.ParticipantStatus.Joined) {
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
        PlayGamesPlatform.Instance.TurnBased.TakeTurn(mMatch.MatchId,
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
            outcome.SetParticipantResult(p.ParticipantId,
                p.ParticipantId.Equals(mMatch.SelfParticipantId) ?
                    MatchOutcome.ParticipantResult.Win :
                    MatchOutcome.ParticipantResult.Loss);
        }

        SetStandBy("Finishing match...");
        PlayGamesPlatform.Instance.TurnBased.Finish(mMatch.MatchId,
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
        PlayGamesPlatform.Instance.TurnBased.AcknowledgeFinished(mMatch.MatchId, (bool success) => {
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
        PlayGamesPlatform.Instance.TurnBased.Leave(mMatch.MatchId, (bool success) => {
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
        PlayGamesPlatform.Instance.TurnBased.LeaveDuringTurn(mMatch.MatchId, GetNextToPlay(mMatch),
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
        PlayGamesPlatform.Instance.TurnBased.Cancel(mMatch.MatchId, (bool success) => {
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
        PlayGamesPlatform.Instance.TurnBased.Rematch(mMatch.MatchId,
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
