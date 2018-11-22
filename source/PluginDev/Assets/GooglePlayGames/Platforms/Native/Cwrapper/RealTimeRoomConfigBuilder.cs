// <copyright file="RealTimeRoomConfigBuilder.cs" company="Google Inc.">
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

#if UNITY_ANDROID
namespace GooglePlayGames.Native.Cwrapper
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class RealTimeRoomConfigBuilder
    {
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void RealTimeRoomConfig_Builder_PopulateFromPlayerSelectUIResponse(
            HandleRef self,
         /* from(RealTimeMultiplayerManager_PlayerSelectUIResponse_t) */IntPtr response);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void RealTimeRoomConfig_Builder_SetVariant(
            HandleRef self,
         /* from(uint32_t) */uint variant);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void RealTimeRoomConfig_Builder_AddPlayerToInvite(
            HandleRef self,
         /* from(char const *) */string player_id);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(RealTimeRoomConfig_Builder_t) */ IntPtr RealTimeRoomConfig_Builder_Construct();

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void RealTimeRoomConfig_Builder_SetExclusiveBitMask(
            HandleRef self,
         /* from(uint64_t) */ulong exclusive_bit_mask);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void RealTimeRoomConfig_Builder_SetMaximumAutomatchingPlayers(
            HandleRef self,
         /* from(uint32_t) */uint maximum_automatching_players);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(RealTimeRoomConfig_t) */ IntPtr RealTimeRoomConfig_Builder_Create(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void RealTimeRoomConfig_Builder_SetMinimumAutomatchingPlayers(
            HandleRef self,
         /* from(uint32_t) */uint minimum_automatching_players);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void RealTimeRoomConfig_Builder_Dispose(
            HandleRef self);
    }
}
#endif //UNITY_ANDROID

