// <copyright file="MultiplayerParticipant.cs" company="Google Inc.">
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
    using System.Text;

    internal static class MultiplayerParticipant
    {
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ParticipantStatus_t) */ Types.ParticipantStatus MultiplayerParticipant_Status(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint32_t) */ uint MultiplayerParticipant_MatchRank(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool MultiplayerParticipant_IsConnectedToRoom(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr MultiplayerParticipant_DisplayName(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool MultiplayerParticipant_HasPlayer(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr MultiplayerParticipant_AvatarUrl(
            HandleRef self,
         /* from(ImageResolution_t) */Types.ImageResolution resolution,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(MatchResult_t) */ Types.MatchResult MultiplayerParticipant_MatchResult(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Player_t) */ IntPtr MultiplayerParticipant_Player(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void MultiplayerParticipant_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool MultiplayerParticipant_Valid(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool MultiplayerParticipant_HasMatchResult(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr MultiplayerParticipant_Id(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
