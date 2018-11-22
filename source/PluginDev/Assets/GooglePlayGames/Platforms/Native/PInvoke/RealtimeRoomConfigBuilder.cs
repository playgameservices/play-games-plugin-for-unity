// <copyright file="RealtimeRoomConfigBuilder.cs" company="Google Inc.">
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
    using System.Runtime.InteropServices;
    using C = GooglePlayGames.Native.Cwrapper.RealTimeRoomConfigBuilder;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal class RealtimeRoomConfigBuilder : BaseReferenceHolder
    {

        internal RealtimeRoomConfigBuilder(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        internal RealtimeRoomConfigBuilder PopulateFromUIResponse(PlayerSelectUIResponse response)
        {
            C.RealTimeRoomConfig_Builder_PopulateFromPlayerSelectUIResponse(SelfPtr(),
                response.AsPointer());

            return this;
        }

        internal RealtimeRoomConfigBuilder SetVariant(uint variantValue)
        {
            uint variant;
            unchecked {
                variant = variantValue == 0 ? (uint)-1 : variantValue;
            }
            C.RealTimeRoomConfig_Builder_SetVariant(SelfPtr(), variant);
            return this;
        }

        internal RealtimeRoomConfigBuilder AddInvitedPlayer(string playerId)
        {
            C.RealTimeRoomConfig_Builder_AddPlayerToInvite(SelfPtr(), playerId);
            return this;
        }

        internal RealtimeRoomConfigBuilder SetExclusiveBitMask(ulong bitmask)
        {
            C.RealTimeRoomConfig_Builder_SetExclusiveBitMask(SelfPtr(), bitmask);
            return this;
        }

        internal RealtimeRoomConfigBuilder SetMinimumAutomatchingPlayers(uint minimum)
        {
            C.RealTimeRoomConfig_Builder_SetMinimumAutomatchingPlayers(SelfPtr(), minimum);
            return this;
        }

        internal RealtimeRoomConfigBuilder SetMaximumAutomatchingPlayers(uint maximum)
        {
            C.RealTimeRoomConfig_Builder_SetMaximumAutomatchingPlayers(SelfPtr(), maximum);
            return this;
        }

        internal RealtimeRoomConfig Build()
        {
            return new RealtimeRoomConfig(C.RealTimeRoomConfig_Builder_Create(SelfPtr()));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.RealTimeRoomConfig_Builder_Dispose(selfPointer);
        }

        internal static RealtimeRoomConfigBuilder Create()
        {
            return new RealtimeRoomConfigBuilder(C.RealTimeRoomConfig_Builder_Construct());
        }
    }
}

#endif
