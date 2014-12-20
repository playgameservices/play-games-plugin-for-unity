/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Collections;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.BasicApi {
/// <summary>
/// Defines an abstract interface for a Play Games Client. Concrete implementations
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
/// </summary>
public interface IPlayGamesClient {
    /// <summary>
    /// Starts the authentication process. If silent == true, no UIs will be shown
    /// (if UIs are needed, it will fail rather than show them). If silent == false,
    /// this may show UIs, consent dialogs, etc.
    /// At the end of the process, callback will be invoked to notify of the result.
    /// Once the callback returns true, the user is considered to be authenticated
    /// forever after.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="silent">If set to <c>true</c> silent.</param>
    void Authenticate(System.Action<bool> callback, bool silent);

    /// <summary>
    /// Returns whether or not user is authenticated.
    /// </summary>
    /// <returns><c>true</c> if the user is authenticated; otherwise, <c>false</c>.</returns>
    bool IsAuthenticated();

    /// <summary>
    /// Signs the user out.
    /// </summary>
    void SignOut();

    /// <summary>
    /// Returns the authenticated user's ID. Note that this value may change if a user signs
    /// on and signs in with a different account.
    /// </summary>
    /// <returns>The user's ID, <code>null</code> if the user is not logged in.</returns>
    string GetUserId();

    /// <summary>
    /// Returns a human readable name for the user, if they are logged in.
    /// </summary>
    /// <returns>The user's human-readable name. <code>null</code> if they are not logged
    /// in</returns>
    string GetUserDisplayName();

    /// <summary>
    /// Returns the user's avatar url, if they are logged in and have an avatar.
    /// </summary>
    /// <returns>The URL to load the avatar image. <code>null</code> if they are not logged
    /// in</returns>
    string GetUserImageUrl();

    /// <summary>
    /// Returns the achievement corresponding to the passed achievement identifier.
    /// </summary>
    /// <returns>The achievement corresponding to the identifer. <code>null</code> if no such
    /// achievement is found or if authentication has not occurred.</returns>
    /// <param name="achievementId">The identifier of the achievement.</param>
    Achievement GetAchievement(string achievementId);

    /// <summary>
    /// Unlocks the achievement with the passed identifier. If the operation succeeds, the callback
    /// will be invoked on the game thread with <code>true</code>. If the operation fails, the
    /// callback will be invoked with <code>false</code>. This operation will immediately fail if
    /// the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>). If the achievement is already unlocked, this call will
    /// succeed immediately.
    /// </summary>
    /// <param name="achievementId">The ID of the achievement to unlock.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void UnlockAchievement(string achievementId, Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Reveals the achievement with the passed identifier. If the operation succeeds, the callback
    /// will be invoked on the game thread with <code>true</code>. If the operation fails, the
    /// callback will be invoked with <code>false</code>. This operation will immediately fail if
    /// the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>). If the achievement is already in a revealed state, this call will
    /// succeed immediately.
    /// </summary>
    /// <param name="achievementId">The ID of the achievement to reveal.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void RevealAchievement(string achievementId, Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Increments the achievement with the passed identifier. If the operation succeeds, the
    /// callback will be invoked on the game thread with <code>true</code>. If the operation fails,
    /// the  callback will be invoked with <code>false</code>. This operation will immediately fail
    /// if the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>).
    /// </summary>
    /// <param name="achievementId">The ID of the achievement to increment.</param>
    /// <param name="steps">The number of steps to increment by.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void IncrementAchievement(string achievementId, int steps,
                              Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Shows the appropriate platform-specific achievements UI.
    /// </summary>
    void ShowAchievementsUI();

    /// <summary>
    /// Shows the leaderboard UI for a specific leaderboard (if the passed ID is not
    /// <code>null</code>) or for all leaderboards (if the ID is <code>null</code>).
    /// </summary>
    /// <param name="leaderboardId">The leaderboard to display. <code>null</code> to display
    /// all.</param>
    void ShowLeaderboardUI(string leaderboardId);

    /// <summary>
    /// Submits the passed score to the passed leaderboard. This operation will immediately fail
    /// if the user is not authenticated (i.e. the callback will immediately be invoked with
    /// <code>false</code>).
    /// </summary>
    /// <param name="leaderboardId">Leaderboard identifier.</param>
    /// <param name="score">Score.</param>
    /// <param name="successOrFailureCalllback">Callback used to indicate whether the operation
    /// succeeded or failed.</param>
    void SubmitScore(string leaderboardId, long score, Action<bool> successOrFailureCalllback);

    /// <summary>
    /// Loads state from the cloud for the passed slot.
    /// </summary>
    /// <param name="slot">The slot to read from.</param>
    /// <param name="listener">The listener to use to report results and resolve possible
    /// state conflicts.</param>
    void LoadState(int slot, OnStateLoadedListener listener);

    /// <summary>
    /// Updates state in the passed slot to the passed data.
    /// </summary>
    /// <param name="slot">The slot to read from.</param>
    /// <param name="listener">The listener to use to report results and resolve possible
    /// state conflicts.</param>
    void UpdateState(int slot, byte[] data, OnStateLoadedListener listener);

    /// <summary>
    /// Returns a real-time multiplayer client.
    /// </summary>
    /// <seealso cref="GooglePlayGames.Multiplayer.IRealTimeMultiplayerClient"/>
    /// <returns>The rtmp client.</returns>
    Multiplayer.IRealTimeMultiplayerClient GetRtmpClient();

    /// <summary>
    /// Returns a turn-based multiplayer client.
    /// </summary>
    /// <returns>The tbmp client.</returns>
    Multiplayer.ITurnBasedMultiplayerClient GetTbmpClient();

    /// <summary>
    /// Gets the saved game client.
    /// </summary>
    /// <returns>The saved game client.</returns>
    SavedGame.ISavedGameClient GetSavedGameClient();

    /// <summary>
    /// Registers the invitation delegate.
    /// </summary>
    /// <param name="invitationDelegate">Invitation delegate.</param>
    void RegisterInvitationDelegate(InvitationReceivedDelegate invitationDelegate);
}

    /// <summary>
    /// Delegate that handles an incoming invitation (for both RTMP and TBMP).
    /// </summary>
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

