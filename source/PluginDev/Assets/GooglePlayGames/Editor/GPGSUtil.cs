/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GooglePlayGames {

    public static class GPGSUtil {

        private const string GameInfoPath = "Assets/GooglePlayGames/GameInfo.cs";
        private const string GameInfoTemplatePath = "Assets/GooglePlayGames/Editor/GameInfo.template";

        public static string SlashesToPlatformSeparator(string path) {
            return path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
        }

        public static string ReadFile(string filePath) {
            filePath = SlashesToPlatformSeparator(filePath);
            if (!File.Exists(filePath)) {
                Alert("Plugin error: file not found: " + filePath);
                return null;
            }
            StreamReader sr = new StreamReader(filePath);
            string body = sr.ReadToEnd();
            sr.Close();
            return body;
        }

        public static string ReadEditorTemplate(string name) {
            return ReadFile(SlashesToPlatformSeparator("Assets/GooglePlayGames/Editor/" + name + ".txt"));
        }

        public static string ReadFully(string path) {
            return ReadFile(SlashesToPlatformSeparator(path));
        }

        public static void WriteFile(string file, string body) {
            file = SlashesToPlatformSeparator(file);
            using (var wr = new StreamWriter(file, false)) {
                wr.Write(body);
            }
        }

        public static bool LooksLikeValidAppId(string s) {
            if (s.Length < 5) {
                return false;
            }
            foreach (char c in s) {
                if (c < '0' || c > '9') {
                    return false;
                }
            }
            return true;
        }

        public static bool LooksLikeValidClientId(string s) {
            return s.EndsWith(".googleusercontent.com");
        }

        public static bool LooksLikeValidBundleId(string s) {
            return s.Length > 3;
        }

        public static bool LooksLikeValidPackageName(string s) {
            return !s.Contains(" ") && s.Split(new char[] { '.' }).Length > 1;
        }

        public static void Alert(string s) {
            Alert(GPGSStrings.Error, s);
        }

        public static void Alert(string title, string s) {
            EditorUtility.DisplayDialog(title, s, GPGSStrings.Ok);
        }

        public static string GetAndroidSdkPath() {
            string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            if (sdkPath != null && (sdkPath.EndsWith("/") || sdkPath.EndsWith("\\"))) {
                sdkPath = sdkPath.Substring(0, sdkPath.Length - 1);
            }
            return sdkPath;
        }

        public static bool HasAndroidSdk() {
            string sdkPath = GetAndroidSdkPath();
            return sdkPath != null && sdkPath.Trim() != "" && System.IO.Directory.Exists(sdkPath);
        }

        public static void UpdateGameInfo() {
            string fileBody = GPGSUtil.ReadFully(GameInfoTemplatePath);
            var appId = GPGSProjectSettings.Instance.Get("proj.AppId", null);
            if (appId != null) {
                fileBody = fileBody.Replace("__APPID__", appId);
            }

            var clientId = GPGSProjectSettings.Instance.Get("ios.ClientId", null);
            if (clientId != null) {
                fileBody = fileBody.Replace("__CLIENTID__", clientId);
            }

            var bundleId = GPGSProjectSettings.Instance.Get("ios.BundleId", null);
            if (bundleId != null) {
                fileBody = fileBody.Replace("__BUNDLEID__", bundleId);
            }
            GPGSUtil.WriteFile(GameInfoPath, fileBody);
        }
    }
}

