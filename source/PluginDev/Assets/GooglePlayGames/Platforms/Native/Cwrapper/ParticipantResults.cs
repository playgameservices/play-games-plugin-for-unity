// <copyright file="ParticipantResults.cs" company="Google Inc.">
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
    using System.Runtime.InteropServices;

internal static class ParticipantResults
    {
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ParticipantResults_t) */ IntPtr ParticipantResults_WithResult(
            HandleRef self,
         /* from(char const *) */string participant_id,
         /* from(uint32_t) */uint placing,
         /* from(MatchResult_t) */Types.MatchResult result);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool ParticipantResults_Valid(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(MatchResult_t) */ Types.MatchResult ParticipantResults_MatchResultForParticipant(
            HandleRef self,
         /* from(char const *) */string participant_id);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint32_t) */ uint ParticipantResults_PlaceForParticipant(
            HandleRef self,
         /* from(char const *) */string participant_id);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool ParticipantResults_HasResultsForParticipant(
            HandleRef self,
         /* from(char const *) */string participant_id);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void ParticipantResults_Dispose(
            HandleRef self);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
