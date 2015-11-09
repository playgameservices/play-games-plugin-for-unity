// <copyright file="InternalHooks.cs" company="Google Inc.">
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

    internal static class InternalHooks
    {

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void InternalHooks_ConfigureForUnityPlugin(HandleRef builder);


        #if UNITY_ANDROID
        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern IntPtr InternalHooks_GetApiClient(HandleRef services);
        #endif
    }
}
#endif
