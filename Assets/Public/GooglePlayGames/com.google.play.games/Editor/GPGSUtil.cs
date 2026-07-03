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
    using System.Linq;
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
        private const string GameInfoRelativePath = "GooglePlayGames/Runtime/Scripts/GameInfo.cs";

        private const string RootFolderName = "com.google.play.games";

        /// <summary>
        /// The root path of the Google Play Games plugin
        /// </summary>
        public static string RootPath
        {
            get
            {
                if (string.IsNullOrEmpty(mRootPath) || !Directory.Exists(mRootPath))
                {
#if UNITY_2018_4_OR_NEWER
                    mRootPath = Path.GetFullPath(Path.Combine("Packages",RootFolderName));
                    if(Directory.Exists(mRootPath))
                        return mRootPath;
#endif

                    string[] dirs = new[] {
#if UNITY_2018_4_OR_NEWER
                        // search for remote UPM installation
                        Path.Join("Library","PackageCache"),
                        "Packages",
#endif
                        "Assets"
                    }.Distinct().SelectMany((path) => {
                        return Directory.GetDirectories(path, RootFolderName + "*", SearchOption.AllDirectories);
                    }).Distinct().ToArray();

                    mRootPath = dirs.Select((dir) => SlashesToPlatformSeparator(dir)).FirstOrDefault((dir) => File.Exists(Path.Combine(dir,GameInfoRelativePath)));

                    if (string.IsNullOrEmpty(mRootPath))
                    {
                        Alert("Plugin error: com.google.play.games folder was renamed");
                        throw new Exception("com.google.play.games folder was renamed");
                    }

                    // UPM package root path is 'Library/PackageCache/com.google.play.games@.*/
                    // where the suffix can be a version number if installed with URS
                    // or a hash if from disk or tarball
                    if (mRootPath.Contains(RootFolderName + '@'))
                    {
                        mRootPath = mRootPath.Replace("Packages", "Library/PackageCache");
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
            get { return SlashesToPlatformSeparator(Path.Combine("Assets", GameInfoRelativePath)); }
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
                {SERVICEID_ELEMENT_PLACEHOLDER, SERVICEID_ELEMENT_PLACEHOLDER},
                {SERVICEIDPLACEHOLDER, SERVICEIDKEY},
                {APPIDPLACEHOLDER, APPIDKEY},
                {CLASSNAMEPLACEHOLDER, CLASSNAMEKEY},
                {WEBCLIENTIDPLACEHOLDER, WEBCLIENTIDKEY},
                {PLUGINVERSIONPLACEHOLDER, PLUGINVERSIONKEY},
                // Causes the placeholder to be replaced with overridden value at runtime.
                {NEARBY_PERMISSIONS_PLACEHOLDER, NEARBY_PERMISSIONS_PLACEHOLDER}
            };

        /// <summary>
        /// Replaces / in file path to be the os specific separator.
        /// </summary>
        /// <returns>The path.</returns>
        /// <param name="path">Path with correct separators.</param>
        public static string SlashesToPlatformSeparator(string path)
        {
            return Path.DirectorySeparatorChar == '/' ? path : path.Replace('/', Path.DirectorySeparatorChar);
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

            using(var sr = new StreamReader(filePath))
                return sr.ReadToEnd();
        }

        /// <summary>
        /// Reads the editor template.
        /// </summary>
        /// <returns>The editor template contents.</returns>
        /// <param name="name">Name of the template in the editor directory.</param>
        public static string ReadEditorTemplate(string name)
        {
            return ReadFile(Path.Combine(RootPath,"Editor",string.Format("{0}.txt", name)));
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

            string[] parts = s.Split('.');
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
#if UNITY_2019_1_OR_NEWER
            // Unity 2019.x added installation of the Android SDK in the AndroidPlayer directory
            // so fallback to searching for it there.
            if (string.IsNullOrEmpty(sdkPath) || EditorPrefs.GetBool("SdkUseEmbedded"))
            {
                string androidPlayerDir = BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None);
                if (!string.IsNullOrEmpty(androidPlayerDir))
                {
                    string androidPlayerSdkDir = Path.Combine(androidPlayerDir, "SDK");
                    if (Directory.Exists(androidPlayerSdkDir))
                    {
                        sdkPath = androidPlayerSdkDir;
                    }
                }
            }
#endif
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
            return sdkPath != null && sdkPath.Trim() != string.Empty && Directory.Exists(sdkPath);
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
            string dirName = string.Join("/",parts.Prepend(string.IsNullOrEmpty(classDirectory) ? "Assets" : classDirectory));

            string nameSpace = className;

            EnsureDirExists(dirName);
            foreach (DictionaryEntry ent in resourceKeys)
            {
                string key = MakeIdentifier((string) ent.Key);
                constantsValues += "        public const string " +
                                   key + " = \"" + ent.Value + "\"; // <GPGSID>\n";
            }

            string namespaceStart = string.IsNullOrEmpty(nameSpace) ? "namespace " + nameSpace + "\n{" : string.Empty;
            string fileBody = GPGSUtil.ReadEditorTemplate("template-Constants").Replace(NAMESPACESTARTPLACEHOLDER,namespaceStart);

            fileBody = fileBody.Replace(CLASSNAMEPLACEHOLDER, parts[parts.Length - 1]);
            fileBody = fileBody.Replace(CONSTANTSPLACEHOLDER, constantsValues);

            fileBody = fileBody.Replace(NAMESPACEENDPLACEHOLDER,nameSpace != string.Empty ? "}" : string.Empty);

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
                string value = GPGSProjectSettings.Instance.Get(ent.Value);
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


        const string androidNamespaceURL = "http://schemas.android.com/apk/res/android";
        public static void PatchAndroidManifest(string manifestPath = null)
        {
            if(string.IsNullOrEmpty(manifestPath))
                manifestPath = Path.Combine(Application.dataPath,"Plugins","Android","AndroidManifest.xml");

            if(!File.Exists(manifestPath))
            {
                EditorUtility.DisplayDialog("Google Play Games Error","Cannot find AndroidManifest.xml to modified","OK");
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(manifestPath);

            bool changed = false;
            xmlDoc.NodeChanged += (sender,xncea) => changed = true;
            xmlDoc.NodeRemoved += (sender,xncea) => changed = true;
            xmlDoc.NodeInserted += (sender,xncea) => changed = true;

            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("android",androidNamespaceURL);

            SetAndroidAttributeOrRemoveIfNoValue(xmlDoc,nsmgr,"com.google.android.gms.games.unityVersion",PluginVersion.VersionString,true);

            SetAndroidAttributeOrRemoveIfNoValue(xmlDoc,nsmgr,"com.google.android.gms.games.APP_ID",GPGSProjectSettings.Instance.Get(GPGSUtil.APPIDKEY),true);

            SetAndroidAttributeOrRemoveIfNoValue(xmlDoc,nsmgr,"com.google.android.gms.games.WEB_CLIENT_ID",GPGSProjectSettings.Instance.Get(GPGSUtil.WEBCLIENTIDKEY),false);

            Debug.Log("TestInit");

            if(SetAndroidAttributeOrRemoveIfNoValue(xmlDoc,nsmgr,"com.google.android.gms.nearby.connection.SERVICE_ID",GPGSProjectSettings.Instance.Get(GPGSUtil.SERVICEIDKEY),false))
            {
                foreach(var permission in new[]{ "BLUETOOTH","BLUETOOTH_ADMIN","ACCESS_WIFI_STATE","CHANGE_WIFI_STATE","ACCESS_COARSE_LOCATION" })
                    xmlDoc.FindOrCreate(nsmgr,androidNamespaceURL,"manifest/uses-permission","android:name","android.permission." + permission);
            }

            if(changed)
                xmlDoc.Save(manifestPath);
        }

        /** <returns>value is set</returns> */
        static bool SetAndroidAttributeOrRemoveIfNoValue(XmlDocument xmlDoc,XmlNamespaceManager nsmgr,string key,string value,bool shouldPrependU003)
        {
            if(!string.IsNullOrEmpty(value))
            {
                var element = xmlDoc.FindOrCreate(nsmgr,androidNamespaceURL,"manifest/application/meta-data","android:name",key);
                element.SetAttributeNS(androidNamespaceURL,"android:value",shouldPrependU003 ? ("\\u003" + value) : value);
                return true;
            }

            foreach(var node in xmlDoc.SelectNodesWithAttribute("manifest/application/meta-data","android:name",key,nsmgr).OfType<XmlNode>())
                node.ParentNode.RemoveChild(node);

            return false;
        }

        public static void SetAttributeNS(this XmlElement element,string namespaceURL,string attributeName,string attributeValue)
        {
            var attr = element?.Attributes?.OfType<XmlAttribute>().FirstOrDefault((item) => item.Name == attributeName);
            if(attr == null)
                attr = element.SetAttributeNode(element.OwnerDocument.CreateAttribute(attributeName,namespaceURL));

            if(attr.Value != attributeValue)
                attr.Value = attributeValue;
        }

        public static XmlElement FindOrCreate(this XmlDocument xmlDoc,XmlNamespaceManager nsmgr,string attributeNamespace,string path,string attributeName,string attributeValue)
        {
            var nodes = xmlDoc.SelectNodesWithAttribute(path,attributeName,attributeValue,nsmgr);
            if(nodes.Count > 0)
            {
                var result = nodes.OfType<XmlElement>().FirstOrDefault();

                foreach(var node in nodes.OfType<XmlNode>().Where((item) => item != result))
                    node.ParentNode.RemoveChild(node);

                if(result != null)
                    return result;
            }

            var element = xmlDoc.DocumentElement;
            var stack = new Stack<string>();
            while(path.LastIndexOf('/') is int i && i > 0)
            {
                stack.Push(path.Substring(i + 1));
                path = path.Remove(i);
                element = xmlDoc.SelectNodes(path,nsmgr)?.OfType<XmlElement>().FirstOrDefault();
                if(element != null)
                    break;
            }

            while(stack.TryPop(out string name))
            {
                element = element.AppendChild(xmlDoc.CreateElement(name)) as XmlElement;
            }

            element.SetAttributeNS(attributeNamespace,attributeName,attributeValue);

            return element;
        }

        public static XmlNodeList SelectNodesWithAttribute(this XmlDocument xmlDoc,string path,string attributeName,string attributeValue,XmlNamespaceManager nsmgr)
        {
            return xmlDoc.SelectNodes($"{path}[@{attributeName}='{attributeValue}']",nsmgr);
        }
    }
}
