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
using System.Collections.Generic;

namespace GooglePlayGames.Native.PInvoke {
static class PInvokeUtilities {

    private static readonly DateTime UnixEpoch =
        DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);

    internal static HandleRef CheckNonNull(HandleRef reference) {
        if (IsNull(reference)) {
            throw new System.InvalidOperationException();
        }

        return reference;
    }

    internal static bool IsNull(HandleRef reference) {
        return IsNull(HandleRef.ToIntPtr(reference));
    }

    internal static bool IsNull(IntPtr pointer) {
        return pointer.Equals(IntPtr.Zero);
    }

    internal static DateTime FromMillisSinceUnixEpoch(long millisSinceEpoch) {
        // DateTime in C# uses Gregorian Calendar rather than millis since epoch.
        // We account for this by manually constructing a timestamp for Unix Epoch
        // and incrementing it by the indicated number of milliseconds.
        return UnixEpoch.Add(TimeSpan.FromMilliseconds(millisSinceEpoch));
    }

    internal delegate UIntPtr OutStringMethod(StringBuilder out_string, UIntPtr out_size);

    internal static String OutParamsToString(OutStringMethod outStringMethod) {
        UIntPtr requiredSize = outStringMethod(null, UIntPtr.Zero);
        if (requiredSize.Equals(UIntPtr.Zero)) {
            return null;
        }

        StringBuilder sizedBuilder = new StringBuilder((int)requiredSize.ToUInt32());
        outStringMethod(sizedBuilder, requiredSize);
        return sizedBuilder.ToString();
    }

    internal delegate UIntPtr OutBytesMethod([In, Out] byte[] out_bytes, UIntPtr out_size);

    internal static byte[] OutParamsToBytes(OutBytesMethod outBytesMethod) {
        UIntPtr requiredSize = outBytesMethod(null, UIntPtr.Zero);

        if (requiredSize.Equals(UIntPtr.Zero)) {
            return new byte[0];
        }

        byte[] bytes = new byte[requiredSize.ToUInt64()];
        outBytesMethod(bytes, requiredSize);
        return bytes;
    }

    internal static IEnumerable<T> ToEnumerable<T>(UIntPtr size, Func<UIntPtr, T> getElement) {
        for(ulong i = 0; i < size.ToUInt64(); i++) {
            yield return getElement(new UIntPtr(i));
        }
    }

    internal static IEnumerator<T> ToEnumerator<T>(UIntPtr size, Func<UIntPtr, T> getElement) {
        return ToEnumerable<T>(size, getElement).GetEnumerator();
    }

    internal static UIntPtr ArrayToSizeT<T>(T[] array) {
        if (array == null) {
            return UIntPtr.Zero;
        }

        return new UIntPtr((ulong)array.Length);
    }
}
}


#endif
