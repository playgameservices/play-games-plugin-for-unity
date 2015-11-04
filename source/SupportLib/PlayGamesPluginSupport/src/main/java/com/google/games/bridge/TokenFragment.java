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

import android.Manifest;
import android.annotation.TargetApi;
import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentSender;
import android.content.pm.PackageManager;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.annotation.RequiresPermission;
import android.support.design.widget.Snackbar;
import android.support.v4.app.ActivityCompat;
import android.util.Log;
import android.view.View;
import android.widget.Toast;

import com.google.android.gms.auth.GoogleAuthUtil;
import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GoogleApiAvailability;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.plus.Plus;

import java.util.ArrayList;
import java.util.List;

/**
 * Activity fragment with no UI added to the parent activity in order to manage
 * the accessing of the player's email address and tokens.
 */
@TargetApi(Build.VERSION_CODES.HONEYCOMB)
public class TokenFragment extends Fragment implements GoogleApiClient.ConnectionCallbacks,
        GoogleApiClient.OnConnectionFailedListener {

    private static final String TAG = "TokenFragment";
    private static final String FRAGMENT_TAG = "gpg.TokenSupport";
    private static final int RC_SIGN_IN = 9001;
    private static final int REQUEST_ACCT_PERM = 10;
    private static final int OK_KEY = 0xabab;

    private GoogleApiClient mGoogleApiClient;

    // map of pending results used to pass back information to the caller.
    // the key is a unique key added to the intent extras so the bridge activity
    // can find the result to respond.
    private static final List<TokenRequest> pendingTokenRequests = new ArrayList<>();
    private boolean mShouldResolve = false;
    private boolean mIsResolving = false;

    private boolean mPendingPermissionRequest = false;
    private int mPermissionResult = Integer.MIN_VALUE;

    /**
     * External entry point for getting tokens and email address.  This
     * creates the fragment if needed and queues up the request.  The fragment, once
     * active processes the list of requests.
     *
     * @param parentActivity   - the activity to attach the fragment to.
     * @param rationale        - the rationale to display when requesting permission.
     * @param fetchEmail       - true indicates get the email.
     * @param fetchAccessToken - true indicates get the access token.
     * @param fetchIdToken     - true indicates get the id token.
     * @param scope            - the scope for getting the id token.
     * @return PendingResult for retrieving the results when ready.
     */
    public static PendingResult fetchToken(Activity parentActivity,
                                           String rationale, boolean fetchEmail,
                                           boolean fetchAccessToken, boolean fetchIdToken, String scope) {
        TokenRequest request = new TokenRequest(fetchEmail, fetchAccessToken, fetchIdToken, scope);
        request.setRationale(rationale);
        synchronized (pendingTokenRequests) {
            pendingTokenRequests.add(request);
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
            Log.d(TAG, "Fragment exists.. calling processRequests");
            fragment.processRequests(CommonStatusCodes.SUCCESS);
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

        if (mGoogleApiClient == null || !mGoogleApiClient.isConnected()) {
            Log.d(TAG, "mGoogleApiClient not created yet...");
            if (mGoogleApiClient != null && !mGoogleApiClient.isConnecting()) {
                mGoogleApiClient.connect();
            }
            return;
        }

        if (!pendingTokenRequests.isEmpty()) {
            if (!permissionResolved()) {
                return;
            }
            else if (mPermissionResult == PackageManager.PERMISSION_DENIED) {
                errorCode = CommonStatusCodes.AUTH_API_ACCESS_FORBIDDEN;
            }
        }

        Log.d(TAG, "Pending map in processRequests is " + pendingTokenRequests.size());
        while (!pendingTokenRequests.isEmpty()) {

            // check again since we can disconnect in the loop.
            if (!mGoogleApiClient.isConnected()) {
                if (mGoogleApiClient.isConnecting()) {
                    Log.w(TAG, "Still connecting.... hold on...");
                } else {
                    Log.w(TAG, "Google API Client not connected! calling connect");
                    mGoogleApiClient.connect();
                }
                return;
            }

            try {
                synchronized (pendingTokenRequests) {
                    if (!pendingTokenRequests.isEmpty()) {
                        request = pendingTokenRequests.remove(0);
                    }
                }
                if (request == null) {
                    continue;
                }
                if (errorCode == CommonStatusCodes.SUCCESS) {
                    doGetToken(request);
                } else {
                    request.setResult(errorCode);
                }
            } catch (Throwable th) {
                if (request != null) {
                    Log.e(TAG, "Cannot process request", th);
                    request.setResult(CommonStatusCodes.ERROR);
                }
            }
        }
        Log.d(TAG, "Done with processRequests!");

    }

    @RequiresPermission(Manifest.permission.GET_ACCOUNTS)
    @TargetApi(23)
    private boolean permissionResolved()
    {
        int rc = ActivityCompat.checkSelfPermission(getActivity(), Manifest.permission.GET_ACCOUNTS);
        if (rc == PackageManager.PERMISSION_GRANTED) {
            mPermissionResult = rc;
        } else if (!mPendingPermissionRequest && mPermissionResult == Integer.MIN_VALUE) {
            Log.d(TAG, "GET_ACCOUNTS not granted, requesting.");
            mPendingPermissionRequest = true;

            if (shouldShowRequestPermissionRationale(Manifest.permission.GET_ACCOUNTS) &&
                    getActivity().getCurrentFocus() != null) {
                String rationale = pendingTokenRequests.get(0).getRationale();
                if (rationale == null || rationale.isEmpty()) {
                    rationale = "This application requires your email address or identity token";
                }

                Log.i(TAG, "Displaying permission rationale to provide additional context.");
                Snackbar.make(getActivity().getCurrentFocus(),
                        rationale,
                        Snackbar.LENGTH_INDEFINITE)
                        .setAction("OK", new View.OnClickListener() {
                            @Override
                            public void onClick(View view) {
                                // Request permission
                                ActivityCompat.requestPermissions(getActivity(),
                                        new String[]{Manifest.permission.GET_ACCOUNTS},
                                        REQUEST_ACCT_PERM);
                            }
                        })
                        .show();
            } else {
                ActivityCompat.requestPermissions(
                        getActivity(), new String[]{Manifest.permission.GET_ACCOUNTS}, REQUEST_ACCT_PERM);
            }

           return false;

        } else {
            Log.i(TAG, "Request is denied, permission for GET_ACCOUNTS is not granted: (" + mPermissionResult + ")");
        }

        return true;
    }


    /**
     * Gets the email, and/or tokens as requested.
     *
     * @param tokenRequest - the request to process.
     */
    private void doGetToken(final TokenRequest tokenRequest) {
        final Activity theActivity = getActivity();
        final GoogleApiClient googleApiClient = this.mGoogleApiClient;

        Log.d(TAG, "Calling doGetToken for " +
                Plus.PeopleApi.getCurrentPerson(googleApiClient).getDisplayName() +
                "e: " + tokenRequest.doEmail + " a:" + tokenRequest.doAccessToken + " i:" +
                tokenRequest.doIdToken);

        AsyncTask<Object, Integer, TokenRequest> t =
                new AsyncTask<Object, Integer, TokenRequest>() {
                    @Override
                    protected TokenRequest doInBackground(Object[] params) {
                        // initialize the email to null, since it used by all the token getters.
                        String email = null;
                        String accessToken;
                        String idToken;
                        int statusCode = CommonStatusCodes.SUCCESS;


                        if (tokenRequest.doEmail || tokenRequest.doIdToken || tokenRequest.doAccessToken) {
                            Log.d(TAG, "Calling getAccountName");
                            try {
                                // get the email first
                                email = Plus.AccountApi.getAccountName(googleApiClient);
                                tokenRequest.setEmail(email);
                            } catch (Throwable th) {
                                Log.e(TAG, "Exception getting email: " + th.getMessage(), th);
                                statusCode = CommonStatusCodes.INTERNAL_ERROR;
                                email = null;
                            }
                        }

                        if (tokenRequest.doAccessToken && email != null) {
                            // now the access token
                            String accessScope = "oauth2:https://www.googleapis.com/auth/plus.me";
                            try {
                                Log.d(TAG, "getting accessToken for " + email);
                                accessToken = GoogleAuthUtil.getToken(theActivity, email, accessScope);
                                tokenRequest.setAccessToken(accessToken);
                            } catch (Throwable th) {
                                Log.e(TAG, "Exception getting access token", th);
                                statusCode = CommonStatusCodes.INTERNAL_ERROR;
                            }

                        }

                        if (tokenRequest.doIdToken && email != null) {

                            if (tokenRequest.getScope() != null &&
                                    !tokenRequest.getScope().isEmpty()) {
                                try {
                                    Log.d(TAG, "Getting ID token.  Scope = " +
                                            tokenRequest.getScope() + " email: " + email);
                                    idToken = GoogleAuthUtil.getToken(theActivity, email,
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
        if (requestCode == RC_SIGN_IN) {
            // If the error resolution was not successful we should not resolve further.
            if (resultCode != Activity.RESULT_OK) {
                mShouldResolve = false;
            }

            mIsResolving = false;
            mGoogleApiClient.connect();
        }
        super.onActivityResult(requestCode, resultCode, data);
    }

    @Override
    public void onStart() {

        Log.d(TAG, "onStart");
        mGoogleApiClient = new GoogleApiClient.Builder(this.getActivity())
                .addApi(Plus.API)
                .addScope(Plus.SCOPE_PLUS_LOGIN)
                .addConnectionCallbacks(this)
                .addOnConnectionFailedListener(this)
                .build();

        mShouldResolve = true;
        mGoogleApiClient.connect();
        mPermissionResult = Integer.MIN_VALUE;

        super.onStart();
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


    @Override
    public void onPause() {
        Log.d(TAG, "onPause called");

        // disconnect here so if the user is changed, we'll reconnect as the different user.
        mGoogleApiClient.disconnect();
        super.onPause();
    }

    @Override
    public void onConnected(Bundle bundle) {
        // onConnected indicates that an account was selected on the device, that the selected
        // account has granted any requested permissions to our app and that we were able to
        // establish a service connection to Google Play services.
        Log.d(TAG, "onConnected:" + bundle);
        mShouldResolve = false;
        mPermissionResult = Integer.MIN_VALUE;
        processRequests(CommonStatusCodes.SUCCESS);
    }

    @Override
    public void onConnectionSuspended(int i) {
        Log.d(TAG, "Connection suspended");
    }

    @Override
    public void onConnectionFailed(ConnectionResult connectionResult) {
        // Could not connect to Google Play Services.  The user needs to select an account,
        // grant permissions or resolve an error in order to sign in. Refer to the javadoc for
        // ConnectionResult to see possible error codes.
        Log.d(TAG, "onConnectionFailed:" + connectionResult);
        mPermissionResult = Integer.MIN_VALUE;

        if (!mIsResolving && mShouldResolve) {
            if (connectionResult.hasResolution()) {
                try {
                    mIsResolving = true;
                    connectionResult.startResolutionForResult(getActivity(), RC_SIGN_IN);
                } catch (IntentSender.SendIntentException e) {
                    Log.e(TAG, "Could not resolve ConnectionResult.", e);
                    mIsResolving = false;
                    mGoogleApiClient.connect();
                }
            } else {
                // Could not resolve the connection result, show the user an
                // error dialog.
                showErrorDialog(connectionResult);
                mIsResolving = true;
            }
        } else {
            processRequests(connectionResult.getErrorCode());
        }
    }

    /**
     * Called when the Fragment is no longer started.  This is generally
     * tied to {@link Activity#onStop() Activity.onStop} of the containing
     * Activity's lifecycle.
     */
    @Override
    public void onStop() {
        if (mGoogleApiClient != null) {
            mGoogleApiClient.disconnect();
        }
        mPermissionResult = Integer.MIN_VALUE;
        super.onStop();
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        Log.d(TAG, "onRequestPermissionsResult: " + requestCode + "grants: " + grantResults.length);
        if (requestCode == REQUEST_ACCT_PERM) {
            mPendingPermissionRequest = false;
            if (permissions.length == 1 && permissions[0].equals(Manifest.permission.GET_ACCOUNTS)) {
                mPermissionResult = grantResults[0];
            }
            if (mPermissionResult == PackageManager.PERMISSION_GRANTED) {

                processRequests(CommonStatusCodes.SUCCESS);
            } else {
                Log.w(TAG, "Request for GET_ACCOUNTS was denied");
                processRequests(CommonStatusCodes.AUTH_API_ACCESS_FORBIDDEN);
            }
        } else {
            super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }

    private void showErrorDialog(ConnectionResult connectionResult) {
        GoogleApiAvailability apiAvailability = GoogleApiAvailability.getInstance();
        final int resultCode = apiAvailability.isGooglePlayServicesAvailable(getActivity());

        if (resultCode != ConnectionResult.SUCCESS) {
            if (apiAvailability.isUserResolvableError(resultCode)) {
                apiAvailability.getErrorDialog(getActivity(), resultCode, RC_SIGN_IN,
                        new DialogInterface.OnCancelListener() {
                            @Override
                            public void onCancel(DialogInterface dialog) {
                                mShouldResolve = false;
                                processRequests(resultCode);
                            }
                        }).show();
            } else {
                Log.w(TAG, "Google Play Services Error:" + connectionResult);
                String errorString = apiAvailability.getErrorString(resultCode);
                Toast.makeText(getActivity(), errorString, Toast.LENGTH_SHORT).show();

                mShouldResolve = false;
                processRequests(resultCode);
            }
        }
    }

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

        public String getRationale() {
            return rationale;
        }

        public void setRationale(String rationale) {
            this.rationale = rationale;
        }
    }
}
