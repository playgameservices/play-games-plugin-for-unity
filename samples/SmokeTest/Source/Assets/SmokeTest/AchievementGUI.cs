// <copyright file="AchievementGUI.cs" company="Google Inc.">
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
    using UnityEngine;
    using UnityEngine.SocialPlatforms;
    using GooglePlayGames;

    public class AchievementGUI : MonoBehaviour
    {

        private MainGui mOwner;
        private string mStatus;

        // Constructed by the main gui
        internal AchievementGUI(MainGui owner)
        {
            mOwner = owner;
            mStatus = "";
        }

        internal void OnGUI()
        {
            float height = Screen.height / 11f;
            GUILayout.BeginVertical(GUILayout.Height(Screen.height),GUILayout.Width(Screen.width));
            GUILayout.Label("SmokeTest: Achievements", GUILayout.Height(height));
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUILayout.Button("Ach Reveal", GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoAchievementReveal(GPGSIds.achievement_achievementtoreveal);
            }
            if (GUILayout.Button("Ach Reveal Incremental", GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoAchievementReveal(GPGSIds.achievement_achievement_hidden_incremental);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUILayout.Button("Ach Unlock",GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoAchievementUnlock();
            }
            if (GUILayout.Button("Ach Increment",GUILayout.ExpandHeight(true)))
            {
                DoAchievementIncrement(GPGSIds.achievement_achievementtoincrement);
            }
            if (GUILayout.Button("Ach Increment Hidden",GUILayout.ExpandHeight(true)))
            {
                DoAchievementIncrement(GPGSIds.achievement_achievement_hidden_incremental);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));

            if (GUILayout.Button("List Achievements",GUILayout.ExpandHeight(true)))
            {
                DoListAchievements();
            }
            if (GUILayout.Button("List Achievement Descriptions",GUILayout.ExpandHeight(true)))
            {
                DoListAchievementDescriptions();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));

            if (GUILayout.Button("Ach ShowUI",GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                DoAchievementUI();
            }
            if (GUILayout.Button("Back",GUILayout.ExpandHeight(true),
                GUILayout.Height(height),GUILayout.ExpandWidth(true)))
            {
                mOwner.SetUI(MainGui.Ui.Main);
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(mStatus);
            GUILayout.EndVertical();

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

        internal void DoAchievementReveal(string achId)
        {
            SetStandBy("Revealing achievement...");
            Social.ReportProgress(
                achId,
                0.0f,
                (bool success) =>
                {
                    EndStandBy();
                    mStatus = success ? "Revealed successfully." : "*** Failed to reveal ach.";
                    Debug.Log("AchievementToReveal completed: " + mStatus);
                    ShowEffect(success);
                });
        }

        internal void DoAchievementUnlock()
        {
            SetStandBy("Unlocking achievement...");
            Social.ReportProgress(
                GPGSIds.achievement_achievementtounlock,
                100.0f,
                (bool success) =>
                {
                    EndStandBy();
                    mStatus = success ? "Unlocked successfully." : "*** Failed to unlock ach.";
                    ShowEffect(success);
                });
        }

        internal void DoAchievementIncrement(string achId)
        {
            PlayGamesPlatform p = (PlayGamesPlatform)Social.Active;

            SetStandBy("Incrementing achievement...");
            p.IncrementAchievement(
                achId,
                1,
                (bool success) =>
                {
                    EndStandBy();
                    mStatus = success ? "Incremented successfully." : "*** Failed to increment ach.";
                    ShowEffect(success);
                });
        }

        internal void DoAchievementUI()
        {
            Social.ShowAchievementsUI();
            ShowEffect(true);
        }

        internal void DoListAchievements()
        {
            Social.LoadAchievements(achievements =>
                {
                    mStatus = "Loaded " + achievements.Length + " achievments";
                    foreach(IAchievement ach in achievements)
                    {
                        mStatus += "\n    " + ach.id + ": " + ach.completed;
                    }
                });
        }

        internal void DoListAchievementDescriptions()
        {
            Social.LoadAchievementDescriptions(achievements =>
                {
                    mStatus = "Loaded " + achievements.Length + " achievments";
                    foreach (IAchievementDescription ach in achievements)
                    {
                        mStatus += "\n    " + ach.id + ": " + ach.title;
                    }
                });
        }

    }
}
