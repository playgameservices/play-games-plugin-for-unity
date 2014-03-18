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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.Android {
    internal class AndroidTbmpClient : ITurnBasedMultiplayerClient {
        AndroidClient mClient = null;
        int mMaxMatchDataSize = 0;
        
        // the match we got from the notification
        TurnBasedMatch mMatchFromNotification = null;
        
        // the match delegate we invoke when we get a match from the notification
        MatchDelegate mMatchDelegate = null;
        
        internal AndroidTbmpClient(AndroidClient client) {
            mClient = client;
        }
        
        // called on UI thread
        public void OnSignInSucceeded() {
            Logger.d("AndroidTbmpClient.OnSignInSucceeded");
            Logger.d("Querying for max match data size...");
            mMaxMatchDataSize = mClient.GHManager.CallGmsApi<int>("games.Games",
                    "TurnBasedMultiplayer", "getMaxMatchDataSize");
            Logger.d("Max match data size: " + mMaxMatchDataSize);
        }
        
        // called on game thread
        public void CreateQuickMatch(int minOpponents, int maxOpponents, int variant, 
                    Action<bool, TurnBasedMatch> callback) {
            Logger.d(string.Format("AndroidTbmpClient.CreateQuickMatch, opponents {0}-{1}, var {2}",
                    minOpponents, maxOpponents, variant));

            mClient.CallClientApi("tbmp create quick game", () => {
                ResultProxy proxy = new ResultProxy(this, "createMatch");
                proxy.SetMatchCallback(callback);
                AndroidJavaClass tbmpUtil = JavaUtil.GetClass(JavaConsts.SupportTbmpUtilsClass);
                using (AndroidJavaObject pendingResult = tbmpUtil.CallStatic<AndroidJavaObject>(
                        "createQuickMatch", mClient.GHManager.GetApiClient(), 
                        minOpponents, maxOpponents, variant)) {
                    pendingResult.Call("setResultCallback", proxy);
                }
            }, (bool success) => {
                if (!success) {
                    Logger.w("Failed to create tbmp quick match: client disconnected.");
                    if (callback != null) {
                        callback.Invoke(false, null);
                    }
                }
            });            
        }

        // called on game thread
        public void CreateWithInvitationScreen(int minOpponents, int maxOpponents, 
                    int variant, Action<bool, TurnBasedMatch> callback) {
            Logger.d(string.Format("AndroidTbmpClient.CreateWithInvitationScreen, " + 
                    "opponents {0}-{1}, variant {2}", minOpponents, maxOpponents,
                    variant));
            
            mClient.CallClientApi("tbmp launch invitation screen", () => {
                AndroidJavaClass klass = JavaUtil.GetClass(
                    JavaConsts.SupportSelectOpponentsHelperActivity);
                klass.CallStatic("launch", false, mClient.GetActivity(),
                                 new SelectOpponentsProxy(this, callback, variant),
                                 Logger.DebugLogEnabled,
                                 minOpponents, maxOpponents);
            }, (bool success) => {
                if (!success) {
                    Logger.w("Failed to create tbmp w/ invite screen: client disconnected.");
                    if (callback != null) {
                        callback.Invoke(false, null);
                    }
                }
            });
        }

        // called on game thread
        public void AcceptFromInbox(Action<bool, TurnBasedMatch> callback) {
            Logger.d(string.Format("AndroidTbmpClient.AcceptFromInbox"));
            
            mClient.CallClientApi("tbmp launch inbox", () => {
                AndroidJavaClass klass = JavaUtil.GetClass(
                    JavaConsts.SupportInvitationInboxHelperActivity);
                klass.CallStatic("launch", false, mClient.GetActivity(),
                    new InvitationInboxProxy(this, callback),
                    Logger.DebugLogEnabled);
            }, (bool success) => {
                if (!success) {
                    Logger.w("Failed to accept tbmp w/ inbox: client disconnected.");
                    if (callback != null) {
                        callback.Invoke(false, null);
                    }
                }
            });
        }

        // called on game thread
        public void AcceptInvitation(string invitationId, Action<bool, TurnBasedMatch> callback) {
            Logger.d("AndroidTbmpClient.AcceptInvitation invitationId=" + invitationId);
            TbmpApiCall("accept invitation", "acceptInvitation", null, callback, invitationId);
        }
        
        // called on game thread
        public void DeclineInvitation(string invitationId) {
            Logger.d("AndroidTbmpClient.DeclineInvitation, invitationId=" + invitationId);
            TbmpApiCall("decline invitation", "declineInvitation", null, null, invitationId);
        }
        
        // called from game thread 
        private void TbmpApiCall(string simpleDesc, string methodName,
                Action<bool> callback, Action<bool, TurnBasedMatch> tbmpCallback,
                params object[] args) {
            mClient.CallClientApi(simpleDesc, () => {
                ResultProxy proxy = new ResultProxy(this, methodName);
                if (callback != null) {
                    proxy.SetSuccessCallback(callback);
                }
                if (tbmpCallback != null) {
                    proxy.SetMatchCallback(tbmpCallback);
                }
                mClient.GHManager.CallGmsApiWithResult("games.Games", "TurnBasedMultiplayer",
                                                       methodName, proxy, args);
            }, (bool success) => {
                if (!success) {
                    Logger.w("Failed to " + simpleDesc + ": client disconnected.");
                    if (callback != null) {
                        callback.Invoke(false);
                    }
                }
            });
        }

        // called on game thread
        public void TakeTurn(string matchId, byte[] data, string pendingParticipantId, 
                    Action<bool> callback) {

            Logger.d(string.Format("AndroidTbmpClient.TakeTurn matchId={0}, data={1}, " + 
                    "pending={2}", matchId,
                    (data == null ? "(null)" : "[" + data.Length + "bytes]"),
                    pendingParticipantId));
                    
            TbmpApiCall("tbmp take turn", "takeTurn", callback, null, 
                    matchId, data, pendingParticipantId);
        }

        // called on game thread
        public int GetMaxMatchDataSize() {
            return mMaxMatchDataSize;
        }

        public void Finish(string matchId, byte[] data, MatchOutcome outcome, Action<bool> callback) {
            Logger.d(string.Format("AndroidTbmpClient.Finish matchId={0}, data={1} outcome={2}",
                    matchId, data == null ? "(null)" : data.Length + " bytes", outcome));
                        
            Logger.d("Preparing list of participant results as Android ArrayList.");
            AndroidJavaObject participantResults = new AndroidJavaObject("java.util.ArrayList");
            if (outcome != null) {
                foreach (string pid in outcome.ParticipantIds) {
                    Logger.d("Converting participant result to Android object: " + pid);
                    AndroidJavaObject thisParticipantResult = new AndroidJavaObject(
                        JavaConsts.ParticipantResultClass, pid,
                        JavaUtil.GetAndroidParticipantResult(outcome.GetResultFor(pid)),
                        outcome.GetPlacementFor(pid));
                    
                    // (yes, the return type of ArrayList.add is bool, strangely)
                    Logger.d("Adding participant result to Android ArrayList.");
                    participantResults.Call<bool>("add", thisParticipantResult);
                    thisParticipantResult.Dispose();
                }
            }
            
            TbmpApiCall("tbmp finish w/ outcome", "finishMatch", callback, null, 
                    matchId, data, participantResults);
        }

        public void AcknowledgeFinished(string matchId, Action<bool> callback) {
            Logger.d("AndroidTbmpClient.AcknowledgeFinished, matchId=" + matchId);            
            TbmpApiCall("tbmp ack finish", "finishMatch", callback, null, matchId);
        }

        public void Leave(string matchId, Action<bool> callback) {
            Logger.d("AndroidTbmpClient.Leave, matchId=" + matchId);
            TbmpApiCall("tbmp leave", "leaveMatch", callback, null, matchId); 
        }

        public void LeaveDuringTurn(string matchId, string pendingParticipantId, 
                    Action<bool> callback) {
            Logger.d("AndroidTbmpClient.LeaveDuringTurn, matchId=" + matchId + ", pending=" + 
                    pendingParticipantId);
            TbmpApiCall("tbmp leave during turn", "leaveMatchDuringTurn", callback, null, matchId, 
                    pendingParticipantId);
        }

        public void Cancel(string matchId, Action<bool> callback) {
            Logger.d("AndroidTbmpClient.Cancel, matchId=" + matchId);
            TbmpApiCall("tbmp cancel", "cancelMatch", callback, null, matchId);
        }

        public void Rematch(string matchId, Action<bool, TurnBasedMatch> callback) {
            Logger.d("AndroidTbmpClient.Rematch, matchId=" + matchId);
            TbmpApiCall("tbmp rematch", "rematch", null, callback, matchId);
        }
        
        public void RegisterMatchDelegate(MatchDelegate deleg) {
            Logger.d("AndroidTbmpClient.RegisterMatchDelegate");
            if (deleg == null) {
                Logger.w("Can't register a null match delegate.");
                return;
            }
            
            mMatchDelegate = deleg;
            
            // if we have a pending match to deliver, deliver it now
            if (mMatchFromNotification != null) {
                Logger.d("Delivering pending match to the newly registered delegate.");
                TurnBasedMatch match = mMatchFromNotification;
                mMatchFromNotification = null;
                PlayGamesHelperObject.RunOnGameThread(() => {
                    deleg.Invoke(match, true);
                });                
            }
        }
        
        private void OnSelectOpponentsResult(bool success, AndroidJavaObject opponents,
                                             bool hasAutoMatch, AndroidJavaObject autoMatchCriteria,
                                             Action<bool, TurnBasedMatch> callback, int variant) {
            Logger.d("AndroidTbmpClient.OnSelectOpponentsResult, success=" + success +
                     ", hasAutoMatch=" + hasAutoMatch);

            if (!success) {
                Logger.w("Tbmp select opponents dialog terminated with failure.");
                if (callback != null) {
                    Logger.d("Reporting select-opponents dialog failure to callback.");
                    PlayGamesHelperObject.RunOnGameThread(() => {
                        callback.Invoke(false, null);
                    });
                }
                return;
            }
            
            Logger.d("Creating TBMP match from opponents received from dialog.");
             
            mClient.CallClientApi("create match w/ opponents from dialog", () => {
                ResultProxy proxy = new ResultProxy(this, "createMatch");
                proxy.SetMatchCallback(callback);
                AndroidJavaClass tbmpUtil = JavaUtil.GetClass(JavaConsts.SupportTbmpUtilsClass);
                using (AndroidJavaObject pendingResult = tbmpUtil.CallStatic<AndroidJavaObject>(
                       "create", mClient.GHManager.GetApiClient(),
                        opponents, variant, hasAutoMatch ? autoMatchCriteria : null)) {
                    
                    pendingResult.Call("setResultCallback", proxy);
                }
            }, (bool ok) => {
                if (!ok) {
                    Logger.w("Failed to create match w/ opponents from dialog: client disconnected.");
                    if (callback != null) {
                        callback.Invoke(false, null);
                    }
                }
            });
        }
        
        private void OnInvitationInboxResult(bool success, string invitationId, 
                Action<bool, TurnBasedMatch> callback) {
            Logger.d("AndroidTbmpClient.OnInvitationInboxResult, success=" + success + ", " + 
                    "invitationId=" + invitationId);
            
            if (!success) {
                Logger.w("Tbmp invitation inbox returned failure result.");
                if (callback != null) {
                    Logger.d("Reporting tbmp invitation inbox failure to callback.");
                    PlayGamesHelperObject.RunOnGameThread(() => {
                        callback.Invoke(false, null);
                    });
                }
                return;
            }
            
            Logger.d("Accepting invite received from inbox: " + invitationId);
            TbmpApiCall("accept invite returned from inbox", "acceptInvitation", 
                    null, callback, invitationId);
        }
        
        private void OnInvitationInboxTurnBasedMatch(AndroidJavaObject matchObj,
                Action<bool, TurnBasedMatch> callback) {
            Logger.d("AndroidTbmpClient.OnInvitationTurnBasedMatch");
            
            Logger.d("Converting received match to our format...");
            TurnBasedMatch match = JavaUtil.ConvertMatch(mClient.PlayerId, matchObj);
            Logger.d("Resulting match: " + match);
            
            if (callback != null) {
                Logger.d("Invoking match callback w/ success.");
                PlayGamesHelperObject.RunOnGameThread(() => {
                    callback.Invoke(true, match);
                });
            }
        }
        
        internal void HandleMatchFromNotification(TurnBasedMatch match) {
            Logger.d("AndroidTbmpClient.HandleMatchFromNotification");
            Logger.d("Got match from notification: " + match);
            
            if (mMatchDelegate != null) {
                Logger.d("Delivering match directly to match delegate.");
                MatchDelegate del = mMatchDelegate;
                PlayGamesHelperObject.RunOnGameThread(() => {
                    del.Invoke(match, true);
                });
            } else {
                Logger.d("Since we have no match delegate, holding on to the match until we have one.");
                mMatchFromNotification = match;
            }
        }
        
        private class ResultProxy : AndroidJavaProxy {
            private AndroidTbmpClient mOwner;
            private string mMethod = "?";
            private Action<bool> mSuccessCallback = null;
            private Action<bool, TurnBasedMatch> mMatchCallback = null;
            private List<int> mSuccessCodes = new List<int>();
                        
            internal ResultProxy(AndroidTbmpClient owner, string method) 
                        : base(JavaConsts.ResultCallbackClass) {
                mOwner = owner;
                mSuccessCodes.Add(JavaConsts.STATUS_OK);
                mSuccessCodes.Add(JavaConsts.STATUS_DEFERRED);
                mSuccessCodes.Add(JavaConsts.STATUS_STALE_DATA);
                mMethod = method;
            }
            
            public void SetSuccessCallback(Action<bool> callback) {
                mSuccessCallback = callback;
            }
            
            public void SetMatchCallback(Action<bool, TurnBasedMatch> callback) {
                mMatchCallback = callback;
            }
            
            public void AddSuccessCodes(params int[] codes) {
                foreach (int code in codes) {
                    mSuccessCodes.Add(code);
                }
            }
            
            public void onResult(AndroidJavaObject result) {
                Logger.d("ResultProxy got result for method: " + mMethod);
                int statusCode = JavaUtil.GetStatusCode(result);
                bool isSuccess = mSuccessCodes.Contains(statusCode);
                TurnBasedMatch match = null;
                
                if (isSuccess) {
                    Logger.d("SUCCESS result from method " + mMethod + ": " + statusCode);
                    if (mMatchCallback != null) {
                        Logger.d("Attempting to get match from result of " + mMethod);
                        AndroidJavaObject matchObj = JavaUtil.CallNullSafeObjectMethod(result, "getMatch");
                        if (matchObj != null) {
                            Logger.d("Successfully got match from result of " + mMethod);
                            match = JavaUtil.ConvertMatch(mOwner.mClient.PlayerId, matchObj);
                            matchObj.Dispose();
                        } else {
                            Logger.w("Got a NULL match from result of " + mMethod);
                        }
                    }
                } else {
                    Logger.w("ERROR result from " + mMethod + ": " + statusCode);
                }
                
                if (mSuccessCallback != null) {
                    Logger.d("Invoking success callback (success=" + isSuccess + ") for " + 
                            "result of method " + mMethod);
                    PlayGamesHelperObject.RunOnGameThread(() => {
                        mSuccessCallback.Invoke(isSuccess);
                    });
                }
                if (mMatchCallback != null) {
                    Logger.d("Invoking match callback for result of method " + mMethod + ": " + 
                        "(success=" + isSuccess + ", match=" +
                        (match == null ? "(null)" : match.ToString()));
                    PlayGamesHelperObject.RunOnGameThread(() => {
                        mMatchCallback.Invoke(isSuccess, match);
                    });
                }
            }
        }
        
        private class SelectOpponentsProxy : AndroidJavaProxy {
            AndroidTbmpClient mOwner;
            Action<bool, TurnBasedMatch> mCallback;
            int mVariant;
            
            internal SelectOpponentsProxy(AndroidTbmpClient owner, 
                    Action<bool, TurnBasedMatch> callback, int variant) :
                    base(JavaConsts.SupportSelectOpponentsHelperActivityListener) {
                mOwner = owner;
                mCallback = callback;
                mVariant = variant;
            }
            
            public void onSelectOpponentsResult(bool success, AndroidJavaObject opponents,
                        bool hasAutoMatch, AndroidJavaObject autoMatchCriteria) {
                mOwner.OnSelectOpponentsResult(success, opponents, hasAutoMatch, autoMatchCriteria,
                        mCallback, mVariant);
            }
        }
        
        private class InvitationInboxProxy : AndroidJavaProxy {
            AndroidTbmpClient mOwner;
            Action<bool, TurnBasedMatch> mCallback;
            
            internal InvitationInboxProxy(AndroidTbmpClient owner, 
                    Action<bool, TurnBasedMatch> callback)
                        : base(JavaConsts.SupportInvitationInboxHelperActivityListener) {
                mOwner = owner;
                mCallback = callback;
            }
            
            public void onInvitationInboxResult(bool success, string invitationId) {
                mOwner.OnInvitationInboxResult(success, invitationId, mCallback);
            }
            
            public void onTurnBasedMatch(AndroidJavaObject match) {
                mOwner.OnInvitationInboxTurnBasedMatch(match, mCallback);
            }
        }
    }
}

#endif