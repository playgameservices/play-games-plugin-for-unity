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
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi {
public class DummyClient : IPlayGamesClient {
    public void Authenticate(System.Action<bool> callback, bool silent) {
        LogUsage();
        if (callback != null) {
            callback.Invoke(false);
        }
    }

    public bool IsAuthenticated() {
        LogUsage();
        return false;
    }

    public void SignOut() {
        LogUsage();
    }

    public string GetUserId() {
        LogUsage();
        return "DummyID";
    }

    public string GetUserDisplayName() {
        LogUsage();
        return "Player";
    }

    public string GetUserImageUrl() {
        LogUsage();
        return null;
    }

    public List<Achievement> GetAchievements() {
        LogUsage();
        return new List<Achievement>();
    }

    public Achievement GetAchievement(string achId) {
        LogUsage();
        return null;
    }

    public void UnlockAchievement(string achId, Action<bool> callback) {
        LogUsage();
        if (callback != null) {
            callback.Invoke(false);
        }
    }

    public void RevealAchievement(string achId, Action<bool> callback) {
        LogUsage();
        if (callback != null) {
            callback.Invoke(false);
        }
    }

    public void IncrementAchievement(string achId, int steps, Action<bool> callback) {
        LogUsage();
        if (callback != null) {
            callback.Invoke(false);
        }
    }

    public void ShowAchievementsUI() {
        LogUsage();
    }

    public void ShowLeaderboardUI(string lbId) {
        LogUsage();
    }

    public void SubmitScore(string lbId, long score, Action<bool> callback) {
        LogUsage();
        if (callback != null) {
            callback.Invoke(false);
        }
    }

    public void LoadState(int slot, OnStateLoadedListener listener) {
        LogUsage();
        if (listener != null) {
            listener.OnStateLoaded(false, slot, null);
        }
    }

    public void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
        LogUsage();
    }

    public Multiplayer.IRealTimeMultiplayerClient GetRtmpClient() {
        LogUsage();
        return null;
    }

    public Multiplayer.ITurnBasedMultiplayerClient GetTbmpClient() {
        LogUsage();
        return null;
    }

    public SavedGame.ISavedGameClient GetSavedGameClient() {
        LogUsage();
        return null;
    }

    public void RegisterInvitationDelegate(InvitationReceivedDelegate deleg) {
        LogUsage();
    }

    public Invitation GetInvitationFromNotification() {
        LogUsage();
        return null;
    }

    public bool HasInvitationFromNotification() {
        LogUsage();
        return false;
    }

    private static void LogUsage() {
        Logger.d("Received method call on DummyClient - using stub implementation.");
    }
}
}

