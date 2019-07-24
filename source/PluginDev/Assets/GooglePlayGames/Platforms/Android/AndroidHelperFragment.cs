// <copyright file="AndroidTokenClient.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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
//  limitations under the License.
// </copyright>

#if UNITY_ANDROID
namespace GooglePlayGames.Android
{
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.BasicApi.SavedGame;
    using OurUtils;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    internal class AndroidHelperFragment
    {
        private const string HelperFragmentClass = "com.google.games.bridge.HelperFragment";

        public static AndroidJavaObject GetActivity()
        {
            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        public static AndroidJavaObject GetDefaultPopupView()
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var activity = AndroidHelperFragment.GetActivity())
            {
                return helperFragment.CallStatic<AndroidJavaObject>("getDecorView", activity);
            }
        }

        public static void ShowAchievementsUI(Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task =
                helperFragment.CallStatic<AndroidJavaObject>("showAchievementUi", AndroidHelperFragment.GetActivity()))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(
                    task,
                    uiCode =>
                    {
                        Debug.Log("ShowAchievementsUI result " + uiCode);
                        cb.Invoke((UIStatus) uiCode);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("ShowAchievementsUI failed with exception");
                        cb.Invoke(UIStatus.InternalError);
                    });
            }
        }

        public static void ShowCaptureOverlayUI()
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            {
                helperFragment.CallStatic("showCaptureOverlayUi", AndroidHelperFragment.GetActivity());
            }
        }

        public static void ShowAllLeaderboardsUI(Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task =
                helperFragment.CallStatic<AndroidJavaObject>("showAllLeaderboardsUi",
                    AndroidHelperFragment.GetActivity()))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(
                    task,
                    uiCode =>
                    {
                        Debug.Log("ShowAllLeaderboardsUI result " + uiCode);
                        cb.Invoke((UIStatus) uiCode);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("ShowAllLeaderboardsUI failed with exception");
                        cb.Invoke(UIStatus.InternalError);
                    });
            }
        }

        public static void ShowLeaderboardUI(string leaderboardId, LeaderboardTimeSpan timeSpan, Action<UIStatus> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>("showLeaderboardUi",
                AndroidHelperFragment.GetActivity(), leaderboardId,
                AndroidJavaConverter.ToLeaderboardVariantTimeSpan(timeSpan)))
            {
                AndroidTaskUtils.AddOnSuccessListener<int>(
                    task,
                    uiCode =>
                    {
                        Debug.Log("ShowLeaderboardUI result " + uiCode);
                        cb.Invoke((UIStatus) uiCode);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("ShowLeaderboardUI failed with exception");
                        cb.Invoke(UIStatus.InternalError);
                    });
            }
        }

        public static void ShowSelectSnapshotUI(bool showCreateSaveUI, bool showDeleteSaveUI,
            int maxDisplayedSavedGames, string uiTitle, Action<SelectUIStatus, ISavedGameMetadata> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>("showSelectSnapshotUi",
                AndroidHelperFragment.GetActivity(), uiTitle, showCreateSaveUI, showDeleteSaveUI,
                maxDisplayedSavedGames))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    result =>
                    {
                        SelectUIStatus status = (SelectUIStatus) result.Get<int>("status");
                        Debug.Log("ShowSelectSnapshotUI result " + status);

                        AndroidJavaObject javaMetadata = result.Get<AndroidJavaObject>("metadata");
                        AndroidSnapshotMetadata metadata =
                            javaMetadata == null
                                ? null
                                : new AndroidSnapshotMetadata(javaMetadata, /* contents= */null);

                        cb.Invoke(status, metadata);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("ShowSelectSnapshotUI failed with exception");
                        cb.Invoke(SelectUIStatus.InternalError, null);
                    });
            }
        }

        public static void ShowRtmpSelectOpponentsUI(uint minOpponents, uint maxOpponents,
            Action<UIStatus, InvitationResultHolder> cb)
        {
            ShowSelectOpponentsUI(minOpponents, maxOpponents, /* isRealTime= */ true, cb);
        }

        public static void ShowTbmpSelectOpponentsUI(uint minOpponents, uint maxOpponents,
            Action<UIStatus, InvitationResultHolder> cb)
        {
            ShowSelectOpponentsUI(minOpponents, maxOpponents, /* isRealTime= */ false, cb);
        }

        private static void ShowSelectOpponentsUI(uint minOpponents, uint maxOpponents, bool isRealTime,
            Action<UIStatus, InvitationResultHolder> cb)
        {
            string methodName = isRealTime ? "showRtmpSelectOpponentsUi" : "showTbmpSelectOpponentsUi";
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>(methodName,
                AndroidHelperFragment.GetActivity(), (int) minOpponents, (int) maxOpponents))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    result =>
                    {
                        int status = result.Get<int>("status");
                        if ((UIStatus) status != UIStatus.Valid)
                        {
                            cb.Invoke((UIStatus) status, null);
                            return;
                        }

                        List<string> playerIdsToInvite;
                        using (var ids = result.Get<AndroidJavaObject>("playerIdsToInvite"))
                        {
                            playerIdsToInvite = CreatePlayerIdsToInvite(ids);
                        }

                        InvitationResultHolder resultHolder = new InvitationResultHolder(
                            result.Get<int>("minAutomatchingPlayers"),
                            result.Get<int>("maxAutomatchingPlayers"),
                            playerIdsToInvite
                        );

                        cb.Invoke((UIStatus) status, resultHolder);
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("showSelectOpponentsUi failed with exception");
                        cb.Invoke(UIStatus.InternalError, null);
                    });
            }
        }

        public enum WaitingRoomUIStatus
        {
            Valid = 1,
            Cancelled = 2,
            LeftRoom = 3,
            InvalidRoom = 4,
            Busy = -1,
            InternalError = -2,
        }

        public static void ShowWaitingRoomUI(AndroidJavaObject room, int minParticipantsToStart,
            Action<WaitingRoomUIStatus, AndroidJavaObject> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>("showWaitingRoomUI",
                AndroidHelperFragment.GetActivity(), room, minParticipantsToStart))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    result =>
                    {
                        cb.Invoke((WaitingRoomUIStatus) result.Get<int>("status"),
                            result.Get<AndroidJavaObject>("room"));
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("ShowWaitingRoomUI failed with exception");
                        cb.Invoke(WaitingRoomUIStatus.InternalError, null);
                    });
            }
        }

        public static void ShowInboxUI(Action<UIStatus, TurnBasedMatch> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>("showInboxUi",
                AndroidHelperFragment.GetActivity()))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    result =>
                    {
                        int status = result.Get<int>("status");
                        if ((UIStatus) status != UIStatus.Valid)
                        {
                            cb.Invoke((UIStatus) status, null);
                            return;
                        }

                        using (var turnBasedMatch = result.Get<AndroidJavaObject>("turnBasedMatch"))
                        {
                            cb.Invoke((UIStatus) status, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch));
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("showInboxUi failed with exception");
                        cb.Invoke(UIStatus.InternalError, null);
                    });
            }
        }

        public static void ShowInvitationInboxUI(Action<UIStatus, Invitation> cb)
        {
            using (var helperFragment = new AndroidJavaClass(HelperFragmentClass))
            using (var task = helperFragment.CallStatic<AndroidJavaObject>("showInvitationInboxUI",
                AndroidHelperFragment.GetActivity()))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    result =>
                    {
                        int status = result.Get<int>("status");
                        if ((UIStatus) status != UIStatus.Valid)
                        {
                            cb.Invoke((UIStatus) status, null);
                            return;
                        }

                        using (var invitation = result.Get<AndroidJavaObject>("invitation"))
                        {
                            cb.Invoke((UIStatus) status, AndroidJavaConverter.ToInvitation(invitation));
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception =>
                    {
                        Debug.Log("ShowInvitationInboxUI failed with exception");
                        cb.Invoke(UIStatus.InternalError, null);
                    });
            }
        }

        private static List<string> CreatePlayerIdsToInvite(AndroidJavaObject playerIdsObject)
        {
            int size = playerIdsObject.Call<int>("size");
            List<string> playerIdsToInvite = new List<string>();
            for (int i = 0; i < size; i++)
            {
                playerIdsToInvite.Add(playerIdsObject.Call<string>("get", i));
            }

            return playerIdsToInvite;
        }

        public class InvitationResultHolder
        {
            public int MinAutomatchingPlayers;
            public int MaxAutomatchingPlayers;
            public List<string> PlayerIdsToInvite;

            public InvitationResultHolder(int MinAutomatchingPlayers, int MaxAutomatchingPlayers,
                List<string> PlayerIdsToInvite)
            {
                this.MinAutomatchingPlayers = MinAutomatchingPlayers;
                this.MaxAutomatchingPlayers = MaxAutomatchingPlayers;
                this.PlayerIdsToInvite = PlayerIdsToInvite;
            }
        }
    }
}
#endif