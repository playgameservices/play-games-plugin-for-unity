// <copyright file="IRealTimeMultiplayerClient.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
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

namespace GooglePlayGames.BasicApi.Multiplayer
{
// move inside the namespace so the namespace is declared if not using GPGS
#if UNITY_ANDROID
  using System;
  using System.Collections.Generic;
  using GooglePlayGames.BasicApi.Multiplayer;

  /// <summary>
  /// API entry point for Real-Time multiplayer.
  /// </summary>
  /// <remarks> To know more about multiplayer,
  /// terminology, etc, please refer to the online guide at:
  /// https://github.com/playgameservices/play-games-plugin-for-unity
  /// </remarks>
  public interface IRealTimeMultiplayerClient
  {
    /// <summary>
    /// Creates a game with random automatch opponents. No UI will be shown.
    /// </summary>
    /// <remarks>
    /// The participants will be automatically selected among users who are currently
    /// looking for opponents.
    /// After calling this method, your listener's
    /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress" />
    /// method will be called to indicate room setup progress. Eventually,
    /// <see cref="RealTimeMultiplayerListener.OnRoomConnected" />
    /// will be called to indicate that the room setup is either complete or has failed
    /// (check the <b>success</b> parameter of the callback). If you wish to
    /// cancel room setup, call <see cref="LeaveRoom"/>.
    /// </remarks>
    /// <param name="minOpponents">Minimum number of opponents (not counting the
    /// current player -- so for a 2-player game, pass 1).</param>
    /// <param name="maxOpponents">Max number of opponents (not counting the current
    /// player -- so for a 2-player game, pass 1).</param>
    /// <param name="variant">Variant. Use 0 for default.</param>
    /// <param name="listener">Listener. The listener to notify of relevant events.</param>
    void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
                         RealTimeMultiplayerListener listener);

    /// <summary>
    /// Creates a game with random automatch opponents using exclusiveBitMask.
    /// </summary>
    /// <remarks> No UI will be shown.
    /// The participants will be automatically selected among users who are currently
    /// looking for opponents.
    /// After calling this method, your listener's
    /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress" />
    /// method will be called to indicate room setup progress. Eventually,
    /// <see cref="RealTimeMultiplayerListener.OnRoomConnected" />
    /// will be called to indicate that the room setup is either complete or has failed
    /// (check the <b>success</b> parameter of the callback). If you wish to
    /// cancel room setup, call <see cref="LeaveRoom"/>.
    /// </remarks>
    /// <param name="minOpponents">Minimum number of opponents (not counting the
    /// current player -- so for a 2-player game, pass 1).</param>
    /// <param name="maxOpponents">Max number of opponents (not counting the current
    /// player -- so for a 2-player game, pass 1).</param>
    /// <param name="variant">Variant. Use 0 for default.</param>
    /// <param name="exclusiveBitMask">Exclusive bit mask. Players are matched if the masks logically AND'ed = 0</param>
    /// <param name="listener">Listener. The listener to notify of relevant events.</param>
    void CreateQuickGame(uint minOpponents, uint maxOpponents, uint variant,
        ulong exclusiveBitMask,
        RealTimeMultiplayerListener listener);

    /// <summary>
    /// Creates a game with an invitation screen.
    /// </summary>
    /// <remarks> An invitation screen will be shown
    /// where the user can select who to invite to a multiplayer game. The invitation
    /// screen also allows the user to add random automatch opponents. After the invitation
    /// screen is dismissed, the room connection process will begin. The listener's
    /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress"/> will be called
    /// to report room setup progress, and eventually
    /// <see cref="RealTimeMultiplayerListener.OnRoomConnected"/> will be called to
    /// indicate that the room setup is either complete or has failed (check the
    /// <b>success</b> parameter of the callback).
    /// </remarks>
    /// <param name="minOpponents">Minimum number of opponents, not including the
    /// current player.</param>
    /// <param name="maxOppponents">Maximum number of oppponents, not including the
    /// current player.</param>
    /// <param name="variant">Variant. Use 0 for default.</param>
    /// <param name="listener">Listener. This listener will be notified of relevant
    /// events.</param>
    void CreateWithInvitationScreen(uint minOpponents, uint maxOppponents, uint variant,
                                    RealTimeMultiplayerListener listener);

    /// <summary>
    /// Shows the waiting room UI and waits for all participants to join.
    /// </summary>
    void ShowWaitingRoomUI();

    /// <summary>Gets all invitations.</summary>
    /// <param name="callback">Callback.</param>
    void GetAllInvitations(Action<Invitation[]> callback);

