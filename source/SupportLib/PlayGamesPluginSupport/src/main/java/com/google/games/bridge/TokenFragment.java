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
import androidx.annotation.NonNull;
import com.google.android.gms.auth.api.Auth;
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInClient;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.auth.api.signin.GoogleSignInResult;
import com.google.android.gms.auth.api.signin.GoogleSignInStatusCodes;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.Scope;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.GamesClient;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;

/**
 * Activity fragment with no UI added to the parent activity in order to manage
 * the accessing of the player's email address and tokens.
 */
public class TokenFragment extends Fragment
{

    private static final String TAG = "TokenFragment";
    private static final String FRAGMENT_TAG = "gpg.AuthTokenSupport";
    private static final int RC_ACCT = 9002;

    // Pending token request.  There can be only one outstanding request at a
    // time.
    private static final Object lock = new Object();
    private static TokenRequest pendingTokenRequest;
    private static TokenFragment helperFragment;
    private GoogleSignInClient mGoogleSignInClient;

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
     *                   GamesOptions.setShowConnectingPopup. This is usful for
     *                   VR apps.
     * @param accountName - if non-null, the account name to use when
     *                    authenticating.

     *
     * @return PendingResult for retrieving the results when ready.
     */
    public static PendingResult fetchToken(Activity parentActivity,
                                           boolean silent,
                                           boolean requestAuthCode,
                                           boolean requestEmail,
                                           boolean requestIdToken,
                                           String webClientId,
                                           boolean forceRefreshToken,
                                           String[] additionalScopes,
                                           boolean hidePopups,
                                           String accountName) {
        TokenRequest request = new TokenRequest(silent,
                requestAuthCode, requestEmail, requestIdToken, webClientId,
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
        if (mGoogleSignInClient != null) {
           mGoogleSignInClient.signOut();
           mGoogleSignInClient = null;   
        }
    }

    private void signIn() {
        Log.d(TAG, "signIn");
        
        TokenRequest request_;
        synchronized (lock) {
            request_ = pendingTokenRequest;
        }
        final TokenRequest request = request_;
        final GoogleSignInClient signInClient = mGoogleSignInClient;

        if (signInClient != null && request != null) {
            if (request.canReuseAccount()) { 
                final GoogleSignInAccount account = GoogleSignIn.getLastSignedInAccount(getActivity());
                if (GoogleSignIn.hasPermissions(account, request.scopes)) {
                    Log.d(TAG, "Checking the last signed-in account if it can be used.");
                    Games.getGamesClient(getActivity(), account).getAppId()
                        .addOnCompleteListener(new OnCompleteListener<String>() {
                            @Override
                            public void onComplete(@NonNull Task<String> task) {
                                if (task.isSuccessful()) {
                                    Log.d(TAG, "Signed-in with the last signed-in account.");
                                    onSignedIn(CommonStatusCodes.SUCCESS, account);
                                } else {
                                    signInClient.signOut().addOnCompleteListener(
                                        new OnCompleteListener<Void>() {
                                            @Override
                                            public void onComplete(@NonNull Task<Void> task) {
                                                if (task.isSuccessful()) {
                                                    Log.d(TAG, "Can't reuse the last signed-in account. Second attempt after sign out.");
                                                    // Last signed account should be null now
                                                    signIn();
                                                } else {
                                                    Log.e(TAG, "Can't reuse the last signed-in account and sign out failed.");
                                                    onSignedIn(CommonStatusCodes.SIGN_IN_REQUIRED, null);
                                                }
                                            }
                                        });
                                }
                            }
                        });
                    return;
                }
            }

            Log.d(TAG, "signInClient.silentSignIn");
            signInClient.silentSignIn()
                .addOnSuccessListener(
                    getActivity(),
                    new OnSuccessListener<GoogleSignInAccount>() {
                        @Override
                        public void onSuccess(GoogleSignInAccount result) {
                            Log.d(TAG, "silentSignIn.onSuccess");
                            onSignedIn(CommonStatusCodes.SUCCESS, result);
                        }
                    })
                .addOnFailureListener(
                    getActivity(),
                    new OnFailureListener() {
                        @Override
                        public void onFailure(Exception exception) {
                            Log.d(TAG, "silentSignIn.onFailure");
                            int statusCode = CommonStatusCodes.INTERNAL_ERROR;
                            if (exception instanceof ApiException) {
                                statusCode = ((ApiException) exception).getStatusCode();
                            }
                            // INTERNAL_ERROR in some PlayServices versions will be returned instead RESOLUTION_REQUIRED 
                            // if the user has the outdated PlayServices or Play Games app is not installed
                            if (statusCode == CommonStatusCodes.SIGN_IN_REQUIRED 
                                    || statusCode == CommonStatusCodes.INTERNAL_ERROR
                                    || statusCode == CommonStatusCodes.RESOLUTION_REQUIRED) {
                                if (!request.getSilent()) {
                                    Intent signInIntent = signInClient.getSignInIntent();
                                    startActivityForResult(signInIntent, RC_ACCT);
                                } else {
                                    Log.i(TAG, "Sign-in failed. Run in silent mode and UI sign-in required.");
                                    onSignedIn(CommonStatusCodes.SIGN_IN_REQUIRED, null);
                                }
                            } else {
                                Log.e(TAG, "Sign-in failed with status code: " + statusCode);
                                onSignedIn(statusCode, null);
                            }
                        }
                    });
        }
    }

    /**
     * Processes the token requests that are queued up.
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

        // Build the GoogleSignInClient
        if (buildClient(getActivity(), request)) {
            signIn();
        } else {
            synchronized (lock) {
                pendingTokenRequest = null;
            }
        }
        Log.d(TAG, "Done with processRequest, result is pending.");
    }

    private boolean buildClient(Activity activity, TokenRequest request) {
        Log.d(TAG,"Building client for: " + request);
        GoogleSignInOptions.Builder builder = new GoogleSignInOptions.Builder();
        if (request.doAuthCode) {
            if (!request.getWebClientId().isEmpty() && !request.getWebClientId().equals("__WEB_CLIENTID__")) {
                builder.requestServerAuthCode(request.getWebClientId(),
                        request.getForceRefresh());
            } else {
                Log.e(TAG, "Web client ID is needed for Auth Code");
                request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                return false;
            }
        }

        if (request.doEmail) {
            builder.requestEmail();
        }

        if (request.doIdToken) {
            if (!request.getWebClientId().isEmpty() && !request.getWebClientId().equals("__WEB_CLIENTID__")) {
                builder.requestIdToken(request.getWebClientId());
            } else {
                Log.e(TAG, "Web client ID is needed for ID Token");
                request.setResult(CommonStatusCodes.DEVELOPER_ERROR);
                return false;
            }
        }
        if (request.scopes != null) {
            for (Scope s : request.scopes) {
                builder.requestScopes(s);
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
        mGoogleSignInClient = GoogleSignIn.getClient(activity, options);
        return true;
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
                GoogleSignInAccount account = result.getSignInAccount();
                onSignedIn(result.getStatus().getStatusCode(), account);
            } else if (resultCode == Activity.RESULT_CANCELED) {
                onSignedIn(CommonStatusCodes.CANCELED, null);
            } else if (result != null) {
                Log.e(TAG,"GoogleSignInResult error status code: " + result.getStatus());
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

    private void onSignedIn(int statusCode, GoogleSignInAccount acct) {
        TokenRequest request;
        synchronized (lock) {
            request = pendingTokenRequest;
            pendingTokenRequest = null;
        }

        if (request != null) {
            if (acct != null) {
                request.setAuthCode(acct.getServerAuthCode());
                request.setEmail(acct.getEmail());
                request.setIdToken(acct.getIdToken());
            }
            if (statusCode != CommonStatusCodes.SUCCESS) {
                Log.e(TAG,"Setting result error status code to: " + statusCode);
            }
            request.setResult(statusCode);
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
        private boolean silent;
        private boolean doAuthCode;
        private boolean doEmail;
        private boolean doIdToken;
        private String webClientId;
        private boolean forceRefresh;
        private boolean hidePopups;
        private String accountName;
        private Scope[] scopes;

        public TokenRequest(boolean silent, boolean fetchAuthCode, boolean fetchEmail,
                            boolean fetchIdToken, String webClientId, boolean
                            forceRefresh, String[] oAuthScopes,
                            boolean hidePopups, String accountName) {
            pendingResponse = new TokenPendingResult();
            this.silent = silent;
            doAuthCode = fetchAuthCode;
            doEmail = fetchEmail;
            doIdToken = fetchIdToken;
            this.webClientId = webClientId;
            this.forceRefresh = forceRefresh;
            if(oAuthScopes != null && oAuthScopes.length > 0) {
                scopes = new Scope[oAuthScopes.length];
                for (int i = 0; i < oAuthScopes.length; ++i) {
                    scopes[i] = new Scope(oAuthScopes[i]);
                }
            } else {
                scopes = null;
            }
            this.hidePopups = hidePopups;
            this.accountName = accountName;
        }

        public boolean canReuseAccount() {
            return !doAuthCode && !doIdToken;
        }

        public PendingResult<TokenResult> getPendingResponse() {
            return pendingResponse;
        }
        public boolean getSilent() {
            return silent;
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

    public static View createInvisibleView(Activity parentActivity) {
        View view = new View(parentActivity);
        view.setVisibility(View.INVISIBLE);
        view.setClickable(false);
        return view;
    }
}
