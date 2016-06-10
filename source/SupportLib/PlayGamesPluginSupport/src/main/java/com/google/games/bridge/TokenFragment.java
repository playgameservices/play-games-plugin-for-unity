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

import android.accounts.AccountManager;
import android.annotation.TargetApi;
import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Build;
import android.util.Log;

import com.google.android.gms.auth.GoogleAuthUtil;
import com.google.android.gms.common.AccountPicker;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.PendingResult;

import java.util.ArrayList;
import java.util.List;

/**
 * Activity fragment with no UI added to the parent activity in order to manage
 * the accessing of the player's email address and tokens.
 */
@TargetApi(Build.VERSION_CODES.HONEYCOMB)
public class TokenFragment extends Fragment {

    private static final String TAG = "TokenFragment";
    private static final String FRAGMENT_TAG = "gpg.TokenSupport";
    private static final int RC_ACCT = 9002;

    // map of pending results used to pass back information to the caller.
    // the key is a unique key added to the intent extras so the bridge activity
    // can find the result to respond.
    private static final List<TokenRequest> pendingTokenRequests = new ArrayList<>();

    private static String selectedAccountName;
    private static String currentPlayerId;
    private static boolean mIsSelecting = false;

    /**
     * External entry point for getting tokens and email address.  This
     * creates the fragment if needed and queues up the request.  The fragment, once
     * active processes the list of requests.
     *
     * @param parentActivity   - the activity to attach the fragment to.
     * @param playerId         - the authenticated player id.  This is used to detect
     *                         switching players.
     * @param rationale        - the rationale to display when requesting permission.
     * @param fetchEmail       - true indicates get the email.
     * @param fetchAccessToken - true indicates get the access token.
     * @param fetchIdToken     - true indicates get the id token.
     * @param scope            - the scope for getting the id token.
     * @return PendingResult for retrieving the results when ready.
     */
    public static PendingResult fetchToken(Activity parentActivity,
                                           String playerId,
                                           String rationale, boolean fetchEmail,
                                           boolean fetchAccessToken, boolean fetchIdToken, String scope) {
        TokenRequest request = new TokenRequest(fetchEmail, fetchAccessToken, fetchIdToken, scope);
        request.setRationale(rationale);

        synchronized (pendingTokenRequests) {
            if (playerId == null || !playerId.equals(currentPlayerId)) {
                currentPlayerId = playerId;
                selectedAccountName = null;
            }

            // see if we can short circuit this process if the player already selected an account.
            if (selectedAccountName != null && fetchEmail && !fetchAccessToken && !fetchIdToken) {
                Log.i(TAG, "Returning accountName: " + selectedAccountName);
                request.setEmail(selectedAccountName);
                request.setResult(CommonStatusCodes.SUCCESS);
                return request.getPendingResponse();
            } else {
                pendingTokenRequests.add(request);
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
                synchronized (pendingTokenRequests) {
                    pendingTokenRequests.remove(request);
                }
            }
        } else {
            synchronized (pendingTokenRequests) {
                if (!mIsSelecting) {
                    Log.d(TAG, "Fragment exists.. and not selecting calling processRequests");
                    fragment.processRequests(CommonStatusCodes.SUCCESS);
                }
            }
        }

        return request.getPendingResponse();
    }

    /**
     * Processes the token requests that are queued up.
     * First checking that the google api client is connected.
     * If the error code is not SUCCESS, then all the requests in the queue
     * are failed using the given error code.
     *
     * @param errorCode - if not SUCCESS, all requests are failed using this code.
     */
    private void processRequests(int errorCode) {

        TokenRequest request = null;
        String acctName;

        // First, if the error code is set, fail all the pending calls
        if (errorCode != CommonStatusCodes.SUCCESS) {
            synchronized (pendingTokenRequests) {
                while (!pendingTokenRequests.isEmpty()) {
                    request = pendingTokenRequests.remove(0);
                    request.setResult(errorCode);
                }
            }
            return;
        }

        // Next, see if we still need to select an account.  We do this in the
        // synchronized block so we don't miss a callback from a previous call.
        synchronized (pendingTokenRequests) {
            if (!pendingTokenRequests.isEmpty()) {
                request = pendingTokenRequests.get(0);
            }
            acctName = selectedAccountName;
        }

        // no request, no need to continue.
        if (request == null) {
            return;
        }

        // If the accountName is null, then prompt the user to select one.
        if (acctName == null) {
            // set the flag so we don't prompt twice in a row.
            synchronized (pendingTokenRequests) {
                mIsSelecting = true;
            }

            // build the intent to get the account
            Intent intent = AccountPicker.newChooseAccountIntent(null, null, new String[]{"com.google"},
                    true, request.getRationale(), null, null, null);
            startActivityForResult(intent, RC_ACCT);
        } else {
            // we have the accountName already, process the requests.

            synchronized (pendingTokenRequests) {
                try {
                    while (!pendingTokenRequests.isEmpty()) {
                        request = pendingTokenRequests.remove(0);
                        if (request != null) {
                            doGetToken(request, acctName);
                        }
                    }

                } catch (Throwable th) {
                    // catch everything so we can return something to the callback for each call.
                    if (request != null) {
                        Log.e(TAG, "Cannot process request", th);
                        request.setResult(CommonStatusCodes.ERROR);
                    }
                }
            }
        }
        Log.d(TAG, "Done with processRequests!");
    }

