// <copyright file="PInvokeUtilities.cs" company="Google Inc.">
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
    using System.Text;
    using System.Collections.Generic;

    static class PInvokeUtilities
    {
        private static readonly DateTime UnixEpoch =
            DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);

        internal static HandleRef CheckNonNull(HandleRef reference)
        {
            if (IsNull(reference))
            {
                throw new System.InvalidOperationException();
            }

            return reference;
        }

        internal static bool IsNull(HandleRef reference)
        {
            return IsNull(HandleRef.ToIntPtr(reference));
        }

        internal static bool IsNull(IntPtr pointer)
        {
            return pointer.Equals(IntPtr.Zero);
        }

        internal static DateTime FromMillisSinceUnixEpoch(long millisSinceEpoch)
        {
            // DateTime in C# uses Gregorian Calendar rather than millis since epoch.
            // We account for this by manually constructing a timestamp for Unix Epoch
            // and incrementing it by the indicated number of milliseconds.
            return UnixEpoch.Add(TimeSpan.FromMilliseconds(millisSinceEpoch));
        }

        internal delegate UIntPtr OutStringMethod([In, Out] byte[] out_bytes, UIntPtr out_size);

        internal static String OutParamsToString(OutStringMethod outStringMethod)
        {
            UIntPtr requiredSize = outStringMethod(null, UIntPtr.Zero);
            if (requiredSize.Equals(UIntPtr.Zero))
            {
                return null;
            }

            string str = null;
            try
            {
                byte[] array = new byte[requiredSize.ToUInt32()];
                outStringMethod(array, requiredSize);
                str = Encoding.UTF8.GetString(array, 0, (int)requiredSize.ToUInt32() - 1);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Exception creating string from char array: " + e);
                str = string.Empty;
            }
             return str;
        }

        internal delegate UIntPtr OutMethod<T>([In, Out] T[] out_bytes,UIntPtr out_size);

        internal static T[] OutParamsToArray<T>(OutMethod<T> outMethod)
        {
            UIntPtr requiredSize = outMethod(null, UIntPtr.Zero);

            if (requiredSize.Equals(UIntPtr.Zero))
            {
                return new T[0];
            }

            T[] array = new T[requiredSize.ToUInt64()];
            outMethod(array, requiredSize);
            return array;
        }

        internal static IEnumerable<T> ToEnumerable<T>(UIntPtr size, Func<UIntPtr, T> getElement)
        {
            for (ulong i = 0; i < size.ToUInt64(); i++)
            {
                yield return getElement(new UIntPtr(i));
            }
        }

        internal static IEnumerator<T> ToEnumerator<T>(UIntPtr size, Func<UIntPtr, T> getElement)
        {
            return ToEnumerable<T>(size, getElement).GetEnumerator();
        }

        internal static UIntPtr ArrayToSizeT<T>(T[] array)
        {
            if (array == null)
            {
                return UIntPtr.Zero;
            }

            return new UIntPtr((ulong)array.Length);
        }

        internal static long ToMilliseconds(TimeSpan span)
        {
            double millis = span.TotalMilliseconds;

            if (millis > long.MaxValue)
            {
                return long.MaxValue;
            }

            if (millis < long.MinValue)
            {
                return long.MinValue;
            }

            return Convert.ToInt64(millis);
        }
    }
}


#endif