    /// <summary>
    /// Creates a real-time game starting with the inbox screen.
    /// </summary>
    /// <remarks>On the inbox screen,
    /// the player can select an invitation to accept, in which case the room setup
    /// process will start. The listener's
    /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress"/> will be called
    /// to report room setup progress, and eventually
    /// <see cref="RealTimeMultiplayerListener.OnRoomConnected"/> will be called to
    /// indicate that the room setup is either complete or has failed (check the
    /// <b>success</b> parameter of the callback).
    /// </remarks>
    /// <param name="listener">Listener. The listener to notify of relevant events.</param>
    void AcceptFromInbox(Multiplayer.RealTimeMultiplayerListener listener);

    /// <summary>
    /// Accepts an invitation, given its ID.
    /// </summary>
    /// <remarks>This will not show any UI. The listener's
    /// <see cref="RealTimeMultiplayerListener.OnRoomSetupProgress"/> will be called
    /// to report room setup progress, and eventually
    /// <see cref="RealTimeMultiplayerListener.OnRoomConnected"/> will be called to
    /// indicate that the room setup is either complete or has failed (check the
    /// <b>success</b> parameter of the callback).
    /// </remarks>
    /// <param name="invitationId">Invitation id to accept.</param>
    /// <param name="listener">Listener. Listener to notify of relevant events.</param>
    void AcceptInvitation(string invitationId, RealTimeMultiplayerListener listener);

    /// <summary>Sends a message to all other participants.</summary>
    /// <param name="reliable">If set to <c>true</c>, mesasge is reliable; if not,
    /// it is unreliable. Unreliable messages are faster, but are not guaranteed to arrive
    /// and may arrive out of order.</param>
    /// <param name="data">Data. The data to send.</param>
    void SendMessageToAll(bool reliable, byte[] data);

    /// <summary>
    /// Same as <see cref="SendMessageToAll(bool,byte[])" />, but allows you to specify
    /// offset and length of the data buffer.
    /// </summary>
    /// <param name="reliable">If set to <c>true</c>, message is reliable.</param>
    /// <param name="data">Data.</param>
    /// <param name="offset">Offset. Offset of the data buffer where data starts.</param>
    /// <param name="length">Length. Length of data (from offset).</param>
    void SendMessageToAll(bool reliable, byte[] data, int offset, int length);

    /// <summary>
    /// Send a message to a particular participant.
    /// </summary>
    /// <param name="reliable">If set to <c>true</c>, message is reliable; if not,
    /// it is unreliable. Unreliable messages are faster, but are not guaranteed to arrive
    /// and may arrive out of order.</param>
    /// <param name="participantId">Participant ID. The participant to whom the message
    /// will be sent.</param>
    /// <param name="data">Data. The data to send.</param>
    void SendMessage(bool reliable, string participantId, byte[] data);

    /// <summary>
    /// Same as <see cref="SendMessage(bool,string,byte[])" />, but allows you to specify
    /// the offset and length of the data buffer.
    /// </summary>
    void SendMessage(bool reliable, string participantId, byte[] data, int offset, int length);

    /// <summary>Gets the connected participants, including self.</summary>
    /// <returns>The connected participants, including self. This list is guaranteed
    /// to be ordered lexicographically by Participant ID, which means the ordering will be
    /// the same to all participants.</returns>
    List<Multiplayer.Participant> GetConnectedParticipants();

    /// <summary>Gets the participant that represents the current player.</summary>
    /// <returns>Self.</returns>
    Multiplayer.Participant GetSelf();

    /// <summary>Gets a participant by ID.</summary>
    /// <returns>The participant, or <c>null</c> if not found.</returns>
    /// <param name="participantId">Participant id.</param>
    Participant GetParticipant(string participantId);

    /// <summary>Gets the invitation used to create the game, if any.</summary>
    /// <returns>The invitation.  Will be null if no invitation was accepted.</returns>
    Invitation GetInvitation();

    /// <summary>
    /// Leaves the room.
    /// </summary>
    /// <remarks>Call this method to leave the room after you have
    /// started room setup. Leaving the room is not an immediate operation -- you
    /// must wait for <see cref="RealTimeMultiplayerListener.OnLeftRoom"/>
    /// to be called. If you leave a room before setup is complete, you will get
    /// a call to
    /// <see cref="RealTimeMultiplayerListener.OnRoomConnected"/> with <b>false</b>
    /// parameter instead. If you attempt to leave a room that is shutting down or
    /// has shutdown already, you will immediately receive the
    /// <see cref="RealTimeMultiplayerListener.OnLeftRoom"/> callback.
    /// </remarks>
    void LeaveRoom();

    /// <summary>
    /// Returns whether or not the room is connected (ready to play).
    /// </summary>
    /// <returns><c>true</c> if the room is connected; otherwise, <c>false</c>.</returns>
    bool IsRoomConnected();

    /// <summary>Declines the invitation.</summary>
    /// <param name="invitationId">Invitation id to decline.</param>
    void DeclineInvitation(string invitationId);
  }
    #endif
}