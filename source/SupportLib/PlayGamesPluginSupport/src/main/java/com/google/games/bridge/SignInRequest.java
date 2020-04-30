package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.auth.api.Auth;
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInClient;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.auth.api.signin.GoogleSignInResult;
import com.google.android.gms.auth.api.signin.GoogleSignInStatusCodes;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.Scope;
import com.google.android.gms.common.api.Status;
import com.google.android.gms.games.Games;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;

class SignInRequest implements HelperFragment.Request {
    private static final String TAG = "SignInRequest";

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

    private HelperFragment helperFragment;

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

    public void process(HelperFragment helperFragment) {
        this.helperFragment = helperFragment;
        signIn();
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == HelperFragment.RC_SIGN_IN) {
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

    if (signInClient == null) {
      return;
    }

    Activity activity = helperFragment.getActivity();
    Log.d(TAG, "canReuseAccount: " + canReuseAccount());
    if (canReuseAccount()) {
      final GoogleSignInAccount account = GoogleSignIn.getLastSignedInAccount(activity);
      Log.d(TAG, "lastSignedInAccount is " + (account != null ? "not null" : "null"));
      if (GoogleSignIn.hasPermissions(account, scopes)) {
        Log.d(TAG, "Checking the last signed-in account if it can be used.");
        Games.getGamesClient(activity, account)
            .getAppId()
            .addOnCompleteListener(
                new OnCompleteListener<String>() {
                  @Override
                  public void onComplete(/* @NonNull */ Task<String> task) {
                    if (task.isSuccessful()) {
                      Log.d(TAG, "Signed-in with the last signed-in account.");
                      setSuccess(account);
                    } else {
                      Log.d(TAG, "getAppId task is not successful." + "Trying to sign out.");
                      signInClient
                          .signOut()
                          .addOnCompleteListener(
                              new OnCompleteListener<Void>() {
                                @Override
                                public void onComplete(/* @NonNull */ Task<Void> task) {
                                  if (task.isSuccessful()) {
                                    Log.d(
                                        TAG,
                                        "Can't reuse the last signed-in account. Second attempt"
                                            + " after sign out.");
                                    // Last signed account should be null now
                                    signIn();
                                  } else {
                                    Log.e(
                                        TAG,
                                        "Can't reuse the last signed-in account and sign out"
                                            + " failed.");
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
    signInClient
        .silentSignIn()
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
                if (statusCode == CommonStatusCodes.SIGN_IN_REQUIRED
                    || statusCode == CommonStatusCodes.INTERNAL_ERROR
                    || statusCode == CommonStatusCodes.RESOLUTION_REQUIRED) {
                  if (!silent) {
                    Intent signInIntent = signInClient.getSignInIntent();
                    helperFragment.startActivityForResult(signInIntent, HelperFragment.RC_SIGN_IN);
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
        HelperFragment.finishRequest(this);
    }

    private void setSuccess(GoogleSignInAccount account) {
        resultTaskSource.setResult(account);
        pendingResponse.setAccount(account);
        pendingResponse.setStatus(CommonStatusCodes.SUCCESS);
        HelperFragment.finishRequest(this);
    }

    @Override
    public String toString() {
        return Integer.toHexString(hashCode()) + " (a:" +
                doAuthCode + " e:" + doEmail + " i:" + doIdToken +
                " wc: " + webClientId + " f: " + forceRefresh +")";
    }
}
