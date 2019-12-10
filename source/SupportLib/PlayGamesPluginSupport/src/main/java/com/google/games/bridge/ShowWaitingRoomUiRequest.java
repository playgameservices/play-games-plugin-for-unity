package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.RealTimeMultiplayerClient;
import com.google.android.gms.games.multiplayer.Multiplayer;
import com.google.android.gms.games.multiplayer.realtime.Room;
import com.google.android.gms.games.GamesActivityResultCodes;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;

class ShowWaitingRoomUiRequest implements HelperFragment.Request {
    private static final String TAG = "ShowWaitingRoomRequest";

    /**
     * Should be aligned to:
     * PluginDev/Assets/GooglePlayGames/Platforms/Android/AndroidHelperFragment.cs enum WaitingRoomUIStatus
     * */
    static final int UI_STATUS_VALID = 1;
    static final int UI_STATUS_CANCELLED = 2;
    static final int UI_STATUS_LEFT_ROOM = 3;
    static final int UI_STATUS_INVALID_ROOM = 4;
    static final int UI_STATUS_BUSY = -1;

    private final TaskCompletionSource<Result> resultTaskSource = new TaskCompletionSource<>();
    private Room room;
    private int minParticipantsToStart;

    ShowWaitingRoomUiRequest(Room room, int minParticipantsToStart) {
        this.room = room;
        this.minParticipantsToStart = minParticipantsToStart;
    }

    public class Result {
        public int status;
        public Room room;

        Result(int status, Room room) {
            this.status = status;
            this.room = room;
        }
    }

    Task<Result> getTask() {
        return resultTaskSource.getTask();
    }

    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        RealTimeMultiplayerClient client = Games.getRealTimeMultiplayerClient(activity, account);
        client
            .getWaitingRoomIntent(room, minParticipantsToStart)
            .addOnSuccessListener(
                activity,
                    new OnSuccessListener<Intent>() {
                        @Override
                        public void onSuccess(Intent intent) {
                            Utils.startActivityForResult(helperFragment, intent, HelperFragment.RC_SHOW_WAITING_ROOM_UI);
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
        if (requestCode == HelperFragment.RC_SHOW_WAITING_ROOM_UI) {
            Room room = (Room) data.getParcelableExtra(Multiplayer.EXTRA_ROOM);
            if (resultCode == Activity.RESULT_OK) {
                setResult(UI_STATUS_VALID, room);
            } else if (resultCode == Activity.RESULT_CANCELED) {
                setResult(UI_STATUS_CANCELLED, room);
            } else if (resultCode == GamesActivityResultCodes.RESULT_LEFT_ROOM) {
                setResult(UI_STATUS_LEFT_ROOM, room);
            } else if (resultCode == GamesActivityResultCodes.RESULT_INVALID_ROOM) {
                setResult(UI_STATUS_INVALID_ROOM, room);
            } else {
                Log.d(TAG, "onActivityResult unknown resultCode: " + resultCode);
                setResult(CommonUIStatus.INTERNAL_ERROR, room);
            }
        }
    }

    void setResult(int status, Room room) {
        Result result = new Result(status, room);
        resultTaskSource.setResult(result);
        HelperFragment.finishRequest(this);
    }

    void setFailure(Exception e) {
        resultTaskSource.setException(e);
        HelperFragment.finishRequest(this);
    }
}
