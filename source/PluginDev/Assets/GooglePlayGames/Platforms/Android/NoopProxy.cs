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

#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.Android {
    internal class NoopProxy : AndroidJavaProxy {
        string mInterfaceClass;
        
        internal NoopProxy(string javaInterfaceClass) : base(javaInterfaceClass) {
            mInterfaceClass = javaInterfaceClass;
        }
        
        public override AndroidJavaObject Invoke(string methodName, object[] args) {
            Logger.d("NoopProxy for "  + mInterfaceClass + " got call to " + methodName);
            return null;
        }

        public override AndroidJavaObject Invoke(string methodName, AndroidJavaObject[] javaArgs) {
            Logger.d("NoopProxy for "  + mInterfaceClass + " got call to " + methodName);
            return null;
        }
    }
}

#endif
