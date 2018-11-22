// <copyright file="MultiplayerInvitation.cs" company="Google Inc.">
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

internal static class MultiplayerInvitation
    {
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint32_t) */ uint MultiplayerInvitation_AutomatchingSlotsAvailable(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(MultiplayerParticipant_t) */ IntPtr MultiplayerInvitation_InvitingParticipant(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint32_t) */ uint MultiplayerInvitation_Variant(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint64_t) */ ulong MultiplayerInvitation_CreationTime(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr MultiplayerInvitation_Participants_Length(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(MultiplayerParticipant_t) */ IntPtr MultiplayerInvitation_Participants_GetElement(
            HandleRef self,
         /* from(size_t) */UIntPtr index);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool MultiplayerInvitation_Valid(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(MultiplayerInvitationType_t) */ Types.MultiplayerInvitationType MultiplayerInvitation_Type(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr MultiplayerInvitation_Id(
            HandleRef self,
            [In, Out] /* from(char *) */byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void MultiplayerInvitation_Dispose(
            HandleRef self);
    }
}
#endif //UNITY_ANDROID

