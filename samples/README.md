# Samples
_Copyright (c) 2014 Google Inc. All rights reserved._

These are sample games that use the Google Play Games plugin for Unity.

**Minimal** is a simple sample that has an "Authenticate" button and nothing else.
It is meant to show the minimal amount of code necessary to set up an integration
with Google Play Games.

**Cubic Pilot** is a complete game that demonstrates how to use the plugin to implement sign in,
achievements, leaderboards and cloud save.  It also supports gamepad controllers.

**SmokeTest** is a "kitchen sink" sample - with buttons for each API call.

**Tic Tac Toe** is a complete game that demonstrates how to use the plugin to implement turn-based multiplayer.

**Quiz Racer** is a complete game that demonstrates how to use the plugin to implement real-time multiplayer.

**Nearby Droids** is a game demonstrating the nearby connection API.  This sample requires android play services sdk 23 or higher.
The nearby connections API is only available on Android.  The UI in this game uses the 'new' Unity UI system, 
so Unity 5.0 or higher is recommended.  For more details and how to build and configure the sample, 
see [Nearby Connections documentation](https://github.com/playgameservices/play-games-plugin-for-unity/blob/master/NEARBY.md).

> **Note:**
> This text assumes you have already read and are familiar with the
contents of the [Getting Started Guide](https://github.com/playgameservices/play-games-plugin-for-unity/blob/master/README.md)
(which is in the README.md file that exists in the root of this Github
repository). Make sure to read that file first!

## How to Build

To build a sample, you must first [configure a game project
game](https://developers.google.com/games/services/console/enabling) in the 
Google Play Developer Console. Follow the instructions on creating a client ID 
for Android and/or iOS (depending on what platforms you intend to run the game).
More information about this can be found in the **Getting Started Guide**.

All the samples follow the same basic steps, see the section for the the specific
sample for what resources need to be configured in the play console.

 1. Create a new Unity Project
 2. [Import the Google Play Games plugin](https://github.com/playgameservices/play-games-plugin-for-unity#plugin-installation)
 3. Import the &lt;sampleName&gt;.unitypackage (Assets &gt; Import Package)
 4. In the Play Games developer web app, create the linked application remembering to use the
    same configuration information when configuring your game in Unity.
 5. Create the resource configuration needed for the sample in the play console.
 6. Then at the bottom of the Achievement (or Leaderboards, or Events) list,
     click "Get Resources", and copy
    the resource definitions to the clipboard.
 7. Back in the Unity editor, open the Google Play Games setup dialog
    4.1 Enter the constants class name for the sample (listed below for each sample)
    4.2 Paste the resource definitions from the Play Console into the text box.
 8. Click Setup
 9. Build and run!

## Configuration for Minimal

Minimal lives up to its name and needs the minimum configuration to test.

 1. Create 1 Achievment named "Welcome"
 2. Use the constants class name: `Minimal.GPGSIds`

## Configuration for CubicPilot

 Make sure Saved Games is enabled.

 There are several achievements for CubicPilot:

1. Name: "Not a Disaster..."<br/>
    Description: "Get 1 point without dying."
2. Name: "Point Blank!"<br/>
    Description: "Kill an enemy at point blank range."
3. Name: "Full Combo"<br/>
    Description: "Complete the maximum combo bonus chain."
4. Name: "Clear All Levels"<br/>
    Description: "Clear all the levels."
5. Name: "Perfect Accuracy"<br/>
    Description: "Clear a level with 100% accuracy."
6. Name: "Sargent"<br/>
    Description: "Advancement based on levels completed."
7. Name: "Captain"<br/>
    Description: "Advancement based on levels completed."
8. Name: "Admiral"<br/>
    Description: "Advancement based on levels completed."

These are incremental achievements that have counts to be met to unlock the
achievement.

1. Name: "1 Dozen Stars"<br/>
    Description: "Earn 12 stars"<br/>
    Incremental: Yes<br/>
    Steps needed: 12
2. Name: "Two Dozen Stars"<br/>
    Description: "Earn 24 stars"<br/>
    Incremental: Yes<br/>
    Steps needed: 24
3. Name: "3 Dozen Stars"<br/>
    Description: "Earn 36 stars"<br/>
    Incremental: Yes<br/>
    Steps needed: 36
4. Name: "Five Minute Master"<br/>
    Description: "Play for a total of 5 minutes."<br/>
    Incremental: Yes<br/>
    Steps needed: 300
5. Name: "30 Minutes of Excitement"<br/>
    Description: "Play for a total of 30 minutes."<br/>
    Incremental: Yes<br/>
    Steps needed: 1800
6. Name: "Procrastinate Much?"<br/>
    Description: "Play for a total of 1 hour."<br/>
    Incremental: Yes<br/>
    Steps needed: 3600
7. Name: "Played 2 rounds"<br/>
    Description: "Play 2 rounds"<br/>
    Incremental: Yes<br/>
    Steps needed: 2
8. Name: "Played 10 rounds"<br/>
    Description: "Play 10 rounds"<br/>
    Incremental: Yes<br/>
    Steps needed: 10
9. Name: "Played 25 rounds"<br/>
    Description: "Play 25 rounds"<br/>
    Incremental: Yes<br/>
    Steps needed: 25


Create 1 leaderboard:

1. Name: Cubic Pilot Hall of Fame"

Use `CubicPilot.GPGSIds` as the name of the constants class when setting up the
game. Also, open the
`File &gt; Build Settings &gt; Player Settings &gt; Resolution &amp; Presentation`
and disable `Portrait` and `Portrait Upside Down` rotations by unchecking them.

> **Note:**
> If the game loads with just a background and no menu, then Unity didn't
> properly detect the game's scenes. These are located in CubicPilot/Scenes and
> should be dragged into the Android Build Settings
> (`File &gt; Build Settings &gt; Android`).

## Configuration for QuizRacer
This is a real-time multi-player game.  Create the achievements:

Make sure real-time multiplayer is enabled.

1. Name: "Play"<br/>
    Description:  "Play Quiz Racer"
2. Name: "Score"<br/>
    Description: "Score at least 1 point"
3. Name: "Win"<br/>
    Description: "Win a match"
4. Name: "Lose"<br/>
    Description: "Lose a match"
5. Name: "Don't play"<br/>
    Description: "Don't play a match"

Use `QuizRacer.GPGSIds` for the constants class when setting up the game.

## Configuration for SmokeTest
This is a sample demonstrating each function.

Make sure Save Games, Real-time and Turn-based multiplayer are enabled.

Events:

1. Name: "SmokingEvent"

Achievements:

1. Name: "AchievementToReveal"<br/>
    Description: "A hidden achievement to be revealed later"<br/>
    Initial State: "Hidden"
2. Name: "AchievementToUnlock"<br/>
    Description: "A normal achievement"<br/>
3. Name: "AchievementToIncrement"
    Description: "Incremental achievement to unlock - 25 times"<br/>
    Incremental: yes<br/>
    Steps needed: 25
4. Name: "Achievement hidden incremental"<br/>
    Description: "Initially hidden, incremental achievement"<br/>
    Incremental: yes<br/>
    Steps needed: 25<br/>
    Initial State: Hidden
5. Name: "Lucky5"<br/>
    Description: "You need 5 achievements to publish"

Leaderboards:

1. Name: "Leaders in SmokeTesting"


Use `SmokeTest.GPGSIds` for the constants class when setting up the game.

## Configuration for TicTacToe
This is a turn-based game.

Events:
    1. Name: "Play"

Use `TicTacToe.GPGSIds` for the constants class when setting up the game.

## Configuration for TrivalQuest
This is a sample for Events and Quests.  The Quest definition configuration
is left as an exercise for you.

Events:

1. Name: "Red"
2. Name: "Green"
3. Name: "Blue"
4. Name: "Yellow"

Use `TrivialQuest.GPGSIds` for the constants class when setting up the game.

## Acknowledgements

The Cubic Pilot sample uses the Jura font by [Daniel Johnson](https://plus.google.com/113574588462430984234/about), available from the [Google Web Fonts project](http://www.google.com/fonts).

