package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.google.android.gms.games.Games;
import com.google.android.gms.games.multiplayer.Multiplayer;
import com.google.android.gms.games.multiplayer.realtime.RoomConfig;
import com.google.example.games.basegameutils.GameHelper;

import java.util.ArrayList;

public abstract class UiHelperActivity extends HelperActivity {
    boolean mAttempted = false;
    static final int RC_UI = 9876;

    protected abstract void deliverFailure();
    protected abstract void deliverSuccess(Intent data);
    protected abstract Intent getUiIntent();

    @Override
    public void onSignInFailed() {
        super.onSignInFailed();
        Log.e(TAG, "Sign-in failed on UI helper activity.");
        GameHelper.SignInFailureReason reason = mHelper.getSignInError();
        if (reason != null) {
            Log.e(TAG, "Sign-in failure reason: " + reason.toString());
        }
        debugLog("Delivering failure to listener.");
        deliverFailure();
        finish();
    }

    @Override
    public void onSignInSucceeded() {
        super.onSignInSucceeded();
        if (mAttempted) {
            Log.w(TAG, "Ignoring onSignInSuceeded because we already launched the UI.");
            return;
        }
        mAttempted = true;
        Intent i = getUiIntent();
        debugLog("Launching intent");
        startActivityForResult(i, RC_UI);
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode != RC_UI) {
            debugLog("Ignoring activity result with request code " + requestCode);
            return;
        }

        if (resultCode != Activity.RESULT_OK) {
            Log.w(TAG, "UI cancelled.");
            deliverFailure();
            finish();
            return;
        }
        debugLog("UI succeeded.");
        deliverSuccess(data);
        finish();
    }
}
