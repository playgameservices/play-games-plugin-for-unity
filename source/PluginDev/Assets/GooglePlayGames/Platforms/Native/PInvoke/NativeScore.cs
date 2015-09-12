// <copyright file="NativeScore.cs" company="Google Inc.">
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

    internal class NativeScore : BaseReferenceHolder
    {
        private const ulong MinusOne = 18446744073709551615L;

        internal NativeScore(IntPtr selfPtr) :base(selfPtr)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            Score.Score_Dispose(SelfPtr());
        }


        internal ulong GetDate()
        {
            // Not implemented in the C++ SDK....
            return MinusOne;
        }

        internal string GetMetadata()
        {
            return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                Score.Score_Metadata(SelfPtr(), out_string, out_size));
        }

        internal ulong GetRank()
        {
            return Score.Score_Rank(SelfPtr());
        }

        internal ulong GetValue()
        {
            return Score.Score_Value(SelfPtr());
        }

        internal PlayGamesScore AsScore(string leaderboardId, string selfPlayerId)
        {
            DateTime date;
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            ulong val = GetDate();
            if (val == MinusOne)
            {
                val = 0;
            }
            date = UnixEpoch.AddMilliseconds(val);

            PlayGamesScore retval = new PlayGamesScore(
                                        date,
                                        leaderboardId,
                                        GetRank(),
                                        selfPlayerId,
                                        GetValue(),
                                        GetMetadata());
            return retval;
        }

        internal static NativeScore FromPointer(IntPtr pointer)
        {
            if (pointer.Equals(IntPtr.Zero))
            {
                return null;
            }
            return new NativeScore(pointer);
        }
    }
}
#endif
