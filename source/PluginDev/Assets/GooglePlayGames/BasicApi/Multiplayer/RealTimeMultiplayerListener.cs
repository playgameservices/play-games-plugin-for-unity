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

using System;
using System.Collections.Generic;

namespace GooglePlayGames.BasicApi.Multiplayer {
    /// <summary>
    /// Real time multiplayer listener. This listener will be called to notify you
    /// of real-time room events.
    /// </summary>
    public interface RealTimeMultiplayerListener {
        /// <summary>
        /// Called during room setup to notify of room setup progress.
        /// </summary>
        /// <param name="percent">The room setup progress in percent (0.0 to 100.0).</param>
        void OnRoomSetupProgress(float percent);
        
        /// <summary>
        /// Notifies that room setup is finished. If <c>success == true</c>, you should
        /// react by staring to play the game; otherwise, show an error screen.
        /// </summary>
        /// <param name="success">Whether setup was successful.</param>
        void OnRoomConnected(bool success);
        
        /// <summary>
        /// Notifies that the current player has left the room. This may have happened
        /// because you called LeaveRoom, or because an error occurred and the player
        /// was dropped from the room. You should react by stopping your game and
        /// possibly showing an error screen (unless leaving the room was the player's
        /// request, naturally).
        /// </summary>
        void OnLeftRoom();
        
        /// <summary>
        /// Called when peers connect to the room.
        /// </summary>
        /// <param name="participantIds">Participant identifiers.</param>
        void OnPeersConnected(string[] participantIds);
        
        /// <summary>
        /// Called when peers disconnect from the room.
        /// </summary>
        /// <param name="participantIds">Participant identifiers.</param>
        void OnPeersDisconnected(string[] participantIds);
        
        /// <summary>
        /// Called when a real-time message is received.
        /// </summary>
        /// <param name="isReliable">Whether the message was sent as a reliable message or not.</param>
        /// <param name="senderId">Sender identifier.</param>
        /// <param name="data">Data.</param>
        void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data);
    }
}
