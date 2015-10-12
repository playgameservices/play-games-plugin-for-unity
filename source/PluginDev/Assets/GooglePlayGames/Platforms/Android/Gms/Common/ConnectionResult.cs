// <copyright file="ConnectionResult.cs" company="Google Inc.">
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
namespace Com.Google.Android.Gms.Common
{
    
    public class ConnectionResult : JavaObjWrapper
    {
        public ConnectionResult (IntPtr ptr) : base(ptr)
        {
        }
        const string CLASS_NAME = "com/google/android/gms/common/ConnectionResult";

        public static int SUCCESS
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SUCCESS");
            }
        }
        public static int SERVICE_MISSING
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SERVICE_MISSING");
            }
        }
        public static int SERVICE_VERSION_UPDATE_REQUIRED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SERVICE_VERSION_UPDATE_REQUIRED");
            }
        }
        public static int SERVICE_DISABLED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SERVICE_DISABLED");
            }
        }
        public static int SIGN_IN_REQUIRED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SIGN_IN_REQUIRED");
            }
        }
        public static int INVALID_ACCOUNT
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "INVALID_ACCOUNT");
            }
        }
        public static int RESOLUTION_REQUIRED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "RESOLUTION_REQUIRED");
            }
        }
        public static int NETWORK_ERROR
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "NETWORK_ERROR");
            }
        }
        public static int INTERNAL_ERROR
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "INTERNAL_ERROR");
            }
        }
        public static int SERVICE_INVALID
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SERVICE_INVALID");
            }
        }
        public static int DEVELOPER_ERROR
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "DEVELOPER_ERROR");
            }
        }
        public static int LICENSE_CHECK_FAILED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "LICENSE_CHECK_FAILED");
            }
        }
        public static int CANCELED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "CANCELED");
            }
        }
        public static int TIMEOUT
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "TIMEOUT");
            }
        }
        public static int INTERRUPTED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "INTERRUPTED");
            }
        }
        public static int API_UNAVAILABLE
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "API_UNAVAILABLE");
            }
        }
        public static int SIGN_IN_FAILED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SIGN_IN_FAILED");
            }
        }
        public static int SERVICE_UPDATING
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SERVICE_UPDATING");
            }
        }
        public static int SERVICE_MISSING_PERMISSION
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "SERVICE_MISSING_PERMISSION");
            }
        }
        public static int DRIVE_EXTERNAL_STORAGE_REQUIRED
        {
            get
            {
                return JavaObjWrapper.GetStaticIntField(CLASS_NAME, "DRIVE_EXTERNAL_STORAGE_REQUIRED");
            }
        }
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
        public ConnectionResult(int arg_int_1, object arg_object_2, string arg_string_3)
        {
            base.CreateInstance(CLASS_NAME,  arg_int_1,  arg_object_2,  arg_string_3);
        }
        public ConnectionResult(int arg_int_1, object arg_object_2)
        {
            base.CreateInstance(CLASS_NAME,  arg_int_1,  arg_object_2);
        }
        public ConnectionResult(int arg_int_1)
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
        public int describeContents()
        {
            return base.InvokeCall<int>("describeContents","()I");
        }
        public object getResolution()
        {
            return base.InvokeCall<object>("getResolution","()Landroid/app/PendingIntent;");
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
        public int getErrorCode()
        {
            return base.InvokeCall<int>("getErrorCode","()I");
        }
        public string getErrorMessage()
        {
            return base.InvokeCall<string>("getErrorMessage","()Ljava/lang/String;");
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
#endif