// <copyright file="AndroidPlatformConfiguration.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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

#if UNITY_ANDROID
namespace GooglePlayGames.Native.Cwrapper {
    using System;
    using System.Runtime.InteropServices;

    internal static class AndroidPlatformConfiguration {
    internal delegate void IntentHandler(
         /* from(jobject) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void OnLaunchedWithSnapshotCallback(
         /* from(SnapshotMetadata_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    internal delegate void OnLaunchedWithQuestCallback(
         /* from(Quest_t) */ IntPtr arg0,
         /* from(void *) */ IntPtr arg1);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void AndroidPlatformConfiguration_SetOnLaunchedWithSnapshot(
        HandleRef self,
         /* from(AndroidPlatformConfiguration_OnLaunchedWithSnapshotCallback_t) */ OnLaunchedWithSnapshotCallback callback,
         /* from(void *) */ IntPtr callback_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(AndroidPlatformConfiguration_t) */ IntPtr AndroidPlatformConfiguration_Construct();

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void AndroidPlatformConfiguration_SetOptionalIntentHandlerForUI(
        HandleRef self,
         /* from(AndroidPlatformConfiguration_IntentHandler_t) */ IntentHandler intent_handler,
         /* from(void *) */ IntPtr intent_handler_arg);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void AndroidPlatformConfiguration_Dispose(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern /* from(bool) */ bool AndroidPlatformConfiguration_Valid(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void AndroidPlatformConfiguration_SetActivity(
        HandleRef self,
         /* from(jobject) */ IntPtr android_app_activity);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void AndroidPlatformConfiguration_SetOnLaunchedWithQuest(
        HandleRef self,
         /* from(AndroidPlatformConfiguration_OnLaunchedWithQuestCallback_t) */ OnLaunchedWithQuestCallback callback,
         /* from(void *) */ IntPtr callback_arg);
}
}
#endif // UNITY_ANDROID
