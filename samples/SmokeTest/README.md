== Running the Smoketest sample application ==

This sample demonstrates calling all of the APIs in the plugin.  It does not
attempt to actually use the resources, just to illustrate how to call the
APIs and access the results.

On the first screen, there are 2 buttons - Authenticate and Nearby.  Nearby
connections do not require authentication, so access to this API is immediately
available.

== Building ==
1. Create a new Unity project
2. Import the Google Play Games plugin
3. Import the Smoketest.unitypackage
4. Create a new game on the Play Game Console.
    4.1 Enable Saved Games.
5. Add the linked application for the platform(s) you want to build.
    5.1 Enable Turn-based and Real-time multiplayer for each app.
6. Add the following resources:
    6.1 AchievementToReveal - this is a hidden achievement
    6.2 AchievementToUnlock - this is a visible achievement
    6.3 AchievementToIncrement - this is a visible achievement
            that is incremental.  Set the steps to 10.
    6.4 AchievementHiddenIncremental - this is an incremental achievement that
            is initially hidden.
    6.5 Leaderboard named Leaders in SmokeTesting.
    6.6 Event named SmokingEvent.
7. Click the `Get Resources` link at the bottom of the list of one of the resource
types.  Select the Android or Objective-C tab and copy the resource data to the clipboard.
8. Back in Unity, select Window > Google Play Games > SetUp > Android (or iOS) Setup...
    8.1 If you are building for iOS, enter the bundle ID that matches the linked
application for iOS.
    8.1 Enter the constants class name: SmokeTest.GPGSIds.  This class is generated
from the resource data.
    8.2 Paste the resource data into the text area and press `Setup`.
9. Setup nearby connections by selecting Window > Google Play Games > Setup > Nearby Connections...
    9.1 Enter the service id to use, something like "com.yourcompany.smoketest"
10. Save the project and Build and Run.
