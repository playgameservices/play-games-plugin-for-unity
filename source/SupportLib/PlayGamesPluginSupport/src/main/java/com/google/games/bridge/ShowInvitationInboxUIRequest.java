package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.games.InvitationsClient;
import com.google.android.gms.games.multiplayer.Invitation;
import com.google.android.gms.games.multiplayer.Multiplayer;
import com.google.android.gms.games.multiplayer.Participant;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;

class ShowInvitationInboxUIRequest implements HelperFragment.Request {
    private static final String TAG = "ShowInvitationInboxUI";

    private final TaskCompletionSource<Result> resultTaskSource = new TaskCompletionSource<>();

    public ShowInvitationInboxUIRequest() {}

    public class Result {
        public int status;
        public Invitation invitation;

        Result(int status, Invitation invitation) {
            this.status = status;
            this.invitation = invitation;
        }
    }

    Task<Result> getTask() {
        return resultTaskSource.getTask();
    }

    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        InvitationsClient client = Games.getInvitationsClient(activity, account);
        client
            .getInvitationInboxIntent()
            .addOnSuccessListener(
                activity,
                    new OnSuccessListener<Intent>() {
                        @Override
                        public void onSuccess(Intent intent) {
                            Utils.startActivityForResult(helperFragment, intent, HelperFragment.RC_SHOW_INVITATION_INBOX_UI);
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
        if (requestCode == HelperFragment.RC_SHOW_INVITATION_INBOX_UI) {
            if (resultCode == Activity.RESULT_OK) {
                Invitation invitation = (Invitation) data.getParcelableExtra(Multiplayer.EXTRA_INVITATION);
                setResult(CommonUIStatus.VALID, invitation);
            } else if (resultCode == Activity.RESULT_CANCELED) {
                setResult(CommonUIStatus.CANCELLED);
            } else if (resultCode == GamesActivityResultCodes.RESULT_RECONNECT_REQUIRED) {
                setResult(CommonUIStatus.NOT_AUTHORIZED);
            } else {
                Log.d(TAG, "onActivityResult unknown resultCode: " + resultCode);
                setResult(CommonUIStatus.INTERNAL_ERROR);
            }
        }
    }

    void setResult(int status, Invitation invitation) {
        Result result = new Result(status, invitation);
        resultTaskSource.setResult(result);
        HelperFragment.finishRequest(this);
    }

    void setResult(int status) {
        setResult(status, null);
    }

    void setFailure(Exception e) {
        resultTaskSource.setException(e);
        HelperFragment.finishRequest(this);
    }
}
