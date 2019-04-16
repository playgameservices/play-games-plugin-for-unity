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
        public const string ANDROIDRESOURCEKEY = "and.ResourceData";

        /// <summary>Property key for project settings.</summary>
        public const string ANDROIDSETUPDONEKEY = "android.SetupDone";

        /// <summary>Property key for project settings.</summary>
        public const string ANDROIDBUNDLEIDKEY = "and.BundleId";

        /// <summary>Property key for plugin version.</summary>
        public const string PLUGINVERSIONKEY = "proj.pluginVersion";

        /// <summary>Property key for nearby settings done.</summary>
        public const string NEARBYSETUPDONEKEY = "android.NearbySetupDone";

        /// <summary>Property key for project settings.</summary>
        public const string LASTUPGRADEKEY = "lastUpgrade";

        /// <summary>Constant for token replacement</summary>
        private const string SERVICEIDPLACEHOLDER = "__NEARBY_SERVICE_ID__";

        private const string SERVICEID_ELEMENT_PLACEHOLDER = "__NEARBY_SERVICE_ELEMENT__";

        private const string NEARBY_PERMISSIONS_PLACEHOLDER = "__NEARBY_PERMISSIONS__";

        /// <summary>Constant for token replacement</summary>
        private const string APPIDPLACEHOLDER = "__APP_ID__";

        /// <summary>Constant for token replacement</summary>
        private const string CLASSNAMEPLACEHOLDER = "__Class__";

        /// <summary>Constant for token replacement</summary>
        private const string WEBCLIENTIDPLACEHOLDER = "__WEB_CLIENTID__";

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
        /// The game info file path, relative to the plugin root directory.  This is a generated file.
        /// </summary>
        private const string GameInfoRelativePath = "GameInfo.cs";

        /// <summary>
        /// The manifest path, relative to the plugin root directory.
        /// </summary>
        /// <remarks>The Games SDK requires additional metadata in the AndroidManifest.xml
        ///     file. </remarks>
        private const string ManifestRelativePath =
           "Plugins/Android/GooglePlayGamesManifest.plugin/AndroidManifest.xml";

        private const string RootFolderName = "GooglePlayGames";

        /// <summary>
        /// The root path of the Google Play Games plugin
        /// </summary>
        public static string RootPath
        {
            get
            {
                if (string.IsNullOrEmpty(mRootPath))
                {
                    string[] dirs = Directory.GetDirectories("Assets", RootFolderName, SearchOption.AllDirectories);
                    switch (dirs.Length)
                    {
                        case 0:
                            Alert("Plugin error: GooglePlayGames folder was renamed");
                            throw new Exception("GooglePlayGames folder was renamed");

                        case 1:
                            mRootPath = SlashesToPlatformSeparator(dirs[0]);
                            break;

                        default:
                            for (int i = 0; i < dirs.Length; i++)
                            {
                                if (File.Exists(SlashesToPlatformSeparator(Path.Combine(dirs[i], GameInfoRelativePath))))
                                {
                                    mRootPath = SlashesToPlatformSeparator(dirs[i]);
                                    break;
                                }
                            }

                            if (string.IsNullOrEmpty(mRootPath))
                            {
                                Alert("Plugin error: GooglePlayGames folder was renamed");
                                throw new Exception("GooglePlayGames folder was renamed");
                            }

                            break;
                    }
                }

                return mRootPath;
            }
        }

        /// <summary>
        /// The game info file path.  This is a generated file.
        /// </summary>
        private static string GameInfoPath
        {
            get
            {
                return SlashesToPlatformSeparator(Path.Combine(RootPath, GameInfoRelativePath));
            }
        }

        /// <summary>
        /// The manifest path.
        /// </summary>
        /// <remarks>The Games SDK requires additional metadata in the AndroidManifest.xml
        ///     file. </remarks>
        private static string ManifestPath
        {
            get
            {
                return SlashesToPlatformSeparator(Path.Combine(RootPath, ManifestRelativePath));
            }
        }

        /// <summary>
        /// The root path of the Google Play Games plugin
        /// </summary>
        private static string mRootPath = "";

        /// <summary>
        /// The map of replacements for filling in code templates.  The
        /// key is the string that appears in the template as a placeholder,
        /// the value is the key into the GPGSProjectSettings.
        /// </summary>
        private static Dictionary<string, string> replacements =
            new Dictionary<string, string>()
            {
                // Put this element placeholder first, since it has embedded placeholder
                {SERVICEID_ELEMENT_PLACEHOLDER,  SERVICEID_ELEMENT_PLACEHOLDER},
                { SERVICEIDPLACEHOLDER, SERVICEIDKEY },
                { APPIDPLACEHOLDER, APPIDKEY },
                { CLASSNAMEPLACEHOLDER, CLASSNAMEKEY },
                { WEBCLIENTIDPLACEHOLDER, WEBCLIENTIDKEY },
                { PLUGINVERSIONPLACEHOLDER, PLUGINVERSIONKEY},
                // Causes the placeholder to be replaced with overridden value at runtime.
                {  NEARBY_PERMISSIONS_PLACEHOLDER, NEARBY_PERMISSIONS_PLACEHOLDER}
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
            return ReadFile(Path.Combine(RootPath, string.Format("Editor{0}{1}.txt", Path.DirectorySeparatorChar, name)));
        }

        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="file">File path - the slashes will be corrected.</param>
        /// <param name="body">Body of the file to write.</param>
        public static void WriteFile(string file, string body)
        {
            file = SlashesToPlatformSeparator(file);
            DirectoryInfo dir = Directory.GetParent(file);
            dir.Create();
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
            string destFilename = ManifestPath;

            return File.Exists(destFilename);
        }

        /// <summary>
        /// Generates the android manifest.
        /// </summary>
        public static void GenerateAndroidManifest()
        {

            string destFilename = ManifestPath;

            // Generate AndroidManifest.xml
            string manifestBody = GPGSUtil.ReadEditorTemplate("template-AndroidManifest");

            Dictionary<string, string> overrideValues =
                new Dictionary<string, string>();

            if (!string.IsNullOrEmpty (GPGSProjectSettings.Instance.Get (SERVICEIDKEY)))
            {
                overrideValues [NEARBY_PERMISSIONS_PLACEHOLDER] =
                    "        <!-- Required for Nearby Connections -->\n" +
                    "        <uses-permission android:name=\"android.permission.BLUETOOTH\" />\n" +
                    "        <uses-permission android:name=\"android.permission.BLUETOOTH_ADMIN\" />\n" +
                    "        <uses-permission android:name=\"android.permission.ACCESS_WIFI_STATE\" />\n" +
                    "        <uses-permission android:name=\"android.permission.CHANGE_WIFI_STATE\" />\n" +
                    "        <uses-permission android:name=\"android.permission.ACCESS_COARSE_LOCATION\" />\n";
                overrideValues [SERVICEID_ELEMENT_PLACEHOLDER] =
                    "             <!-- Required for Nearby Connections API -->\n" +
                    "             <meta-data android:name=\"com.google.android.gms.nearby.connection.SERVICE_ID\"\n" +
                    "                  android:value=\"__NEARBY_SERVICE_ID__\" />\n";
            }
            else
            {
                overrideValues [NEARBY_PERMISSIONS_PLACEHOLDER] = "";
                overrideValues [SERVICEID_ELEMENT_PLACEHOLDER] = "";
            }

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
        /// Checks the dependencies file and fixes repository paths
        /// if they are incorrect (for example if the user moved plugin
        /// into some subdirectory). This is a generated file containing
        /// the list of dependencies that are needed for the plugin to work.
        /// </summary>
        public static void CheckAndFixDependencies()
        {
            string depPath = SlashesToPlatformSeparator(Path.Combine(GPGSUtil.RootPath, "Editor/GooglePlayGamesPluginDependencies.xml"));

            XmlDocument doc = new XmlDocument();
            doc.Load(depPath);

            XmlNodeList repos = doc.SelectNodes("//androidPackage[contains(@spec,'com.google.games')]//repository"); 
            foreach (XmlNode repo in repos)
            {
                if (!Directory.Exists(repo.InnerText))
                {
                    int pos = repo.InnerText.IndexOf(RootFolderName);
                    if (pos != -1)
                    {
                        repo.InnerText = Path.Combine(RootPath, repo.InnerText.Substring(pos + RootFolderName.Length + 1)).Replace("\\", "/");
                    }
                }
            }

            doc.Save(depPath);
        }

        /// <summary>
        /// Checks the file containing the list of versioned assets and fixes
        /// paths to them if they are incorrect (for example if the user moved
        /// plugin into some subdirectory). This is a generated file.
        /// </summary>
        public static void CheckAndFixVersionedAssestsPaths()
        {
            string[] foundPaths = Directory.GetFiles(RootPath, "GooglePlayGamesPlugin_v*.txt", SearchOption.AllDirectories);

            if (foundPaths.Length == 1)
            {
                string tmpFilePath = Path.GetTempFileName();
                
                StreamWriter writer = new StreamWriter(tmpFilePath);
                using (StreamReader reader = new StreamReader(foundPaths[0]))
                {
                    string assetPath;
                    while ((assetPath = reader.ReadLine()) != null)
                    {
                        int pos = assetPath.IndexOf(RootFolderName);
                        if (pos != -1)
                        {
                            assetPath = Path.Combine(RootPath, assetPath.Substring(pos + RootFolderName.Length + 1)).Replace("\\", "/");
                        }

                        writer.WriteLine(assetPath);
                    }
                }

                writer.Flush();
                writer.Close();

                try
                {
                    File.Copy(tmpFilePath, foundPaths[0], true);
                }
                finally
                {
                    File.Delete(tmpFilePath);
                }
            }
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
