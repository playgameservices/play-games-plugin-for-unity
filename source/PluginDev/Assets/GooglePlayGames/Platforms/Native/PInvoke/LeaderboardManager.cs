/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
using System;
using GooglePlayGames.Native.PInvoke;
using System.Runtime.InteropServices;
using GooglePlayGames.OurUtils;

using C = GooglePlayGames.Native.Cwrapper.LeaderboardManager;

using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native.PInvoke {
internal class LeaderboardManager {

    private readonly GameServices mServices;

    internal LeaderboardManager (GameServices services) {
        mServices = Misc.CheckNotNull(services);
    }

    internal void SubmitScore(string leaderboardId, long score) {
        Misc.CheckNotNull(leaderboardId);
        Logger.d("Native Submitting score: " + score + " for lb " + leaderboardId);
        // Note, we pass empty-string as the metadata - this is ignored by the native SDK.
        C.LeaderboardManager_SubmitScore(mServices.AsHandle(), leaderboardId, (ulong) score, "");
    }

	private void SummaryScoreResponse(string leaderboardId, Action<FetchSummaryScoreResponse> callback) {		
		C.LeaderboardManager_FetchScoreSummary(mServices.AsHandle(), 
		                                       Types.DataSource.CACHE_OR_NETWORK, 
		                                       leaderboardId,
		                                       Types.LeaderboardTimeSpan.ALL_TIME,
		                                       Types.LeaderboardCollection.PUBLIC,
		                                       InternalFetchScoreSummaryCallback,
			                                   Callbacks.ToIntPtr(callback, FetchSummaryScoreResponse.FromPointer));
	}

	internal void CurrentScoreValue(string leaderboardId, Action<ulong> callback) {		
		
		Action<FetchSummaryScoreResponse> action = response => {
			if (response != null) {
				callback(response.ScoreSummary().ScoreValue());				
			}else{
				callback(System.UInt64.MaxValue);
			}
		};
		
		SummaryScoreResponse(leaderboardId, action);
	}


	[AOT.MonoPInvokeCallback(typeof(C.FetchScoreSummaryCallback))]
	private static void InternalFetchScoreSummaryCallback(IntPtr response, IntPtr data) {
			Callbacks.PerformInternalCallback("LeaderboardManager#InternalFetchScoreSummaryCallback",
		                                  Callbacks.Type.Temporary, response, data);
	}

	internal class FetchSummaryScoreResponse : BaseReferenceHolder {
			internal FetchSummaryScoreResponse (IntPtr selfPointer) : base(selfPointer) {
		}
		
		internal Status.ResponseStatus Status() {
			return C.LeaderboardManager_FetchScoreSummaryResponse_GetStatus(SelfPtr());
		}
		
		internal NativeScoreSummary ScoreSummary() {
			return new NativeScoreSummary(C.LeaderboardManager_FetchScoreSummaryResponse_GetData(SelfPtr()));
		}
		
		protected override void CallDispose(HandleRef selfPointer) {
			C.LeaderboardManager_FetchScoreSummaryResponse_Dispose(SelfPtr());
		}
		
		internal static FetchSummaryScoreResponse FromPointer(IntPtr selfPointer) {
			if (PInvokeUtilities.IsNull(selfPointer)) {
				return null;
			}
			
			return new FetchSummaryScoreResponse(selfPointer);
		}
	}


    internal void ShowAllUI(Action<Status.UIStatus> callback) {
        Misc.CheckNotNull(callback);

        C.LeaderboardManager_ShowAllUI(mServices.AsHandle(), Callbacks.InternalShowUICallback,
            Callbacks.ToIntPtr(callback));
    }

    internal void ShowUI(string leaderboardId, Action<Status.UIStatus> callback) {
        Misc.CheckNotNull(callback);

        C.LeaderboardManager_ShowUI(mServices.AsHandle(), leaderboardId,
            Callbacks.InternalShowUICallback, Callbacks.ToIntPtr(callback));
    }
}
}


#endif
