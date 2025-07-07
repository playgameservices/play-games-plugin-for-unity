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

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi
{
    using System;
    using System.Collections.Generic;
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
    /// <see cref="PlayGamesHelperObject.RunOnGameThread(System.Action)"/>).</para>
    /// </remarks>
    public interface IPlayGamesClient
    {
      /// <summary>
      /// Returns the result of the automatic sign-in attempt.
      /// </summary>
      /// <remarks>This returns the result
      /// </remarks>
      /// <param name="callback">Callback</param>
      void Authenticate(Action<SignInStatus> callback);

      /// <summary>
      /// Manually requests that your game performs sign in with Play Games Services.
      /// </summary>
      /// <remarks>
      /// Note that a sign-in attempt will be made automatically when your game's application
      /// started. For this reason most games will not need to manually request to perform sign-in
      /// unless the automatic sign-in attempt failed and your game requires access to Play Games
      /// Services.
      /// </remarks>
      /// <param name="callback"></param>
      void ManuallyAuthenticate(Action<SignInStatus> callback);

      /// <summary>
      /// Returns whether or not user is authenticated.
      /// </summary>
      /// <returns><c>true</c> if the user is authenticated; otherwise, <c>false</c>.</returns>
      bool IsAuthenticated();

      /// <summary>
      /// Requests server-side access to Player Games Services for the currently signed in player.
      /// </summary>
      /// When requested an authorization code is returned that can be used by your game-server to
      /// exchange for an access token and conditionally a refresh token (when <c>forceRefreshToken</c>
      /// is true). The access token may then be used by your game-server to access the Play Games
      /// Services web APIs. This is commonly used to complete a sign-in flow by verifying the Play Games
      /// Services player id.
      ///
      /// If <c>forceRefreshToken</c> is true, when exchanging the authorization code a refresh token
      /// will be returned in addition to the access token. The refresh token allows the game-server to
      /// request additional access tokens, allowing your game-server to continue accesses Play Games
      /// Services while the user is not actively playing your app.
      /// <param name="forceRefreshToken">If <c>true</c> when the returned authorization code is exchanged a
      /// refresh token will be included in addition to an access token.</param>
      /// <param name="callback"></param>
      void RequestServerSideAccess(bool forceRefreshToken, Action<string> callback);

      /// <summary>
      /// Requests server-side access to Play Games Services for the currently signed in player.
      /// </summary>
      /// An authorization code is returned when requested. Your server can then exchange this code
      /// for an access token (and conditionally a refresh token when <c>forceRefreshToken</c> is
      /// <c>true</c>). The access token allows your server to access the Play Games Services web APIs, which
      /// is often used to complete sign-in by verifying the Play Games Services player ID.
      ///
      /// When <c>forceRefreshToken</c> is <c>true</c> during authorization code exchange, a refresh
      /// token is provided along with the access token. This refresh token enables your server to obtain
      /// new access tokens and continue accessing Play Games Services even when the user isn't actively
      /// playing. Note that refresh tokens are only generated for players who have auto sign-in setting
      /// enabled.
      ///
      /// Scopes represent the {@link AuthScope} values requested such as <c>AuthScope.EMAIL</c>,
      /// <c>AuthScope.PROFILE</c>, <c>AuthScope.OPEN_ID</c>. For new permissions, users will see a
      /// consent screen upon the first request. Granting consent (or if permissions were already
      /// granted) results in the {@link AuthResponse} listing the effectively granted {@link AuthScope}.
      /// Declining permission results in an empty list of granted {@link AuthScope} in the {@link
      /// AuthResponse} . Regardless of granted permissions, a successful request will always return the
      /// authorization code.
      /// <param name="forceRefreshToken">If <c>true</c> when the returned authorization code is exchanged a
      /// refresh token will be included in addition to an access token.</param>
      ///<param name="scopes">A list of {@link AuthScope} values representing the OAuth 2.0 permissions being
      ///requested, such as <c>AuthScope.EMAIL</c>, <c>AuthScope.PROFILE</c> and
      /// <c>AuthScope.OPEN_ID</c>.</param>
      /// <param name="callback"></param>
      /// <return>A {@link Task} that completes with an {@link AuthResponse} containing the OAuth 2.0
      /// authorization code as a string and a list of the {@link AuthScope}s that were effectively
      /// granted by the user (see description for details on consent). This authorization code can
      /// be exchanged by your server for an access token (and conditionally a refresh token) that
      /// can be used to access the Play Games Services web APIs and other Google Identity APIs.</return>
      void RequestServerSideAccess(bool forceRefreshToken, List<AuthScope> scopes, Action<AuthResponse> callback);

      /// <summary>
      /// Requests Recall Access to Player Games Services for the currently signed in account
      /// </summary>
      /// When requested a session id is returned that can be used by your game-server to
      /// use Recall Access APIs like LinkPerson , UnlinkPersona and get Details about Recall Tokens
      /// and corresponding personas. See https://developer.android.com/games/pgs/recall?hl=en.
      ///
      /// <remarks>
      ///
      /// </remarks>
      /// <param name="callback"></param>
      void RequestRecallAccessToken(Action<RecallAccess> callback);

      /// <summary>
      /// Returns the authenticated user's ID. Note that this value may change if a user signs
      /// out and signs in with a different account.
      /// </summary>
      /// <returns>The user's ID, null if the user is not logged in.</returns>
      string GetUserId();

      /// <summary>
      /// Loads friends of the authenticated user. This loads the entire list of friends.
      /// </summary>
      /// <param name="callback">Callback invoked when complete.  bool argument
      /// indicates success.</param>
      void LoadFriends(Action<bool> callback);

      /// <summary>
      /// Returns a human readable name for the user, if they are logged in.
      /// </summary>
      /// <returns>The user's human-readable name. null if they are not logged
      /// in</returns>
      string GetUserDisplayName();

      /// <summary>
      /// Returns the user's avatar url, if they are logged in and have an avatar.
      /// </summary>
      /// <returns>The URL to load the avatar image. null if they are not logged
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
      /// Loads the achievements for the current signed in user and invokes
      /// the callback.
      /// </summary>
      void LoadAchievements(Action<Achievement[]> callback);

      /// <summary>
      /// Unlocks the achievement with the passed identifier.
      /// </summary>
      /// <remarks>If the operation succeeds, the callback
      /// will be invoked on the game thread with true. If the operation fails, the
      /// callback will be invoked with false. This operation will immediately fail if
      /// the user is not authenticated (i.e. the callback will immediately be invoked with
      /// false). If the achievement is already unlocked, this call will
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
      /// will be invoked on the game thread with true. If the operation fails, the
      /// callback will be invoked with false. This operation will immediately fail if
      /// the user is not authenticated (i.e. the callback will immediately be invoked with
      /// false). If the achievement is already in a revealed state, this call will
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
      /// callback will be invoked on the game thread with true. If the operation
      /// fails, the  callback will be invoked with false. This operation will
      /// immediately fail if the user is not authenticated (i.e. the callback will immediately be
      /// invoked with false).
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
      /// Shows the appropriate platform-specific friends sharing UI.
      /// <param name="callback">The callback to invoke when complete. If null,
      /// no callback is called. </param>
      /// </summary>
      void AskForLoadFriendsResolution(Action<UIStatus> callback);

      /// <summary>
      /// Returns the latest LoadFriendsStatus obtained from loading friends.
      /// </summary>
      LoadFriendsStatus GetLastLoadFriendsStatus();

      /// <summary>
      /// Shows the Play Games Player Profile UI for a specific user identifier.
      /// </summary>
      /// <param name="otherUserId">User Identifier.</param>
      /// <param name="otherPlayerInGameName">
      /// The game's own display name of the player referred to by userId.
      /// </param>
      /// <param name="currentPlayerInGameName">
      /// The game's own display name of the current player.
      /// </param>
      /// <param name="callback">Callback invoked upon completion.</param>
      void ShowCompareProfileWithAlternativeNameHintsUI(
          string otherUserId, string otherPlayerInGameName, string currentPlayerInGameName,
          Action<UIStatus> callback);

      /// <summary>
      /// Returns if the user has allowed permission for the game to access the friends list.
      /// </summary>
      /// <param name="forceReload">If true, this call will clear any locally cached data and
      /// attempt to fetch the latest data from the server. Normally, this should be set to
      /// <c>false</c> to gain advantages of data caching.</param> <param name="callback">Callback
      /// invoked upon completion.</param>
      void GetFriendsListVisibility(bool forceReload, Action<FriendsListVisibilityStatus> callback);

      /// <summary>
      /// Loads the first page of the user's friends
      /// </summary>
      /// <param name="pageSize">
      /// The number of entries to request for this initial page. Note that if cached
      /// data already exists, the returned buffer may contain more than this size, but it is
      /// guaranteed to contain at least this many if the collection contains enough records.
      /// </param>
      /// <param name="forceReload">
      /// If true, this call will clear any locally cached data and attempt to
      /// fetch the latest data from the server. This would commonly be used for something like a
      /// user-initiated refresh. Normally, this should be set to <c>false</c> to gain advantages
      /// of data caching.</param>
      /// <param name="callback">Callback invoked upon completion.</param>
      void LoadFriends(int pageSize, bool forceReload, Action<LoadFriendsStatus> callback);

      /// <summary>
      /// Loads the friends list page
      /// </summary>
      /// <param name="pageSize">
      /// The number of entries to request for this page. Note that if cached data already
      /// exists, the returned buffer may contain more than this size, but it is guaranteed
      /// to contain at least this many if the collection contains enough records.
      /// </param>
      /// <param name="callback"></param>
      void LoadMoreFriends(int pageSize, Action<LoadFriendsStatus> callback);

      /// <summary>
      /// Shows the leaderboard UI for a specific leaderboard.
      /// </summary>
      /// <remarks>If the passed ID is null, all leaderboards are displayed.
      /// </remarks>
      /// <param name="leaderboardId">The leaderboard to display. null to display
      /// all.</param>
      /// <param name="span">Timespan to display for the leaderboard</param>
      /// <param name="callback">If non-null, the callback to invoke when the
      /// leaderboard is dismissed.
      /// </param>
      void ShowLeaderboardUI(string leaderboardId, LeaderboardTimeSpan span,
                             Action<UIStatus> callback);

      /// <summary>
      /// Loads the score data for the given leaderboard.
      /// </summary>
      /// <param name="leaderboardId">Leaderboard identifier.</param>
      /// <param name="start">Start indicating the top scores or player centric</param>
      /// <param name="rowCount">max number of scores to return. non-positive indicates
      /// no rows should be returned.  This causes only the summary info to
      /// be loaded. This can be limited
      /// by the SDK.</param>
      /// <param name="collection">leaderboard collection: public or social</param>
      /// <param name="timeSpan">leaderboard timespan</param>
      /// <param name="callback">callback with the scores, and a page token.
      ///   The token can be used to load next/prev pages.</param>
      void LoadScores(string leaderboardId, LeaderboardStart start, int rowCount,
                      LeaderboardCollection collection, LeaderboardTimeSpan timeSpan,
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
      /// false).
      /// </remarks>
      /// <param name="leaderboardId">Leaderboard identifier.</param>
      /// <param name="score">Score.</param>
      /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
      /// succeeded or failed.</param>
      void SubmitScore(string leaderboardId, long score, Action<bool> successOrFailureCalllback);

      /// <summary>
      /// Submits the score for the currently signed-in player.
      /// </summary>
      /// <param name="score">Score.</param>
      /// <param name="leaderboardId">leaderboard id.</param>
      /// <param name="metadata">metadata about the score.</param>
      /// <param name="successOrFailureCalllback">Callback upon completion.</param>
      void SubmitScore(string leaderboardId, long score, string metadata,
                       Action<bool> successOrFailureCalllback);

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

      IUserProfile[] GetFriends();
    }
}
#endif
