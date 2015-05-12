// <copyright file="EventManager.cs" company="Google Inc.">
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

namespace GooglePlayGames.Native.PInvoke
{
    using GooglePlayGames.OurUtils;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using C = GooglePlayGames.Native.Cwrapper.EventManager;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    internal class EventManager
    {
        private readonly GameServices mServices;

        internal EventManager(GameServices services)
        {
            mServices = Misc.CheckNotNull(services);
        }

        internal void FetchAll(Types.DataSource source,
                           Action<FetchAllResponse> callback)
        {
            C.EventManager_FetchAll(
                mServices.AsHandle(),
                source,
                InternalFetchAllCallback,
                Callbacks.ToIntPtr<FetchAllResponse>(callback, FetchAllResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchAllCallback))]
        internal static void InternalFetchAllCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "EventManager#FetchAllCallback", Callbacks.Type.Temporary, response, data);
        }

        internal void Fetch(Types.DataSource source, string eventId,
                        Action<FetchResponse> callback)
        {
            C.EventManager_Fetch(
                mServices.AsHandle(),
                source,
                eventId,
                InternalFetchCallback,
                Callbacks.ToIntPtr<FetchResponse>(callback, FetchResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchCallback))]
        internal static void InternalFetchCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "EventManager#FetchCallback", Callbacks.Type.Temporary, response, data);
        }

        internal void Increment(string eventId, uint steps)
        {
            C.EventManager_Increment(mServices.AsHandle(), eventId, steps);
        }

        internal class FetchResponse : BaseReferenceHolder
        {
            internal FetchResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.ResponseStatus ResponseStatus()
            {
                return C.EventManager_FetchResponse_GetStatus(SelfPtr());
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            internal NativeEvent Data()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                return new NativeEvent(C.EventManager_FetchResponse_GetData(SelfPtr()));
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.EventManager_FetchResponse_Dispose(selfPointer);
            }

            internal static FetchResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new FetchResponse(pointer);
            }
        }

        internal class FetchAllResponse : BaseReferenceHolder
        {
            internal FetchAllResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.ResponseStatus ResponseStatus()
            {
                return C.EventManager_FetchAllResponse_GetStatus(SelfPtr());
            }

            internal List<NativeEvent> Data()
            {
                IntPtr[] events = PInvokeUtilities.OutParamsToArray<IntPtr>((out_arg, out_size) => C.EventManager_FetchAllResponse_GetData(SelfPtr(), out_arg, out_size));

                return events.Select(ptr => new NativeEvent(ptr)).ToList();
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.EventManager_FetchAllResponse_Dispose(selfPointer);
            }

            internal static FetchAllResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new FetchAllResponse(pointer);
            }
        }
    }
}

#endif // (UNITY_ANDROID || UNITY_IPHONE)
