// <copyright file="JavaUtils.cs" company="Google Inc.">
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

#if UNITY_ANDROID

namespace GooglePlayGames.Native {
using System;
using UnityEngine;
using System.Reflection;
using GooglePlayGames.OurUtils;
internal static class JavaUtils {

    private static ConstructorInfo IntPtrConstructor =
        typeof(AndroidJavaObject).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new []{typeof(IntPtr)},
            null);

    /// <summary>
    /// Converts an jobject (represented as an IntPtr) to an AndroidJavaObject by invoking the
    /// hidden IntPtr-taking constructor via reflection. I'm not a fan of bypassing visibility
    /// protection like this, but I haven't found another way of doing it.
    /// </summary>
    /// <returns>An AndroidJavaObject corresponding to the passed pointer.</returns>
    /// <param name="jobject">An IntPtr corresponding to a jobject</param>
    internal static AndroidJavaObject JavaObjectFromPointer(IntPtr jobject) {
        if (jobject == IntPtr.Zero) {
            return null;
        }

        return (AndroidJavaObject) IntPtrConstructor.Invoke(new object[] { jobject });
    }

    /// <summary>
    /// Calls a method on a java object while handling null return values.
    /// Sadly, it appears that calling a method that returns a null Object in Java so we work
    /// around this by catching null pointer exceptions a checking for the word "null".
    /// </summary>
    internal static AndroidJavaObject NullSafeCall(this AndroidJavaObject target,
        string methodName, params object[] args) {
        try {
            return target.Call<AndroidJavaObject>(methodName, args);
        } catch (Exception ex) {
            if (ex.Message.Contains("null")) {
                // expected -- means method returned null
                return null;
            } else {
                Logger.w("CallObjectMethod exception: " + ex);
                return null;
            }
        }
    }

}
}
#endif
