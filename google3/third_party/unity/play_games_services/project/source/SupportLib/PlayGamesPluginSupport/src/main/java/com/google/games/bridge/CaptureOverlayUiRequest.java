package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.VideosClient;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;


class CaptureOverlayUiRequest implements HelperFragment.Request {
    private static final String TAG = "CaptureOverlayUiRequest";

    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        VideosClient client = Games.getVideosClient(activity, account);
        client
            .getCaptureOverlayIntent()
            .addOnSuccessListener(
                activity,
                new OnSuccessListener<Intent>() {
                    @Override
                    public void onSuccess(Intent intent) {
                        Utils.startActivityForResult(helperFragment, intent, HelperFragment.RC_CAPTURE_OVERLAY_UI);
                        helperFragment.finishRequest(CaptureOverlayUiRequest.this);
                    }
                })
            .addOnFailureListener(
                activity,
                new OnFailureListener() {
                    @Override
                    public void onFailure(Exception e) {
                        Log.d(TAG, "Show CaptureOverlay UI failed");
                    }
                });
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {
    }
}
