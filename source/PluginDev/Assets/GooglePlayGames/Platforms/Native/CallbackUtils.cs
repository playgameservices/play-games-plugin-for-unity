// <copyright file="CallbackUtils.cs" company="Google Inc.">
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
namespace GooglePlayGames.Native
{
    using System;
    using GooglePlayGames.OurUtils;
    using Types = GooglePlayGames.Native.Cwrapper.Types;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;


    internal static class CallbackUtils
    {
        internal static Action<T> ToOnGameThread<T>(Action<T> toConvert)
        {
            if (toConvert == null)
            {
                return delegate
                {
                };
            }

            return (val) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val));
        }

        internal static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert)
        {
            if (toConvert == null)
            {
                return delegate
                {
                };
            }

            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }

        internal static Action<T1, T2, T3> ToOnGameThread<T1, T2, T3>(Action<T1, T2, T3> toConvert)
        {
            if (toConvert == null)
            {
                return delegate
                {
                };
            }

            return (val1, val2, val3) =>
            PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2, val3));
        }
    }
}
#endif // #if (UNITY_ANDROID || UNITY_IPHONE)
