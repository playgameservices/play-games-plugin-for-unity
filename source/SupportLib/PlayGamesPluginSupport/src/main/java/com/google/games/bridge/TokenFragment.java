/*
 * Copyright (C) Google Inc.
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

package com.google.games.bridge;

import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.Intent;
import android.util.Log;
import android.view.View;

import com.google.android.gms.auth.api.Auth;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.auth.api.signin.GoogleSignInResult;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.Scope;
import com.google.android.gms.games.Games;

/**
 * Activity fragment with no UI added to the parent activity in order to manage
 * the accessing of the player's email address and tokens.
 */
public class TokenFragment extends Fragment {

    private static final String TAG = "TokenFragment";
    private static final String FRAGMENT_TAG = "gpg.AuthTokenSupport";
    private static final int RC_ACCT = 9002;

    // Pending token request.  There can be only one outstanding request at a
    // time.
    private static final Object lock = new Object();
    private static TokenRequest pendingTokenRequest;
    private static TokenFragment helperFragment;

    private static String currentEmail;
    private static String currentAuthCode;
    private static String currentIdToken;
    private GoogleApiClient mGoogleApiClient;
    private boolean requested_authcode;
    private boolean requested_email;
    private boolean requested_id_token;

    /**
     * External entry point for getting tokens and email address.  This
     * creates the fragment if needed and queues up the request.  The fragment, once
     * active processes the list of requests.
     *
     * @param parentActivity - the parent activity to attach the fragment to.
     * @param requestAuthCode - request a server auth code to exchange on the
     *                        server for an OAuth token.
     * @param requestEmail - request the email address of the user.  This
     *                     requires the consent of the user.
     * @param requestIdToken - request an OAuth ID token for the user.  This
     *                       requires the consent of the user.
     * @param webClientId - the client id of the associated web application.
     *                    This is required when requesting auth code or id
     *                    token.  The client id must be associated with the same
     *                    client as this application.
     * @param forceRefreshToken - force refreshing the token when requesting
     *                          a server auth code.  This causes the consent
     *                          screen to be presented to the user, so it
     *                          should be used only when necessary (if at all).
     * @param additionalScopes - Additional scopes to request.  These will most
     *                         likely require the consent of the user.
     * @param hidePopups - Hides the popups during authentication and game
     *                   services events.  This done by calling
     *                   GamesOptions.setShowConnectingPopup and
     *                   GoogleAPIClient.setViewForPopups.  This is usful for
     *                   VR apps.
     * @param accountName - if non-null, the account name to use when
     *                    authenticating.

     *
     * @return PendingResult for retrieving the results when ready.
     */
    public static PendingResult fetchToken(Activity parentActivity,
                                           boolean requestAuthCode,
                                           boolean requestEmail,
                                           boolean requestIdToken,
                                           String webClientId,
                                           boolean forceRefreshToken,
                                           String[] additionalScopes,
                                           boolean hidePopups,
                                           String accountName) {
        TokenRequest request = new TokenRequest(requestAuthCode,
                requestEmail, requestIdToken, webClientId,
                forceRefreshToken, additionalScopes, hidePopups, accountName);

            // we have all the info we need, so just return.
            if (
            (!requestAuthCode || currentAuthCode != null) &&
            (!requestEmail || currentEmail != null) &&
                    (!requestIdToken || currentIdToken != null)
                    ) {
                request.setAuthCode(currentAuthCode);
                request.setEmail(currentEmail);
                request.setIdToken(currentIdToken);
                request.setResult(CommonStatusCodes.SUCCESS);
                return request.getPendingResponse();
            } else {
                boolean ok = false;
                synchronized (lock) {
                    if (pendingTokenRequest == null) {
                        pendingTokenRequest = request;
                        ok = true;
                    }
                }
                if(!ok) {
                    Log.e(TAG, "Already a pending token request!");
                    request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                }
            }

        TokenFragment fragment = (TokenFragment)
                parentActivity.getFragmentManager().findFragmentByTag(FRAGMENT_TAG);

        if (fragment == null) {
            try {
                Log.d(TAG, "Creating fragment");
                fragment = new TokenFragment();
                FragmentTransaction trans = parentActivity.getFragmentManager().beginTransaction();
                trans.add(fragment, FRAGMENT_TAG);
                trans.commit();
            } catch (Throwable th) {
                Log.e(TAG, "Cannot launch token fragment:" + th.getMessage(), th);
                request.setResult(CommonStatusCodes.ERROR);
                synchronized (lock) {
                    pendingTokenRequest = null;
                }
            }
        } else {
                    Log.d(TAG, "Fragment exists.. calling processRequests");
                    fragment.processRequest();
        }

        return request.getPendingResponse();
    }

