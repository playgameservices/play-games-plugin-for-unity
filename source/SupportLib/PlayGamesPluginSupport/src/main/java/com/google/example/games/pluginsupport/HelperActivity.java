package com.google.example.games.pluginsupport;


import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.view.View;

import com.google.example.games.basegameutils.GameHelper;

public class HelperActivity extends Activity implements GameHelper.GameHelperListener {
    String TAG = "PluginSupport";
    static final int BG_COLOR = 0x40ffffff;
    protected GameHelper mHelper = null;
    public static String EXTRA_DEBUG_ENABLED = "EXTRA_DEBUG_ENABLED";
    protected boolean mDebugEnabled = false;

    HelperActivity() {
        TAG += "/" + getClass().getSimpleName();
    }

    @Override
    protected void onStop() {
        debugLog("onStop()");
        mHelper.onStop();
        super.onStop();
    }

    @Override
    protected void onStart() {
        debugLog("onStart()");
        mHelper.onStart(this);
        super.onStart();
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        debugLog("onActivityResult(" + requestCode + ", " + resultCode + ", " + data);
        mHelper.onActivityResult(requestCode, resultCode, data);
        super.onActivityResult(requestCode, resultCode, data);
    }

    public void onCreate(Bundle savedInstanceState) {
        mDebugEnabled = getIntent() != null &&
                getIntent().getBooleanExtra(EXTRA_DEBUG_ENABLED, false);
        debugLog("onCreate()");

        mHelper = new GameHelper(this, GameHelper.CLIENT_ALL);
        mHelper.enableDebugLog(mDebugEnabled, TAG + "/GameHelper");
        mHelper.setShowErrorDialogs(false);
        mHelper.setup(this);
        View v = new View(this);
        v.setBackgroundColor(BG_COLOR);
        setContentView(v);
        super.onCreate(savedInstanceState);
    }

    @Override
    public void onSignInFailed() {
        debugLog("onSignInFailed()");
    }

    @Override
    public void onSignInSucceeded() {
        debugLog("onSignInSuceeded()");
    }

    protected void debugLog(String message) {
        if (mDebugEnabled) {
            Log.d(TAG, message);
        }
    }
}
