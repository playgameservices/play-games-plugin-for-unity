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
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.annotation.Nullable;
import android.util.Log;
import android.view.View;

import com.google.android.gms.auth.api.Auth;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.auth.api.signin.GoogleSignInResult;
import com.google.android.gms.auth.api.signin.GoogleSignInStatusCodes;
import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.ResultCallback;
import com.google.android.gms.common.api.Scope;
import com.google.android.gms.games.Games;


/**
 * Activity fragment with no UI added to the parent activity in order to manage
 * the accessing of the player's email address and tokens.
 */
public class TokenFragment extends Fragment
        implements GoogleApiClient.ConnectionCallbacks,
        GoogleApiClient.OnConnectionFailedListener
{

    private static final String TAG = "TokenFragment";
    private static final String FRAGMENT_TAG = "gpg.AuthTokenSupport";
    private static final int RC_ACCT = 9002;

    private static final String PREF_DECLINED_KEY = TAG + ".userDeclined";

    // Pending token request.  There can be only one outstanding request at a
    // time.
    private static final Object lock = new Object();
    private static TokenRequest pendingTokenRequest;
    private static TokenFragment helperFragment;
    private GoogleApiClient mGoogleApiClient;

    private static boolean mStartUpSignInCheckPerformed = false;
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

                boolean ok = false;
                synchronized (lock) {
                    if (pendingTokenRequest == null) {
                        pendingTokenRequest = request;
                        ok = true;
                    }
                }
                if(!ok) {
                    Log.e(TAG, "Already a pending token request (requested == ): " + request);
                    Log.e(TAG, "Already a pending token request: " + pendingTokenRequest);
                    request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                    return request.getPendingResponse();
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

    /**
     * This calls silent signin and gets the user info including the auth code.
     * If silent sign-in fails, the failure is returned.
     * @return PendingResult for waiting on result.
     */
    public static PendingResult getAnotherAuthCode(Activity parentActivity,
                                                   final boolean reauthIfNeeded,
                                                   String webClientId) {

        TokenRequest request = new TokenRequest(true,
                true, true, webClientId,
                false, null, true, null);


        final TokenFragment fragment = (TokenFragment)
                parentActivity.getFragmentManager().findFragmentByTag(FRAGMENT_TAG);
        if (fragment == null) {
            // The fragment should already be here, so return an error.
            Log.e(TAG,"Fragment is not found.  Could not be authenticated already?");
            request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
        } else {
            if (fragment.mGoogleApiClient != null &&
                    fragment.mGoogleApiClient.hasConnectedApi(Games.API))  {

                boolean ok = false;
                synchronized (lock) {
                    if (pendingTokenRequest == null) {
                        pendingTokenRequest = request;
                        ok = true;
                    }
                }
                if(!ok) {
                    Log.e(TAG, "Already a pending token request (requested == ): " + request);
                    Log.e(TAG, "Already a pending token request: " + pendingTokenRequest);
                    request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                    return request.getPendingResponse();
                }

                Auth.GoogleSignInApi.silentSignIn(fragment.mGoogleApiClient)
                        .setResultCallback(
                        new ResultCallback<GoogleSignInResult>() {
                            @Override
                            public void onResult(
                                    @NonNull GoogleSignInResult googleSignInResult) {
                                if (googleSignInResult.isSuccess()) {
                                    fragment.onSignedIn(googleSignInResult.getStatus().getStatusCode(),
                                            googleSignInResult.getSignInAccount());
                                } else if (
                                        googleSignInResult.getStatus().getStatusCode() == CommonStatusCodes.SIGN_IN_REQUIRED
                                        && reauthIfNeeded) {
                                    Intent signInIntent = Auth.GoogleSignInApi
                                            .getSignInIntent(fragment.mGoogleApiClient);
                                    fragment.startActivityForResult(signInIntent, RC_ACCT);
                                } else {
                                    Log.e(TAG,"Error with " +
                                            "silentSignIn: " +
                                            googleSignInResult.getStatus());
                                    fragment.onSignedIn(googleSignInResult.getStatus().getStatusCode(),
                                            null);
                                }
                            }
                        }
                );
            } else {
                Log.d(TAG,"No connected Games API, waiting for onConnected");
            }
        }

        return request.getPendingResponse();
    }

    public static void signOut(Activity activity) {

        TokenFragment fragment = (TokenFragment)
                activity.getFragmentManager().findFragmentByTag(FRAGMENT_TAG);
        if (fragment != null) {
            fragment.reset();
        }
        synchronized (lock) {
            pendingTokenRequest = null;
        }
   }

    /**
     * signs out and disconnects the client.
     */
    private void reset() {
        if (mGoogleApiClient != null) {
            if (mGoogleApiClient.hasConnectedApi(Games.API)) {
                try {
                    Games.signOut(mGoogleApiClient);
                } catch (RuntimeException e) {
                    Log.w(TAG, "Caught exception when calling Games.signOut: " +
                    e.getMessage(), e);
                }
                try {
                Auth.GoogleSignInApi.signOut(mGoogleApiClient);
                } catch (RuntimeException e) {
                    Log.w(TAG, "Caught exception when calling GoogleSignInAPI.signOut: " +
                            e.getMessage(), e);
                }
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

            boolean signIn = true;
            if (!mStartUpSignInCheckPerformed) {
                mStartUpSignInCheckPerformed = true;
                SharedPreferences sharedPref = getActivity().getPreferences(Context.MODE_PRIVATE);
                signIn = !sharedPref.getBoolean(PREF_DECLINED_KEY, false);
            }

            if (signIn || mGoogleApiClient.hasConnectedApi(Games.API))  {

                Auth.GoogleSignInApi.silentSignIn(mGoogleApiClient).setResultCallback(
                        new ResultCallback<GoogleSignInResult>() {
                            @Override
                            public void onResult(
                                    @NonNull GoogleSignInResult googleSignInResult) {
                                if (googleSignInResult.isSuccess()) {
                                    onSignedIn(googleSignInResult.getStatus().getStatusCode(),
                                            googleSignInResult.getSignInAccount());
                                } else if (googleSignInResult.getStatus().getStatusCode() == CommonStatusCodes.SIGN_IN_REQUIRED) {
                                    Intent signInIntent = Auth.GoogleSignInApi
                                            .getSignInIntent(mGoogleApiClient);
                                    startActivityForResult(signInIntent, RC_ACCT);
                                } else {
                                    Log.e(TAG,"Error with " +
                                            "silentSignIn: " +
                                            googleSignInResult.getStatus());
                                    onSignedIn(googleSignInResult.getStatus().getStatusCode(),
                                            null);
                                }
                            }
                        }
                );
            } else {
                Log.d(TAG,"No connected Games API");
                onSignedIn(CommonStatusCodes.ERROR, null);
            }
        }

        Log.d(TAG, "Done with processRequest, result is pending.");
    }

    private void buildClient(TokenRequest request) {


        Log.d(TAG,"Building client for: " + request);
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


            GoogleSignInOptions options = builder.build();

            GoogleApiClient.Builder clientBuilder = new GoogleApiClient.Builder(
                    getActivity())
                    .addApi(Auth.GOOGLE_SIGN_IN_API, options);
            clientBuilder.addApi(Games.API);

            clientBuilder.addConnectionCallbacks(this)
                    .addOnConnectionFailedListener(this);

            if (request.hidePopups) {
                View invisible = new View(getActivity());
                invisible.setVisibility(View.INVISIBLE);
                invisible.setClickable(false);
                clientBuilder.setViewForPopups(invisible);
            }
            mGoogleApiClient = clientBuilder.build();
            mGoogleApiClient.connect(GoogleApiClient.SIGN_IN_MODE_OPTIONAL);

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
            if (result != null && result.isSuccess()) {
                GoogleSignInAccount acct =  result.getSignInAccount();
                onSignedIn(result.getStatus().getStatusCode(), acct);
            } else if (resultCode == Activity.RESULT_CANCELED) {
                onSignedIn(CommonStatusCodes.CANCELED, null);
            } else if (result != null) {
                Log.e(TAG,"GoogleSignInResult error: " + result.getStatus());
                onSignedIn(result.getStatus().getStatusCode(), null);
            } else {
                Log.e(TAG, "Google SignIn Result is null, resultCode is " +
                        resultCode + "(" +
                GoogleSignInStatusCodes.getStatusCodeString(resultCode) + ")");
                onSignedIn(CommonStatusCodes.ERROR, null);
            }
            return;
        }
        super.onActivityResult(requestCode, resultCode, data);
    }

    private void onSignedIn(int resultCode, GoogleSignInAccount acct) {

        if (resultCode == CommonStatusCodes.CANCELED) {
            if (pendingTokenRequest != null)
            {
                pendingTokenRequest.cancel();
            }
            SaveDeclinedSignInPreference(true);
        }

        TokenRequest request;
        synchronized (lock) {
            request = pendingTokenRequest;
            pendingTokenRequest = null;
        }
        if (request != null) {
            if (acct != null) {
                SaveDeclinedSignInPreference(false);
                request.setAuthCode(acct.getServerAuthCode());
                request.setEmail(acct.getEmail());
                request.setIdToken(acct.getIdToken());
            }
            Log.e(TAG,"Setting result error code to: " + resultCode);
            request.setResult(resultCode);
        }
    }

    private void SaveDeclinedSignInPreference(boolean declined) {
        SharedPreferences sharedPref = getActivity().getPreferences(Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPref.edit();
        editor.putBoolean(PREF_DECLINED_KEY, declined);
        editor.commit();
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

    @Override
    public void onConnected(@Nullable final Bundle bundle) {
        Log.i(TAG,"onConnected called");
        if (mGoogleApiClient == null) {
            return;
        }
        if (mGoogleApiClient.hasConnectedApi(Games.API)) {
            Auth.GoogleSignInApi.silentSignIn(mGoogleApiClient).setResultCallback(
                    new ResultCallback<GoogleSignInResult>() {
                        @Override
                        public void onResult(
                                @NonNull GoogleSignInResult googleSignInResult) {
                            if (googleSignInResult.isSuccess()) {
                                onSignedIn(
                                        googleSignInResult.getStatus()
                                                .getStatusCode(),
                                        googleSignInResult.getSignInAccount());
                            } else {
                                Log.e(TAG, "Error with silentSignIn when connected: " +
                                        googleSignInResult.getStatus());
                                onSignedIn(googleSignInResult.getStatus()
                                        .getStatusCode(),googleSignInResult.getSignInAccount());
                            }
                        }
                    }
            );
        }
    }

    /**
     * Does nothing, but the interface requires an implementation.  A typical
     * application would disable any UI that requires a connection.  When the
     * connection is restored, onConnected will be called.
     *
     * @param cause - The reason of the disconnection.
     */
    @Override
    public void onConnectionSuspended(int cause) {
        Log.d(TAG, "onConnectionSuspended() called: " + cause);
    }

    @Override
    public void onConnectionFailed(@NonNull ConnectionResult connectionResult) {
        Log.e(TAG,"onConnectionFailed: " + connectionResult.getErrorCode() +
               ": " + connectionResult.getErrorMessage());
        if (connectionResult.hasResolution()) {
            // Just start the intent, it will be easier.
            Intent signInIntent = Auth.GoogleSignInApi
                    .getSignInIntent(mGoogleApiClient);
            startActivityForResult(signInIntent, RC_ACCT);
        } else {
            onSignedIn(connectionResult.getErrorCode(), null);
        }
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
                    " wc: " + webClientId + " f: " + forceRefresh +")";
        }

        public String getWebClientId() {
            return webClientId==null?"":webClientId;
        }

        public boolean getForceRefresh() {
            return forceRefresh;
        }
    }

    public static boolean checkGooglePlayServicesAvailable() {
        GooglePlayServicesUtil.isGooglePlayServicesAvailable(null);
        return false;
    }
}
