// <copyright file="NativeScoreSummary.cs" company="Google Inc.">
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
    using System.Runtime.InteropServices;
    using GooglePlayGames.Native.Cwrapper;


    internal class NativeScoreSummary : BaseReferenceHolder {

        internal NativeScoreSummary(IntPtr selfPtr) :base(selfPtr)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            ScoreSummary.ScoreSummary_Dispose(selfPointer);
        }

        internal ulong ApproximateResults()
        {
            return ScoreSummary.ScoreSummary_ApproximateNumberOfScores(SelfPtr());
        }

        internal NativeScore LocalUserScore()
        {
            return NativeScore.FromPointer(
                ScoreSummary.ScoreSummary_CurrentPlayerScore(SelfPtr()));
        }

        internal static NativeScoreSummary FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new NativeScoreSummary(pointer);
        }
    }
}
#endif
