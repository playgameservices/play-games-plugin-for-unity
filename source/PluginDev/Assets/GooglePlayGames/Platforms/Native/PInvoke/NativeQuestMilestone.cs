// <copyright file="NativeQuestMilestone.cs" company="Google Inc.">
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
    using GooglePlayGames.BasicApi.Quests;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using C = GooglePlayGames.Native.Cwrapper.QuestMilestone;

    [Obsolete("Quests are being removed in 2018.")]
    internal class NativeQuestMilestone : BaseReferenceHolder, IQuestMilestone
    {
        internal NativeQuestMilestone(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        public string Id
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.QuestMilestone_Id(SelfPtr(), out_string, out_size));
            }
        }

        public string EventId
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.QuestMilestone_EventId(SelfPtr(), out_string, out_size));
            }
        }

        public string QuestId
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.QuestMilestone_QuestId(SelfPtr(), out_string, out_size));
            }
        }

        public ulong CurrentCount
        {
            get
            {
                return C.QuestMilestone_CurrentCount(SelfPtr());
            }
        }

        public ulong TargetCount
        {
            get
            {
                return C.QuestMilestone_TargetCount(SelfPtr());
            }
        }

        public byte[] CompletionRewardData
        {
            get
            {
                return PInvokeUtilities.OutParamsToArray<byte>((out_bytes, out_size) =>
                C.QuestMilestone_CompletionRewardData(SelfPtr(), out_bytes, out_size));
            }
        }

        public MilestoneState State
        {
            get
            {
                var state = C.QuestMilestone_State(SelfPtr());
                switch (state)
                {
                    case Types.QuestMilestoneState.CLAIMED:
                        return MilestoneState.Claimed;
                    case Types.QuestMilestoneState.COMPLETED_NOT_CLAIMED:
                        return MilestoneState.CompletedNotClaimed;
                    case Types.QuestMilestoneState.NOT_COMPLETED:
                        return MilestoneState.NotCompleted;
                    case Types.QuestMilestoneState.NOT_STARTED:
                        return MilestoneState.NotStarted;
                    default:
                        throw new InvalidOperationException("Unknown state: " + state);
                }
            }
        }

        internal bool Valid()
        {
            return C.QuestMilestone_Valid(SelfPtr());
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.QuestMilestone_Dispose(selfPointer);
        }

        public override string ToString()
        {
            return string.Format("[NativeQuestMilestone: Id={0}, EventId={1}, QuestId={2}, " +
                "CurrentCount={3}, TargetCount={4}, State={5}]",
                Id, EventId, QuestId, CurrentCount, TargetCount, State);
        }

        internal static NativeQuestMilestone FromPointer(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return null;
            }

            return new NativeQuestMilestone(pointer);
        }

    }
}
#endif
