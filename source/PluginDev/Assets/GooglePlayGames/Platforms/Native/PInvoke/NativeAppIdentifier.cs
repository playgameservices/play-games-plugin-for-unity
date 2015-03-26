// <copyright file="NativeAppIdentifier.cs" company="Google Inc.">
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

namespace GooglePlayGames.Native.PInvoke
{
    using GooglePlayGames.Native.Cwrapper;
    using System;
    using System.Runtime.InteropServices;

    using C = GooglePlayGames.Native.Cwrapper.NearbyConnectionTypes;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    internal class NativeAppIdentifier : BaseReferenceHolder
    {

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern IntPtr NearbyUtils_ConstructAppIdentifier(string appId);

        internal NativeAppIdentifier(IntPtr pointer)
            : base(pointer)
        {
        }

        internal string Id()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_arg, out_size) => C.AppIdentifier_GetIdentifier(SelfPtr(), out_arg, out_size));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.AppIdentifier_Dispose(selfPointer);
        }

        internal static NativeAppIdentifier FromString(string appId)
        {
            return new NativeAppIdentifier(NearbyUtils_ConstructAppIdentifier(appId));
        }
    }
}
#endif // #if (UNITY_ANDROID)
