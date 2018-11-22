// <copyright file="ParticipantResults.cs" company="Google Inc.">
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
    using C = GooglePlayGames.Native.Cwrapper.ParticipantResults;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

    internal class ParticipantResults : BaseReferenceHolder
    {

        internal ParticipantResults(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        internal bool HasResultsForParticipant(string participantId)
        {
            return C.ParticipantResults_HasResultsForParticipant(SelfPtr(), participantId);
        }

        internal uint PlacingForParticipant(string participantId)
        {
            return C.ParticipantResults_PlaceForParticipant(SelfPtr(), participantId);
        }

        internal Types.MatchResult ResultsForParticipant(string participantId)
        {
            return C.ParticipantResults_MatchResultForParticipant(SelfPtr(), participantId);
        }

        internal ParticipantResults WithResult(string participantId, uint placing,
                                           Types.MatchResult result)
        {
            return new ParticipantResults(C.ParticipantResults_WithResult(
                    SelfPtr(), participantId, placing, result));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.ParticipantResults_Dispose(selfPointer);
        }
    }
}

#endif
