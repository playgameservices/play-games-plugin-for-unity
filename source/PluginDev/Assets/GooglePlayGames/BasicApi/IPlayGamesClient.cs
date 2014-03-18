/*
 * Copyright (C) 2013 Google Inc.
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
    /**
     * Defines an abstract interface for a Play Games Client. Concrete implementations
     * might be, for example, the client for Android or for iOS. One fundamental concept
     * that implementors of this class must adhere to is stable authentication state.
     * This means that once Authenticate() returns true through its callback, the user is
     * considered to be forever after authenticated while the app is running. The implementation
     * must make sure that this is the case -- for example, it must try to silently
     * re-authenticate the user if authentication is lost or wait for the authentication
     * process to get fixed if it is temporarily in a bad state (such as when the
     * Activity in Android has just been brought to the foreground and the connection to
     * the Games services hasn't yet been established). To the user of this
     * interface, once the user is authenticated, they're forever authenticated.
     * Unless, of course, there is an unusual permanent failure such as the underlying
     * service dying, in which it's acceptable that API method calls will fail.
     *
     * All methods can be called from the game thread. The user of this interface
     * DOES NOT NEED to call them from the UI thread of the game. Transferring to the UI
     * thread when necessary is a responsibility of the implementors of this interface.
     *
     * CALLBACKS: all callbacks must be invoked in Unity's main thread.
     * Implementors of this interface must guarantee that (suggestion: use
     * GameThreadQueueRunner).
     */
    public interface IPlayGamesClient {
        // Starts the authentication process. If silent == true, no UIs will be shown
        // (if UIs are needed, it will fail rather than show them). If silent == false,
        // this may show UIs, consent dialogs, etc.
        // At the end of the process, callback will be invoked to notify of the result.
        // Once the callback returns true, the user is considered to be authenticated
        // forever after.
        void Authenticate(System.Action<bool> callback, bool silent);
        
        // Returns whether or not user is authenticated
        bool IsAuthenticated();

        // Signs out
        void SignOut();

        // Returns the authenticated user's ID
        string GetUserId();

        // Returns the authenticated user's display name
        string GetUserDisplayName();

        // Return a specific achievement
        Achievement GetAchievement(string achId);

        // Unlock achievement
        void UnlockAchievement(string achId, Action<bool> callback);

        // Reveal achievement
        void RevealAchievement(string achId, Action<bool> callback);

        // Increment achievement
        void IncrementAchievement(string achId, int steps, Action<bool> callback);

        // Show achievements UI
        void ShowAchievementsUI();

        // Show leaderboards UI (if lbId == null, show all leaderboards)
        void ShowLeaderboardUI(string lbId);

        // Report a score to given leaderboard
        void SubmitScore(string lbId, long score, Action<bool> callback);

        // Set the buffer encrypter/decrypter used when saving cloud data to local storage.
        // This is only used in platforms where local storage of cloud data is not
        // implemented by the underlying library (currently, only iOS).
        void SetCloudCacheEncrypter(BufferEncrypter encrypter);

        // Load state from the cloud
        void LoadState(int slot, OnStateLoadedListener listener);

        // Save state to cloud
        void UpdateState(int slot, byte[] data, OnStateLoadedListener listener);
        
        // Return a real-time multiplayer client
        Multiplayer.IRealTimeMultiplayerClient GetRtmpClient();
        
        // Return a turn-based multiplayer client
        Multiplayer.ITurnBasedMultiplayerClient GetTbmpClient();
        
        // Register an invitation delegate for RTMP/TBMP invitations
        void RegisterInvitationDelegate(InvitationReceivedDelegate deleg);
    }

    // Delegate that encrypts or decrypts a buffer for local storage.
    public delegate byte[] BufferEncrypter(bool encrypt, byte[] data);
    
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

