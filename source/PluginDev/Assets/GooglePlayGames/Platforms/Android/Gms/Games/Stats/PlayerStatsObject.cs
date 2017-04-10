// <copyright file="PlayerStatsObject.cs" company="Google Inc.">
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
using UnityEngine;
namespace Com.Google.Android.Gms.Games.Stats
{
    public class PlayerStatsObject : JavaObjWrapper , PlayerStats
    {
        const string CLASS_NAME = "com/google/android/gms/games/stats/PlayerStats";

        public PlayerStatsObject (IntPtr ptr) : base(ptr)
        {
        }
        public static float UNSET_VALUE
        {
            get
            {
                return JavaObjWrapper.GetStaticFloatField(CLASS_NAME, "UNSET_VALUE");
            }
        }
        public static int CONTENTS_FILE_DESCRIPTOR
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "CONTENTS_FILE_DESCRIPTOR");
            }
        }
        public static int PARCELABLE_WRITE_RETURN_VALUE
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "PARCELABLE_WRITE_RETURN_VALUE");
            }
        }
        public float getAverageSessionLength()
        {
            return InvokeCall<float>("getAverageSessionLength", "()F");
        }
        public float getChurnProbability()
        {
            return InvokeCall<float>("getChurnProbability", "()F");
        }
        public int getDaysSinceLastPlayed()
        {
            return InvokeCall<int>("getDaysSinceLastPlayed", "()I");
        }
        public int getNumberOfPurchases()
        {
            return InvokeCall<int>("getNumberOfPurchases", "()I");
        }
        public int getNumberOfSessions()
        {
            return InvokeCall<int>("getNumberOfSessions", "()I");
        }
        public float getSessionPercentile()
        {
            return InvokeCall<float>("getSessionPercentile", "()F");
        }
        public float getSpendPercentile()
        {
            return InvokeCall<float>("getSpendPercentile", "()F");
        }
        public float getSpendProbability()
        {
            return InvokeCall<float>("getSpendProbability", "()F");
        }
        public float getHighSpenderProbability()
        {
            return InvokeCall<float>("getHighSpenderProbability", "()F");
        }
        public float getTotalSpendNext28Days()
        {
            return InvokeCall<float>("getTotalSpendNext28Days", "()F");
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
#endif
