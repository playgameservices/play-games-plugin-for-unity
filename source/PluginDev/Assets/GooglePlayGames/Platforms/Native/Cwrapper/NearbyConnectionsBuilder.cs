// <copyright file="NearbyConnectionsBuilder.cs" company="Google Inc.">
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

// Android only feature
#if (UNITY_ANDROID)


namespace GooglePlayGames.Native.Cwrapper {
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
internal static class NearbyConnectionsBuilder {
    internal delegate void OnInitializationFinishedCallback(
         /* from(InitializationStatus_t) */ NearbyConnectionsStatus.InitializationStatus arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void OnLogCallback(
         /* from(LogLevel_t) */ Types.LogLevel arg0,
         /* from(char const *) */ string arg1,
         /* from(void *) */ IntPtr arg2);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void NearbyConnections_Builder_SetOnInitializationFinished(
        HandleRef self,
         /* from(NearbyConnections_Builder_OnInitializationFinishedCallback_t) */ OnInitializationFinishedCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(NearbyConnections_Builder_t) */ IntPtr NearbyConnections_Builder_Construct();

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void NearbyConnections_Builder_SetClientId(
        HandleRef self,
         /* from(int64_t) */ long client_id);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void NearbyConnections_Builder_SetOnLog(
        HandleRef self,
         /* from(NearbyConnections_Builder_OnLogCallback_t) */ OnLogCallback callback,
         /* from(void *) */ IntPtr callback_arg,
         /* from(LogLevel_t) */ Types.LogLevel min_level);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void NearbyConnections_Builder_SetDefaultOnLog(
        HandleRef self,
         /* from(LogLevel_t) */ Types.LogLevel min_level);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(NearbyConnections_t) */ IntPtr NearbyConnections_Builder_Create(
        HandleRef self,
         /* from(PlatformConfiguration_t) */ IntPtr platform);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void NearbyConnections_Builder_Dispose(
        HandleRef self);
}
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
