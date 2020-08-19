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
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.SavedGame;
    using OurUtils;
    using UnityEngine;
    using UnityEngine.SocialPlatforms;
    using System;
    using System.Collections.Generic;

    internal class AndroidJavaConverter
    {
        internal static System.DateTime ToDateTime(long milliseconds)
        {
            System.DateTime result = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            return result.AddMilliseconds(milliseconds);
        }

        // Convert to LeaderboardVariant.java#TimeSpan
        internal static int ToLeaderboardVariantTimeSpan(LeaderboardTimeSpan span)
        {
            switch (span)
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
            switch (collection)
            {
                case LeaderboardCollection.Social:
                  return 3 /* COLLECTION_FRIENDS */;
                case LeaderboardCollection.Public:
                default:
                    return 0 /* COLLECTION_PUBLIC */;
            }
        }

        // Convert to PageDirection.java#Direction
        internal static int ToPageDirection(ScorePageDirection direction)
        {
            switch (direction)
            {
                case ScorePageDirection.Forward:
                    return 0 /* NEXT */;
                case ScorePageDirection.Backward:
                    return 1 /* PREV */;
                default:
                    return -1 /* NONE */;
            }
        }

        internal static Player ToPlayer(AndroidJavaObject player)
        {
            if (player == null)
            {
                return null;
            }

            string displayName = player.Call<String>("getDisplayName");
            string playerId = player.Call<String>("getPlayerId");
            string avatarUrl = player.Call<String>("getIconImageUrl");
            return new Player(displayName, playerId, avatarUrl);
        }

        internal static PlayerProfile ToPlayerProfile(AndroidJavaObject player) {
          if (player == null) {
            return null;
          }

          string displayName = player.Call<String>("getDisplayName");
          string playerId = player.Call<String>("getPlayerId");
          string avatarUrl = player.Call<String>("getIconImageUrl");
          bool isFriend =
              player.Call<AndroidJavaObject>("getRelationshipInfo").Call<int>("getFriendStatus") ==
              4 /* PlayerFriendStatus.Friend*/;
          return new PlayerProfile(displayName, playerId, avatarUrl, isFriend);
        }

        internal static List<string> ToStringList(AndroidJavaObject stringList)
        {
            if (stringList == null)
            {
                return new List<string>();
            }

            int size = stringList.Call<int>("size");
            List<string> converted = new List<string>(size);

            for (int i = 0; i < size; i++)
            {
                converted.Add(stringList.Call<string>("get", i));
            }

            return converted;
        }

        // from C#: List<string> to Java: ArrayList<String>
        internal static AndroidJavaObject ToJavaStringList(List<string> list)
        {
            AndroidJavaObject converted = new AndroidJavaObject("java.util.ArrayList");
            for (int i = 0; i < list.Count; i++)
            {
                converted.Call<bool>("add", list[i]);
            }

            return converted;
        }

        internal static FriendsListVisibilityStatus ToFriendsListVisibilityStatus(int playerListVisibility) {
            switch (playerListVisibility)
            {
                case /* FriendsListVisibilityStatus.UNKNOWN */ 0:
                    return FriendsListVisibilityStatus.Unknown;
                case /* FriendsListVisibilityStatus.VISIBLE */ 1:
                    return FriendsListVisibilityStatus.Visible;
                case /* FriendsListVisibilityStatus.REQUEST_REQUIRED */ 2:
                    return FriendsListVisibilityStatus.ResolutionRequired;
                case /* FriendsListVisibilityStatus.FEATURE_UNAVAILABLE */ 3:
                    return FriendsListVisibilityStatus.Unavailable;
                default:
                    return FriendsListVisibilityStatus.Unknown;
            }
        }

        internal static IUserProfile[] playersBufferToArray(AndroidJavaObject playersBuffer) {
          int count = playersBuffer.Call<int>("getCount");
          IUserProfile[] users = new IUserProfile[count];
          for (int i = 0; i < count; ++i) {
            using (var player = playersBuffer.Call<AndroidJavaObject>("get", i)) {
              users[i] = AndroidJavaConverter.ToPlayerProfile(player);
            }
          }

          playersBuffer.Call("release");
          return users;
        }
    }
}
#endif
