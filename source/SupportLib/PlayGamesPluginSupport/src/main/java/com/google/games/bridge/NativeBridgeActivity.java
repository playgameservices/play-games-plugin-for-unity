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
import android.app.Activity;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentSender;
import android.content.pm.PackageManager;
import android.os.AsyncTask;
import android.os.Bundle;
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

import java.util.HashMap;

/**
 * A simple bridge activity that receives an intent from the Google Play Games
 * native SDK and forwards the result of executing the intent back to the SDK
 * via JNI.
 */
public final class NativeBridgeActivity extends Activity implements
        GoogleApiClient.ConnectionCallbacks, GoogleApiClient.OnConnectionFailedListener {

    private static final String BRIDGED_INTENT = "BRIDGED_INTENT";
    private static final int GPG_RESPONSE_CODE = 0x475047;
    private static final int RC_SIGN_IN = 9001;
    private static final String TOKEN_RESULT_KEY = "token_result";
    private static final String SCOPE_KEY = "scope_key";
    private static final int REQUEST_ACCT_PERM = 10;
    private static final int BG_COLOR = 0x40ffffff;
    private static final String TAG = "NativeBridgeActivity";

    // map of pending results used to pass back information to the caller.
    // the key is a unique key added to the intent extras so the bridge activity
    // can find the result to respond.
    private static final HashMap<String, TokenPendingResult> pendingResultMap = new HashMap<>();

    // true indicates we are waiting for the activity to return so we can
    // forward the response.
    // if onDestroy is called with a pending result, the result is simulated and
    // forwarded to the SDK.
    private boolean pendingResult;

    private GoogleApiClient mGoogleApiClient;
    private boolean mShouldResolve = false;
    private boolean mIsResolving = false;

    // This method should be implemented by invoking gpg::AndroidSupport::OnActivityResult
    private native void forwardActivityResult(int requestCode, int resultCode, Intent data);

    static {
        System.loadLibrary("gpg");
    }

    public void onCreate(Bundle savedInstanceState) {
        View v = new View(this);
        v.setBackgroundColor(BG_COLOR);
        setContentView(v);
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onStart() {

        boolean shouldFinish = false;

        Intent bridgedIntent = getIntent().getParcelableExtra(BRIDGED_INTENT);
        if (bridgedIntent != null) {
            startActivityForResult(bridgedIntent, GPG_RESPONSE_CODE);
        } else {
            String key = getIntent().getStringExtra(TOKEN_RESULT_KEY);
            String scope = getIntent().getStringExtra(SCOPE_KEY);

            TokenPendingResult pr = pendingResultMap.get(key);

            if (pr != null) {
                if (scope == null || scope.isEmpty()) {
                    Log.w(TAG, "Scope account empty or null");
                }

                mGoogleApiClient = new GoogleApiClient.Builder(this)
                        .addApi(Plus.API)
                        .addScope(Plus.SCOPE_PLUS_LOGIN)
                        .addConnectionCallbacks(this)
                        .addOnConnectionFailedListener(this)
                        .build();

                mGoogleApiClient.connect();

                if (mGoogleApiClient.isConnected()) {
                    Log.d(TAG, "  CONNECTED __ SOWWWWOEEET");
                    doGetToken(pr, scope);
                } else {
                    Log.d(TAG, "not connected!");
                }

            } else {
                Log.e(TAG, "Pending result is missing?!??");
                shouldFinish = true;
            }
        }

        super.onStart();
        if (shouldFinish) {
            finish();
        }
    }

    /**
     * Same as calling {@link #startActivityForResult(android.content.Intent, int, android.os.Bundle)}
     * with no options.
     *
     * @param intent      The intent to start.
     * @param requestCode If >= 0, this code will be returned in
     *                    onActivityResult() when the activity exits.
     * @throws android.content.ActivityNotFoundException
     * @see #startActivity
     */
    @Override
    public void startActivityForResult(Intent intent, int requestCode) {
        // set the pending result flag if this activity is for GPG
        pendingResult = requestCode == GPG_RESPONSE_CODE;
        if (pendingResult) {
            Log.d(TAG, "starting GPG activity: " + intent);
        } else {
            Log.i(TAG, "starting non-GPG activity: " + requestCode + " " + intent);
        }
        super.startActivityForResult(intent, requestCode);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {

        if (requestCode == RC_SIGN_IN) {
            // If the error resolution was not successful we should not resolve further.
            if (resultCode != RESULT_OK) {
                mShouldResolve = false;
            }

            mIsResolving = false;
            mGoogleApiClient.connect();
            return;
        }

        if (requestCode == GPG_RESPONSE_CODE) {
            Log.d(TAG, "Forwarding activity result to native SDK.");
            forwardActivityResult(requestCode, resultCode, data);

            // clear the pending flag.
            pendingResult = false;
        } else {
            Log.d(TAG, "onActivityResult for unknown request code: " + requestCode + " calling finish()");
        }

        finish();

        super.onActivityResult(requestCode, resultCode, data);
    }

    public static void launchBridgeIntent(Activity parentActivity, Intent intent) {


        Log.d(TAG, "Launching bridge activity: parent:" + parentActivity + " intent " + intent);
        Intent bridgeIntent = new Intent(parentActivity, NativeBridgeActivity.class);
        bridgeIntent.putExtra(BRIDGED_INTENT, intent);
        parentActivity.startActivity(bridgeIntent);
    }

    public static PendingResult fetchToken(Activity parentActivity, String scope) {


        TokenPendingResult pr = new TokenPendingResult();
        String key = pr.toString() + Long.toHexString(System.currentTimeMillis());
        pendingResultMap.put(key, pr);

        try {
            Intent bridgeIntent = new Intent(parentActivity, NativeBridgeActivity.class);
            bridgeIntent.putExtra(TOKEN_RESULT_KEY, key);
            bridgeIntent.putExtra(SCOPE_KEY, scope);
            parentActivity.startActivity(bridgeIntent);
        } catch (Throwable th) {
            Log.e(TAG, "Cannot launch bridge activity", th);
            pr.setToken(null, null, null, CommonStatusCodes.ERROR);
            pendingResultMap.remove(key);
        }


        return pr;
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        Log.d(TAG, "onRequestPermissionsResult: " + requestCode + "grants: " + grantResults.length);
        if (requestCode == REQUEST_ACCT_PERM) {
            if (grantResults.length == 1
                    && grantResults[0] == PackageManager.PERMISSION_GRANTED) {

                String key = getIntent().getStringExtra(TOKEN_RESULT_KEY);
                String scope = getIntent().getStringExtra(SCOPE_KEY);
                TokenPendingResult pr = pendingResultMap.get(key);
                doGetToken(pr, scope);
            }
        } else {
            super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }

    private void doGetToken(final TokenPendingResult pr, final String scope) {
        final Activity theActivity = this;
        final GoogleApiClient googleApiClient = this.mGoogleApiClient;


        Log.d(TAG, "Calling doGetToken");
        AsyncTask<Object, Integer, TokenPendingResult> t =
                new AsyncTask<Object, Integer, TokenPendingResult>() {
                    @Override
                    protected TokenPendingResult doInBackground(Object[] params) {
                        String email = "";
                        String accessToken = "";
                        String idToken = "";
                        try {

                            Log.d(TAG, "Calling getAccountName");

                            // get the email first
                            email = Plus.AccountApi.getAccountName(googleApiClient);

                            Log.d(TAG, "Calling account name == " + email);

                            // now the access token
                            String accessScope = "oauth2:https://www.googleapis.com/auth/plus.me";

                            accessToken = GoogleAuthUtil.getToken(theActivity, email, accessScope);

                            Log.d(TAG, "Calling accessToken is = " + accessToken);

                            if (scope != null && !scope.isEmpty()) {
                                Log.d(TAG, "Getting ID token.  Scope = " + scope);
                                idToken = GoogleAuthUtil.getToken(theActivity, email, scope);
                            } else {
                                Log.w(TAG, "Skipping ID token: scope is empty");
                            }

                            pr.setToken(accessToken, idToken, email, CommonStatusCodes.SUCCESS);
                        } catch (Throwable e) {
                            Log.e(TAG, "Exception getting token", e);
                            pr.setToken(accessToken, idToken, email, CommonStatusCodes.DEVELOPER_ERROR);
                        }
                        return pr;
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
                        pr.cancel();
                        pendingResultMap.remove(getIntent().getStringExtra(TOKEN_RESULT_KEY));
                        theActivity.finish();
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
                    protected void onPostExecute(TokenPendingResult tokenPendingResult) {
                        Log.d(TAG, "onPostEXecute for the token fetch");
                        super.onPostExecute(tokenPendingResult);
                        pendingResultMap.remove(getIntent().getStringExtra(TOKEN_RESULT_KEY));
                        theActivity.finish();
                    }
                };
        t.execute();
    }

    /**
     * Perform any final cleanup before an activity is destroyed.  This can
     * happen either because the activity is finishing (someone called
     * {@link #finish} on it, or because the system is temporarily destroying
     * this instance of the activity to save space.  You can distinguish
     * between these two scenarios with the {@link #isFinishing} method.
     * <p/>
     * <p><em>Note: do not count on this method being called as a place for
     * saving data! For example, if an activity is editing data in a content
     * provider, those edits should be committed in either {@link #onPause} or
     * {@link #onSaveInstanceState}, not here.</em> This method is usually implemented to
     * free resources like threads that are associated with an activity, so
     * that a destroyed activity does not leave such things around while the
     * rest of its application is still running.  There are situations where
     * the system will simply kill the activity's hosting process without
     * calling this method (or any others) in it, so it should not be used to
     * do things that are intended to remain around after the process goes
     * away.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onPause
     * @see #onStop
     * @see #finish
     * @see #isFinishing
     */
    @Override
    protected void onDestroy() {

        // if we have a pending Result and being destroyed, this most likely
        // means the activity we are waiting for is also destroyed, so cancel it out
        // with the SDK.
        if (pendingResult) {
            Log.w(TAG, "onDestroy called with pendingResult == true.  forwarding canceled result");
            forwardActivityResult(GPG_RESPONSE_CODE, RESULT_CANCELED, null);
            pendingResult = false;
        }

        String key = getIntent().getStringExtra(TOKEN_RESULT_KEY);
        if (key != null) {
            Log.w(TAG, "onDestroy called during token fetching!");
            pendingResultMap.remove(key);
        }
        super.onDestroy();
    }


    /**
     * Called after {@link #onStop} when the current activity is being
     * re-displayed to the user (the user has navigated back to it).  It will
     * be followed by {@link #onStart} and then {@link #onResume}.
     * <p/>
     * <p>For activities that are using raw  Cursor objects (instead of
     * creating them through
     * {@link #managedQuery(android.net.Uri, String[], String, String[], String)},
     * this is usually the place
     * where the cursor should be requeried (because you had deactivated it in
     * {@link #onStop}.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onStop
     * @see #onStart
     * @see #onResume
     */
    @Override
    protected void onRestart() {
        Log.d(TAG, "onRestart");

        super.onRestart();
    }

    /**
     * Called as part of the activity lifecycle when an activity is going into
     * the background, but has not (yet) been killed.  The counterpart to
     * {@link #onResume}.
     * <p/>
     * <p>When activity B is launched in front of activity A, this callback will
     * be invoked on A.  B will not be created until A's {@link #onPause} returns,
     * so be sure to not do anything lengthy here.
     * <p/>
     * <p>This callback is mostly used for saving any persistent state the
     * activity is editing, to present a "edit in place" model to the user and
     * making sure nothing is lost if there are not enough resources to start
     * the new activity without first killing this one.  This is also a good
     * place to do things like stop animations and other things that consume a
     * noticeable amount of CPU in order to make the switch to the next activity
     * as fast as possible, or to close resources that are exclusive access
     * such as the camera.
     * <p/>
     * <p>In situations where the system needs more memory it may kill paused
     * processes to reclaim resources.  Because of this, you should be sure
     * that all of your state is saved by the time you return from
     * this function.  In general {@link #onSaveInstanceState} is used to save
     * per-instance state in the activity and this method is used to store
     * global persistent data (in content providers, files, etc.)
     * <p/>
     * <p>After receiving this call you will usually receive a following call
     * to {@link #onStop} (after the next activity has been resumed and
     * displayed), however in some cases there will be a direct call back to
     * {@link #onResume} without going through the stopped state.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onResume
     * @see #onSaveInstanceState
     * @see #onStop
     */
    @Override
    protected void onPause() {
        Log.d(TAG, "onPause");

        super.onPause();
    }


    /**
     * Called when you are no longer visible to the user.  You will next
     * receive either {@link #onRestart}, {@link #onDestroy}, or nothing,
     * depending on later user activity.
     * <p/>
     * <p>Note that this method may never be called, in low memory situations
     * where the system does not have enough memory to keep your activity's
     * process running after its {@link #onPause} method is called.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onRestart
     * @see #onResume
     * @see #onSaveInstanceState
     * @see #onDestroy
     */
    @Override
    protected void onStop() {
        Log.d(TAG, "onStop");

        if (mGoogleApiClient != null) {
            mGoogleApiClient.disconnect();
        }
        super.onStop();
    }


    /**
     * Called after {@link #onRestoreInstanceState}, {@link #onRestart}, or
     * {@link #onPause}, for your activity to start interacting with the user.
     * This is a good place to begin animations, open exclusive-access devices
     * (such as the camera), etc.
     * <p/>
     * <p>Keep in mind that onResume is not the best indicator that your activity
     * is visible to the user; a system window such as the keyguard may be in
     * front.  Use {@link #onWindowFocusChanged} to know for certain that your
     * activity is visible to the user (for example, to resume a game).
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onRestoreInstanceState
     * @see #onRestart
     * @see #onPostResume
     * @see #onPause
     */
    @Override
    protected void onResume() {
        Log.d(TAG, "onResume");

        super.onResume();
    }

    @Override
    public void onConnected(Bundle bundle) {
        // onConnected indicates that an account was selected on the device, that the selected
        // account has granted any requested permissions to our app and that we were able to
        // establish a service connection to Google Play services.
        Log.d(TAG, "onConnected:" + bundle);
        mShouldResolve = false;

        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.GET_ACCOUNTS) !=
                PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(
                    this, new String[]{Manifest.permission.GET_ACCOUNTS}, REQUEST_ACCT_PERM);
        } else {
            TokenPendingResult pr = pendingResultMap.get(getIntent().getStringExtra(TOKEN_RESULT_KEY));
            String scope = getIntent().getStringExtra(SCOPE_KEY);
            doGetToken(pr, scope);
        }

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

        if (!mIsResolving && mShouldResolve) {
            if (connectionResult.hasResolution()) {
                try {
                    mIsResolving = true;
                    connectionResult.startResolutionForResult(this, RC_SIGN_IN);
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
            TokenPendingResult pr = pendingResultMap.remove(
                    getIntent().getStringExtra(TOKEN_RESULT_KEY));
            if (pr != null) {
                pr.setToken(null, null, null, connectionResult.getErrorCode());
            } else {
                Log.w(TAG, "pending result is null!! when cannot connect!");
            }
            finish();
        }
    }

    private void showErrorDialog(ConnectionResult connectionResult) {
        GoogleApiAvailability apiAvailability = GoogleApiAvailability.getInstance();
        final int resultCode = apiAvailability.isGooglePlayServicesAvailable(this);

        if (resultCode != ConnectionResult.SUCCESS) {
            if (apiAvailability.isUserResolvableError(resultCode)) {
                apiAvailability.getErrorDialog(this, resultCode, RC_SIGN_IN,
                        new DialogInterface.OnCancelListener() {
                            @Override
                            public void onCancel(DialogInterface dialog) {
                                mShouldResolve = false;
                                TokenPendingResult pr =
                                        pendingResultMap.remove(
                                                getIntent().getStringExtra(TOKEN_RESULT_KEY));
                                if (pr != null) {
                                    pr.setToken(null, null, null, resultCode);
                                } else {
                                    Log.w(TAG, "pending result is null!! when cannot connect!");
                                }
                                finish();
                            }
                        }).show();
            } else {
                Log.w(TAG, "Google Play Services Error:" + connectionResult);
                String errorString = apiAvailability.getErrorString(resultCode);
                Toast.makeText(this, errorString, Toast.LENGTH_SHORT).show();

                mShouldResolve = false;
                TokenPendingResult pr = pendingResultMap.remove(
                        getIntent().getStringExtra(TOKEN_RESULT_KEY));
                if (pr != null) {
                    pr.setToken(null, null, null, connectionResult.getErrorCode());
                } else {
                    Log.w(TAG, "pending result is null!! when cannot connect!");
                }
                finish();
            }
        }
    }
}
