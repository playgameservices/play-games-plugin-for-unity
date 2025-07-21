// <copyright file="FriendsGui.cs" company="Google Inc.">
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
    using System;
    using System.Linq;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using GooglePlayGames.OurUtils;

    public class FriendsGUI : MonoBehaviour
    {
        private MainGui mOwner;
        private FriendsListVisibilityStatus mFriendsListVisibilityStatus = FriendsListVisibilityStatus.Unknown;

        // Constructed by the main gui
        internal FriendsGUI(MainGui owner)
        {
            mOwner = owner;
        }

        internal void OnGUI()
        {
            float height = Screen.height / 11f;
            GUILayout.BeginVertical(GUILayout.Height(Screen.height), GUILayout.Width(Screen.width));
            GUILayout.Label("SmokeTest: Friends", GUILayout.Height(height));
            GUILayout.Label("Friend List Visibility Status: " + mFriendsListVisibilityStatus,
                GUILayout.Height(height));
            GUILayout.Label("Number of friends loaded: " + Social.localUser.friends.Length,
                GUILayout.Height(height));
            GUILayout.Label("Load Friends Status: " + PlayGamesPlatform.Instance.GetLastLoadFriendsStatus(),
                GUILayout.Height(height));
            string firstFriend = "";
            string firstFriendId= "";
            if (Social.localUser.friends.Length > 0)
            {
                firstFriend = Social.localUser.friends[0].userName;
                firstFriendId = Social.localUser.friends[0].id;
            }
            GUILayout.Label("First Friend: " + firstFriend,GUILayout.Height(height));
            GUILayout.BeginHorizontal(GUILayout.Height(height));

            if (GUILayout.Button("Back", GUILayout.ExpandHeight(true), GUILayout.Height(height),
                GUILayout.ExpandWidth(true)))
            {
                mOwner.SetUI(MainGui.Ui.Main);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));

            if (mFriendsListVisibilityStatus == FriendsListVisibilityStatus.Unknown)
            {
                PlayGamesPlatform.Instance.GetFriendsListVisibility( /* forceReload= */ true,
                    friendsListVisibilityStatus => { mFriendsListVisibilityStatus = friendsListVisibilityStatus; });
            }

            // Show friends paginated
            if (GUILayout.Button("Load Friends", GUILayout.ExpandHeight(true), GUILayout.Height(height),
                GUILayout.ExpandWidth(true)))
            {
                PlayGamesPlatform.Instance.LoadFriends(2, /* forceReload= */ false, /* callback= */ null);
            }

            if (GUILayout.Button("Load More Friends", GUILayout.ExpandHeight(true),
                GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                PlayGamesPlatform.Instance.LoadMoreFriends(2, /* callback= */ null);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));

            if (GUILayout.Button("Load All Friends", GUILayout.ExpandHeight(true),
                GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                Social.localUser.LoadFriends(/* callback= */ null);
            }

            if (GUILayout.Button("AskForLoadFriendsResolution", GUILayout.ExpandHeight(true),
                GUILayout.Height(height),
                GUILayout.ExpandWidth(true)))
            {
                PlayGamesPlatform.Instance.AskForLoadFriendsResolution(status =>
                {
                    // Will be updated next OnGui call
                    mFriendsListVisibilityStatus = FriendsListVisibilityStatus.Unknown;
                });
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (Social.localUser.friends.Length > 0)
            {
                if (GUILayout.Button("Show Profile: " + firstFriend, GUILayout.ExpandHeight(true),
                    GUILayout.Height(height),
                    GUILayout.ExpandWidth(true)))
                {
                    PlayGamesPlatform.Instance.ShowCompareProfileWithAlternativeNameHintsUI(
                        firstFriendId, /* otherPlayerInGameName= */ null, /* currentPlayerInGameName= */ null,
                        /* callback= */ null);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }
    }
}
