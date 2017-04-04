// <copyright file="GameServicesBuilder.cs" company="Google Inc.">
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using GooglePlayGames.OurUtils;
    using GooglePlayGames.Native.Cwrapper;
    using C = GooglePlayGames.Native.Cwrapper.Builder;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    internal class GameServicesBuilder : BaseReferenceHolder
    {

        internal delegate void AuthFinishedCallback(Types.AuthOperation operation,
        CommonErrorStatus.AuthStatus status);

        internal delegate void AuthStartedCallback(Types.AuthOperation operation);

        private GameServicesBuilder(IntPtr selfPointer)
            : base(selfPointer)
        {
            InternalHooks.InternalHooks_ConfigureForUnityPlugin(SelfPtr(),
                    PluginVersion.VersionString);
        }

        internal void SetOnAuthFinishedCallback(AuthFinishedCallback callback)
        {
            C.GameServices_Builder_SetOnAuthActionFinished(SelfPtr(), InternalAuthFinishedCallback,
                Callbacks.ToIntPtr(callback));
        }

        internal void EnableSnapshots()
        {
            C.GameServices_Builder_EnableSnapshots(SelfPtr());
        }

        internal void AddOauthScope(string scope)
        {
            C.GameServices_Builder_AddOauthScope(SelfPtr(), scope);
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnAuthActionFinishedCallback))]
        private static void InternalAuthFinishedCallback(Types.AuthOperation op,
                                                     CommonErrorStatus.AuthStatus status, IntPtr data)
        {

            AuthFinishedCallback callback =
                Callbacks.IntPtrToPermanentCallback<AuthFinishedCallback>(data);

            if (callback == null)
            {
                return;
            }

            try
            {
                callback(op, status);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing InternalAuthFinishedCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal void SetOnAuthStartedCallback(AuthStartedCallback callback)
        {
            C.GameServices_Builder_SetOnAuthActionStarted(SelfPtr(), InternalAuthStartedCallback,
                Callbacks.ToIntPtr(callback));
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnAuthActionStartedCallback))]
        private static void InternalAuthStartedCallback(Types.AuthOperation op, IntPtr data)
        {
            AuthStartedCallback callback =
                Callbacks.IntPtrToPermanentCallback<AuthStartedCallback>(data);

            try
            {
                if (callback != null)
                {
                    callback(op);
                }
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing InternalAuthStartedCallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal void SetShowConnectingPopup(bool flag)
        {
            C.GameServices_Builder_SetShowConnectingPopup(SelfPtr(), flag);
        }

        protected override void CallDispose(HandleRef selfPointer)
        {
            C.GameServices_Builder_Dispose(selfPointer);
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnTurnBasedMatchEventCallback))]
        private static void InternalOnTurnBasedMatchEventCallback(Types.MultiplayerEvent eventType,
                                                              string matchId, IntPtr match, IntPtr userData)
        {
            var callback = Callbacks.IntPtrToPermanentCallback
            <Action<Types.MultiplayerEvent, string, NativeTurnBasedMatch>>(userData);
            using (var nativeMatch = NativeTurnBasedMatch.FromPointer(match))
            {
                try
                {
                    if (callback != null)
                    {
                        callback(eventType, matchId, nativeMatch);
                    }
                }
                catch (Exception e)
                {
                    Logger.e("Error encountered executing InternalOnTurnBasedMatchEventCallback. " +
                        "Smothering to avoid passing exception into Native: " + e);
                }
            }
        }

        internal void SetOnTurnBasedMatchEventCallback(
            Action<Types.MultiplayerEvent, string, NativeTurnBasedMatch> callback)
        {
            IntPtr callbackPointer = Callbacks.ToIntPtr(callback);
            C.GameServices_Builder_SetOnTurnBasedMatchEvent(SelfPtr(),
                InternalOnTurnBasedMatchEventCallback, callbackPointer);
        }

        [AOT.MonoPInvokeCallback(typeof(C.OnMultiplayerInvitationEventCallback))]
        private static void InternalOnMultiplayerInvitationEventCallback(
            Types.MultiplayerEvent eventType, string matchId, IntPtr match, IntPtr userData)
        {
            var callback = Callbacks.IntPtrToPermanentCallback
            <Action<Types.MultiplayerEvent, string, MultiplayerInvitation>>(userData);

            using (var nativeInvitation = MultiplayerInvitation.FromPointer(match))
            {
                try
                {
                    if (callback != null)
                    {
                        callback(eventType, matchId, nativeInvitation);
                    }
                }
                catch (Exception e)
                {
                    Logger.e("Error encountered executing " +
                        "InternalOnMultiplayerInvitationEventCallback. " +
                        "Smothering to avoid passing exception into Native: " + e);
                }
            }
        }

        internal void SetOnMultiplayerInvitationEventCallback(
            Action<Types.MultiplayerEvent, string, MultiplayerInvitation> callback)
        {
            IntPtr callbackPointer = Callbacks.ToIntPtr(callback);
            C.GameServices_Builder_SetOnMultiplayerInvitationEvent(SelfPtr(),
                InternalOnMultiplayerInvitationEventCallback, callbackPointer);
        }

        internal GameServices Build(PlatformConfiguration configRef)
        {
            IntPtr pointer = C.GameServices_Builder_Create(SelfPtr(),
                             HandleRef.ToIntPtr(configRef.AsHandle()));

            if (pointer.Equals(IntPtr.Zero))
            {
                // TODO(hsakai): For now, explode noisily. In the actual plugin, this probably
                // should result in something besides an exception.
                throw new System.InvalidOperationException("There was an error creating a " +
                    "GameServices object. Check for log errors from GamesNativeSDK");
            }

            return new GameServices(pointer);
        }

        internal static GameServicesBuilder Create()
        {
            IntPtr b = C.GameServices_Builder_Construct();
            return new GameServicesBuilder(b);
        }
    }
}


#endif
