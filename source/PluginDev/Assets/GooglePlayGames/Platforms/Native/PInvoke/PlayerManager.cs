// <copyright file="PlayerManager.cs" company="Google Inc.">
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

#if UNITY_ANDROID

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.OurUtils;
    using C = GooglePlayGames.Native.Cwrapper.PlayerManager;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal class PlayerManager
    {

        private readonly GameServices mGameServices;

        internal PlayerManager(GameServices services)
        {
            mGameServices = Misc.CheckNotNull(services);
        }

        internal void FetchSelf(Action<FetchSelfResponse> callback)
        {
            C.PlayerManager_FetchSelf(mGameServices.AsHandle(), Types.DataSource.CACHE_OR_NETWORK,
                InternalFetchSelfCallback, Callbacks.ToIntPtr(callback, FetchSelfResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchSelfCallback))]
        private static void InternalFetchSelfCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback("PlayerManager#InternalFetchSelfCallback",
                Callbacks.Type.Temporary, response, data);
        }

        internal void FetchList(string[] userIds, Action<NativePlayer[]> callback)
        {
            FetchResponseCollector coll = new FetchResponseCollector();
            coll.pendingCount = userIds.Length;
            coll.callback = callback;
            foreach(string id in userIds)
            {
                C.PlayerManager_Fetch(mGameServices.AsHandle(),
                    Types.DataSource.CACHE_OR_NETWORK,
                    id,
                    InternalFetchCallback,
                    Callbacks.ToIntPtr<FetchResponse>(
                        (rsp) => HandleFetchResponse(coll, rsp),
                        FetchResponse.FromPointer)
                    );
            }
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchCallback))]
        private static void InternalFetchCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback("PlayerManager#InternalFetchCallback",
                Callbacks.Type.Temporary, response, data);
        }

        internal void HandleFetchResponse(FetchResponseCollector collector,
            FetchResponse resp)
        {
            if (resp.Status() == Status.ResponseStatus.VALID ||
                resp.Status() == Status.ResponseStatus.VALID_BUT_STALE)
            {
                NativePlayer player = resp.GetPlayer();
                collector.results.Add(player);
            }
            collector.pendingCount--;
            if (collector.pendingCount == 0)
            {
                collector.callback(collector.results.ToArray());
            }
        }

        internal void FetchFriends(Action<BasicApi.ResponseStatus, List<Player>> callback)
        {
            C.PlayerManager_FetchConnected(mGameServices.AsHandle(),
                Types.DataSource.CACHE_OR_NETWORK,
                InternalFetchConnectedCallback,
                Callbacks.ToIntPtr<FetchListResponse>(
                    (rsp) => HandleFetchCollected(rsp, callback),
                    FetchListResponse.FromPointer)
            );
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchListCallback))]
        private static void InternalFetchConnectedCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback("PlayerManager#InternalFetchConnectedCallback",
                Callbacks.Type.Temporary, response, data);
        }

        internal void HandleFetchCollected(FetchListResponse rsp,
            Action<GooglePlayGames.BasicApi.ResponseStatus, List<Player>> callback)
        {
            List<Player> players = new List<Player>();
            if (rsp.Status() == Status.ResponseStatus.VALID ||
                rsp.Status() == Status.ResponseStatus.VALID_BUT_STALE)
            {
                Logger.d("Got "  + rsp.Length().ToUInt64() + " players");
                foreach (NativePlayer p in rsp)
                {
                   players.Add(p.AsPlayer());
                }
            }
            callback((BasicApi.ResponseStatus)rsp.Status(), players);
        }

        internal class FetchListResponse : BaseReferenceHolder, IEnumerable<NativePlayer>
        {
            internal FetchListResponse(IntPtr selfPointer) : base(selfPointer)
            {
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.PlayerManager_FetchListResponse_Dispose(SelfPtr());
            }

            internal Status.ResponseStatus Status()
            {
                return C.PlayerManager_FetchListResponse_GetStatus(SelfPtr());
            }

#region IEnumerable implementation

            public IEnumerator<NativePlayer> GetEnumerator()
            {
                return PInvokeUtilities.ToEnumerator<NativePlayer>(Length(),
                (index) => GetElement(index));
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

#endregion

            internal UIntPtr Length()
            {
                return C.PlayerManager_FetchListResponse_GetData_Length(SelfPtr());
            }

            internal NativePlayer GetElement(UIntPtr index)
            {
                if (index.ToUInt64() >= Length().ToUInt64())
                {
                    throw new ArgumentOutOfRangeException();
                }
                return new NativePlayer(
                    C.PlayerManager_FetchListResponse_GetData_GetElement(SelfPtr(), index));
            }

            internal static FetchListResponse FromPointer(IntPtr selfPointer)
            {
                if (PInvokeUtilities.IsNull(selfPointer))
                {
                    return null;
                }

                return new FetchListResponse(selfPointer);
            }
        }

        internal class FetchResponseCollector
        {
            internal int pendingCount;
            internal List<NativePlayer> results = new List<NativePlayer>();
            internal Action<NativePlayer[]> callback;
        }

        internal class FetchResponse : BaseReferenceHolder
        {
            internal FetchResponse(IntPtr selfPointer) : base(selfPointer)
            {
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.PlayerManager_FetchResponse_Dispose(SelfPtr());
            }

            internal NativePlayer GetPlayer()
            {
                return new NativePlayer(
                    C.PlayerManager_FetchResponse_GetData(SelfPtr())
                );
            }

            internal Status.ResponseStatus Status()
            {
                return C.PlayerManager_FetchResponse_GetStatus(SelfPtr());
            }

            internal static FetchResponse FromPointer(IntPtr selfPointer)
            {
                if (PInvokeUtilities.IsNull(selfPointer))
                {
                    return null;
                }

                return new FetchResponse(selfPointer);
            }
        }

        internal class FetchSelfResponse : BaseReferenceHolder
        {
            internal FetchSelfResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.ResponseStatus Status()
            {
                return C.PlayerManager_FetchSelfResponse_GetStatus(SelfPtr());
            }

            internal NativePlayer Self()
            {
                return new NativePlayer(C.PlayerManager_FetchSelfResponse_GetData(SelfPtr()));
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.PlayerManager_FetchSelfResponse_Dispose(SelfPtr());
            }

            internal static FetchSelfResponse FromPointer(IntPtr selfPointer)
            {
                if (PInvokeUtilities.IsNull(selfPointer))
                {
                    return null;
                }

                return new FetchSelfResponse(selfPointer);
            }
        }
    }
}


#endif
