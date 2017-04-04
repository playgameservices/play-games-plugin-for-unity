// <copyright file="Builder.cs" company="Google Inc.">
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.Native.Cwrapper
{
    using System;
    using System.Runtime.InteropServices;

    internal static class Builder
    {
        internal delegate void OnLogCallback(
        /* from(LogLevel_t) */ Types.LogLevel arg0,
        /* from(char const *) */ string arg1,
        /* from(void *) */ IntPtr arg2);

        internal delegate void OnAuthActionStartedCallback(
        /* from(AuthOperation_t) */ Types.AuthOperation arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void OnAuthActionFinishedCallback(
        /* from(AuthOperation_t) */ Types.AuthOperation arg0,
        /* from(AuthStatus_t) */ CommonErrorStatus.AuthStatus arg1,
        /* from(void *) */ IntPtr arg2);

        internal delegate void OnMultiplayerInvitationEventCallback(
        /* from(MultiplayerEvent_t) */ Types.MultiplayerEvent arg0,
        /* from(char const *) */ string arg1,
        /* from(MultiplayerInvitation_t) */ IntPtr arg2,
        /* from(void *) */ IntPtr arg3);

        internal delegate void OnTurnBasedMatchEventCallback(
        /* from(MultiplayerEvent_t) */ Types.MultiplayerEvent arg0,
        /* from(char const *) */ string arg1,
        /* from(TurnBasedMatch_t) */ IntPtr arg2,
        /* from(void *) */ IntPtr arg3);

        internal delegate void OnQuestCompletedCallback(
        /* from(Quest_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetOnAuthActionStarted(
            HandleRef self,
         /* from(GameServices_Builder_OnAuthActionStartedCallback_t) */OnAuthActionStartedCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_AddOauthScope(
            HandleRef self,
         /* from(char const *) */string scope);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetLogging(
            HandleRef self,
         /* from(GameServices_Builder_OnLogCallback_t) */OnLogCallback callback,
         /* from(void *) */IntPtr callback_arg,
         /* from(LogLevel_t) */Types.LogLevel min_level);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(GameServices_Builder_t) */ IntPtr GameServices_Builder_Construct();

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_EnableSnapshots(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetOnLog(
            HandleRef self,
         /* from(GameServices_Builder_OnLogCallback_t) */OnLogCallback callback,
         /* from(void *) */IntPtr callback_arg,
         /* from(LogLevel_t) */Types.LogLevel min_level);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetDefaultOnLog(
            HandleRef self,
         /* from(LogLevel_t) */Types.LogLevel min_level);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetOnAuthActionFinished(
            HandleRef self,
         /* from(GameServices_Builder_OnAuthActionFinishedCallback_t) */OnAuthActionFinishedCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetOnTurnBasedMatchEvent(
            HandleRef self,
         /* from(GameServices_Builder_OnTurnBasedMatchEventCallback_t) */OnTurnBasedMatchEventCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetOnQuestCompleted(
            HandleRef self,
         /* from(GameServices_Builder_OnQuestCompletedCallback_t) */OnQuestCompletedCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetOnMultiplayerInvitationEvent(
            HandleRef self,
         /* from(GameServices_Builder_OnMultiplayerInvitationEventCallback_t) */OnMultiplayerInvitationEventCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_SetShowConnectingPopup(
            HandleRef self,
            /* from(bool) */ bool flag);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(GameServices_t) */ IntPtr GameServices_Builder_Create(
            HandleRef self,
         /* from(PlatformConfiguration_t) */IntPtr platform);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void GameServices_Builder_Dispose(
            HandleRef self);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
