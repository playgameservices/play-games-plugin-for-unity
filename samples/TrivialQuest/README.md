# TrivialQuest
This sample demonstrates the use of Events and Quests in the Play Games Plugin
for Unity.  You should **follow the normal setup instructions for other samples**,
but in addition you will need to perform the following steps to get the full
sample functionality:

  1. Go to the Google Play Developer Console and open your game project.
  2. Add the linked application for the platform(s) you want to build.
  3. Add the following resources:
    3.1 Event named red
    3.2 Event named green
    3.2 Event named blue
    3.2 Event named yellow
  4. Click the `Get Resources` link at the bottom of the events page.
    Select the Android or Objective-C tab and copy the resource data to the clipboard.
  5. Back in Unity, select Window > Google Play Games > SetUp > Android (or iOS) Setup...
    5.1 If you are building for iOS, enter the bundle ID that matches the linked
application for iOS.
    5.2 Enter the constants class name: TrivalQuest.GPGSIds.  This class is generated
from the resource data.
    5.3 Paste the resource data into the text area and press `Setup`.
  6. Create at least one **Quest** based on achieving some of the events that
  you just created, make sure the Quest start date is in the past and the
  Quest end date is in the future.  For example, you could create a Quest with
  the following metadata:

        Name: Attack 5 Reds
        Description: Attack a red monster at least 5 times.
        Completion Criteria: 'red' is increased by 5.
        Start Date: (today's date)
        End Date: (a week from now)

  Once you have completed all of these steps, open the game and sign in.
  You should be able to see the quests you have created, complete them,
  and claim the rewards.
