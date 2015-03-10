package com.google.games.bridge;


import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.WindowManager;

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
        startActivityForResult(bridgedIntent, GPG_RESPONSE_CODE);

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
        super.startActivityForResult(intent, requestCode);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
       if (requestCode == GPG_RESPONSE_CODE) {
            Log.d(TAG, "Forwarding activity result to native SDK.");
            forwardActivityResult(requestCode, resultCode, data);

            // clear the pending flag.
            pendingResult = false;
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
            forwardActivityResult(GPG_RESPONSE_CODE,RESULT_CANCELED, null);
            pendingResult = false;
        }

        super.onDestroy();
    }




    /**
     * Called after {@link #onStop} when the current activity is being
     * re-displayed to the user (the user has navigated back to it).  It will
     * be followed by {@link #onStart} and then {@link #onResume}.
     * <p/>
     * <p>For activities that are using raw {@link Cursor} objects (instead of
     * creating them through
     * {@link #managedQuery(android.net.Uri, String[], String, String[], String)},
     * this is usually the place
     * where the cursor should be requeried (because you had deactivated it in
     * {@link #onStop}.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onStop
     * @see #onStart
     * @see #onResume
     */
    @Override
    protected void onRestart() {
        Log.d(TAG, "onRestart");

        super.onRestart();
    }

    /**
     * Called as part of the activity lifecycle when an activity is going into
     * the background, but has not (yet) been killed.  The counterpart to
     * {@link #onResume}.
     * <p/>
     * <p>When activity B is launched in front of activity A, this callback will
     * be invoked on A.  B will not be created until A's {@link #onPause} returns,
     * so be sure to not do anything lengthy here.
     * <p/>
     * <p>This callback is mostly used for saving any persistent state the
     * activity is editing, to present a "edit in place" model to the user and
     * making sure nothing is lost if there are not enough resources to start
     * the new activity without first killing this one.  This is also a good
     * place to do things like stop animations and other things that consume a
     * noticeable amount of CPU in order to make the switch to the next activity
     * as fast as possible, or to close resources that are exclusive access
     * such as the camera.
     * <p/>
     * <p>In situations where the system needs more memory it may kill paused
     * processes to reclaim resources.  Because of this, you should be sure
     * that all of your state is saved by the time you return from
     * this function.  In general {@link #onSaveInstanceState} is used to save
     * per-instance state in the activity and this method is used to store
     * global persistent data (in content providers, files, etc.)
     * <p/>
     * <p>After receiving this call you will usually receive a following call
     * to {@link #onStop} (after the next activity has been resumed and
     * displayed), however in some cases there will be a direct call back to
     * {@link #onResume} without going through the stopped state.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onResume
     * @see #onSaveInstanceState
     * @see #onStop
     */
    @Override
    protected void onPause() {
        Log.d(TAG, "onPause");

        super.onPause();
    }



    /**
     * Called when you are no longer visible to the user.  You will next
     * receive either {@link #onRestart}, {@link #onDestroy}, or nothing,
     * depending on later user activity.
     * <p/>
     * <p>Note that this method may never be called, in low memory situations
     * where the system does not have enough memory to keep your activity's
     * process running after its {@link #onPause} method is called.
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onRestart
     * @see #onResume
     * @see #onSaveInstanceState
     * @see #onDestroy
     */
    @Override
    protected void onStop() {
        Log.d(TAG, "onStop");

        super.onStop();
    }



    /**
     * Called after {@link #onRestoreInstanceState}, {@link #onRestart}, or
     * {@link #onPause}, for your activity to start interacting with the user.
     * This is a good place to begin animations, open exclusive-access devices
     * (such as the camera), etc.
     * <p/>
     * <p>Keep in mind that onResume is not the best indicator that your activity
     * is visible to the user; a system window such as the keyguard may be in
     * front.  Use {@link #onWindowFocusChanged} to know for certain that your
     * activity is visible to the user (for example, to resume a game).
     * <p/>
     * <p><em>Derived classes must call through to the super class's
     * implementation of this method.  If they do not, an exception will be
     * thrown.</em></p>
     *
     * @see #onRestoreInstanceState
     * @see #onRestart
     * @see #onPostResume
     * @see #onPause
     */
    @Override
    protected void onResume() {
        Log.d(TAG, "onResume");

        super.onResume();
    }
}
