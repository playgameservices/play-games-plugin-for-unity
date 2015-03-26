// <copyright file="Quest.cs" company="Google Inc.">
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))


namespace GooglePlayGames.Native.Cwrapper
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class Quest
    {
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr Quest_Description(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr Quest_BannerUrl(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(int64_t) */ long Quest_ExpirationTime(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr Quest_IconUrl(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(QuestState_t) */ Types.QuestState Quest_State(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Quest_t) */ IntPtr Quest_Copy(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool Quest_Valid(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(int64_t) */ long Quest_StartTime(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void Quest_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(QuestMilestone_t) */ IntPtr Quest_CurrentMilestone(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(int64_t) */ long Quest_AcceptedTime(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr Quest_Id(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr Quest_Name(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
