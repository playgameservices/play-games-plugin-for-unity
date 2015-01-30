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

public static class Strings {
    public const string GameTitle = "Cubic Pilot";
    public const string BuildString = "Version 0.8";

    public const string GameOverPlayerDied = "Kaboom!";
    public const string GameOverCiviliansDied = "Civilian ship killed!";
    public const string RetryingIn = "Retrying in";
    public const string LevelCleared = "Level Cleared!";
    public const string NextLevelIn = "Next level in";
    public const string Stage = "Sector";

    public const string Play = "Play!";

    public const string ExpString = "Pilot Level {0} ({1}) - exp {2}/{3}";
    public const string ExpStringMaxLevel = "Pilot Level {0} ({1}) [MAXIMUM] - exp {2}";
    public const string TotalScore = "Total Score";
    public const string TotalStarsFmt = "\u2605 {0:D2}";

    public const string GamePaused = "Game Paused";
    public const string ResumeGame = "Resume";
    public const string QuitGame = "Quit";
    public const string SaveGame = "Save Progress";

    public const string AccuracyFmt = "Accuracy: {0}%";

    public static string[] StarsMessage = { "",
        "That was ok, but you missed a lot of shots.\nTo get 2\u2605, improve your accuracy to 50%!",
        "Great performance!\nTo get 3\u2605, improve your accuracy to 75%!",
        "Awesome performance!" };

    public const string Achievements = "Achievements";
    public const string Leaderboards = "High Scores";

    public const string SignInBlurb = "Sign in to unlock achievements, post scores\n" +
        "and save your progress to the cloud!";
    public const string SignedInBlurb = "You are signed in with your Google account.\n"  +
        "Your progress will be saved to the cloud automatically!";

    public const string ComboFmt = "COMBO {0:F1}x";

    public const string SignIn = "Sign in";
    public const string SignOut = "Sign Out";

    public const string SigningIn = "Signing in and loading your progress...";

    public const string EndText =
        "Wait.. what!? How did... Oh, wow, you\n" +
        "actually finished the game! This was not\n" +
        "supposed to happen.... I mean, *ahem*\n" +
        "Congratulations! Awesome!!!\n\n" +
        "But how did you get past those\n" +
        "indestructible... I mean, never mind, you\n" +
        "were totally supposed to get to this screen.\n" +
        "Congrats! Seriously!!";

    public const string LevelProgFmt = "Sector progress: {0}%";
    public const string StageFmt = "Sector {0} ({1}%)";

    public const string Tutorial1 = "Drag up/down here\nto steer your ship.";
    public const string Tutorial2 = "Tap anywhere on the right half\nof the screen to fire.";
    public const string Tutorial3 = "Destroy enemies before they reach\nthe blue civilian ships!";
}
