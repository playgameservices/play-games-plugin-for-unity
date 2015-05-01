// <copyright file="QuestManager.cs" company="Google Inc.">
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

    internal static class QuestManager
    {
        internal delegate void FetchCallback(
        /* from(QuestManager_FetchResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void FetchListCallback(
        /* from(QuestManager_FetchListResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void AcceptCallback(
        /* from(QuestManager_AcceptResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void ClaimMilestoneCallback(
        /* from(QuestManager_ClaimMilestoneResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void QuestUICallback(
        /* from(QuestManager_QuestUIResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_FetchList(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(int32_t) */int fetch_flags,
         /* from(QuestManager_FetchListCallback_t) */FetchListCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_Accept(
            HandleRef self,
         /* from(Quest_t) */IntPtr quest,
         /* from(QuestManager_AcceptCallback_t) */AcceptCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_ShowAllUI(
            HandleRef self,
         /* from(QuestManager_QuestUICallback_t) */QuestUICallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_ShowUI(
            HandleRef self,
         /* from(Quest_t) */IntPtr quest,
         /* from(QuestManager_QuestUICallback_t) */QuestUICallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_ClaimMilestone(
            HandleRef self,
         /* from(QuestMilestone_t) */IntPtr milestone,
         /* from(QuestManager_ClaimMilestoneCallback_t) */ClaimMilestoneCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_Fetch(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(char const *) */string quest_id,
         /* from(QuestManager_FetchCallback_t) */FetchCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_FetchResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus QuestManager_FetchResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Quest_t) */ IntPtr QuestManager_FetchResponse_GetData(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_FetchListResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus QuestManager_FetchListResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr QuestManager_FetchListResponse_GetData_Length(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Quest_t) */ IntPtr QuestManager_FetchListResponse_GetData_GetElement(
            HandleRef self,
         /* from(size_t) */UIntPtr index);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_AcceptResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(QuestAcceptStatus_t) */ CommonErrorStatus.QuestAcceptStatus QuestManager_AcceptResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Quest_t) */ IntPtr QuestManager_AcceptResponse_GetAcceptedQuest(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_ClaimMilestoneResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(QuestClaimMilestoneStatus_t) */ CommonErrorStatus.QuestClaimMilestoneStatus QuestManager_ClaimMilestoneResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(QuestMilestone_t) */ IntPtr QuestManager_ClaimMilestoneResponse_GetClaimedMilestone(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Quest_t) */ IntPtr QuestManager_ClaimMilestoneResponse_GetQuest(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void QuestManager_QuestUIResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(UIStatus_t) */ CommonErrorStatus.UIStatus QuestManager_QuestUIResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Quest_t) */ IntPtr QuestManager_QuestUIResponse_GetAcceptedQuest(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(QuestMilestone_t) */ IntPtr QuestManager_QuestUIResponse_GetMilestoneToClaim(
            HandleRef self);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
