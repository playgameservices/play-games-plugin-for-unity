﻿// <copyright file="AndroidTokenClient.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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
//  limitations under the License.
// </copyright>

#if UNITY_ANDROID
namespace GooglePlayGames.Android
{
    using System;
    using BasicApi;
    using OurUtils;
    using Com.Google.Android.Gms.Common.Api;
    using UnityEngine;
    using System.Collections.Generic;

    internal class AndroidTokenClient : TokenClient
    {
        private const string TokenFragmentClass = "com.google.games.bridge.TokenFragment";

        /*
         * fetchToken(Activity parentActivity,
                      boolean silent,
                      boolean requestAuthCode,
                      boolean requestEmail,
                      boolean requestIdToken,
                      String webClientId,
                      boolean forceRefreshToken,
                      String[] additionalScopes,
                      boolean hidePopups,
                      String accountName)
         */
        private const string FetchTokenSignature =
            "(Landroid/app/Activity;ZZZZLjava/lang/String;Z[Ljava/lang/String;ZLjava/lang/String;)Lcom/google/android/gms/common/api/PendingResult;";

        private const string FetchTokenMethod = "fetchToken";

        private const string GetAnotherAuthCodeMethod = "getAnotherAuthCode";
        private const string GetAnotherAuthCodeSignature =
              "(Landroid/app/Activity;ZZZLjava/lang/String;)Lcom/google/android/gms/common/api/PendingResult;";

        // These are the configuration values.
        private bool requestEmail;
        private bool requestAuthCode;
        private bool requestIdToken;
        private List<string> oauthScopes;
        private string webClientId;
        private bool forceRefresh;
        private bool hidePopups;
        private string accountName;

        // These are the results
        private string email;
        private string authCode;
        private string idToken;

        public static AndroidJavaObject CreateInvisibleView() {
            using (var jc = new AndroidJavaClass(TokenFragmentClass))
            {
                return jc.CallStatic<AndroidJavaObject>("createInvisibleView", GetActivity());
            }
        }

