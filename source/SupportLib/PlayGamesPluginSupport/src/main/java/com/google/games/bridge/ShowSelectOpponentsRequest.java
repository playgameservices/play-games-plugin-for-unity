package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.multiplayer.Multiplayer;
import com.google.android.gms.games.TurnBasedMultiplayerClient;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.TaskCompletionSource;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.OnSuccessListener;
import java.util.List;

class ShowSelectOpponentsRequest implements HelperFragment.Request {
    private static final String TAG = "ShowSelectOpponents";

    private final int minPlayers;
    private final int maxPlayers;

    private final TaskCompletionSource<Result> resultTaskSource = new TaskCompletionSource<>();

    public class Result {
        public int status;
        public int minAutomatchingPlayers;
        public int maxAutomatchingPlayers;
        public List<String> playerIdsToInvite;

        Result(int status, int minAutomatchingPlayers, int maxAutomatchingPlayers, List<String> playerIdsToInvite) {
            this.status = status;
            this.minAutomatchingPlayers = minAutomatchingPlayers;
            this.maxAutomatchingPlayers = maxAutomatchingPlayers;
            this.playerIdsToInvite = playerIdsToInvite;
        }
    }

    ShowSelectOpponentsRequest(int minPlayers, int maxPlayers) {
        this.minPlayers = minPlayers;
        this.maxPlayers = maxPlayers;
    }

    Task<Result> getTask() {
        return resultTaskSource.getTask();
    }

    @Override
    public void process(final HelperFragment helperFragment) {
        final Activity activity = helperFragment.getActivity();
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        TurnBasedMultiplayerClient client = Games.getTurnBasedMultiplayerClient(activity, account);
        client.getSelectOpponentsIntent(minPlayers, maxPlayers)
            .addOnSuccessListener(
                activity,
                new OnSuccessListener<Intent>() {
                    @Override
                    public void onSuccess(Intent intent) {
                        helperFragment.startActivityForResult(intent, HelperFragment.RC_SELECT_OPPONENTS_UI);
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

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == HelperFragment.RC_SELECT_OPPONENTS_UI) {
            if (resultCode == Activity.RESULT_OK) {
                setResult(CommonUIStatus.VALID,
                    data.getIntExtra(Multiplayer.EXTRA_MIN_AUTOMATCH_PLAYERS, minPlayers),
                    data.getIntExtra(Multiplayer.EXTRA_MAX_AUTOMATCH_PLAYERS, maxPlayers),
                    data.getStringArrayListExtra(Games.EXTRA_PLAYER_IDS));
            } else if (resultCode == Activity.RESULT_CANCELED) {
                setResult(CommonUIStatus.CANCELLED);
            } else {
                Log.d(TAG, "onActivityResult unknown resultCode: " + resultCode);
                setResult(CommonUIStatus.INTERNAL_ERROR);
            }
        }
    }

    void setResult(Integer status, int minAutomatchingPlayers, int maxAutomatchingPlayers, List<String> playerIdsToInvite) {
        Result result = new Result(status, minAutomatchingPlayers, maxAutomatchingPlayers, playerIdsToInvite);
        resultTaskSource.setResult(result);
        HelperFragment.finishRequest(this);
    }

    void setResult(Integer status) {
        setResult(status,
            /* minAutomatchingPlayers= */ 0,
            /* maxAutomatchingPlayers= */ 0,
            /* playerIdsToInvite= */ null);
    }

    void setFailure(Exception e) {
        resultTaskSource.setException(e);
        HelperFragment.finishRequest(this);
    }
}
