package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.auth.api.Auth;
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.auth.api.signin.GoogleSignInResult;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.Scope;
import com.google.android.gms.common.api.Status;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;
import java.util.ArrayList;

class RequestPermissionsRequest implements HelperFragment.Request {
  private static final String TAG = "RequestPermissions";

  private final TaskCompletionSource<GoogleSignInAccount> resultTaskSource =
      new TaskCompletionSource<>();
  private Scope[] scopes;
  private HelperFragment helperFragment;

  public RequestPermissionsRequest(Scope[] scopes) {
    this.scopes = scopes;
  }

  Task<GoogleSignInAccount> getTask() {
    return resultTaskSource.getTask();
  }

  public void process(HelperFragment helperFragment) {
    this.helperFragment = helperFragment;
    final Activity activity = helperFragment.getActivity();
    GoogleSignInAccount account = HelperFragment.getAccount(activity);
    Scope[] unauthorizedScopes = getUnauthorizedScopes(account, scopes);
    if (unauthorizedScopes.length == 0) {
      setSuccess(GoogleSignIn.getAccountForScopes(activity, scopes[0], scopes));
    } else {
      Intent intent = getSignInIntentForAccountAndScopes(activity, account, unauthorizedScopes);
      helperFragment.startActivityForResult(intent, HelperFragment.RC_SHOW_REQUEST_PERMISSIONS_UI);
    }
  }

  private Scope[] getUnauthorizedScopes(GoogleSignInAccount account, Scope[] scopes) {
    ArrayList<Scope> unauthorizedScopes = new ArrayList<Scope>();
    for (Scope scope : scopes) {
      if (!GoogleSignIn.hasPermissions(account, scope)) {
        unauthorizedScopes.add(scope);
      }
    }
    return unauthorizedScopes.toArray(new Scope[unauthorizedScopes.size()]);
  }

  private static Intent getSignInIntentForAccountAndScopes(
      /* @NonNull */ Activity activity, /* @Nullable */
      GoogleSignInAccount account, /* @NonNull */
      Scope... scopes) {
    GoogleSignInOptions.Builder optionsBuilder = new GoogleSignInOptions.Builder();

    if (scopes.length > 0) {
      optionsBuilder.requestScopes(scopes[0], scopes);
    }

    if (account != null && account.getEmail() != null) {
      optionsBuilder.setAccountName(account.getEmail());
    }

    return GoogleSignIn.getClient(activity, optionsBuilder.build()).getSignInIntent();
  }

  public void onActivityResult(int requestCode, int resultCode, Intent data) {
    if (requestCode == HelperFragment.RC_SHOW_REQUEST_PERMISSIONS_UI) {
      GoogleSignInResult result = Auth.GoogleSignInApi.getSignInResultFromIntent(data);
      if (result != null && result.isSuccess()) {
        GoogleSignInAccount account =
            GoogleSignIn.getAccountForScopes(helperFragment.getActivity(), scopes[0], scopes);
        setSuccess(account);
      } else if (resultCode == Activity.RESULT_CANCELED) {
        if (result != null) {
          setFailure(result.getStatus().getStatusCode());
        } else {
          setFailure(CommonStatusCodes.CANCELED);
        }
      } else if (result != null) {
        setFailure(result.getStatus().getStatusCode());
      } else {
        setFailure(CommonStatusCodes.ERROR);
      }
    }
  }

  void setFailure(int code) {
    resultTaskSource.setException(new ApiException(new Status(code)));
    HelperFragment.finishRequest(this);
  }

  private void setSuccess(GoogleSignInAccount account) {
    resultTaskSource.setResult(account);
    HelperFragment.finishRequest(this);
  }
}
