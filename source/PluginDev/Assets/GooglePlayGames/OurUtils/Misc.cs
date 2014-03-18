/*
 * Copyright (C) 2013 Google Inc.
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

using System;
using UnityEngine;
using System.Collections.Generic;

namespace GooglePlayGames.OurUtils {
    public class Misc {
         public static bool BuffersAreIdentical(byte[] a, byte[] b) {
            int i;
            if (a == b) {
                // not only identical but the very same!
                return true;
            }
            if (a == null || b == null) {
                // one of them is null, the other one isn't
                return false;
            }
            if (a.Length != b.Length) {
                return false;
            }
            for (i = 0; i < a.Length; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }
            return true;
        }
        
        public static void CopyList<T>(List<T> destList, List<T> sourceList) {
            destList.Clear();
            foreach (T t in sourceList) {
                destList.Add(t);
            }
        }
        
        public static byte[] GetSubsetBytes(byte[] array, int offset, int length) {
            if (offset == 0 && length == array.Length) {
                return array;
            }
            
            byte[] piece = new byte[length];
            Array.Copy(array, offset, piece, 0, length);
            return piece;
        }
    }
}

