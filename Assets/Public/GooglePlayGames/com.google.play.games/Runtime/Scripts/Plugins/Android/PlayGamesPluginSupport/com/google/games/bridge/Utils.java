package com.google.games.bridge;

import android.app.Fragment;
import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.util.Log;

/**
 * Utility functions
 */
public final class Utils {

    private static final String TAG = "Utils";

    private Utils() {}

    public static void startActivityForResult(Fragment fragment, Intent intent, int requestCode) {
        try {
            fragment.startActivityForResult(intent, requestCode);
        } catch (ActivityNotFoundException e) {
            Log.e(TAG,"Activity not found. Please install Play Games App.");
        }
    }
}
