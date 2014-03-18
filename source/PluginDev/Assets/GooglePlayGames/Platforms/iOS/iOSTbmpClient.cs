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
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.OurUtils;
using MiniJSON;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GooglePlayGames.IOS;

namespace GooglePlayGames.IOS {
    public class IOSTbmpClient : ITurnBasedMultiplayerClient {

        private const int MATCH_DATA_MAX = 131072;

        private delegate void GPGTurnBasedMatchGetCallback(string matchAsJson, bool isError, int callbackId);
        private delegate void GPGTurnBasedSuccessCallback( bool success, int errorCode, int callbackId);
        private delegate void GPGTurnBasedMatchYourTurnNotificationCallback(bool gameOver, string matchAsJson);


        public int GetMaxMatchDataSize ()
        {
            return MATCH_DATA_MAX;
        }

        
        // the match delegate we invoke when we get a match from the notification
        static MatchDelegate mMatchDelegate = null;


        static Dictionary<int, Action<bool, TurnBasedMatch>> sMatchGetCallbacks = new Dictionary<int, Action<bool, TurnBasedMatch>>();
        static int sCallbackGetId = 0;

        static Dictionary<int, Action<bool>> sMatchSuccessCallbacks = new Dictionary<int, Action<bool>>();
        static int sCallbackSuccessId = 0;

        // Some helper methods. Put these in a separate class?
        private static Participant.ParticipantStatus convertIntFromiOSToParticipantStatus(int iOSStatus) {
            switch (iOSStatus) {
            case -1:
                return Participant.ParticipantStatus.Unknown;
            case 0:
                return Participant.ParticipantStatus.NotInvitedYet;
            case 1:
                return Participant.ParticipantStatus.Invited;
            case 2:
                return Participant.ParticipantStatus.Joined;
            case 3:
                return Participant.ParticipantStatus.Declined;
            case 4:
                return Participant.ParticipantStatus.Left;
            case 5:
                return Participant.ParticipantStatus.Finished;
            case 6:
                return Participant.ParticipantStatus.Unresponsive;
            }
            return Participant.ParticipantStatus.Unknown;
        }              

        private static TurnBasedMatch.MatchStatus convertIntoFromiOSToMatchStatus(int iOSStatus) {
            switch (iOSStatus) {
            case 0:
                return TurnBasedMatch.MatchStatus.AutoMatching;
            case 1:
                return TurnBasedMatch.MatchStatus.Active;
            case 2:
                return TurnBasedMatch.MatchStatus.Complete;
            case 3:
                return TurnBasedMatch.MatchStatus.Cancelled;
            case 4:
                return TurnBasedMatch.MatchStatus.Expired;
            case 5:
                return TurnBasedMatch.MatchStatus.Deleted;
            }
            return TurnBasedMatch.MatchStatus.Unknown;
        }

        private static TurnBasedMatch.MatchTurnStatus convertIntoFromiOSToMatchTurnStatus(int iOSStatus) {
            switch (iOSStatus) {
            case 0:
                return TurnBasedMatch.MatchTurnStatus.Invited;
            case 1:
                return TurnBasedMatch.MatchTurnStatus.TheirTurn;
            case 2:
                return TurnBasedMatch.MatchTurnStatus.MyTurn;
            case 3:
                return TurnBasedMatch.MatchTurnStatus.Complete;
            }
            return TurnBasedMatch.MatchTurnStatus.Unknown;

        }

        private static int convertParticipantResultToiOSInt(MatchOutcome.ParticipantResult result)  {
            switch (result) {
            case MatchOutcome.ParticipantResult.Unset:
                return -1;
            case MatchOutcome.ParticipantResult.None:
                return 3;
            case MatchOutcome.ParticipantResult.Win:
                return 0;
            case MatchOutcome.ParticipantResult.Loss:
                return 1;
            case MatchOutcome.ParticipantResult.Tie:
                return 2;
            }
            // Don't know what this is. Best to leave it as None for now.
            return 3;
        }

