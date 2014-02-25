# Google Play Games plugin for Unity
_Copyright (c) 2013 Google Inc. All rights reserved._

The Google Play Games plugin for Unity&reg; is an open-source project whose goal
is to provide a plugin that allows game developers to integrate with
the Google Play Games API from a game written in Unity&reg;. However, this project is
not in any way endorsed or supervised by Unity Technologies.

_Unity&reg; is a trademark of Unity Technologies._

_iOS is a trademark of Apple, Inc._

## Overview

The Google Play Games plugin for Unity allows you to access the Google Play Games 
API through Unity's [social interface](http://docs.unity3d.com/Documentation/ScriptReference/Social.html). The initial release of the plugin provides support for the 
following features of the Google Play Games API:<br/>

* sign in
* unlock/reveal/increment achievement
* post score to leaderboard
* cloud save read/write
* show built-in achievement UI
* show built-in leaderboards UI

Features:

* easy GUI-oriented project setup (integrated into the Unity GUI)
* cross-platform support (Android and iOS) with no need for platform glue code.
* (Android) no need to override/customize the player Activity
* (Android) no need to override/customize AndroidManifest.xml
* (iOS) integrates into XCode build

System requirements:

* Unity&reg; 4.2 or above
* To deploy on Android:
    * Android SDK
    * Google Play Services library
* To deploy on iOS:
    * XCode 4 or above
    * Google Plus SDK for iOS
    * Google Play Games SDK for iOS

Note: This first release does not yet include support for multiplayer, listing friends, 
interactive posts, raw data access, etc.

## Upgrading