    public static void signOut() {
        currentAuthCode = null;
        currentEmail = null;
        currentIdToken = null;
        synchronized (lock) {
            pendingTokenRequest = null;
        }
        if (helperFragment != null) {
            helperFragment.reset();
        }

    }

    /**
     * signs out and disconnects the client.
     */
    private void reset() {
        if (mGoogleApiClient != null) {
            if (mGoogleApiClient.hasConnectedApi(Games.API)) {
                Games.signOut(mGoogleApiClient);
                Auth.GoogleSignInApi.signOut(mGoogleApiClient);
            }
            mGoogleApiClient.disconnect();
            mGoogleApiClient = null;
        }
    }

    /**
     * Processes the token requests that are queued up.
     * First checking that the google api client is connected.
     */
    private void processRequest() {

        TokenRequest request;
            synchronized (lock) {
                request = pendingTokenRequest;
            }

        // no request, no need to continue.
        if (request == null) {
            return;
        }

        // Build the GoogleAPIClient
        buildClient(request);
        synchronized (lock) {
            request = pendingTokenRequest;
        }
        if (request != null) {
            // Sign-in, the result is processed in OnActivityResult()
            doAuthenticate(request);
        }

        Log.d(TAG, "Done with processRequest!");
    }

    private void buildClient(TokenRequest request) {

        // Validate that we need to rebuild the client.
        if (mGoogleApiClient == null ||
                requested_authcode != request.doAuthCode ||
                requested_email != request.doEmail ||
                requested_id_token != request.doIdToken) {


            GoogleSignInOptions.Builder builder = new GoogleSignInOptions
                    .Builder(GoogleSignInOptions.DEFAULT_GAMES_SIGN_IN);
            if (request.doAuthCode) {
                if (!request.getWebClientId().isEmpty()) {
                    builder.requestServerAuthCode(request.getWebClientId(),
                            request.getForceRefresh());
                } else {
                    Log.e(TAG, "Web client ID is needed for Auth Code");
                    request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                    synchronized (lock) {
                        pendingTokenRequest = null;
                    }
                    return;
                }
            }

            if (request.doEmail) {
                builder.requestEmail();
            }

            if (request.doIdToken) {
                if (!request.getWebClientId().isEmpty()) {
                    builder.requestIdToken(request.getWebClientId());
                } else {
                    Log.e(TAG, "Web client ID is needed for ID Token");
                    request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                    synchronized (lock) {
                        pendingTokenRequest = null;
                    }
                    return;
                }
            }
            if (request.scopes != null) {
                for (String s : request.scopes) {
                    builder.requestScopes(new Scope(s));
                }
            }

            if (request.hidePopups) {
                Log.d(TAG, "hiding popup views for games API");
                builder.addExtension(
                        Games.GamesOptions.builder().setShowConnectingPopup(false)
                                .build());
            }

            if (request.accountName != null && !request.accountName.isEmpty()) {
                builder.setAccountName(request.accountName);
            }

            requested_authcode = request.doAuthCode;
            requested_email = request.doEmail;
            requested_id_token = request.doIdToken;

            GoogleSignInOptions options = builder.build();

            GoogleApiClient.Builder clientBuilder = new GoogleApiClient.Builder(
                    getActivity())
                    .addApi(Auth.GOOGLE_SIGN_IN_API, options);
            clientBuilder.addApi(Games.API);

            if (request.hidePopups) {
                View invisible = new View(getContext());
                invisible.setVisibility(View.INVISIBLE);
                invisible.setClickable(false);
                clientBuilder.setViewForPopups(invisible);
            }
            mGoogleApiClient = clientBuilder.build();
            mGoogleApiClient.connect(GoogleApiClient.SIGN_IN_MODE_OPTIONAL);
        }
    }

    void doAuthenticate(TokenRequest request) {
        if (request == null) {
            return;
        }

        if (mGoogleApiClient == null) {
            throw new IllegalStateException("client is null!");
        }

        // check if we have all the info we need
        if ((requested_authcode && currentAuthCode == null) ||
                (requested_email && currentEmail == null) ||
                (requested_id_token && currentIdToken == null)) {
            Intent signInIntent = Auth.GoogleSignInApi.getSignInIntent(mGoogleApiClient);
            startActivityForResult(signInIntent, RC_ACCT);
        } else {
            request.setAuthCode(currentAuthCode);
            request.setEmail(currentEmail);
            request.setIdToken(currentIdToken);
            request.setResult(CommonStatusCodes.SUCCESS);
            synchronized (lock) {
                pendingTokenRequest = null;
            }
        }
    }


