package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.SnapshotsClient;
import com.google.android.gms.games.snapshot.SnapshotMetadata;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;


class SelectSnapshotUiRequest implements HelperFragment.Request {
    private static final String TAG = "SelectSnapshotUiRequest";

    /**
     * Should be aligned to:
     * PluginDev/Assets/GooglePlayGames/BasicApi/SavedGame/ISavedGameClient.cs enum SelectUIStatus
     * */
    static final int SELECT_UI_STATUS_GAME_SELECTED = 1;
    static final int SELECT_UI_STATUS_USER_CLOSED_UI = 2;
    static final int SELECT_UI_STATUS_INTERNAL_ERROR = -1;
    static final int SELECT_UI_STATUS_AUTHENTICATION_ERROR = -3;
    static final int SELECT_UI_STATUS_UI_BUSY = -5;

    private final TaskCompletionSource<Result> resultTaskSource = new TaskCompletionSource<>();

    private String title;
    private boolean allowAddButton;
    private boolean allowDelete;
    private int maxSnapshots;

    public class Result {
        public int status;
        public SnapshotMetadata metadata;

        Result(int status, SnapshotMetadata metadata) {
            this.status = status;
            this.metadata = metadata;
        }
    }

    SelectSnapshotUiRequest(/* @NonNull */ String title, boolean allowAddButton, boolean allowDelete, int maxSnapshots)
    {
        this.title = title;
        this.allowAddButton = allowAddButton;
        this.allowDelete = allowDelete;
        this.maxSnapshots = maxSnapshots;
    }

    Task<Result> getTask() {
        return resultTaskSource.getTask();
    }

    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        SnapshotsClient client = PlayGames.getSnapshotsClient(activity);
        client
            .getSelectSnapshotIntent(title, allowAddButton, allowDelete, maxSnapshots)
            .addOnSuccessListener(
                activity,
                new OnSuccessListener<Intent>() {
                    @Override
                    public void onSuccess(Intent intent) {
                        Utils.startActivityForResult(helperFragment, intent, HelperFragment.RC_SELECT_SNAPSHOT_UI);
                    }
                })
            .addOnFailureListener(
                activity,
                new OnFailureListener() {
                    @Override
                    public void onFailure(Exception e) {
                        setFailure(e);
                    }
                });
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == HelperFragment.RC_SELECT_SNAPSHOT_UI) {
            if (resultCode == Activity.RESULT_OK) {
                setResult(SELECT_UI_STATUS_GAME_SELECTED, (SnapshotMetadata) data.getParcelableExtra(SnapshotsClient.EXTRA_SNAPSHOT_METADATA));
            } else if (resultCode == Activity.RESULT_CANCELED) {
                setResult(SELECT_UI_STATUS_USER_CLOSED_UI);
            } else if (resultCode == GamesActivityResultCodes.RESULT_RECONNECT_REQUIRED) {
                setResult(SELECT_UI_STATUS_AUTHENTICATION_ERROR);
            } else {
                Log.d(TAG, "onActivityResult unknown resultCode: " + resultCode);
                setResult(SELECT_UI_STATUS_INTERNAL_ERROR);
            }
        }
    }

    void setResult(int status, SnapshotMetadata metadata) {
        Result result = new Result(status, metadata);
        resultTaskSource.setResult(result);
        HelperFragment.finishRequest(this);
    }

    void setResult(int status) {
        setResult(status, /* metadata= */null);
    }

    void setFailure(Exception e) {
        resultTaskSource.setException(e);
        HelperFragment.finishRequest(this);
    }
}
