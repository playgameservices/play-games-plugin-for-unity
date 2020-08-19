package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.PlayersClient;
import com.google.android.gms.tasks.Task;

class CompareProfileUiRequest extends SimpleUiRequest {
    private static final String TAG = "CompareProfileUiRequest";
    private String playerId;
    private String otherPlayerInGameName;
    private String currentPlayerInGameName;

    CompareProfileUiRequest(String playerId,
                            String otherPlayerInGameName,
                            String currentPlayerInGameName) {
        this.playerId = playerId;
        this.otherPlayerInGameName = otherPlayerInGameName;
        this.currentPlayerInGameName = currentPlayerInGameName;
    }

    @Override
    protected Task<Intent> getIntent(Activity activity) {
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        PlayersClient playersClient = Games.getPlayersClient(activity, account);
        return playersClient.getCompareProfileIntentWithAlternativeNameHints(
                playerId, otherPlayerInGameName, currentPlayerInGameName);
    }
}
