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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.Android {
    internal class OnStateLoadedProxy : AndroidJavaProxy {
        private OnStateLoadedListener mListener;
        private AndroidClient mAndroidClient;

        internal OnStateLoadedProxy(AndroidClient androidClient, OnStateLoadedListener listener) :
                base("com.google.android.gms.appstate.OnStateLoadedListener") {
            mListener = listener;
            mAndroidClient = androidClient;
        }

        private void OnStateConflict(int stateKey, string resolvedVersion,
                    byte[] localData, byte[] serverData) {
            Logger.d("OnStateLoadedProxy.onStateConflict called, stateKey=" + stateKey +
                ", resolvedVersion=" + resolvedVersion);

            debugLogData("localData", localData);
            debugLogData("serverData", serverData);

            if (mListener != null) {
                Logger.d("OnStateLoadedProxy.onStateConflict invoking conflict callback.");
                PlayGamesHelperObject.RunOnGameThread(() => {
                    byte[] resolvedData = mListener.OnStateConflict(stateKey,
                        localData, serverData);
                    mAndroidClient.ResolveState(stateKey, resolvedVersion, resolvedData, mListener);
                });
            } else {
                Logger.w("No conflict callback specified! Cannot resolve cloud save conflict.");
            }
        }

        private void OnStateLoaded(int statusCode, int stateKey, byte[] localData) {
            Logger.d("OnStateLoadedProxy.onStateLoaded called, status " + statusCode +
                ", stateKey=" + stateKey);
            debugLogData("localData", localData);

            bool success = false;

            switch (statusCode) {
            case JavaConsts.STATUS_OK:
                Logger.d("Status is OK, so success.");
                success = true;
                break;
            case JavaConsts.STATUS_NO_DATA:
                Logger.d("Status is NO DATA (no network?), so it's a failure.");
                success = false;
                localData = null;
                break;
            case JavaConsts.STATUS_STALE_DATA:
                Logger.d("Status is STALE DATA, so considering as success.");
                success = true;
                break;
            case JavaConsts.STATUS_KEY_NOT_FOUND:
                Logger.d("Status is KEY NOT FOUND, which is a success, but with no data.");
                success = true;
                localData = null;
                break;
            default:
                Logger.d("Status interpreted as failure.");
                success = false;
                localData = null;
                break;
            }

            if (mListener != null) {
                Logger.d("OnStateLoadedProxy.onStateLoaded invoking load callback.");
                PlayGamesHelperObject.RunOnGameThread(() => {
                    mListener.OnStateLoaded(success, stateKey, localData);
                });
            } else {
                Logger.w("No load callback specified!");
            }
        }

        private void debugLogData(string tag, byte[] data) {
            Logger.d("   " + tag + ": " + Logger.describe(data));
        }

        // We have to override Invoke because apparently the default implementation tries
        // to call getLength with the wrong signature on java.lang.reflect.Array and
        // crashes.
        public override AndroidJavaObject Invoke (string methodName, AndroidJavaObject[] javaArgs) {
            Logger.d("OnStateLoadedProxy.Invoke, method=" + methodName);
            Logger.d("    args=" + (javaArgs == null ? "(null)" :
                    "AndroidJavaObject[" + javaArgs.Length + "]"));

            if (methodName.Equals("onStateLoaded")) {
                Logger.d("Parsing onStateLoaded args.");
                int statusCode = javaArgs[0].Call<int>("intValue");
                int stateKey = javaArgs[1].Call<int>("intValue");
                byte[] localData = ConvertByteArray(javaArgs[2]);
                Logger.d("onStateLoaded args parsed, calling.");
                OnStateLoaded(statusCode, stateKey, localData);
            } else if (methodName.Equals("onStateConflict")) {
                Logger.d("Parsing onStateConflict args.");
                int stateKey = javaArgs[0].Call<int>("intValue");
                string ver = javaArgs[1].Call<string>("toString");
                byte[] localData = ConvertByteArray(javaArgs[2]);
                byte[] serverData = ConvertByteArray(javaArgs[3]);
                Logger.d("onStateConflict args parsed, calling.");
                OnStateConflict(stateKey, ver, localData, serverData);
            } else {
                Logger.e("Unexpected method invoked on OnStateLoadedProxy: " + methodName);
            }
            return null;
        }

        private static byte[] ConvertByteArray(AndroidJavaObject byteArrayObj) {
            Debug.Log("ConvertByteArray.");

            if (byteArrayObj == null) {
                return null;
            }

            AndroidJavaClass jc = new AndroidJavaClass("java.lang.reflect.Array");
            Debug.Log("Calling java.lang.reflect.Array.getLength.");
            int len = jc.CallStatic<int>("getLength", byteArrayObj);

            byte[] b = new byte[len];
            int i;
            for (i = 0; i < len; i++) {
                b[i] = jc.CallStatic<byte>("getByte", byteArrayObj, i);
            }

            return b;
        }
    }
}

#endif
