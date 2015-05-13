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

public class GameConsts {
    // Play Games plugin debug logs enabled?
    public const bool PlayGamesDebugLogsEnabled = true;

    // what is the maximum level?
    public const int MaxLevel = 11;

    // arena bounds (this is slightly larger than the screen!)
    public const float ArenaMinX = -20.0f;
    public const float ArenaMaxX = 20.0f;
    public const float ArenaMinY = -10.0f;
    public const float ArenaMaxY = 10.0f;

    // player movement constraints
    public const float PlayerMinY = -7.0f;
    public const float PlayerMaxY = 7.0f;
    public const float MaxPlayerYSpeed = 30.0f;

    // touch sensivity factor (1.0 means swiping across screen moves
    // the player from minimum Y to maximum Y).
    public const float TouchSensivity = 1.2f;

    // where do enemies appear?
    public const float EnemySpawnX = ArenaMaxX + 5.0f;

    // auto advance time (retry when game over, or advance to next level)
    public const float AutoRetryTime = 5.0f;
    public const float AutoAdvanceTime = 10.0f;

    // how long enemies stay in "I've been hit" color after being hit
    public const float EnemyHitColorDuration = 0.5f;

    // how long each level is
    public const float LevelDuration = 10.0f;

    // how long the "combo" lasts if the player doesn't hit anything
    public const float ComboCooldown = 5.0f;

    // what accuracy (%) is needed to earn 2 and 3 stars, respectively
    public const int AccuracyTwoStars = 50;
    public const int AccuracyThreeStars = 75;

    // below what X coordinate we consider an enemy to have been killed
    // "at close range" for the purposes of achievements
    public const float EnemyCloseRangeXThresh = -6;

    // score toast font size
    public const float ScoreToastFontSize = 60;

