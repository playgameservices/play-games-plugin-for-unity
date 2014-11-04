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
using UnityEngine.SocialPlatforms;

namespace GooglePlayGames.BasicApi {
    public class Achievement : IAchievement{
        public string Id = "";
        public bool IsIncremental = false;
        public bool IsRevealed = false;
        public bool IsUnlocked = false;
        public int CurrentSteps = 0;
        public int TotalSteps = 0;
        public string Description = "";
        public string Name = "";

        public override string ToString () {
            return string.Format("[Achievement] id={0}, name={1}, desc={2}, type={3}, " +
                " revealed={4}, unlocked={5}, steps={6}/{7}", Id, Name,
                Description, IsIncremental ? "INCREMENTAL" : "STANDARD",
                IsRevealed, IsUnlocked, CurrentSteps, TotalSteps);
        }

        public Achievement() {
		}

		#region IAchievement implementation

        public void ReportProgress(Action<bool> callback)
        {
            PlayGamesPlatform.Instance.ReportProgress(Id, percentCompleted, callback);
        }

        public bool completed
        {
            get { return IsUnlocked; }
        }

        public bool hidden
        {
            get { return !IsRevealed; }
        }

        public string id
        {
            get
            {
                return Id;
            }
            set
            {
                Id = value;
            }
        }

        public DateTime lastReportedDate
        {
            get { throw new NotImplementedException(); }
        }

        public double percentCompleted
        {
            get
            {
                return (double)CurrentSteps / (double)TotalSteps;
            }
            set
            {
                CurrentSteps = (int)Math.Round(TotalSteps * value / 100.0);
            }
        }

		#endregion

    }
}

