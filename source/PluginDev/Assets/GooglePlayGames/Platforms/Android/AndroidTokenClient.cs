// <copyright file="AndroidTokenClient.cs" company="Google Inc.">
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
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.OurUtils;
    using Com.Google.Android.Gms.Common.Api;
    using UnityEngine;

    internal class AndroidTokenClient: TokenClient
    {
        private const string TokenFragmentClass = "com.google.games.bridge.TokenFragment";
        private const string FetchTokenSignature =
            "(Landroid/app/Activity;Ljava/lang/String;ZZZLjava/lang/String;)Lcom/google/android/gms/common/api/PendingResult;";
        private const string FetchTokenMethod = "fetchToken";

        private bool fetchingEmail = false;
        private bool fetchingAccessToken = false;
        private bool fetchingIdToken = false;
        private string accountName;
        private string accessToken;
        private string idToken;
        private string idTokenScope;
        private Action<string> idTokenCb;
        private string rationale;

        private bool apiAccessDenied = false;
        private int apiWarningFreq = 100000;
        private int apiWarningCount = 0;
        private int webClientWarningFreq = 100000;
        private int webClientWarningCount = 0;

        public static AndroidJavaObject GetActivity()
        {
            using (var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return jc.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        public void SetRationale(string rationale)
        {
            this.rationale = rationale;
        }

        internal void Fetch(string scope,
                            bool fetchEmail,
                            bool fetchAccessToken,
                            bool fetchIdToken,
                            Action<bool> doneCallback)
        {
            if (apiAccessDenied)
            {
                //don't spam the log, only do this every so often
                if (apiWarningCount++ % apiWarningFreq == 0)
                {
                    GooglePlayGames.OurUtils.Logger.w("Access to API denied");
                    // avoid int overflow
                    apiWarningCount = (apiWarningCount/ apiWarningFreq) + 1;
                }

                doneCallback(false);
                return;
            }
            PlayGamesHelperObject.RunOnGameThread(() =>
            FetchToken(scope, rationale, fetchEmail, fetchAccessToken, fetchIdToken, (rc, access, id, email) =>
                    {
                        if (rc != (int)CommonStatusCodes.Success)
                        {
                            apiAccessDenied = rc == (int)CommonStatusCodes.AuthApiAccessForbidden;
                            GooglePlayGames.OurUtils.Logger.w("Non-success returned from fetch: " + rc);
                            doneCallback(false);
                            return;
                        }

                        if (fetchAccessToken)
                        {
                            GooglePlayGames.OurUtils.Logger.d("a = " + access);
                        }
                        if (fetchEmail)
                        {
                            GooglePlayGames.OurUtils.Logger.d("email = " + email);
                        }

                        if (fetchIdToken)
                        {
                            GooglePlayGames.OurUtils.Logger.d("idt = " + id);
                        }

                        if (fetchAccessToken && !string.IsNullOrEmpty(access))
                        {
                            accessToken = access;
                        }
                        if (fetchIdToken && !string.IsNullOrEmpty(id))
                        {
                            idToken = id;
                            idTokenCb(idToken);
                        }
                        if (fetchEmail && !string.IsNullOrEmpty(email))
                        {
                            this.accountName = email;
                        }
                        doneCallback(true);
                    }));
        }


        internal static void FetchToken(string scope, string rationale, bool fetchEmail,
                                        bool fetchAccessToken, bool fetchIdToken, Action<int, string, string, string> callback)
        {
            object[] objectArray = new object[6];
            jvalue[] jArgs = AndroidJNIHelper.CreateJNIArgArray(objectArray);
            try
            {
                using (var bridgeClass = new AndroidJavaClass(TokenFragmentClass))
                {
                    using (var currentActivity = AndroidTokenClient.GetActivity())
                    {
                        // Unity no longer supports constructing an AndroidJavaObject using an IntPtr,
                        // so I have to manually munge with JNI here.
                        IntPtr methodId = AndroidJNI.GetStaticMethodID(bridgeClass.GetRawClass(),
                                              FetchTokenMethod,
                                              FetchTokenSignature);
                        jArgs[0].l = currentActivity.GetRawObject();
                        jArgs[1].l = AndroidJNI.NewStringUTF(rationale);
                        jArgs[2].z = fetchEmail;
                        jArgs[3].z = fetchAccessToken;
                        jArgs[4].z = fetchIdToken;
                        jArgs[5].l = AndroidJNI.NewStringUTF(scope);

                        IntPtr ptr =
                            AndroidJNI.CallStaticObjectMethod(bridgeClass.GetRawClass(), methodId, jArgs);

                        PendingResult<TokenResult> pr = new PendingResult<TokenResult>(ptr);
                        pr.setResultCallback(new TokenResultCallback(callback));
                    }
                }
            }
            catch (Exception e)
            {
                GooglePlayGames.OurUtils.Logger.e("Exception launching token request: " + e.Message);
                GooglePlayGames.OurUtils.Logger.e(e.ToString());
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(objectArray, jArgs);
            }
        }

        /// <summary>
        /// Gets the account name of the currently signed-in user to later use for token retrieval.
        /// </summary>
        /// <remarks>Currently only used internally to encourage using the unique player ID instead.</remarks>
        /// <returns>The current user's Google account name.</returns>
        private string GetAccountName()
        {
            if (string.IsNullOrEmpty(accountName))
            {
                if (!fetchingEmail)
                {
                    fetchingEmail = true;
                    Fetch(idTokenScope, true, false, false, (ok) => fetchingEmail = false);
                }
            }

            return accountName;
        }

        /// <summary>Gets the current user's email.</summary>
        /// <returns>A string representing the email.</returns>
        public string GetEmail()
        {
            return GetAccountName();
        }

        /// <summary>Gets the access token currently associated with the Unity activity.</summary>
        /// <returns>The OAuth 2.0 access token.</returns>
        public string GetAccessToken()
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                if (!fetchingAccessToken)
                {
                    fetchingAccessToken = true;
                    Fetch(idTokenScope, false, true, false, (rc) => fetchingAccessToken = false);
                }
            }
            return accessToken;
        }

        /// <summary>Gets the OpenID Connect ID token for authentication with a server backend.</summary>
        /// <param name="serverClientId">Server client ID from console.developers.google.com or the Play Games
        /// services console.</param>
        /// <param name="idTokenCallback"> A callback to be invoked after token is retrieved. Will be passed null value
        /// on failure. </param>
        [Obsolete("Use PlayGamesPlatform.GetServerAuthCode()")]
        public void GetIdToken(string serverClientId, Action<string> idTokenCallback)
        {
            if (string.IsNullOrEmpty(serverClientId))
            {
                if (webClientWarningCount++ % webClientWarningFreq == 0)
                {
                    GooglePlayGames.OurUtils.Logger.w("serverClientId is empty, cannot get Id Token");
                    webClientWarningCount = webClientWarningCount / webClientWarningFreq + 1;
                }
                idTokenCallback(null);
                return;
            }
            string newScope = "audience:server:client_id:" + serverClientId;
            if (string.IsNullOrEmpty(idToken) || (newScope != idTokenScope))
            {
                if (!fetchingIdToken)
                {
                    fetchingIdToken = true;
                    idTokenScope = newScope;
                    idTokenCb = idTokenCallback;
                    Fetch(idTokenScope, false, false, true, (ok) => {
                        fetchingIdToken = false;

                        if(!ok)
                        {
                            idTokenCb(null);
                        }
                        else
                        {
                            idTokenCb(idToken);
                        }
                    });
                }
            }
            else
            {
                idTokenCallback(idToken);
            }
        }
    }

    class TokenResult : Google.Developers.JavaObjWrapper , Result
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

        public String getAccessToken()
        {
            return InvokeCall<string>("getAccessToken", "()Ljava/lang/String;");
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
            callback(arg_Result_1.getStatus().getStatusCode(), arg_Result_1.getAccessToken(), arg_Result_1.getIdToken(),
                arg_Result_1.getEmail());
        }
    }
}
#endif
