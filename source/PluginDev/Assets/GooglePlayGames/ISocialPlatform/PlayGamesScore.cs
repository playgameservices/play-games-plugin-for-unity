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
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace GooglePlayGames {
    /// <summary>
    /// Represents a Google Play Games score that can be sent to a leaderboard.
    /// </summary>
    public class PlayGamesScore : IScore {
        string mLbId = null;
        long mValue = 0;
        long mTimestamp = 0;
        int mRank = 0;
        
        internal PlayGamesScore() {}
        
		public PlayGamesScore (string mLbId, long timestamp, int rankValue, long value) {
    		this.mLbId = mLbId;
			this.mTimestamp = timestamp;
			this.mRank = rankValue;
			this.mValue = value;
    	}

        /// <summary>
        /// Reports the score. Equivalent to <see cref="PlayGamesPlatform.ReportScore" />.
        /// </summary>
        public void ReportScore (Action<bool> callback)
        {
            PlayGamesPlatform.Instance.ReportScore(mValue, mLbId, callback);
        }

        /// <summary>
        /// Gets or sets the leaderboard id.
        /// </summary>
        /// <returns>
        /// The leaderboard id.
        /// </returns>
        public string leaderboardID {
            get {
                return mLbId;
            }
            set {
                mLbId = value;
            }
        }

        /// <summary>
        /// Gets or sets the score value.
        /// </summary>
        /// <returns>
        /// The value.
        /// </returns>
        public long value {
            get {
                return mValue;
            }
            set {
                mValue = value;
            }
        }

        /// <summary>
        /// Returns the date of the last score update.
        /// </summary>
        public DateTime date {
            get {
				return new DateTime(mTimestamp).AddYears(1970);
            }
        }

        /// <summary>
        /// Not implemented. Returns the value converted to a string, unformatted.
        /// </summary>
        public string formattedValue {
            get {
                return mValue.ToString();
            }
        }

        /// <summary>
        /// Not implemented. Returns the empty string.
        /// </summary>
        public string userID {
            get {
                return "";
            }
        }

        /// <summary>
        /// Returns player rank in the leaderboard.
        /// </summary>
        public int rank {
            get {
                return mRank;
            }
        }
        
        public override string ToString ()
    	{
			return string.Format ("[PlayGamesScore: leaderboardID={0}, value={1}, date={2}, Timestamp={3}, rank={4}]", leaderboardID, value, date, mTimestamp, rank);
    	}
    }
}
