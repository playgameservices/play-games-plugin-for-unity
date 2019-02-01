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
import android.support.annotation.NonNull;
import android.util.Log;
import android.view.View;
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
import com.google.android.gms.common.api.Status;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.AchievementsClient;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.games.GamesClient;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;

/**
 * Activity fragment with no UI added to the parent activity in order to manage
 * the accessing of the player's email address and tokens.
 */
public class TokenFragment extends Fragment
{

    private static final String TAG = "TokenFragment";
    private static final String FRAGMENT_TAG = "gpg.AuthTokenSupport";
    private static final int RC_SIGN_IN = 9002;
    private static final int RC_ACHIEVEMENT_UI = 9003;

    // Pending token request.  There can be only one outstanding request at a
    // time.
    private static final Object lock = new Object();
    private static Request pendingRequest;
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
        SignInRequest request = new SignInRequest(silent, requestAuthCode, requestEmail, 
                requestIdToken, webClientId, forceRefreshToken, additionalScopes, hidePopups,
                accountName);

        if(!TokenFragment.startRequest(parentActivity, request)) {
            request.setFailure(CommonStatusCodes.DEVELOPER_ERROR);
        }

        return request.getPendingResponse();
    }

    /** 
     * Should be aligned to:
     * PluginDev/Assets/GooglePlayGames/BasicApi/CommonTypes.cs enum UIStatus
     * */ 
    private static final int UI_STATUS_VALID = 1;
    private static final int UI_STATUS_INTERNAL_ERROR = -2;
    private static final int UI_STATUS_NOT_AUTHORIZED = -3;
    private static final int UI_STATUS_UI_BUSY = -12;

    /**
     * This calls silent signin and gets the user info including the auth code.
     * If silent sign-in fails, the failure is returned.
     * @return PendingResult for waiting on result.
     */
    public static PendingResult getAnotherAuthCode(Activity parentActivity,
                                                   final boolean reauthIfNeeded,
                                                   String webClientId) {
        return fetchToken(parentActivity,
                          /* silent= */!reauthIfNeeded,
                          /* requestAuthCode= */true,
                          /* requestEmail= */false,
                          /* requestIdToken= */false,
                          /* webClientId= */webClientId,
                          /* forceRefreshToken= */false,
                          /* additionalScopes= */null,
                          /* hidePopups= */true,
                          /* accountName= */null);
    }

    public static Task<Integer> showAchievementUi(Activity parentActivity) {
        AchievementUiRequest request = new AchievementUiRequest();

        if(!TokenFragment.startRequest(parentActivity, request)) {
            request.setResult(UI_STATUS_UI_BUSY);
        }

        return request.getTask();
    }

    public static void signOut(Activity activity) {
        GoogleSignInClient signInClient = GoogleSignIn.getClient(activity, GoogleSignInOptions.DEFAULT_GAMES_SIGN_IN);
        signInClient.signOut();
        synchronized (lock) {
            pendingRequest = null;
        }
    }

    private static boolean startRequest(Activity parentActivity, Request request) {
        boolean ok = false;
        synchronized (lock) {
            if (pendingRequest == null) {
                pendingRequest = request;
                ok = true;
            }
        }
        if (ok) {
            TokenFragment helperFragment = TokenFragment.getHelperFragment(parentActivity);
            if (helperFragment.isResumed()) {
                helperFragment.processRequest();
            }
        }
        return ok;
    }

    private static TokenFragment getHelperFragment(Activity parentActivity) {
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
                return null;
            }
        }
        return fragment;
    }
    /**
     * Processes the token requests that are queued up.
     */
    private void processRequest() {     
        Request request;
        synchronized (lock) {
            request = pendingRequest;
        }
        // no request, no need to continue.
        if (request == null) {
            return;
        }

        request.process(this);
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
        if (requestCode == RC_SIGN_IN || requestCode == RC_ACHIEVEMENT_UI) {
            Request request;
            synchronized (lock) {
                request = pendingRequest;
            }
            // no request, no need to continue.
            if (request == null) {
                return;
            }
            request.onActivityResult(requestCode, resultCode, data);
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
        if (helperFragment == null) {
            helperFragment = this;
        }
        processRequest();
    }

    private static void finishRequest(Request request) {
        synchronized (lock) {
            if(pendingRequest == request) {
                pendingRequest = null;
            }
        }
    }

    private interface Request {
        void process(TokenFragment helperFragment);
        void onActivityResult(int requestCode, int resultCode, Intent data);
    };

    private static class AchievementUiRequest implements Request {
        private final TaskCompletionSource<Integer> resultTaskSource = new TaskCompletionSource<>();

        public Task<Integer> getTask() {
            return resultTaskSource.getTask();
        }

        public void process(final TokenFragment helperFragment) {
            final Activity activity = helperFragment.getActivity();
            GoogleSignInAccount account = TokenFragment.getAccount(activity);
            AchievementsClient achievementClient = Games.getAchievementsClient(activity, account);
            achievementClient
                .getAchievementsIntent()
                .addOnSuccessListener(
                    activity,
                        new OnSuccessListener<Intent>() {
                            @Override
                            public void onSuccess(Intent intent) {
                                helperFragment.startActivityForResult(intent, RC_ACHIEVEMENT_UI);
                            }
                        })
                .addOnFailureListener(
                    activity,
                        new OnFailureListener() {
                            @Override
                            public void onFailure(Exception e) {
                                setFailure(e);
                            }
                        });
          

        }

        public void onActivityResult(int requestCode, int resultCode, Intent data) {
            if (requestCode == RC_ACHIEVEMENT_UI) {
                if (resultCode == Activity.RESULT_OK || resultCode == Activity.RESULT_CANCELED) {
                    setResult(UI_STATUS_VALID);
                } else if (resultCode == GamesActivityResultCodes.RESULT_RECONNECT_REQUIRED) {
                    setResult(UI_STATUS_NOT_AUTHORIZED);
                } else {
                    Log.d(TAG, "AchievementUiRequest.onActivityResult unknown resultCode: " + resultCode);
                    setResult(UI_STATUS_INTERNAL_ERROR);
                }
            }
        }

        void setResult(Integer result) {
            resultTaskSource.setResult(result);
            TokenFragment.finishRequest(this);
        }

        void setFailure(Exception e) {
            resultTaskSource.setException(e);
            TokenFragment.finishRequest(this);
        }
    }

    /**
     * Sign-in request.
     */
    private static class SignInRequest implements Request {
        private final TaskCompletionSource<GoogleSignInAccount> resultTaskSource = new TaskCompletionSource<>();
        private final TokenPendingResult pendingResponse = new TokenPendingResult();
        private final boolean silent;
        private final boolean doAuthCode;
        private final boolean doEmail;
        private final boolean doIdToken;
        private final String webClientId;
        private final boolean forceRefresh;
        private final boolean hidePopups;
        private final String accountName;
        private final Scope[] scopes;

        private TokenFragment helperFragment;

        public SignInRequest(boolean silent, boolean fetchAuthCode, boolean fetchEmail,
                            boolean fetchIdToken, String webClientId, boolean
                            forceRefresh, String[] oAuthScopes,
                            boolean hidePopups, String accountName) {
            this.silent = silent;
            this.doAuthCode = fetchAuthCode;
            this.doEmail = fetchEmail;
            this.doIdToken = fetchIdToken;
            this.webClientId = webClientId;
            this.forceRefresh = forceRefresh;
            this.hidePopups = hidePopups;
            this.accountName = accountName;
            if(oAuthScopes != null && oAuthScopes.length > 0) {
                this.scopes = new Scope[oAuthScopes.length];
                for (int i = 0; i < oAuthScopes.length; ++i) {
                    this.scopes[i] = new Scope(oAuthScopes[i]);
                }
            } else {
                this.scopes = null;
            }
        }

        public Task<GoogleSignInAccount> getTask() {
            return resultTaskSource.getTask();
        }

        public PendingResult<TokenResult> getPendingResponse() {
            return pendingResponse;
        }

        public void process(TokenFragment helperFragment) {
            this.helperFragment = helperFragment;
            signIn();
        }

        public void onActivityResult(int requestCode, int resultCode, Intent data) {
            if (requestCode == RC_SIGN_IN) {
                GoogleSignInResult result = Auth.GoogleSignInApi.getSignInResultFromIntent(data);
                if (result != null && result.isSuccess()) {
                    GoogleSignInAccount account = result.getSignInAccount();
                    setSuccess(account);
                } else if (resultCode == Activity.RESULT_CANCELED) {
                    setFailure(CommonStatusCodes.CANCELED);
                } else if (result != null) {
                    Log.e(TAG,"GoogleSignInResult error status code: " + result.getStatus());
                    setFailure(result.getStatus().getStatusCode());
                } else {
                    Log.e(TAG, "Google SignIn Result is null, resultCode is " +
                            resultCode + "(" +
                    GoogleSignInStatusCodes.getStatusCodeString(resultCode) + ")");
                    setFailure(CommonStatusCodes.ERROR);
                }
            }
        }

        private void signIn() {
            Log.d(TAG, "signIn");

            final GoogleSignInClient signInClient = buildClient();
    
            if (signInClient != null) {
                Activity activity = helperFragment.getActivity();
                if (canReuseAccount()) { 
                    final GoogleSignInAccount account = GoogleSignIn.getLastSignedInAccount(activity);
                    if (GoogleSignIn.hasPermissions(account, scopes)) {
                        Log.d(TAG, "Checking the last signed-in account if it can be used.");
                        Games.getGamesClient(activity, account).getAppId()
                            .addOnCompleteListener(new OnCompleteListener<String>() {
                                @Override
                                public void onComplete(@NonNull Task<String> task) {
                                    if (task.isSuccessful()) {
                                        Log.d(TAG, "Signed-in with the last signed-in account.");
                                        setSuccess(account);
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
                                                        setFailure(CommonStatusCodes.SIGN_IN_REQUIRED);
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
                        activity,
                        new OnSuccessListener<GoogleSignInAccount>() {
                            @Override
                            public void onSuccess(GoogleSignInAccount result) {
                                Log.d(TAG, "silentSignIn.onSuccess");
                                setSuccess(result);
                            }
                        })
                    .addOnFailureListener(
                        activity,
                        new OnFailureListener() {
                            @Override
                            public void onFailure(Exception exception) {
                                Log.d(TAG, "silentSignIn.onFailure");
                                int statusCode = CommonStatusCodes.INTERNAL_ERROR;
                                if (exception instanceof ApiException) {
                                    statusCode = ((ApiException) exception).getStatusCode();
                                }
                                // INTERNAL_ERROR will be returned if the user has the outdated PlayServices
                                if (statusCode == CommonStatusCodes.SIGN_IN_REQUIRED || statusCode == CommonStatusCodes.INTERNAL_ERROR) {
                                    if (!silent) {
                                        Intent signInIntent = signInClient.getSignInIntent();
                                        helperFragment.startActivityForResult(signInIntent, RC_SIGN_IN);
                                    } else {
                                        Log.i(TAG, "Sign-in failed. Run in silent mode and UI sign-in required.");
                                        setFailure(CommonStatusCodes.SIGN_IN_REQUIRED);
                                    }
                                } else {
                                    Log.e(TAG, "Sign-in failed with status code: " + statusCode);
                                    setFailure(statusCode);
                                }
                            }
                        });
            }
        }

        private GoogleSignInClient buildClient() {
            Log.d(TAG,"Building client for: " + this);
            GoogleSignInOptions.Builder builder = new GoogleSignInOptions.Builder();
            if (doAuthCode) {
                if (!getWebClientId().isEmpty() && !getWebClientId().equals("__WEB_CLIENTID__")) {
                    builder.requestServerAuthCode(getWebClientId(), forceRefresh);
                } else {
                    Log.e(TAG, "Web client ID is needed for Auth Code");
                    setFailure(CommonStatusCodes.DEVELOPER_ERROR);
                    return null;
                }
            }
    
            if (doEmail) {
                builder.requestEmail();
            }
    
            if (doIdToken) {
                if (!getWebClientId().isEmpty() && !getWebClientId().equals("__WEB_CLIENTID__")) {
                    builder.requestIdToken(getWebClientId());
                } else {
                    Log.e(TAG, "Web client ID is needed for ID Token");
                    setFailure(CommonStatusCodes.DEVELOPER_ERROR);
                    return null;
                }
            }
            if (scopes != null) {
                for (Scope s : scopes) {
                    builder.requestScopes(s);
                }
            }
    
            if (hidePopups) {
                Log.d(TAG, "hiding popup views for games API");
                builder.addExtension(Games.GamesOptions.builder().setShowConnectingPopup(false).build());
            }
    
            if (accountName != null && !accountName.isEmpty()) {
                builder.setAccountName(accountName);
            }
    
            GoogleSignInOptions options = builder.build();
            return GoogleSignIn.getClient(helperFragment.getActivity(), options);
        }

        private boolean canReuseAccount() {
            return !doAuthCode && !doIdToken;
        }

        private String getWebClientId() {
            return webClientId == null ? "" : webClientId;
        }

        void setFailure(int code) {
            Log.e(TAG,"Setting result error status code to: " + code);
            pendingResponse.setStatus(code);
            resultTaskSource.setException(new ApiException(new Status(code)));
            TokenFragment.finishRequest(this);
        }

        private void setSuccess(GoogleSignInAccount account) {
            resultTaskSource.setResult(account);
            pendingResponse.setAccount(account);
            pendingResponse.setStatus(CommonStatusCodes.SUCCESS);
            TokenFragment.finishRequest(this);
        }

        @Override
        public String toString() {
            return Integer.toHexString(hashCode()) + " (a:" +
                    doAuthCode + " e:" + doEmail + " i:" + doIdToken +
                    " wc: " + webClientId + " f: " + forceRefresh +")";
        }
    }

    public static GoogleSignInAccount getAccount(Activity activity) {
        return GoogleSignIn.getLastSignedInAccount(activity);
    }

    public static View createInvisibleView(Activity parentActivity) {
        View view = new View(parentActivity);
        view.setVisibility(View.INVISIBLE);
        view.setClickable(false);
        return view;
    }
}
