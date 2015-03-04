/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#if (UNITY_ANDROID || UNITY_IPHONE)
using System;
using System.Runtime.InteropServices;
using System.Text;
using GooglePlayGames.Native.PInvoke;
using GooglePlayGames.BasicApi;

using C = GooglePlayGames.Native.Cwrapper.Achievement;
using Types = GooglePlayGames.Native.Cwrapper.Types;

namespace GooglePlayGames.Native {
internal class NativeAchievement : BaseReferenceHolder {

    internal NativeAchievement (IntPtr selfPointer) : base(selfPointer) {
    }

    internal uint CurrentSteps() {
        return C.Achievement_CurrentSteps(SelfPtr());
    }

    internal string Description() {
        return PInvokeUtilities.OutParamsToString((out_string, out_size) =>
                C.Achievement_Description(SelfPtr(), out_string, out_size));
    }

    internal string Id() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, out_size) => C.Achievement_Id(SelfPtr(), out_string, out_size));
    }

    internal string Name() {
        return PInvokeUtilities.OutParamsToString(
            (out_string, out_size) => C.Achievement_Name(SelfPtr(), out_string, out_size));
    }

    internal Types.AchievementState State() {
        return C.Achievement_State(SelfPtr());
    }

    internal uint TotalSteps() {
        return C.Achievement_TotalSteps(SelfPtr());
    }

    internal Types.AchievementType Type() {
        return C.Achievement_Type(SelfPtr());
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.Achievement_Dispose(selfPointer);
    }

    internal Achievement AsAchievement() {
        Achievement achievement = new Achievement();

        achievement.Id = Id();
        achievement.Name = Name();
        achievement.Description = Description();

        if (Type() == Types.AchievementType.INCREMENTAL) {
            achievement.IsIncremental = true;
            achievement.CurrentSteps = (int)CurrentSteps();
            achievement.TotalSteps = (int)TotalSteps();
        }

        achievement.IsRevealed = State() == Types.AchievementState.REVEALED;
        achievement.IsUnlocked = State() == Types.AchievementState.UNLOCKED;

        return achievement;
    }

}
}


#endif
