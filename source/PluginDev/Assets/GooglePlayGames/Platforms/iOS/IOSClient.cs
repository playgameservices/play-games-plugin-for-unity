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

#if UNITY_IPHONE
using System;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.OurUtils;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UnityEngine;

namespace GooglePlayGames.IOS {
    public class IOSClient : IPlayGamesClient {
        // Callback types:
        private delegate void GPGSSuccessCallback(bool success, int userdata);
        private delegate void GPGSUpdateStateCallback(bool success, int slot);
        private delegate int GPGSStateConflictCallback(int slot,
            IntPtr localBufPtr, int localDataSize,
            IntPtr serverBufPtr, int serverDataSize,
            IntPtr resolvBufPtr, int resolvBufCap);
        private delegate void GPGSLoadStateCallback(bool success, int slot,
            IntPtr dataBuf, int dataSize);
		private delegate void GPGSPushNotificationCallback(bool isRealTime,
		                                                   string invitationId,
		                                                   string inviterId, 
		                                                   string inviterName, 
		                                                   int variant);



        // Entry points exposed by the iOS native code (.m files in Assets/Plugins/iOS):
        [DllImport("__Internal")]
        private static extern bool GPGSAuthenticateWithCallback(GPGSSuccessCallback cb, bool silent);

        [DllImport("__Internal")]
        private static extern void GPGSEnableDebugLog(bool enable);

        [DllImport("__Internal")]
        private static extern void GPGSGetPlayerId(StringBuilder outbuf, int bufSize);

        [DllImport("__Internal")]
        private static extern void GPGSGetPlayerName(StringBuilder outbuf, int bufSize);
        
        [DllImport("__Internal")]
        private static extern void GPGSSignOut();

        [DllImport("__Internal")]
        private static extern bool GPGSQueryAchievement(string achId,
            ref bool outIsIncremental, ref bool outIsRevealed,
            ref bool outIsUnlocked, ref int outCurSteps, ref int outTotalSteps);

        [DllImport("__Internal")]
        private static extern void GPGSUnlockAchievement(string achId, GPGSSuccessCallback cb, int userdata);

        [DllImport("__Internal")]
        private static extern void GPGSRevealAchievement(string achId, GPGSSuccessCallback cb, int userdata);

        [DllImport("__Internal")]
        private static extern void GPGSIncrementAchievement(string achId, int steps,
            GPGSSuccessCallback cb, int userdata);

        [DllImport("__Internal")]
        private static extern void GPGSShowAchievementsUI();

        [DllImport("__Internal")]
        private static extern void GPGSShowLeaderboardsUI(string lbId);

        [DllImport("__Internal")]
        private static extern void GPGSSubmitScore(string lbId, long score,
            GPGSSuccessCallback cb, int userdata);

        [DllImport("__Internal")]
        private static extern void GPGSUpdateState(int slot, byte[] data, int dataSize,
            GPGSUpdateStateCallback updateCb, GPGSStateConflictCallback conflictCb);

        [DllImport("__Internal")]
        private static extern void GPGSLoadState(int slot, GPGSLoadStateCallback loadCb,
            GPGSStateConflictCallback conflictCb);
            
		[DllImport("__Internal")]
		private static extern void GPGSRegisterInviteDelegate(GPGSPushNotificationCallback notificationCb);

        // Default capacity for stringbuilder buffers
        private const int DefaultStringBufferCapacity = 256;

        private IRealTimeMultiplayerClient mRtmpClient;

        private ITurnBasedMultiplayerClient mTbmpClient;


		private static InvitationReceivedDelegate sInvitationDelegate;

        // Pending callbacks from such calls as UnlockAchievement, etc. These are keyed
        // by an identifier that we send to the low-level C API, which is then reported
        // back on the callback (the userdata parameter of most calls)
        static Dictionary<int, System.Action<bool>> mCallbackDict = new Dictionary<int, System.Action<bool>>();
        static int mNextCallbackId = 0;

        bool mAuthenticated = false;
		bool mRegisteredInvitationListener = false;

        System.Action<bool> mAuthCallback = null;
        static IOSClient sInstance = null;

        // maps slot# to OnStateLoadedListener (each slot can have a different listener,
        // because the caller may want to load several slots concurrently).
        Dictionary<int,OnStateLoadedListener> mStateLoadedListener =
                new Dictionary<int, OnStateLoadedListener>();

