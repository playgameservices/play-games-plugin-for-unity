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

import com.google.android.gms.auth.api.Auth;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.auth.api.signin.GoogleSignInResult;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.Scope;

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
    private static TokenRequest pendingTokenRequest;

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
     * @param parentActivity   - the activity to attach the fragment to.
     * @param fetchServerAuthCode - true indicates get the serverAuthCode.
     * @param fetchEmail       - true indicates get the email.
     * @param fetchIdToken     - true indicates get the id token.
     * @param webClientId      - web client Id needed for authcode and
     *                         id token.
     * @param forceRefresh     - force refresh of auth code and refresh token.
     *
     * @return PendingResult for retrieving the results when ready.
     */
    public static PendingResult fetchToken(Activity parentActivity,
                                           boolean fetchServerAuthCode,
                                           boolean fetchEmail,
                                           boolean fetchIdToken,
                                           String webClientId,
                                           boolean forceRefresh,
                                           String[] oauthScopes) {
        TokenRequest request = new TokenRequest(fetchServerAuthCode,
                fetchEmail, fetchIdToken, webClientId, forceRefresh, oauthScopes);

            // we have all the info we need, so just return.
            if (
            (!fetchServerAuthCode || currentAuthCode != null) &&
            (!fetchEmail || currentEmail != null) &&
                    (!fetchIdToken || currentIdToken != null)
                    ) {
                request.setAuthCode(currentAuthCode);
                request.setEmail(currentEmail);
                request.setIdToken(currentIdToken);
                request.setResult(CommonStatusCodes.SUCCESS);
                return request.getPendingResponse();
            } else {
                boolean ok = false;
                synchronized (TAG) {
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
                synchronized (TAG) {
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
        synchronized (TAG) {
            pendingTokenRequest = null;
        }
    }

    /**
     * Processes the token requests that are queued up.
     * First checking that the google api client is connected.
     */
    private void processRequest() {

        TokenRequest request;
            synchronized (TAG) {
                request = pendingTokenRequest;
            }

        // no request, no need to continue.
        if (request == null) {
            return;
        }

        // Build the GoogleAPIClient
        buildClient(request);
        synchronized (TAG) {
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
                    Log.e(TAG,"Web client ID is needed for Auth Code");
                    request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                    synchronized (TAG) {
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
                    Log.e(TAG,"Web client ID is needed for ID Token");
                    request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                    synchronized (TAG) {
                        pendingTokenRequest = null;
                    }
                    return;
                }
            }
            if (request.scopes != null) {
                for(String s: request.scopes) {
                    builder.requestScopes(new Scope(s));
                }
            }

            requested_authcode = request.doAuthCode;
            requested_email = request.doEmail;
            requested_id_token = request.doIdToken;

            GoogleSignInOptions options = builder.build();

            mGoogleApiClient = new GoogleApiClient.Builder(getContext())
                    .addApi(Auth.GOOGLE_SIGN_IN_API, options)
                    .build();
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
            synchronized (TAG) {
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
            synchronized (TAG) {
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
        private String[] scopes;

        public TokenRequest(boolean fetchAuthCode, boolean fetchEmail,
                            boolean fetchIdToken, String webClientId, boolean
                            forceRefresh, String[] oAuthScopes) {
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
        }

        public PendingResult getPendingResponse() {
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
