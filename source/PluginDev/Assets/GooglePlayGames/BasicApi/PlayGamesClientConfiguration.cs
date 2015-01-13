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

using System;
using GooglePlayGames.OurUtils;
using GooglePlayGames.BasicApi.Multiplayer;

namespace GooglePlayGames.BasicApi {

/// <summary>
/// Provides configuration for <see cref="PlayGamesPlatform"/>. If you wish to use either Saved
/// Games or Cloud Save, you must create an instance of this class with those features enabled.
/// Note that Cloud Save is deprecated, and is not available for new games. You should strongly
/// favor Saved Game.
/// </summary>
public struct PlayGamesClientConfiguration {

    public static readonly PlayGamesClientConfiguration DefaultConfiguration =
        new Builder().Build();

    private readonly bool mEnableSavedGames;
    private readonly bool mEnableDeprecatedCloudSave;
    private readonly InvitationReceivedDelegate mInvitationDelegate;
    private readonly MatchDelegate mMatchDelegate;

    private PlayGamesClientConfiguration(Builder builder) {
        this.mEnableSavedGames = builder.mEnableSaveGames;
        this.mEnableDeprecatedCloudSave = builder.mEnableDeprecatedCloudSave;
        this.mInvitationDelegate = builder.mInvitationDelegate;
        this.mMatchDelegate = builder.mMatchDelegate;
    }

    public bool EnableSavedGames {
        get {
            return mEnableSavedGames;
        }
    }

    public bool EnableDeprecatedCloudSave {
        get {
            return mEnableDeprecatedCloudSave;
        }
    }

    public InvitationReceivedDelegate InvitationDelegate {
        get {
            return mInvitationDelegate;
        }
    }

    public MatchDelegate MatchDelegate {
        get {
            return mMatchDelegate;
        }
    }

    public class Builder {
        internal bool mEnableSaveGames = false;
        internal bool mEnableDeprecatedCloudSave = false;
        internal InvitationReceivedDelegate mInvitationDelegate = delegate {};
        internal MatchDelegate mMatchDelegate = delegate {};

        public Builder EnableSavedGames() {
            mEnableSaveGames = true;
            return this;
        }

        /// <summary>
        /// Enables the now-deprecated cloud save. This is only present for backwards-compatibility 
        /// and legacy purposes. New games cannot have cloud save enabled, and must use Saved Games.
        /// Existing games should migrate off of cloud save as soon as possible.
        /// </summary>
        /// <returns>The builder instance</returns>
        public Builder EnableDeprecatedCloudSave() {
            Logger.w("Cloud save is deprecated and is not available for new games. " +
                "Please migrate to Saved Games as soon as possible.");
            mEnableDeprecatedCloudSave = true;
            return this;
        }

        public Builder WithInvitationDelegate(InvitationReceivedDelegate invitationDelegate) {
            this.mInvitationDelegate = Misc.CheckNotNull(invitationDelegate);
            return this;
        }

        public Builder WithMatchDelegate(MatchDelegate matchDelegate) {
            this.mMatchDelegate = Misc.CheckNotNull(matchDelegate);
            return this;
        }

        public PlayGamesClientConfiguration Build() {
            return new PlayGamesClientConfiguration(this);
        }
    }
}
}

