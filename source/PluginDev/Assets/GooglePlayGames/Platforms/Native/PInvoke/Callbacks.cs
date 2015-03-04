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
using System.Text;
using GooglePlayGames.OurUtils;
using GooglePlayGames.Native.Cwrapper;

namespace GooglePlayGames.Native.PInvoke {
static class Callbacks {

    internal static readonly Action<CommonErrorStatus.UIStatus> NoopUICallback = status => {
        Logger.d("Received UI callback: " + status);
    };

    internal delegate void ShowUICallbackInternal(CommonErrorStatus.UIStatus status, IntPtr data);

    internal static IntPtr ToIntPtr<T>(Action<T> callback, Func<IntPtr, T> conversionFunction)
        where T : BaseReferenceHolder {
        Action<IntPtr> pointerReceiver = result => {
            using (T converted = conversionFunction(result)) {
                if (callback != null) {
                    callback(converted);
                }
            }
        };

        return ToIntPtr(pointerReceiver);
    }

    internal static IntPtr ToIntPtr(Delegate callback) {
        if (callback == null) {
            return IntPtr.Zero;
        }

        // Once the callback is passed off to native, we don't retain a reference to it - which
        // means it's eligible for garbage collecting or being moved around by the runtime. If
        // the garbage collector runs before the native code invokes the callback, chaos will
        // ensue.
        //
        // To handle this, we manually pin the callback in memory (this means it cannot be
        // collected or moved) - the GCHandle will be freed when the callback returns the and
        // handle is converted back to callback via IntPtrToCallback.
        var handle = GCHandle.Alloc(callback, GCHandleType.Pinned);

        return GCHandle.ToIntPtr(handle);
    }


    internal static T IntPtrToTempCallback<T>(IntPtr handle) where T : class {
        return IntPtrToCallback<T>(handle, true);
    }

    private static T IntPtrToCallback<T>(IntPtr handle, bool unpinHandle) where T : class {
        if (PInvokeUtilities.IsNull(handle)) {
            return null;
        }

        var gcHandle = GCHandle.FromIntPtr(handle);
        try {
            return (T)gcHandle.Target;
        } catch(System.InvalidCastException e) {
            Logger.e("GC Handle pointed to unexpected type: " + gcHandle.Target.ToString() +
                ". Expected " +  typeof(T));
            throw e;
        } finally {
            if (unpinHandle) {
                gcHandle.Free();
            }
        }
    }

    // TODO(hsakai): Better way of handling this.
    internal static T IntPtrToPermanentCallback<T>(IntPtr handle) where T : class {
        return IntPtrToCallback<T>(handle, false);
    }

    [AOT.MonoPInvokeCallback(typeof(ShowUICallbackInternal))]
    internal static void InternalShowUICallback(CommonErrorStatus.UIStatus status, IntPtr data) {
        Logger.d("Showing UI Internal callback: " + status);
        var callback = IntPtrToTempCallback<Action<CommonErrorStatus.UIStatus>>(data);

        try {
            callback(status);
        } catch (Exception e) {
            Logger.e("Error encountered executing InternalShowAllUICallback. " +
                "Smothering to avoid passing exception into Native: " + e);
        }
    }

    internal enum Type { Permanent, Temporary };

    internal static void PerformInternalCallback(string callbackName, Type callbackType,
        IntPtr response, IntPtr userData) {
        Logger.d("Entering internal callback for " + callbackName);

        Action<IntPtr> callback = callbackType == Type.Permanent
            ? IntPtrToPermanentCallback<Action<IntPtr>>(userData)
            : IntPtrToTempCallback<Action<IntPtr>>(userData);

        if (callback == null) {
            return;
        }

        try {
            callback(response);
        } catch (Exception e) {
            Logger.e("Error encountered executing " + callbackName + ". " +
            "Smothering to avoid passing exception into Native: " + e);
        }
    }

    internal static Action<T> AsOnGameThreadCallback<T>(Action<T> toInvokeOnGameThread) {
        return result => {
            if (toInvokeOnGameThread == null) {
                return;
            }

            PlayGamesHelperObject.RunOnGameThread(() => toInvokeOnGameThread(result));
        };
    }

    internal static Action<T1, T2> AsOnGameThreadCallback<T1, T2>(
        Action<T1, T2> toInvokeOnGameThread) {
        return (result1, result2) => {
            if (toInvokeOnGameThread == null) {
                return;
            }

            PlayGamesHelperObject.RunOnGameThread(() => toInvokeOnGameThread(result1, result2));
        };
    }

}
}


#endif
