// <copyright file="StatsManager.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
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
namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.Native.Cwrapper;
    using C = GooglePlayGames.Native.Cwrapper.StatsManager;

    internal class StatsManager
    {
        private readonly GameServices mServices;

        internal StatsManager(GameServices services)
        {
            mServices = Misc.CheckNotNull(services);
        }

        internal void FetchForPlayer(Action<FetchForPlayerResponse> callback)
        {
            Misc.CheckNotNull(callback);

            C.StatsManager_FetchForPlayer(mServices.AsHandle(), Types.DataSource.CACHE_OR_NETWORK,
                InternalFetchForPlayerCallback,
                Callbacks.ToIntPtr<FetchForPlayerResponse>(callback, FetchForPlayerResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchForPlayerCallback))]
        private static void InternalFetchForPlayerCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback("StatsManager#InternalFetchForPlayerCallback",
                Callbacks.Type.Temporary, response, data);
        }

        internal class FetchForPlayerResponse : BaseReferenceHolder
        {
            internal FetchForPlayerResponse(IntPtr selfPointer) : base(selfPointer)
            {
            }

            internal CommonErrorStatus.ResponseStatus Status()
            {
                return C.StatsManager_FetchForPlayerResponse_GetStatus(SelfPtr());
            }

            internal NativePlayerStats PlayerStats()
            {
                IntPtr p = C.StatsManager_FetchForPlayerResponse_GetData(SelfPtr());
                return new NativePlayerStats(p);
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.StatsManager_FetchForPlayerResponse_Dispose(selfPointer);
            }

            internal static FetchForPlayerResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero)) {
                    return null;
                }

                return new FetchForPlayerResponse(pointer);
            }
        }
    }
}
#endif //UNITY_ANDROID