        public static AndroidJavaObject GetActivity()
        {
            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        public void SetRequestAuthCode(bool flag, bool forceRefresh)
        {
            requestAuthCode = flag;
            this.forceRefresh = forceRefresh;
        }

        public void SetRequestEmail(bool flag)
        {
            requestEmail = flag;
        }

        public void SetRequestIdToken(bool flag)
        {
            requestIdToken = flag;
        }

        public void SetWebClientId(string webClientId)
        {
            this.webClientId = webClientId;
        }

        public void SetHidePopups(bool flag)
        {
            this.hidePopups = flag;
        }

        public void SetAccountName(string accountName)
        {
            this.accountName = accountName;
        }

        public void AddOauthScopes(params string[] scopes)
        {
            if (scopes != null)
            {
                if (oauthScopes == null)
                {
                    oauthScopes = new List<string>();
                }
                oauthScopes.AddRange(scopes);
            }
        }

        public void Signout()
        {
            authCode = null;
            email = null;
            idToken = null;
            PlayGamesHelperObject.RunOnGameThread(() => {
                Debug.Log("Calling Signout in token client");
                AndroidJavaClass cls = new AndroidJavaClass(TokenFragmentClass);
                cls.CallStatic("signOut", GetActivity());
            });
        }

        public void FetchTokens(bool silent, Action<int> callback)
        {
            PlayGamesHelperObject.RunOnGameThread(() => DoFetchToken(silent, callback));
        }

        internal void DoFetchToken(bool silent, Action<int> callback)
        {
            object[] objectArray = new object[10];
            jvalue[] jArgs = AndroidJNIHelper.CreateJNIArgArray(objectArray);

            try
            {
                using (var bridgeClass = new AndroidJavaClass(TokenFragmentClass))
                {
                    using (var currentActivity = GetActivity())
                    {
                        // Unity no longer supports constructing an AndroidJavaObject using an IntPtr,
                        // so I have to manually munge with JNI here.
                        IntPtr methodId = AndroidJNI.GetStaticMethodID(bridgeClass.GetRawClass(),
                                              FetchTokenMethod,
                                              FetchTokenSignature);
                        jArgs[0].l = currentActivity.GetRawObject();
                        jArgs[1].z = silent;
                        jArgs[2].z = requestAuthCode;
                        jArgs[3].z = requestEmail;
                        jArgs[4].z = requestIdToken;
                        jArgs[5].l = AndroidJNI.NewStringUTF(webClientId);
                        jArgs[6].z = forceRefresh;
                        jArgs[7].l = AndroidJNIHelper.ConvertToJNIArray(oauthScopes.ToArray());
                        jArgs[8].z = hidePopups;
                        jArgs[9].l = AndroidJNI.NewStringUTF(accountName);

                        IntPtr ptr =
                            AndroidJNI.CallStaticObjectMethod(bridgeClass.GetRawClass(), methodId, jArgs);

                        PendingResult<TokenResult> pr = new PendingResult<TokenResult>(ptr);
                        pr.setResultCallback(new TokenResultCallback((rc, authCode, email, idToken) =>
                        {
                            this.authCode = authCode;
                            this.email = email;
                            this.idToken = idToken;
                            callback(rc);
                            }));
                    }
                }
            }
            catch (Exception e)
            {
                OurUtils.Logger.e("Exception launching token request: " + e.Message);
                OurUtils.Logger.e(e.ToString());
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(objectArray, jArgs);
            }
        }

        /// <summary>Gets the email selected by the current player.</summary>
        /// <remarks>This is not necessarily the email address of the player.  It
        /// is just the account selected by the player from a list of accounts
        /// present on the device.
        /// </remarks>
        /// <returns>A string representing the email.</returns>
        public string GetEmail()
        {
            return email;
        }

        public string GetAuthCode()
        {
            return authCode;
        }

        /// <summary>
        /// Gets another server auth code.
        /// </summary>
        /// <remarks>This method should be called after authenticating, and exchanging
        /// the initial server auth code for a token.  This is implemented by signing in
        /// silently, which if successful returns almost immediately and with a new
        /// server auth code.
        /// </remarks>
        /// <param name="reAuthenticateIfNeeded">Calls Authenticate if needed when
        /// retrieving another auth code. </param>
        /// <param name="callback">Callback.</param>
        public void GetAnotherServerAuthCode(bool reAuthenticateIfNeeded, Action<string> callback)
        {
            PlayGamesHelperObject.RunOnGameThread(() => DoGetAnotherServerAuthCode(reAuthenticateIfNeeded, callback));
        }

        internal void DoGetAnotherServerAuthCode(bool reAuthenticateIfNeeded, Action<string> callback)
        {
            object[] objectArray = new object[5];
            jvalue[] jArgs = AndroidJNIHelper.CreateJNIArgArray(objectArray);

            try
            {
                using (var bridgeClass = new AndroidJavaClass(TokenFragmentClass))
                {
                    using (var currentActivity = GetActivity())
                    {
                        // Unity no longer supports constructing an AndroidJavaObject using an IntPtr,
                        // so I have to manually munge with JNI here.
                        IntPtr methodId = AndroidJNI.GetStaticMethodID(bridgeClass.GetRawClass(),
                                GetAnotherAuthCodeMethod,
                                GetAnotherAuthCodeSignature);
                        jArgs[0].l = currentActivity.GetRawObject();
                        jArgs[1].z = reAuthenticateIfNeeded;
                        jArgs[2].z = requestEmail;
                        jArgs[3].z = requestIdToken;
                        jArgs[4].l = AndroidJNI.NewStringUTF(webClientId);

                        IntPtr ptr =
                            AndroidJNI.CallStaticObjectMethod(bridgeClass.GetRawClass(), methodId, jArgs);

                        PendingResult<TokenResult> pr = new PendingResult<TokenResult>(ptr);
                        pr.setResultCallback(new TokenResultCallback((rc, authCode, email, idToken) =>
                        {
                            this.authCode = authCode;
                            callback(authCode);
                        }));
                     }
                }
            }
            catch (Exception e)
            {
                OurUtils.Logger.e("Exception launching auth code request: " + e.Message);
                OurUtils.Logger.e(e.ToString());
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(objectArray, jArgs);
            }
        }

        /// <summary>Gets the OpenID Connect ID token for authentication with a server backend.</summary>
        /// <param name="serverClientId">Server client ID from console.developers.google.com or the Play Games
        /// services console.</param>
        /// <param name="idTokenCallback"> A callback to be invoked after token is retrieved. Will be passed null value
        /// on failure. </param>
        public string GetIdToken()
        {
            return idToken;
        }

    }

    class TokenResult : Google.Developers.JavaObjWrapper, Result
    {
        #region Result implementation

        public TokenResult(IntPtr ptr)
            : base(ptr)
        {
        }

        public Status getStatus()
        {
            IntPtr obj = InvokeCall<IntPtr>("getStatus", "()Lcom/google/android/gms/common/api/Status;");
            return new Status(obj);
        }

        #endregion

        public int getStatusCode()
        {
            return InvokeCall<int>("getStatusCode", "()I");

        }

        public String getAuthCode()
        {
            return InvokeCall<string>("getAuthCode", "()Ljava/lang/String;");
        }

        public String getEmail()
        {
            return InvokeCall<string>("getEmail", "()Ljava/lang/String;");
        }

        public String getIdToken()
        {
            return InvokeCall<string>("getIdToken", "()Ljava/lang/String;");
        }

    }

    class TokenResultCallback : ResultCallbackProxy<TokenResult>
    {
        private Action<int, string, string, string> callback;

        public TokenResultCallback(Action<int, string, string, string> callback)
        {
            this.callback = callback;
        }

        public override void OnResult(TokenResult arg_Result_1)
        {
            if (callback != null) {
                    callback(arg_Result_1.getStatusCode(),
                             arg_Result_1.getAuthCode(),
                             arg_Result_1.getEmail(),
                             arg_Result_1.getIdToken());
            }
        }

        public
#if UNITY_2017_1_OR_NEWER
        override
#endif
        string toString()
        {
            return ToString();
        }
    }
}
#endif
