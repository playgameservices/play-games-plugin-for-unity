package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;


class SimpleUiRequest implements HelperFragment.Request {
    private static final String TAG = "SimpleUiRequest";

    private final TaskCompletionSource<Integer> resultTaskSource = new TaskCompletionSource<>();

    public Task<Integer> getTask() {
        return resultTaskSource.getTask();
    }

    protected Task<Intent> getIntent(Activity activity) {
        return null;
    }

    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        getIntent(activity)
            .addOnSuccessListener(
                activity,
                new OnSuccessListener<Intent>() {
                    @Override
                    public void onSuccess(Intent intent) {
                        Utils.startActivityForResult(helperFragment, intent, HelperFragment.RC_SIMPLE_UI);
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
        if (requestCode == HelperFragment.RC_SIMPLE_UI) {
            if (resultCode == Activity.RESULT_OK || resultCode == Activity.RESULT_CANCELED) {
                setResult(CommonUIStatus.VALID);
            } else if (resultCode == GamesActivityResultCodes.RESULT_RECONNECT_REQUIRED) {
                setResult(CommonUIStatus.NOT_AUTHORIZED);
            } else {
                Log.d(TAG, "onActivityResult unknown resultCode: " + resultCode);
                setResult(CommonUIStatus.INTERNAL_ERROR);
            }
        }
    }

    void setResult(int result) {
        resultTaskSource.setResult(result);
        HelperFragment.finishRequest(this);
    }

    void setFailure(Exception e) {
        resultTaskSource.setException(e);
        HelperFragment.finishRequest(this);
    }
}
