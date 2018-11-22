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
#if UNITY_ANDROID

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
           .Build();
        /// <summary>
        /// Flag indicating to enable saved games API.
        /// </summary>
        private readonly bool mEnableSavedGames;

        /// <summary>
        /// Array of scopes to be requested from user. None is considered as 'games_lite'.
        /// </summary>
        private readonly string[] mScopes;

        /// <summary>
        /// The flag to indicate a server auth code should be requested when authenticating.
        /// </summary>
        private readonly bool mRequestAuthCode;

        /// <summary>
        /// The flag indicating the auth code should be refresh, causing re-consent and issuing a new refresh token.
        /// </summary>
        private readonly bool mForceRefresh;

        /// <summary>
        /// The flag indicating popup UIs should be hidden.
        /// </summary>
        private readonly bool mHidePopups;

        /// <summary>
        /// The flag indicating the email address should returned when authenticating.
        /// </summary>
        private readonly bool mRequestEmail;

        /// <summary>
        /// The flag indicating the id token should be returned when authenticating.
        /// </summary>
        private readonly bool mRequestIdToken;

        /// <summary>
        /// The account name to attempt to use when signing in.  Null indicates use the default.
        /// </summary>
        private readonly string mAccountName;

        /// <summary>
        /// The invitation delegate.
        /// </summary>
        private readonly InvitationReceivedDelegate mInvitationDelegate;

        /// <summary>
        /// The match delegate.
        /// </summary>
        private readonly MatchDelegate mMatchDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="GooglePlayGames.BasicApi.PlayGamesClientConfiguration"/> struct.
        /// </summary>
        /// <param name="builder">Builder for this configuration.</param>
        private PlayGamesClientConfiguration(Builder builder)
        {
            this.mEnableSavedGames = builder.HasEnableSaveGames();
            this.mInvitationDelegate = builder.GetInvitationDelegate();
            this.mMatchDelegate = builder.GetMatchDelegate();
            this.mScopes = builder.getScopes();
            this.mHidePopups = builder.IsHidingPopups();
            this.mRequestAuthCode = builder.IsRequestingAuthCode();
            this.mForceRefresh = builder.IsForcingRefresh();
            this.mRequestEmail = builder.IsRequestingEmail();
            this.mRequestIdToken = builder.IsRequestingIdToken();
            this.mAccountName = builder.GetAccountName();
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

        public bool IsHidingPopups
        {
            get
            {
                return mHidePopups;
            }
        }

        public bool IsRequestingAuthCode
        {
            get
            {
                return mRequestAuthCode;
            }
        }

        public bool IsForcingRefresh
        {
            get
            {
                return mForceRefresh;
            }
        }

        public bool IsRequestingEmail
        {
            get
            {
                return mRequestEmail;
            }
        }

        public bool IsRequestingIdToken
        {
            get
            {
                return mRequestIdToken;
            }
        }

        public string AccountName
        {
            get
            {
                return mAccountName;
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
        /// Builder class for the configuration.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// The flag to enable save games. Default is false.
            /// </summary>
            private bool mEnableSaveGames = false;

            /// <summary>
            /// The scopes to request from the user. Default is none.
            /// </summary>
            private List<string> mScopes = null;

            /// <summary>
            /// The flag indicating that popup UI should be hidden.
            /// </summary>
            private bool mHidePopups = false;

            /// <summary>
            /// The flag to indicate a server auth code should be requested when authenticating.
            /// </summary>
            private bool mRequestAuthCode = false;

            /// <summary>
            /// The flag indicating the auth code should be refresh, causing re-consent and issuing a new refresh token.
            /// </summary>
            private bool mForceRefresh = false;

            /// <summary>
            /// The flag indicating the email address should returned when authenticating.
            /// </summary>
            private bool mRequestEmail = false;

            /// <summary>
            /// The flag indicating the id token should be returned when authenticating.
            /// </summary>
            private bool mRequestIdToken = false;

            /// <summary>
            /// The account name to use as a default when authenticating.
            /// </summary>
            /// <remarks>
            /// This is only used when requesting auth code or id token.
            /// </remarks>
            private string mAccountName = null;

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
            /// Enables the saved games.
            /// </summary>
            /// <returns>The builder.</returns>
            public Builder EnableSavedGames()
            {
                mEnableSaveGames = true;
                return this;
            }

            /// <summary>
            /// Enables hiding popups.  This is recommended for VR apps.
            /// </summary>
            /// <returns>The hide popups.</returns>
            public Builder EnableHidePopups()
            {
                mHidePopups = true;
                return this;
            }

            public Builder RequestServerAuthCode(bool forceRefresh)
            {
              mRequestAuthCode = true;
              mForceRefresh = forceRefresh;
              return this;
            }

            public Builder RequestEmail()
            {
                mRequestEmail = true;
                return this;
            }

            public Builder RequestIdToken()
            {
                mRequestIdToken = true;
                return this;
            }

            public Builder SetAccountName(string accountName)
            {
                mAccountName = accountName;
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
            /// Build this instance.
            /// </summary>
            /// <returns>the client configuration instance</returns>
            public PlayGamesClientConfiguration Build()
            {
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

            internal bool IsRequestingAuthCode()
            {
                return mRequestAuthCode;
            }

            internal bool IsHidingPopups()
            {
                return mHidePopups;
            }

            internal bool IsForcingRefresh()
            {
                return mForceRefresh;
            }

            internal bool IsRequestingEmail()
            {
                return mRequestEmail;
            }

            internal bool IsRequestingIdToken()
            {
                return mRequestIdToken;
            }

            internal string GetAccountName()
            {
                return mAccountName;
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

        }
    }
}
#endif
