// <copyright file="LeaderboardGUI.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
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

namespace SmokeTest
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class LeaderboardGUI : MonoBehaviour
    {

        private MainGui mOwner;
        private string mStatus;

        // Constructed by the main gui
        internal LeaderboardGUI(MainGui owner)
        {
            mOwner = owner;
            mStatus = "";
        }

        internal void OnGUI()
        {
            float height = Screen.height / 11f;
            GUILayout.BeginVertical(GUILayout.Height(Screen.height),GUILayout.Width(Screen.width));
            GUILayout.Label("SmokeTest: Leaderboards", GUILayout.Height(height));
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUILayout.Button("LB Show UI", GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoLeaderboardUI();
            }
            if (GUILayout.Button("Post Score", GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoPostScore();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUILayout.Button("Load Public Scores", GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoPublicLoadScores();
            }
            if (GUILayout.Button("Load Leaderboard", GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoLoadLeaderboard();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Social Scores", GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoSocialLoadScores();
            }
            if (GUILayout.Button("Back",GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                mOwner.SetUI(MainGui.Ui.Main);
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(mStatus);
            GUILayout.EndVertical();
        }

        internal long GenScore()
        {
            return (long)DateTime.Today.Subtract(new DateTime(2013, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        internal void DoPostScore()
        {
            long score = GenScore();
            SetStandBy("Posting score: " + score);
            Social.ReportScore(
                score,
                GPGSIds.leaderboard_leaders_in_smoketesting,
                (bool success) =>
                {
                    EndStandBy();
                    mStatus = success ? "Successfully reported score " + score :
                        "*** Failed to report score " + score;
                    ShowEffect(success);
                });
        }

        internal void DoLeaderboardUI()
        {
            Social.ShowLeaderboardUI();
            ShowEffect(true);
        }

        internal void DoSocialLoadScores()
        {
            PlayGamesPlatform.Instance.LoadScores(
                GPGSIds.leaderboard_leaders_in_smoketesting,
                LeaderboardStart.PlayerCentered,
                100,
                LeaderboardCollection.Social,
                LeaderboardTimeSpan.AllTime,
                (data) =>
                {
                    mStatus = "Leaderboard data valid: " + data.Valid;
                    mStatus += "\n approx:" +data.ApproximateCount + " have " + data.Scores.Length;
                });
        }

        internal void DoPublicLoadScores()
        {
            PlayGamesPlatform.Instance.LoadScores(
                GPGSIds.leaderboard_leaders_in_smoketesting,
                LeaderboardStart.PlayerCentered,
                100,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                (data) =>
                {
                    mStatus  = "LB data Status: " + data.Status;
                    mStatus += " valid: " + data.Valid;
                    mStatus += "\n approx:" +data.ApproximateCount + " have " + data.Scores.Length;
                });
        }

        internal void DoLoadLeaderboard()
        {
            ILeaderboard lb = PlayGamesPlatform.Instance.CreateLeaderboard();
            lb.userScope = UserScope.FriendsOnly;
            lb.id = GPGSIds.leaderboard_leaders_in_smoketesting;
            lb.LoadScores(ok =>
                {
                    if (ok)
                    {
                        LoadUsersAndDisplay(lb);
                    }
                    else
                    {
                        mStatus = "Leaderboard loading: " + lb.title + " ok = " + ok;
                    }
                });
        }

        internal void LoadUsersAndDisplay(ILeaderboard lb)
        {
            // get the use ids
            List<string> userIds = new List<string>();

            foreach(IScore score in lb.scores)
            {
                userIds.Add(score.userID);
            }
            Social.LoadUsers(userIds.ToArray(), (users) =>
                {
                    mStatus = "Leaderboard loading: " + lb.title + " count = " +
                        lb.scores.Length;
                    foreach(IScore score in lb.scores) {
                        IUserProfile user = FindUser(users, score.userID);
                        mStatus += "\n" + score.formattedValue + " by " +
                            (string)(
                                (user != null) ? user.userName : "**unk_" + score.userID + "**");
                    }
                });
        }

        private IUserProfile FindUser(IUserProfile[] users, string userid)
        {
            foreach (IUserProfile user in users)
            {
                if (user.id == userid)
                {
                    return user;
                }
            }
            return null;
        }

        void SetStandBy(string msg)
        {
            mStatus = msg;
        }

        void EndStandBy()
        {
            mStatus += " (Done!)";
        }

        internal void ShowEffect(bool success)
        {
            Camera.main.backgroundColor = success ?
                new Color(0.0f, 0.0f, 0.8f, 1.0f) :
                new Color(0.8f, 0.0f, 0.0f, 1.0f);
        }

    }

}
