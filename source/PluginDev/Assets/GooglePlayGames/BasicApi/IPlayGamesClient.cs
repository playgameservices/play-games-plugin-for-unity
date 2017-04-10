// <copyright file="IPlayGamesClient.cs" company="Google Inc.">
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
  using UnityEngine;
  using UnityEngine.SocialPlatforms;

  /// <summary>
  /// Defines an abstract interface for a Play Games Client.
  /// </summary>
  /// <remarks>Concrete implementations
  /// might be, for example, the client for Android or for iOS. One fundamental concept
  /// that implementors of this class must adhere to is stable authentication state.
  /// This means that once Authenticate() returns true through its callback, the user is
  /// considered to be forever after authenticated while the app is running. The implementation
  /// must make sure that this is the case -- for example, it must try to silently
  /// re-authenticate the user if authentication is lost or wait for the authentication
  /// process to get fixed if it is temporarily in a bad state (such as when the
  /// Activity in Android has just been brought to the foreground and the connection to
  /// the Games services hasn't yet been established). To the user of this
  /// interface, once the user is authenticated, they're forever authenticated.
  /// Unless, of course, there is an unusual permanent failure such as the underlying
  /// service dying, in which it's acceptable that API method calls will fail.
  ///
  /// <para>All methods can be called from the game thread. The user of this interface
  /// DOES NOT NEED to call them from the UI thread of the game. Transferring to the UI
  /// thread when necessary is a responsibility of the implementors of this interface.</para>
  ///
  /// <para>CALLBACKS: all callbacks must be invoked in Unity's main thread.
  /// Implementors of this interface must guarantee that (suggestion: use
  /// <see cref="GooglePlayGames.OurUtils.RunOnGameThread(System.Action action)"/>).</para>
  /// </remarks>
  public interface IPlayGamesClient
  {
    /// <summary>
    /// Starts the authentication process.
    /// </summary>
    /// <remarks>If silent == true, no UIs will be shown
    /// (if UIs are needed, it will fail rather than show them). If silent == false,
    /// this may show UIs, consent dialogs, etc.
    /// At the end of the process, callback will be invoked to notify of the result.
    /// Once the callback returns true, the user is considered to be authenticated
    /// forever after.
    /// </remarks>
    /// <param name="callback">Callback.</param>
    /// <param name="silent">If set to <c>true</c> silent.</param>
    void Authenticate(System.Action<bool, string> callback, bool silent);

    /// <summary>
    /// Returns whether or not user is authenticated.
    /// </summary>
    /// <returns><c>true</c> if the user is authenticated; otherwise, <c>false</c>.</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Signs the user out.
    /// </summary>
    void SignOut();

    /// <summary>Retrieves an OAuth 2.0 bearer token for the client.</summary>
    /// <returns>A string representing the bearer token.</returns>
    string GetToken();

    /// <summary>
    /// Returns the authenticated user's ID. Note that this value may change if a user signs
    /// on and signs in with a different account.
    /// </summary>
    /// <returns>The user's ID, <code>null</code> if the user is not logged in.</returns>
    string GetUserId();

    /// <summary>
    /// Load friends of the authenticated user.
    /// </summary>
    /// <param name="callback">Callback invoked when complete.  bool argument
    /// indicates success.</param>
    void LoadFriends(Action<bool> callback);

    /// <summary>
    /// Returns a human readable name for the user, if they are logged in.
    /// </summary>
    /// <returns>The user's human-readable name. <code>null</code> if they are not logged
    /// in</returns>
    string GetUserDisplayName();

    /// <summary>
    /// Returns an id token, which can be verified server side, if they are logged in.
    /// </summary>
    /// <param name="idTokenCallback"> A callback to be invoked after token is retrieved. Will be passed null value
    /// on failure. </param>
    void GetIdToken(Action<string> idTokenCallback);

    /// <summary>
    /// Gets an access token.
    /// </summary>
    /// <returns>An it token. <code>null</code> if they are not logged
    /// in</returns>
    string GetAccessToken();

    /// <summary>
    /// Asynchronously retrieves the server auth code for this client.
    /// </summary>
    /// <remarks>
    /// Note: This function is currently only implemented for Android.
    /// </remarks>
    /// <param name="serverClientId">The Client ID.</param>
    /// <param name="callback">Callback for response.</param>
    void GetServerAuthCode(string serverClientId, Action<CommonStatusCodes, string> callback);

    /// <summary>
    /// Gets the user's email.
    /// </summary>
    /// <remarks>The email address returned is selected by the user from the accounts present
    /// on the device.  There is no guarantee this uniquely identifies the player.
    /// For unique identification use the id property of the local player.
    /// The user can also choose to not select any email address, meaning it is not
    /// available.
    /// </remarks>
    /// <returns>The user email or null if not authenticated or the permission is
    /// not available.</returns>
    string GetUserEmail();

    /// <summary>
    /// Gets the user's email with a callback.
    /// </summary>
    /// <remarks>The email address returned is selected by the user from the accounts present
    /// on the device.  There is no guarantee this uniquely identifies the player.
    /// For unique identification use the id property of the local player.
    /// The user can also choose to not select any email address, meaning it is not
    /// available.
    /// </remarks>
    /// <param name="callback">The callback with a status code of the request,
    /// and string which is the email.  It can be null.</param>
    void GetUserEmail(Action<CommonStatusCodes, string> callback);

    /// <summary>
    /// Returns the user's avatar url, if they are logged in and have an avatar.
    /// </summary>
    /// <returns>The URL to load the avatar image. <code>null</code> if they are not logged
    /// in</returns>
    string GetUserImageUrl();

    /// <summary>Gets the player stats.</summary>
    /// <param name="callback">Callback for response.</param>
    void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback);

    /// <summary>
    /// Loads the users specified.  This is mainly used by the leaderboard
    /// APIs to get the information of a high scorer.
    /// </summary>
    /// <param name="userIds">User identifiers.</param>
    /// <param name="callback">Callback.</param>
    void LoadUsers(string[] userIds, Action<IUserProfile[]> callback);

    /// <summary>
    /// Returns the achievement corresponding to the passed achievement identifier.
    /// </summary>
    /// <returns>The achievement corresponding to the identifer. <code>null</code> if no such
    /// achievement is found or if authentication has not occurred.</returns>
    /// <param name="achievementId">The identifier of the achievement.</param>
    Achievement GetAchievement(string achievementId);

    /// <summary>
    /// Loads the achievements for the current signed in user and invokes
    /// the callback.
    /// </summary>
    void LoadAchievements(Action<Achievement[]> callback);

    /// <summary>
    /// Unlocks the achievement with the passed identifier.
    /// </summary>
    /// <remarks>If the operation succeeds, the callback
    /// will be invoked on the game thread with <code>true</code>. If the operation fails, the
    /// callback will be invoked with <code>false</code>. This operation will immediately fail if
    /// the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>). If the achievement is already unlocked, this call will
    /// succeed immediately.
    /// </remarks>
    /// <param name="achievementId">The ID of the achievement to unlock.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void UnlockAchievement(string achievementId, Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Reveals the achievement with the passed identifier.
    /// </summary>
    /// <remarks>If the operation succeeds, the callback
    /// will be invoked on the game thread with <code>true</code>. If the operation fails, the
    /// callback will be invoked with <code>false</code>. This operation will immediately fail if
    /// the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>). If the achievement is already in a revealed state, this call will
    /// succeed immediately.
    /// </remarks>
    /// <param name="achievementId">The ID of the achievement to reveal.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void RevealAchievement(string achievementId, Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Increments the achievement with the passed identifier.
    /// </summary>
    /// <remarks>If the operation succeeds, the
    /// callback will be invoked on the game thread with <code>true</code>. If the operation fails,
    /// the  callback will be invoked with <code>false</code>. This operation will immediately fail
    /// if the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>).
    /// </remarks>
    /// <param name="achievementId">The ID of the achievement to increment.</param>
    /// <param name="steps">The number of steps to increment by.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void IncrementAchievement(string achievementId, int steps,
                                  Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Set an achievement to have at least the given number of steps completed.
    /// </summary>
    /// <remarks>
    /// Calling this method while the achievement already has more steps than
    /// the provided value is a no-op. Once the achievement reaches the
    /// maximum number of steps, the achievement is automatically unlocked,
    /// and any further mutation operations are ignored.
    /// </remarks>
    /// <param name="achId">Ach identifier.</param>
    /// <param name="steps">Steps.</param>
    /// <param name="callback">Callback.</param>
    void SetStepsAtLeast(string achId, int steps, Action<bool> callback);

    /// <summary>
    /// Shows the appropriate platform-specific achievements UI.
    /// <param name="callback">The callback to invoke when complete.  If null,
    /// no callback is called. </param>
    /// </summary>
    void ShowAchievementsUI(Action<UIStatus> callback);

    /// <summary>
    /// Shows the leaderboard UI for a specific leaderboard.
    /// </summary>
    /// <remarks>If the passed ID is <code>null</code>, all leaderboards are displayed.
    /// </remarks>
    /// <param name="leaderboardId">The leaderboard to display. <code>null</code> to display
    /// all.</param>
    /// <param name="span">Timespan to display for the leaderboard</param>
    /// <param name="callback">If non-null, the callback to invoke when the
    /// leaderboard is dismissed.
    /// </param>
    void ShowLeaderboardUI(string leaderboardId,
            LeaderboardTimeSpan span,
            Action<UIStatus> callback);

    /// <summary>
    /// Loads the score data for the given leaderboard.
    /// </summary>
    /// <param name="leaderboardId">Leaderboard identifier.</param>
    /// <param name="start">Start indicating the top scores or player centric</param>
    /// <param name="rowCount">max number of scores to return. non-positive indicates
    // no rows should be returned.  This causes only the summary info to
    /// be loaded. This can be limited
    // by the SDK.</param>
    /// <param name="collection">leaderboard collection: public or social</param>
    /// <param name="timeSpan">leaderboard timespan</param>
    /// <param name="callback">callback with the scores, and a page token.
    ///   The token can be used to load next/prev pages.</param>
    void LoadScores(string leaderboardId, LeaderboardStart start,
                    int rowCount, LeaderboardCollection collection,
                    LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback);

    /// <summary>
    /// Loads the more scores for the leaderboard.
    /// </summary>
    /// <remarks>The token is accessed
    /// by calling LoadScores() with a positive row count.
    /// </remarks>
    /// <param name="token">Token for tracking the score loading.</param>
    /// <param name="rowCount">max number of scores to return.
    ///    This can be limited by the SDK.</param>
    /// <param name="callback">Callback.</param>
    void LoadMoreScores(ScorePageToken token, int rowCount,
            Action<LeaderboardScoreData> callback);

    /// <summary>
    /// Returns the max number of scores returned per call.
    /// </summary>
    /// <returns>The max results.</returns>
    int LeaderboardMaxResults();

    /// <summary>
    /// Submits the passed score to the passed leaderboard.
    /// </summary>
    /// <remarks>This operation will immediately fail
    /// if the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>).
    /// </remarks>
    /// <param name="leaderboardId">Leaderboard identifier.</param>
    /// <param name="score">Score.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void SubmitScore(string leaderboardId, long score,
            Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Submits the score for the currently signed-in player.
    /// </summary>
    /// <param name="score">Score.</param>
    /// <param name="board">leaderboard id.</param>
    /// <param name="metadata">metadata about the score.</param>
    /// <param name="callback">Callback upon completion.</param>
    void SubmitScore(string leaderboardId, long score, string metadata,
            Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Returns a real-time multiplayer client.
    /// </summary>
    /// <seealso cref="GooglePlayGames.Multiplayer.IRealTimeMultiplayerClient"/>
    /// <returns>The rtmp client.</returns>
    IRealTimeMultiplayerClient GetRtmpClient();

    /// <summary>
    /// Returns a turn-based multiplayer client.
    /// </summary>
    /// <returns>The tbmp client.</returns>
    ITurnBasedMultiplayerClient GetTbmpClient();

    /// <summary>
    /// Gets the saved game client.
    /// </summary>
    /// <returns>The saved game client.</returns>
    SavedGame.ISavedGameClient GetSavedGameClient();

    /// <summary>
    /// Gets the events client.
    /// </summary>
    /// <returns>The events client.</returns>
    Events.IEventsClient GetEventsClient();

    /// <summary>
    /// Gets the quests client.
    /// </summary>
    /// <returns>The quests client.</returns>
    Quests.IQuestsClient GetQuestsClient();

    /// <summary>
    /// Registers the invitation delegate.
    /// </summary>
    /// <param name="invitationDelegate">Invitation delegate.</param>
    void RegisterInvitationDelegate(InvitationReceivedDelegate invitationDelegate);

    IUserProfile[] GetFriends();

    /// <summary>
    /// Gets the Android API client.  Returns null on non-Android players.
    /// </summary>
    /// <returns>The API client.</returns>
    IntPtr GetApiClient();
  }

  /// <summary>
  /// Delegate that handles an incoming invitation (for both RTMP and TBMP).
  /// </summary>
  /// <param name="invitation">The invitation received.</param>
  /// <param name="shouldAutoAccept">If this is true, then the game should immediately
  /// accept the invitation and go to the game screen without prompting the user. If
  /// false, you should prompt the user before accepting the invitation. As an example,
  /// when a user taps on the "Accept" button on a notification in Android, it is
  /// clear that they want to accept that invitation right away, so the plugin calls this
  /// delegate with shouldAutoAccept = true. However, if we receive an incoming invitation
  /// that the player hasn't specifically indicated they wish to accept (for example,
  /// we received one in the background from the server), this delegate will be called
  /// with shouldAutoAccept=false to indicate that you should confirm with the user
  /// to see if they wish to accept or decline the invitation.</param>
    public delegate void InvitationReceivedDelegate(Invitation invitation, bool shouldAutoAccept);
}
#endif
