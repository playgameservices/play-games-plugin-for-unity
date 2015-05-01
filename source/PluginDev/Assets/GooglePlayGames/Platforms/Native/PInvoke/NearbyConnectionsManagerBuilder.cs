// <copyright file="NearbyConnectionsManagerBuilder.cs" company="Google Inc.">
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

namespace GooglePlayGames.Native.PInvoke
{
    using GooglePlayGames.OurUtils;
    using System;
    using System.Runtime.InteropServices;
    using C = GooglePlayGames.Native.Cwrapper.NearbyConnectionsBuilder;
    using N = GooglePlayGames.Native.Cwrapper.NearbyConnectionTypes;
    using S = GooglePlayGames.Native.Cwrapper.NearbyConnectionsStatus;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    internal class NearbyConnectionsManagerBuilder : BaseReferenceHolder
    {
        internal NearbyConnectionsManagerBuilder()
            : base(C.NearbyConnections_Builder_Construct())
        {
        }

        internal NearbyConnectionsManagerBuilder SetOnInitializationFinished(
            Action<S.InitializationStatus> callback)
        {
            C.NearbyConnections_Builder_SetOnInitializationFinished(SelfPtr(),
                InternalOnInitializationFinishedCallback,
                Callbacks.ToIntPtr(callback));
            return this;
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnInitializationFinishedCallback))]
        private static void InternalOnInitializationFinishedCallback(S.InitializationStatus status,
                                                                 IntPtr userData)
        {

            Action<S.InitializationStatus> callback =
                Callbacks.IntPtrToPermanentCallback<Action<S.InitializationStatus>>(userData);

            if (callback == null)
            {
                Logger.w("Callback for Initialization is null. Received status: " + status);
                return;
            }

            try
            {
                callback(status);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing " +
                    "NearbyConnectionsManagerBuilder#InternalOnInitializationFinishedCallback. " +
                    "Smothering exception: " + e);
            }
        }

        internal NearbyConnectionsManagerBuilder SetLocalClientId(long localClientId)
        {
            C.NearbyConnections_Builder_SetClientId(SelfPtr(), localClientId);
            return this;
        }

        internal NearbyConnectionsManagerBuilder SetDefaultLogLevel(Types.LogLevel minLevel)
        {
            C.NearbyConnections_Builder_SetDefaultOnLog(SelfPtr(), minLevel);
            return this;
        }

        internal NearbyConnectionsManager Build(PlatformConfiguration configuration)
        {
            return new NearbyConnectionsManager(
                C.NearbyConnections_Builder_Create(SelfPtr(), configuration.AsPointer()));
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.NearbyConnections_Builder_Dispose(selfPointer);
        }
    }
}

#endif // #if (UNITY_ANDROID )
