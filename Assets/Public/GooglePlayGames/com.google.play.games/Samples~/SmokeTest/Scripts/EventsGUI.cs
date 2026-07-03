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
    using System;
    using System.Linq;
    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using UnityEngine;

    public class EventsGUI : MonoBehaviour
    {
        private MainGui mOwner;
        private string mStatus;

        internal EventsGUI(MainGui owner)
        {
            mOwner = owner;
            mStatus = "";
        }

        internal void OnGUI()
        {   
            float height = Screen.height / 11f;
            GUILayout.BeginVertical(GUILayout.Height(Screen.height), GUILayout.Width(Screen.width));
            GUILayout.Label("SmokeTest: Events", GUILayout.Height(height));
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUI.Button(mOwner.CalcGrid(0, 1), "Fetch All Events"))
            {
                FetchAll();
            }
            else if (GUI.Button(mOwner.CalcGrid(1, 1), "Fetch Event"))
            {
                FetchOne(GPGSIds.event_smokingevent);   
            }
            else if (GUI.Button(mOwner.CalcGrid(0, 2), "Increment Event"))
            {
                PlayGamesPlatform.Instance.Events.IncrementEvent(
                    GPGSIds.event_smokingevent, 10);
            }

            if (GUI.Button(mOwner.CalcGrid(1, 6), "Back"))
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
         
        internal void FetchAll()
        {
            SetStandBy("Fetching All Events...");
            PlayGamesPlatform.Instance.Events.FetchAllEvents(
                DataSource.ReadNetworkOnly,
                (status, events) =>
                {
                    EndStandBy();
                    mStatus = "Fetch All Status: " + status + "\n";
                    mStatus += "Events: [" +
                                string.Join(",", events.Select(g => g.Id).ToArray()) + "]";
                    events.ForEach(e =>
                        GooglePlayGames.OurUtils.Logger.d("Retrieved event: " + e));
                });
        }

        internal void FetchOne(string eventId)
        {
            SetStandBy("Fetching Event");
            PlayGamesPlatform.Instance.Events.FetchEvent(
                DataSource.ReadNetworkOnly,
                eventId,
                (status, fetchedEvent) =>
                {
                    EndStandBy();
                    mStatus = "Fetch Status: " + status + "\n";
                    if (fetchedEvent != null)
                    {
                        mStatus += "Event: [" + fetchedEvent.Id + ", " + fetchedEvent.Description + "]: " +
                                    fetchedEvent.CurrentCount;
                        GooglePlayGames.OurUtils.Logger.d("Fetched event: " + fetchedEvent);
                    }

                });
        }
    }
}