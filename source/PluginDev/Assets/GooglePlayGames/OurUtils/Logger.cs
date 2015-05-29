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
using UnityEngine;

namespace GooglePlayGames.OurUtils {
public class Logger {
    static bool debugLogEnabled = false;

    public static bool DebugLogEnabled {
        get {
            return debugLogEnabled;
        }
        set {
            debugLogEnabled = value;
        }
    }

    private static bool warningLogEnabled = true;

    public static bool WarningLogEnabled {
        get {
            return warningLogEnabled;
        }
        set {
            warningLogEnabled = value;
        }
    }

    public static void d(string msg) {
        if (debugLogEnabled) {
            Debug.Log(ToLogMessage("", "DEBUG", msg));
        }
    }

    public static void w(string msg) {
        if (warningLogEnabled) {
            Debug.LogWarning(ToLogMessage("!!!", "WARNING", msg));
        }
    }

    public static void e(string msg) {
        if (warningLogEnabled) {
            Debug.LogWarning(ToLogMessage("***", "ERROR", msg));
        }
    }

    public static string describe(byte[] b) {
        return b == null ? "(null)" : "byte[" + b.Length + "]";
    }

    private static string ToLogMessage(String prefix, String logType, String msg) {
        return String.Format("{0} [Play Games Plugin DLL] {1} {2}: {3}",
            prefix, DateTime.Now.ToString("MM/dd/yy H:mm:ss zzz"), logType, msg);
    }
}
}

