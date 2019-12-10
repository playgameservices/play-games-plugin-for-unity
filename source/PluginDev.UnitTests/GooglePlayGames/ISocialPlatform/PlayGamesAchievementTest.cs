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
using GooglePlayGames;

namespace GooglePlayGames.UnitTests
{
    [TestFixture]
    public class PlayGamesAchievementTest
    {
        [Test]
        public void LastReportedDateReturnsEpoch()
        {
            Assert.AreEqual(
                new DateTime(1970, 1, 1, 0, 0, 0, 0),
                new PlayGamesAchievement().lastReportedDate);
        }

        [Test]
        public void NeverCompleted()
        {
            Assert.IsFalse(new PlayGamesAchievement().completed);
        }

        [Test]
        public void NeverHidden()
        {
            Assert.IsFalse(new PlayGamesAchievement().hidden);
        }

        [Test]
        public void ReportProgressUsesCorrectState()
        {
            string capturedId = "initial" ;
            double capturedProgress = 0.0;
            Action<bool> capturedCallback = null;

            PlayGamesAchievement achievement = new PlayGamesAchievement(
                (id, progress, callback) => {
                    capturedId = id;
                    capturedProgress = progress;
                    capturedCallback = callback;
                });

            Action<bool> dummy = ignored => { };
            achievement.id = "expectedId";
            achievement.percentCompleted = 50.0;

            achievement.ReportProgress(dummy);

            Assert.AreSame(capturedCallback, dummy);
            Assert.AreEqual("expectedId", capturedId);
            Assert.AreEqual(50.0, capturedProgress);
        }

    }
}