        private static TurnBasedMatch GetMatchFromJsonString(string matchAsJson) {
            Dictionary<string, object> parsedMatch = Json.Deserialize(matchAsJson) as Dictionary<string, object>;
            
            // Let's create our participant list
            List<Participant> participantList = new List<Participant>();
            
            List<object> parsedParticipants = parsedMatch["participants"] as List<object>;
            foreach (Dictionary<string, object> nextParticipant in parsedParticipants) {
                Dictionary<string, object> playerRepresentation = nextParticipant["player"] as Dictionary<string, object>;
                Player participantAsPlayer = new Player((string)playerRepresentation["playerDisplayName"],
                                                        (string)playerRepresentation["playerId"]);
                Participant participant = new Participant((string)nextParticipant["displayName"],
                                                          (string)nextParticipant["participantId"],
                                                          convertIntFromiOSToParticipantStatus(Convert.ToInt32((System.Int64)nextParticipant["status"])),
                                                          participantAsPlayer,
                                                          (bool)nextParticipant["isConnectedToRoom"]);
                participantList.Add(participant);
                Debug.Log("Adding participant " + participant.ToString());
            }
            
            
            string matchData = (string)parsedMatch["matchData"];
            byte[] bytes;
            if (matchData == null) {
                Debug.Log ("Got a null matchData.");
                bytes = null;
            } else {
                bytes = System.Convert.FromBase64String(matchData);
            }
            
            
            TurnBasedMatch matchCreated = new TurnBasedMatch((string)parsedMatch["matchId"],
                                              bytes,
                                              (bool)parsedMatch["canRematch"],
                                              (string)parsedMatch["localParticipantId"],
                                              participantList,
                                              Convert.ToInt32((System.Int64)parsedMatch["availableAutomatchSlots"]),
                                              (string)parsedMatch["pendingParticipantId"],
                                              convertIntoFromiOSToMatchTurnStatus(Convert.ToInt32((System.Int64)parsedMatch["turnStatus"])),
                                              convertIntoFromiOSToMatchStatus(Convert.ToInt32((System.Int64)parsedMatch["matchStatus"])),
                                              Convert.ToInt32((System.Int64)parsedMatch["variant"]));
            
            Debug.Log("Final parsed match object is " + matchCreated.ToString());
            return matchCreated;

        }


        [MonoPInvokeCallback(typeof(GPGTurnBasedMatchGetCallback))]
        private static void TbmpMatchGetCallback(string matchAsJson, bool isError, int callbackid) {
            TurnBasedMatch matchCreated;
            if (isError) {
                Logger.e("Found an error creating a room.");
                matchCreated = null;
            } else {
                Logger.d("Received a giant match object! " + matchAsJson);
                matchCreated = GetMatchFromJsonString(matchAsJson);
            }
            Debug.Log("Fetching my callback for callbackid" + callbackid);
            Action<bool, TurnBasedMatch> callback = sMatchGetCallbacks[callbackid];
            sMatchGetCallbacks.Remove(callbackid);
            Debug.Log("Invoking my callback for callbackid" + callbackid);
            callback.Invoke(! isError, matchCreated); 
        }

        [MonoPInvokeCallback(typeof(GPGTurnBasedSuccessCallback))]
        private static void TbmpMatchSuccessCallback(bool isError, int userData, int callbackid) {
            Action<bool> callback = sMatchSuccessCallbacks[callbackid];
            sMatchSuccessCallbacks.Remove(callbackid);
            callback.Invoke(!isError);
        }

        [MonoPInvokeCallback(typeof(GPGTurnBasedMatchYourTurnNotificationCallback))]
        private static void TbmpYourTurnCallback(bool gameOver, string matchAsJson) {
            TurnBasedMatch matchFound = GetMatchFromJsonString(matchAsJson);
            if (gameOver) {
                mMatchDelegate(matchFound, false);
            } else {
                mMatchDelegate(matchFound, false);
            }
        }



