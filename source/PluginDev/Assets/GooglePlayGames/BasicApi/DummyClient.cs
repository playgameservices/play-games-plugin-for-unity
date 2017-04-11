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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.BasicApi
{
    using System;
    using GooglePlayGames.BasicApi.Multiplayer;
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
        /// Starts the authentication process.
        /// </summary>
        /// <remarks> If silent == true, no UIs will be shown
        /// (if UIs are needed, it will fail rather than show them). If silent == false,
        /// this may show UIs, consent dialogs, etc.
        /// At the end of the process, callback will be invoked to notify of the result.
        /// Once the callback returns true, the user is considered to be authenticated
        /// forever after.
        /// </remarks>
        /// <param name="callback">Callback when completed.</param>
        /// <param name="silent">If set to <c>true</c> silent.</param>
        public void Authenticate(Action<bool, string> callback, bool silent)
        {
            LogUsage();
            if (callback != null)
            {
                callback(false, "Not implemented on this platform");
            }
        }

        /// <summary>
        /// Returns whether or not user is authenticated.
        /// </summary>
        /// <returns>true if authenticated</returns>
        /// <c>false</c>
        public bool IsAuthenticated()
        {
            LogUsage();
            return false;
        }

        /// <summary>
        /// Signs the user out.
        /// </summary>
        public void SignOut()
        {
            LogUsage();
        }


        /// <summary>
        /// Retrieves an id token, which can be verified server side, if they are logged in.
        /// </summary>
        /// <param name="idTokenCallback">The callback invoked with the token</param>
        /// <returns>The identifier token.</returns>
        public string GetIdToken()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Returns the authenticated user's ID. Note that this value may change if a user signs
        /// on and signs in with a different account.
        /// </summary>
        /// <returns>The user identifier.</returns>
        public string GetUserId()
        {
            LogUsage();
            return "DummyID";
        }


        public string GetServerAuthCode()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Gets the user's email.
        /// </summary>
        /// <remarks>The email address returned is selected by the user from the accounts present
        /// on the device. There is no guarantee this uniquely identifies the player.
        /// For unique identification use the id property of the local player.
        /// The user can also choose to not select any email address, meaning it is not
        /// available.</remarks>
        /// <returns>The user email or null if not authenticated or the permission is
        /// not available.</returns>
        public string GetUserEmail()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the player stats.
        /// </summary>
        /// <param name="callback">Callback for response.</param>
        public void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            LogUsage();
            callback(CommonStatusCodes.ApiNotConnected, new PlayerStats());
        }

        /// <summary>
        /// Returns a human readable name for the user, if they are logged in.
        /// </summary>
        /// <returns>The user display name.</returns>
        public string GetUserDisplayName()
        {
            LogUsage();
            return "Player";
        }

        /// <summary>
        /// Returns the user's avatar url, if they are logged in and have an avatar.
        /// </summary>
        /// <returns>The user image URL.</returns>
        public string GetUserImageUrl()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Loads the players specified.
        /// </summary>
        /// <remarks> This is mainly used by the leaderboard
        /// APIs to get the information of a high scorer.
        /// </remarks>
        /// <param name="userIds">User identifiers.</param>
        /// <param name="callback">Callback to invoke when completed.</param>
        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(null);
            }
        }

        /// <summary>
        /// Loads the achievements for the current player.
        /// </summary>
        /// <param name="callback">Callback to invoke when completed.</param>
        public void LoadAchievements(Action<Achievement[]> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(null);
            }
        }

        /// <summary>
        /// Returns the achievement corresponding to the passed achievement identifier.
        /// </summary>
        /// <returns>The achievement.</returns>
        /// <param name="achId">Achievement identifier.</param>
        public Achievement GetAchievement(string achId)
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Unlocks the achievement.
        /// </summary>
        /// <param name="achId">Achievement identifier.</param>
        /// <param name="callback">Callback to invoke when complete.</param>
        public void UnlockAchievement(string achId, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Reveals the achievement.
        /// </summary>
        /// <param name="achId">Achievement identifier.</param>
        /// <param name="callback">Callback to invoke when complete.</param>
        public void RevealAchievement(string achId, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Increments the achievement.
        /// </summary>
        /// <param name="achId">Achievement identifier.</param>
        /// <param name="steps">Steps to increment by..</param>
        /// <param name="callback">Callback to invoke when complete.</param>
        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Set an achievement to have at least the given number of steps completed.
        /// </summary>
        /// <remarks>
        /// Calling this method while the achievement already has more steps than
        /// the provided value is a no-op. Once the achievement reaches the
        /// maximum number of steps, the achievement is automatically unlocked,
        /// and any further mutation operations are ignored.
        /// </remarks>
        /// <param name="achId">Achievement identifier.</param>
        /// <param name="steps">Steps to increment to at least.</param>
        /// <param name="callback">Callback to invoke when complete.</param>
        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Shows the achievements UI
        /// </summary>
        /// <param name="callback">Callback to invoke when complete.</param>
        public void ShowAchievementsUI(Action<UIStatus> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(UIStatus.VersionUpdateRequired);
            }
        }

        /// <summary>
        /// Shows the leaderboard UI
        /// </summary>
        /// <param name="leaderboardId">Leaderboard identifier.</param>
        /// <param name="span">Timespan to display.</param>
        /// <param name="callback">Callback to invoke when complete.</param>
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
        /// Returns the max number of scores returned per call.
        /// </summary>
        /// <returns>The max results.</returns>
        public int LeaderboardMaxResults()
        {
            return 25;
        }

        /// <summary>
        /// Loads the score data for the given leaderboard.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard identifier.</param>
        /// <param name="start">Start indicating the top scores or player centric</param>
        /// <param name="rowCount">Row count.</param>
        /// <param name="collection">Collection to display.</param>
        /// <param name="timeSpan">Time span.</param>
        /// <param name="callback">Callback to invoke when complete.</param>
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
        /// Loads the more scores for the leaderboard.
        /// </summary>
        /// <remarks>The token is accessed
        /// by calling LoadScores() with a positive row count.
        /// </remarks>
        /// <param name="token">Token used to start loading scores.</param>
        /// <param name="rowCount">Max number of scores to return.
        ///  This can be limited by the SDK.</param>
        /// <param name="callback">Callback to invoke when complete.</param>
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
        /// Submits the score.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard identifier.</param>
        /// <param name="score">Score to submit.</param>
        /// <param name="callback">Callback to invoke when complete.</param>
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
        {
            LogUsage();
            if (callback != null)
            {
                callback.Invoke(false);
            }
        }

        /// <summary>
        /// Submits the score for the currently signed-in player
        /// to the leaderboard associated with a specific id
        /// and metadata (such as something the player did to earn the score).
        /// </summary>
        /// <param name="leaderboardId">Leaderboard identifier.</param>
        /// <param name="score">Score value to submit.</param>
        /// <param name="metadata">Metadata about the score.</param>
        /// <param name="callback">Callback upon completion.</param>
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
        /// Returns a real-time multiplayer client.
        /// </summary>
        /// <seealso cref="GooglePlayGames.Multiplayer.IRealTimeMultiplayerClient"></seealso>
        /// <returns>The rtmp client.</returns>
        public IRealTimeMultiplayerClient GetRtmpClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Returns a turn-based multiplayer client.
        /// </summary>
        /// <returns>The tbmp client.</returns>
        public ITurnBasedMultiplayerClient GetTbmpClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Gets the saved game client.
        /// </summary>
        /// <returns>The saved game client.</returns>
        public SavedGame.ISavedGameClient GetSavedGameClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Gets the events client.
        /// </summary>
        /// <returns>The events client.</returns>
        public GooglePlayGames.BasicApi.Events.IEventsClient GetEventsClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Gets the quests client.
        /// </summary>
        /// <returns>The quests client.</returns>
        public GooglePlayGames.BasicApi.Quests.IQuestsClient GetQuestsClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Gets the video client.
        /// </summary>
        /// <returns>The video client.</returns>
        public GooglePlayGames.BasicApi.Video.IVideoClient GetVideoClient()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Registers the invitation delegate.
        /// </summary>
        /// <param name="invitationDelegate">Invitation delegate.</param>
        public void RegisterInvitationDelegate(InvitationReceivedDelegate invitationDelegate)
        {
            LogUsage();
        }

        /// <summary>
        /// Gets the invitation from notification.
        /// </summary>
        /// <returns>The invitation from notification.</returns>
        public Invitation GetInvitationFromNotification()
        {
            LogUsage();
            return null;
        }

        /// <summary>
        /// Determines whether this instance has invitation from notification.
        /// </summary>
        /// <returns><c>true</c> if this instance has invitation from notification; otherwise, <c>false</c>.</returns>
        public bool HasInvitationFromNotification()
        {
            LogUsage();
            return false;
        }

        /// <summary>
        /// Load friends of the authenticated user
        /// </summary>
        /// <param name="callback">Callback invoked when complete. bool argument
        /// indicates success.</param>
        public void LoadFriends(Action<bool> callback)
        {
            LogUsage();
            callback(false);
        }

        /// <summary>
        /// Gets the friends.
        /// </summary>
        /// <returns>The friends.</returns>
        public IUserProfile[] GetFriends()
        {
            LogUsage();
            return new IUserProfile[0];
        }

        /// <summary>
        /// Gets the Android API client. Returns null on non-Android players.
        /// </summary>
        /// <returns>The API client.</returns>
        public IntPtr GetApiClient()
        {
            LogUsage();
            return IntPtr.Zero;
        }

        /// <summary>
        /// Logs the usage.
        /// </summary>
        private static void LogUsage()
        {
            Logger.d("Received method call on DummyClient - using stub implementation.");
        }
    }
}
#endif
