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

namespace GooglePlayGames.BasicApi.Multiplayer {
    /// <summary>
    /// Represents a player. A player is different from a participant! The participant is
    /// an entity that takes part in a particular match; a Player is a real-world person
    /// (tied to a Google account). The player exists across matches, the Participant
    /// only exists in the context of a particular match.
    /// </summary>
    public class Player {
        private string mDisplayName = "";
        private string mPlayerId = "";

        internal Player(string displayName, string playerId) {
            mDisplayName = displayName;
            mPlayerId = playerId;
        }

        /// Player's display name.
        public string DisplayName {
            get {
                return mDisplayName;
            }
        }

        /// Player's ID. Always the same for a particular person. It does not vary across matches.
        public string PlayerId {
            get {
                return mPlayerId;
            }
        }

        public override string ToString() {
            return string.Format("[Player: '{0}' (id {1})]", mDisplayName, mPlayerId);
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Player))
                return false;
            Player other = (Player)obj;
            return mPlayerId == other.mPlayerId;
        }

        public override int GetHashCode() {
            unchecked {
                return (mPlayerId != null ? mPlayerId.GetHashCode() : 0);
            }
        }
    }
}

