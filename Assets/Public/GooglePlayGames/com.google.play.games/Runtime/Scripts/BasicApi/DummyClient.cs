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

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.OurUtils;
    using UnityEngine.SocialPlatforms;

    /// <summary>
    /// Dummy client used in Editor.
    /// </summary>
    /// <remarks>Google Play Game Services are not supported in the Editor
    /// environment, so this client is used as a placeholder.
    /// </remarks>
    public class DummyClient : IPlayGamesClient
    {
        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="callback">Callback to handle the sign-in status.</param>
        public void Authenticate(Action<SignInStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(SignInStatus.Canceled);
            }
        }

        /// <summary>
        /// Manually authenticates the user.
        /// </summary>
        /// <param name="callback">Callback to handle the sign-in status.</param>
        public void ManuallyAuthenticate(Action<SignInStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(SignInStatus.Canceled);
            }
        }

        /// <summary>
        /// Checks if the user is authenticated.
        /// </summary>
        /// <returns>Returns false indicating user is not authenticated.</returns>
        public bool IsAuthenticated()
        {
            LogUsage();
            return false;
        }

        /// <summary>
        /// Requests server-side access with a refresh token.
        /// </summary>
        /// <param name="forceRefreshToken">Flag to force refresh the token.</param>
        /// <param name="callback">Callback to handle the response.</param>
        public void RequestServerSideAccess(bool forceRefreshToken, Action<string> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(null);
            }
        }

        /// <summary>
        /// Requests server-side access with specific scopes.
        /// </summary>
        /// <param name="forceRefreshToken">Flag to force refresh the token.</param>
        /// <param name="scopes">List of requested authorization scopes.</param>
        /// <param name="callback">Callback to handle the response.</param>
        public void RequestServerSideAccess(bool forceRefreshToken, List<AuthScope> scopes, Action<AuthResponse> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(null);
            }
        }

        /// <summary>
        /// Requests recall of the access token.
        /// </summary>
        /// <param name="callback">Callback to handle the recall response.</param>
        public void RequestRecallAccessToken(Action<RecallAccess> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(null);
            }
        }

        /// <summary>
        /// Retrieves the user ID.
        /// </summary>
        /// <returns>Returns a dummy user ID.</returns>
        public string GetUserId()
        {
            LogUsage();
            return "DummyID";
        }

        /// <summary>
        /// Retrieves the player statistics.
        /// </summary>
        /// <param name="callback">Callback to handle the player stats response.</param>
        public void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            LogUsage();
            callback(CommonStatusCodes.ApiNotConnected, new PlayerStats());
        }

        /// <summary>
        /// Retrieves the user's display name.
        /// </summary>
        /// <returns>Returns a dummy display name.</returns>
        public string GetUserDisplayName()
        {
            LogUsage();
            return "Player";
        }

        /// <summary>
        /// Retrieves the user's image URL.
        /// </summary>
        /// <returns>Returns null since no image is available.</returns>
        public string GetUserImageUrl()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Loads user profiles for the given user IDs.
        /// </summary>
        /// <param name="userIds">List of user IDs.</param>
        /// <param name="callback">Callback to handle the user profile response.</param>
        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(null);
            }
        }

        /// <summary>
        /// Loads achievements for the current user.
        /// </summary>
        /// <param name="callback">Callback to handle the achievement response.</param>
        public void LoadAchievements(Action<Achievement[]> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(null);
            }
        }

        /// <summary>
        /// Unlocks the specified achievement.
        /// </summary>
        /// <param name="achId">The achievement ID to unlock.</param>
        /// <param name="callback">Callback to handle the unlock result.</param>
        public void UnlockAchievement(string achId, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Reveals the specified achievement.
        /// </summary>
        /// <param name="achId">The achievement ID to reveal.</param>
        /// <param name="callback">Callback to handle the reveal result.</param>
        public void RevealAchievement(string achId, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Increments the specified achievement by a number of steps.
        /// </summary>
        /// <param name="achId">The achievement ID to increment.</param>
        /// <param name="steps">The number of steps to increment the achievement.</param>
        /// <param name="callback">Callback to handle the increment result.</param>
        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Sets the steps of the specified achievement to at least a certain number.
        /// </summary>
        /// <param name="achId">The achievement ID to update.</param>
        /// <param name="steps">The number of steps to set.</param>
        /// <param name="callback">Callback to handle the result of setting the steps.</param>
        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Displays the achievements UI.
        /// </summary>
        /// <param name="callback">Callback to handle the UI status.</param>
        public void ShowAchievementsUI(Action<UIStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(UIStatus.VersionUpdateRequired);
            }
        }

        /// <summary>
        /// Requests the load friends resolution UI.
        /// </summary>
        /// <param name="callback">Callback to handle the UI status.</param>
        public void AskForLoadFriendsResolution(Action<UIStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(UIStatus.VersionUpdateRequired);
            }
        }

        /// <summary>
        /// Retrieves the last load friends status.
        /// </summary>
        /// <returns>Returns the last known load friends status.</returns>
        public LoadFriendsStatus GetLastLoadFriendsStatus()
        {
            LogUsage();
            return LoadFriendsStatus.Unknown;
        }

        /// <summary>
        /// Loads friends with paging options.
        /// </summary>
        /// <param name="pageSize">The number of friends to load per page.</param>
        /// <param name="forceReload">Flag to force reload of the friends list.</param>
        /// <param name="callback">Callback to handle the load friends status.</param>
        public void LoadFriends(int pageSize, bool forceReload,
                                Action<LoadFriendsStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(LoadFriendsStatus.Unknown);
            }
        }

        /// <summary>
        /// Loads additional friends if available.
        /// </summary>
        /// <param name="pageSize">The number of additional friends to load.</param>
        /// <param name="callback">Callback to handle the load friends status.</param>
        public void LoadMoreFriends(int pageSize, Action<LoadFriendsStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(LoadFriendsStatus.Unknown);
            }
        }

        /// <summary>
        /// Displays the compare profile UI for a player.
        /// </summary>
        /// <param name="userId">The user ID of the player to compare.</param>
        /// <param name="otherPlayerInGameName">The in-game name of the other player.</param>
        /// <param name="currentPlayerInGameName">The in-game name of the current player.</param>
        /// <param name="callback">Callback to handle the UI status.</param>
        public void ShowCompareProfileWithAlternativeNameHintsUI(string userId,
                                                                string otherPlayerInGameName,
                                                                string currentPlayerInGameName,
                                                                Action<UIStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(UIStatus.VersionUpdateRequired);
            }
        }

        /// <summary>
        /// Retrieves the visibility status of the friends list.
        /// </summary>
        /// <param name="forceReload">Flag to force reload the friends list visibility.</param>
        /// <param name="callback">Callback to handle the friends list visibility status.</param>
        public void GetFriendsListVisibility(bool forceReload,
                                            Action<FriendsListVisibilityStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(FriendsListVisibilityStatus.Unknown);
            }
        }

        /// <summary>
        /// Displays the leaderboard UI for a specific leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The ID of the leaderboard.</param>
        /// <param name="span">The time span for the leaderboard.</param>
        /// <param name="callback">Callback to handle the UI status.</param>
        public void ShowLeaderboardUI(
            string leaderboardId,
            LeaderboardTimeSpan span,
            Action<UIStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(UIStatus.VersionUpdateRequired);
            }
        }

        /// <summary>
        /// Retrieves the maximum number of leaderboard results that can be loaded.
        /// </summary>
        /// <returns>Returns the maximum number of leaderboard results.</returns>
        public int LeaderboardMaxResults()
        {
            return 25;
        }

        /// <summary>
        /// Loads the leaderboard scores based on the specified parameters.
        /// </summary>
        /// <param name="leaderboardId">The ID of the leaderboard to load scores from.</param>
        /// <param name="start">The start position for loading scores.</param>
        /// <param name="rowCount">The number of scores to load.</param>
        /// <param name="collection">The collection type (e.g., public or social).</param>
        /// <param name="timeSpan">The time span for the leaderboard scores.</param>
        /// <param name="callback">Callback to handle the leaderboard score data.</param>
        public void LoadScores(
            string leaderboardId,
            LeaderboardStart start,
            int rowCount,
            LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(new LeaderboardScoreData(
                    leaderboardId,
                    ResponseStatus.LicenseCheckFailed));
            }
        }

        /// <summary>
        /// Loads more leaderboard scores based on the provided pagination token.
        /// </summary>
        /// <param name="token">The token used for pagination.</param>
        /// <param name="rowCount">The number of scores to load.</param>
        /// <param name="callback">Callback to handle the leaderboard score data.</param>
        public void LoadMoreScores(
            ScorePageToken token,
            int rowCount,
            Action<LeaderboardScoreData> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback(new LeaderboardScoreData(
                    token.LeaderboardId,
                    ResponseStatus.LicenseCheckFailed));
            }
        }

        /// <summary>
        /// Submits a score to a specific leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The ID of the leaderboard.</param>
        /// <param name="score">The score to submit.</param>
        /// <param name="callback">Callback to handle the score submission result.</param>
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Submits a score with additional metadata to a specific leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The ID of the leaderboard.</param>
        /// <param name="score">The score to submit.</param>
        /// <param name="metadata">Additional metadata to submit with the score.</param>
        /// <param name="callback">Callback to handle the score submission result.</param>
        public void SubmitScore(
            string leaderboardId,
            long score,
            string metadata,
            Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Retrieves the saved game client.
        /// </summary>
        /// <returns>Returns null since no saved game client is available.</returns>
        public SavedGame.ISavedGameClient GetSavedGameClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Retrieves the events client.
        /// </summary>
        /// <returns>Returns null since no events client is available.</returns>
        public GooglePlayGames.BasicApi.Events.IEventsClient GetEventsClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Loads friends with a simple boolean flag indicating success or failure.
        /// </summary>
        /// <param name="callback">Callback to handle the load result.</param>
        public void LoadFriends(Action<bool> callback)
        {
            LogUsage();
            callback(false);
        }

        /// <summary>
        /// Retrieves the list of friends for the current user.
        /// </summary>
        /// <returns>Returns an empty array since no friends are loaded.</returns>
        public IUserProfile[] GetFriends()
        {
            LogUsage();
            return new IUserProfile[0];
        }

        /// <summary>
        /// Logs method usage for debugging purposes.
        /// </summary>
        private static void LogUsage()
        {
            Logger.d("Received method call on DummyClient - using stub implementation.");
        }

    }
}
#endif
