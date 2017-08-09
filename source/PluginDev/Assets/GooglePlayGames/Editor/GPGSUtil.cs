// <copyright file="GPGSUtil.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// Keep this even on unsupported configurations.

namespace GooglePlayGames.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Utility class to perform various tasks in the editor.
    /// </summary>
    public static class GPGSUtil
    {
        /// <summary>Property key for project settings.</summary>
        public const string SERVICEIDKEY = "App.NearbdServiceId";

        /// <summary>Property key for project settings.</summary>
        public const string APPIDKEY = "proj.AppId";

        /// <summary>Property key for project settings.</summary>
        public const string CLASSDIRECTORYKEY = "proj.classDir";

        /// <summary>Property key for project settings.</summary>
        public const string CLASSNAMEKEY = "proj.ConstantsClassName";

        /// <summary>Property key for project settings.</summary>
        public const string WEBCLIENTIDKEY = "and.ClientId";

        /// <summary>Property key for project settings.</summary>
        public const string IOSCLIENTIDKEY = "ios.ClientId";

        /// <summary>Property key for project settings.</summary>
        public const string IOSBUNDLEIDKEY = "ios.BundleId";

        /// <summary>Property key for project settings.</summary>
        public const string ANDROIDRESOURCEKEY = "and.ResourceData";

        /// <summary>Property key for project settings.</summary>
        public const string ANDROIDSETUPDONEKEY = "android.SetupDone";

        /// <summary>Property key for project settings.</summary>
        public const string ANDROIDBUNDLEIDKEY = "and.BundleId";

        /// <summary>Property key for project settings.</summary>
        public const string IOSRESOURCEKEY = "ios.ResourceData";

        /// <summary>Property key for project settings.</summary>
        public const string IOSSETUPDONEKEY = "ios.SetupDone";

        /// <summary>Property key for plugin version.</summary>
        public const string PLUGINVERSIONKEY = "proj.pluginVersion";

        /// <summary>Property key for nearby settings done.</summary>
        public const string NEARBYSETUPDONEKEY = "android.NearbySetupDone";

        /// <summary>Property key for project settings.</summary>
        public const string LASTUPGRADEKEY = "lastUpgrade";

        /// <summary>Constant for token replacement</summary>
        private const string SERVICEIDPLACEHOLDER = "__NEARBY_SERVICE_ID__";

        /// <summary>Constant for token replacement</summary>
        private const string APPIDPLACEHOLDER = "__APP_ID__";

        /// <summary>Constant for token replacement</summary>
        private const string CLASSNAMEPLACEHOLDER = "__Class__";

        /// <summary>Constant for token replacement</summary>
        private const string WEBCLIENTIDPLACEHOLDER = "__WEB_CLIENTID__";

        /// <summary>Constant for token replacement</summary>
        private const string IOSCLIENTIDPLACEHOLDER = "__IOS_CLIENTID__";

        /// <summary>Constant for token replacement</summary>
        private const string IOSBUNDLEIDPLACEHOLDER = "__BUNDLEID__";

        /// <summary>Constant for token replacement</summary>
        private const string PLUGINVERSIONPLACEHOLDER = "__PLUGIN_VERSION__";

        /// <summary>Constant for require google plus token replacement</summary>
        private const string REQUIREGOOGLEPLUSPLACEHOLDER = "__REQUIRE_GOOGLE_PLUS__";

        /// <summary>Property key for project settings.</summary>
        private const string TOKENPERMISSIONKEY = "proj.tokenPermissions";

        /// <summary>Constant for token replacement</summary>
        private const string NAMESPACESTARTPLACEHOLDER = "__NameSpaceStart__";

        /// <summary>Constant for token replacement</summary>
        private const string NAMESPACEENDPLACEHOLDER = "__NameSpaceEnd__";

        /// <summary>Constant for token replacement</summary>
        private const string CONSTANTSPLACEHOLDER = "__Constant_Properties__";

        /// <summary>
        /// The game info file path.  This is a generated file.
        /// </summary>
        private const string GameInfoPath = "Assets/GooglePlayGames/GameInfo.cs";

        /// <summary>
        /// The map of replacements for filling in code templates.  The
        /// key is the string that appears in the template as a placeholder,
        /// the value is the key into the GPGSProjectSettings.
        /// </summary>
        private static Dictionary<string, string> replacements =
            new Dictionary<string, string>()
            {
                { SERVICEIDPLACEHOLDER, SERVICEIDKEY },
                { APPIDPLACEHOLDER, APPIDKEY },
                { CLASSNAMEPLACEHOLDER, CLASSNAMEKEY },
                { WEBCLIENTIDPLACEHOLDER, WEBCLIENTIDKEY },
                { IOSCLIENTIDPLACEHOLDER, IOSCLIENTIDKEY },
                { IOSBUNDLEIDPLACEHOLDER, IOSBUNDLEIDKEY },
                { PLUGINVERSIONPLACEHOLDER, PLUGINVERSIONKEY}
            };

        /// <summary>
        /// Replaces / in file path to be the os specific separator.
        /// </summary>
        /// <returns>The path.</returns>
        /// <param name="path">Path with correct separators.</param>
        public static string SlashesToPlatformSeparator(string path)
        {
            return path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <returns>The file contents.  The slashes are corrected.</returns>
        /// <param name="filePath">File path.</param>
        public static string ReadFile(string filePath)
        {
            filePath = SlashesToPlatformSeparator(filePath);
            if (!File.Exists(filePath))
            {
                Alert("Plugin error: file not found: " + filePath);
                return null;
            }

            StreamReader sr = new StreamReader(filePath);
            string body = sr.ReadToEnd();
            sr.Close();
            return body;
        }

        /// <summary>
        /// Reads the editor template.
        /// </summary>
        /// <returns>The editor template contents.</returns>
        /// <param name="name">Name of the template in the editor directory.</param>
        public static string ReadEditorTemplate(string name)
        {
            return ReadFile(SlashesToPlatformSeparator("Assets/GooglePlayGames/Editor/" + name + ".txt"));
        }

        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="file">File path - the slashes will be corrected.</param>
        /// <param name="body">Body of the file to write.</param>
        public static void WriteFile(string file, string body)
        {
            file = SlashesToPlatformSeparator(file);
            using (var wr = new StreamWriter(file, false))
            {
                wr.Write(body);
            }
        }

        /// <summary>
        /// Validates the string to be a valid nearby service id.
        /// </summary>
        /// <returns><c>true</c>, if like valid service identifier was looksed, <c>false</c> otherwise.</returns>
        /// <param name="s">string to test.</param>
        public static bool LooksLikeValidServiceId(string s)
        {
            if (s.Length < 3)
            {
                return false;
            }

            foreach (char c in s)
            {
                if (!char.IsLetterOrDigit(c) && c != '.')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Looks the like valid app identifier.
        /// </summary>
        /// <returns><c>true</c>, if valid app identifier, <c>false</c> otherwise.</returns>
        /// <param name="s">the string to test.</param>
        public static bool LooksLikeValidAppId(string s)
        {
            if (s.Length < 5)
            {
                return false;
            }

            foreach (char c in s)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Looks the like valid client identifier.
        /// </summary>
        /// <returns><c>true</c>, if valid client identifier, <c>false</c> otherwise.</returns>
        /// <param name="s">the string to test.</param>
        public static bool LooksLikeValidClientId(string s)
        {
            return s.EndsWith(".googleusercontent.com");
        }

        /// <summary>
        /// Looks the like a valid bundle identifier.
        /// </summary>
        /// <returns><c>true</c>, if valid bundle identifier, <c>false</c> otherwise.</returns>
        /// <param name="s">the string to test.</param>
        public static bool LooksLikeValidBundleId(string s)
        {
            return s.Length > 3;
        }

        /// <summary>
        /// Looks like a valid package.
        /// </summary>
        /// <returns><c>true</c>, if  valid package name, <c>false</c> otherwise.</returns>
        /// <param name="s">the string to test.</param>
        public static bool LooksLikeValidPackageName(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("cannot be empty");
            }

            string[] parts = s.Split(new char[] { '.' });
            foreach (string p in parts)
            {
                char[] bytes = p.ToCharArray();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i == 0 && !char.IsLetter(bytes[i]))
                    {
                        throw new Exception("each part must start with a letter");
                    }
                    else if (char.IsWhiteSpace(bytes[i]))
                    {
                        throw new Exception("cannot contain spaces");
                    }
                    else if (!char.IsLetterOrDigit(bytes[i]) && bytes[i] != '_')
                    {
                        throw new Exception("must be alphanumeric or _");
                    }
                }
            }

            return parts.Length >= 1;
        }

        /// <summary>
        /// Determines if is setup done.
        /// </summary>
        /// <returns><c>true</c> if is setup done; otherwise, <c>false</c>.</returns>
        public static bool IsSetupDone()
        {
            bool doneSetup = true;
            #if UNITY_ANDROID
            doneSetup = GPGSProjectSettings.Instance.GetBool(ANDROIDSETUPDONEKEY, false);
            // check gameinfo
            if (File.Exists(GameInfoPath))
            {
                string contents = ReadFile(GameInfoPath);
                if (contents.Contains(APPIDPLACEHOLDER))
                {
                    Debug.Log("GameInfo not initialized with AppId.  " +
                        "Run Window > Google Play Games > Setup > Android Setup...");
                    return false;
                }
            }
            else
            {
                Debug.Log("GameInfo.cs does not exist.  Run Window > Google Play Games > Setup > Android Setup...");
                return false;
            }
            #elif (UNITY_IPHONE && !NO_GPGS)
            doneSetup = GPGSProjectSettings.Instance.GetBool(IOSSETUPDONEKEY, false);
            // check gameinfo
            if (File.Exists(GameInfoPath))
            {
                string contents = ReadFile(GameInfoPath);
                if (contents.Contains(IOSCLIENTIDPLACEHOLDER))
                {
                    Debug.Log("GameInfo not initialized with Client Id.  " +
                        "Run Window > Google Play Games > Setup > iOS Setup...");
                    return false;
                }
            }
            else
            {
                Debug.Log("GameInfo.cs does not exist.  Run Window > Google Play Games > Setup > iOS Setup...");
                return false;
            }

            #endif

            return doneSetup;
        }

        /// <summary>
        /// Makes legal identifier from string.
        /// Returns a legal C# identifier from the given string.  The transformations are:
        ///   - spaces => underscore _
        ///   - punctuation => empty string
        ///   - leading numbers are prefixed with underscore.
        /// </summary>
        /// <returns>the id</returns>
        /// <param name="key">Key to convert to an identifier.</param>
        public static string MakeIdentifier(string key)
        {
            string s;
            string retval = string.Empty;
            if (string.IsNullOrEmpty(key))
            {
                return "_";
            }

            s = key.Trim().Replace(' ', '_');

            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    retval += c;
                }
            }

            return retval;
        }

        /// <summary>
        /// Displays an error dialog.
        /// </summary>
        /// <param name="s">the message</param>
        public static void Alert(string s)
        {
            Alert(GPGSStrings.Error, s);
        }

        /// <summary>
        /// Displays a dialog with the given title and message.
        /// </summary>
        /// <param name="title">the title.</param>
        /// <param name="message">the message.</param>
        public static void Alert(string title, string message)
        {
            EditorUtility.DisplayDialog(title, message, GPGSStrings.Ok);
        }

        /// <summary>
        /// Gets the android sdk path.
        /// </summary>
        /// <returns>The android sdk path.</returns>
        public static string GetAndroidSdkPath()
        {
            string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            if (sdkPath != null && (sdkPath.EndsWith("/") || sdkPath.EndsWith("\\")))
            {
                sdkPath = sdkPath.Substring(0, sdkPath.Length - 1);
            }

            return sdkPath;
        }

        /// <summary>
        /// Determines if the android sdk exists.
        /// </summary>
        /// <returns><c>true</c> if  android sdk exists; otherwise, <c>false</c>.</returns>
        public static bool HasAndroidSdk()
        {
            string sdkPath = GetAndroidSdkPath();
            return sdkPath != null && sdkPath.Trim() != string.Empty && System.IO.Directory.Exists(sdkPath);
        }

        /// <summary>
        /// Gets the unity major version.
        /// </summary>
        /// <returns>The unity major version.</returns>
        public static int GetUnityMajorVersion()
        {
#if UNITY_5
            string majorVersion = Application.unityVersion.Split('.')[0];
            int ver;
            if (!int.TryParse(majorVersion, out ver))
            {
                ver = 0;
            }

            return ver;
#elif UNITY_4_6
            return 4;
#else
            return 0;
#endif

        }

        /// <summary>
        /// Checks for the android manifest file exsistance.
        /// </summary>
        /// <returns><c>true</c>, if the file exists <c>false</c> otherwise.</returns>
        public static bool AndroidManifestExists()
        {
            string destFilename = GPGSUtil.SlashesToPlatformSeparator(
                                      "Assets/Plugins/Android/MainLibProj/AndroidManifest.xml");

            return File.Exists(destFilename);
        }

        /// <summary>
        /// Generates the android manifest.
        /// </summary>
        public static void GenerateAndroidManifest()
        {
            string destFilename = GPGSUtil.SlashesToPlatformSeparator(
                                      "Assets/Plugins/Android/MainLibProj/AndroidManifest.xml");

            // Generate AndroidManifest.xml
            string manifestBody = GPGSUtil.ReadEditorTemplate("template-AndroidManifest");

            Dictionary<string, string> overrideValues =
                new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> ent in replacements)
            {
                string value =
                    GPGSProjectSettings.Instance.Get(ent.Value, overrideValues);
                manifestBody = manifestBody.Replace(ent.Key, value);
            }

            GPGSUtil.WriteFile(destFilename, manifestBody);
            GPGSUtil.UpdateGameInfo();
        }

        /// <summary>
        /// Writes the resource identifiers file.  This file contains the
        /// resource ids copied (downloaded?) from the play game app console.
        /// </summary>
        /// <param name="classDirectory">Class directory.</param>
        /// <param name="className">Class name.</param>
        /// <param name="resourceKeys">Resource keys.</param>
        public static void WriteResourceIds(string classDirectory, string className, Hashtable resourceKeys)
        {
            string constantsValues = string.Empty;
            string[] parts = className.Split('.');
            string dirName = classDirectory;
            if (string.IsNullOrEmpty(dirName))
            {
                dirName = "Assets";
            }

            string nameSpace = string.Empty;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                dirName += "/" + parts[i];
                if (nameSpace != string.Empty)
                {
                    nameSpace += ".";
                }

                nameSpace += parts[i];
            }

            EnsureDirExists(dirName);
            foreach (DictionaryEntry ent in resourceKeys)
            {
                string key = MakeIdentifier((string)ent.Key);
                constantsValues += "        public const string " +
                key + " = \"" + ent.Value + "\"; // <GPGSID>\n";
            }

            string fileBody = GPGSUtil.ReadEditorTemplate("template-Constants");
            if (nameSpace != string.Empty)
            {
                fileBody = fileBody.Replace(
                    NAMESPACESTARTPLACEHOLDER,
                    "namespace " + nameSpace + "\n{");
            }
            else
            {
                fileBody = fileBody.Replace(NAMESPACESTARTPLACEHOLDER, string.Empty);
            }

            fileBody = fileBody.Replace(CLASSNAMEPLACEHOLDER, parts[parts.Length - 1]);
            fileBody = fileBody.Replace(CONSTANTSPLACEHOLDER, constantsValues);
            if (nameSpace != string.Empty)
            {
                fileBody = fileBody.Replace(
                    NAMESPACEENDPLACEHOLDER,
                    "}");
            }
            else
            {
                fileBody = fileBody.Replace(NAMESPACEENDPLACEHOLDER, string.Empty);
            }

            WriteFile(Path.Combine(dirName, parts[parts.Length - 1] + ".cs"), fileBody);
        }

        /// <summary>
        /// Updates the game info file.  This is a generated file containing the
        /// app and client ids.
        /// </summary>
        public static void UpdateGameInfo()
        {
            string fileBody = GPGSUtil.ReadEditorTemplate("template-GameInfo");

            foreach (KeyValuePair<string, string> ent in replacements)
            {
                string value =
                    GPGSProjectSettings.Instance.Get(ent.Value);
                fileBody = fileBody.Replace(ent.Key, value);
            }

            GPGSUtil.WriteFile(GameInfoPath, fileBody);
        }

        /// <summary>
        /// Ensures the dir exists.
        /// </summary>
        /// <param name="dir">Directory to check.</param>
        public static void EnsureDirExists(string dir)
        {
            dir = SlashesToPlatformSeparator(dir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// Deletes the dir if exists.
        /// </summary>
        /// <param name="dir">Directory to delete.</param>
        public static void DeleteDirIfExists(string dir)
        {
            dir = SlashesToPlatformSeparator(dir);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Gets the Google Play Services library version.  This is only
        /// needed for Unity versions less than 5.
        /// </summary>
        /// <returns>The GPS version.</returns>
        /// <param name="libProjPath">Lib proj path.</param>
        private static int GetGPSVersion(string libProjPath)
        {
            string versionFile = libProjPath + "/res/values/version.xml";

            XmlTextReader reader = new XmlTextReader(new StreamReader(versionFile));
            bool inResource = false;
            int version = -1;

            while (reader.Read())
            {
                if (reader.Name == "resources")
                {
                    inResource = true;
                }

                if (inResource && reader.Name == "integer")
                {
                    if ("google_play_services_version".Equals(
                            reader.GetAttribute("name")))
                    {
                        reader.Read();
                        Debug.Log("Read version string: " + reader.Value);
                        version = Convert.ToInt32(reader.Value);
                    }
                }
            }

            reader.Close();
            return version;
        }
    }
}
