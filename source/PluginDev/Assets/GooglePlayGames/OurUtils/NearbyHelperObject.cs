// <copyright file="PlayGamesHelperObject.cs" company="Google Inc.">
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

namespace GooglePlayGames.OurUtils
{
    using GooglePlayGames.BasicApi.Nearby;
    using System;
    using System.Collections;
    using UnityEngine;
    using System.Collections.Generic;

    public class NearbyHelperObject : MonoBehaviour
    {
        // our (singleton) instance
        private static NearbyHelperObject instance = null;

        // timers to keep track of discovery and advertising
        private static double AdvertisingRemaining = 0;
        private static double DiscoveryRemaining = 0;

        // nearby client to stop discovery and to stop advertising
        private static INearbyConnectionClient mClient = null;

        public static void CreateObject(INearbyConnectionClient client)
        {
            if (instance != null)
            {
                return;
            }

            mClient = client;
            if (Application.isPlaying)
            {
                // add an invisible game object to the scene
                GameObject obj = new GameObject("PlayGames_QueueRunner");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<NearbyHelperObject>();
            }
            else
            {
                instance = new NearbyHelperObject();
            }
        }

        private static double ToSeconds(TimeSpan? span)
        {
            if (!span.HasValue)
            {
                return 0;
            }

            if (span.Value.TotalSeconds < 0)
            {
                return 0;
            }

            return span.Value.TotalSeconds;
        }
        public static void StartAdvertisingTimer(TimeSpan? span)
        {
            AdvertisingRemaining = ToSeconds(span);
        }

        public static void StartDiscoveryTimer(TimeSpan? span)
        {
            DiscoveryRemaining = ToSeconds(span);
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void Update()
        {
            // check if currently advertising
            if (AdvertisingRemaining > 0)
            {
                AdvertisingRemaining -= Time.deltaTime;
                if (AdvertisingRemaining < 0)
                {
                    mClient.StopAdvertising();
                }
            }

            // check if currently discovering
            if (DiscoveryRemaining > 0)
            {
                DiscoveryRemaining -= Time.deltaTime;
                if (DiscoveryRemaining < 0)
                {
                    mClient.StopDiscovery(mClient.GetServiceId());
                }
            }
        }
    }
}
