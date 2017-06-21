// <copyright file="NativeQuest.cs" company="Google Inc.">
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
    using C = GooglePlayGames.Native.Cwrapper.Quest;

    internal class NativeQuest : BaseReferenceHolder, IQuest
    {

        [Obsolete("Quests are being removed in 2018.")]
        private volatile NativeQuestMilestone mCachedMilestone;

        internal NativeQuest(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        public string Id
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Quest_Id(SelfPtr(), out_string, out_size));
            }
        }

        public string Name
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Quest_Name(SelfPtr(), out_string, out_size));
            }
        }

        public string Description
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Quest_Description(SelfPtr(), out_string, out_size));
            }
        }

        public string BannerUrl
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Quest_BannerUrl(SelfPtr(), out_string, out_size));
            }
        }

        public string IconUrl
        {
            get
            {
                return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Quest_IconUrl(SelfPtr(), out_string, out_size));
            }
        }

        public DateTime StartTime
        {
            get
            {
                return PInvokeUtilities.FromMillisSinceUnixEpoch(C.Quest_StartTime(SelfPtr()));
            }
        }

        public DateTime ExpirationTime
        {
            get
            {
                return PInvokeUtilities.FromMillisSinceUnixEpoch(C.Quest_ExpirationTime(SelfPtr()));
            }
        }

        public DateTime? AcceptedTime
        {
            get
            {
                long acceptedTime = C.Quest_AcceptedTime(SelfPtr());

                if (acceptedTime == 0)
                {
                    return null;
                }

                return PInvokeUtilities.FromMillisSinceUnixEpoch(acceptedTime);
            }
        }

        [Obsolete("Quests are being removed in 2018.")]
        public IQuestMilestone Milestone
        {
            get
            {
                if (mCachedMilestone == null)
                {
                    mCachedMilestone =
                    NativeQuestMilestone.FromPointer(C.Quest_CurrentMilestone(SelfPtr()));
                }

                return mCachedMilestone;
            }
        }

        public QuestState State
        {
            get
            {
                var state = C.Quest_State(SelfPtr());
                switch (state)
                {
                    case Types.QuestState.UPCOMING:
                        return QuestState.Upcoming;
                    case Types.QuestState.OPEN:
                        return QuestState.Open;
                    case Types.QuestState.ACCEPTED:
                        return QuestState.Accepted;
                    case Types.QuestState.COMPLETED:
                        return QuestState.Completed;
                    case Types.QuestState.EXPIRED:
                        return QuestState.Expired;
                    case Types.QuestState.FAILED:
                        return QuestState.Failed;
                    default:
                        throw new InvalidOperationException("Unknown state: " + state);
                }
            }
        }

        internal bool Valid()
        {
            return C.Quest_Valid(SelfPtr());
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.Quest_Dispose(selfPointer);
        }

        public override string ToString()
        {
            if (IsDisposed())
            {
                return "[NativeQuest: DELETED]";
            }

            return string.Format("[NativeQuest: Id={0}, Name={1}, Description={2}, " +
                "BannerUrl={3}, IconUrl={4}, State={5}, StartTime={6}, ExpirationTime={7}, " +
                "AcceptedTime={8}]",
                Id, Name, Description, BannerUrl, IconUrl, State, StartTime, ExpirationTime,
                AcceptedTime);
        }

        internal static NativeQuest FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }

            return new NativeQuest(pointer);
        }
    }
}
#endif
