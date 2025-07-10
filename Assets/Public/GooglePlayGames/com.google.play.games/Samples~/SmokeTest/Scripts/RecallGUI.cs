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

namespace SmokeTest
{
    using UnityEngine;
    using GooglePlayGames;

    public class RecallGUI : MonoBehaviour
    {
        private MainGui mOwner;
        private string mStatus;

        internal RecallGUI(MainGui owner)
        {
            mOwner = owner;
            mStatus = "";
        }

        internal void OnGUI()
        {
            float height = Screen.height / 11f;
            GUILayout.BeginVertical(GUILayout.Height(Screen.height), GUILayout.Width(Screen.width));
            GUILayout.Label("SmokeTest: Recall", GUILayout.Height(height));
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUILayout.Button("Get Recall Access Token", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                GetRecallAccessToken();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));

            if (GUILayout.Button("Back", GUILayout.ExpandHeight(true),
                GUILayout.Height(height), GUILayout.ExpandWidth(true)))
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
            Camera.main.backgroundColor =
                success ? new Color(0.0f, 0.0f, 0.8f, 1.0f) : new Color(0.8f, 0.0f, 0.0f, 1.0f);
        }

        internal void GetRecallAccessToken()
        {
            SetStandBy("Fetching recall access token......");
            PlayGamesPlatform.Instance.RequestRecallAccess(
                recallAccess => {
                    EndStandBy();
                    string recallSessionId = recallAccess.sessionId;
                    mStatus = "Recall Session ID for current session is: " + recallSessionId;
                    Debug.Log("Recall Token fetched successfully: " + mStatus);
                    ShowEffect(recallSessionId != null && recallSessionId != "");
                });
            return;
        }
    }
}