        // local files where we store the cloud cache (we implement this on the plugin side
        // because in iOS the library does not support offline loading of cloud data. So we
        // fake that using files.
        const string CloudCacheDir = "/gpgscc";
        string CloudCacheFileFmt = CloudCacheDir + "/{0}.dat";

        // cloud cache encrypter
        BufferEncrypter mCloudEncrypter = null;

        public IOSClient() {
            mCloudEncrypter = DummyEncrypter;
            if (Logger.DebugLogEnabled) {
                Logger.d("Note: debug logs enabled on IOSClient.");
                GPGSEnableDebugLog(true);
            }
            if (sInstance != null) {
                Logger.e("Second instance of IOSClient created. This is not supposed to happen " +
                    "and will likely break things.");
            }
            sInstance = this;

            mRtmpClient = new IOSRtmpClient();
            mTbmpClient = new IOSTbmpClient();
        }

        public void Authenticate(System.Action<bool> callback, bool silent) {
            Logger.d("IOSClient.Authenticate");
            mAuthCallback = callback;
            // Authenticate currently returns void, so there's nothing
            // we can do with this value right now beyond log it
            bool tryingSilentSignIn = GPGSAuthenticateWithCallback(AuthCallback, silent);
            Logger.d("Trying silent signin = " + tryingSilentSignIn);
        }
        
        public void SignOut() {
            Logger.d("IOSClient.SignOut");
            GPGSSignOut();
            mAuthenticated = false;
        }

        // Due to limitations of Mono on iOS, callbacks invoked from C have to be static,
        // and have to have this MonoPInvokeCallback annotation.
        [MonoPInvokeCallback(typeof(GPGSSuccessCallback))]
        private static void AuthCallback(bool success, int userdata) {
            Logger.d("IOSClient.AuthCallback, success=" + success);
            sInstance.mAuthenticated = success;
            if (sInstance.mAuthCallback != null) {
                sInstance.mAuthCallback.Invoke(success);
            }
        }

        public bool IsAuthenticated () {
            return mAuthenticated;
        }

        public string GetUserId () {
            Logger.d("IOSClient.GetUserId");
            StringBuilder sb = new StringBuilder(512);
            GPGSGetPlayerId(sb, sb.Capacity);
            Logger.d("Returned user id: " + sb.ToString());
            return sb.ToString();
        }

        public string GetUserDisplayName () {
            Logger.d("IOSClient.GetUserDisplayName");
            StringBuilder sb = new StringBuilder(512);
            GPGSGetPlayerName(sb, sb.Capacity);
            Logger.d("Returned user display name: " + sb.ToString());
            return sb.ToString();
        }

        public Achievement GetAchievement (string achId) {
            Logger.d("IOSClient.GetAchievement " + achId);
            Achievement a = new Achievement();
            a.Id = achId;
            a.Name = a.Description = "";

            bool isRevealed = false, isUnlocked = false, isIncremental = false;
            int curSteps = 0, totalSteps = 0;

            GPGSQueryAchievement(achId, ref isIncremental, ref isRevealed,
                ref isUnlocked, ref curSteps, ref totalSteps);

            a.IsIncremental = isIncremental;
            a.IsRevealed = isRevealed;
            a.IsUnlocked = isUnlocked;
            a.CurrentSteps = curSteps;
            a.TotalSteps = totalSteps;
            return a;
        }

        public void UnlockAchievement(string achId, System.Action<bool> callback) {
            Logger.d("IOSClient.UnlockAchievement, achId=" + achId);

            int key = 0;
            GPGSSuccessCallback cb = null;
            if (callback != null) {
                key = RegisterSuccessCallback("UnlockAchievement", callback);
                cb = ApiCallSuccessCallback;
            }
            GPGSUnlockAchievement(achId, cb, key);
        }

        public void RevealAchievement (string achId, System.Action<bool> callback) {
            Logger.d("IOSClient.RevealAchievement, achId=" + achId);

            int key = 0;
            GPGSSuccessCallback cb = null;
            if (callback != null) {
                key = RegisterSuccessCallback("RevealAchievement", callback);
                cb = ApiCallSuccessCallback;
            }
            GPGSRevealAchievement(achId, cb, key);
        }

