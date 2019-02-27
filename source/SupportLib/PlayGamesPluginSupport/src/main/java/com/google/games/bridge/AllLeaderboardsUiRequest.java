package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.LeaderboardsClient;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;


class AllLeaderboardsUiRequest extends SimpleUiRequest {
    private static final String TAG = "AllLeaderboardsUiRequest";

    @Override
    protected Task<Intent> getIntent(Activity activity) {
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        LeaderboardsClient client = Games.getLeaderboardsClient(activity, account);
        return client.getAllLeaderboardsIntent();
    }
}
