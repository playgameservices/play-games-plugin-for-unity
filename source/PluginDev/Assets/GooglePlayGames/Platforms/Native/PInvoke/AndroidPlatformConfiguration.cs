/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#if (UNITY_ANDROID || UNITY_IPHONE)
using System;
using System.Runtime.InteropServices;
using GooglePlayGames.OurUtils;
using GooglePlayGames.Native.Cwrapper;

#if UNITY_ANDROID
using C = GooglePlayGames.Native.Cwrapper.AndroidPlatformConfiguration;

namespace GooglePlayGames.Native.PInvoke {

sealed class AndroidPlatformConfiguration : PlatformConfiguration {

    private delegate void IntentHandlerInternal(IntPtr intent, IntPtr userData);

    private AndroidPlatformConfiguration (IntPtr selfPointer) : base(selfPointer) {
    }

    internal void SetActivity(System.IntPtr activity) {
        C.AndroidPlatformConfiguration_SetActivity(SelfPtr(), activity);
    }

    internal void SetOptionalIntentHandlerForUI(Action<IntPtr> intentHandler) {
        Misc.CheckNotNull(intentHandler);
        C.AndroidPlatformConfiguration_SetOptionalIntentHandlerForUI(SelfPtr(),
            InternalIntentHandler, Callbacks.ToIntPtr(intentHandler));
    }

    internal void EnableAppState() {
        InternalHooks.InternalHooks_EnableAppState(SelfPtr());
    }

    protected override void CallDispose(HandleRef selfPointer) {
        C.AndroidPlatformConfiguration_Dispose(selfPointer);
    }

    [AOT.MonoPInvokeCallback(typeof(IntentHandlerInternal))]
    private static void InternalIntentHandler(IntPtr intent, IntPtr userData) {
        Callbacks.PerformInternalCallback("AndroidPlatformConfiguration#InternalIntentHandler",
            Callbacks.Type.Permanent, intent, userData);
    }

    internal static AndroidPlatformConfiguration Create() {
        return new AndroidPlatformConfiguration(C.AndroidPlatformConfiguration_Construct());
    }
}
}
#endif

#endif
