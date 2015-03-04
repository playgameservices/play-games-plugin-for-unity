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
#if (UNITY_ANDROID || UNITY_IPHONE)
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GooglePlayGames.Native.Cwrapper {
internal static class PlayerManager {
    internal delegate void FetchSelfCallback(
         /* from(PlayerManager_FetchSelfResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void FetchCallback(
         /* from(PlayerManager_FetchResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void FetchListCallback(
         /* from(PlayerManager_FetchListResponse_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_FetchInvitable(
        HandleRef self,
         /* from(DataSource_t) */ Types.DataSource data_source,
         /* from(PlayerManager_FetchListCallback_t) */ FetchListCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_FetchConnected(
        HandleRef self,
         /* from(DataSource_t) */ Types.DataSource data_source,
         /* from(PlayerManager_FetchListCallback_t) */ FetchListCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_Fetch(
        HandleRef self,
         /* from(DataSource_t) */ Types.DataSource data_source,
         /* from(char const *) */ string player_id,
         /* from(PlayerManager_FetchCallback_t) */ FetchCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_FetchRecentlyPlayed(
        HandleRef self,
         /* from(DataSource_t) */ Types.DataSource data_source,
         /* from(PlayerManager_FetchListCallback_t) */ FetchListCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_FetchSelf(
        HandleRef self,
         /* from(DataSource_t) */ Types.DataSource data_source,
         /* from(PlayerManager_FetchSelfCallback_t) */ FetchSelfCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_FetchSelfResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus PlayerManager_FetchSelfResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(Player_t) */ IntPtr PlayerManager_FetchSelfResponse_GetData(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_FetchResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus PlayerManager_FetchResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(Player_t) */ IntPtr PlayerManager_FetchResponse_GetData(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void PlayerManager_FetchListResponse_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus PlayerManager_FetchListResponse_GetStatus(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(size_t) */ UIntPtr PlayerManager_FetchListResponse_GetData_Length(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(Player_t) */ IntPtr PlayerManager_FetchListResponse_GetData_GetElement(
        HandleRef self,
         /* from(size_t) */ UIntPtr index);
}
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
