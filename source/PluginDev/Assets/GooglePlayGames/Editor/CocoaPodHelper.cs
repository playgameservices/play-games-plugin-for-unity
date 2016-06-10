// <copyright file="CocoaPodHelper.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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
#if UNITY_IPHONE && !NO_GPGS

namespace GooglePlayGames.Editor
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class CocoaPodHelper
    {
        // pod can be in 2 places. El Capitain does not allow
        // installs into /usr/bin, so pod ends up in /usr/local/bin
        private static string[] podPaths = {
                "/usr/bin/pod",
                "/usr/local/bin/pod"
        };

        public static bool Update(string projDir)
        {
            string podPath = null;
            foreach (string p in podPaths)
            {
                if (File.Exists(p))
                {
                    podPath = p;
                }
            }
            if (podPath == null || !File.Exists(podPath))
            {
                UnityEngine.Debug.LogError("pod executable not found: " + podPath);
                return false;
            }
            if (!Directory.Exists(projDir))
            {

                UnityEngine.Debug.LogError("project not found: " + projDir);
                return false;
            }
            return ExecuteCommand("update", projDir);
        }

        private static bool ExecuteCommand(string command, string projDir)
        {
            string podPath = null;
            foreach (string p in podPaths)
            {
                if (File.Exists(p))
                {
                    podPath = p;
                }
            }
            if (podPath == null || !File.Exists(podPath))
            {
                UnityEngine.Debug.LogError("pod executable not found: " + podPath);
                return false;
            }

            using (var process = new Process())
            {
                if (!process.StartInfo.EnvironmentVariables.ContainsKey("LANG"))
                {
                    process.StartInfo.EnvironmentVariables.Add("LANG", "en_US.UTF-8");
                }

                process.StartInfo.WorkingDirectory = projDir;
                process.StartInfo.FileName = podPath;
                process.StartInfo.Arguments = command;
                UnityEngine.Debug.Log("Executing " + podPath + " command: " +
                    process.StartInfo.Arguments);
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;

                try
                {
                    process.Start();
                    process.StandardError.ReadToEnd();
                    var stdOutput = process.StandardOutput.ReadToEnd();
                    var stdError = process.StandardError.ReadToEnd();

                    UnityEngine.Debug.Log("pod stdout: " + stdOutput);

                    if (stdError != null && stdError.Length > 0)
                    {
                        UnityEngine.Debug.LogError("pod stderr: " + stdError);
                    }

                    if (!process.WaitForExit(10 * 1000))
                    {
                        throw new Exception("pod did not exit in a timely fashion");
                    }

                    return process.ExitCode == 0;

                }
                catch (Exception e)
                {
                    throw new Exception(
                        "Encountered unexpected error while running pod",
                        e);
                }
            }
        }
    }
}
#endif