If you have already integrated your project with a previous version of the plugin and wish to upgrade to a new version, please refer to the [upgrade instructions](https://github.com/playgameservices/play-games-plugin-for-unity/blob/master/UPGRADING.txt).

## Configure Your Game

To use the plugin, you must first [configure your 
game](https://developers.google.com/games/services/console/enabling) in the 
Google Play Developer Console. Follow the instructions on creating a client ID 
for Android and/or iOS (depending on what platforms you intend to deploy or game 
on). Be particularly careful when entering your package name and your 
certificate fingerprints, since mistakes on those screens can be difficult to 
recover from.

Please note the following pieces of information when creating your client IDs, 
as they will be necessary later:

**Android**

* Your package name (e.g. "com.example.awesomegame")
* Your application ID (e.g. 123456789012)

**iOS**

* Your bundle identifier (e.g. "com.example.AwesomeGame")
* Your application ID (e.g. 123456789012)*
* Your client ID (e.g. "46798138751-afwa4iejsfjskj.apps.googleusercontent.com")

_[*] The application ID is the number that Google Play Developer Console 
assigns to your project. Please note that is not the same as your Apple 
application ID._

**Note:** Do not forget to add your test accounts (the accounts with which you 
will try signing in) to the **Testing** section of the Developer Console, 
otherwise you will not be able to sign in to your game.

## Add Achievements and Leaderboards

Add 
[achievements](https://developers.google.com/games/services/common/concepts/achievements) 
and 
[leaderboards](https://developers.google.com/games/services/common/concepts/leaderboards) 
to your game in the Google Play Developer Console. For each achievement and 
leaderboard you configure, make sure to note the corresponding **achievement ID** or **leaderboard ID**, as those will be needed when making the API calls. 
Achievement and leaderboard IDs are alphanumeric strings (e.g. 
"Cgkx9eiuwi8_AQ").

## Load Your Game Project

Next, load your game project into the Unity editor.

If you do not have a game project to work with, you can use the **Minimal** sample
available in the **samples** directory. Using that sample will allow you to
quickly test your setup and make sure you can access the API.

If you want to test a larger sample after you are familiar with the plugin, 
try the **CubicPilot** game.
More information about building the samples can be found in the
[samples README](https://github.com/playgameservices/play-games-plugin-for-unity/blob/master/samples/README.md) file.

## Plugin Installation

To download the plugin, clone this Git repository into your file system (or download it as 
a ZIP file and unpack it). Then, look for the **unitypackage** file in 
the **current-build** directory:

    current-build/GooglePlayGamesPluginForUnity-X.YY.ZZ.unitypackage

To install the plugin, simply open your game project in Unity and import that file into
your project's assets, as you would any other Unity package. In Unity 4.2 and
4.3, this is accomplished through the **Assets | Import Package | Custom
Package** menu item (you can also reach this menu it by right-clicking the
**Assets** folder). After importing, you should see that two new menu items
were added to the File menu: **"Play Games Android setup"** and **"Play Games
iOS setup"**. If you don't see the new menu items, refresh the assets by
clicking **Assets | Refresh** and try again.

## Android Setup

To configure your Unity game to run with Google Play Games on Android, first
open the Android SDK manager and verify that you have downloaded the **Google
Play Services** package. The Android SDK manager is usually available in your
SDK installation directory, under the "tools" subdirectory, and is called
**android** (or **android.exe** on Windows). The **Google Play Services**
package is available under the **Extras** folder. If it is not installed
or is out of date, install or update it before proceeding.

Next, set up the path to your Android SDK installation in Unity. This is located in the
preferences menu, under the **External Tools** section. 

Next, configure your game's package name. To do this, click **File | Build Settings**, 
select the **Android** platform and click **Player Settings** to show Unity's 
Player Settings window. In that window, look for the **Bundle Identifier** setting
under **Other Settings**. Enter your package name there (for example
_com.example.my.awesome.game_).

Next, click the **File | Play Games - Android setup** menu item. This will display the Android setup screen, where you must input your Application ID (e.g. 12345678012).

After filling in the application ID, click the **Setup** button.

**Important:** The application ID and package name settings must match exactly
the values you used when setting up your project on the Developer Console.

### Additional instructions on building for Android on Windows

If you are using Windows, you must make sure that your Java SDK installation can be accessed by Unity. To do this:

1. Set the JAVA_HOME environment variable to your Java SDK installation path (for example, `C:\Program Files\Java\jdk1.7.0_45`).
2. Add the Java SDK's `bin` folder to your `PATH` environment variable (for example, `C:\Program Files\Java\jdk1.7.0_45\bin`)
3. Reboot.

**How to edit environment variables:** In Windows 2000/XP/Vista/7, 
right-click **My Computer**, then **Properties**, then go to **Advanced System Properties**
(or **System Properties** and then click the **Advanced** tab), then
click **Environment Variables**. On Windows 8, press **Windows Key + W** and
search for **environment variables**.
For more information, consult the documentation for your version of Windows. 


## iOS Setup

To configure your Unity game to run with Google Play Games on iOS, download the 
Games SDK and the Google+ iOS SDK, which are available from 
[our downloads page](https://developers.google.com/games/services/downloads/). Unpack the 
downloads into a directory of your choice. The necessary bundles and frameworks 
are:

* GoogleOpenSource.framework
* GooglePlus.bundle
* GooglePlus.framework
* PlayGameServices.bundle
* PlayGameServices.framework

Next, open the iOS build settings dialog. To do so, click **File | Build Settings**,
select the **iOS** platform, and click **Player Settings**. Find the **Bundle Identifier**
setting and enter your bundle identifier there.

Next, click the **File | Play Games - iOS **setup menu item. This will display the 
iOS setup screen, where you must input:

* Your application ID (e.g. 123456789012)
* Your client ID (e.g. "46798138751-afwa4iejsfjskj.apps.googleusercontent.com")
* Your bundle identifier (e.g. "com.example.AwesomeGame")

All of these settings must match exactly the values you used when setting up 
your client ID on the Developer Console previously.

When ready, click the **Setup** button to finish the configuration process.

**Important:** If you ever change your bundle ID, you must perform the iOS setup
again in order to update the necessary files where this information gets replicated.

## Run the Project

If you are working with the **Minimal** sample, you should be able to build
and run the project at this point. You will see a screen with an **Authenticate** button,
and you should be able to sign in when you click it.

To build and run on
Android, click **File | Build Settings**, select the **Android** platform, then
**Switch to Platform**, then **Build and Run**.

To build and run on iOS, click **File | Build Settings**, select the **iOS** platform,
then **Switch to Platform**, then **Build**. This will export an XCode project and
will display additional instructions on completing the build.

The remainder of this guide assumes you are now attempting to write your
own code to integrate Play Games services into your game.

## ISocialPlatform Compliance

The Google Play Games plugin implements a subset of Unity's [social interface](http://docs.unity3d.com/Documentation/ScriptReference/Social.html), for compatibility 
with games that already use that interface when integrating with other 
platforms. However, some features are unique to Play Games and are 
offered as extensions to the standard social interface provided by Unity. 

The standard API calls can be accessed through the **Social.Active** object, 
which is a reference to an **ISocialPlatform** interface. The non-standard 
Google Play Games extensions can be accessed by casting the **Social.Active** 
object to the **PlayGamesPlatform** class, where the additional methods are 
available. 

## Initialization

To initialize the plugin and make it your default social platform,
call **PlayGamesPlatform.Activate**:

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    
    // recommended for debugging:
    PlayGamesPlatform.DebugLogEnabled = true;
    
    // Activate the Google Play Games platform
    PlayGamesPlatform.Activate();
```

After activated, you can access the Play Games platform through 
**Social.Active**. You should only call **PlayGamesPlatform.Activate** once in 
your application. Making this call will not display anything on the screen and 
will not interact with the user in any way.

## Sign in

To sign in, call **Social.localUser.Authenticate**, which is part of the 
standard Unity social platform interface.

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    // authenticate user:
    Social.localUser.Authenticate((bool success) => {
        // handle success or failure
    });
```

Authentication will show the required consent dialogs. If the user has already 
signed into the game in the past, this process will be silent and the user will 
not have to interact with any dialogs.

Note that you cannot make any games API calls (unlock achievements, post scores, 
etc) until you get a successful return value from **Authenticate**, so it is 
good practice to put up a standby screen until the callback is called, to make 
sure the user can't start playing the game until the authentication process 
completes.

## Revealing/Unlocking an Achievement

To unlock an achievement, use the **Social.ReportProgress** method with a 
progress value of 100.0f:

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    // unlock achievement (achievement ID "Cfjewijawiu_QA")
    Social.ReportProgress("Cfjewijawiu_QA", 100.0f, (bool success) => {
        // handle success or failure
    });
```

Notice that according to the expected behavior of 
[Social.ReportProgress](http://docs.unity3d.com/Documentation/ScriptReference/Social.ReportProgress.html), 
a progress of 0.0f means revealing the achievement and a progress of 100.0f 
means unlocking the achievement. Therefore, to reveal an achievement (that was 
previously hidden) without unlocking it, simply call Social.ReportProgress with 
a progress of 0.0f.

## Incrementing an Achievement

If your achievement is incremental, the Play Games implementation of 
**Social.ReportProgress** will try to behave as closely as possible to the 
expected behavior according to Unity's social API, but may not be exact. For 
this reason, we recommend that you do not use Social.ReportProgress for 
incremental achievements. Instead, use the 
**PlayGamesPlatform.IncrementAchievement** method, which is a Play Games 
extension.

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    // increment achievement (achievement ID "Cfjewijawiu_QA") by 5 steps
    ((PlayGamesPlatform) Social.Active).IncrementAchievement(
        "Cfjewijawiu_QA", 5, (bool success) => {
            // handle success or failure
    });
```

## Posting a Score to a Leaderboard

To post a score to a leaderboard, call **Social.ReportScore**.

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    // post score 12345 to leaderboard ID "Cfji293fjsie_QA")
    Social.ReportScore(12345, "Cfji293fjsie_QA", (bool success) => {
        // handle success or failure
    });
```

Note that the platform and the server will automatically discard scores that are 
lower than the player's existing high score, so you can submit scores freely 
without any checks to test whether or not the score is greater than the player's 
existing score.

## Showing the Achievements UI

To show the built-in UI for all leaderboards, call 
**Social.ShowAchievementsUI**.

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    // show achievements UI
    Social.ShowAchievementsUI();
```

This will show a standard UI appropriate for the look and feel of the platform 
(Android or iOS).

## Showing the Leaderboard UI

To show the built-in UI for all leaderboards, call **Social.ShowLeaderboardUI**.

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    // show leaderboard UI
    Social.ShowLeaderboardUI();
```

If you wish to show a particular leaderboard instead of all leaderboards, you 
can pass a leaderboard ID to the method. This, however, is a Play Games 
extension, so the Social.Active object needs to be cast to a PlayGamesPlatform 
object first:

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    ...
    // show leaderboard UI
    (PlayGamesPlatform) Social.Active).ShowLeaderboardUI("Cfji293fjsie_QA");
```

## Saving Game State to the Cloud

To save game state to the cloud, use the **PlayGamesPlatform.UpdateState** 
method.

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    using GooglePlayGames.BasicApi;
    public class MyClass : OnStateLoadedListener {
        void SaveState() {
            // serialize your game state to a byte array:
            byte[] mySaveState = ...;
            int slot = 0; // slot number to use
            ((PlayGamesPlatform) Social.Active).UpdateState(slot,
                mySaveState, this);
        }
        public void OnStateSaved(bool success, int slot) {
            // handle success or failure
        }
        ...
```

The **OnStateSaved** method of the **OnStateLoadedListener** will be called to 
indicate the success or failure of the cloud save operation.

## Loading Game State from the Cloud

To load game state from the cloud, use the **PlayGamesPlatform.LoadState** 
method:<br/>
 
```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    using GooglePlayGames.BasicApi;
    public class MyClass : OnStateLoadedListener {
        void LoadState() {
            int slot = 0; // slot number to use
            ((PlayGamesPlatform) Social.Active).LoadState(slot, this);
        }
        public void OnStateLoaded(bool success, int slot, byte[] data) {
            if (success) {
                // do something with data[]
            } else {
                // handle failure
            }
        }
        ....
```

## Resolving State Conflicts

A conflict happens when a device attempts to save state to the cloud but the 
data currently on the cloud was written by a different device. When this 
condition occurs, the OnStateConflict method of your OnStateLoadedListener will 
be called, and you choose between (or merge) the two states and return a new 
byte array representing the resolved state:<br/>
 
```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;
    using GooglePlayGames.BasicApi;
    public class MyClass : OnStateLoadedListener {
        public byte[] OnStateConflict(int slot, byte[] local, byte[] server) {
            // resolve conflict and return a byte[] representing the
            // resolved state.
        }
    }
```

## Using the Cloud Cache Encryption Hook

When the user is offline, cloud save data is stored locally on the device for later
synchronization. On Android, this cache is maintained by the Google Play Services
application. However, on iOS, the cache is implemented by the plugin itself by
writing to files. If you are concerned that a malicious user might try to tamper with
those cache files, you can install an encryption/decryption hook to provide code
that the plugin will use to encrypt the buffer before it is written to disk and
decrypt it once it is read from disk.

**Important:** the encryption/decryption hook will be called only when
saving files to local disk on operating systems where this feature is not
already provided by the underlying library (currently, only iOS). Data
will **not** be encrypted using this method when saved to the server.

````csharp
    // Define our encrypter/decrypter method:
    byte[] MyEncrypter(bool encrypt, byte[] data) {
        if (encrypt) {
            // data[] is the plain data that we must encrypt.
            byte[] encryptedData = ....; // your encryption code here
            return encryptedData;
        } else {
            // data[] is the encrypted data that we must decrypt.
            byte[] plainData = ....; // your decryption code here
            return plainData;
        }
    }

    // Set the cloud cache encrypter/decrypter
    // (Do this on your initialization, before attempting to sign in,
    // but after calling PlayGamesPlatform.Activate())
    ((PlayGamesPlatform) Social.Active).SetCloudCacheEncrypter(MyEncrypter);
````

## Sign out

To sign the user out, use the **PlayGamesPlatform.SignOut** method.

```csharp
    using GooglePlayGames;
    using UnityEngine.SocialPlatforms;

    // sign out
    ((PlayGamesPlatform) Social.Active).SignOut();
```

After signing out, no further API calls can be made until the user authenticates again.


## Building for Android

To build your game for Android, do as you would normally do in Unity. Select 
**File | Build Settings**, then select the **Android** platform and build. If 
you are signing your APK file, please make sure that you are signing it with the 
correct certificate, that is, the one that corresponds to the SHA1 certificate 
fingerprint you entered in the Developer Console during the setup.

## Building for iOS

To build your game for iOS, do as you would normally do in Unity. Select **File | Build Settings**, then select the **iOS** platform. However, do not choose 
**Build & Run**. Instead, choose only **Build**, and select an output directory 
to save the XCode project.

After building the XCode project, the Play Games postprocessor will run to 
configure the Info.plist settings on your project. You will see a log of the 
operation, which if successful, will give you additional instructions to finish 
configuring your XCode project.

The additional steps are:

1. Add these frameworks. To do this, click the top-level project (the item on the
list labeled **Unity-iPhone, 1 target, iOS SDK**), then click the **Build Phases**
tab and expand the **Link Binary with Libraries** item. Then, add the following
frameworks to that list:
<br/><br/>
**AddressBook.framework**<br/>
**AssetsLibrary.framework**<br/>
**CoreData**<br/>
**CoreLocation**<br/>
**CoreMotion**<br/>
**CoreText**<br/>
**MediaPlayer**<br/>
**QuartzCore**<br/>
**Security**<br/>
**SystemConfiguration**<br/><br/>
2. Add the following bundles and frameworks from the Google Plus and Google Play 
   Games SDK that you have previously downloaded. If you have not downloaded 
   these files yet, they can be found [in the downloads section](https://developers.google.com/games/services/downloads) of the Google Play Games developer site. To add these frameworks you can simply drag
and drop those 5 files on the top-level project item (labeled **Unity-iPhone**).<br/><br/>
   	**GoogleOpenSource.framework**<br/>
   	**GooglePlus.bundle**<br/>
   	**GooglePlus.framework**<br/>
   	**PlayGameServices.bundle**<br/>
   	**PlayGameServices.framework**<br/><br/>
3. Add the **"-ObjC"** linker flag. To do this, select the top-level project 
   object, then go to the **Build Settings**
   tab. Search for **"Other Linker Flags"** using the search tool, double click
   the **Other Linker Flags** item and add **"-ObjC"** to that list (attention to case!).

**Note:** If you export the project a second time to the same XCode project 
directory, you can use Unity's **Append** option to avoid overwriting these 
settings. If you use **Replace**, however, you might have to reapply some settings.

## Building for iOS to run on the simulator

To run your game in the simulator as opposed to a real device, you must export 
it from Unity with the "Simulator SDK" instead of the "Device SDK". To do this, 
open your game project in Unity, select **File | Build Settings**, select iOS, 
then click on **Player Settings**. Scroll down to find the **"SDK Version"** 
option, and change it to **"Simulator SDK".**

Then, export your project and perform the post-export steps as described above.

Next, you must manually enable some API entry points that are used by the 
plugin, but which are by default disabled in the Unity runtime code. To do 
this, open the **Libraries/RegisterMonoModule.cpp** file in your exported project.

You will notice there are two sections near the top of the file that<br/>
are delimited by **#if !(TARGET_IPHONE_SIMULATOR)** and** #endif**.

```c
    extern "C" {
        ....
        #if !(TARGET_IPHONE_SIMULATOR)
            ...declarations... (zone A)
        #endif
    }
        
    void RegisterMonoModules()
    {
        ...
        #if !(TARGET_IPHONE_SIMULATOR)
            ...function calls... (zone B)
        #endif
    }
```

 <br/>
To enable the simulator to make the necessary C function calls, you need to:

1. Find the line that declares the **mono_dl_register_symbol()** function in 
   **Zone A**.
2. Move it to the outside of **Zone A**, right after the **#endif** (but still 
   inside the **extern "C"** block)
3. Find ALL the calls to **mono_dl_register_symbol()** in **Zone B**.
4. Move them ALL outside **Zone B**, after the **#endif** (but still inside the 
   **RegisterMonoModules()** function).

The final structure should be similar to the following:

```c
    extern "C" {
        ....
        #if !(TARGET_IPHONE_SIMULATOR)
            .. declarations .. (zone A)
        #endif
        void mono_dl_register_symbol(const char* name, void *addr);
    }
        
    void RegisterMonoModules()
    {
        ...
        #if !(TARGET_IPHONE_SIMULATOR)
            ...function calls... (zone B)
        #endif
        mono_dl_register_symbol("GPGSFooBar1", (void*)&GPGSFooBar1);
        mono_dl_register_symbol("GPGSFooBar2", (void*)&GPGSFooBar2);
        mono_dl_register_symbol("GPGSFooBar3", (void*)&GPGSFooBar3);
        mono_dl_register_symbol("GPGSFooBar4", (void*)&GPGSFooBar4);
        ....
    }
```

**Note:** If you are using the current version of Unity (4.2), re-exporting to an existing 
XCode project path will overwrite **Libraries/RegisterMonoModule.cpp**, even if 
you use the **Append** option. Therefore, you must perform these changes every time 
you export the project. To simplify your workflow, consider copying the files to 
a different location before re-exporting, and copy them back after the process 
is complete.

## (Advanced) Using the Plugin Without Overriding the Default Social Platform

When you call `PlayGamesPlatform.Activate`, Google Play Games becomes your default social platform implementation, which means that static calls to methods in `Social` and `Social.Active` will be carried out by the Google Play Games plugin. This is the desired behavior for most games using the plugin.

However, if for some reason you wish to keep the default implementation accessible (for example, to use it to submit achievements and leaderboards to a different social platform), you can use the Google Play Games plugin without overriding the default one. To do this:

1. Do not call `PlayGamesPlatform.Activate`
2. If `Xyz` is the name of a method you wish to call on the `Social` class, do not call `Social.Xyz`. Instead, call `PlayGamesPlatform.Instance.Xyz`
3. Do not use `Social.Active` when interacting with Google Play Games. Instead, use `PlayGamesPlatform.Instance`.

That way, you can even submit scores and achievements simultaneously to two or more social platforms:

    // Submit achievement to original default social platform
    Social.ReportProgress("MyAchievementIdHere", 100.0f, callback);
    
    // Submit achievement to Google Play
    PlayGamesPlatform.Instance.ReportProgress("MyGooglePlayAchievementIdHere", 100.0f, callback);

## Note from Maintainer

The plugin was developed and is currently maintained by [Bruno Oliveira](https://plus.google.com/+BrunoOliveira). Feel free to add me on Google+ and nag me to fix bugs or take a look at your questions on Stack Overflow!

## Special Thanks

This section lists people who have contributed to this project by writing code, improving documentation or fixing bugs.

* [Dgizusse](https://github.com/Dgizusse) for figuring out that setting JAVA_HOME is necessary on Windows.
* [antonlicht](https://github.com/antonlicht) for fixing a bug with the parameter type of showErrorDialog on the support library.

