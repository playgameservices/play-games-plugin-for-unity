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
using GooglePlayGames.BasicApi.Events;
using GooglePlayGames.BasicApi.Video;
using GooglePlayGames.BasicApi.SavedGame;

namespace GooglePlayGames.UnitTests {
    using UnityEngine.SocialPlatforms;

class BaseMockPlayGamesClient : IPlayGamesClient {

    internal bool Authenticated { get; set; }

    internal BaseMockPlayGamesClient() {
        Authenticated = true;
    }

    public virtual void Authenticate(bool silent, Action<SignInStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public bool IsAuthenticated() {
        return Authenticated;
    }

    public virtual void SignOut() {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadFriends(Action<bool> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadFriends(int pageSize, bool forceReload,
        Action<LoadFriendsStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual string GetIdToken() {
        throw new NotSupportedException("unsupported");
    }

    public virtual string GetServerAuthCode() {
        throw new NotSupportedException("unsupported");
    }

    public virtual string GetUserEmail() {
        throw new NotSupportedException("unsupported");
    }

    public virtual void GetAnotherServerAuthCode(bool reAuthenticateIfNeeded,
        Action<string> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadUsers(string[] userIds, Action<IUserProfile[]> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadAchievements(Action<Achievement[]> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void SetStepsAtLeast(string achId, int steps, Action<bool> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void ShowAchievementsUI(Action<UIStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void AskForLoadFriendsResolution(Action<UIStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual LoadFriendsStatus GetLastLoadFriendsStatus() {
        throw new NotSupportedException("unsupported");
    }

    public virtual void ShowCompareProfileWithAlternativeNameHintsUI(
        string otherUserId, string otherPlayerInGameName, string currentPlayerInGameName,
        Action<UIStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void GetFriendsListVisibility(bool forceReload,
             Action<FriendsListVisibilityStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadMoreFriends(int pageSize, Action<LoadFriendsStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual int LeaderboardMaxResults() {
        throw new NotSupportedException("unsupported");
    }

    public virtual void SubmitScore(string leaderboardId, long score, string metadata,
            Action<bool> successOrFailureCalllback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void RequestPermissions(string[] scopes, Action<SignInStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual bool HasPermissions(string[] scopes) {
        throw new NotSupportedException("unsupported");
    }

    public virtual IUserProfile[] GetFriends() {
        throw new NotSupportedException("unsupported");
    }

    public virtual IEventsClient GetEventsClient() {
        throw new NotSupportedException("unsupported");
    }

    public virtual IVideoClient GetVideoClient() {
        throw new NotSupportedException("unsupported");
    }

    public virtual void SetGravityForPopups(Gravity gravity) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void ShowLeaderboardUI(string leaderboardId,
            LeaderboardTimeSpan span,
            Action<UIStatus> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadScores(string leaderboardId, LeaderboardStart start,
            int rowCount, LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback) {
        throw new NotSupportedException("unsupported");
    }

    public virtual void LoadMoreScores(ScorePageToken token, int rowCount,
            Action<LeaderboardScoreData> callback) {
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

    public BasicApi.SavedGame.ISavedGameClient GetSavedGameClient() {
        throw new NotSupportedException("unsupported");
    }

    static void NotSupported() {
        throw new NotSupportedException("unsupported");
    }
}
}

