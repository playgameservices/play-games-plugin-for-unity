// <copyright file="PlayerSelectUIResponse.cs" company="Google Inc.">
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
    using System;
    using C = GooglePlayGames.Native.Cwrapper.TurnBasedMultiplayerManager;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using MultiplayerStatus = GooglePlayGames.Native.Cwrapper.CommonErrorStatus.MultiplayerStatus;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class PlayerSelectUIResponse : BaseReferenceHolder, IEnumerable<string>
    {
        internal PlayerSelectUIResponse(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        internal Status.UIStatus Status()
        {
            return C.TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetStatus(SelfPtr());
        }

        private string PlayerIdAtIndex(UIntPtr index)
        {
            return PInvokeUtilities.OutParamsToString(
                (out_string, size) => C.TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetPlayerIds_GetElement(
                    SelfPtr(), index, out_string, size));
        }

        public IEnumerator<string> GetEnumerator()
        {
            return PInvokeUtilities.ToEnumerator<string>(
                C.TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetPlayerIds_Length(SelfPtr()),
                PlayerIdAtIndex
            );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal uint MinimumAutomatchingPlayers()
        {
            return C.TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetMinimumAutomatchingPlayers(SelfPtr());
        }

        internal uint MaximumAutomatchingPlayers()
        {
            return C.TurnBasedMultiplayerManager_PlayerSelectUIResponse_GetMaximumAutomatchingPlayers(SelfPtr());
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.TurnBasedMultiplayerManager_PlayerSelectUIResponse_Dispose(selfPointer);
        }

        internal static PlayerSelectUIResponse FromPointer(IntPtr pointer)
        {
            if (PInvokeUtilities.IsNull(pointer))
            {
                return null;
            }

            return new PlayerSelectUIResponse(pointer);
        }
    }
}


#endif
