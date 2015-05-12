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

namespace GooglePlayGames.Native.Cwrapper
{

    using System;
    using System.Runtime.InteropServices;

    internal static class EventManager
    {
        
        internal delegate void FetchAllCallback(
        /* from(EventManager_FetchAllResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void FetchCallback(
        /* from(EventManager_FetchResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void EventManager_FetchAll(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(EventManager_FetchAllCallback_t) */FetchAllCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void EventManager_Fetch(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(char const *) */string event_id,
         /* from(EventManager_FetchCallback_t) */FetchCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void EventManager_Increment(
            HandleRef self,
         /* from(char const *) */string event_id,
         /* from(uint32_t) */uint steps);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void EventManager_FetchAllResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus EventManager_FetchAllResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr EventManager_FetchAllResponse_GetData(
            HandleRef self,
         /* from(Event_t *) */IntPtr[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void EventManager_FetchResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus EventManager_FetchResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Event_t) */ IntPtr EventManager_FetchResponse_GetData(
            HandleRef self);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
