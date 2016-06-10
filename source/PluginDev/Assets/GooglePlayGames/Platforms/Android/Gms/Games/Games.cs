// <copyright file="Games.cs" company="Google Inc.">
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
using Com.Google.Android.Gms.Games.Stats;
using Google.Developers;
using System;
using Com.Google.Android.Gms.Common.Api;
using UnityEngine;
namespace Com.Google.Android.Gms.Games
{
    public class Games : JavaObjWrapper
    {
        public Games (IntPtr ptr) : base(ptr)
        {
        }
        const string CLASS_NAME = "com/google/android/gms/games/Games";

        public static string EXTRA_PLAYER_IDS
        {
            get
            {
                return JavaObjWrapper.GetStaticStringField(CLASS_NAME, "EXTRA_PLAYER_IDS");
            }
        }
        public static string EXTRA_STATUS
        {
            get
            {
                return JavaObjWrapper.GetStaticStringField(CLASS_NAME, "EXTRA_STATUS");
            }
        }
        public static object SCOPE_GAMES
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "SCOPE_GAMES", "Lcom/google/android/gms/common/api/Scope;");
            }
        }
        public static object API
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "API", "Lcom/google/android/gms/common/api/Api;");
            }
        }
        public static object GamesMetadata
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "GamesMetadata", "Lcom/google/android/gms/games/GamesMetadata;");
            }
        }
        public static object Achievements
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Achievements", "Lcom/google/android/gms/games/achievement/Achievements;");
            }
        }
        public static object Events
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Events", "Lcom/google/android/gms/games/event/Events;");
            }
        }
        public static object Leaderboards
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Leaderboards", "Lcom/google/android/gms/games/leaderboard/Leaderboards;");
            }
        }
        public static object Invitations
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Invitations", "Lcom/google/android/gms/games/multiplayer/Invitations;");
            }
        }
        public static object TurnBasedMultiplayer
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "TurnBasedMultiplayer", "Lcom/google/android/gms/games/multiplayer/turnbased/TurnBasedMultiplayer;");
            }
        }
        public static object RealTimeMultiplayer
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "RealTimeMultiplayer", "Lcom/google/android/gms/games/multiplayer/realtime/RealTimeMultiplayer;");
            }
        }
        public static object Players
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Players", "Lcom/google/android/gms/games/Players;");
            }
        }
        public static object Notifications
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Notifications", "Lcom/google/android/gms/games/Notifications;");
            }
        }
        public static object Quests
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Quests", "Lcom/google/android/gms/games/quest/Quests;");
            }
        }
        public static object Requests
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Requests", "Lcom/google/android/gms/games/request/Requests;");
            }
        }
        public static object Snapshots
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "Snapshots", "Lcom/google/android/gms/games/snapshot/Snapshots;");
            }
        }
        public static StatsObject Stats
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<StatsObject>(CLASS_NAME, "Stats", "Lcom/google/android/gms/games/stats/Stats;");
            }
        }
        public static string getAppId(GoogleApiClient arg_GoogleApiClient_1)
        {
            return JavaObjWrapper.StaticInvokeCall<string>(CLASS_NAME, "getAppId","(Lcom/google/android/gms/common/api/GoogleApiClient;)Ljava/lang/String;",  arg_GoogleApiClient_1);
        }
        public static string getCurrentAccountName(GoogleApiClient arg_GoogleApiClient_1)
        {
            return JavaObjWrapper.StaticInvokeCall<string>(CLASS_NAME, "getCurrentAccountName","(Lcom/google/android/gms/common/api/GoogleApiClient;)Ljava/lang/String;",  arg_GoogleApiClient_1);
        }
        public static int getSdkVariant(GoogleApiClient arg_GoogleApiClient_1)
        {
            return JavaObjWrapper.StaticInvokeCall<int>(CLASS_NAME, "getSdkVariant","(Lcom/google/android/gms/common/api/GoogleApiClient;)I",  arg_GoogleApiClient_1);
        }
        public static object getSettingsIntent(GoogleApiClient arg_GoogleApiClient_1)
        {
            return JavaObjWrapper.StaticInvokeCall<object>(CLASS_NAME, "getSettingsIntent","(Lcom/google/android/gms/common/api/GoogleApiClient;)Landroid/content/Intent;",  arg_GoogleApiClient_1);
        }
        public static void setGravityForPopups(GoogleApiClient arg_GoogleApiClient_1, int arg_int_2)
        {
            JavaObjWrapper.StaticInvokeCallVoid(CLASS_NAME, "setGravityForPopups","(Lcom/google/android/gms/common/api/GoogleApiClient;I)V",  arg_GoogleApiClient_1,  arg_int_2);
        }
        public static void setViewForPopups(GoogleApiClient arg_GoogleApiClient_1, object arg_object_2)
        {
            JavaObjWrapper.StaticInvokeCallVoid(CLASS_NAME, "setViewForPopups","(Lcom/google/android/gms/common/api/GoogleApiClient;Landroid/view/View;)V",  arg_GoogleApiClient_1,  arg_object_2);
        }
        public static PendingResult<Status> signOut(GoogleApiClient arg_GoogleApiClient_1)
        {
            return JavaObjWrapper.StaticInvokeCall<PendingResult<Status>>(CLASS_NAME, "signOut","(Lcom/google/android/gms/common/api/GoogleApiClient;)Lcom/google/android/gms/common/api/PendingResult;",  arg_GoogleApiClient_1);
        }
    }
    public class Games_BaseGamesApiMethodImpl<R> : JavaObjWrapper
        where R : Result
    {
        public Games_BaseGamesApiMethodImpl (IntPtr ptr) : base(ptr)
        {
        }
        const string CLASS_NAME = "com/google/android/gms/games/Games$BaseGamesApiMethodImpl";

        public Games_BaseGamesApiMethodImpl(GoogleApiClient arg_GoogleApiClient_1)
        {
            base.CreateInstance(CLASS_NAME,  arg_GoogleApiClient_1);
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
#endif