    // GAME OVER / YOU'VE WON screens:
    public const float GameOverFontSize = 100;
    public const float RetryTextOffset = 80;
    public const float RetryTextFontSize = 70;
    public const float StarsY = -100;
    public const float StarsFontSize = 100;
    public const float StarsMessageY = 250;
    public const float StarsMessageFontSize = 50;
    public static Color StarsMessageColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);

    // period for blinking messages (like the combo message, etc)
    public const float BlinkPeriod = 0.75f;

    // combo multipliers
    public static float[] ComboMult = { 1.0f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f };

    // Tutorial settings
    public class Tutorial {
        public const float PhaseDuration = 4.0f;
        public const float SteerArrowX = 100;
        public const float SteerTextX = 400;
        public const float FireTextX = 400;
        public const float FontSize = 45;
    }

    // HUD settings
    public class Hud {
        public const float StageFontSize = 35;
        public const float StageX = 150;
        public const float StageY = 40;

        public const float ScoreX = 120;
        public const float ScoreY = StageY;
        public const float ScoreFontSize = 70;

        public const float AccuracyX = 120;
        public const float AccuracyY = 80;
        public const float AccuracyFontSize = 30;

        // how fast the displayed score can change (for animation effect)
        public const float ScoreDisplayChangeSpeed = 1000;

        // how long a displayed message remains on the screen
        public const float MessageDuration = 2.0f;
        public const float MessageFontSize = 80;
        public const float MessageTransitionDuration = MessageDuration * 0.25f;

        // combo counter position
        public const float ComboX = 0;
        public const float ComboY = 100;
        public const float ComboFontSize = 40;
        public static Color ComboColor = new Color(0.0f, 0.0f, 0.8f, 1.0f);
        public const float ComboTransitionDuration = 0.5f;
    }

    // Pause screen settings
    public class PauseScreen {
        public const float TitleY = -180;
        public const float TitleFontSize = 80;
        public const float ButtonFontSize = 50;
        public const float ButtonWidth = 320;
        public const float ButtonHeight = 200;

        public const float ResumeX = 250 - 0.5f * ButtonWidth;
        public const float ResumeY = 50 - 0.5f * ButtonHeight;

        public const float QuitX = -250 - 0.5f * ButtonWidth;
        public const float QuitY = ResumeY;

        public const float PauseButtonX = 120;
        public const float PauseButtonY = 120;
        public const float PauseButtonSize = 100;
    }

    // Main Menu Settings
    public class Menu {
        public const float TransitionDuration = 0.5f;

        public const float TitleY = 100;
        public const float TitleFontSize = 100;

        // "Please wait" message settings:
        public const float PleaseWaitFontSize = 60;

        // Play button
        public const float PlayButtonFontSize = 60;
        public const float PlayButtonWidth = 400;
        public const float PlayButtonHeight = 200;

        // Achievements and leaderboards buttons
        public const float AchButtonWidth = PlayButtonWidth;
        public const float AchButtonHeight = PlayButtonHeight;
        public const float AchButtonY = -AchButtonHeight/2 - 200;
        public const float AchButtonX = -420 - AchButtonWidth/2;
        public const float AchFontSize = 40;
        public const float LbButtonWidth = AchButtonWidth;
        public const float LbButtonHeight = AchButtonHeight;
        public const float LbButtonX = 420 - LbButtonWidth/2;
        public const float LbButtonY = AchButtonY;
        public const float LbFontSize = AchFontSize;
        public const float LoadButtonWidth = PlayButtonWidth;
        public const float LoadButtonHeight = PlayButtonHeight;
        public const float LoadButtonX = AchButtonX + LoadButtonWidth + 20;
        public const float LoadButtonY = AchButtonY;


        // Sign-in button
        public const float SignInButtonWidth = 360;
        public const float SignInButtonHeight = 160;
        public const float SignInButtonX = -580;
        public const float SignInButtonY = SignInButtonHeight + 150;
        public const float SignInButtonFontSize = 50;

        // Placement of Google+ logo
        public const float GooglePlusLogoX = -550;
        public const float GooglePlusLogoY = SignInButtonY - 40;
        public const float GooglePlusLogoSize = 80;

        // Placement of the version string
        public const float BuildStringX = 20;
        public const float BuildStringY = 50;
        public const float BuildStringFontSize = 35;

        // Sign-in encouragement text
        public const float SignInBlurbX = 250;
        public const float SignInBlurbY = 230;
        public const float SignInBlurbFontSize = 40;
        public static Color SignInBlurbColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        public static Color SignedInBlurbColor = ThemeColor;
        public const float SignInBarY = 330;
        public const float SignInBarHeight = 200;

        // sign-out button
        public const float SignOutButtonWidth = 250;
        public const float SignOutButtonHeight = 110;
        public const float SignOutButtonX = SignOutButtonWidth + 10;
        public const float SignOutButtonY = SignOutButtonHeight + 10;
        public const float SignOutButtonFontSize = 15;

        // for level selection screen:
        public const int LevelSelRows = 3;
        public const int LevelSelCols = 4;
        public const float LevelButtonSize = 200;
        public const float LevelGridStartX = -330;
        public const float LevelGridStartY = -220;
        public const float LevelButtonSpacing = 20;
        public const float LevelFontSize = 100;
        public const float LevelScoreOffsetX = 0;
        public const float LevelScoreOffsetY = 65;
        public const float LevelScoreFontSize = 30;

        // "up" button from level selection screen to main screen:
        public const float UpButtonLeft = 20;
        public const float UpButtonTop = 20;
        public const float UpButtonWidth = 200;
        public const float UpButtonHeight = 120;
        public const float UpButtonFontSize = 80;

        // pilot info
        public const float PilotInfoY = 30;
        public const float PilotInfoYSmallFont = 30;
        public const float PilotInfoX = 20;
        public const float PilotInfoFontSize = 35;
        public const float PilotInfoFontSizeSmall = 35;

        // total score and stars display
        public const float TotalScoreLabelX = 400;
        public const float TotalScoreLabelFontSize = 40;
        public const float TotalScoreLabelY = 50;
        public const float TotalScoreX = 150;
        public const float TotalScoreY = 50;
        public const float TotalScoreFontSize = 60;
        public const float StarsX = 90;
        public const float StarsY = 120;
        public const float StarsFontSize = 50;
    }

    // General theme color
    public static Color ThemeColor = new Color(51/256.0f, 181/256.0f, 229/256.0f, 1.0f);

    // Level progression settings
    public class Progression {
        // Maximum level
        public const int MaxLevel = 10;

        // Pilot experience points needed for each experience level:
        public static int[] ExpForLevel = {
            -1, 0, 25, 50, 100, 200, 400, 800, 1600, 2400, 3200 };

        // Level titles
        public static string[] Titles = {
            "", "Rookie", "Cadet", "Professional", "Lieutenant",
            "Leader", "Wing Commander", "Commodore",
            "Vice-Marshal", "Marshal", "Chief-Marshal"
        };

        // Fire cooldown time for each level
        public static float[] FireCooldown = {
            -1.0f, 1.0f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f
        };

        // Player's damage at each level
        public static int[] Damage = { 0, 1, 1, 2, 2, 2, 3 };

        // Player's shield points at each level
        public static int[] ShieldPoints = { 0, 0, 0, 0, 0, 1, 1, 1, 2, 3 };

        // Player's laser speed
        public static float[] LaserSpeed = { 0.0f, 20.0f, 22.0f, 24.0f, 26.0f, 28.0f, 30.0f };
    }

    public class EndScreen {
        public const float EndTextX = -400;
        public const float EndTextY = -100;
        public const float EndTextFontSize = 45;
    }
}
