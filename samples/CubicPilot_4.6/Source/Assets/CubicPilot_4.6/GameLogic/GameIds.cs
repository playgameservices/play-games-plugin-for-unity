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

using UnityEngine;
using System.Collections;

// Note: DO NOT edit or reomve the GPGSID markers next to the achievement/leaderboard
// IDs. They are used by the git pre-commit script to check if you are accidentally
// trying to commit actual IDs instead of placeholders to the repository.

public static class GameIds {
    // Achievements IDs (as given by Developer Console)
    public class Achievements {
        public const string NotADisaster = "PLACEHOLDER"; // <GPGSID>
        public const string PointBlank = "PLACEHOLDER"; // <GPGSID>
        public const string FullCombo = "PLACEHOLDER"; // <GPGSID>
        public const string ClearAllLevels = "PLACEHOLDER"; // <GPGSID>
        public const string PerfectAccuracy = "PLACEHOLDER"; // <GPGSID>

        public readonly static string[] ForRank = {
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER" // <GPGSID>
        };
        public readonly static int[] RankRequired = { 3, 6, 10 };

        public readonly static string[] ForTotalStars = {
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER" // <GPGSID>
        };
        public readonly static int[] TotalStarsRequired = { 12, 24, 36 };

        // incrementals:
        public readonly static string[] IncGameplaySeconds = {
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER" // <GPGSID>
        };
        public readonly static string[] IncGameplayRounds = {
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER", // <GPGSID>
            "PLACEHOLDER" // <GPGSID>
        };
    }

    // Leaderboard ID (as given by Developer Console)
    public readonly static string LeaderboardId = "PLACEHOLDER"; // <GPGSID>
}

