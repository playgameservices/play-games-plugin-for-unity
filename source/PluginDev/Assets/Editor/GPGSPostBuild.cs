using System;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using GooglePlayGames;
using GooglePlayGames.Editor;
using System.Text.RegularExpressions;


public static class GPGSPostBuild {

    private const string UrlTypes = "CFBundleURLTypes";
    private const string UrlBundleName = "CFBundleURLName";
    private const string UrlScheme = "CFBundleURLSchemes";

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        if (target != BuildTarget.iPhone) {
            return;
        }

        if (GetBundleId() == null) {
            UnityEngine.Debug.LogError("The iOS bundle ID has not been set up through the " +
            "'iOS Setup' submenu of 'Google Play Games' - the generated xcode project will " +
            "not work properly.");
            return;
        }

        UnityEngine.Debug.Log("Adding URL Types for authentication using PlistBuddy.");

        UpdateGeneratedInfoPlistFile(pathToBuiltProject + "/Info.plist");
        UpdateGeneratedPbxproj(pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj");

        EditorWindow.GetWindow<GPGSInstructionWindow>(
            utility: true,
            title: "Building for IOS",
            focus: true);
    }

    /// <summary>
    /// Updates the new project's Info.plist file to include an entry for the Url scheme mandated
    /// by the Google+ login. This means that the plist file needs to have an entry in the for
    /// indicated here: <see cref="https://developers.google.com/+/mobile/ios/getting-started#step_3_add_a_url_type"/>
    /// <para>This boils down to having an entry in the CFBundleURLTypes top level plist field with
    /// a CFBundleURLName equal to the bundle ID of the game, and a single element array for
    /// CFBundleURLSchemes also containing the bundle ID.</para>
    /// <para>We make use of the apple-provided PlistBuddy utility to edit the plist file.</para>
    /// </summary>
    /// <param name="pathToPlist">Path to plist.</param>
    private static void UpdateGeneratedInfoPlistFile(string pathToPlist) {
        PlistBuddyHelper buddy = PlistBuddyHelper.ForPlistFile(pathToPlist);

        // If the top-level UrlTypes field doesn't exist, add it here.
        if (buddy.EntryValue(UrlTypes) == null) {
            buddy.AddArray(UrlTypes);
        }

        var gamesSchemeIndex = GamesUrlSchemeIndex(buddy);

        EnsureGamesUrlScheme(buddy, gamesSchemeIndex);
    }


    /// <summary>
    /// Ensures the games URL scheme is well formed. This is done by removing the UrlScheme field
    /// and adding a fresh one in a known-good state.
    /// </summary>
    /// <param name="buddy">Buddy.</param>
    /// <param name="index">Index.</param>
    private static void EnsureGamesUrlScheme(PlistBuddyHelper buddy, int index) {
        buddy.RemoveEntry(UrlTypes, index, UrlScheme);
        buddy.AddArray(UrlTypes, index, UrlScheme);
        buddy.AddString(PlistBuddyHelper.ToEntryName(UrlTypes, index, UrlScheme, 0),
            GetBundleId());
    }

    private static string GetBundleId() {
        return GPGSProjectSettings.Instance.Get("ios.BundleId", null);
    }


    /// <summary>
    /// Finds the index of the CFBundleURLTypes array where the entry for Play Games is stored. If
    /// this is not present, a new entry will be appended to the end of this array.
    /// </summary>
    /// <returns>The index in the CFBundleURLTypes array corresponding to Play Games.</returns>
    /// <param name="buddy">The helper corresponding to the plist file.</param>
    private static int GamesUrlSchemeIndex(PlistBuddyHelper buddy) {
        int index = 0;

        while(buddy.EntryValue(UrlTypes, index) != null) {
            var urlName = buddy.EntryValue(UrlTypes, index, UrlBundleName);

            if (GetBundleId().Equals(urlName)) {
                return index;
            }

            index++;
        }

        // The current array does not contain the Games url scheme - add a value to the end.
        buddy.AddDictionary(UrlTypes, index);
        buddy.AddString(PlistBuddyHelper.ToEntryName(UrlTypes, index, UrlBundleName),
            GetBundleId());

        return index;
    }

    /// <summary>
    /// Updates the generated pbxproj to reduce manual work required by developers. Currently
    /// this just adds the '-fobjc-arc' flag for the Play Games ObjC source files.
    /// </summary>
    /// <param name="pbxprojPath">Pbxproj path.</param>
    private static void UpdateGeneratedPbxproj(string pbxprojPath) {
        // We're looking for lines in the form:
        // ... = {isa = PBXBuildFile; fileRef = DEADBEEF /* GPGSFileName.mm */; };
        // And we want to append "settings = {COMPILER_FLAGS = "-fobjc-arc"};" to the content in
        // between the braces. This is done with a regex replace.
        // The expression is structured as follows:
        // - Begin a capturing group.
        // - Find any line that begins with "<anything>{isa = PBXBuildFile" followed by a
        //   reference to a file beginning with "GPGS" and ending with ".m" (e.g. "GPGSFile.m")
        // - Close the capture group - leaving a trailing "};"
        // - Replace that line with the captured group with
        //   "settings = {COMPILER_FLAGS = "-fobjc-arc";};};" appended. The trailing "};" is needed
        //   because we omitted the "};" from the group.
        var withFlagAdded = Regex.Replace(
                                GPGSUtil.ReadFully(pbxprojPath),
                                @"(.*\{isa\s*=\s*PBXBuildFile.*GPGS\w*\.m.*)\}\;",
                                @"$1settings = {COMPILER_FLAGS = ""-fobjc-arc""; }; };");

        // Overwrite the pbxproj with the updated value.
        GPGSUtil.WriteFile(pbxprojPath, withFlagAdded);
    }
}
