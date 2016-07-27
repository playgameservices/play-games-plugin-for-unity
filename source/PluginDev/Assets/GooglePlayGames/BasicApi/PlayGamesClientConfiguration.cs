// <copyright file="PlayGamesClientConfiguration.cs" company="Google Inc.">
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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.BasicApi
{
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.OurUtils;
    using System.Collections.Generic;

    /// <summary>
    /// Provides configuration for <see cref="PlayGamesPlatform"/>. If you wish to use either Saved
    /// Games or Cloud Save, you must create an instance of this class with those features enabled.
    /// Note that Cloud Save is deprecated, and is not available for new games. You should strongly
    /// favor Saved Game.
    /// </summary>
    public struct PlayGamesClientConfiguration
    {
        /// <summary>
        /// The default configuration.
        /// </summary>
        public static readonly PlayGamesClientConfiguration DefaultConfiguration =
            new Builder()
           .WithPermissionRationale("Select email address to send to this game or hit cancel to not share.")
           .Build();
        /// <summary>
        /// Flag indicating to enable saved games API.
        /// </summary>
        private readonly bool mEnableSavedGames;

        /// <summary>
        /// Flag indicating to request use of a player's Google+ social graph.
        /// </summary>
        private readonly bool mRequireGooglePlus;
        
        /// <summary>
        /// Array of scopes to be requested from user. None is considered as 'games_lite'.
        /// </summary>
        private readonly string[] mScopes;

        /// <summary>
        /// The invitation delegate.
        /// </summary>
        private readonly InvitationReceivedDelegate mInvitationDelegate;

        /// <summary>
        /// The match delegate.
        /// </summary>
        private readonly MatchDelegate mMatchDelegate;

        /// <summary>
        /// The permission rationale message to show in Android when requesting
        /// the GET_ACCOUNTS permission to get email and tokens.
        /// </summary>
        private readonly string mPermissionRationale;

        /// <summary>
        /// Initializes a new instance of the <see cref="GooglePlayGames.BasicApi.PlayGamesClientConfiguration"/> struct.
        /// </summary>
        /// <param name="builder">Builder for this configuration.</param>
        private PlayGamesClientConfiguration(Builder builder)
        {
            this.mEnableSavedGames = builder.HasEnableSaveGames();
            this.mInvitationDelegate = builder.GetInvitationDelegate();
            this.mMatchDelegate = builder.GetMatchDelegate();
            this.mPermissionRationale = builder.GetPermissionRationale();
            this.mRequireGooglePlus = builder.HasRequireGooglePlus();
            this.mScopes = builder.getScopes();
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="GooglePlayGames.BasicApi.PlayGamesClientConfiguration"/>
        /// enable saved games.
        /// </summary>
        /// <value><c>true</c> if enable saved games; otherwise, <c>false</c>.</value>
        public bool EnableSavedGames
        {
            get
            {
                return mEnableSavedGames;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="GooglePlayGames.BasicApi.PlayGamesClientConfiguration"/>
        /// requests a player's Google+ social graph.
        /// </summary>
        /// <value><c>true</c> if requests Google+ social graph; otherwise, <c>false</c>.</value>
        public bool RequireGooglePlus
        {
            get
            {
                return mRequireGooglePlus;
            }
        }
        
        /// <summary>
        /// Gets a array of scopes to be requested from the user.
        /// </summary>
        /// <value>String array of scopes.</value>
        public string[] Scopes
        {
            get
            {
                return mScopes;
            }
        }

        /// <summary>
        /// Gets the invitation delegate.
        /// </summary>
        /// <value>The invitation delegate.</value>
        public InvitationReceivedDelegate InvitationDelegate
        {
            get
            {
                return mInvitationDelegate;
            }
        }

        /// <summary>
        /// Gets the match delegate.
        /// </summary>
        /// <value>The match delegate.</value>
        public MatchDelegate MatchDelegate
        {
            get
            {
                return mMatchDelegate;
            }
        }

        /// <summary>
        /// Gets the permission rationale.
        /// </summary>
        /// <value>The permission rationale.</value>
        public string PermissionRationale
        {
            get
            {
                return mPermissionRationale;
            }
        }

        /// <summary>
        /// Builder class for the configuration.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// The flag to enable save games. Default is false.
            /// </summary>
            private bool mEnableSaveGames = false;

            /// <summary>
            /// The flag to request Google+. Default is false.
            /// </summary>
            private bool mRequireGooglePlus = false;
            
            /// <summary>
            /// The scopes to request from the user. Default is none.
            /// </summary>
            private List<string> mScopes = null;

            /// <summary>
            /// The invitation delegate.  Default is a no-op;
            /// </summary>
            private InvitationReceivedDelegate mInvitationDelegate = delegate
            {
            };

            /// <summary>
            /// The match delegate.  Default is a no-op.
            /// </summary>
            private MatchDelegate mMatchDelegate = delegate
            {
            };

            /// <summary>
            /// The rationale for the GET_ACCOUNTS permission in android.
            /// Default is empty.
            /// </summary>
            private string mRationale;

            /// <summary>
            /// Enables the saved games.
            /// </summary>
            /// <returns>The builder.</returns>
            public Builder EnableSavedGames()
            {
                mEnableSaveGames = true;
                return this;
            }

            /// <summary>
            /// Requests use of the player's Google+ social graph.
            /// </summary>
            /// <remarks>
            /// Set this to request use of the player's Google+ social graph. Setting
            /// this will require Android users to have a Google+ profile in order
            /// to be able to sign in (on iOS, users must always have one).
            /// </remarks>
            /// <returns>The builder.</returns>
            public Builder RequireGooglePlus()
            {
                mRequireGooglePlus = true;
                return this;
            }

            /// <summary>
            /// Requests an Oauth scope from the user.
            /// </summary>
            /// <remarks>
            /// Not setting one will default to 'games_lite' and will not show a consent
            /// dialog to the user. Valid examples are 'profile' and 'email'.
            /// Full list: https://developers.google.com/identity/protocols/googlescopes
            /// To exchange the auth code with an id_token (or user id) on your server, 
            /// you must add at least one scope.
            /// </remarks>
            /// <returns>The builder.</returns>
            public Builder AddOauthScope(string scope)
            {
                if (mScopes == null) mScopes = new List<string>();
                mScopes.Add(scope);
                return this;
            }

            /// <summary>
            /// Adds the invitation delegate.  This is called when an invitation
            /// is received.
            /// </summary>
            /// <returns>The builder</returns>
            /// <param name="invitationDelegate">Invitation delegate.</param>
            public Builder WithInvitationDelegate(InvitationReceivedDelegate invitationDelegate)
            {
                this.mInvitationDelegate = Misc.CheckNotNull(invitationDelegate);
                return this;
            }

            /// <summary>
            /// Adds the match delegate.  This is called when a match notification
            /// is received.
            /// </summary>
            /// <returns>The builder.</returns>
            /// <param name="matchDelegate">Match delegate.</param>
            public Builder WithMatchDelegate(MatchDelegate matchDelegate)
            {
                this.mMatchDelegate = Misc.CheckNotNull(matchDelegate);
                return this;
            }

            /// <summary>
            /// Adds the permission rationale.  This is used only in Android
            /// when accessing the email or tokens of the player.  This is the
            /// rationale for asking for the GET_ACCOUNTS permission.
            /// </summary>
            /// <returns>The permission rationale.</returns>
            /// <param name="rationale">Rationale to display.</param>
            public Builder WithPermissionRationale(string rationale)
            {
                this.mRationale = rationale;
                return this;
            }

            /// <summary>
            /// Build this instance.
            /// </summary>
            /// <returns>the client configuration instance</returns>
            public PlayGamesClientConfiguration Build()
            {
                mRequireGooglePlus = GameInfo.RequireGooglePlus();
                return new PlayGamesClientConfiguration(this);
            }

            /// <summary>
            /// Determines whether this instance has enable save games.
            /// </summary>
            /// <returns><c>true</c> if this instance has enable save games; otherwise, <c>false</c>.</returns>
            internal bool HasEnableSaveGames()
            {
                return mEnableSaveGames;
            }

            /// <summary>
            /// Determines whether this instance has Google+ required.
            /// </summary>
            /// <returns><c>true</c> if this instance has Google+ required; otherwise, <c>false</c>.</returns>
            internal bool HasRequireGooglePlus()
            {
                return mRequireGooglePlus;
            }

            /// <summary>
            /// Gets the Oauth scopes to be requested from the user.
            /// </summary>
            /// <returns>String array of scopes.</returns>
            internal string[] getScopes() {
                return mScopes == null? new string[0] : mScopes.ToArray();
            }

            /// <summary>
            /// Gets the match delegate.
            /// </summary>
            /// <returns>The match delegate.</returns>
            internal MatchDelegate GetMatchDelegate()
            {
                return mMatchDelegate;
            }

            /// <summary>
            /// Gets the invitation delegate.
            /// </summary>
            /// <returns>The invitation delegate.</returns>
            internal InvitationReceivedDelegate GetInvitationDelegate()
            {
                return mInvitationDelegate;
            }

            /// <summary>
            /// Gets the permission rationale.
            /// </summary>
            /// <returns>The permission rationale.</returns>
            internal string GetPermissionRationale()
            {
                return mRationale;
            }
        }
    }
}
#endif
