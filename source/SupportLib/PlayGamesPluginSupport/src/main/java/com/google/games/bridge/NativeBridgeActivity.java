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
        Intent bridgedIntent = (Intent) getIntent().getParcelableExtra(BRIDGED_INTENT);
        startActivityForResult(bridgedIntent, GPG_RESPONSE_CODE);

        super.onStart();
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == GPG_RESPONSE_CODE) {
            Log.d(TAG, "Forwarding activity result to native SDK.");
            forwardActivityResult(requestCode, resultCode, data);
        }

        finish();

        super.onActivityResult(requestCode, resultCode, data);
    }

    public static void launchBridgeIntent(Activity parentActivity, Intent intent) {
        Log.d(TAG, "Launching bridge activity");
        Intent bridgeIntent = new Intent(parentActivity, NativeBridgeActivity.class);
        bridgeIntent.putExtra(BRIDGED_INTENT, intent);
        parentActivity.startActivity(bridgeIntent);
    }
}
