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
using UnityEngine;
using GooglePlayGames.OurUtils;
using System.Collections.Generic;

namespace GooglePlayGames.Android {
    internal class GameHelperManager {
        const string SignInHelperManagerClass = "com.google.example.games.pluginsupport.SignInHelperManager";
        const string BaseGameUtilsPkg = "com.google.example.games.basegameutils";
        const string GameHelperClass = BaseGameUtilsPkg + ".GameHelper";
        const string GameHelperListenerClass = GameHelperClass + "$GameHelperListener";

        AndroidJavaObject mGameHelper = null;
        AndroidClient mAndroidClient = null;

        // Codes that indicate the origin of an OnSignInSucceeded/OnSignInFailed callback
        const int ORIGIN_MAIN_ACTIVITY = 1000;
        const int ORIGIN_SIGN_IN_HELPER_ACTIVITY = 1001;

        internal enum ConnectionState {
            Disconnected, Connecting, Connected
        }
        ConnectionState mConnectionState = ConnectionState.Disconnected;

        internal ConnectionState State { get { return mConnectionState; } }

        internal GameHelperManager(AndroidClient client) {
            mAndroidClient = client;
            Logger.d("Setting up GameHelperManager.");

            // create GameHelper
            Logger.d("GHM creating GameHelper.");
            int flags = JavaConsts.GAMEHELPER_CLIENT_ALL;
            Logger.d("GHM calling GameHelper constructor with flags=" + flags);
            mGameHelper = new AndroidJavaObject(GameHelperClass, mAndroidClient.GetActivity(), 
                    flags);
            if (mGameHelper == null) {
                throw new System.Exception("Failed to create GameHelper.");
            }

            // set up the GameHelper
            Logger.d("GHM setting up GameHelper.");
            mGameHelper.Call("enableDebugLog", Logger.DebugLogEnabled, "GameHelper");
            
            
            GameHelperListener listenerProxy = new GameHelperListener(this, ORIGIN_MAIN_ACTIVITY);
            
            
            // IMPORTANT: we need to tweak the default behavior of GameHelper because 
            // it should never attempt to automatically start the 
            // sign in flow, because this must be done by the SignInActivity! This
            // is why we call setMaxAutoSignInAttempts(0). This is important because if THIS
            // GameHelper were to attempt the sign-in flow, nothing would work, because
            // it needs onActivityResult to be hooked up, and we have no way to access
            // onActivityResult on the Unity player activity. This is why we use a separate
            // Activity for the sign in flow.
            
            Logger.d("GHM Setting GameHelper options.");
            mGameHelper.Call("setMaxAutoSignInAttempts", 0);
            AndroidJavaClass gameOptionsClass = JavaUtil.GetGmsClass("games.Games$GamesOptions");
            AndroidJavaObject builder = gameOptionsClass.CallStatic<AndroidJavaObject>("builder");
            AndroidJavaObject tmp = builder.Call<AndroidJavaObject>("setSdkVariant", 
                    JavaConsts.SDK_VARIANT);
            AndroidJavaObject options = builder.Call<AndroidJavaObject>("build");
            mGameHelper.Call("setGamesApiOptions", options);
            options.Dispose();
            options = null;
            tmp.Dispose();
            tmp = null;
            builder.Dispose();
            builder = null;
            
            Logger.d("GHM calling GameHelper.setup");
            mGameHelper.Call("setup", listenerProxy);
            Logger.d("GHM: GameHelper setup done.");

            // set up callbacks so we're informed of pause/unpause events
            Logger.d("GHM Setting up lifecycle.");
            PlayGamesHelperObject.SetPauseCallback((bool paused) => {
                if (paused) {
                    OnPause();
                } else {
                    OnResume();
                }
            });

            // start initial auth
            Logger.d("GHM calling GameHelper.onStart to try initial auth.");
            mConnectionState = ConnectionState.Connecting;
            mGameHelper.Call("onStart", mAndroidClient.GetActivity());
        }

        void OnResume() {
            Logger.d("GHM got OnResume, relaying to GameHelper");
            mConnectionState = ConnectionState.Connecting;
            mGameHelper.Call("onStart", mAndroidClient.GetActivity());
        }

        void OnPause() {
            Logger.d("GHM got OnPause, relaying to GameHelper");
            mConnectionState = ConnectionState.Connecting;
            mGameHelper.Call("onStop");
        }

