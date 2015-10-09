// <copyright file="LoadPlayerStatsResultObject.cs" company="Google Inc.">
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
    
    public class Stats_LoadPlayerStatsResultObject : JavaObjWrapper , Stats_LoadPlayerStatsResult, Result
    {
        const string CLASS_NAME = "com/google/android/gms/games/stats/Stats$LoadPlayerStatsResult";

        public Stats_LoadPlayerStatsResultObject (IntPtr ptr) : base(ptr)
        {
        }
        public PlayerStats getPlayerStats()
        {
            IntPtr obj = InvokeCall<IntPtr>("getPlayerStats", "()Lcom/google/android/gms/games/stats/PlayerStats;");
            return new PlayerStatsObject(obj);
        }
        public Status getStatus()
        {
            IntPtr obj = InvokeCall<IntPtr>("getStatus", "()Lcom/google/android/gms/common/api/Status;");
            return new Status(obj);
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