        public void IncrementAchievement (string achId, int steps, System.Action<bool> callback) {
            Logger.d("IOSClient.IncrementAchievement, achId=" + achId + ", steps=" + steps);

            int key = 0;
            GPGSSuccessCallback cb = null;
            if (callback != null) {
                key = RegisterSuccessCallback("IncrementAchievement", callback);
                cb = ApiCallSuccessCallback;
            }
            GPGSIncrementAchievement(achId, steps, cb, key);
        }

        public void ShowAchievementsUI () {
            Logger.d("IOSClient.ShowAchievementsUI");
            GPGSShowAchievementsUI();
        }

        public void ShowLeaderboardUI (string lbId) {
            Logger.d("IOSClient.ShowLeaderboardsUI");
            GPGSShowLeaderboardsUI(lbId);
        }

        public void SubmitScore (string lbId, long score, System.Action<bool> callback) {
            Logger.d(string.Format("IOSClient.SubmitScore lbId={0}, score={1}, cb={2}",
                lbId, score, (callback != null ? "non-null" : "null")));

            int key = 0;
            GPGSSuccessCallback cb = null;
            if (callback != null) {
                key = RegisterSuccessCallback("SubmitScore", callback);
                cb = ApiCallSuccessCallback;
            }
            GPGSSubmitScore(lbId, score, cb, key);
        }

        private static OnStateLoadedListener GetListener(int slot) {
            if (sInstance.mStateLoadedListener.ContainsKey(slot)) {
                return sInstance.mStateLoadedListener[slot];
            } else {
                return null;
            }
        }

        // Due to limitations of Mono on iOS, callbacks invoked from C have to be static,
        // and have to have this MonoPInvokeCallback annotation.
        [MonoPInvokeCallback(typeof(GPGSUpdateStateCallback))]
        private static void UpdateStateCallback(bool success, int slot) {
            OnStateLoadedListener listener = GetListener(slot);
            Logger.d("IOSClient.UpdateStateCallback, slot=" + slot + ", success=" + success);
            if (null != listener) {
                Logger.d("IOSClient.UpdateStateCallback calling OnSLL.OnStateSaved");
                listener.OnStateSaved(success, slot);
            } else {
                Logger.d("IOSClient.UpdateStateCallback: no OnSLL to call!");
            }
        }

        // Due to limitations of Mono on iOS, callbacks invoked from C have to be static,
        // and have to have this MonoPInvokeCallback annotation.
        [MonoPInvokeCallback(typeof(GPGSSuccessCallback))]
        private static void ApiCallSuccessCallback(bool success, int key) {
            Logger.d("IOSClient.ApiCallSuccessCallback success=" + success + ", key=" + key);
            CallRegisteredSuccessCallback(key, success);
        }


        // Due to limitations of Mono on iOS, callbacks invoked from C have to be static,
        // and have to have this MonoPInvokeCallback annotation.
        [MonoPInvokeCallback(typeof(GPGSStateConflictCallback))]
        private static int StateConflictCallback(int slot,
                IntPtr localBufPtr, int localDataSize, IntPtr serverBufPtr, int serverDataSize,
                IntPtr resolvBufPtr, int resolvBufCap) {
            string prefix = "IOSClient.StateConflictCallback ";
            Logger.d(prefix + "slot " + slot + " " +
                "localbuf " + localBufPtr + " (" + localDataSize + " bytes); " +
                "serverbuf " + serverBufPtr + " (" + serverDataSize + " bytes); " +
                "resolvbuf " + resolvBufPtr + " (capacity " + resolvBufCap + ")");

            byte[] localData = null;
            byte[] serverData = null;
            if (localBufPtr.ToInt32() != 0) {
                localData = new byte[localDataSize];
                Marshal.Copy(localBufPtr, localData, 0, localDataSize);
            }
            if (serverBufPtr.ToInt32() != 0) {
                serverData = new byte[serverDataSize];
                Marshal.Copy(serverBufPtr, serverData, 0, serverDataSize);
            }

            OnStateLoadedListener listener = GetListener(slot);

            byte[] resolvData = null;

            if (OurUtils.Misc.BuffersAreIdentical(localData, serverData)) {
                Logger.d(prefix + "Bypassing fake conflict " +
                    "(local data is IDENTICAL to server data).");
                resolvData = localData != null ? localData : serverData;
            } else if (listener != null) {
                Logger.d(prefix + "calling OnSLL.OnStateConflict.");
                resolvData = listener.OnStateConflict(slot, localData, serverData);
                Logger.d(prefix +" resolvData has " + resolvData.Length + " bytes");
                if (resolvData.Length > resolvBufCap) {
                    Logger.e("Resolved data length is " + resolvData.Length + " which exceeds " +
                        "resolved buffer capacity " + resolvBufCap);
                    resolvData = null;
                }
            } else {
                Logger.d(prefix + "no listener, so choosing local data.");
            }

            if (resolvData == null) {
                // fallback
                Logger.w("Using fallback strategy due to unexpected resolution.");
                resolvData = localData != null ? localData : serverData;
                if (resolvData == null) {
                    Logger.w("ERROR: unexpected cloud conflict where all data sets are null. " +
                        "Fixing by storing byte[1] { 0 }.");
                    resolvData = new byte[1] { 0 };
                }
            }

            int len = resolvBufCap < resolvData.Length ? resolvBufCap : resolvData.Length;
            Logger.d(prefix + "outputting " + len + " bytes to resolved buffer.");
            Marshal.Copy(resolvData, 0, resolvBufPtr, len);
            Logger.d(prefix + "finishing up.");
            return len;
        }

