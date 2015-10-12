// <copyright file="PendingResult.cs" company="Google Inc.">
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
    
    public class PendingResult<R> : JavaObjWrapper
        where R : Result
    {
        public PendingResult (IntPtr ptr) : base(ptr)
        {
        }
        const string CLASS_NAME = "com/google/android/gms/common/api/PendingResult";

        public PendingResult() : base("com.google.android.gms.common.api.PendingResult")
        {
        }
        public R await(long arg_long_1, object arg_object_2)
        {
            return base.InvokeCall<R>("await","(JLjava/util/concurrent/TimeUnit;)Lcom/google/android/gms/common/api/Result;", arg_long_1,  arg_object_2);
        }
        public R await()
        {
            return base.InvokeCall<R>("await","()Lcom/google/android/gms/common/api/Result;");
        }
        public bool isCanceled()
        {
            return base.InvokeCall<bool>("isCanceled","()Z");
        }
        public void cancel()
        {
            base.InvokeCallVoid("cancel","()V");
        }
        public void setResultCallback(ResultCallback<R> arg_ResultCallback_1)
        {
            base.InvokeCallVoid("setResultCallback","(Lcom/google/android/gms/common/api/ResultCallback;)V", arg_ResultCallback_1);
        }
        public void setResultCallback(ResultCallback<R> arg_ResultCallback_1, long arg_long_2, object arg_object_3)
        {
            base.InvokeCallVoid("setResultCallback","(Lcom/google/android/gms/common/api/ResultCallback;JLjava/util/concurrent/TimeUnit;)V", arg_ResultCallback_1,  arg_long_2,  arg_object_3);
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
#endif
