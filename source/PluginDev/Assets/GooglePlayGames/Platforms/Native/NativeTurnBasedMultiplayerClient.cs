// <copyright file="NativeTurnBasedMultiplayerClient.cs" company="Google Inc.">
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

namespace GooglePlayGames.Native
{
    using System;
    using System.Collections;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.Native.PInvoke;
    using GooglePlayGames.OurUtils;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    public class NativeTurnBasedMultiplayerClient : ITurnBasedMultiplayerClient
    {

        private readonly TurnBasedManager mTurnBasedManager;
        private readonly NativeClient mNativeClient;
        private volatile Action<TurnBasedMatch, bool> mMatchDelegate;

        internal NativeTurnBasedMultiplayerClient(NativeClient nativeClient, TurnBasedManager manager)
        {
            this.mTurnBasedManager = manager;
            this.mNativeClient = nativeClient;
        }

        /// <summary>
        /// Starts a game with randomly selected opponent(s). No exclusivebitmask.
        /// </summary>
        /// <param name="minOpponents">Minimum number opponents, not counting the current
        /// player -- so for a 2-player game, use 1).</param>
        /// <param name="maxOpponents">Max opponents, not counting the current player.</param>
        /// <param name="variant">Variant. Use 0 for default.</param>
        /// <param name="callback">Callback. Called when match setup is complete or fails.
        /// If it succeeds, will be called with (true, match); if it fails, will be
        /// called with (false, null).</param>
        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
           Action<bool, TurnBasedMatch> callback)
        {
            CreateQuickMatch(minOpponents, maxOpponents, variant, 0L, callback);
        }

