# Developer Cheat Sheet #

Want to make your own fixes / adjustments / additions to the Play Games Plug-in? Here's how!

## Where is everything? ##

You can find most of the cs files you'll want to touch in the `source/PluginDev/Assets/GooglePlayGames` directory. Most of the files that do the work interacting with Unity are in the `BasicApi` subdirectory. The `iSocialPlatform` directory contains a nice wrapper to ensure that the Play Games objects can be used via the iSocial interface. The `Platforms/Android` and `Platforms/iOS `subdirectory contain a bunch of "wrapper" code that interacts with the native Java / Obj-C libraries specifically.

If you're looking for the code that does the work on the native side of things, you can find it in the source/PluginDev/Assets/Plugins/Android or iOS directory.

## Testing ##

From Unity, open up source/PluginDev as a project.
Build it to your target platform.

On iOS, it's common to do much of your objective-c development within the exported Xcode project itself. Just remember when you're done to copy those .m and .h file back to the source/PluginDev/Assets/Plugins/iOS directory when you're done, or they'll be lost for good. You might consider setting up a bash script that includes the following lines:

<pre>
rsync -av &lt;test game directory&gt;/Libraries/GPGS*.m source/PluginDev/Assets/Plugins/iOS/
rsync -av &lt;test game directory&gt;/Libraries/GPGS*.h source/PluginDev/Assets/Plugins/iOS/
</pre>

**TODO:** Add any necessary tips here on developing for the Android side of things.

## Building ##

When you're done testing, you should remember to build your project by going back to Unity and selecting **GooglePlayGames -> Export Plugin Package...** Export the newest package into the current-build folder.

## Submitting ##

Before submitting a pull request, you must fill out a [Google Individual Contributor License Agreement](https://developers.google.com/open-source/cla/individual). You only need to do this once, and for your convenience there's an electronic version of the form at the bottom of the page. (Sorry; we dislike red tape as much as you, but it's something you gotta do.)
