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
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.OurUtils;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.Android {
    internal class JavaUtil {
        private static Dictionary<string, AndroidJavaClass> mClassDict = 
                new Dictionary<string, AndroidJavaClass>();
        private static Dictionary<string, AndroidJavaObject> mFieldDict = 
                new Dictionary<string, AndroidJavaObject>();
        
        public static AndroidJavaClass GetGmsClass(string className) {
            return GetClass(JavaConsts.GmsPkg + "." + className);
        }
        
        public static AndroidJavaClass GetClass(string className) {
            if (mClassDict.ContainsKey(className)) {
                return mClassDict[className];
            }
            
            try {
                AndroidJavaClass cls = new AndroidJavaClass(className);
                mClassDict[className] = cls;
                return cls;
            } catch (Exception ex) {
                Logger.e("JavaUtil failed to load Java class: " + className);
                throw ex;
            }
        }
        
        // Loads and caches a static field from a GmsCore class
        public static AndroidJavaObject GetGmsField(string className, string fieldName) {
            string key = className + "/" + fieldName;
            if (mFieldDict.ContainsKey(key)) {
                return mFieldDict[key];
            }
            
            AndroidJavaClass cls = GetGmsClass(className);
            AndroidJavaObject obj = cls.GetStatic<AndroidJavaObject>(fieldName);
            mFieldDict[key] = obj;
            return obj;
        }
        
        // Gets the status code from a Result object
        public static int GetStatusCode(AndroidJavaObject result) {
            if (result == null) {
                return -1;
            }
            AndroidJavaObject status = result.Call<AndroidJavaObject>("getStatus");
            return status.Call<int>("getStatusCode");
        }
        
        // Sadly, it appears that calling a method that returns a null Object in Java
        // will cause AndroidJavaObject to crash, so we use this ugly workaround:
        public static AndroidJavaObject CallNullSafeObjectMethod(AndroidJavaObject target, string methodName,
                params object[] args) {
            try {
                return target.Call<AndroidJavaObject>(methodName, args);
            } catch (Exception ex) {
                if (ex.Message.Contains("null")) {
                    // expected -- means method returned null
                    return null;
                } else {
                    Logger.w("CallObjectMethod exception: " + ex);
                    return null;
                }
            }
        }
        
        public static byte[] ConvertByteArray(AndroidJavaObject byteArrayObj) {
            Debug.Log("ConvertByteArray.");
            
            if (byteArrayObj == null || byteArrayObj.GetRawObject().ToInt32() == 0) {
                return null;
            }
            
            byte[] b = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(byteArrayObj.GetRawObject());
            
            return b;
        }
        
        public static int GetAndroidParticipantResult(MatchOutcome.ParticipantResult result) {
            switch (result) {
                case MatchOutcome.ParticipantResult.Win:
                    return JavaConsts.MATCH_RESULT_WIN;
                case MatchOutcome.ParticipantResult.Loss:
                    return JavaConsts.MATCH_RESULT_LOSS;
                case MatchOutcome.ParticipantResult.Tie:
                    return JavaConsts.MATCH_RESULT_TIE;
                case MatchOutcome.ParticipantResult.None:
                    return JavaConsts.MATCH_RESULT_NONE;
                default:
                    return JavaConsts.MATCH_RESULT_UNINITIALIZED;
            }
        }
        
        public static TurnBasedMatch.MatchStatus ConvertMatchStatus(int code) {
            switch (code) {
            case JavaConsts.MATCH_STATUS_ACTIVE:
                return TurnBasedMatch.MatchStatus.Active;
            case JavaConsts.MATCH_STATUS_AUTO_MATCHING:
                return TurnBasedMatch.MatchStatus.AutoMatching;
            case JavaConsts.MATCH_STATUS_CANCELED:
                return TurnBasedMatch.MatchStatus.Cancelled;
            case JavaConsts.MATCH_STATUS_COMPLETE:
                return TurnBasedMatch.MatchStatus.Complete;
            case JavaConsts.MATCH_STATUS_EXPIRED:
                return TurnBasedMatch.MatchStatus.Expired;
            default:
                Logger.e("Unknown match status code: " + code);
                return TurnBasedMatch.MatchStatus.Unknown;
            }
        }
        
        public static TurnBasedMatch.MatchTurnStatus ConvertTurnStatus(int code) {
            switch (code) {
            case JavaConsts.MATCH_TURN_STATUS_COMPLETE:
                return TurnBasedMatch.MatchTurnStatus.Complete;
            case JavaConsts.MATCH_TURN_STATUS_INVITED:
                return TurnBasedMatch.MatchTurnStatus.Invited;
            case JavaConsts.MATCH_TURN_STATUS_MY_TURN:
                return TurnBasedMatch.MatchTurnStatus.MyTurn;
            case JavaConsts.MATCH_TURN_STATUS_THEIR_TURN:
                return TurnBasedMatch.MatchTurnStatus.TheirTurn;
            default:
                Logger.e("Unknown match turn status: " + code);
                return TurnBasedMatch.MatchTurnStatus.Unknown;
            }
        }
        
        public static Participant ConvertParticipant(AndroidJavaObject participant) {
            string displayName = participant.Call<string>("getDisplayName");
            string participantId = participant.Call<string>("getParticipantId");
            Participant.ParticipantStatus status = Participant.ParticipantStatus.Unknown;
            Player player = null;
            bool connected = participant.Call<bool>("isConnectedToRoom");
            
            int statusValue = participant.Call<int>("getStatus");
            switch (statusValue) {
            case JavaConsts.STATUS_NOT_INVITED_YET:
                status = Participant.ParticipantStatus.NotInvitedYet;
                break;
            case JavaConsts.STATUS_INVITED:
                status = Participant.ParticipantStatus.Invited;
                break;
            case JavaConsts.STATUS_JOINED:
                status = Participant.ParticipantStatus.Joined;
                break;
            case JavaConsts.STATUS_DECLINED:
                status = Participant.ParticipantStatus.Declined;
                break;
            case JavaConsts.STATUS_LEFT:
                status = Participant.ParticipantStatus.Left;
                break;
            case JavaConsts.STATUS_FINISHED:
                status = Participant.ParticipantStatus.Finished;
                break;
            case JavaConsts.STATUS_UNRESPONSIVE:
                status = Participant.ParticipantStatus.Unresponsive;
                break;
            default:
                status = Participant.ParticipantStatus.Unknown;
                break;
            }
            
            AndroidJavaObject playerObj = JavaUtil.CallNullSafeObjectMethod(participant, "getPlayer");
            if (playerObj != null) {
                player = new Player(playerObj.Call<string>("getDisplayName"),
                                    playerObj.Call<string>("getPlayerId"));
                playerObj.Dispose();
                playerObj = null;
            }
            
            return new Participant(displayName, participantId, status, player, connected);
        }
        
        public static TurnBasedMatch ConvertMatch(string playerId, AndroidJavaObject matchObj) {
            List<AndroidJavaObject> toDispose = new List<AndroidJavaObject>();
            Logger.d("AndroidTbmpClient.ConvertMatch, playerId=" + playerId);
            
            string matchId;
            byte[] data;
            bool canRematch;
            int availableAutomatchSlots;
            string selfParticipantId;
            List<Participant> participants = new List<Participant>();
            string pendingParticipantId;
            TurnBasedMatch.MatchTurnStatus turnStatus;
            TurnBasedMatch.MatchStatus matchStatus;
            int variant;
            
            matchId = matchObj.Call<string>("getMatchId");
            AndroidJavaObject dataObj = JavaUtil.CallNullSafeObjectMethod(matchObj, "getData");
            toDispose.Add(dataObj);
            data = JavaUtil.ConvertByteArray(dataObj);
            canRematch = matchObj.Call<bool>("canRematch");
            availableAutomatchSlots = matchObj.Call<int>("getAvailableAutoMatchSlots");
            selfParticipantId = matchObj.Call<string>("getParticipantId", playerId);
            
            AndroidJavaObject participantIds = matchObj.Call<AndroidJavaObject>("getParticipantIds");
            toDispose.Add(participantIds);
            int participantCount = participantIds.Call<int>("size");
            
            for (int i = 0; i < participantCount; i++) {
                string thisId = participantIds.Call<string>("get", i);
                AndroidJavaObject thisPart = matchObj.Call<AndroidJavaObject>("getParticipant", thisId);
                toDispose.Add(thisPart);
                Participant p = JavaUtil.ConvertParticipant(thisPart);
                participants.Add(p);
            }
            
            pendingParticipantId = matchObj.Call<string>("getPendingParticipantId");
            turnStatus = JavaUtil.ConvertTurnStatus(matchObj.Call<int>("getTurnStatus"));
            matchStatus = JavaUtil.ConvertMatchStatus(matchObj.Call<int>("getStatus"));
            variant = matchObj.Call<int>("getVariant");
            
            // cleanup
            foreach (AndroidJavaObject obj in toDispose) {
                if (obj != null) {
                    obj.Dispose();
                }
            }
            
            // participants should be sorted by participant ID
            participants.Sort();
            
            return new TurnBasedMatch(matchId, data, canRematch, selfParticipantId,
                                      participants, availableAutomatchSlots,
                                      pendingParticipantId, turnStatus, matchStatus, variant);
        }
        
    }
}
#endif