        void OnSignInFailed(int origin) {
            Logger.d("GHM got onSignInFailed, origin " + origin + ", notifying AndroidClient.");
            mConnectionState = ConnectionState.Disconnected;
            
            // if the origin is the Sign In Helper activity, check if there's an error to show
            if (origin == ORIGIN_SIGN_IN_HELPER_ACTIVITY) {
                Logger.d("GHM got onSignInFailed from Sign In Helper. Showing error message.");
                using (AndroidJavaClass klass = new AndroidJavaClass(SignInHelperManagerClass)) {
                    klass.CallStatic("showErrorDialog", mAndroidClient.GetActivity());
                }
                Logger.d("Error message shown.");
            }
            
            mAndroidClient.OnSignInFailed();
        }

        void OnSignInSucceeded(int origin) {
            Logger.d("GHM got onSignInSucceeded, origin " + origin + ", notifying AndroidClient.");
            if (origin == ORIGIN_MAIN_ACTIVITY) {
                mConnectionState = ConnectionState.Connected;
                mAndroidClient.OnSignInSucceeded();
            } else if (origin == ORIGIN_SIGN_IN_HELPER_ACTIVITY) {
                // the sign in helper Activity succeeded sign in, so we are ready to
                // start setting up our GameHelper.
                Logger.d("GHM got helper's OnSignInSucceeded, so calling GameHelper.onStart");
                mConnectionState = ConnectionState.Connecting;
                mGameHelper.Call("onStart", mAndroidClient.GetActivity());
            }
        }

        internal void BeginUserInitiatedSignIn() {
            Logger.d("GHM Starting user-initiated sign in.");
            mConnectionState = ConnectionState.Connecting;
            AndroidJavaClass c = new AndroidJavaClass(SignInHelperManagerClass);
            Logger.d("GHM launching sign-in Activity via SignInHelperManager.launchSignIn");
            c.CallStatic("launchSignIn", mAndroidClient.GetActivity(),
                    new GameHelperListener(this, ORIGIN_SIGN_IN_HELPER_ACTIVITY),
                    Logger.DebugLogEnabled);
        }

        public AndroidJavaObject GetApiClient() {
            return mGameHelper.Call<AndroidJavaObject>("getApiClient");
        }

        public bool IsConnected() {
            return mGameHelper.Call<bool>("isSignedIn");
        }
        
        public void SignOut() {
            Logger.d("GHM SignOut");
            if (mConnectionState != ConnectionState.Connected) {
                Logger.w("GameHelperManager.SignOut should only be called when Connected.");
            }
            mGameHelper.Call("signOut");
            mConnectionState = ConnectionState.Disconnected;
        }

        // Proxy for GameHelperListener
        private class GameHelperListener : AndroidJavaProxy {
            private GameHelperManager mContainer;
            private int mOrigin;

            internal GameHelperListener(GameHelperManager mgr, int origin) :
                    base(GameHelperManager.GameHelperListenerClass) {
                mContainer = mgr;
                mOrigin = origin;
            }
            void onSignInFailed() {
                Logger.d("GHM/GameHelperListener got onSignInFailed, origin " +
                        mOrigin + ", notifying GHM.");
                mContainer.OnSignInFailed(mOrigin);
            }
            void onSignInSucceeded() {
                Logger.d("GHM/GameHelperListener got onSignInSucceeded, origin " +
                        mOrigin + ", notifying GHM.");
                mContainer.OnSignInSucceeded(mOrigin);
            }
        }
        
        private object[] makeGmsCallArgs(object[] args) {
            object[] fullArgs = new object[args.Length + 1];
            int i;
            fullArgs[0] = GetApiClient();
            for (i = 1; i < fullArgs.Length; i++) {
                fullArgs[i] = args[i - 1];
            }
            return fullArgs;
        }
        
        public ReturnType CallGmsApi<ReturnType>(string className, string fieldName,
                string methodName, params object[] args) {
            object[] fullArgs = makeGmsCallArgs(args);
            
            if (fieldName != null) {
                return JavaUtil.GetGmsField(className, fieldName).Call<ReturnType>(methodName, 
                        fullArgs);
            } else {
                return JavaUtil.GetGmsClass(className).CallStatic<ReturnType>(methodName, fullArgs);
            }
        }
        
        public void CallGmsApi(string className, string fieldName, string methodName, 
                params object[] args) {
            object[] fullArgs = makeGmsCallArgs(args);
            
            if (fieldName != null) {
                JavaUtil.GetGmsField(className, fieldName).Call(methodName, fullArgs);
            } else {
                JavaUtil.GetGmsClass(className).CallStatic(methodName, fullArgs);
            }
        }
        
        public void CallGmsApiWithResult(string className, string fieldName,
                string methodName, AndroidJavaProxy callbackProxy, params object[] args) {
            
            using (AndroidJavaObject pendingResult = CallGmsApi<AndroidJavaObject>(className, 
                    fieldName, methodName, args)) {
                pendingResult.Call("setResultCallback", callbackProxy);
            }
        }
    }
}
#endif
