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

namespace CubicPilot.GameLogic
{
    public static class GameIds
    {
        // Achievements IDs (as given by Developer Console)
        public class Achievements
        {
            public const string NotADisaster = GPGSIds.achievement_not_a_disaster;
            public const string PointBlank = GPGSIds.achievement_point_blank;
            public const string FullCombo = GPGSIds.achievement_full_combo;
            public const string ClearAllLevels = GPGSIds.achievement_clear_all_levels;
            public const string PerfectAccuracy = GPGSIds.achievement_perfect_accuracy;
            public readonly static string[] ForRank =
                {
                    GPGSIds.achievement_sargent,
                    GPGSIds.achievement_captain,
                    GPGSIds.achievement_admiral
                };

            public readonly static int[] RankRequired = { 3, 6, 10 };

            public readonly static string[] ForTotalStars =
                {
                    GPGSIds.achievement_1_dozen_stars,
                    GPGSIds.achievement_two_dozen_stars,
                    GPGSIds.achievement_3_dozen_stars
                };

            public readonly static int[] TotalStarsRequired = { 12, 24, 36 };

            // incrementals:
            public readonly static string[] IncGameplaySeconds =
                {
                    GPGSIds.achievement_five_minute_master,
                    GPGSIds.achievement_30_minutes_of_excitement,
                    GPGSIds.achievement_procrastinate_much
                };

            public readonly static string[] IncGameplayRounds =
                {
                    GPGSIds.achievement_played_2_rounds,
                    GPGSIds.achievement_played_10_rounds,
                    GPGSIds.achievement_played_25_rounds
                };
        }

        // Leaderboard ID (as given by Developer Console)
        public readonly static string LeaderboardId = GPGSIds.leaderboard_cubic_pilot_hall_of_fame;
    }
}
