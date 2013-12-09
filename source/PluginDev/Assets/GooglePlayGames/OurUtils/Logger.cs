/*
 * Copyright (C) 2013 Google Inc.
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
using UnityEngine;

namespace GooglePlayGames.OurUtils {
    public class Logger {
        static string LOG_PREF = "[Play Games Plugin DLL] ";
        static bool debugLogEnabled = false;

        public static bool DebugLogEnabled {
            get {
                return debugLogEnabled;
            }
            set {
                debugLogEnabled = value;
            }
        }

        public static void d(string msg) {
            if (debugLogEnabled) {
                Debug.Log(LOG_PREF + msg);
            }
        }

        public static void w(string msg) {
            Debug.LogWarning("!!! " + LOG_PREF + " WARNING: " + msg);
        }

        public static void e(string msg) {
            Debug.LogWarning("*** " + LOG_PREF + " ERROR: " + msg);
        }

        public static string describe(byte[] b) {
            return b == null ? "(null)" : "byte[" + b.Length + "]";
        }
    }
}

