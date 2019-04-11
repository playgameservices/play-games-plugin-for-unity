// <copyright file="ITurnBasedMultiplayerClient.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi.Multiplayer
{
    using System;

    /// <summary>
    /// API entry point for turn-based multiplayer.
    /// </summary>
    public interface ITurnBasedMultiplayerClient
    {
        /// <summary>
        /// Starts a game with randomly selected opponent(s). No UI will be shown.
        /// </summary>
        /// <param name="minOpponents">Minimum number opponents, not counting the current
        /// player -- so for a 2-player game, use 1).</param>
        /// <param name="maxOpponents">Max opponents, not counting the current player.</param>
        /// <param name="variant">Variant. Use 0 for default.</param>
        /// <param name="callback">Callback. Called when match setup is complete or fails.
        /// If it succeeds, will be called with (true, match); if it fails, will be
        /// called with (false, null).</param>
        void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
                              Action<bool, TurnBasedMatch> callback);

        /// <summary>
        /// Starts a game with randomly selected opponent(s) using exclusiveBitMask.
        ///  No UI will be shown.
        /// </summary>
        /// <param name="minOpponents">Minimum number opponents, not counting the current
        /// player -- so for a 2-player game, use 1).</param>
        /// <param name="maxOpponents">Max opponents, not counting the current player.</param>
        /// <param name="variant">Variant. Use 0 for default.</param>
        /// <param name="exclusiveBitmask">The bitmask used to match players.  The
        /// xor operation of all the bitmasks must be 0 to match players.</param>
        /// <param name="callback">Callback. Called when match setup is complete or fails.
        /// If it succeeds, will be called with (true, match); if it fails, will be
        /// called with (false, null).</param>
        void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitmask, Action<bool, TurnBasedMatch> callback);

        /// <summary>
        /// Starts a game with an invitation screen.
        /// </summary>
        /// <remarks>An invitation screen will be shown
        /// allowing the player to select opponents to play against.
        /// </remarks>
        /// <param name="minOpponents">Minimum number of opponents, not including self
        /// (so for a 2-player game, use 1).</param>
        /// <param name="maxOpponents">Maximum number of opponents.</param>
        /// <param name="variant">Variant. Use 0 for default.</param>
        /// <param name="callback">Callback. Will be called with (true, match) on success,
        /// and (false, null) if there is an error or the user cancelled.</param>
        void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
                                        Action<bool, TurnBasedMatch> callback);

        /// <summary>
        /// Starts a game with an invitation screen. </summary>
        /// <remarks> An invitation screen will be shown
        /// allowing the player to select opponents to play against.
        /// </remarks>
        /// <param name="minOpponents">Minimum number of opponents, not including self
        /// (so for a 2-player game, use 1).</param>
        /// <param name="maxOpponents">Maximum number of opponents.</param>
        /// <param name="variant">Variant. Use 0 for default.</param>
        /// <param name="callback">Callback. Will be called with (UIStatus, match).  The UIStatus
        /// parameter indicates the type of error or if the user cancelled the UI.</param>
        void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<UIStatus, TurnBasedMatch> callback);

        /// <summary>
        /// Gets all invitations.
        /// </summary>
        /// <param name="callback">Callback.</param>
        void GetAllInvitations(Action<Invitation[]> callback);

        /// <summary>
        /// Gets all matches.
        /// </summary>
        /// <param name="callback">Callback.</param>
        void GetAllMatches(Action<TurnBasedMatch[]> callback);

        /// <summary>
        /// Gets match for given match id.
        /// </summary>
        /// <param name="matchId">Match id</param>
        /// <param name="callback">Callback.</param>
        void GetMatch(string matchId, Action<bool, TurnBasedMatch> callback);
        
        /// <summary>
        /// Starts a game by showing the match inbox.</summary>
        /// <remarks> The player's match inbox will be
        /// shown, allowing the player to pick an ongoing match or accept an outstanding
        /// invite. Once they choose a match or invitation, your callback will be called.
        /// Notice that the inbox contains all the matches the player are involved in,
        /// whether or not it's their turn, so the player may select a match where it is
        /// not their turn to play. Your code must react to that appropriately by showing
        /// the match, but not letting the player make a move.
        /// </remarks>
        /// <param name="callback">Callback. Will be called with (true, match) on success,
        /// or (false, null) if there is an error or the user cancels.</param>
        void AcceptFromInbox(Action<bool, TurnBasedMatch> callback);

        /// <summary>
        /// Accepts the given invitation.
        /// </summary>
        /// <param name="invitationId">Invitation id to accept.</param>
        /// <param name="callback">Callback.</param>
        void AcceptInvitation(string invitationId, Action<bool, TurnBasedMatch> callback);

        /// <summary>
        /// Register a match delegate to be called when a match arrives.</summary>
        /// <remarks> Matches may arrive
        /// as notifications on the device when it's the player's turn. If the match
        /// arrived via notification (this can be determined from the delegate's parameters),
        /// the recommended implementation is to take the player directly to the game
        /// screen so they can play their turn.
        /// </remarks>
        /// <param name="del">Delegate to notify when a match arrives.</param>
        void RegisterMatchDelegate(MatchDelegate del);

        /// <summary>
        /// Take a turn.</summary>
        /// <remarks>Before you call this method, make sure that it is actually the
        /// player's turn in the match, otherwise this call will fail.
        /// </remarks>
        /// <param name="match">Match identifier.</param>
        /// <param name="data">Data. New match data.</param>
        /// <param name="pendingParticipantId">ID of participant who is next to play. If
        /// this is null and there are automatch slots open, the turn will be passed
        /// to one of the automatch players. Passing null when there are no open
        /// automatch slots is an error.</param>
        /// <param name="callback">Callback. Will be called with true for success,
        /// false for failure.</param>
        void TakeTurn(TurnBasedMatch match, byte[] data, string pendingParticipantId,
                      Action<bool> callback);

        /// <summary>
        /// Gets the size of the max match data, in bytes.
        /// </summary>
        /// <returns>The max match data size in bytes.</returns>
        int GetMaxMatchDataSize();

        /// <summary>
        /// Finishes a match.
        /// </summary>
        /// <param name="match">Match identifier.</param>
        /// <param name="data">Data. Final match data.</param>
        /// <param name="outcome">Outcome. The outcome of the match (who won, who lost, ...)</param>
        /// <param name="callback">Callback. Called with true for success, false for failure</param>
        void Finish(TurnBasedMatch match, byte[] data, MatchOutcome outcome, Action<bool> callback);

        /// <summary>
        /// Acknowledges that a match was finished.</summary>
        /// <remarks>
        /// Call this on a finished match that you
        /// have just shown to the user, to acknowledge that the user has seen the results
        /// of the finished match. This will remove the match from the user's inbox.
        /// </remarks>
        /// <param name="match">Match identifier.</param>
        /// <param name="callback">Callback. Called with true for success, false for failure.</param>
        void AcknowledgeFinished(TurnBasedMatch match, Action<bool> callback);

        /// <summary>
        /// Leave the match (not during turn). Call this to leave the match when it is not your
        /// turn.
        /// </summary>
        /// <param name="match">Match identifier.</param>
        /// <param name="callback">Callback.</param>
        void Leave(TurnBasedMatch match, Action<bool> callback);

        /// <summary>
        /// Leave the match (during turn). Call this to leave the match when it's your turn.
        /// </summary>
        /// <param name="match">Match identifier.</param>
        /// <param name="pendingParticipantId">ID of next participant to play.</param>
        /// <param name="callback">Callback.</param>
        void LeaveDuringTurn(TurnBasedMatch match, string pendingParticipantId,
                             Action<bool> callback);

        /// <summary>
        /// Cancel a match.</summary>
        /// <remarks>Cancelling a match means the match will be cancelled to all
        /// participants. Only cancel matches in extreme cases (corrupt data, irrecoverable
        /// logic errors); if the player is no longer
        /// interested in the match, use <see cref="Leave"/> instead of <see cref="Cancel"/>.
        /// </remarks>
        /// <param name="match">Match identifier.</param>
        /// <param name="callback">Callback.</param>
        void Cancel(TurnBasedMatch match, Action<bool> callback);

        /// <summary>
        /// Dismiss a match.
        /// </summary>
        /// <remarks>Dismissing a match hides it from the dismisser's match list UI
        /// and causes the match to eventually expire. Other match participants can continue 
        /// to play until the dismissed match expires after two weeks, or until the match is 
        /// played to completion or canceled (whichever happens first). To other participants, 
        /// the dismisser still appears as a participant in the match. Another player cannot 
        /// take the dismisser's place.
        /// </remarks>
        /// <param name="match">Match identifier.</param>
        void Dismiss(TurnBasedMatch match);

        /// <summary>
        /// Request a rematch.</summary>
        /// <remarks>
        /// This can be used on a finished match in order to start a new
        /// match with the same opponents.
        /// </remarks>
        /// <param name="match">Match identifier.</param>
        /// <param name="callback">Callback.</param>
        void Rematch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback);

        /// <summary>
        /// Declines the invitation.
        /// </summary>
        /// <param name="invitationId">Invitation identifier.</param>
        void DeclineInvitation(string invitationId);
    }

    /// <summary>
/// Match delegate. Called when a match arrives.
/// <param name="shouldAutoLaunch">If this is true, then the game should immediately
/// proceed to the game screen to play this match, without prompting the user. If
/// false, you should prompt the user before going to the match screen. As an example,
/// when a user taps on the "Play" button on a notification in Android, it is
/// clear that they want to play that match right away, so the plugin calls this
/// delegate with shouldAutoLaunch = true. However, if we receive an incoming notification
/// that the player hasn't specifically indicated they wish to accept (for example,
/// we received one in the background from the server), this delegate will be called
/// with shouldAutoLaunch=false to indicate that you should confirm with the user
/// before switching to the game.</param>
/// </summary>
    public delegate void MatchDelegate(TurnBasedMatch match, bool shouldAutoLaunch);
}
#endif