        /// <summary>
        /// Starts a game with randomly selected opponent(s) using exclusiveBitMask.
        ///  No UI will be shown.
        /// </summary>
        /// <param name="minOpponents">Minimum number opponents, not counting the current
        /// player -- so for a 2-player game, use 1).</param>
        /// <param name="maxOpponents">Max opponents, not counting the current player.</param>
        /// <param name="variant">Variant. Use 0 for default.</param>
        /// <param name="exclusiveBitmask">The bitmask used to match players. The
        /// xor operation of all the bitmasks must be 0 to match players.</param>
        /// <param name="callback">Callback. Called when match setup is complete or fails.
        /// If it succeeds, will be called with (true, match); if it fails, will be
        /// called with (false, null).</param>
        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitmask, Action<bool, TurnBasedMatch> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            using (var configBuilder = TurnBasedMatchConfigBuilder.Create())
            {
                configBuilder.SetVariant(variant)
                    .SetMinimumAutomatchingPlayers(minOpponents)
                    .SetMaximumAutomatchingPlayers(maxOpponents)
                    .SetExclusiveBitMask(exclusiveBitmask);

                using (var config = configBuilder.Build())
                {
                    mTurnBasedManager.CreateMatch(config, BridgeMatchToUserCallback(
                        (status, match) => callback(status == UIStatus.Valid, match)));
                }
            }
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<bool, TurnBasedMatch> callback)
        {
            CreateWithInvitationScreen(minOpponents, maxOpponents, variant,
                (status, match) => callback(status == UIStatus.Valid, match));
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<UIStatus, TurnBasedMatch> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            mTurnBasedManager.ShowPlayerSelectUI(minOpponents, maxOpponents, true, result =>
                {
                    if (result.Status() != Cwrapper.CommonErrorStatus.UIStatus.VALID)
                    {
                        callback((UIStatus)result.Status(), null);
                        return;
                    }

                    using (var configBuilder = TurnBasedMatchConfigBuilder.Create())
                    {
                        configBuilder.PopulateFromUIResponse(result)
                            .SetVariant(variant);
                        using (var config = configBuilder.Build())
                        {
                            mTurnBasedManager.CreateMatch(config, BridgeMatchToUserCallback(callback));
                        }
                    }
                });
        }

        public void GetAllInvitations(Action<Invitation[]> callback)
        {
            mTurnBasedManager.GetAllTurnbasedMatches(allMatches =>
                {
                    Invitation[] invites = new Invitation[allMatches.InvitationCount()];
                    int i=0;
                    foreach (var invitation in allMatches.Invitations())
                    {
                        invites[i++] = invitation.AsInvitation();
                    }
                    callback(invites);
                });
        }

        public void GetAllMatches(Action<TurnBasedMatch[]> callback)
        {
            mTurnBasedManager.GetAllTurnbasedMatches(allMatches =>
                {
                    int count = allMatches.MyTurnMatchesCount() +
                        allMatches.TheirTurnMatchesCount() +
                        allMatches.CompletedMatchesCount();

                    TurnBasedMatch[] matches = new TurnBasedMatch[count];
                    int i=0;
                    foreach (var match in allMatches.MyTurnMatches())
                    {
                        matches[i++] = match.AsTurnBasedMatch(mNativeClient.GetUserId());
                    }
                    foreach (var match in allMatches.TheirTurnMatches())
                    {
                        matches[i++] = match.AsTurnBasedMatch(mNativeClient.GetUserId());
                    }
                    foreach (var match in allMatches.CompletedMatches())
                    {
                        matches[i++] = match.AsTurnBasedMatch(mNativeClient.GetUserId());
                    }
                    callback(matches);
                });
        }
        
        public void GetMatch(string matchId, Action<bool, TurnBasedMatch> callback)
        {
            mTurnBasedManager.GetMatch(matchId, response =>
            {
                using (var foundMatch = response.Match())
                {
                    if (foundMatch == null)
                    {
                        Logger.e(string.Format("Could not find match {0}", matchId));
                        callback(false, null);
                    }
                    else
                    {
                        callback(true, foundMatch.AsTurnBasedMatch(mNativeClient.GetUserId()));
                    }
                }
            });
        }

        private Action<TurnBasedManager.TurnBasedMatchResponse> BridgeMatchToUserCallback(
            Action<UIStatus, TurnBasedMatch> userCallback)
        {
            return callbackResult =>
            {
                using (var match = callbackResult.Match())
                {
                    if (match == null)
                    {
                        UIStatus status = UIStatus.InternalError;
                        switch(callbackResult.ResponseStatus())
                        {
                            case Cwrapper.CommonErrorStatus.MultiplayerStatus.VALID:
                                status = UIStatus.Valid;
                                break;
                            case Cwrapper.CommonErrorStatus.MultiplayerStatus.VALID_BUT_STALE:
                                status = UIStatus.Valid;
                                break;
                            case Cwrapper.CommonErrorStatus.MultiplayerStatus.ERROR_INTERNAL:
                                status = UIStatus.InternalError;
                                break;
                            case Cwrapper.CommonErrorStatus.MultiplayerStatus.ERROR_NOT_AUTHORIZED:
                                status = UIStatus.NotAuthorized;
                                break;
                            case Cwrapper.CommonErrorStatus.MultiplayerStatus.ERROR_VERSION_UPDATE_REQUIRED:
                                status = UIStatus.VersionUpdateRequired;
                                break;
                            case Cwrapper.CommonErrorStatus.MultiplayerStatus.ERROR_TIMEOUT:
                                status = UIStatus.Timeout;
                                break;
                        }
                        userCallback(status, null);
                    }
                    else
                    {
                        var converted = match.AsTurnBasedMatch(mNativeClient.GetUserId());
                        Logger.d("Passing converted match to user callback:" + converted);
                        userCallback(UIStatus.Valid, converted);
                    }
                }
            };
        }

        public void AcceptFromInbox(Action<bool, TurnBasedMatch> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            mTurnBasedManager.ShowInboxUI(callbackResult =>
                {
                    using (var match = callbackResult.Match())
                    {
                        if (match == null)
                        {
                            callback(false, null);
                        }
                        else
                        {
                            var converted = match.AsTurnBasedMatch(mNativeClient.GetUserId());
                            Logger.d("Passing converted match to user callback:" + converted);
                            callback(true, converted);
                        }
                    }
                });
        }

        public void AcceptInvitation(string invitationId, Action<bool, TurnBasedMatch> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            FindInvitationWithId(invitationId,
                invitation =>
                {
                    if (invitation == null)
                    {
                        Logger.e("Could not find invitation with id " + invitationId);
                        callback(false, null);
                        return;
                    }

                    mTurnBasedManager.AcceptInvitation(invitation, BridgeMatchToUserCallback(
                        (status, match) => callback(status == UIStatus.Valid, match)));
                }
            );
        }

        private void FindInvitationWithId(string invitationId, Action<MultiplayerInvitation> callback)
        {
            mTurnBasedManager.GetAllTurnbasedMatches(allMatches =>
                {
                    if (allMatches.Status() <= 0)
                    {
                        callback(null);
                        return;
                    }

                    foreach (var invitation in allMatches.Invitations())
                    {
                        using (invitation)
                        {
                            if (invitation.Id().Equals(invitationId))
                            {
                                callback(invitation);
                                return;
                            }
                        }
                    }

                    callback(null);
                });
        }

        public void RegisterMatchDelegate(MatchDelegate del)
        {
            if (del == null)
            {
                mMatchDelegate = null;
            }
            else
            {
                mMatchDelegate = Callbacks.AsOnGameThreadCallback<TurnBasedMatch, bool>(
                    (match, autoLaunch) => del(match, autoLaunch));
            }
        }

        internal void HandleMatchEvent(Types.MultiplayerEvent eventType, string matchId,
                                   NativeTurnBasedMatch match)
        {
            // Capture the current value of the delegate to protect against racing updates.
            var currentDelegate = mMatchDelegate;
            if (currentDelegate == null)
            {
                return;
            }

            // Ignore REMOVED events - this plugin has no use for them.
            if (eventType == Types.MultiplayerEvent.REMOVED)
            {
                Logger.d("Ignoring REMOVE event for match " + matchId);
                return;
            }

            bool shouldAutolaunch = eventType == Types.MultiplayerEvent.UPDATED_FROM_APP_LAUNCH;

            match.ReferToMe();
            Callbacks.AsCoroutine(WaitForLogin(()=>
                {currentDelegate(match.AsTurnBasedMatch(mNativeClient.GetUserId()), shouldAutolaunch);
                    match.ForgetMe();}));
        }

        IEnumerator WaitForLogin(Action method)
        {
            if (string.IsNullOrEmpty(mNativeClient.GetUserId()))
            {
                yield return null;
            }
            method.Invoke();
        }


        public void TakeTurn(TurnBasedMatch match, byte[] data, string pendingParticipantId,
                         Action<bool> callback)
        {
            Logger.describe(data);
            callback = Callbacks.AsOnGameThreadCallback(callback);
            // Find the indicated match, take the turn if the match is:
            // a) Still present
            // b) Still of a matching version (i.e. there were no updates to it since the passed match
            //    was retrieved)
            // c) The indicated pending participant matches a participant in the match.
            FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback,
                (pendingParticipant, foundMatch) =>
                {
                    // If we got here, the match and the participant are in a good state.
                    mTurnBasedManager.TakeTurn(foundMatch, data, pendingParticipant,
                        result =>
                        {
                            if (result.RequestSucceeded())
                            {
                                callback(true);
                            }
                            else
                            {
                                Logger.d("Taking turn failed: " + result.ResponseStatus());
                                callback(false);
                            }
                        });
                });
        }

        private void FindEqualVersionMatch(TurnBasedMatch match, Action<bool> onFailure,
                                       Action<NativeTurnBasedMatch> onVersionMatch)
        {
            mTurnBasedManager.GetMatch(match.MatchId, response =>
                {
                    using (var foundMatch = response.Match())
                    {
                        if (foundMatch == null)
                        {
                            Logger.e(string.Format("Could not find match {0}", match.MatchId));
                            onFailure(false);
                            return;
                        }

                        if (foundMatch.Version() != match.Version)
                        {
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
                                                      Action<MultiplayerParticipant, NativeTurnBasedMatch> onFoundParticipantAndMatch)
        {
            FindEqualVersionMatch(match, onFailure, foundMatch =>
                {
                    // If we received a null participantId, we're using an automatching player instead -
                    // issue the callback using that.
                    if (participantId == null)
                    {
                        using (var sentinelParticipant = MultiplayerParticipant.AutomatchingSentinel())
                        {
                            onFoundParticipantAndMatch(sentinelParticipant, foundMatch);
                            return;
                        }
                    }

                    using (var participant = foundMatch.ParticipantWithId(participantId))
                    {
                        if (participant == null)
                        {
                            Logger.e(string.Format("Located match {0} but desired participant with ID " +
                                    "{1} could not be found", match.MatchId, participantId));
                            onFailure(false);
                            return;
                        }

                        onFoundParticipantAndMatch(participant, foundMatch);
                    }
                });
        }

        public int GetMaxMatchDataSize()
        {
            throw new NotImplementedException();
        }

        public void Finish(TurnBasedMatch match, byte[] data, MatchOutcome outcome,
                       Action<bool> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            FindEqualVersionMatch(match, callback, foundMatch =>
                {
                    ParticipantResults results = foundMatch.Results();

                    foreach (string participantId in outcome.ParticipantIds)
                    {
                        Types.MatchResult matchResult =
                            ResultToMatchResult(outcome.GetResultFor(participantId));
                        uint placing = outcome.GetPlacementFor(participantId);

                        if (results.HasResultsForParticipant(participantId))
                        {
                            // If the match already has results for this participant, make sure that they're
                            // consistent with what's already there.
                            var existingResults = results.ResultsForParticipant(participantId);
                            var existingPlacing = results.PlacingForParticipant(participantId);

                            if (matchResult != existingResults || placing != existingPlacing)
                            {
                                Logger.e(string.Format("Attempted to override existing results for " +
                                        "participant {0}: Placing {1}, Result {2}",
                                        participantId, existingPlacing, existingResults));
                                callback(false);
                                return;
                            }
                        }
                        else
                        {
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

        private static Types.MatchResult ResultToMatchResult(MatchOutcome.ParticipantResult result)
        {
            switch (result)
            {
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

        public void AcknowledgeFinished(TurnBasedMatch match, Action<bool> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            FindEqualVersionMatch(match, callback, foundMatch =>
                {
                    mTurnBasedManager.ConfirmPendingCompletion(
                        foundMatch, response => callback(response.RequestSucceeded()));
                });
        }

        public void Leave(TurnBasedMatch match, Action<bool> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            FindEqualVersionMatch(match, callback, foundMatch =>
                {
                    mTurnBasedManager.LeaveMatchDuringTheirTurn(foundMatch, status => callback(status > 0));
                });
        }

        public void LeaveDuringTurn(TurnBasedMatch match, string pendingParticipantId,
                                Action<bool> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback,
                (pendingParticipant, foundMatch) =>
                {
                    mTurnBasedManager.LeaveDuringMyTurn(foundMatch, pendingParticipant,
                        status => callback(status > 0));
                });
        }

        public void Cancel(TurnBasedMatch match, Action<bool> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            FindEqualVersionMatch(match, callback, foundMatch =>
                {
                    mTurnBasedManager.CancelMatch(foundMatch, status => callback(status > 0));
                });
        }

        public void Dismiss(TurnBasedMatch match)
        {
            FindEqualVersionMatch(match, success => {
                // actually just called on failure
                Logger.e(string.Format("Could not find match {0}", match.MatchId));
            }, mTurnBasedManager.DismissMatch);
        }
        
        public void Rematch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback)
        {
            callback = Callbacks.AsOnGameThreadCallback(callback);
            FindEqualVersionMatch(match, failed => callback(false, null), foundMatch =>
                {
                    mTurnBasedManager.Rematch(foundMatch, BridgeMatchToUserCallback(
                        (status, m) => callback(status == UIStatus.Valid, m)));
                });
        }

        public void DeclineInvitation(string invitationId)
        {
            FindInvitationWithId(invitationId, invitation =>
                {
                    if (invitation == null)
                    {
                        return;
                    }

                    mTurnBasedManager.DeclineInvitation(invitation);
                });
        }
    }
}
#endif
