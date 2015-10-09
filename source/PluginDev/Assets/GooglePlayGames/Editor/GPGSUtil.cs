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

namespace GooglePlayGames
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using UnityEditor;
    using UnityEngine;

    public static class GPGSUtil
    {
        private const string SERVICEIDPLACEHOLDER = "__NEARBY_SERVICE_ID__";
        public const string SERVICEIDKEY = "App.NearbdServiceId";

        private const string APPIDPLACEHOLDER = "__APP_ID__";
        public const string APPIDKEY = "proj.AppId";

        private const string CLASSNAMEPLACEHOLDER = "__Class__";
        public const string CLASSNAMEKEY = "proj.ConstantsClassName";

        private const string WEBCLIENTIDPLACEHOLDER = "__WEB_CLIENTID__";
        public const string WEBCLIENTIDKEY = "and.ClientId";


        private const string IOSCLIENTIDPLACEHOLDER = "__IOS_CLIENTID__";
        public const string IOSCLIENTIDKEY = "ios.ClientId";

        private const string IOSBUNDLEIDPLACEHOLDER = "__BUNDLEID__";
        public const string IOSBUNDLEIDKEY = "ios.BundleId";

        private const string TOKENPERMISSIONSHOLDER = "__TOKEN_PERMISSIONS__";
        private const string TOKENPERMISSIONKEY = "proj.tokenPermissions";

        private const string NAMESPACESTARTPLACEHOLDER = "__NameSpaceStart__";
        private const string NAMESPACEENDPLACEHOLDER = "__NameSpaceEnd__";

        private const string CONSTANTSPLACEHOLDER = "__Constant_Properties__";

        public const string LASTUPGRADEKEY = "lastUpgrade";

        public const string ANDROIDRESOURCEKEY = "and.ResourceData";
        public const string IOSRESOURCEKEY = "ios.ResourceData";

        private const string GameInfoPath = "Assets/GooglePlayGames/GameInfo.cs";
        public const string IOSSETUPDONEKEY = "ios.SetupDone";

        private const string TokenPermissions =
            "<uses-permission android:name=\"android.permission.GET_ACCOUNTS\"/>\n" +
            "<uses-permission android:name=\"android.permission.USE_CREDENTIALS\"/>";


        /// <summary>
        /// The map of replacements for filling in code templates.  The
        /// key is the string that appears in the template as a placeholder,
        /// the value is the key into the GPGSProjectSettings.
        /// </summary>
        private static Dictionary<string,string> Replacements =
            new Dictionary<string, string>()
        {
            {SERVICEIDPLACEHOLDER, SERVICEIDKEY},
            {APPIDPLACEHOLDER, APPIDKEY},
            {CLASSNAMEPLACEHOLDER, CLASSNAMEKEY},
            {WEBCLIENTIDPLACEHOLDER, WEBCLIENTIDKEY},
            {IOSCLIENTIDPLACEHOLDER, IOSCLIENTIDKEY},
            {IOSBUNDLEIDPLACEHOLDER, IOSBUNDLEIDKEY},
            {TOKENPERMISSIONSHOLDER, TOKENPERMISSIONKEY}
        };

        public static string SlashesToPlatformSeparator(string path)
        {
            return path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
        }

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

        public static string ReadEditorTemplate(string name)
        {
            return ReadFile(SlashesToPlatformSeparator("Assets/GooglePlayGames/Editor/" + name + ".txt"));
        }

        public static string ReadFully(string path)
        {
            return ReadFile(SlashesToPlatformSeparator(path));
        }

        public static void WriteFile(string file, string body)
        {
            file = SlashesToPlatformSeparator(file);
            using (var wr = new StreamWriter(file, false))
            {
                wr.Write(body);
            }
        }

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

        public static bool LooksLikeValidClientId(string s)
        {
            return s.EndsWith(".googleusercontent.com");
        }

        public static bool LooksLikeValidBundleId(string s)
        {
            return s.Length > 3;
        }

        public static bool LooksLikeValidPackageName(string s)
        {
            if (string.IsNullOrEmpty (s))
            {
                throw new Exception("cannot be empty");
            }

            string[] parts = s.Split(new char[] { '.' });
            foreach (string p in parts)
            {
                char[] bytes = p.ToCharArray();
                for(int i=0;i<bytes.Length;i++) {
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
        /// Makes legal identifier from string.
        /// Returns a legal C# identifier from the given string.  The transformations are:
        ///   - spaces => underscore _
        ///   - punctuation => empty string
        ///   - leading numbers are prefixed with underscore.
        /// </summary>
        /// <returns>the id</returns>
        /// <param name="key">Key.</param>
        static string makeIdentifier (string key)
        {
            string s;
            string retval = "";
            if (string.IsNullOrEmpty (key)) {
                return "_";
            }

            s = key.Trim().Replace (' ', '_');

            foreach (char c in s) {
                    if (char.IsLetterOrDigit(c) || c == '_') {
                        retval += c;
                    }
            }
            return retval;
        }

        public static void Alert(string s)
        {
            Alert(GPGSStrings.Error, s);
        }

        public static void Alert(string title, string s)
        {
            EditorUtility.DisplayDialog(title, s, GPGSStrings.Ok);
        }

        public static string GetAndroidSdkPath()
        {
            string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            if (sdkPath != null && (sdkPath.EndsWith("/") || sdkPath.EndsWith("\\")))
            {
                sdkPath = sdkPath.Substring(0, sdkPath.Length - 1);
            }

            return sdkPath;
        }

        public static bool HasAndroidSdk()
        {
            string sdkPath = GetAndroidSdkPath();
            return sdkPath != null && sdkPath.Trim() != string.Empty && System.IO.Directory.Exists(sdkPath);
        }

        public static void CopySupportLibs()
        {
            string sdkPath = GetAndroidSdkPath();
            string supportJarPath = sdkPath +
                                    GPGSUtil.SlashesToPlatformSeparator(
                                        "/extras/android/support/v4/android-support-v4.jar");
            string supportJarDest =
                GPGSUtil.SlashesToPlatformSeparator("Assets/Plugins/Android/libs/android-support-v4.jar");

            string libProjPath = sdkPath +
                                 GPGSUtil.SlashesToPlatformSeparator(
                                     "/extras/google/google_play_services/libproject/google-play-services_lib");

            string libProjAM =
                libProjPath + GPGSUtil.SlashesToPlatformSeparator("/AndroidManifest.xml");
            string libProjDestDir = GPGSUtil.SlashesToPlatformSeparator(
                                        "Assets/Plugins/Android/google-play-services_lib");

            // check that the Google Play Services lib project is there
            if (!System.IO.Directory.Exists(libProjPath) || !System.IO.File.Exists(libProjAM))
            {
                Debug.LogError("Google Play Services lib project not found at: " + libProjPath);
                EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.LibProjNotFound,
                    GPGSStrings.AndroidSetup.LibProjNotFoundBlurb, GPGSStrings.Ok);
                return;
            }

            // check version
            int version = GetGPSVersion(libProjPath);
            if (version < 0)
            {
                Debug.LogError("Google Play Services lib version cannot be found!");
            }
            if (version < PluginVersion.MinGmsCoreVersionCode)
            {
                if (!EditorUtility.DisplayDialog(string.Format(
                            GPGSStrings.AndroidSetup.LibProjVerTooOld,
                            version, PluginVersion.MinGmsCoreVersionCode),
                        GPGSStrings.Ok, GPGSStrings.Cancel))
                {
                    Debug.LogError("Google Play Services lib is too old! " +
                       " Found version " +
                        version + " require at least version " +
                        PluginVersion.MinGmsCoreVersionCode);
                    return;
                }
                Debug.Log("Ignoring the version mismatch and continuing the build.");
            }

            // clear out the destination library project
            GPGSUtil.DeleteDirIfExists(libProjDestDir);

            // Copy Google Play Services library
            FileUtil.CopyFileOrDirectory(libProjPath, libProjDestDir);

            if (!System.IO.File.Exists(supportJarPath))
            {
                // check for the new location
                supportJarPath = sdkPath + GPGSUtil.SlashesToPlatformSeparator(
                    "/extras/android/support/v7/appcompat/libs/android-support-v4.jar");
                Debug.LogError("Android support library v4 not found at: " + supportJarPath);
                if (!System.IO.File.Exists(supportJarPath))
                {
                    EditorUtility.DisplayDialog(GPGSStrings.AndroidSetup.SupportJarNotFound,
                        GPGSStrings.AndroidSetup.SupportJarNotFoundBlurb, GPGSStrings.Ok);
                    return;
                }
            }

            // create needed directories
            GPGSUtil.EnsureDirExists("Assets/Plugins");
            GPGSUtil.EnsureDirExists("Assets/Plugins/Android");

            // Clear out any stale version of the support jar.
            File.Delete(supportJarDest);

            // Copy Android Support Library
            FileUtil.CopyFileOrDirectory(supportJarPath, supportJarDest);
        }

        public static void GenerateAndroidManifest(bool needTokenPermissions)
        {
            string destFilename = GPGSUtil.SlashesToPlatformSeparator(
                                      "Assets/Plugins/Android/MainLibProj/AndroidManifest.xml");

            // Generate AndroidManifest.xml
            string manifestBody = GPGSUtil.ReadEditorTemplate("template-AndroidManifest");

            Dictionary<string,string> overrideValues = new Dictionary<string,string>();
            if (!needTokenPermissions) {
                overrideValues[TOKENPERMISSIONKEY] = "";
                overrideValues[WEBCLIENTIDPLACEHOLDER] = "";
            } else
            {
                overrideValues[TOKENPERMISSIONKEY] = TokenPermissions;
            }

            foreach(KeyValuePair<string, string> ent in Replacements)
            {
                string value =
                    GPGSProjectSettings.Instance.Get(ent.Value, overrideValues);
                manifestBody = manifestBody.Replace(ent.Key, value);
            }

            GPGSUtil.WriteFile(destFilename, manifestBody);
            GPGSUtil.UpdateGameInfo();
        }

        public static void WriteResourceIds(string className, Hashtable resourceKeys)
        {

            string constantsValues = string.Empty;
            string[] parts = className.Split('.');
            string dirName = "Assets";
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
                string key = makeIdentifier ((string)ent.Key);
                constantsValues += "        public const string " +
                    key + " = \"" + ent.Value + "\"; // <GPGSID>\n";
            }

            string fileBody = GPGSUtil.ReadEditorTemplate("template-Constants");
            if (nameSpace != string.Empty)
            {
                fileBody = fileBody.Replace(NAMESPACESTARTPLACEHOLDER,
                    "namespace " + nameSpace + "\n{");
            }
            else
            {
                fileBody = fileBody.Replace(NAMESPACESTARTPLACEHOLDER, "");
            }
            fileBody = fileBody.Replace(CLASSNAMEPLACEHOLDER, parts[parts.Length - 1]);
            fileBody = fileBody.Replace(CONSTANTSPLACEHOLDER, constantsValues);
            if (nameSpace != string.Empty)
            {
                fileBody = fileBody.Replace(NAMESPACEENDPLACEHOLDER,
                    "}");
            }
            else
            {
                fileBody = fileBody.Replace(NAMESPACEENDPLACEHOLDER, "");
            }
            WriteFile(dirName + "/" + parts[parts.Length-1] + ".cs", fileBody);
        }

        public static void UpdateGameInfo()
        {
            string fileBody = GPGSUtil.ReadEditorTemplate("template-GameInfo");

            foreach(KeyValuePair<string, string> ent in Replacements)
            {
                string value =
                    GPGSProjectSettings.Instance.Get(ent.Value);
                fileBody = fileBody.Replace(ent.Key, value);
            }

            GPGSUtil.WriteFile(GameInfoPath, fileBody);
        }

        public static void EnsureDirExists(string dir)
        {
            dir = dir.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }

        public static void DeleteDirIfExists(string dir)
        {
            if (System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.Delete(dir, true);
            }
        }

        static int GetGPSVersion(string libProjPath)
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
