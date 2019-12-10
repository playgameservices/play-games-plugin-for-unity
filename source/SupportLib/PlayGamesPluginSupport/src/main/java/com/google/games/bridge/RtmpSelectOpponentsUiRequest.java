package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.games.Games;
import com.google.android.gms.tasks.Task;

class RtmpSelectOpponentsUiRequest extends BaseSelectOpponentsUiRequest {

    public RtmpSelectOpponentsUiRequest(int minPlayers, int maxPlayers) {
        super(minPlayers, maxPlayers);
    }

    @Override
    Task<Intent> getIntentTask(Activity activity, GoogleSignInAccount account){
        return Games.getRealTimeMultiplayerClient(activity, account)
                    .getSelectOpponentsIntent(getMinPlayers(), getMaxPlayers());
    }
}
