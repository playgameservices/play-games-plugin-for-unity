// <copyright file="PlayerGameEventsGUI.cs" company="Google Inc.">
// Copyright (C) 2026 Google Inc. All Rights Reserved.
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
    using GooglePlayGames.BasicApi;
    using System.Collections.Generic;

    public class PlayerGameEventsGUI : MonoBehaviour
    {
        private MainGui mOwner;
        private string mStatus;

        internal PlayerGameEventsGUI(MainGui owner)
        {
            mOwner = owner;
            mStatus = "PlayerGameEvents";
        }

        internal void OnGUI()
        {
            float height = Screen.height / 11f;
            GUILayout.BeginVertical(GUILayout.Height(Screen.height), GUILayout.Width(Screen.width));
            GUILayout.Label("SmokeTest: PlayerGameEvents", GUILayout.Height(height));

            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUILayout.Button("Record Single Event", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                RecordSingleEvent();
            }
            if (GUILayout.Button("Record Multiple Events", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                RecordMultipleEvents();
            }
            if (GUILayout.Button("Request Events Upload", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                RequestEventsUpload();
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

        internal void RecordSingleEvent()
        {
            SetStandBy("Recording single event...");
            PlayerGameEvent playerGameEvent = new PlayerGameEvent.Builder("test_event")
                .AddProperty("prop1", 123L)
                .Build();
            PlayGamesPlatform.Instance.RecordEvent(playerGameEvent);
            mStatus = "Called RecordEvent";
        }

        internal void RecordMultipleEvents()
        {
            SetStandBy("Recording multiple events...");
            List<PlayerGameEvent> events = new List<PlayerGameEvent>();
            events.Add(new PlayerGameEvent.Builder("test_event_multiple_1")
                .AddProperty("prop_str", "value1")
                .Build());
            events.Add(new PlayerGameEvent.Builder("test_event_multiple_2")
                .AddProperty("prop_bool", true)
                .Build());
            PlayGamesPlatform.Instance.RecordEvents(events);
            mStatus = "Called RecordEvents";
        }

        internal void RequestEventsUpload()
        {
            SetStandBy("Requesting events upload...");
            PlayGamesPlatform.Instance.RequestEventsUpload();
            mStatus = "Called RequestEventsUpload";
        }
    }
}
