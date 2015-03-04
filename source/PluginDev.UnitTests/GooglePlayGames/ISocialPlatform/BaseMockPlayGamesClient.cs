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
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.BasicApi.SavedGame;

namespace GooglePlayGames.UnitTests {

class BaseMockPlayGamesClient : IPlayGamesClient {

    internal bool Authenticated { get; set; }

    internal BaseMockPlayGamesClient() {
        Authenticated = true;
    }

    public virtual void Authenticate(System.Action<bool> callback, bool silent) {
        throw new NotSupportedException("unsupported");
    }

    public bool IsAuthenticated() {
        return Authenticated;
    }

    public virtual void SignOut() {
        throw new NotSupportedException("unsupported");
    }

    public virtual string GetUserId() {
        throw new NotSupportedException("unsupported");
    }

    public virtual string GetUserDisplayName() {
        throw new NotSupportedException("unsupported");
    }

    public string GetUserImageUrl() {
        throw new NotImplementedException("unsupported");
    }

    public List<Achievement> GetAchievements() {
        throw new NotSupportedException("unsupported");
    }

    public virtual Achievement GetAchievement(string achId) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void UnlockAchievement(string achId, Action<bool> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void RevealAchievement(string achId, Action<bool> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void IncrementAchievement(string achId, int steps, Action<bool> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void ShowAchievementsUI() {
        throw new NotSupportedException("unsupported");
    }

    public virtual void ShowLeaderboardUI(string lbId) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void SubmitScore(string lbId, long score, Action<bool> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadState(int slot, OnStateLoadedListener listener) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
        throw new NotSupportedException("unsupported");
    }

    public BasicApi.Multiplayer.IRealTimeMultiplayerClient GetRtmpClient() {
        throw new NotSupportedException("unsupported");
    }

    public BasicApi.Multiplayer.ITurnBasedMultiplayerClient GetTbmpClient() {
        throw new NotSupportedException("unsupported");
    }

    public BasicApi.SavedGame.ISavedGameClient GetSavedGameClient() {
        throw new NotSupportedException("unsupported");
    }

    public void RegisterInvitationDelegate(InvitationReceivedDelegate deleg) {
        throw new NotSupportedException("unsupported");
    }

    public Invitation GetInvitationFromNotification() {
        throw new NotSupportedException("unsupported");
    }

    public bool HasInvitationFromNotification() {
        throw new NotSupportedException("unsupported");
    }

    static void NotSupported() {
        throw new NotSupportedException("unsupported");
    }
}
}