        [DllImport("__Internal")]
        private static extern void GPGSTBMPCreateWithInviteScreen(int minOpponents, int maxOpponents, int variant, int callbackId,
                                                                  GPGTurnBasedMatchGetCallback creationCallback);

        
        [DllImport("__Internal")]
        private static extern void GPGSTBMPCreateQuickMatch(int minOpponents, int maxOpponents, int variant,int callbackId,
                                                            GPGTurnBasedMatchGetCallback creationCallback);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPTakeTurnInMatch(string matchId, byte[] data, int dataLen, string nextPlayerId, int callbackId,
                                                           GPGTurnBasedSuccessCallback callbackj);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPShowInvitesAndFindMatch(int callbackId, GPGTurnBasedMatchGetCallback callbackj);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPLeaveDuringTurn(string matchId, string pendingParticipantId, int callbackId, 
                                                           GPGTurnBasedSuccessCallback callback);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPFinishMatch(string matchId, byte[] data, int dataLen, string matchResults, int callbackId, 
                                                           GPGTurnBasedSuccessCallback callback);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPAcknowledgeFinish(string matchId, int callbackId, 
                                                       GPGTurnBasedSuccessCallback callback);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPLeaveOutofTurn(string matchId, int callbackId, 
                                                             GPGTurnBasedSuccessCallback callback);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPCancelMatch(string matchId, int callbackId, 
                                                          GPGTurnBasedSuccessCallback callback);

        [DllImport("__Internal")]
        private static extern void GPGSTBMPRemach(string matchId, int callbackId, GPGTurnBasedMatchGetCallback callback);   

        [DllImport("__Internal")]
        private static extern void GPGSTBMPAcceptMatchWithId(string invitationId, int callbackId, GPGTurnBasedMatchGetCallback callback);   

        [DllImport("__Internal")]
        private static extern void GPGSTBMPDeclineMatchWithId(string invitationId); 

        [DllImport("__Internal")]
        private static extern void GPGSTBMPRegisterYourTurnNotificationCallback(GPGTurnBasedMatchYourTurnNotificationCallback callback); 


        public void RegisterMatchDelegate (MatchDelegate deleg)
        {
            Logger.d("iOSTbmpClient.RegisterMatchDelegate");
            if (deleg == null) {
                Logger.w("Can't register a null match delegate.");
                return;
            }
            
            mMatchDelegate = deleg;

            GPGSTBMPRegisterYourTurnNotificationCallback(TbmpYourTurnCallback);
        }

        public void CreateQuickMatch (int minOpponents, int maxOpponents, int variant, Action<bool, TurnBasedMatch> callback)
        {
            sCallbackGetId++;
            sMatchGetCallbacks.Add(sCallbackGetId, callback);
            GPGSTBMPCreateQuickMatch(minOpponents, maxOpponents, variant, sCallbackGetId, TbmpMatchGetCallback);   
        }

        
        public void CreateWithInvitationScreen (int minOpponents, int maxOpponents, int variant, Action<bool, TurnBasedMatch> callback)
        {
            sCallbackGetId++;
            sMatchGetCallbacks.Add(sCallbackGetId, callback);
            GPGSTBMPCreateWithInviteScreen(minOpponents, maxOpponents, variant, sCallbackGetId, TbmpMatchGetCallback);   
        }


        
        public void AcceptInvitation (string invitationId, Action<bool, TurnBasedMatch> callback)
        {
            Logger.d("iOSTbmpClient.AcceptInvitation");
            sCallbackGetId++;
            sMatchGetCallbacks.Add(sCallbackGetId, callback);
            GPGSTBMPAcceptMatchWithId(invitationId, sCallbackGetId, TbmpMatchGetCallback);

        }
        
        public void DeclineInvitation (string invitationId)
        {
            Logger.d("iOSTbmpClient.DeclineInvitation");
            GPGSTBMPDeclineMatchWithId(invitationId);
        }

        // Show matches and let a player pick one of them
        public void AcceptFromInbox (Action<bool, TurnBasedMatch> callback)
        {
            Logger.d("iOSTbmpClient.AcceptFromInbox");
            sCallbackGetId++;
            sMatchGetCallbacks.Add(sCallbackGetId, callback);
            GPGSTBMPShowInvitesAndFindMatch(sCallbackGetId, TbmpMatchGetCallback);
        }

