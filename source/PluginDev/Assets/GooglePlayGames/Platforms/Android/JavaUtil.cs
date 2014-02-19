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

#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.Android {
    internal class JavaUtil {
        public const string GmsPkg = "com.google.android.gms";
        public const string ResultCallbackClass = JavaUtil.GmsPkg + ".common.api.ResultCallback";
        
        private static Dictionary<string, AndroidJavaClass> mClassDict = 
                new Dictionary<string, AndroidJavaClass>();
        private static Dictionary<string, AndroidJavaObject> mFieldDict = 
                new Dictionary<string, AndroidJavaObject>();
        
        // Loads and caches a class from the GmsCore package
        public static AndroidJavaClass GetGmsClass(string className) {
            if (mClassDict.ContainsKey(className)) {
                return mClassDict[className];
            }
            
            try {
                AndroidJavaClass cls = new AndroidJavaClass(GmsPkg + "." + className);
                mClassDict[className] = cls;
                return cls;
            } catch (Exception ex) {
                Logger.e("Failed to load GmsCore class " + GmsPkg + "." + className);
                throw ex;
            }
        }
        
        // Loads and caches a static field from a GmsCore class
        public static AndroidJavaObject GetGmsField(string className, string fieldName) {
            string key = className + "/" + fieldName;
            if (mFieldDict.ContainsKey(key)) {
                return mFieldDict[key];
            }
            
            AndroidJavaClass cls = GetGmsClass(className);
            AndroidJavaObject obj = cls.GetStatic<AndroidJavaObject>(fieldName);
            mFieldDict[key] = obj;
            return obj;
        }
        
        // Gets the status code from a Result object
        public static int GetStatusCode(AndroidJavaObject result) {
            if (result == null) {
                return -1;
            }
            AndroidJavaObject status = result.Call<AndroidJavaObject>("getStatus");
            return status.Call<int>("getStatusCode");
        }
        
        // Sadly, it appears that calling a method that returns a null Object in Java
        // will cause AndroidJavaObject to crash, so we use this ugly workaround:
        public static AndroidJavaObject CallNullSafeObjectMethod(AndroidJavaObject target, string methodName,
                params object[] args) {
            try {
                return target.Call<AndroidJavaObject>(methodName, args);
            } catch (Exception ex) {
                if (ex.Message.Contains("null")) {
                    // expected -- means method returned null
                    return null;
                } else {
                    Logger.w("CallObjectMethod exception: " + ex);
                    return null;
                }
            }
        }
    }
}
#endif
