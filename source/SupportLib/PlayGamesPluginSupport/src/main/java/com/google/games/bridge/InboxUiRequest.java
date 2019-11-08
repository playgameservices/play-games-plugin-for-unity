package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.games.multiplayer.Multiplayer;
import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatch;
import com.google.android.gms.games.TurnBasedMultiplayerClient;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;


class InboxUiRequest implements HelperFragment.Request {
    private static final String TAG = "SimpleUiRequest";

    private final TaskCompletionSource<Result> resultTaskSource = new TaskCompletionSource<>();

    public class Result {
        public int status;
        public TurnBasedMatch turnBasedMatch;

        Result(int status, TurnBasedMatch turnBasedMatch) {
            this.status = status;
            this.turnBasedMatch = turnBasedMatch;
        }
    }

    public Task<Result> getTask() {
        return resultTaskSource.getTask();
    }

    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        TurnBasedMultiplayerClient client = Games.getTurnBasedMultiplayerClient(activity, account);
        client.getInboxIntent()
            .addOnSuccessListener(
                activity,
                new OnSuccessListener<Intent>() {
                    @Override
                    public void onSuccess(Intent intent) {
                        Utils.startActivityForResult(helperFragment, intent, HelperFragment.RC_INBOX_UI);
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
        if (requestCode == HelperFragment.RC_INBOX_UI) {
            if (resultCode == Activity.RESULT_OK) {
                setResult(CommonUIStatus.VALID, (TurnBasedMatch) data.getParcelableExtra(Multiplayer.EXTRA_TURN_BASED_MATCH));
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

    void setResult(int status, TurnBasedMatch turnBasedMatch) {
        Result result = new Result(status, turnBasedMatch);
        resultTaskSource.setResult(result);
        HelperFragment.finishRequest(this);
    }

    void setResult(int result) {
        setResult(result, /* turnBasedMatch= */ null);
    }

    void setFailure(Exception e) {
        resultTaskSource.setException(e);
        HelperFragment.finishRequest(this);
    }
}
