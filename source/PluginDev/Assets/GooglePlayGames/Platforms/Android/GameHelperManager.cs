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
            mGameHelper = new AndroidJavaObject(GameHelperClass, mAndroidClient.GetActivity());
            if (mGameHelper == null) {
                throw new System.Exception("Failed to create GameHelper.");
            }

            // set up the GameHelper
            Logger.d("GHM setting up GameHelper.");
            mGameHelper.Call("enableDebugLog", Logger.DebugLogEnabled, "GameHelper");
            int flags = mGameHelper.GetStatic<int>("CLIENT_ALL");
            Logger.d("GHM client request flags: " + flags);
            GameHelperListener listenerProxy = new GameHelperListener(this, ORIGIN_MAIN_ACTIVITY);
            Logger.d("GHM calling GameHelper.setRequestedClients, " + flags);
            mGameHelper.Call("setRequestedClients", flags);
            Logger.d("GHM calling GameHelper.setup");
            mGameHelper.Call("setup", listenerProxy);

            // set up callbacks so we're informed of pause/unpause events
            PlayGamesHelperObject.SetPauseCallback((bool paused) => {
                if (paused) {
                    OnPause();
                } else {
                    OnResume();
                }
            });

            // start silent auth
            Logger.d("GHM calling GameHelper.onStart to try silent auth.");
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
            c.CallStatic("launchSignIn", mAndroidClient.GetActivity(),
                    new GameHelperListener(this, ORIGIN_SIGN_IN_HELPER_ACTIVITY));
        }

        public AndroidJavaObject GetGamesClient() {
            return mGameHelper.Call<AndroidJavaObject>("getGamesClient");
        }

        public AndroidJavaObject GetAppStateClient() {
            return mGameHelper.Call<AndroidJavaObject>("getAppStateClient");
        }

        public bool IsConnected() {
            return GetGamesClient().Call<bool>("isConnected");
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
    }
}
#endif
