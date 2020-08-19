// <copyright file="MainGui.cs" company="Google Inc.">
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

using System;
using System.Collections.Generic;
using System.Linq;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.OurUtils;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace SmokeTest
{
    public class MainGui : MonoBehaviour
    {
        public GUISkin GuiSkin;

        private const int Spacing = 4;
        private const int Margin = 8;
        private const float FontSizeFactor = 35;
        private const int GridCols = 2;
        private const int GridRows = 10;

        private static PlayGamesClientConfiguration ClientConfiguration;

        private Ui mUi = Ui.Main;

        private bool mStandby = false;
        private string mStandbyMessage = string.Empty;
        private string _mStatus = "Ready";
        private string mSavedGameFilename = "default_name";
        private ISavedGameMetadata mCurrentSavedGame = null;
        private string mSavedGameFileContent = string.Empty;
        private IConflictResolver mConflictResolver = null;
        private ISavedGameMetadata mConflictOriginal = null;
        private string mConflictOriginalData = null;
        private ISavedGameMetadata mConflictUnmerged = null;
        private string mConflictUnmergedData = null;
        private string mLastLocalSave = null;

        private string mConflictLocalVersion = null;
        private string mConflictServerVersion = null;
        private bool mHadCloudConflict = false;

        private string idToken = "";
        private string authCode = "";
        private string statsMessage = string.Empty;

        private NearbyGUI mNearbyGui;
        private AchievementGUI mAchievementGui;
        private LeaderboardGUI mLeaderboardGui;
        private VideoGUI mVideoGui;
        private FriendsGUI mFriendsGui;

        // which UI are we showing?
        public enum Ui {
          Main,
          SavedGame,
          EditSavedGameName,
          WriteSavedGame,
          ResolveSaveConflict,
          Events,
          NearbyConnections,
          Achievements,
          Leaderboards,
          Video,
          UserInfo,
          PopupGravity,
          Permissions,
          Friends
        }

        public void Start()
        {
            Screen.orientation = ScreenOrientation.Portrait;

            PlayGamesPlatform.DebugLogEnabled = true;
            this.mNearbyGui = new NearbyGUI(this);
            this.mAchievementGui = new AchievementGUI(this);
            this.mLeaderboardGui = new LeaderboardGUI(this);
            this.mVideoGui = new VideoGUI(this);
            this.mFriendsGui = new FriendsGUI(this);
        }

        public void SetUI(Ui page)
        {
            idToken = null;
            authCode = null;
            this.mUi = page;
        }

        public Rect CalcGrid(int col, int row)
        {
            return this.CalcGrid(col, row, 1, 1);
        }

        Rect CalcGrid(int col, int row, int colcount, int rowcount)
        {
            int cellW = (Screen.width - 2 * Margin - (GridCols - 1) * Spacing) / GridCols;
            int cellH = (Screen.height - 2 * Margin - (GridRows - 1) * Spacing) / GridRows;
            return new Rect(Margin + col * (cellW + Spacing),
                Margin + row * (cellH + Spacing),
                cellW + (colcount - 1) * (Spacing + cellW),
                cellH + (rowcount - 1) * (Spacing + cellH));
        }

        public byte[] OnStateConflict(int slot, byte[] local, byte[] server)
        {
            mHadCloudConflict = true;

            mConflictLocalVersion = System.Text.ASCIIEncoding.Default.GetString(local);
            mConflictServerVersion = System.Text.ASCIIEncoding.Default.GetString(server);

            GooglePlayGames.OurUtils.Logger.d(
                string.Format(
                    "Found conflict! local:{0}, server:{1}",
                    mConflictLocalVersion,
                    mConflictServerVersion));
            return local;
        }

        public void OnStateSaved(bool success, int slot)
        {
            Status = "Cloud save " + (success ? "successful" : "failed") + " word: " + mLastLocalSave;
            ShowEffect(success);
        }

        public void OnStateLoaded(bool success, int slot, byte[] data)
        {
            EndStandBy();
            if (success)
            {
                Status = "Loaded from cloud: " + System.Text.ASCIIEncoding.Default.GetString(data);
            }
            else
            {
                Status = "*** Failed to load from cloud.";
            }

            Status += ". conflict=" + (mHadCloudConflict ? "yes" : "no");

            if (mHadCloudConflict)
            {
                Status += string.Format(" local={0}, server={1}", mConflictLocalVersion, mConflictServerVersion);
            }

            ShowEffect(success);
        }

        internal void ShowStandbyUi()
        {
            GUI.Label(this.CalcGrid(0, 2, 2, 1), this.mStandbyMessage);
        }

        internal void ShowNotAuthUi()
        {
            this.DrawTitle(null);
            this.DrawStatus();
            if (GUI.Button(this.CalcGrid(0, 1), "Authenticate - Simple"))
            {
                this.DoAuthenticate(new PlayGamesClientConfiguration.Builder().Build());
            }
            else if (GUI.Button(this.CalcGrid(1, 1), "Authenticate - ID Token"))
            {
                this.DoAuthenticate(new PlayGamesClientConfiguration.Builder()
                    .RequestIdToken()
                    .Build());
            }
            else if (GUI.Button(this.CalcGrid(0, 2), "Authenticate - Server Auth Code"))
            {
                this.DoAuthenticate(new PlayGamesClientConfiguration.Builder()
                    .RequestServerAuthCode(false)
                    .Build());
            }
            else if (GUI.Button(this.CalcGrid(1, 2), "Authenticate - Enable Saved Games"))
            {
                this.DoAuthenticate(new PlayGamesClientConfiguration.Builder()
                    .EnableSavedGames()
                    .Build());
            }
            else if (GUI.Button(this.CalcGrid(0, 3), "Authenticate - Hide Popups"))
            {
                this.DoAuthenticate(new PlayGamesClientConfiguration.Builder()
                    .EnableHidePopups()
                    .Build());
            }
            else if (GUI.Button(this.CalcGrid(1, 3), "Authenticate - Full"))
            {
                this.DoAuthenticate(new PlayGamesClientConfiguration.Builder()
                    .RequestIdToken()
                    .RequestEmail()
                    .RequestServerAuthCode(false)
                    .EnableSavedGames()
                    .Build());
            }
            else if (GUI.Button(this.CalcGrid(0, 4), "SignInInteractivity - NoPrompt"))
            {
                this.DoAuthenticate(SignInInteractivity.NoPrompt,
                    new PlayGamesClientConfiguration.Builder().Build());
            }
            else if (GUI.Button(this.CalcGrid(1, 4), "SignInInteractivity - CanPromptOnce"))
            {
                this.DoAuthenticate(SignInInteractivity.CanPromptOnce,
                    new PlayGamesClientConfiguration.Builder().Build());
            }
            else if (GUI.Button(this.CalcGrid(0, 5), "SignInInteractivity - CanPromptAlways"))
            {
                this.DoAuthenticate(SignInInteractivity.CanPromptAlways,
                    new PlayGamesClientConfiguration.Builder().Build());
            }
            else if (GUI.Button(this.CalcGrid(1, 5), "Nearby Connections"))
            {
                SetUI(Ui.NearbyConnections);
            }
        }

        internal void ShowRegularUi()
        {
            this.DrawTitle(null);
            this.DrawStatus();

            if (GUI.Button(this.CalcGrid(0, 1), "Achievements"))
            {
                SetUI(Ui.Achievements);
            }

            if (GUI.Button(this.CalcGrid(1, 1), "Events"))
            {
                SetUI(Ui.Events);
            }
            else if (GUI.Button(CalcGrid(0, 2), "Leaderboards"))
            {
                SetUI(Ui.Leaderboards);
            }
            else if (GUI.Button(CalcGrid(1, 2), "User Info"))
            {
                SetUI(Ui.UserInfo);
            } else if (GUI.Button(this.CalcGrid(0, 3), "Friends"))
            {
                SetUI(Ui.Friends);
            } else if (GUI.Button(this.CalcGrid(1, 3), "Saved Game"))
            {
                SetUI(Ui.SavedGame);
            } else if (GUI.Button(this.CalcGrid(0, 4), "Video"))
            {
                SetUI(Ui.Video);
            } else if (GUI.Button(this.CalcGrid(1, 4), "Nearby Connections"))
            {
                SetUI(Ui.NearbyConnections);
            } else if (GUI.Button(this.CalcGrid(0, 5), "Popup Gravity"))
            {
                SetUI(Ui.PopupGravity);
            } else if (GUI.Button(this.CalcGrid(1, 5), "Permissions"))
            {
                SetUI(Ui.Permissions);
            } else if (GUI.Button(this.CalcGrid(0, 6), "Sign Out"))
            {
                this.DoSignOut();
            }
        }
        internal void ShowEditSavedGameName()
        {
            this.DrawTitle("EDIT SAVED GAME FILENAME");
            this.DrawStatus();

            this.mSavedGameFilename = GUI.TextArea(this.CalcGrid(0, 1), this.mSavedGameFilename);

            if (GUI.Button(this.CalcGrid(1, 7), "Back"))
            {
                SetUI(Ui.SavedGame);
                ShowEffect(true);
            }
        }

        internal void ShowResolveConflict()
        {
            this.DrawTitle("RESOLVE SAVE GAME CONFLICT");
            this.DrawStatus();

            if (this.mConflictResolver == null)
            {
                Status = "No pending conflict";
                SetUI(Ui.SavedGame);
                return;
            }

            string msg = "Original: " + mConflictOriginal.Filename + ":" + mConflictOriginal.Description + "\n" +
                         "Data: " + mConflictOriginalData;
            GUI.Label(CalcGrid(0, 1, 2, 2), msg);

            msg = "Unmerged: " + mConflictUnmerged.Filename + ":" + mConflictUnmerged.Description + "\n" +
                  "Data: " + mConflictUnmergedData;
            GUI.Label(CalcGrid(0, 2, 2, 2), msg);

            if (GUI.Button(CalcGrid(0, 3), "Use Original"))
            {
                mConflictResolver.ChooseMetadata(mConflictOriginal);
                SetStandBy("Choosing original, retrying open");
                SetUI(Ui.SavedGame);
            }
            else if (GUI.Button(CalcGrid(1, 3), "Use Unmerged"))
            {
                mConflictResolver.ChooseMetadata(mConflictUnmerged);
                SetStandBy("Choosing unmerged, retrying open");
                SetUI(Ui.SavedGame);
            }
            else if (GUI.Button(CalcGrid(0, 4), "Use new data"))
            {
                SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
                builder = builder.WithUpdatedDescription(mConflictOriginal.Description + " (resolved).");
                mConflictResolver.ResolveConflict(mConflictOriginal, builder.Build(),
                    System.Text.ASCIIEncoding.Default.GetBytes(mSavedGameFileContent + " resolved"));
            }

            if (GUI.Button(CalcGrid(1, 7), "Back"))
            {
                SetUI(Ui.SavedGame);
                ShowEffect(true);
            }
        }

        internal void ShowWriteSavedGame()
        {
            DrawTitle("WRITE SAVED GAME");
            DrawStatus();

            mSavedGameFileContent = GUI.TextArea(CalcGrid(0, 1), mSavedGameFileContent);

            if (mCurrentSavedGame == null || !mCurrentSavedGame.IsOpen)
            {
                Status = "No opened saved game selected.";
                SetUI(Ui.SavedGame);
                return;
            }

            var update = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedDescription("Saved at " + DateTime.Now.ToString())
                .WithUpdatedPlayedTime(mCurrentSavedGame.TotalTimePlayed.Add(TimeSpan.FromHours(1)))
                .Build();

            if (GUI.Button(CalcGrid(0, 7), "Write"))
            {
                SetStandBy("Writing update");
                PlayGamesPlatform.Instance.SavedGame.CommitUpdate(
                    mCurrentSavedGame,
                    update,
                    System.Text.ASCIIEncoding.Default.GetBytes(mSavedGameFileContent),
                    (status, updated) =>
                    {
                        Status = "Write status was: " + status;
                        SetUI(Ui.SavedGame);
                        EndStandBy();
                    });
                mCurrentSavedGame = null;
            }
            else if (GUI.Button(CalcGrid(1, 7), "Cancel"))
            {
                SetUI(Ui.SavedGame);
            }
        }

        internal void OpenSavedGame(ConflictResolutionStrategy strategy)
        {
            SetStandBy("Opening using strategy: " + strategy);
            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
                mSavedGameFilename,
                DataSource.ReadNetworkOnly,
                strategy,
                (status, openedFile) =>
                {
                    Status = "Open status for file " + mSavedGameFilename + ": " + status + "\n";
                    if (openedFile != null)
                    {
                        Status += "Successfully opened file: " + openedFile;
                        GooglePlayGames.OurUtils.Logger.d("Opened file: " + openedFile);
                        mCurrentSavedGame = openedFile;
                    }

                    EndStandBy();
                });
        }

        internal void DoDeleteSavedGame()
        {
            if (mCurrentSavedGame == null)
            {
                ShowEffect(false);
                Status = "No save game selected";
                return;
            }

            PlayGamesPlatform.Instance.SavedGame.Delete(mCurrentSavedGame);
            Status = mCurrentSavedGame.Filename + " deleted.";
            mCurrentSavedGame = null;
        }

        internal void DoReadSavedGame()
        {
            if (mCurrentSavedGame == null)
            {
                ShowEffect(false);
                Status = "No save game selected";
                return;
            }

            if (!mCurrentSavedGame.IsOpen)
            {
                ShowEffect(false);
                Status = "Current saved game is not open. Open it first.";
                return;
            }

            SetStandBy("Reading file: " + mSavedGameFilename);
            var openedFile = mSavedGameFilename;
            PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(
                mCurrentSavedGame,
                (status, binaryData) =>
                {
                    Status = "Reading file " + openedFile + ", status: " + status + "\n";

                    if (binaryData != null)
                    {
                        var stringContent = System.Text.ASCIIEncoding.Default.GetString(binaryData);
                        Status += "File content: " + stringContent;
                        mSavedGameFileContent = stringContent;
                    }
                    else
                    {
                        mSavedGameFileContent = string.Empty;
                    }

                    EndStandBy();
                });
        }

        internal void DoShowSavedGameUI()
        {
            SetStandBy("Showing saved game UI");
            PlayGamesPlatform.Instance.SavedGame.ShowSelectSavedGameUI(
                "Saved Game UI",
                10,
                false,
                false,
                (status, savedGame) =>
                {
                    Status = "UI Status: " + status;
                    if (savedGame != null)
                    {
                        Status +=
                            "Retrieved saved game with description: " + savedGame.Description;
                        mCurrentSavedGame = savedGame;
                    }

                    EndStandBy();
                });
        }

        internal void DoOpenManual()
        {
            SetStandBy("Manual opening file: " + mSavedGameFilename);
            PlayGamesPlatform.Instance.SavedGame.OpenWithManualConflictResolution(
                mSavedGameFilename,
                DataSource.ReadNetworkOnly,
                true,
                (resolver, original, originalData, unmerged, unmergedData) =>
                {
                    GooglePlayGames.OurUtils.Logger.d("Entering conflict callback");
                    mConflictResolver = resolver;
                    mConflictOriginal = original;
                    mConflictOriginalData = System.Text.ASCIIEncoding.Default.GetString(originalData);
                    mConflictUnmerged = unmerged;
                    mConflictUnmergedData = System.Text.ASCIIEncoding.Default.GetString(unmergedData);
                    SetUI(Ui.ResolveSaveConflict);
                    EndStandBy();
                    GooglePlayGames.OurUtils.Logger.d("Encountered manual open conflict.");
                },
                (status, openedFile) =>
                {
                    Status = "Open status for file " + mSavedGameFilename + ": " + status + "\n";
                    if (openedFile != null)
                    {
                        Status += "Successfully opened file: " + openedFile;
                        GooglePlayGames.OurUtils.Logger.d("Opened file: " + openedFile);
                        mCurrentSavedGame = openedFile;
                    }

                    EndStandBy();
                });
        }

        internal void DoFetchAll()
        {
            SetStandBy("Fetching All Saved Games");
            PlayGamesPlatform.Instance.SavedGame.FetchAllSavedGames(
                DataSource.ReadNetworkOnly,
                (status, savedGames) =>
                {
                    Status = "Fetch All Status: " + status + "\n";
                    Status += "Saved Games: [" +
                              string.Join(",", savedGames.Select(g => g.Filename).ToArray()) + "]";
                    savedGames.ForEach(g =>
                        GooglePlayGames.OurUtils.Logger.d("Retrieved save game: " + g));
                    EndStandBy();
                });
        }

        internal void ShowSavedGameUi()
        {
            DrawTitle("SAVED GAME - Using file: " + mSavedGameFilename);
            DrawStatus();

            if (GUI.Button(CalcGrid(0, 1), "Show UI"))
            {
                DoShowSavedGameUI();
            }
            else if (GUI.Button(CalcGrid(1, 1), "Open Manual"))
            {
                DoOpenManual();
            }
            else if (GUI.Button(CalcGrid(0, 2), "Open Keep Original"))
            {
                OpenSavedGame(ConflictResolutionStrategy.UseOriginal);
            }
            else if (GUI.Button(CalcGrid(1, 2), "Open Keep Unmerged"))
            {
                OpenSavedGame(ConflictResolutionStrategy.UseUnmerged);
            }
            else if (GUI.Button(CalcGrid(0, 3), "Read"))
            {
                DoReadSavedGame();
            }
            else if (GUI.Button(CalcGrid(1, 3), "Write"))
            {
                SetUI(Ui.WriteSavedGame);
            }
            else if (GUI.Button(CalcGrid(0, 4), "Fetch All"))
            {
                DoFetchAll();
            }
            else if (GUI.Button(CalcGrid(1, 4), "Edit Filename"))
            {
                SetUI(Ui.EditSavedGameName);
            }
            else if (GUI.Button(CalcGrid(0, 5), "Delete"))
            {
                DoDeleteSavedGame();
            }
            else if (GUI.Button(CalcGrid(1, 6), "Back"))
            {
                SetUI(Ui.Main);
                ShowEffect(true);
            }
        }
        internal void ShowEventsUi()
        {
            DrawStatus();
            DrawTitle("Events");
            if (GUI.Button(CalcGrid(0, 1), "Fetch All Events"))
            {
                SetStandBy("Fetching All Events");
                PlayGamesPlatform.Instance.Events.FetchAllEvents(
                    DataSource.ReadNetworkOnly,
                    (status, events) =>
                    {
                        Status = "Fetch All Status: " + status + "\n";
                        Status += "Events: [" +
                                  string.Join(",", events.Select(g => g.Id).ToArray()) + "]";
                        events.ForEach(e =>
                            GooglePlayGames.OurUtils.Logger.d("Retrieved event: " + e));
                        EndStandBy();
                    });
            }
            else if (GUI.Button(CalcGrid(1, 1), "Fetch Event"))
            {
                SetStandBy("Fetching Event");
                PlayGamesPlatform.Instance.Events.FetchEvent(
                    DataSource.ReadNetworkOnly,
                    GPGSIds.event_smokingevent,
                    (status, fetchedEvent) =>
                    {
                        Status = "Fetch Status: " + status + "\n";
                        if (fetchedEvent != null)
                        {
                            Status += "Event: [" + fetchedEvent.Id + ", " + fetchedEvent.Description + "]: " +
                                      fetchedEvent.CurrentCount;
                            GooglePlayGames.OurUtils.Logger.d("Fetched event: " + fetchedEvent);
                        }

                        EndStandBy();
                    });
            }
            else if (GUI.Button(CalcGrid(0, 2), "Increment Event"))
            {
                PlayGamesPlatform.Instance.Events.IncrementEvent(
                    GPGSIds.event_smokingevent, 10);
            }

            if (GUI.Button(CalcGrid(1, 6), "Back"))
            {
                SetUI(Ui.Main);
            }
        }

        private void ShowPopupGravityUi()
        {
            DrawStatus();
            DrawTitle("PopupGravity Ui");
            if (GUI.Button(CalcGrid(0, 1), "Top"))
            {
                PlayGamesPlatform.Instance.SetGravityForPopups(Gravity.TOP);
                Status = "Popup will appear on top";
            }
            else if (GUI.Button(CalcGrid(1, 1), "Bottom"))
            {
                PlayGamesPlatform.Instance.SetGravityForPopups(Gravity.BOTTOM);
                Status = "Popup will appear at bottom";
            }
            else if (GUI.Button(CalcGrid(0, 2), "Left"))
            {
                PlayGamesPlatform.Instance.SetGravityForPopups(Gravity.LEFT);
                Status = "Popup will appear on left";
            }
            else if (GUI.Button(CalcGrid(1, 2), "Right"))
            {
                PlayGamesPlatform.Instance.SetGravityForPopups(Gravity.RIGHT);
                Status = "Popup will appear on right";
            }
            else if (GUI.Button(CalcGrid(0, 3), "Center horizontal"))
            {
                PlayGamesPlatform.Instance.SetGravityForPopups(Gravity.CENTER_HORIZONTAL);
                Status = "Popup will appear on center";
            }
            else if (GUI.Button(CalcGrid(1, 3), "Back"))
            {
                SetUI(Ui.Main);
                ShowEffect(true);
            }
        }

        private void ShowPermissionsUi()
        {
            DrawStatus();
            DrawTitle("Permissions Ui");

            if (GUI.Button(CalcGrid(0, 1), "Has Permission - Email"))
            {
                Status = "Email Permission " + PlayGamesPlatform.Instance.HasPermission("email");
            }
            else if (GUI.Button(CalcGrid(1, 1), "Request Permission- Email"))
            {
                Status = "Asking permission for email";
                PlayGamesPlatform.Instance.RequestPermission("email", code => { Status = "Result code " + code; });
            }
            else if (GUI.Button(CalcGrid(1, 6), "Back"))
            {
                SetUI(Ui.Main);
                ShowEffect(true);
            }
        }

        internal void ShowUserInfoUi()
        {
            GUI.Label(
                this.CalcGrid(0, 1, 2, 1),
                "User info for " + Social.localUser.userName);

            GUI.Label(
                this.CalcGrid(0, 2, 2, 1),
                "Email: " +
                ((PlayGamesLocalUser) Social.localUser).Email);

            GUI.Label(
                this.CalcGrid(0, 3, 2, 1),
                "ID Token: " + idToken);

            GUI.Label(
                this.CalcGrid(0, 4, 2, 1),
                "Server Auth Code: " + authCode
            );

            string friendString = "";

            GUI.Label(
                CalcGrid(0, 5, 2, 2),
                "Friends: " + friendString);

            if (statsMessage == string.Empty && Social.localUser.authenticated)
            {
                statsMessage = "loading stats....";
                ((PlayGamesLocalUser) Social.localUser).GetStats(
                    (result, stats) =>
                    {
                        statsMessage = result + " number of sessions: " +
                                       stats.NumberOfSessions;
                    });
            }

            GUI.Label(CalcGrid(0, 7, 2, 1), "Player Stats: " + statsMessage);


            if (GUI.Button(CalcGrid(0, 8), "Back"))
            {
                mUi = Ui.Main;
            }
            else if (GUI.Button(CalcGrid(1, 8), "New AuthCode"))
            {
                PlayGamesPlatform.Instance.GetAnotherServerAuthCode(false,
                    (newAuthCode) => this.authCode = newAuthCode);
            }
        }

        internal void DrawTitle(string title)
        {
            GUI.Label(
                this.CalcGrid(0, 0, 2, 1),
                title == null ? "Play Games Unity Plugin - Smoke Test" : title);
        }

        internal string Status
        {
            get { return _mStatus; }
            set { _mStatus = value; }
        }

        internal void DrawStatus()
        {
            GUI.Label(this.CalcGrid(0, 8, 2, 2), this.Status);
        }

        internal void ShowEffect(bool success)
        {
            Camera.main.backgroundColor =
                success ? new Color(0.0f, 0.0f, 0.8f, 1.0f) : new Color(0.8f, 0.0f, 0.0f, 1.0f);
        }

        internal int CalcFontSize()
        {
            return (int) (Screen.width * FontSizeFactor / 1000.0f);
        }

        // Update is called once per frame
        internal void OnGUI()
        {
            GUI.skin = GuiSkin;
            GUI.skin.label.fontSize = CalcFontSize();
            GUI.skin.button.fontSize = CalcFontSize();
            GUI.skin.textArea.fontSize = CalcFontSize();

            if (mStandby)
            {
                idToken = null;
                authCode = null;
                ShowStandbyUi();
            }
            else if (mUi == Ui.NearbyConnections)
            {
                mNearbyGui.OnGUI();
            }
            else if (Social.localUser.authenticated)
            {
                switch (mUi)
                {
                    case Ui.Achievements:
                        mAchievementGui.OnGUI();
                        break;
                    case Ui.Leaderboards:
                        mLeaderboardGui.OnGUI();
                        break;
                    case Ui.EditSavedGameName:
                        ShowEditSavedGameName();
                        break;
                    case Ui.Friends:
                        mFriendsGui.OnGUI();
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
                    case Ui.Events:
                        ShowEventsUi();
                        break;
                    case Ui.NearbyConnections:
                        mNearbyGui.OnGUI();
                        break;
                    case Ui.Video:
                        mVideoGui.OnGUI();
                        break;
                    case Ui.UserInfo:
                        // start loading the id token:
                        if (idToken == null)
                        {
                            idToken = ((PlayGamesLocalUser) Social.localUser).GetIdToken();
                        }

                        if (authCode == null)
                        {
                            authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                        }

                        ShowUserInfoUi();
                        break;
                    case Ui.PopupGravity:
                        ShowPopupGravityUi();
                        break;
                    case Ui.Permissions:
                        ShowPermissionsUi();
                        break;
                    default:
                        // check for a status of interest, and if there
                        // is one, then don't touch it.  Otherwise
                        // show the logged in user.
                        if (string.IsNullOrEmpty(Status) || Status == "Ready")
                        {
                            Status = "Authenticated. Hello, " +
                                     Social.localUser.userName + " (" +
                                     Social.localUser.id + ")";
                        }

                        ShowRegularUi();
                        break;
                }
            }
            else
            {
                ShowNotAuthUi();
            }
        }

        internal void SetStandBy(string message)
        {
            mStandby = true;
            mStandbyMessage = message;
        }

        internal void EndStandBy()
        {
            mStandby = false;
        }

        internal void DoAuthenticate(SignInInteractivity interactivity, PlayGamesClientConfiguration configuration)
        {
            SetStandBy("Authenticating...");
            ClientConfiguration = configuration;
            PlayGamesPlatform.InitializeInstance(ClientConfiguration);
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(interactivity, (code) =>
            {
                EndStandBy();
                if (code == SignInStatus.Success)
                {
                    Status = "Authenticated. Hello, " + Social.localUser.userName + " (" +
                             Social.localUser.id + ")";
                }
                else
                {
                    Status = "*** Failed to authenticate with " + code;
                }

                ShowEffect(code == SignInStatus.Success);
            });
        }

        internal void DoAuthenticate(PlayGamesClientConfiguration configuration)
        {
            DoAuthenticate(SignInInteractivity.CanPromptAlways, configuration);
        }

        internal void DoSignOut()
        {
            ((PlayGamesPlatform) Social.Active).SignOut();
            Status = "Signing out.";
        }


        internal char RandCharFrom(string s)
        {
            int i = UnityEngine.Random.Range(0, s.Length);
            i = i < 0 ? 0 : i >= s.Length ? s.Length - 1 : i;
            return s[i];
        }

        internal string GenString()
        {
            string x = string.Empty;
            int syl = UnityEngine.Random.Range(4, 7);
            while (x.Length < syl)
            {
                x += RandCharFrom("bcdfghjklmnpqrstvwxyz");
                x += RandCharFrom("aeiou");
                if (UnityEngine.Random.Range(0, 10) > 7)
                {
                    x += RandCharFrom("nsr");
                }
            }

            return x;
        }
    }
}
