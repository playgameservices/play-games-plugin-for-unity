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
    internal class OnStateResultProxy : AndroidJavaProxy {
        private OnStateLoadedListener mListener;
        private AndroidClient mAndroidClient;

        internal OnStateResultProxy(AndroidClient androidClient, OnStateLoadedListener listener) :
                base(JavaConsts.ResultCallbackClass) {
            mListener = listener;
            mAndroidClient = androidClient;
        }

        private void OnStateConflict(int stateKey, string resolvedVersion,
                    byte[] localData, byte[] serverData) {
            Logger.d("OnStateResultProxy.onStateConflict called, stateKey=" + stateKey +
                ", resolvedVersion=" + resolvedVersion);

            debugLogData("localData", localData);
            debugLogData("serverData", serverData);

            if (mListener != null) {
                Logger.d("OnStateResultProxy.onStateConflict invoking conflict callback.");
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
            Logger.d("OnStateResultProxy.onStateLoaded called, status " + statusCode +
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
                Logger.e("Cloud load failed with status code " + statusCode);
                success = false;
                localData = null;
                break;
            }

            if (mListener != null) {
                Logger.d("OnStateResultProxy.onStateLoaded invoking load callback.");
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

        public void onResult(AndroidJavaObject result) {
            Logger.d("OnStateResultProxy.onResult, result=" + result);
            
            int statusCode = JavaUtil.GetStatusCode(result);
            Logger.d("OnStateResultProxy: status code is " + statusCode);
            
            if (result == null) {
                Logger.e("OnStateResultProxy: result is null.");
                return;
            }
            
            Logger.d("OnstateResultProxy: retrieving result objects...");
            AndroidJavaObject loadedResult = JavaUtil.CallNullSafeObjectMethod(result, 
                    "getLoadedResult");
            AndroidJavaObject conflictResult = JavaUtil.CallNullSafeObjectMethod(result,
                    "getConflictResult");
            
            Logger.d("Got result objects.");
            Logger.d("loadedResult = " + loadedResult);
            Logger.d("conflictResult = " + conflictResult);

            if (conflictResult != null) {
                Logger.d("OnStateResultProxy: processing conflict.");
                int stateKey = conflictResult.Call<int>("getStateKey");
                string ver = conflictResult.Call<string>("getResolvedVersion");
                byte[] localData = JavaUtil.ConvertByteArray(JavaUtil.CallNullSafeObjectMethod(
                        conflictResult, "getLocalData"));
                byte[] serverData = JavaUtil.ConvertByteArray(JavaUtil.CallNullSafeObjectMethod(
                        conflictResult, "getServerData"));
                Logger.d("OnStateResultProxy: conflict args parsed, calling.");
                OnStateConflict(stateKey, ver, localData, serverData);
            } else if (loadedResult != null) {
                Logger.d("OnStateResultProxy: processing normal load.");
                int stateKey = loadedResult.Call<int>("getStateKey");
                byte[] localData = JavaUtil.ConvertByteArray(JavaUtil.CallNullSafeObjectMethod(
                        loadedResult, "getLocalData"));
                Logger.d("OnStateResultProxy: loaded args parsed, calling.");
                OnStateLoaded(statusCode, stateKey, localData);
            } else {
                Logger.e("OnStateResultProxy: both loadedResult and conflictResult are null!");
            }
        }

        
    }
}

#endif
