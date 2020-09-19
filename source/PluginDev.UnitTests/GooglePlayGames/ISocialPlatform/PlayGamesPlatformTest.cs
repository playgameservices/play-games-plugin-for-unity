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
using NUnit.Framework;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.UnitTests {
    [TestFixture]
    public class PlayGamesPlatformTest {

        private static readonly Action<bool> SentinelCallback = ignored => {};

        [SetUp]
        public void SetUp() {
            // Ensure that our internal logger is disabled
            Logger.WarningLogEnabled = false;
            Logger.DebugLogEnabled = false;
        }

        // Achievement tests
        class AchievementClient : BaseMockPlayGamesClient {
            public string RevealedId { get; set; }
            public Action<bool> RevealedCallback { get; set; }

            public string UnlockedId { get; set; }
            public Action<bool> UnlockedCallback { get; set; }

            public string IncrementedId { get; set; }
            public int? NewSteps { get; set; }
            public Action<bool> IncrementedCallback { get; set; }

            public Achievement CurrentAchievement { get; set; }

            public int ShownUiCount { get; set; }

            public override void RevealAchievement(string achId, Action<bool> callback) {
                RevealedId = achId;
                RevealedCallback = callback;
            }

            public override void UnlockAchievement(string achId, Action<bool> callback) {
                UnlockedId = achId;
                UnlockedCallback = callback;
            }

            public override void SetStepsAtLeast(string achId, int steps, Action<bool> callback) {
                IncrementedId = achId;
                NewSteps = steps;
                IncrementedCallback = callback;
            }

            public override void ShowAchievementsUI(Action<UIStatus> callback) {
                ShownUiCount++;
            }

            public override void LoadAchievements(Action<Achievement[]> callback) {
                Achievement[] achievements;
                if (CurrentAchievement == null) {
                    achievements = new Achievement[0];
                } else {
                    achievements = new Achievement[1];
                    achievements[0] = CurrentAchievement;
                }

                callback.Invoke(achievements);
            }
        }

        [Test]
        public void AchievementReportProgressFailsWhenNotAuthenticated() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);
            mockClient.Authenticated = false;
            var result = new CapturingAction<bool>();

            platform.ReportProgress("achievement", 0.0, result.invoke);

            Assert.IsFalse(result.Captured);
        }

        [Test]
        public void AchievementReportProgressIsRevealNoMapping() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.ReportProgress("achievement", 0.00000001, SentinelCallback);

            Assert.AreEqual("achievement", mockClient.RevealedId);
        }

        [Test]
        public void AchievementReportProgressIsRevealWithMapping() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);
            platform.AddIdMapping("mappedId", "realId");
            Achievement achievemnt = new Achievement();
            achievemnt.Id = "realId";
            mockClient.CurrentAchievement = achievemnt;

            platform.ReportProgress("mappedId", 0.00000001, SentinelCallback);

            Assert.AreEqual("realId", mockClient.RevealedId);
        }

        [Test]
        public void AchievementReportProgressNonIncremental() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            Achievement nonIncremental = new Achievement();
            nonIncremental.Id = "nonIncremental";
            mockClient.CurrentAchievement = nonIncremental;

            platform.ReportProgress("nonIncremental", 50, SentinelCallback);

            Assert.IsNull(mockClient.UnlockedId);
            Assert.IsNull(mockClient.UnlockedCallback);
        }

        [Test]
        public void AchievementReportProgressUnknownAchievementTreatedAsNonIncremental() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.ReportProgress("unknown", 50, SentinelCallback);

            Assert.IsNull(mockClient.UnlockedId);
            Assert.IsNull(mockClient.UnlockedCallback);
        }

        [Test]
        public void AchievementReportProgressIncrementalAllInOneGo() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            IncrementViaReportProgress(mockClient, platform, 0, 100, 100);

            Assert.AreEqual("incremental", mockClient.IncrementedId);
            Assert.AreEqual(100, mockClient.NewSteps.Value);
        }

        [Test]
        public void AchievementReportProgressIncrementalProgressSmallAmount() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            IncrementViaReportProgress(mockClient, platform, 0, 100, 25);

            Assert.AreEqual("incremental", mockClient.IncrementedId);
            Assert.AreEqual(25, mockClient.NewSteps.Value);
        }

        [Test]
        public void AchievementReportProgressIncrementalBuildsOnInitialProgress() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            IncrementViaReportProgress(mockClient, platform, 25, 100, 60);

            Assert.AreEqual("incremental", mockClient.IncrementedId);
            Assert.AreEqual(60, mockClient.NewSteps.Value);
        }

        [Test]
        public void AchievementReportProgressIncrementalAllowsMoreThanOneHundredPercent() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            IncrementViaReportProgress(mockClient, platform, 0, 100, 200);

            Assert.AreEqual("incremental", mockClient.IncrementedId);
            Assert.AreEqual(200, mockClient.NewSteps.Value);
        }

        [Test]
        public void AchievementIncrementRequiresAuthentication() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);
            var capturingCallback = new CapturingAction<bool>();

            mockClient.Authenticated = false;

            platform.IncrementAchievement("noAuth", 10, capturingCallback.invoke);

            Assert.IsFalse(capturingCallback.Captured);
        }

        [Test]
        public void AchievementIncrementForUnmappedId() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.SetStepsAtLeast("unmapped", 20, SentinelCallback);

            Assert.AreEqual("unmapped", mockClient.IncrementedId);
            Assert.AreEqual(20, mockClient.NewSteps);
            Assert.AreSame(SentinelCallback, mockClient.IncrementedCallback);
        }

        [Test]
        public void AchievementIncrementForMappedId() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);
            platform.AddIdMapping("unmapped", "mapped");

            platform.SetStepsAtLeast("unmapped", 30, SentinelCallback);

            Assert.AreEqual("mapped", mockClient.IncrementedId);
            Assert.AreEqual(30, mockClient.NewSteps);
            Assert.AreSame(SentinelCallback, mockClient.IncrementedCallback);
        }

        [Test]
        public void ShowAchievementsUiWorksWhenAuthenticated() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.ShowAchievementsUI();

            Assert.AreEqual(1, mockClient.ShownUiCount);
        }

        [Test]
        public void ShowAchievementsUiIsNoOpWhenUnauthenticated() {
            var mockClient = new AchievementClient();
            var platform = new PlayGamesPlatform(mockClient);

            mockClient.Authenticated = false;

            platform.ShowAchievementsUI();

            Assert.AreEqual(0, mockClient.ShownUiCount);
        }

        static void IncrementViaReportProgress(AchievementClient mockClient, PlayGamesPlatform platform,
                                       int current, int total, double progress) {
            Achievement incremental = new Achievement();
            incremental.Id = "incremental";
            incremental.IsIncremental = true;
            incremental.CurrentSteps = current;
            incremental.TotalSteps = total;
            mockClient.CurrentAchievement = incremental;
            platform.ReportProgress("incremental", progress, SentinelCallback);
        }

        // Leaderboard tests
        class LeaderboardClient : BaseMockPlayGamesClient {
            public string SubmittedId { get; set; }
            public long? SubmittedScore { get; set; }
            public Action<bool> SubmitCallback { get; set; }

            public bool UIShown { get; set; }
            public string ShownId { get; set; }

            public override void SubmitScore(string leaderboardId, long score, Action<bool> callback) {
                SubmittedId = leaderboardId;
                SubmittedScore = score;
                SubmitCallback = callback;
            }

            public override void ShowLeaderboardUI(string leaderboardId,
                LeaderboardTimeSpan span,
                Action<UIStatus> callback) {
                UIShown = true;
                ShownId = leaderboardId;
            }
        }

        [Test]
        public void ReportScoreIsNoOpWhenUnauthenticated() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);
            var capturingCallback = new CapturingAction<bool>();

            mockClient.Authenticated = false;

            platform.ReportScore(100, "leaderboard", capturingCallback.invoke);
            Assert.IsNull(mockClient.SubmittedId);
            Assert.IsNull(mockClient.SubmittedScore);
            Assert.IsFalse(capturingCallback.Captured);
        }

        [Test]
        public void ReportScoreIsWorksWhenAuthenticated() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.ReportScore(200L, "leaderboard", SentinelCallback);
            Assert.AreEqual("leaderboard", mockClient.SubmittedId);
            Assert.AreEqual(200L, mockClient.SubmittedScore);
            Assert.AreSame(SentinelCallback, mockClient.SubmitCallback);
        }

        [Test]
        public void ReportScoreIsWorksWhenIdMapped() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.AddIdMapping("unmappedLeaderboard", "mappedLeaderboard");

            platform.ReportScore(300L, "unmappedLeaderboard", SentinelCallback);

            Assert.AreEqual("mappedLeaderboard", mockClient.SubmittedId);
            Assert.AreEqual(300L, mockClient.SubmittedScore);
            Assert.AreSame(SentinelCallback, mockClient.SubmitCallback);
        }

        [Test]
        public void ShowLeaderboardUINoOpWhenUnauthenticated() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);

            mockClient.Authenticated = false;

            platform.ShowLeaderboardUI();

            Assert.IsFalse(mockClient.UIShown);
        }

        [Test]
        public void ShowLeaderboardUIWorksWhenAuthenticatedAndNoDefaultLeaderboard() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.ShowLeaderboardUI();

            Assert.IsTrue(mockClient.UIShown);
            Assert.IsNull(mockClient.ShownId);
        }

        [Test]
        public void ShowLeaderboardUIWorksWhenAuthenticatedAndUnmappedDefaultLeaderboard() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);
            platform.SetDefaultLeaderboardForUI("default");

            platform.ShowLeaderboardUI();

            Assert.IsTrue(mockClient.UIShown);
            Assert.AreEqual("default", mockClient.ShownId);
        }

        [Test]
        public void ShowLeaderboardUIWorksWhenAuthenticatedAndMappedDefaultLeaderboard() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);
            platform.AddIdMapping("unmappedDefault", "mappedDefault");
            platform.SetDefaultLeaderboardForUI("unmappedDefault");

            platform.ShowLeaderboardUI();

            Assert.IsTrue(mockClient.UIShown);
            Assert.AreEqual("mappedDefault", mockClient.ShownId);
        }

        [Test]
        public void ShowLeaderboardUIWithIdIsNoOpWhenUnauthenticated() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);
            mockClient.Authenticated = false;

            platform.ShowLeaderboardUI("notShown");
            Assert.IsFalse(mockClient.UIShown);
            Assert.IsNull(mockClient.ShownId);
        }

        [Test]
        public void ShowLeaderboardUIWithIdIsWorksWhenAuthenticated() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.ShowLeaderboardUI("leaderboard");

            Assert.IsTrue(mockClient.UIShown);
            Assert.AreEqual("leaderboard", mockClient.ShownId);
        }

        [Test]
        public void ShowLeaderboardUIWithIdIsWorksWhenMapped() {
            var mockClient = new LeaderboardClient();
            var platform = new PlayGamesPlatform(mockClient);
            platform.AddIdMapping("unmappedLeaderboard", "mappedLeaderboard");

            platform.ShowLeaderboardUI("unmappedLeaderboard");

            Assert.IsTrue(mockClient.UIShown);
            Assert.AreEqual("mappedLeaderboard", mockClient.ShownId);
        }

        // Authentication tests
        class LoginClient : BaseMockPlayGamesClient {
            public bool? AuthenticatedSilently { get; set; }
            public Action<SignInStatus> AuthenticationCallback { get; set; }

            public String UserId { get; set; }
            public String UserDisplayName { get; set; }

            public int SignOutCount { get; set; }

            public override void Authenticate(bool silent, Action<SignInStatus> callback) {
                AuthenticatedSilently = silent;
                AuthenticationCallback = callback;
            }

            public override string GetUserId() {
                return UserId;
            }

            public override string GetUserDisplayName() {
                return UserDisplayName;
            }

            public override void SignOut() {
                SignOutCount++;
            }
        }

        [Test]
        public void AuthenticateProxiedToClient() {
            var mockClient = new LoginClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.Authenticate(SentinelCallback);

            Assert.IsFalse(mockClient.AuthenticatedSilently.Value);
        }

        [Test]
        public void AuthenticateSilentlyProxied() {
            var mockClient = new LoginClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.Authenticate(SentinelCallback, true);

            Assert.IsTrue(mockClient.AuthenticatedSilently.Value);
        }

        [Test]
        public void AuthenticateLoudlyProxied() {
            var mockClient = new LoginClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.Authenticate(SentinelCallback, false);

            Assert.IsFalse(mockClient.AuthenticatedSilently.Value);
        }

        [Test]
        public void GetUserId() {
            var mockClient = new LoginClient();
            var platform = new PlayGamesPlatform(mockClient);

            mockClient.UserId = "userId";

            Assert.AreEqual("userId", platform.GetUserId());
        }

        [Test]
        public void GetUserDisplayName() {
            var mockClient = new LoginClient();
            var platform = new PlayGamesPlatform(mockClient);

            mockClient.UserDisplayName = "displayName";

            Assert.AreEqual("displayName", platform.GetUserDisplayName());
        }

        [Test]
        public void SignOutWorks() {
            var mockClient = new LoginClient();
            var platform = new PlayGamesPlatform(mockClient);

            platform.SignOut();

            Assert.AreEqual(1, mockClient.SignOutCount);
        }
    }

     class CapturingAction<T> {
        internal bool Invoked { get; private set; }

        private T captured;

        internal T Captured {
            get {
                return captured;
            }
            set {
                if (this.Invoked) {
                    throw new InvalidOperationException();
                }
                Invoked = true;
                captured = value;
            }
        }

        public void invoke(T value) {
            Captured = value;
        }
    }
}
