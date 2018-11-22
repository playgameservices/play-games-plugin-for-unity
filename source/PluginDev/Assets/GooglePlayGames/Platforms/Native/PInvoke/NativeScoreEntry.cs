// <copyright file="NativeScoreEntry.cs" company="Google Inc.">
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



#if UNITY_ANDROID

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using GooglePlayGames.Native.Cwrapper;


    internal class NativeScoreEntry : BaseReferenceHolder
    {
        private const ulong MinusOne = 18446744073709551615L;

        internal NativeScoreEntry(IntPtr selfPtr)
            : base(selfPtr)
        {
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            ScorePage.ScorePage_Entry_Dispose(selfPointer);
        }

        internal ulong GetLastModifiedTime()
        {
            return ScorePage.ScorePage_Entry_LastModifiedTime(SelfPtr());
        }

        internal string GetPlayerId()
        {
            return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                ScorePage.ScorePage_Entry_PlayerId(SelfPtr(), out_string, out_size));
        }

        internal NativeScore GetScore()
        {
            return new NativeScore(ScorePage.ScorePage_Entry_Score(SelfPtr()));
        }

        internal PlayGamesScore AsScore(string leaderboardId)
        {
            DateTime date;
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            ulong val = GetLastModifiedTime();
            if (val == MinusOne)
            {
                val = 0;
            }
            date = UnixEpoch.AddMilliseconds(val);

            PlayGamesScore score = new PlayGamesScore(
                                       date,
                leaderboardId,
                            GetScore().GetRank(),
                                       GetPlayerId(),
                        GetScore().GetValue(),
                        GetScore().GetMetadata());
            return score;
        }
    }
}
#endif
