// <copyright file="PlayerStats.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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

    internal static class PlayerStats
    {
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_Valid (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void PlayerStats_Dispose (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_HasAverageSessionLength (
            HandleRef self);

        [DllImport (SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(float) */ float PlayerStats_AverageSessionLength (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_HasChurnProbability (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(float) */ float PlayerStats_ChurnProbability (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_HasDaysSinceLastPlayed (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint32_t) */ int PlayerStats_DaysSinceLastPlayed (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_HasNumberOfPurchases (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint32_t) */ int PlayerStats_NumberOfPurchases (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_HasNumberOfSessions (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(uint32_t) */ int PlayerStats_NumberOfSessions (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_HasSessionPercentile (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(float) */ float PlayerStats_SessionPercentile (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        [return: MarshalAs (UnmanagedType.I1)]
        internal static extern /* from(bool) */ bool PlayerStats_HasSpendPercentile (
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(float) */ float PlayerStats_SpendPercentile (
            HandleRef self);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)