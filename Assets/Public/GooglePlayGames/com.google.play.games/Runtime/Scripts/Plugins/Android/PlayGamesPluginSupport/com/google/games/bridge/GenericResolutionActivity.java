package com.google.games.bridge;

import android.app.Activity;
import android.app.PendingIntent;
import android.content.Intent;
import android.content.IntentSender.SendIntentException;
import android.os.Bundle;

/**
 * Activity used for showing UI requesting the user to complete an action.
 */
public final class GenericResolutionActivity extends Activity {
    static final int SELECT_UI_STATUS_RESULT_OK = Activity.RESULT_OK;
    static final int SELECT_UI_STATUS_INTERNAL_ERROR = Activity.RESULT_CANCELED;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        PendingIntent pendingIntent =
                getIntent().getParcelableExtra("RequestFriendsAccessPermissionPendingIntent");
        try {
            this.startIntentSenderForResult(
                    pendingIntent.getIntentSender(),
                    HelperFragment.RC_RESOLUTION_DIALOG, /* fillInIntent */
                    null,
                    /* flagsMask */ 0,
                    /* flagsValues */ 0,
                    /* extraFlags */ 0,
                    /* options */ null);
        } catch (SendIntentException e) {
            finishWithResult(SELECT_UI_STATUS_INTERNAL_ERROR);
        }
    }

    @Override
    public void onActivityResult(int requestCode, int result, Intent data) {
        if (requestCode == HelperFragment.RC_RESOLUTION_DIALOG) {
            if (result == Activity.RESULT_OK) {
                finishWithResult(SELECT_UI_STATUS_RESULT_OK);
            } else {
                finishWithResult(SELECT_UI_STATUS_INTERNAL_ERROR);
            }
        } else {
            super.onActivityResult(requestCode, result, data);
        }
    }

    private void finishWithResult(int resultCode) {
        setResult(resultCode);
        finish();
    }
}