    /**
     * Receive the result from a previous call to
     * {@link #startActivityForResult(Intent, int)}.  This follows the
     * related Activity API as described there in
     * {@link Activity#onActivityResult(int, int, Intent)}.
     *
     * @param requestCode The integer request code originally supplied to
     *                    startActivityForResult(), allowing you to identify who this
     *                    result came from.
     * @param resultCode  The integer result code returned by the child activity
     *                    through its setResult().
     * @param data        An Intent, which can return result data to the caller
     */
    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == RC_ACCT) {
            GoogleSignInResult result =
                    Auth.GoogleSignInApi.getSignInResultFromIntent(data);
            TokenRequest request;
            synchronized (lock) {
                request = pendingTokenRequest;
                pendingTokenRequest = null;
            }
            GoogleSignInAccount acct = result.getSignInAccount();
                if (request != null) {
                    if (acct != null) {
                        request.setAuthCode(acct.getServerAuthCode());
                        request.setEmail(acct.getEmail());
                        request.setIdToken(acct.getIdToken());
                        currentAuthCode = acct.getServerAuthCode();
                        currentEmail = acct.getEmail();
                        currentIdToken = acct.getIdToken();
                    }
                    request.setResult(result.getStatus().getStatusCode());
                }
            return;
        }
        super.onActivityResult(requestCode, resultCode, data);
    }


    @Override
    public void onStart() {
        Log.d(TAG, "onStart()");
        super.onStart();

        // This just connects the client.  If there is no user signed in, you
        // still need to call Auth.GoogleSignInApi.getSignInIntent() to start
        // the sign-in process.
        if (mGoogleApiClient != null) {
            mGoogleApiClient.connect(GoogleApiClient.SIGN_IN_MODE_OPTIONAL);
        }
    }

    @Override
    public void onStop() {
        Log.d(TAG, "onStop()");
        super.onStop();
        if (mGoogleApiClient != null && mGoogleApiClient.isConnected()) {
            mGoogleApiClient.disconnect();
        }
    }

    /**
     * Called when the fragment is visible to the user and actively running.
     * This is generally
     * tied to {@link Activity#onResume() Activity.onResume} of the containing
     * Activity's lifecycle.
     */
    @Override
    public void onResume() {
        Log.d(TAG, "onResume called");
        super.onResume();
        if (helperFragment == null) {
            helperFragment = this;
        }
            processRequest();
    }

    /**
     * Helper class containing the request for information.
     */
    private static class TokenRequest {
        private TokenPendingResult pendingResponse;
        private boolean doAuthCode;
        private boolean doEmail;
        private boolean doIdToken;
        private String webClientId;
        private boolean forceRefresh;
        private boolean hidePopups;
        private String accountName;
        private String[] scopes;

        public TokenRequest(boolean fetchAuthCode, boolean fetchEmail,
                            boolean fetchIdToken, String webClientId, boolean
                            forceRefresh, String[] oAuthScopes,
                            boolean hidePopups, String accountName) {
            pendingResponse = new TokenPendingResult();
            doAuthCode = fetchAuthCode;
            doEmail = fetchEmail;
            doIdToken = fetchIdToken;
            this.webClientId =webClientId;
            this.forceRefresh = forceRefresh;
            if(oAuthScopes != null && oAuthScopes.length > 0) {
                scopes = new String[oAuthScopes.length];
                System.arraycopy(oAuthScopes,0,scopes,0,oAuthScopes.length);
            } else {
                scopes = null;
            }
            this.hidePopups = hidePopups;
            this.accountName = accountName;
        }

        public PendingResult<TokenResult> getPendingResponse() {
            return pendingResponse;
        }

        public void setResult(int code) {
            pendingResponse.setStatus(code);
        }

        public void setEmail(String email) {
            pendingResponse.setEmail(email);
        }

        public void cancel() {
            pendingResponse.cancel();
        }

        public void setAuthCode(String authCode) {
            pendingResponse.setAuthCode(authCode);
        }

        public void setIdToken(String idToken) {
            pendingResponse.setIdToken(idToken);
        }

        public String getEmail() {
            return pendingResponse.result.getEmail();
        }

        public String getIdToken() {
            return pendingResponse.result.getIdToken();
        }

        public String getAuthCode() {
            return pendingResponse.result.getAuthCode();
        }

        @Override
        public String toString() {
            return Integer.toHexString(hashCode()) + " (a:" +
                    doAuthCode + " e:" + doEmail + " i:" + doIdToken +
                    ")";
        }

        public String getWebClientId() {
            return webClientId==null?"":webClientId;
        }

        public boolean getForceRefresh() {
            return forceRefresh;
        }
    }
}
