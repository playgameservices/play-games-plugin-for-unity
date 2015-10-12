// <copyright file="StatsObject.cs" company="Google Inc.">
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

#if UNITY_ANDROID
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
using Google.Developers;
using System;
using System.Reflection;
using Com.Google.Android.Gms.Common.Api;
using UnityEngine;
namespace Com.Google.Android.Gms.Games.Stats
{
    
    public class StatsObject : JavaObjWrapper , Stats
    {
        const string CLASS_NAME = "com/google/android/gms/games/stats/Stats";

        public StatsObject (IntPtr ptr) : base(ptr)
        {
        }
        public PendingResult<Stats_LoadPlayerStatsResultObject> loadPlayerStats(GoogleApiClient arg_GoogleApiClient_1, bool arg_bool_2)
        {
            IntPtr obj = InvokeCall<IntPtr>("loadPlayerStats", "(Lcom/google/android/gms/common/api/GoogleApiClient;Z)Lcom/google/android/gms/common/api/PendingResult;",  arg_GoogleApiClient_1,  arg_bool_2);
            return new PendingResult<Stats_LoadPlayerStatsResultObject>(obj);
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
#endif