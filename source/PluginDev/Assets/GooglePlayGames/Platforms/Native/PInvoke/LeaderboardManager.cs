// <copyright file="LeaderboardManager.cs" company="Google Inc.">
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.OurUtils;
    using C = GooglePlayGames.Native.Cwrapper.LeaderboardManager;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using UnityEngine.SocialPlatforms;

    internal class LeaderboardManager
    {

        private readonly GameServices mServices;

        internal LeaderboardManager(GameServices services)
        {
            mServices = Misc.CheckNotNull(services);
        }

        internal int LeaderboardMaxResults
        {
            get
            {
                return 25;
            }
        }

        internal void SubmitScore(string leaderboardId, long score, string metadata)
        {
            Misc.CheckNotNull(leaderboardId, "leaderboardId");
            Logger.d("Native Submitting score: " + score +
                " for lb " + leaderboardId + " with metadata: " + metadata);
            C.LeaderboardManager_SubmitScore(mServices.AsHandle(), leaderboardId,
                (ulong)score, metadata ?? "");
        }

        internal void ShowAllUI(Action<Status.UIStatus> callback)
        {
            Misc.CheckNotNull(callback);

            C.LeaderboardManager_ShowAllUI(mServices.AsHandle(), Callbacks.InternalShowUICallback,
                Callbacks.ToIntPtr(callback));
        }

        internal void ShowUI(string leaderboardId, 
            LeaderboardTimeSpan span, Action<Status.UIStatus> callback)
        {
            Misc.CheckNotNull(callback);

            C.LeaderboardManager_ShowUI(mServices.AsHandle(), leaderboardId,
                (Types.LeaderboardTimeSpan)span,
                Callbacks.InternalShowUICallback, 
                Callbacks.ToIntPtr(callback));
        }

        /// <summary>
        /// Loads the leaderboard data.  This is the "top level" call
        /// to load leaderboard data.  A token for fetching scores is created
        /// based on the parameters.
        /// </summary>
        /// <param name="leaderboardId">Leaderboard identifier.</param>
        /// <param name="start">Start of scores location</param>
        /// <param name="rowCount">Row count.</param>
        /// <param name="collection">Collection social or public</param>
        /// <param name="timeSpan">Time span of leaderboard</param>
        /// <param name="playerId">Player identifier.</param>
        /// <param name="callback">Callback.</param>
        public void LoadLeaderboardData(string leaderboardId,
            LeaderboardStart start,
            int rowCount,
            LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            string playerId, Action<LeaderboardScoreData> callback)
        {

            //Create a token we'll use to load scores later.
            NativeScorePageToken nativeToken = new NativeScorePageToken(
                                             C.LeaderboardManager_ScorePageToken(
                                                 mServices.AsHandle(),
                                                 leaderboardId,
                                                 (Types.LeaderboardStart)start,
                                                 (Types.LeaderboardTimeSpan)timeSpan,
                                                 (Types.LeaderboardCollection)collection));
            ScorePageToken token = new ScorePageToken(nativeToken, leaderboardId,
                                       collection, timeSpan);

           // First fetch the leaderboard to get the title
            C.LeaderboardManager_Fetch(mServices.AsHandle(),
                Types.DataSource.CACHE_OR_NETWORK,
                leaderboardId,
                InternalFetchCallback,
                Callbacks.ToIntPtr<FetchResponse>((rsp) =>
                    HandleFetch(token, rsp, playerId, rowCount, callback),
                    FetchResponse.FromPointer));

        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchCallback))]
        private static void InternalFetchCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback("LeaderboardManager#InternalFetchCallback",
                Callbacks.Type.Temporary, response, data);
        }

        /// <summary>
        /// Handles the fetch of a specific leaderboard definition.  This
        /// is called with the expectation that the leaderboard summary and
        /// scores are also needed.
        /// </summary>
        /// <param name="token">token for the current fetching request.</param>
        /// <param name="response">Response.</param>
        /// <param name="selfPlayerId">Self player identifier.</param>
        /// <param name="maxResults">Number of scores to return.</param>
        /// <param name="callback">Callback.</param>
        internal void HandleFetch(ScorePageToken token,
            FetchResponse response,
            string selfPlayerId,
            int maxResults,
            Action<LeaderboardScoreData> callback)
        {

            LeaderboardScoreData data =
                new LeaderboardScoreData(
                    token.LeaderboardId, (ResponseStatus)response.GetStatus());

            if (response.GetStatus() != Status.ResponseStatus.VALID &&
                response.GetStatus() != Status.ResponseStatus.VALID_BUT_STALE)
            {
                Logger.w("Error returned from fetch: " + response.GetStatus());
                callback(data);
                return;
            }

            data.Title = response.Leaderboard().Title();
            data.Id = token.LeaderboardId;

            // now fetch the summary of the leaderboard.

            C.LeaderboardManager_FetchScoreSummary(mServices.AsHandle(),
                Types.DataSource.CACHE_OR_NETWORK,
                token.LeaderboardId,
                (Types.LeaderboardTimeSpan)token.TimeSpan,
                (Types.LeaderboardCollection)token.Collection,
                InternalFetchSummaryCallback,
                Callbacks.ToIntPtr<FetchScoreSummaryResponse>((rsp) =>
                    HandleFetchScoreSummary(data, rsp, selfPlayerId, maxResults, token, callback),
                FetchScoreSummaryResponse.FromPointer)
            );
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchScoreSummaryCallback))]
        private static void InternalFetchSummaryCallback(IntPtr response, IntPtr data)
        {
          Callbacks.PerformInternalCallback("LeaderboardManager#InternalFetchSummaryCallback",
                Callbacks.Type.Temporary, response, data);
        }

        internal void HandleFetchScoreSummary(LeaderboardScoreData data,
            FetchScoreSummaryResponse response,
            string selfPlayerId, int maxResults,
            ScorePageToken token, Action<LeaderboardScoreData> callback)
        {
            if (response.GetStatus() != Status.ResponseStatus.VALID &&
                response.GetStatus() != Status.ResponseStatus.VALID_BUT_STALE)
            {
                Logger.w("Error returned from fetchScoreSummary: " + response);
                data.Status = (ResponseStatus)response.GetStatus();
                callback(data);
                return;
            }

            NativeScoreSummary summary = response.GetScoreSummary();
            data.ApproximateCount = summary.ApproximateResults();
            data.PlayerScore = summary.LocalUserScore().AsScore(data.Id, selfPlayerId);


            // if the maxResults is 0, no scores are needed, so we are done.
            if (maxResults <= 0)
            {
                callback(data);
                return;
            }

            LoadScorePage(data, maxResults, token, callback);
        }

        /// <summary>
        /// Loads the score page. This is used to page through the rows
        /// of leaderboard scores.
        /// </summary>
        /// <param name="data">Data - partially completed result data, can be null</param>
        /// <param name="maxResults">Max results to return</param>
        /// <param name="token">Token to use for getting the score page,</param>
        /// <param name="callback">Callback.</param>
        public void LoadScorePage(LeaderboardScoreData data,
            int maxResults, ScorePageToken token,
            Action<LeaderboardScoreData> callback)
        {

            if (data == null)
            {
                data = new LeaderboardScoreData(token.LeaderboardId);
            }

            NativeScorePageToken nativeToken = (NativeScorePageToken)token.InternalObject;
            C.LeaderboardManager_FetchScorePage(mServices.AsHandle(),
                Types.DataSource.CACHE_OR_NETWORK,
                nativeToken.AsPointer(),
                (uint)maxResults,
                InternalFetchScorePage,
                Callbacks.ToIntPtr<FetchScorePageResponse>((rsp) => {
                    HandleFetchScorePage(data, token, rsp, callback);
                }, FetchScorePageResponse.FromPointer)
            );
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchScorePageCallback))]
        private static void InternalFetchScorePage(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback("LeaderboardManager#InternalFetchScorePage",
                Callbacks.Type.Temporary, response, data);
        }

        internal void HandleFetchScorePage(LeaderboardScoreData data,
            ScorePageToken token,
            FetchScorePageResponse rsp, Action<LeaderboardScoreData> callback)
        {
            data.Status = (ResponseStatus)rsp.GetStatus();
            // add the scores that match the criteria
            if (rsp.GetStatus() != Status.ResponseStatus.VALID &&
                rsp.GetStatus() != Status.ResponseStatus.VALID_BUT_STALE)
            {
                callback(data);
            }

            NativeScorePage page = rsp.GetScorePage();

            if (!page.Valid())
            {
                callback(data);
            }

            if (page.HasNextScorePage())
            {
                data.NextPageToken = new ScorePageToken(
                                               page.GetNextScorePageToken(),
                                               token.LeaderboardId,
                                               token.Collection,
                                               token.TimeSpan);

            }
            if (page.HasPrevScorePage())
            {
                data.PrevPageToken = new ScorePageToken(
                    page.GetPreviousScorePageToken(),
                    token.LeaderboardId,
                    token.Collection,
                    token.TimeSpan);
            }

            foreach (NativeScoreEntry ent in page)
            {
                data.AddScore(ent.AsScore(data.Id));
            }

            callback(data);
        }
    }


    internal class FetchScorePageResponse : BaseReferenceHolder
    {

        internal FetchScorePageResponse(IntPtr selfPointer) : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.LeaderboardManager_FetchScorePageResponse_Dispose(SelfPtr());
        }

        internal Status.ResponseStatus GetStatus()
        {
            return C.LeaderboardManager_FetchScorePageResponse_GetStatus(SelfPtr());
        }

        internal NativeScorePage GetScorePage()
        {
            return NativeScorePage.FromPointer(
                C.LeaderboardManager_FetchScorePageResponse_GetData(SelfPtr()));
        }

        internal static FetchScorePageResponse FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new FetchScorePageResponse(pointer);
        }
    }
    internal class FetchResponse : BaseReferenceHolder
    {
        internal FetchResponse(IntPtr selfPointer) : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.LeaderboardManager_FetchResponse_Dispose(SelfPtr());
        }

        internal NativeLeaderboard Leaderboard()
        {
            return NativeLeaderboard.FromPointer(
                C.LeaderboardManager_FetchResponse_GetData(SelfPtr()));
        }

        internal Status.ResponseStatus GetStatus()
        {
            return C.LeaderboardManager_FetchResponse_GetStatus(SelfPtr());
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

    internal class FetchScoreSummaryResponse : BaseReferenceHolder
    {
        internal FetchScoreSummaryResponse(IntPtr selfPointer) : base(selfPointer)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.LeaderboardManager_FetchScoreSummaryResponse_Dispose(selfPointer);
        }

        internal Status.ResponseStatus GetStatus()
        {
            return C.LeaderboardManager_FetchScoreSummaryResponse_GetStatus(SelfPtr());
        }

        internal NativeScoreSummary GetScoreSummary()
        {
            return NativeScoreSummary.FromPointer(
                C.LeaderboardManager_FetchScoreSummaryResponse_GetData(SelfPtr()
                ));
        }

        internal static FetchScoreSummaryResponse FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new FetchScoreSummaryResponse(pointer);
        }
    }
}


#endif
