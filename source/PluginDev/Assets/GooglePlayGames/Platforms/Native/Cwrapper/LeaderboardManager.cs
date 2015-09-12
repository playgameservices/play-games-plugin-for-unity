// <copyright file="LeaderboardManager.cs" company="Google Inc.">
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

    internal static class LeaderboardManager
    {
        internal delegate void FetchCallback(
        /* from(LeaderboardManager_FetchResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void FetchAllCallback(
        /* from(LeaderboardManager_FetchAllResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void FetchScorePageCallback(
        /* from(LeaderboardManager_FetchScorePageResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void FetchScoreSummaryCallback(
        /* from(LeaderboardManager_FetchScoreSummaryResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void FetchAllScoreSummariesCallback(
        /* from(LeaderboardManager_FetchAllScoreSummariesResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void ShowAllUICallback(
        /* from(UIStatus_t) */ CommonErrorStatus.UIStatus arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void ShowUICallback(
        /* from(UIStatus_t) */ CommonErrorStatus.UIStatus arg0,
        /* from(void *) */ IntPtr arg1);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchAll(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(LeaderboardManager_FetchAllCallback_t) */FetchAllCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchScoreSummary(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(char const *) */string leaderboard_id,
         /* from(LeaderboardTimeSpan_t) */Types.LeaderboardTimeSpan time_span,
         /* from(LeaderboardCollection_t) */Types.LeaderboardCollection collection,
         /* from(LeaderboardManager_FetchScoreSummaryCallback_t) */FetchScoreSummaryCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ScorePage_ScorePageToken_t) */ IntPtr LeaderboardManager_ScorePageToken(
            HandleRef self,
         /* from(char const *) */string leaderboard_id,
         /* from(LeaderboardStart_t) */Types.LeaderboardStart start,
         /* from(LeaderboardTimeSpan_t) */Types.LeaderboardTimeSpan time_span,
         /* from(LeaderboardCollection_t) */Types.LeaderboardCollection collection);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_ShowAllUI(
            HandleRef self,
         /* from(LeaderboardManager_ShowAllUICallback_t) */ShowAllUICallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchScorePage(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(ScorePage_ScorePageToken_t) */IntPtr token,
         /* from(uint32_t) */uint max_results,
         /* from(LeaderboardManager_FetchScorePageCallback_t) */FetchScorePageCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchAllScoreSummaries(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(char const *) */string leaderboard_id,
         /* from(LeaderboardManager_FetchAllScoreSummariesCallback_t) */FetchAllScoreSummariesCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_ShowUI(
            HandleRef self,
         /* from(char const *) */string leaderboard_id,
         /* from(LeaderboardTimeSpan_t) */Types.LeaderboardTimeSpan time_span,
         /* from(LeaderboardManager_ShowUICallback_t) */ShowUICallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_Fetch(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(char const *) */string leaderboard_id,
         /* from(LeaderboardManager_FetchCallback_t) */FetchCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_SubmitScore(
            HandleRef self,
         /* from(char const *) */string leaderboard_id,
         /* from(uint64_t) */ulong score,
         /* from(char const *) */string metadata);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus LeaderboardManager_FetchResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Leaderboard_t) */ IntPtr LeaderboardManager_FetchResponse_GetData(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchAllResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus LeaderboardManager_FetchAllResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr LeaderboardManager_FetchAllResponse_GetData_Length(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(Leaderboard_t) */ IntPtr LeaderboardManager_FetchAllResponse_GetData_GetElement(
            HandleRef self,
         /* from(size_t) */UIntPtr index);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchScorePageResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus LeaderboardManager_FetchScorePageResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ScorePage_t) */ IntPtr LeaderboardManager_FetchScorePageResponse_GetData(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchScoreSummaryResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus LeaderboardManager_FetchScoreSummaryResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ScoreSummary_t) */ IntPtr LeaderboardManager_FetchScoreSummaryResponse_GetData(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void LeaderboardManager_FetchAllScoreSummariesResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus LeaderboardManager_FetchAllScoreSummariesResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr LeaderboardManager_FetchAllScoreSummariesResponse_GetData_Length(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ScoreSummary_t) */ IntPtr LeaderboardManager_FetchAllScoreSummariesResponse_GetData_GetElement(
            HandleRef self,
         /* from(size_t) */UIntPtr index);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
