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
    using S = GooglePlayGames.Native.Cwrapper.NearbyConnectionsStatus;

    public static class NearbyConnectionClientFactory
    {

        public static void Create(Action<INearbyConnectionClient> callback)
        {
            if (Application.isEditor)
            {
                GooglePlayGames.OurUtils.Logger.d("Creating INearbyConnection in editor, using DummyClient.");
                callback.Invoke(new GooglePlayGames.BasicApi.Nearby.DummyNearbyConnectionClient());
            }
#if (UNITY_ANDROID)
            GooglePlayGames.OurUtils.Logger.d("Creating real INearbyConnectionClient");
            GooglePlayGames.Native.NativeNearbyConnectionClientFactory.Create(callback);
#elif (UNITY_IPHONE && !NO_GPGS)
            GooglePlayGames.OurUtils.Logger.e("Nearby connections not supported in iOS... Using Dummy client");
            callback.Invoke(new GooglePlayGames.BasicApi.Nearby.DummyNearbyConnectionClient());
#else
            GooglePlayGames.OurUtils.Logger.e("Cannot create IPlayGamesClient for unknown platform, returning DummyClient");
            return new GooglePlayGames.BasicApi.DummyClient();
#endif
        }

        private static InitializationStatus ToStatus(S.InitializationStatus status)
        {
            switch (status)
            {
                case S.InitializationStatus.VALID:
                    return InitializationStatus.Success;
                case S.InitializationStatus.ERROR_INTERNAL:
                    return InitializationStatus.InternalError;
                case S.InitializationStatus.ERROR_VERSION_UPDATE_REQUIRED:
                    return InitializationStatus.VersionUpdateRequired;
                default:
                    GooglePlayGames.OurUtils.Logger.w("Unknown initialization status: " + status);
                    return InitializationStatus.InternalError;
            }
        }
    }
}
#endif
