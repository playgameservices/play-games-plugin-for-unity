package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.games.Games;
import com.google.android.gms.tasks.Task;

class TbmpSelectOpponentsUiRequest extends BaseSelectOpponentsUiRequest {

    public TbmpSelectOpponentsUiRequest(int minPlayers, int maxPlayers) {
        super(minPlayers, maxPlayers);
    }

    @Override
    Task<Intent> getIntentTask(Activity activity, GoogleSignInAccount account){
        return Games.getTurnBasedMultiplayerClient(activity, account)
                    .getSelectOpponentsIntent(getMinPlayers(), getMaxPlayers());
    }
}
