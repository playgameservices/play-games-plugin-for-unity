// <copyright file="AndroidTokenClient.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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
//  limitations under the License.
// </copyright>

#if UNITY_ANDROID
namespace GooglePlayGames.Android
{
    using Com.Google.Android.Gms.Common.Api;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using OurUtils;
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    internal class AndroidJavaEnums
    {
        // Convert to LeaderboardVariant.java#TimeSpan
        internal static int ToLeaderboardVariantTimeSpan(LeaderboardTimeSpan span) 
        {
            switch(span)
            {
                case LeaderboardTimeSpan.Daily:
                return 0 /* TIME_SPAN_DAILY */;
                case LeaderboardTimeSpan.Weekly:
                return 1 /* TIME_SPAN_WEEKLY */;
                case LeaderboardTimeSpan.AllTime:
                default:
                return 2 /* TIME_SPAN_ALL_TIME */;
            }
        }

        // Convert to LeaderboardVariant.java#Collection
        internal static int ToLeaderboardVariantCollection(LeaderboardCollection collection) 
        {
            switch(collection)
            {
                case LeaderboardCollection.Social:
                return 1 /* COLLECTION_SOCIAL */;
                case LeaderboardCollection.Public:
                default:
                return 0 /* COLLECTION_PUBLIC */;
            }
        }

        // Convert to PageDirection.java#Direction
        internal static int ToPageDirection(ScorePageDirection direction) 
        {
            switch(direction)
            {
                case ScorePageDirection.Forward:
                return 0 /* NEXT */;
                case ScorePageDirection.Backward:
                return 1 /* PREV */;
                default:
                return -1 /* NONE */;
            }
        }
    }
}
#endif
