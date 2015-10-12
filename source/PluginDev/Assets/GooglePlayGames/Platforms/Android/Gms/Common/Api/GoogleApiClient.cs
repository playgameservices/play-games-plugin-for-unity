// <copyright file="GoogleApiClient.cs" company="Google Inc.">
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

//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
#if UNITY_ANDROID
using Com.Google.Android.Gms.Common;
using Google.Developers;
using System;
using UnityEngine;
namespace Com.Google.Android.Gms.Common.Api
{
    
    public class GoogleApiClient : JavaObjWrapper
    {
        public GoogleApiClient (IntPtr ptr) : base(ptr)
        {
        }
        const string CLASS_NAME = "com/google/android/gms/common/api/GoogleApiClient";

        public GoogleApiClient() : base("com.google.android.gms.common.api.GoogleApiClient")
        {
        }
        public object getContext()
        {
            return base.InvokeCall<object>("getContext","()Landroid/content/Context;");
        }
        public void connect()
        {
            base.InvokeCallVoid("connect","()V");
        }
        public void disconnect()
        {
            base.InvokeCallVoid("disconnect","()V");
        }
        public void dump(string arg_string_1, object arg_object_2, object arg_object_3, string[] arg_string_4)
        {
            base.InvokeCallVoid("dump","(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V", arg_string_1,  arg_object_2,  arg_object_3,  arg_string_4);
        }
        public ConnectionResult blockingConnect(long arg_long_1, object arg_object_2)
        {
            return base.InvokeCall<ConnectionResult>("blockingConnect","(JLjava/util/concurrent/TimeUnit;)Lcom/google/android/gms/common/ConnectionResult;", arg_long_1,  arg_object_2);
        }
        public ConnectionResult blockingConnect()
        {
            return base.InvokeCall<ConnectionResult>("blockingConnect","()Lcom/google/android/gms/common/ConnectionResult;");
        }
        public PendingResult<Status> clearDefaultAccountAndReconnect()
        {
            return base.InvokeCall<PendingResult<Status>>("clearDefaultAccountAndReconnect","()Lcom/google/android/gms/common/api/PendingResult;");
        }
        public ConnectionResult getConnectionResult(object arg_object_1)
        {
            return base.InvokeCall<ConnectionResult>("getConnectionResult","(Lcom/google/android/gms/common/api/Api;)Lcom/google/android/gms/common/ConnectionResult;", arg_object_1);
        }
        public int getSessionId()
        {
            return base.InvokeCall<int>("getSessionId","()I");
        }
        public bool isConnecting()
        {
            return base.InvokeCall<bool>("isConnecting","()Z");
        }
        public bool isConnectionCallbacksRegistered(object arg_object_1)
        {
            return base.InvokeCall<bool>("isConnectionCallbacksRegistered","(Lcom/google/android/gms/common/api/GoogleApiClient$ConnectionCallbacks;)Z", arg_object_1);
        }
        public bool isConnectionFailedListenerRegistered(object arg_object_1)
        {
            return base.InvokeCall<bool>("isConnectionFailedListenerRegistered","(Lcom/google/android/gms/common/api/GoogleApiClient$OnConnectionFailedListener;)Z", arg_object_1);
        }
        public void reconnect()
        {
            base.InvokeCallVoid("reconnect","()V");
        }
        public void registerConnectionCallbacks(object arg_object_1)
        {
            base.InvokeCallVoid("registerConnectionCallbacks","(Lcom/google/android/gms/common/api/GoogleApiClient$ConnectionCallbacks;)V", arg_object_1);
        }
        public void registerConnectionFailedListener(object arg_object_1)
        {
            base.InvokeCallVoid("registerConnectionFailedListener","(Lcom/google/android/gms/common/api/GoogleApiClient$OnConnectionFailedListener;)V", arg_object_1);
        }
        public void stopAutoManage(object arg_object_1)
        {
            base.InvokeCallVoid("stopAutoManage","(Landroid/support/v4/app/FragmentActivity;)V", arg_object_1);
        }
        public void unregisterConnectionCallbacks(object arg_object_1)
        {
            base.InvokeCallVoid("unregisterConnectionCallbacks","(Lcom/google/android/gms/common/api/GoogleApiClient$ConnectionCallbacks;)V", arg_object_1);
        }
        public void unregisterConnectionFailedListener(object arg_object_1)
        {
            base.InvokeCallVoid("unregisterConnectionFailedListener","(Lcom/google/android/gms/common/api/GoogleApiClient$OnConnectionFailedListener;)V", arg_object_1);
        }
        public bool hasConnectedApi(object arg_object_1)
        {
            return base.InvokeCall<bool>("hasConnectedApi","(Lcom/google/android/gms/common/api/Api;)Z", arg_object_1);
        }
        public object getLooper()
        {
            return base.InvokeCall<object>("getLooper","()Landroid/os/Looper;");
        }
        public bool isConnected()
        {
            return base.InvokeCall<bool>("isConnected","()Z");
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
#endif