        // Due to limitations of Mono on iOS, callbacks invoked from C have to be static,
        // and have to have this MonoPInvokeCallback annotation.
        [MonoPInvokeCallback(typeof(GPGSLoadStateCallback))]
        private static void LoadStateCallback(bool success, int slot,
                IntPtr dataBuf, int dataSize) {
            string prefix = "IOSClient.LoadStateCallback ";
            Logger.d(prefix + " success="+ success + ", slot=" + slot +
                " dataBuf=" + dataBuf + " dataSize=" + dataSize);
            byte[] data = null;

            if (success) {
                if (dataBuf.ToInt32() != 0) {
                    data = new byte[dataSize];
                    Marshal.Copy(dataBuf, data, 0, dataSize);
                    sInstance.SaveCloudCacheFile(slot, data);
                }
            } else {
                // Cloud load failed... but don't despair! Do we have the data in local cache?
                Logger.d(prefix + "Cloud load failed. Trying to load local cache.");
                byte[] localCache = sInstance.LoadCloudCacheFile(slot);
                if (localCache != null) {
                    // no local cache, so we have to report a failure.
                    Logger.d(prefix + "Local cache not present. Reporting failure.");
                } else {
                    // we have data in local cache, so report success instead of failure,
                    // and return that data.
                    Logger.d(prefix + "Loaded from local cache, so reporting success.");
                    data = localCache;
                    success = true;
                }
            }

            OnStateLoadedListener callback = GetListener(slot);
            if (callback != null) {
                Logger.d(prefix + "calling OnStateLoadedListener with " +
                    (success ? "success" : "failure"));
                callback.OnStateLoaded(success, slot, data);
            }
        }

        public void SetCloudCacheEncrypter(BufferEncrypter encrypter) {
            Logger.d("IOSClient: cloud cache encrypter is now set.");
            mCloudEncrypter = encrypter;
        }

        public void LoadState(int slot, OnStateLoadedListener listener) {
            Logger.d("IOSClient.LoadState slot=" + slot);
            mStateLoadedListener[slot] = listener;
            GPGSLoadState(slot, LoadStateCallback, StateConflictCallback);
        }

        public void UpdateState(int slot, byte[] data, OnStateLoadedListener listener) {
            Logger.d("IOSClient.UpdateState slot " + slot + ", " + data.Length + " bytes");
            mStateLoadedListener[slot] = listener;
            SaveCloudCacheFile(slot, data);
            GPGSUpdateState(slot, data, data.Length, UpdateStateCallback, StateConflictCallback);
        }

        private void CreateCloudCacheDirIfNeeded() {
            string root = Application.persistentDataPath;
            if (!System.IO.Directory.Exists(root + CloudCacheDir)) {
                Logger.d("IOSClient: creating cloud cache dir: " + root + CloudCacheDir);
                System.IO.Directory.CreateDirectory(root + CloudCacheDir);
            }
        }

