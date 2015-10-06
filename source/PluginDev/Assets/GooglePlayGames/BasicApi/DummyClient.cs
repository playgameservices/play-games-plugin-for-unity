// <copyright file="DummyClient.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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


namespace GooglePlayGames.BasicApi
{
    using System;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.OurUtils;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;

    public class DummyClient : IPlayGamesClient
    {
        public void Authenticate(System.Action<bool> callback, bool silent)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        public bool IsAuthenticated()
        {
            LogUsage();
            return false;
        }

        public void SignOut()
        {
            LogUsage();
        }

        public string GetAccessToken()
        {
            LogUsage();
            return "DummyAccessToken";
        }

        public string GetIdToken()
        {
            LogUsage();
            return "DummyIdToken";
        }

        public string GetUserId()
        {
            LogUsage();
            return "DummyID";
        }

        public string GetToken()
        {
            return "DummyToken";
        }

        public string GetUserEmail()
        {
            return string.Empty;
        }

        public void GetPlayerStats(
            Action<CommonStatusCodes, PlayGamesLocalUser.PlayerStats> callback)
        {
            LogUsage();
            callback(CommonStatusCodes.ApiNotConnected,
                    new PlayGamesLocalUser.PlayerStats());
        }

        public string GetUserDisplayName()
        {
            LogUsage();
            return "Player";
        }

        public string GetUserImageUrl()
        {
            LogUsage();
            return null;
        }

        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(null);
            }
        }

        public void LoadAchievements(Action<Achievement[]> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(null);
            }
        }

        public Achievement GetAchievement(string achId)
        {
            LogUsage();
            return null;
        }

        public void UnlockAchievement(string achId, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        public void RevealAchievement(string achId, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        public void ShowAchievementsUI(Action<UIStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(UIStatus.VersionUpdateRequired);
            }
        }

        public void ShowLeaderboardUI(string lbId, LeaderboardTimeSpan span,
            Action<UIStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(UIStatus.VersionUpdateRequired);
            }
        }

        public int LeaderboardMaxResults()
        {
            return 25;
        }

        public void LoadScores(string leaderboardId, LeaderboardStart start,
                           int rowCount, LeaderboardCollection collection,
                           LeaderboardTimeSpan timeSpan,
                           Action<LeaderboardScoreData> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(new LeaderboardScoreData(leaderboardId,
                        ResponseStatus.LicenseCheckFailed));
            }
        }

        public void LoadMoreScores(ScorePageToken token, int rowCount,
                               Action<LeaderboardScoreData> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(new LeaderboardScoreData(token.LeaderboardId,
                        ResponseStatus.LicenseCheckFailed));
            }
        }

        public void SubmitScore(string lbId, long score, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        public void SubmitScore(string lbId, long score, string metadata,
                            Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        public IRealTimeMultiplayerClient GetRtmpClient()
        {
            LogUsage();
            return null;
        }

        public ITurnBasedMultiplayerClient GetTbmpClient()
        {
            LogUsage();
            return null;
        }

        public SavedGame.ISavedGameClient GetSavedGameClient()
        {
            LogUsage();
            return null;
        }

        public GooglePlayGames.BasicApi.Events.IEventsClient GetEventsClient()
        {
            LogUsage();
            return null;
        }

        public GooglePlayGames.BasicApi.Quests.IQuestsClient GetQuestsClient()
        {
            LogUsage();
            return null;
        }

        public void RegisterInvitationDelegate(InvitationReceivedDelegate deleg)
        {
            LogUsage();
        }

        public Invitation GetInvitationFromNotification()
        {
            LogUsage();
            return null;
        }

        public bool HasInvitationFromNotification()
        {
            LogUsage();
            return false;
        }

        public void LoadFriends(Action<bool> callback)
        {
            LogUsage();
            callback(false);
        }

        public IUserProfile[] GetFriends()
        {
            LogUsage();
            return new IUserProfile[0];
        }

        public IntPtr GetApiClient()
        {
            LogUsage();
            return IntPtr.Zero;
        }

        private static void LogUsage()
        {
            Logger.d("Received method call on DummyClient - using stub implementation.");
        }
    }
}
