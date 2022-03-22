// <copyright file="NearbyConnectionClientFactory.cs" company="Google Inc.">
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

// Android only feature

#if (UNITY_ANDROID)
namespace GooglePlayGames
{
    using UnityEngine;
    using System;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.BasicApi.Nearby;

    public static class NearbyConnectionClientFactory
    {
        public static void Create(Action<INearbyConnectionClient> callback)
        {
            if (Application.isEditor)
            {
                GooglePlayGames.OurUtils.Logger.d("Creating INearbyConnection in editor, using DummyClient.");
                callback.Invoke(new GooglePlayGames.BasicApi.Nearby.DummyNearbyConnectionClient());
            }

            callback.Invoke(new GooglePlayGames.Android.AndroidNearbyConnectionClient());
        }
    }
}
#endif //UNITY_ANDROID