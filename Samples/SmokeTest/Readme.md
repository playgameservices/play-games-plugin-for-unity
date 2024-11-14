## Running the Smoketest sample application

This sample demonstrates calling all of the APIs in the plugin.  It does not
attempt to actually use the resources, just to illustrate how to call the
APIs and access the results.

On the first screen, there are 2 buttons - Authenticate and Nearby.  Nearby
connections do not require authentication, so access to this API is immediately
available.

## Creating Play Console Game Project

Follow the steps [here](https://developer.android.com/games/pgs/console/setup)
to create your Play Console Project and Google Cloud Project for building
SmokeTest APK.

Next steps are to create achievements, leaderboards and events.

*   Create three achievements in play console with exact given name:

    *   `achievementtoincrement` - Check
        [this](https://services.google.com/fh/gumdrop/preview/misc/increment_console.png) box to make it
        incremental
    *   `achievementtounlock` - Revealed and non - incremental achievement
    *   `achievementtoreveal` -
        [Hidden](https://services.google.com/fh/gumdrop/preview/misc/hidden_console.png) Achievement,
        non incremental
    *   `achievement_hidden_incremental` - Hidden and Incremental Achievement

*   Create one leaderboard with name - `leaders_in_smoketesting` - Default
    settings

*   Create one event with name - `smokingevent` - Default settings

## Integrating PGP Plugin with SmokeTest

### Importing Packages in Unity Hub

*   Create a blank Unity project in Unity Hub.
*   Import GooglePlayGamesPlugin and SmokeTest unity package
    *   Assets > Import Package > Custom Package ...

### Configuring Build Settings

Open Build Settings ( File > Build Settings )

*   Choose `Android` and click on `Switch Platform`.
*   Go to `Player Settings > Other Settings`
    *   Set the `Package Name` to the one you have given in Play Developer
        Console
    *   Select a version for `Minimum API Level`.
    *   Set `Scripting Backend` to`IL2CPP`
    *   Set `Target Architectures` to `ARM64`
*   Go to `Player Settings > Publishing Settings`
    *   Go to `Keystore Manger` and create a new SHA1 if you don't have any
        existing one.
    *   If you already have an existing keystore and SHA1 certificate set that
        at your custom keystore and use the required key.
    *   Copy the SHA1 use have set in your Unity Hub and paste it into the
        `Credentials` section in Google cloud console -
        [here](https://services.google.com/fh/gumdrop/preview/misc/sha_creds_update.png)

### Configuring Google Play Games in Unity Hub

This setup is required to connect Play Developer Console `AndroidManifest.xml`
file with Unity Project

*   Go to Play Developer Console and copy the `AndroidManifest.xml` from `Grow >
    Play Games Services > Setup and Management > Configuration`.
*   Go to `Window > Google Play Games > Setup > Android Setup`.
*   Paste the copied xml file into text area.
*   Fill the text field for `Constants class name` with `SmokeTest.Scripts.GPGSIds`
*   Paste the name of your package into Go to `Window > Google Play games >
    Setup > Nearby Connections Setup`

### Adding GPGSIds in SmokeTest

*   Open Unity Hub and navigate to file `Assets > SmokeTest > Scripts >
    GPGSIds.cs`.
*   Replace the constants in that file with the constants present in `Assets >
    GPGSIds.cs`.

## Testing and App Signing

The step explains the steps need to upload .aab file and adding testers' Email
ID in order to test the SmokeTest

*   Add the name of testers under two tabs in Play Developer Console :
    *   Grow > Play Games Services > Setup and Management > Testers
    *   Release > Testing > Internal Testing > Testers
*   Create .aab file of unity project and upload over `Release > Testing >
    Internal Testing`

NOTE : Create .aab file by go to `File > Build settings` check the box `Build
App Bundle ( Google Play )` and click `Build`.

## Build and Test APK

For testing the apk you should have an android device, make sure to enable
developer options in the device and connect with your machine.

*   Go to `File > Build Settings`.
*   Click `Build and Run`