        public void TakeTurn (string matchId, byte[] data, string pendingParticipantId, Action<bool> callback)
        {
            Logger.d(string.Format("iOSTbmpClient.TakeTurn matchId={0}, data={1}, " + 
                                   "pending={2}", matchId,
                                   (data == null ? "(null)" : "[" + data.Length + "bytes]"),
                                   pendingParticipantId));
            sCallbackSuccessId++;
            sMatchSuccessCallbacks.Add(sCallbackSuccessId, callback);
            GPGSTBMPTakeTurnInMatch(matchId, data, data.Length, pendingParticipantId, sCallbackSuccessId, TbmpMatchSuccessCallback);
        }
        
        public void LeaveDuringTurn (string matchId, string pendingParticipantId, Action<bool> callback)
        {
            Logger.d(string.Format("iOSTbmpClient.LeaveDuringTurn matchId={0}, pendingParticipant={1}", matchId, pendingParticipantId));
            sCallbackSuccessId++;
            sMatchSuccessCallbacks.Add(sCallbackSuccessId, callback);
            GPGSTBMPLeaveDuringTurn(matchId, pendingParticipantId, sCallbackSuccessId, TbmpMatchSuccessCallback);
        }

        // Not very frequently used
        public void Leave (string matchId, Action<bool> callback)
        {
            Logger.d(string.Format("iOSTbmpClient.LeaveOutOfTurn matchId={0}", matchId));
            sCallbackSuccessId++;
            sMatchSuccessCallbacks.Add(sCallbackSuccessId, callback);
            GPGSTBMPLeaveOutofTurn(matchId, sCallbackSuccessId, TbmpMatchSuccessCallback);
        }

        public void Finish (string matchId, byte[] data, MatchOutcome outcome, Action<bool> callback)
        {
            Logger.d(string.Format("iOSTbmpClient.Finish matchId={0}, outcome={1}", matchId, outcome.ToString()));
            // We probably need to convert our match outcome into something we can read in


            var resultsOutput = new List<Dictionary<string, object>>();
            List<string> allParticipants = outcome.ParticipantIds;
            Dictionary<string, object> nextResult;
            foreach (string participantID in allParticipants) {
                Logger.d("Getting results for " + participantID);
                nextResult = new Dictionary<string, object>();
                // Maybe don't add them if their result is unset? Check with Bruno on this one
                if (outcome.GetResultFor(participantID) != MatchOutcome.ParticipantResult.Unset) {
                    nextResult["participantId"] = participantID;
                    nextResult["placing"] = outcome.GetPlacementFor(participantID);
                    nextResult["result"] = convertParticipantResultToiOSInt(outcome.GetResultFor(participantID));
                }
                resultsOutput.Add(nextResult);
            }
            string resultsAsJson = Json.Serialize(resultsOutput);
            Logger.d("JSonified results are " + resultsAsJson);

            sCallbackSuccessId++;
            sMatchSuccessCallbacks.Add(sCallbackSuccessId, callback);
            GPGSTBMPFinishMatch(matchId, data, data.Length, resultsAsJson, sCallbackSuccessId, TbmpMatchSuccessCallback);

        }

        public void Cancel (string matchId, Action<bool> callback)
        {
            Logger.d(string.Format("iOSTbmpClient.CancelMatch matchId={0}", matchId));
            sCallbackSuccessId++;
            sMatchSuccessCallbacks.Add(sCallbackSuccessId, callback);
            GPGSTBMPCancelMatch(matchId, sCallbackSuccessId, TbmpMatchSuccessCallback);
        }

        public void Rematch (string matchId, Action<bool, TurnBasedMatch> callback)
        {
            Logger.d(string.Format("iOSTbmpClient.Rematch matchId={0}", matchId));
            sCallbackGetId++;
            sMatchGetCallbacks.Add(sCallbackGetId, callback);
            GPGSTBMPRemach(matchId, sCallbackGetId, TbmpMatchGetCallback);   
        }

        public void AcknowledgeFinished (string matchId, Action<bool> callback)
        {
            sCallbackSuccessId++;
            sMatchSuccessCallbacks.Add(sCallbackSuccessId, callback);
            GPGSTBMPAcknowledgeFinish(matchId, sCallbackSuccessId, TbmpMatchSuccessCallback);
        }


    }
}

#endif

