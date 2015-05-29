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

using C = GooglePlayGames.Native.Cwrapper.PlayerManager;
using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native {
internal class PlayerManager {

    private readonly GameServices mGameServices;

    internal PlayerManager (GameServices services) {
        mGameServices = Misc.CheckNotNull(services);
    }

    internal void FetchSelf(Action<FetchSelfResponse> callback) {
        C.PlayerManager_FetchSelf(mGameServices.AsHandle(), Types.DataSource.CACHE_OR_NETWORK,
            InternalFetchSelfCallback, Callbacks.ToIntPtr(callback, FetchSelfResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.FetchSelfCallback))]
    private static void InternalFetchSelfCallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback("PlayerManager#InternalFetchSelfCallback",
            Callbacks.Type.Temporary, response, data);
    }

    internal class FetchSelfResponse : BaseReferenceHolder {
        internal FetchSelfResponse (IntPtr selfPointer) : base(selfPointer) {
        }

        internal Status.ResponseStatus Status() {
            return C.PlayerManager_FetchSelfResponse_GetStatus(SelfPtr());
        }

        internal NativePlayer Self() {
            return new NativePlayer(C.PlayerManager_FetchSelfResponse_GetData(SelfPtr()));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.PlayerManager_FetchSelfResponse_Dispose(SelfPtr());
        }

        internal static FetchSelfResponse FromPointer(IntPtr selfPointer) {
            if (PInvokeUtilities.IsNull(selfPointer)) {
                return null;
            }

            return new FetchSelfResponse(selfPointer);
        }
    }
}
}


#endif
