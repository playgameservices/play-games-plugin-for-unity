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
using System.Collections.Generic;
using GooglePlayGames.Native.Cwrapper;

using C = GooglePlayGames.Native.Cwrapper.TurnBasedMatchConfigBuilder;
using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native.PInvoke {
internal class TurnBasedMatchConfigBuilder : BaseReferenceHolder {

    private TurnBasedMatchConfigBuilder(IntPtr selfPointer) : base(selfPointer) {
    }

    internal TurnBasedMatchConfigBuilder PopulateFromUIResponse(PlayerSelectUIResponse response) {
        C.TurnBasedMatchConfig_Builder_PopulateFromPlayerSelectUIResponse(SelfPtr(),
            response.AsPointer());

        return this;
    }

    internal TurnBasedMatchConfigBuilder SetVariant(uint variant) {
        C.TurnBasedMatchConfig_Builder_SetVariant(SelfPtr(), variant);
        return this;
    }

    internal TurnBasedMatchConfigBuilder AddInvitedPlayer(string playerId) {
        C.TurnBasedMatchConfig_Builder_AddPlayerToInvite(SelfPtr(), playerId);
        return this;
    }

    internal TurnBasedMatchConfigBuilder SetExclusiveBitMask(ulong bitmask) {
        C.TurnBasedMatchConfig_Builder_SetExclusiveBitMask(SelfPtr(), bitmask);
        return this;
    }

    internal TurnBasedMatchConfigBuilder SetMinimumAutomatchingPlayers(uint minimum) {
        C.TurnBasedMatchConfig_Builder_SetMinimumAutomatchingPlayers(SelfPtr(), minimum);
        return this;
    }

    internal TurnBasedMatchConfigBuilder SetMaximumAutomatchingPlayers(uint maximum) {
        C.TurnBasedMatchConfig_Builder_SetMaximumAutomatchingPlayers(SelfPtr(), maximum);
        return this;
    }

    internal TurnBasedMatchConfig Build() {
        return new TurnBasedMatchConfig(C.TurnBasedMatchConfig_Builder_Create(SelfPtr()));
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.TurnBasedMatchConfig_Builder_Dispose(selfPointer);
    }

    internal static TurnBasedMatchConfigBuilder Create() {
        return new TurnBasedMatchConfigBuilder(C.TurnBasedMatchConfig_Builder_Construct());
    }
}
}

#endif
