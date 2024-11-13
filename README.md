# Google Play Games plugin for Unity
_Copyright (c) 2014 Google Inc. All rights reserved._

The Google Play Games plugin for Unity&reg; is an open-source project whose goal
is to provide a plugin that allows game developers to integrate with
the Google Play Games API from a game written in Unity&reg;. However, this project is
not in any way endorsed or supervised by Unity Technologies.

_Unity&reg; is a trademark of Unity Technologies._

_iOS is a trademark of Apple, Inc._

## Downloads

Please download the package from the [Github release page](https://github.com/playgameservices/play-games-plugin-for-unity/releases).

## Overview

The Google Play Games plugin for Unity allows you to access the Google Play Games
API through Unity's [social interface](http://docs.unity3d.com/Documentation/ScriptReference/Social.html).
The plugin provides support for the
following features of the Google Play Games API:<br/>

* sign in
* friends
* unlock/reveal/increment achievement
* post score to leaderboard
* cloud save read/write
* show built-in achievement/leaderboards UI
* events
* [nearby connections](NEARBY.md)

__NOTICE__: This version of the plugin no longer supports iOS.  Google Play games services for iOS is deprecated,
and is not likely to function as expected. Do not use Google Play games
services for iOS in new apps. See the [deprecation announcement](https://android-developers.googleblog.com/2017/04/focusing-our-google-play-games-services.html) blog post for more details.

Features:

* easy GUI-oriented project setup (integrated into the Unity GUI)
* no need to override/customize the player Activity
* no need to override/customize AndroidManifest.xml

## Documentation

For instructions on using the plugin, please refer to [this developer guide](https://developer.android.com/games/pgs/unity/unity-start).

For a sample application demonstrating how to use Google Play Games see [SmokeTest](https://github.com/playgameservices/play-games-plugin-for-unity/tree/master/Samples/SmokeTest) project;


## Upgrading

If you have already integrated your project with a previous version of the
plugin and wish to upgrade to a new version, please refer to the
[upgrade instructions](UPGRADING.txt).

## Retrieving server authentication codes ##
In order to access Google APIs on a backend web server on behalf of the current
player, you need to get an authentication code from the client application and
pass this to your web server application.  This code can then be exchanged for
an access token to make calls to the various APIs.
For more details on this flow see: [Google Sign-In for Websites](https://developers.google.com/identity/sign-in/web/server-side-flow).

To get the Server Auth code:
1. Configure the web client Id of the web application linked to your game in the
   Play Game Console.
2. Call `PlayGamesClientConfiguration.Builder.RequestServerAuthCode(false)` when
   creating the configuration.
3. Call `PlayGamesPlatform.Instance.GetServerAuthCode()` once the player is authenticated.
4. Pass this code to your server application.

```csharp
  PlayGamesPlatform.Instance.RequestServerSideAccess(
    /* forceRefreshToken= */ false,
    code -> {
      // send code to server
    });
```

## Decreasing apk size

It is possible to decrease the size of the Play Games Services Unity Plugin by removing code for the Play Games Services features that your game doesn’t use by using Proguard. Proguard will remove the Play Games Unity plugin code for features that are not used in your game, so your game ships with only the code that is needed and minimizes the size impact of using Play Games Services.

Additionally, it is possible to reduce the size of the entire Unity project using Unity’s [Managed Code Stripping](https://docs.unity3d.com/Manual/ManagedCodeStripping.html), which will compress your entire project.  This can be used in conjunction with Proguard.

### Play Games Services Proguard configuration

1. Go to `File > Build Settings > Player Settings` and click `Publishing Settings` section. Choose `Proguard` for `Minify > Release`. Then, enable `User Proguard File`. If you want the plugin to be proguarded for debug apks as well, you can choose `Proguard` for `Minify > Debug`.
2. Copy the content of [the proguard configuration](scripts/proguard.txt) into `Assets/Plugins/Android/proguard-user.txt`.

## (Advanced) Using the Plugin Without Overriding the Default Social Platform

When you call `PlayGamesPlatform.Activate`, Google Play Games becomes your default social platform implementation, which means that static calls to methods in `Social` and `Social.Active` will be carried out by the Google Play Games plugin. This is the desired behavior for most games using the plugin.

However, if for some reason you wish to keep the default implementation accessible (for example, to use it to submit achievements and leaderboards to a different social platform), you can use the Google Play Games plugin without overriding the default one. To do this:

1. Do not call `PlayGamesPlatform.Activate`
2. If `Xyz` is the name of a method you wish to call on the `Social` class, do not call `Social.Xyz`. Instead, call `PlayGamesPlatform.Instance.Xyz`
3. Do not use `Social.Active` when interacting with Google Play Games. Instead, use `PlayGamesPlatform.Instance`.

That way, you can even submit scores and achievements simultaneously to two or more social platforms:

```csharp
    // Submit achievement to original default social platform
    Social.ReportProgress("MyAchievementIdHere", 100.0f, callback);

    // Submit achievement to Google Play
    PlayGamesPlatform.Instance.ReportProgress("MyGooglePlayAchievementIdHere", 100.0f, callback);
```

## Special Thanks

This section lists people who have contributed to this project by writing code, improving documentation or fixing bugs.

* [Dgizusse](https://github.com/Dgizusse) for figuring out that setting JAVA_HOME is necessary on Windows.
* [antonlicht](https://github.com/antonlicht) for fixing a bug with the parameter type of showErrorDialog on the support library.
* [pR0Ps](https://github.com/pR0Ps) for fixing an issue where OnAchievementsLoaded was not accepting an OPERATION_DEFERRED result code as a success.
* [friikyeu](https://github.com/friikyeu) for helping debug [an issue](https://github.com/playgameservices/play-games-plugin-for-unity/issues/25) that caused API calls to be queued up rather than executed even when connected.