    /**
     * Gets the email, and/or tokens as requested.
     *
     * @param tokenRequest - the request to process.
     */
    private void doGetToken(final TokenRequest tokenRequest, final String accountName) {
        final Activity theActivity = getActivity();

        Log.d(TAG, "Calling doGetToken for " +
                "e: " + tokenRequest.doEmail + " a:" + tokenRequest.doAccessToken + " i:" +
                tokenRequest.doIdToken);

        AsyncTask<Object, Integer, TokenRequest> t =
                new AsyncTask<Object, Integer, TokenRequest>() {
                    @Override
                    protected TokenRequest doInBackground(Object[] params) {
                        // initialize the email to null, since it used by all the token getters.
                        String accessToken;
                        String idToken;
                        int statusCode = CommonStatusCodes.SUCCESS;

                        tokenRequest.setEmail(accountName);

                        if (tokenRequest.doAccessToken && accountName != null) {
                            // now the access token
                            String accessScope = "oauth2:https://www.googleapis.com/auth/plus.me";
                            try {
                                Log.d(TAG, "getting accessToken for " + accountName);
                                accessToken = GoogleAuthUtil.getToken(theActivity, accountName, accessScope);
                                tokenRequest.setAccessToken(accessToken);
                            } catch (Throwable th) {
                                Log.e(TAG, "Exception getting access token", th);
                                statusCode = CommonStatusCodes.INTERNAL_ERROR;
                            }
                        }

                        if (tokenRequest.doIdToken && accountName != null) {
                            if (tokenRequest.getScope() != null &&
                                    !tokenRequest.getScope().isEmpty()) {
                                try {
                                    Log.d(TAG, "Getting ID token.  Scope = " +
                                            tokenRequest.getScope() + " email: " + accountName);
                                    idToken = GoogleAuthUtil.getToken(theActivity, accountName,
                                            tokenRequest.getScope());
                                    tokenRequest.setIdToken(idToken);
                                } catch (Throwable th) {
                                    Log.e(TAG, "Exception getting access token", th);
                                    statusCode = CommonStatusCodes.INTERNAL_ERROR;
                                }
                            } else {
                                Log.w(TAG, "Skipping ID token: scope is empty");
                                statusCode = CommonStatusCodes.DEVELOPER_ERROR;
                            }
                        } else if (tokenRequest.doIdToken) {
                            Log.e(TAG, "Skipping ID token: email is empty?");
                        }

                        Log.d(TAG, "Done with tokenRequest status: " + statusCode);
                        tokenRequest.setResult(statusCode);
                        return tokenRequest;
                    }

                    /**
                     * <p>Applications should preferably override {@link #onCancelled(Object)}.
                     * This method is invoked by the default implementation of
                     * {@link #onCancelled(Object)}.</p>
                     * <p/>
                     * <p>Runs on the UI thread after {@link #cancel(boolean)} is invoked and
                     * {@link #doInBackground(Object[])} has finished.</p>
                     *
                     * @see #onCancelled(Object)
                     * @see #cancel(boolean)
                     * @see #isCancelled()
                     */
                    @Override
                    protected void onCancelled() {
                        super.onCancelled();
                        tokenRequest.cancel();
                    }

                    /**
                     * <p>Runs on the UI thread after {@link #doInBackground}. The
                     * specified result is the value returned by {@link #doInBackground}.</p>
                     * <p/>
                     * <p>This method won't be invoked if the task was cancelled.</p>
                     *
                     * @param tokenPendingResult The result of the operation computed by {@link #doInBackground}.
                     * @see #onPreExecute
                     * @see #doInBackground
                     * @see #onCancelled(Object)
                     */
                    @Override
                    protected void onPostExecute(TokenRequest tokenPendingResult) {
                        Log.d(TAG, "onPostExecute for the token fetch");
                        super.onPostExecute(tokenPendingResult);
                    }
                };
        t.execute();
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

        Log.d(TAG, "onActivityResult: " + requestCode + ": " + resultCode);

        if (requestCode == RC_ACCT) {
            int status = resultCode;
            String accountName = selectedAccountName;
            if (resultCode == Activity.RESULT_OK) {
                status = CommonStatusCodes.SUCCESS;
                accountName = data.getStringExtra(AccountManager.KEY_ACCOUNT_NAME);
            } else if (resultCode == Activity.RESULT_CANCELED) {
                status = CommonStatusCodes.CANCELED;
            }

            // always clear the flag - even if it is an error.
            synchronized (pendingTokenRequests) {
                selectedAccountName = accountName;
                mIsSelecting = false;
            }

            // Process all the requests using this status.
            processRequests(status);
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
        processRequests(CommonStatusCodes.SUCCESS);
        super.onResume();
    }

    /**
     * Helper class containing the request for information.
     */
    private static class TokenRequest {
        private TokenPendingResult pendingResponse;
        private boolean doEmail;
        private boolean doAccessToken;
        private boolean doIdToken;
        private String scope;
        private String rationale;

        public TokenRequest(boolean fetchEmail, boolean fetchAccessToken, boolean fetchIdToken, String scope) {
            pendingResponse = new TokenPendingResult();
            doEmail = fetchEmail;
            doAccessToken = fetchAccessToken;
            doIdToken = fetchIdToken;
            this.scope = scope;
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

        public String getScope() {
            return scope;
        }

        public void setAccessToken(String accessToken) {
            pendingResponse.setAccessToken(accessToken);
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

        public String getAccessToken() {
            return pendingResponse.result.getAccessToken();
        }

        public String getRationale() {
            return rationale;
        }

        public void setRationale(String rationale) {
            this.rationale = rationale;
        }

        @Override
        public String toString() {
            return Integer.toHexString(hashCode()) + " (e:" + doEmail + " a:" + doAccessToken + " i:" + doIdToken + ")";
        }
    }
}
