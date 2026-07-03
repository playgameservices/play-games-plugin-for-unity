/*
 * Copyright (C) Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;

/**
 * A simple bridge activity that receives an intent from the Google Play Games
 * native SDK and forwards the result of executing the intent back to the SDK
 * via JNI.
 */

public final class NativeBridgeActivity extends Activity {

    private static final String BRIDGED_INTENT = "BRIDGED_INTENT";
    private static final int GPG_RESPONSE_CODE = 0x475047;
    private static final int BG_COLOR = 0x40ffffff;
    private static final String TAG = "NativeBridgeActivity";

    // true indicates we are waiting for the activity to return so we can
    // forward the response.
    // if onDestroy is called with a pending result, the result is simulated and
    // forwarded to the SDK.
    private boolean pendingResult;

    // This method should be implemented by invoking gpg::AndroidSupport::OnActivityResult
    private native void forwardActivityResult(int requestCode, int resultCode, Intent data);

    static {
        System.loadLibrary("gpg");
    }

    public void onCreate(Bundle savedInstanceState) {
        View v = new View(this);
        v.setBackgroundColor(BG_COLOR);
        setContentView(v);
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onStart() {
        Intent bridgedIntent = getIntent().getParcelableExtra(BRIDGED_INTENT);
        if (bridgedIntent != null) {
            startActivityForResult(bridgedIntent, GPG_RESPONSE_CODE);
        }
        super.onStart();
    }

    /**
     * Same as calling {@link #startActivityForResult(android.content.Intent, int, android.os.Bundle)}
     * with no options.
     *
     * @param intent      The intent to start.
     * @param requestCode If >= 0, this code will be returned in
     *                    onActivityResult() when the activity exits.
     * @throws android.content.ActivityNotFoundException
     * @see #startActivity
     */
    @Override
    public void startActivityForResult(Intent intent, int requestCode) {
        // set the pending result flag if this activity is for GPG
        pendingResult = requestCode == GPG_RESPONSE_CODE;
        if (pendingResult) {
            Log.d(TAG, "starting GPG activity: " + intent);
        } else {
            Log.i(TAG, "starting non-GPG activity: " + requestCode + " " + intent);
        }
        super.startActivityForResult(intent, requestCode);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == GPG_RESPONSE_CODE) {
            Log.d(TAG, "Forwarding activity result to native SDK.");
            forwardActivityResult(requestCode, resultCode, data);

            // clear the pending flag.
            pendingResult = false;
        } else {
            Log.d(TAG, "onActivityResult for unknown request code: " + requestCode + " calling finish()");
        }

        finish();

        super.onActivityResult(requestCode, resultCode, data);
    }

    public static void launchBridgeIntent(Activity parentActivity, Intent intent) {


        Log.d(TAG, "Launching bridge activity: parent:" + parentActivity + " intent " + intent);
        Intent bridgeIntent = new Intent(parentActivity, NativeBridgeActivity.class);
        bridgeIntent.putExtra(BRIDGED_INTENT, intent);
        parentActivity.startActivity(bridgeIntent);
    }

    /**
     * Perform any final cleanup before an activity is destroyed.  This can
     * happen either because the activity is finishing (someone called
     * {@link #finish} on it, or because the system is temporarily destroying
     * this instance of the activity to save space.  You can distinguish
     * between these two scenarios with the {@link #isFinishing} method.
     * <p/>
     * <p><em>Note: do not count on this method being called as a place for
     * saving data! For example, if an activity is editing data in a content
     * provider, those edits should be committed in either {@link #onPause} or
     * {@link #onSaveInstanceState}, not here.</em> This method is usually implemented to
     * free resources like threads that are associated with an activity, so
     * that a destroyed activity does not leave such things around while the
     * rest of its application is still running.  There are situations where
     * the system will simply kill the activity's hosting process without
     * calling this method (or any others) in it, so it should not be used to
     * do things that are intended to remain around after the process goes
     * away.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onPause
     * @see #onStop
     * @see #finish
     * @see #isFinishing
     */
    @Override
    protected void onDestroy() {

        // if we have a pending Result and being destroyed, this most likely
        // means the activity we are waiting for is also destroyed, so cancel it out
        // with the SDK.
        if (pendingResult) {
            Log.w(TAG, "onDestroy called with pendingResult == true.  forwarding canceled result");
            forwardActivityResult(GPG_RESPONSE_CODE, RESULT_CANCELED, null);
            pendingResult = false;
        }

        super.onDestroy();
    }
}
