/*
 * Copyright (C) 2014 Google Inc.
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
#if (UNITY_ANDROID || UNITY_IPHONE)
using System;
using GooglePlayGames.BasicApi.Multiplayer;
using GooglePlayGames.Native.PInvoke;
using GooglePlayGames.OurUtils;
using System.Collections.Generic;

using Types = GooglePlayGames.Native.Cwrapper.Types;

namespace GooglePlayGames.Native {
public class NativeTurnBasedMultiplayerClient : ITurnBasedMultiplayerClient {

    private readonly TurnBasedManager mTurnBasedManager;
    private readonly NativeClient mNativeClient;
    private volatile Action<TurnBasedMatch, bool> mMatchDelegate;

    internal NativeTurnBasedMultiplayerClient(NativeClient nativeClient, TurnBasedManager manager) {
        this.mTurnBasedManager = manager;
        this.mNativeClient = nativeClient;
    }

    public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
                                 Action<bool, TurnBasedMatch> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        using (var configBuilder = TurnBasedMatchConfigBuilder.Create()) {
            configBuilder.SetVariant(variant);
            configBuilder.SetMinimumAutomatchingPlayers(minOpponents);
            configBuilder.SetMaximumAutomatchingPlayers(maxOpponents);
            using (var config = configBuilder.Build()) {
                mTurnBasedManager.CreateMatch(config, BridgeMatchToUserCallback(callback));
            }
        }
    }

    public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
        Action<bool, TurnBasedMatch> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        mTurnBasedManager.ShowPlayerSelectUI(minOpponents, maxOpponents, true, result => {
            if (result.Status() != GooglePlayGames.Native.Cwrapper.CommonErrorStatus.UIStatus.VALID) {
                callback(false, null);
            }

            using (var configBuilder = TurnBasedMatchConfigBuilder.Create()) {
                configBuilder.PopulateFromUIResponse(result);
                using (var config = configBuilder.Build()) {
                    mTurnBasedManager.CreateMatch(config, BridgeMatchToUserCallback(callback));
                }
            }
        });
    }

    private Action<TurnBasedManager.TurnBasedMatchResponse> BridgeMatchToUserCallback(
        Action<bool, TurnBasedMatch> userCallback) {
        return callbackResult => {
            using (var match = callbackResult.Match()) {
                if (match == null) {
                    userCallback(false, null);
                } else {
                    var converted = match.AsTurnBasedMatch(mNativeClient.GetUserId());
                    Logger.d("Passing converted match to user callback:" + converted);
                    userCallback(true, converted);
                }
            }
        };
    }

    public void AcceptFromInbox(Action<bool, TurnBasedMatch> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        mTurnBasedManager.ShowInboxUI(callbackResult => {
            using (var match = callbackResult.Match()) {
                if (match == null) {
                    callback(false, null);
                } else {
                    var converted =  match.AsTurnBasedMatch(mNativeClient.GetUserId());
                    Logger.d("Passing converted match to user callback:" + converted);
                    callback(true, converted);
                }
            }
        });
    }

    public void AcceptInvitation(string invitationId, Action<bool, TurnBasedMatch> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        FindInvitationWithId(invitationId,
            invitation => {
                if (invitation == null) {
                    Logger.e("Could not find invitation with id " + invitationId);
                    callback(false, null);
                    return;
                }

                mTurnBasedManager.AcceptInvitation(invitation, BridgeMatchToUserCallback(callback));
            }
        );
    }

    private void FindInvitationWithId(string invitationId, Action<MultiplayerInvitation> callback) {
        mTurnBasedManager.GetAllTurnbasedMatches(allMatches => {
            if (allMatches.Status() <= 0) {
                callback(null);
                return;
            }

            foreach (var invitation in allMatches.Invitations()) {
                using (invitation) {
                    if (invitation.Id().Equals(invitationId)) {
                        callback(invitation);
                        return;
                    }
                }
            }

            callback(null);
        });
    }

    public void RegisterMatchDelegate(MatchDelegate del) {
        if (del == null) {
            mMatchDelegate = null;
        } else {
            mMatchDelegate = Callbacks.AsOnGameThreadCallback<TurnBasedMatch, bool>(
                (match, autoLaunch) => del(match, autoLaunch));
        }
    }

    internal void HandleMatchEvent(Types.MultiplayerEvent eventType, string matchId,
        NativeTurnBasedMatch match) {
        // Capture the current value of the delegate to protect against racing updates.
        var currentDelegate = mMatchDelegate;
        if (currentDelegate == null) {
            return;
        }

        // Ignore REMOVED events - this plugin has no use for them.
        if (eventType == Types.MultiplayerEvent.REMOVED) {
            Logger.d("Ignoring REMOVE event for match " + matchId);
            return;
        }

        bool shouldAutolaunch = eventType == Types.MultiplayerEvent.UPDATED_FROM_APP_LAUNCH;

        currentDelegate(match.AsTurnBasedMatch(mNativeClient.GetUserId()), shouldAutolaunch);
    }


    public void TakeTurn(TurnBasedMatch match, byte[] data, string pendingParticipantId,
                         Action<bool> callback) {
        Logger.describe(data);
        callback = Callbacks.AsOnGameThreadCallback(callback);
        // Find the indicated match, take the turn if the match is:
        // a) Still present
        // b) Still of a matching version (i.e. there were no updates to it since the passed match
        //    was retrieved)
        // c) The indicated pending participant matches a participant in the match.
        FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback,
            (pendingParticipant, foundMatch) => {
                // If we got here, the match and the participant are in a good state.
                mTurnBasedManager.TakeTurn(foundMatch, data, pendingParticipant,
                    result => {
                        if (result.RequestSucceeded()) {
                            callback(true);
                        } else {
                            Logger.d("Taking turn failed: " + result.ResponseStatus());
                            callback(false);
                        }
                    });
            });
    }

    private void FindEqualVersionMatch(TurnBasedMatch match, Action<bool> onFailure,
        Action<NativeTurnBasedMatch> onVersionMatch) {
        mTurnBasedManager.GetMatch(match.MatchId, response => {
            using (var foundMatch = response.Match()) {
                if (foundMatch == null) {
                    Logger.e(string.Format("Could not find match {0}", match.MatchId));
                    onFailure(false);
                    return;
                }

                if (foundMatch.Version() != match.Version) {
                    Logger.e(string.Format("Attempted to update a stale version of the " +
                    "match. Expected version was {0} but current version is {1}.",
                        match.Version, foundMatch.Version()));
                    onFailure(false);
                    return;
                }

                onVersionMatch(foundMatch);
            }
        });
    }

    private void FindEqualVersionMatchWithParticipant(TurnBasedMatch match,
        string participantId, Action<bool> onFailure,
        Action<MultiplayerParticipant, NativeTurnBasedMatch> onFoundParticipantAndMatch) {
        FindEqualVersionMatch(match, onFailure, foundMatch => {
            // If we received a null participantId, we're using an automatching player instead -
            // issue the callback using that.
            if (participantId == null) {
                using (var sentinelParticipant = MultiplayerParticipant.AutomatchingSentinel()){
                    onFoundParticipantAndMatch(sentinelParticipant, foundMatch);
                    return;
                }
            }

            using (var participant = foundMatch.ParticipantWithId(participantId)) {
                if (participant == null) {
                    Logger.e(string.Format("Located match {0} but desired participant with ID " +
                        "{1} could not be found", match.MatchId, participantId));
                    onFailure(false);
                    return;
                }

                onFoundParticipantAndMatch(participant, foundMatch);
            }
        });
    }

    public int GetMaxMatchDataSize() {
        throw new NotImplementedException();
    }

    public void Finish(TurnBasedMatch match, byte[] data, MatchOutcome outcome,
                       Action<bool> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        FindEqualVersionMatch(match, callback, foundMatch => {
            ParticipantResults results = foundMatch.Results();

            foreach (string participantId in outcome.ParticipantIds) {
                Types.MatchResult matchResult =
                    ResultToMatchResult(outcome.GetResultFor(participantId));
                uint placing = outcome.GetPlacementFor(participantId);

                if (results.HasResultsForParticipant(participantId)) {
                    // If the match already has results for this participant, make sure that they're
                    // consistent with what's already there.
                    var existingResults = results.ResultsForParticipant(participantId);
                    var existingPlacing = results.PlacingForParticipant(participantId);

                    if (matchResult != existingResults || placing != existingPlacing) {
                        Logger.e(string.Format("Attempted to override existing results for " +
                        "participant {0}: Placing {1}, Result {2}",
                            participantId, existingPlacing, existingResults));
                        callback(false);
                        return;
                    }
                } else {
                    // Otherwise, get updated results and dispose of the old ones.
                    var oldResults = results;
                    results = oldResults.WithResult(participantId, placing, matchResult);
                    oldResults.Dispose();
                }
            }

            mTurnBasedManager.FinishMatchDuringMyTurn(foundMatch, data, results,
                response => callback(response.RequestSucceeded()));
        });
    }

    private static Types.MatchResult ResultToMatchResult(MatchOutcome.ParticipantResult result) {
        switch (result) {
            case MatchOutcome.ParticipantResult.Loss:
                return Types.MatchResult.LOSS;
            case MatchOutcome.ParticipantResult.None:
                return Types.MatchResult.NONE;
            case MatchOutcome.ParticipantResult.Tie:
                return Types.MatchResult.TIE;
            case MatchOutcome.ParticipantResult.Win:
                return Types.MatchResult.WIN;
            default:
                Logger.e("Received unknown ParticipantResult " + result);
                return Types.MatchResult.NONE;
        }
    }

    public void AcknowledgeFinished(TurnBasedMatch match, Action<bool> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        FindEqualVersionMatch(match, callback, foundMatch => {
            mTurnBasedManager.ConfirmPendingCompletion(
                foundMatch,  response => callback(response.RequestSucceeded()));
            });
    }

    public void Leave(TurnBasedMatch match, Action<bool> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        FindEqualVersionMatch(match, callback, foundMatch => {
            mTurnBasedManager.LeaveMatchDuringTheirTurn(foundMatch, status => callback(status > 0));
        });
    }

    public void LeaveDuringTurn(TurnBasedMatch match, string pendingParticipantId,
        Action<bool> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback,
            (pendingParticipant, foundMatch) => {
                mTurnBasedManager.LeaveDuringMyTurn(foundMatch, pendingParticipant,
                    status => callback(status > 0));
            });
    }

    public void Cancel(TurnBasedMatch match, Action<bool> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        FindEqualVersionMatch(match, callback, foundMatch => {
            mTurnBasedManager.CancelMatch(foundMatch, status => callback(status > 0));
        });
    }

    public void Rematch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback) {
        callback = Callbacks.AsOnGameThreadCallback(callback);
        FindEqualVersionMatch(match, failed => callback(false, null), foundMatch => {
            mTurnBasedManager.Rematch(foundMatch, BridgeMatchToUserCallback(callback));
        });
    }

    public void DeclineInvitation(string invitationId) {
        FindInvitationWithId(invitationId, invitation => {
            if (invitation == null) {
                return;
            }

            mTurnBasedManager.DeclineInvitation(invitation);
        });
    }
}
}
#endif
