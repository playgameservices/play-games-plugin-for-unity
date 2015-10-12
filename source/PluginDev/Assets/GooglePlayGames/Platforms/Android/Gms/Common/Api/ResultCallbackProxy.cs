// <copyright file="ResultCallbackProxy.cs" company="Google Inc.">
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
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
using Google.Developers;
using System;
using UnityEngine;
namespace Com.Google.Android.Gms.Common.Api
{
    
    public abstract class ResultCallbackProxy<R> : JavaInterfaceProxy , ResultCallback<R>
        where R : Result
    {
        const string CLASS_NAME = "com/google/android/gms/common/api/ResultCallback";

        public ResultCallbackProxy () : base(CLASS_NAME)
        {
        }
        public abstract void OnResult(R arg_Result_1);
        public void onResult(R arg_Result_1)
        {
            OnResult( arg_Result_1);
        }
        public void onResult(AndroidJavaObject arg_Result_1)
        {
            IntPtr ptr = arg_Result_1.GetRawObject();
            R mapped_arg_Result_1;
            System.Reflection.ConstructorInfo c = typeof(R).GetConstructor(new Type[] { ptr.GetType() });
            if (c != null)
            {
                mapped_arg_Result_1 = (R)c.Invoke(new object[] { ptr });
            }
            else
            {
                // check for no arg ctor
                System.Reflection.ConstructorInfo c0 = typeof(R).GetConstructor(new Type[0]);
                mapped_arg_Result_1 = (R)c0.Invoke(new object[0]);
                System.Runtime.InteropServices.Marshal.PtrToStructure(ptr, mapped_arg_Result_1);
            }
            OnResult(mapped_arg_Result_1);
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
#endif
