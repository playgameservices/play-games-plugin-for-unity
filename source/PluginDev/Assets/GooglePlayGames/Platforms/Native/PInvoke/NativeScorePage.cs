// <copyright file="NativeScorePage.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GooglePlayGames.Native.Cwrapper;


    internal class NativeScorePage : BaseReferenceHolder
    {
        internal NativeScorePage(IntPtr selfPtr)
            : base(selfPtr)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            ScorePage.ScorePage_Dispose(selfPointer);
        }

        internal Types.LeaderboardCollection GetCollection()
        {
            return ScorePage.ScorePage_Collection(SelfPtr());
        }

        private UIntPtr Length()
        {
            return ScorePage.ScorePage_Entries_Length(SelfPtr());
        }

        private NativeScoreEntry GetElement(UIntPtr index)
        {
            if (index.ToUInt64() >= Length().ToUInt64())
            {
                throw new ArgumentOutOfRangeException();
            }

            return new NativeScoreEntry(
                ScorePage.ScorePage_Entries_GetElement(SelfPtr(), index));
        }

        public IEnumerator<NativeScoreEntry> GetEnumerator()
        {
            return PInvokeUtilities.ToEnumerator(
                ScorePage.ScorePage_Entries_Length(SelfPtr()),
                (index) => GetElement(index));
        }

        internal bool HasNextScorePage()
        {
            return ScorePage.ScorePage_HasNextScorePage(SelfPtr());
        }

        internal bool HasPrevScorePage()
        {
            return ScorePage.ScorePage_HasPreviousScorePage(SelfPtr());
        }

        internal NativeScorePageToken GetNextScorePageToken()
        {
            return new NativeScorePageToken(
                ScorePage.ScorePage_NextScorePageToken(SelfPtr()));
        }

        internal NativeScorePageToken GetPreviousScorePageToken()
        {
            return new NativeScorePageToken(
                ScorePage.ScorePage_PreviousScorePageToken(SelfPtr()));
        }

        internal bool Valid()
        {
            return ScorePage.ScorePage_Valid(SelfPtr());
        }

        internal Types.LeaderboardTimeSpan GetTimeSpan()
        {
            return ScorePage.ScorePage_TimeSpan(SelfPtr());
        }

        internal Types.LeaderboardStart GetStart()
        {
            return ScorePage.ScorePage_Start(SelfPtr());
        }

        internal string GetLeaderboardId()
        {
            return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                ScorePage.ScorePage_LeaderboardId(SelfPtr(), out_string, out_size));
        }

        internal static NativeScorePage FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new NativeScorePage(pointer);
        }
    }
}
#endif
