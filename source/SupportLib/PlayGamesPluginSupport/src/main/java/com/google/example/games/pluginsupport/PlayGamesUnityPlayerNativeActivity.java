package com.google.example.games.pluginsupport;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayerNativeActivity;

import java.util.ArrayList;

@Deprecated
public class PlayGamesUnityPlayerNativeActivity extends UnityPlayerNativeActivity
        implements AndroidLifecycleReporter {

    private static final String TAG = "PlayGamesUnityNativeActivity";
    boolean DEBUG = true;
    AndroidLifecycleListener mListener = null;

    @Override
    public void registerAndroidLifecycleListener(AndroidLifecycleListener listener) {
        if (listener == null) {
            Log.w(TAG, "**** WARNING: registering a null listener.");
        }
        debug("Registered new lifecycle listener.");
        mListener = listener;
    }

    @Override
    public void unregisterAndroidLifecycleListener(AndroidLifecycleListener listener) {
        if (mListener == listener) {
            debug("Unregistering listener.");
            mListener = null;
        }
    }

    @Override
    public void onStart() {
        debug("onStart: entered.");

        debug("onStart: calling Unity's onStart().");
        super.onStart();
        debug("onStart: returned from Unity's onStart().");

        if (mListener != null) {
            debug("Calling listener: onStart.");
            mListener.onStart(this);
        }
        debug("onStart: leaving.");
    }

    @Override
    public void onStop() {
        debug("onStop: entered.");

        if (mListener != null) {
            debug("Calling listener: onStop.");
            mListener.onStop(this);
        }

        debug("onStop: calling Unity's onStop()");
        super.onStop();
        debug("onStop: returned from Unity's onStop");
        debug("onStop: leaving.");
    }

    @Override
    public void onPause() {
        debug("onPause: entered.");

        if (mListener != null) {
            debug("Calling listener: onPause");
            mListener.onPause(this);
        }

        debug("onPause: calling Unity's onPause()");
        super.onPause();
        debug("onPause: returned from Unity's onPause");
        debug("onPause: leaving.");
    }

    @Override
    public void onResume() {
        debug("onResume: entered.");

        debug("onResume: calling Unity's onResume().");
        super.onResume();
        debug("onResume: returned from Unity's onResume().");

        if (mListener != null) {
            debug("Calling listener: onResume");
            mListener.onResume(this);
        }

        debug("onResume: leaving.");
    }

    @Override
    public void onDestroy() {
        debug("onDestroy: entered.");

        if (mListener != null) {
            debug("Calling listener: onDestroy");
            mListener.onDestroy(this);
        }

        debug("onDestroy: calling Unity's onDestroy()");
        super.onDestroy();
        debug("onDestroy: returned from Unity's onDestroy");
        debug("onDestroy: leaving.");

    }

    @Override
    public void onActivityResult(int req, int resp, Intent data) {
        debug("onActivityResult: entered.");
        debug("onActivityResult: req=" + req + ", resp=" + resp + ", data=" + data);

        if (mListener != null) {
            debug("Calling listener: onActivityResult");
            mListener.onActivityResult(this, req, resp, data);
        }

        debug("onActivityResult: calling Unity's onActivityResult()");
        super.onActivityResult(req, resp, data);
        debug("onActivityResult: returned from Unity's onActivityResult");
        debug("onActivityResult: leaving.");
    }

    @Override
    public int getAndroidLifecycleReporterVersion() {
        return 1;
    }

    private void debug(String msg) {
        if (DEBUG) {
            Log.d("PlayGamesUnityPlayerNativeActivity", "DEBUG: " + msg);
        }
    }
}
