// <copyright file="Status.cs" company="Google Inc.">
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
using Google.Developers;
using System;
using UnityEngine;
namespace Com.Google.Android.Gms.Common.Api
{
    
    public class Status : JavaObjWrapper, Result
    {
        public Status (IntPtr ptr) : base(ptr)
        {
        }
        const string CLASS_NAME = "com/google/android/gms/common/api/Status";

        public static object CREATOR
        {
            get
            {
                return JavaObjWrapper.GetStaticObjectField<object>(CLASS_NAME, "CREATOR", "Landroid/os/Parcelable$Creator;");
            }
        }
        public static string NULL
        {
            get
            {
                return JavaObjWrapper.GetStaticStringField(CLASS_NAME, "NULL");
            }
        }
        public static int CONTENTS_FILE_DESCRIPTOR
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "CONTENTS_FILE_DESCRIPTOR");
            }
        }
        public static int PARCELABLE_WRITE_RETURN_VALUE
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "PARCELABLE_WRITE_RETURN_VALUE");
            }
        }
        public Status(int arg_int_1, string arg_string_2, object arg_object_3)
        {
            base.CreateInstance(CLASS_NAME,  arg_int_1,  arg_string_2,  arg_object_3);
        }
        public Status(int arg_int_1, string arg_string_2)
        {
            base.CreateInstance(CLASS_NAME,  arg_int_1,  arg_string_2);
        }
        public Status(int arg_int_1)
        {
            base.CreateInstance(CLASS_NAME,  arg_int_1);
        }
        public bool equals(object arg_object_1)
        {
            return base.InvokeCall<bool>("equals","(Ljava/lang/Object;)Z", arg_object_1);
        }
        public string toString()
        {
            return base.InvokeCall<string>("toString","()Ljava/lang/String;");
        }
        public int hashCode()
        {
            return base.InvokeCall<int>("hashCode","()I");
        }
        public bool isInterrupted()
        {
            return base.InvokeCall<bool>("isInterrupted","()Z");
        }
        public Status getStatus()
        {
            return base.InvokeCall<Status>("getStatus","()Lcom/google/android/gms/common/api/Status;");
        }
        public bool isCanceled()
        {
            return base.InvokeCall<bool>("isCanceled","()Z");
        }
        public int describeContents()
        {
            return base.InvokeCall<int>("describeContents","()I");
        }
        public object getResolution()
        {
            return base.InvokeCall<object>("getResolution","()Landroid/app/PendingIntent;");
        }
        public int getStatusCode()
        {
            return base.InvokeCall<int>("getStatusCode","()I");
        }
        public string getStatusMessage()
        {
            return base.InvokeCall<string>("getStatusMessage","()Ljava/lang/String;");
        }
        public bool hasResolution()
        {
            return base.InvokeCall<bool>("hasResolution","()Z");
        }
        public void startResolutionForResult(object arg_object_1, int arg_int_2)
        {
            base.InvokeCallVoid("startResolutionForResult","(Landroid/app/Activity;I)V", arg_object_1,  arg_int_2);
        }
        public void writeToParcel(object arg_object_1, int arg_int_2)
        {
            base.InvokeCallVoid("writeToParcel","(Landroid/os/Parcel;I)V", arg_object_1,  arg_int_2);
        }
        public bool isSuccess()
        {
            return base.InvokeCall<bool>("isSuccess","()Z");
        }
    }
}
//
// ****   GENERATED FILE  DO NOT EDIT !!!  ****//
//