        private byte[] LoadCloudCacheFile(int slot) {
            string root = Application.persistentDataPath;
            string file = root + string.Format(CloudCacheFileFmt, slot);
            string pref = "IOSClient.LoadCloudCacheFile: ";
            Logger.d(pref + "Looking for local cloud cache for slot " + slot + " in " + file);
            if (System.IO.File.Exists(file)) {
                Logger.d(pref + "Loading " + file);
                byte[] encrypted = File.ReadAllBytes(file);
                Logger.d(pref + "Slot " + slot + " loaded successfully, " +
                    encrypted.Length + " bytes");
                byte[] decrypted = mCloudEncrypter(false, encrypted);
                Logger.d(pref + "Decrypted data: " + decrypted.Length + " bytes");
                return decrypted;
            } else {
                Logger.d("IOSClient: Local cloud cache for slot " + slot + " not found.");
                return null;
            }
        }

        private void SaveCloudCacheFile(int slot, byte[] data) {
            string root = Application.persistentDataPath;
            string file = root + string.Format(CloudCacheFileFmt, slot);
            string pref = "IOSClient.SaveCloudCacheFile: ";
            Logger.d(pref + "Saving local cloud cache for slot " + slot + " to " + file);
            CreateCloudCacheDirIfNeeded();
            Logger.d(pref + "Original data: " + data.Length + " bytes");
            byte[] encrypted = mCloudEncrypter(true, data);
            Logger.d(pref + "Encrypted data: " + encrypted.Length + " bytes");
            File.WriteAllBytes(file, encrypted);
            Logger.d(pref + "Slot " + slot + " successfully written.");
        }

        private byte[] DummyEncrypter(bool encrypt, byte[] data) {
            return data;
        }

        /**
         * 
         * Multiplayer Methods
         * 
         */

        public IRealTimeMultiplayerClient GetRtmpClient() {
            return mRtmpClient;
        }

        public ITurnBasedMultiplayerClient GetTbmpClient() {
            return mTbmpClient;
        }

		// Due to limitations of Mono on iOS, callbacks invoked from C have to be static,
		// and have to have this MonoPInvokeCallback annotation.
		[MonoPInvokeCallback(typeof(GPGSPushNotificationCallback))]
		private static void PushNotificationCallback(bool isRealTime,
		                                             string invitationId,
		                                             string inviterId, 
		                                             string inviterName, 
		                                             int variant)
		{
			Logger.d("IOSClient.PushotificationCallback isRealTime=" + isRealTime + ", invitationId=" + invitationId 
			         + " inviterId=" + inviterId + " inviterName= " + inviterName + " variant=" + variant);

			Invitation.InvType invitationType = (isRealTime) ? Invitation.InvType.RealTime : Invitation.InvType.TurnBased;
			Participant inviter = new Participant (inviterName, inviterId, Participant.ParticipantStatus.Unknown, null, false);
			Invitation incomingInvite = new Invitation (invitationType, invitationId, inviter, variant);
			// For now, we're going to always going to make this false.
			sInvitationDelegate.Invoke (incomingInvite, false);
		}

		private void RegisterInvitationListener() {
			// Tell my iOS application to reuqest push notifications
			GPGSRegisterInviteDelegate(PushNotificationCallback);
			mRegisteredInvitationListener = true;

		}


        // Register an invitation delegate for RTMP/TBMP invitations
        public void RegisterInvitationDelegate(InvitationReceivedDelegate deleg) {
			Logger.d("iOSClient.RegisterInvitationDelegate");
			if (deleg == null) {
				Logger.w("iOSClient.RegisterInvitationDelegate called w/ null argument.");
				return;
			}
			sInvitationDelegate = deleg;
			
			// install invitation listener, if we don't have one yet
			if (!mRegisteredInvitationListener) {
				Logger.d("Registering an invitation listener.");
				RegisterInvitationListener();
			}

			// I don't think I need to deal with pending notifications, as the iOS library
            // caches them until the user has signed in
        }




        private static int RegisterSuccessCallback(string methodName, System.Action<bool> callback) {
            int key = mNextCallbackId++;
            mCallbackDict[key] = callback;
            Logger.d("Success callback registered for method " + methodName + ", key " + key);
            return key;
        }

        private static void CallRegisteredSuccessCallback(int key, bool success) {
            if (mCallbackDict.ContainsKey(key)) {
                System.Action<bool> cb = mCallbackDict[key];
                if (cb != null) {
                    mCallbackDict.Remove(key);
                    cb.Invoke(success);
                }
            }
        }
    }
}

#endif
