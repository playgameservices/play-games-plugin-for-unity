// <copyright file="Callbacks.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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

namespace GooglePlayGames.Native.PInvoke
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using GooglePlayGames.Native.Cwrapper;
    using GooglePlayGames.OurUtils;

    static class Callbacks
    {
        internal static readonly Action<CommonErrorStatus.UIStatus> NoopUICallback = status =>
        {
            Logger.d("Received UI callback: " + status);
        };

        internal delegate void ShowUICallbackInternal(CommonErrorStatus.UIStatus status,IntPtr data);

        internal static IntPtr ToIntPtr<T>(Action<T> callback, Func<IntPtr, T> conversionFunction)
        where T : BaseReferenceHolder
        {
            Action<IntPtr> pointerReceiver = result =>
            {
                using (T converted = conversionFunction(result))
                {
                    if (callback != null)
                    {
                        callback(converted);
                    }
                }
            };

            return ToIntPtr(pointerReceiver);
        }

        internal static IntPtr ToIntPtr<T, P>(Action<T, P> callback, Func<IntPtr, P> conversionFunction)
        where P : BaseReferenceHolder
        {
            Action<T, IntPtr> pointerReceiver = (param1, param2) =>
            {
                using (P converted = conversionFunction(param2))
                {
                    if (callback != null)
                    {
                        callback(param1, converted);
                    }
                }
            };

            return ToIntPtr(pointerReceiver);
        }

        internal static IntPtr ToIntPtr(Delegate callback)
        {
            if (callback == null)
            {
                return IntPtr.Zero;
            }

            // Once the callback is passed off to native, we don't retain a reference to it - which
            // means it's eligible for garbage collecting or being moved around by the runtime. If
            // the garbage collector runs before the native code invokes the callback, chaos will
            // ensue.
            //
            // To handle this, we create a normal GCHandle. The GCHandle will be freed when the callback returns the and
            // handle is converted back to callback via IntPtrToCallback.
            var handle = GCHandle.Alloc(callback);
            return GCHandle.ToIntPtr(handle);
        }

        internal static T IntPtrToTempCallback<T>(IntPtr handle) where T : class
        {
            return IntPtrToCallback<T>(handle, true);
        }

        private static T IntPtrToCallback<T>(IntPtr handle, bool unpinHandle) where T : class
        {
            if (PInvokeUtilities.IsNull(handle))
            {
                return null;
            }

            var gcHandle = GCHandle.FromIntPtr(handle);
            try
            {
                return (T)gcHandle.Target;
            }
            catch (System.InvalidCastException e)
            {
                Logger.e("GC Handle pointed to unexpected type: " + gcHandle.Target.ToString() +
                    ". Expected " + typeof(T));
                throw e;
            }
            finally
            {
                if (unpinHandle)
                {
                    gcHandle.Free();
                }
            }
        }

        // TODO(hsakai): Better way of handling this.
        internal static T IntPtrToPermanentCallback<T>(IntPtr handle) where T : class
        {
            return IntPtrToCallback<T>(handle, false);
        }

        [AOT.MonoPInvokeCallback(typeof(ShowUICallbackInternal))]
        internal static void InternalShowUICallback(CommonErrorStatus.UIStatus status, IntPtr data)
        {
            Logger.d("Showing UI Internal callback: " + status);
            var callback = IntPtrToTempCallback<Action<CommonErrorStatus.UIStatus>>(data);

            try
            {
                callback(status);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing InternalShowAllUICallback. " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal enum Type
        {
            Permanent,
            Temporary}

        ;

        internal static void PerformInternalCallback(string callbackName, Type callbackType,
                                                     IntPtr response, IntPtr userData)
        {
            Logger.d("Entering internal callback for " + callbackName);
            Action<IntPtr> callback = callbackType == Type.Permanent
                ? IntPtrToPermanentCallback<Action<IntPtr>>(userData)
                    : IntPtrToTempCallback<Action<IntPtr>>(userData);

            if (callback == null)
            {
                return;
            }

            try
            {
                callback(response);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing " + callbackName + ". " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal static void PerformInternalCallback<T>(string callbackName, Type callbackType,
                                                        T param1, IntPtr param2, IntPtr userData)
        {
            Logger.d("Entering internal callback for " + callbackName);
            Action<T, IntPtr> callback = null;
            try
            {
                callback = callbackType == Type.Permanent
                ? IntPtrToPermanentCallback<Action<T, IntPtr>>(userData)
                    : IntPtrToTempCallback<Action<T, IntPtr>>(userData);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered converting " + callbackName + ". " +
                    "Smothering to avoid passing exception into Native: " + e);
                return;
            }

            Logger.d("Internal Callback converted to action");
            if (callback == null)
            {
                return;
            }

            try
            {
                callback(param1, param2);
            }
            catch (Exception e)
            {
                Logger.e("Error encountered executing " + callbackName + ". " +
                    "Smothering to avoid passing exception into Native: " + e);
            }
        }

        internal static Action<T> AsOnGameThreadCallback<T>(Action<T> toInvokeOnGameThread)
        {
            return result =>
            {
                if (toInvokeOnGameThread == null)
                {
                    return;
                }
                PlayGamesHelperObject.RunOnGameThread(() => toInvokeOnGameThread(result));
            };
        }

        internal static Action<T1, T2> AsOnGameThreadCallback<T1, T2>(
            Action<T1, T2> toInvokeOnGameThread)
        {
            return (result1, result2) =>
            {
                if (toInvokeOnGameThread == null)
                {
                    return;
                }

                PlayGamesHelperObject.RunOnGameThread(() => toInvokeOnGameThread(result1, result2));
            };
        }

        internal static void AsCoroutine(IEnumerator routine)
        {
            PlayGamesHelperObject.RunCoroutine(routine);
        }

        internal static byte[] IntPtrAndSizeToByteArray(IntPtr data, UIntPtr dataLength)
        {
            if (dataLength.ToUInt64() == 0)
            {
                return null;
            }

            byte[] convertedData = new byte[dataLength.ToUInt32()];
            Marshal.Copy(data, convertedData, 0, (int)dataLength.ToUInt32());

            return convertedData;
        }
    }
}


#endif
