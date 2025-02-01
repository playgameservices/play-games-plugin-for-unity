package com.google.games.bridge;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;

class GenericResolutionUiRequest implements HelperFragment.Request {
    private static final String TAG = "FriendsSharingConsent";
    static final int SELECT_UI_STATUS_RESULT_OK = 1; // UIStatus.Valid
    static final int SELECT_UI_STATUS_USER_CLOSED_UI = -6; // UIStatus.UserClosedUI
    static final int SELECT_UI_STATUS_INTERNAL_ERROR = -2; // UIStatus.InternalError

    private final PendingIntent pendingIntent;

    private final TaskCompletionSource<Integer> resultTaskSource = new TaskCompletionSource<>();

    GenericResolutionUiRequest(PendingIntent pendingIntent) {
        this.pendingIntent = pendingIntent;
    }

    Task<Integer> getTask() {
        return resultTaskSource.getTask();
    }

    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        Intent intent = new Intent(activity, GenericResolutionActivity.class);
        intent.putExtra("RequestFriendsAccessPermissionPendingIntent", pendingIntent);
        helperFragment.startActivityForResult(intent, HelperFragment.RC_RESOLUTION_DIALOG);
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == HelperFragment.RC_RESOLUTION_DIALOG) {
            if (resultCode == Activity.RESULT_OK) {
                setResult(SELECT_UI_STATUS_RESULT_OK);
            } else if (resultCode == Activity.RESULT_CANCELED) {
                setResult(SELECT_UI_STATUS_USER_CLOSED_UI);
            } else {
                Log.d(TAG, "onActivityResult unknown resultCode: " + resultCode);
                setResult(SELECT_UI_STATUS_INTERNAL_ERROR);
            }
        }
    }

    void setResult(Integer status) {
        resultTaskSource.setResult(status);
        HelperFragment.finishRequest(this);
    }

    void setFailure(Exception e) {
        resultTaskSource.setException(e);
        HelperFragment.finishRequest(this);
    }
}
