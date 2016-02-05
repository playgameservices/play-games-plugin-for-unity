// <copyright file="NativeAchievement.cs" company="Google Inc.">
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
    using GooglePlayGames.BasicApi;
    using UnityEngine;
    using C = GooglePlayGames.Native.Cwrapper.Achievement;

    internal class NativeAchievement : BaseReferenceHolder
    {
        private const ulong MinusOne = 18446744073709551615L;

        internal NativeAchievement(IntPtr selfPointer)
            : base(selfPointer)
        {
        }

        internal uint CurrentSteps()
        {
            return C.Achievement_CurrentSteps(SelfPtr());
        }

        internal string Description()
        {
            return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Achievement_Description(SelfPtr(), out_string, out_size));
        }

        internal string Id()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_string, out_size) => C.Achievement_Id(SelfPtr(), out_string, out_size));
        }

        internal string Name()
        {
            return PInvokeUtilities.OutParamsToString(
                (out_string, out_size) => C.Achievement_Name(SelfPtr(), out_string, out_size));
        }

        internal Cwrapper.Types.AchievementState State()
        {
            return C.Achievement_State(SelfPtr());
        }

        internal uint TotalSteps()
        {
            return C.Achievement_TotalSteps(SelfPtr());
        }

        internal Cwrapper.Types.AchievementType Type()
        {
            return C.Achievement_Type(SelfPtr());
        }

        internal ulong LastModifiedTime()
        {
            if (C.Achievement_Valid(SelfPtr()))
            {
                return C.Achievement_LastModifiedTime(SelfPtr());
            }
            return 0;
        }

        internal ulong getXP()
        {
            return C.Achievement_XP (SelfPtr ());
        }

        internal string getRevealedImageUrl()
        {
            return PInvokeUtilities.OutParamsToString (
                (out_string, out_size) => C.Achievement_RevealedIconUrl (SelfPtr (), out_string, out_size));
        }

        internal string getUnlockedImageUrl()
        {
            return PInvokeUtilities.OutParamsToString (
                (out_string, out_size) => C.Achievement_UnlockedIconUrl (SelfPtr (), out_string, out_size));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.Achievement_Dispose(selfPointer);
        }

        internal Achievement AsAchievement()
        {
            Achievement achievement = new Achievement();

            achievement.Id = Id();
            achievement.Name = Name();
            achievement.Description = Description();
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            ulong val = LastModifiedTime();
            if (val == MinusOne)
            {
                val = 0;
            }
            achievement.LastModifiedTime  = UnixEpoch.AddMilliseconds(val);
            achievement.Points = getXP ();
            achievement.RevealedImageUrl = getRevealedImageUrl();
            achievement.UnlockedImageUrl = getUnlockedImageUrl();

            if (Type() == Cwrapper.Types.AchievementType.INCREMENTAL)
            {
                achievement.IsIncremental = true;
                achievement.CurrentSteps = (int)CurrentSteps();
                achievement.TotalSteps = (int)TotalSteps();
            }

            achievement.IsRevealed = State() == Cwrapper.Types.AchievementState.REVEALED ||
                State() == Cwrapper.Types.AchievementState.UNLOCKED;
            achievement.IsUnlocked = State() == Cwrapper.Types.AchievementState.UNLOCKED;

            return achievement;
        }

    }
}


#endif
