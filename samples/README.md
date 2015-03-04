# Samples
_Copyright (c) 2014 Google Inc. All rights reserved._

These are sample games that use the Google Play Games plugin for Unity.

**Minimal** is a simple sample that has an "Authenticate" button and nothing else.
It is meant to show the minimal amount of code necessary to set up an integration
with Google Play Games.

**Cubic Pilot** is a complete game that demonstrates how to use the plugin to implement sign in,
achievements, leaderboards and cloud save.

**Cubic Pilot_4.6** is a complete game that demonstrates how to use the plugin to implement sign in,
achievements, leaderboards and cloud save. This sample uses features from **Unity 4.6** for targeting different resolutions and orientations, 
and external controllers and gamepads such as used with the Nexus Player. Warning: **_You must open this sample in
Unity 4.6 or greater_**.

**Tic Tac Toe** is a complete game that demonstrates how to use the plugin to implement turn-based multiplayer.

**Quiz Racer** is a complete game that demonstrates how to use the plugin to implement real-time multiplayer.

**Quiz Racer_4.6** is a complete game that demonstrates how to use the plugin to implement
real-time multiplayer using **Unity 4.6** for targeting different resolutions and orientations, 
and external controllers and gamepads such as used with the Nexus Player. Warning: **_You must open this sample in
Unity 4.6 or greater_**.

Note: this text assumes you have already read and are familiar with the
contents of the [Getting Started Guide](https://github.com/playgameservices/play-games-plugin-for-unity/blob/master/README.md)
(which is in the README.md file that exists in the root of this Github
repository). Make sure to read that file first!

## How to Build

To build a sample, you must first [configure a game project
game](https://developers.google.com/games/services/console/enabling) in the 
Google Play Developer Console. Follow the instructions on creating a client ID 
for Android and/or iOS (depending on what platforms you intend to run the game).
More information about this can be found in the **Getting Started Guide**.

## Configure Achievements and the Leaderboard

For the **Minimal**, **QuizRacer** and **TicTacToe** samples, no achievements or leaderboards are necessary.

For the **Cubic Pilot** sample, you must create the necessary achievements and leaderboards
for this game. To do this, open the **Assets/GameLogic/GameIds.cs** file
and look at the achievements required by the game. Then, as you create the
corresponding achievements and leaderboard in the Developer Console,
and replace the "PLACEHOLDER" strings in the file by the corresponding IDs.

## Set up your package name

Go to the Android and iOS Player Settings window in Unity and configure your
package name and/or Bundle ID. To access this window, click
**File | Build Settings...**, select the appropriate platform and click the
**Switch Platform** button, and then the **Player Settings** button.

Use the package name that corresponds to the Client ID you have configured
in the Developer Console.

## Import the Google Play Games plugin

Import the Google Play Games plugin file (the **GooglePlayGamesPlugin-X.YY.ZZ.unitypackage** 
file) into Unity. Then select **File | Play Games - Android Settings** from the
menu to set up the game for Android, and/or **File | Play Games - iOS
Settings** to set up the game for iOS.

In each of those dialogs, enter the corresponding configuration values as
set up in the Developer Console.

## Import the packaged asset containing the sample

Each sample is packaged as a Unity package.  This package should be imported into your project, and the 
scenes selected in the build dialog.

## Build and Run

Follow the instructions on the **Getting Started Guide** to build and run the
game on Android and/or iOS.

## Acknowledgements

The Cubic Pilot sample uses the Jura font by [Daniel Johnson](https://plus.google.com/113574588462430984234/about), available from the [Google Web Fonts project](http://www.google.com/fonts).

