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
#if (UNITY_ANDROID || UNITY_IPHONE)
using System;
using GooglePlayGames.Native.PInvoke;
using System.Runtime.InteropServices;
using GooglePlayGames.OurUtils;
using System.Collections.Generic;
using GooglePlayGames.Native.Cwrapper;

using C = GooglePlayGames.Native.Cwrapper.TurnBasedMatchConfig;
using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native.PInvoke {
internal class TurnBasedMatchConfig : BaseReferenceHolder {

    internal TurnBasedMatchConfig(IntPtr selfPointer) : base(selfPointer) {
    }

    private string PlayerIdAtIndex(UIntPtr index) {
        return PInvokeUtilities.OutParamsToString(
            (out_string, size) => C.TurnBasedMatchConfig_PlayerIdsToInvite_GetElement(
                SelfPtr(), index, out_string, size));
    }

    internal IEnumerator<string> PlayerIdsToInvite() {
        return PInvokeUtilities.ToEnumerator<string>(
            C.TurnBasedMatchConfig_PlayerIdsToInvite_Length(SelfPtr()),
            PlayerIdAtIndex
        );
    }

    internal uint Variant() {
        return C.TurnBasedMatchConfig_Variant(SelfPtr());
    }

    internal long ExclusiveBitMask() {
        return C.TurnBasedMatchConfig_ExclusiveBitMask(SelfPtr());
    }

    internal uint MinimumAutomatchingPlayers() {
        return C.TurnBasedMatchConfig_MinimumAutomatchingPlayers(SelfPtr());
    }

    internal uint MaximumAutomatchingPlayers() {
        return C.TurnBasedMatchConfig_MaximumAutomatchingPlayers(SelfPtr());
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.TurnBasedMatchConfig_Dispose(selfPointer);
    }
}
}

#endif
