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
#if UNITY_ANDROID
using System;
using System.Linq;
using GooglePlayGames.BasicApi;
using UnityEngine;
using GooglePlayGames.Native.PInvoke;
using GooglePlayGames.OurUtils;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;

using C = GooglePlayGames.Native.Cwrapper.InternalHooks;

namespace GooglePlayGames.Native {
internal class AndroidAppStateClient : AppStateClient {

    private const int STATUS_OK = 0;
    private const int STATUS_STALE_DATA = 3;
    private const int STATUS_NO_DATA = 4;
    private const int STATUS_KEY_NOT_FOUND = 2002;
    private const int STATUS_CONFLICT = 2000;

    private static AndroidJavaClass AppStateManager =
        new AndroidJavaClass("com.google.android.gms.appstate.AppStateManager");

    private const string ResultCallbackClassname =
        "com.google.android.gms.common.api.ResultCallback";

    private readonly GameServices mServices;

    internal AndroidAppStateClient(GameServices services) {
        mServices = Misc.CheckNotNull(services);
    }

    private static AndroidJavaObject GetApiClient(GameServices services) {
        return JavaUtils.JavaObjectFromPointer(C.InternalHooks_GetApiClient(services.AsHandle()));
    }

    public void LoadState(int slot, OnStateLoadedListener listener) {
        Logger.d("LoadState, slot=" + slot);
        using (var apiClient = GetApiClient(mServices)) {
            CallAppState(apiClient, "load", new OnStateResultProxy(mServices, listener), slot);
        }
    }

    public void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
        Logger.d("UpdateState, slot=" + slot);
        using (var apiClient = GetApiClient(mServices)) {
            AppStateManager.CallStatic("update", apiClient, slot, data);
        }

        // Updates on android always succeed - at very least they are written to local disk.
        if (listener != null) {
            PlayGamesHelperObject.RunOnGameThread(() => listener.OnStateSaved(true, slot));
        }
    }

    private static object[] PrependApiClient(AndroidJavaObject apiClient, params object[] args) {
        List<object> argsList = new List<object>();
        argsList.Add(apiClient);
        argsList.AddRange(args);
        return argsList.ToArray();
    }

    private static void CallAppState(AndroidJavaObject apiClient, string methodName,
        params object[] args) {
        AppStateManager.CallStatic(methodName, PrependApiClient(apiClient, args));
    }

    private static void CallAppState(AndroidJavaObject apiClient, string methodName,
        AndroidJavaProxy callbackProxy, params object[] args) {

        var pendingResult = AppStateManager.CallStatic<AndroidJavaObject>(
            methodName, PrependApiClient(apiClient, args));

        using (pendingResult) {
            pendingResult.Call("setResultCallback", callbackProxy);
        }
    }

    private static int GetStatusCode(AndroidJavaObject result) {
        if (result == null) {
            return -1;
        }
        AndroidJavaObject status = result.Call<AndroidJavaObject>("getStatus");
        return status.Call<int>("getStatusCode");
    }

    internal static byte[] ToByteArray(AndroidJavaObject javaByteArray) {
        if (javaByteArray == null) {
            return null;
        }

        return AndroidJNIHelper.ConvertFromJNIArray<byte[]>(javaByteArray.GetRawObject());
    }

    private class OnStateResultProxy : AndroidJavaProxy {
        private readonly GameServices mServices;
        private readonly OnStateLoadedListener mListener;

        internal OnStateResultProxy(GameServices services, OnStateLoadedListener listener)
            : base(ResultCallbackClassname) {
            mServices = Misc.CheckNotNull(services);
            mListener = listener;
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
                    byte[] resolvedData =
                        mListener.OnStateConflict(stateKey, localData, serverData);
                    ResolveState(stateKey, resolvedVersion, resolvedData, mListener);
                });
            } else {
                Logger.w("No conflict callback specified! Cannot resolve cloud save conflict.");
            }
        }

        private void ResolveState(int slot, string resolvedVersion, byte[] resolvedData,
            OnStateLoadedListener listener) {
            Logger.d(string.Format("AndroidClient.ResolveState, slot={0}, ver={1}, " +
                "data={2}", slot, resolvedVersion, resolvedData));
            using (var apiClient = GetApiClient(mServices)) {
                CallAppState(apiClient, "resolve", new OnStateResultProxy(mServices, listener),
                    slot, resolvedVersion, resolvedData);
            }
        }

        private void OnStateLoaded(int statusCode, int stateKey, byte[] localData) {
            Logger.d("OnStateResultProxy.onStateLoaded called, status " + statusCode +
                ", stateKey=" + stateKey);
            debugLogData("localData", localData);

            bool success = false;

            switch (statusCode) {
                case STATUS_OK:
                    Logger.d("Status is OK, so success.");
                    success = true;
                    break;
                case STATUS_NO_DATA:
                    Logger.d("Status is NO DATA (no network?), so it's a failure.");
                    success = false;
                    localData = null;
                    break;
                case STATUS_STALE_DATA:
                    Logger.d("Status is STALE DATA, so considering as success.");
                    success = true;
                    break;
                case STATUS_KEY_NOT_FOUND:
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

            int statusCode = GetStatusCode(result);
            Logger.d("OnStateResultProxy: status code is " + statusCode);

            if (result == null) {
                Logger.e("OnStateResultProxy: result is null.");
                return;
            }

            Logger.d("OnstateResultProxy: retrieving result objects...");
            AndroidJavaObject loadedResult = result.NullSafeCall("getLoadedResult");
            AndroidJavaObject conflictResult = result.NullSafeCall("getConflictResult");

            Logger.d("Got result objects.");
            Logger.d("loadedResult = " + loadedResult);
            Logger.d("conflictResult = " + conflictResult);

            if (conflictResult != null) {
                Logger.d("OnStateResultProxy: processing conflict.");
                int stateKey = conflictResult.Call<int>("getStateKey");
                string ver = conflictResult.Call<string>("getResolvedVersion");
                byte[] localData = ToByteArray(conflictResult.NullSafeCall("getLocalData"));
                byte[] serverData = ToByteArray(conflictResult.NullSafeCall("getServerData"));
                Logger.d("OnStateResultProxy: conflict args parsed, calling.");
                OnStateConflict(stateKey, ver, localData, serverData);
            } else if (loadedResult != null) {
                Logger.d("OnStateResultProxy: processing normal load.");
                int stateKey = loadedResult.Call<int>("getStateKey");
                byte[] localData = ToByteArray(loadedResult.NullSafeCall("getLocalData"));
                Logger.d("OnStateResultProxy: loaded args parsed, calling.");
                OnStateLoaded(statusCode, stateKey, localData);
            } else {
                Logger.e("OnStateResultProxy: both loadedResult and conflictResult are null!");
            }
        }
    }

}
}

#endif
