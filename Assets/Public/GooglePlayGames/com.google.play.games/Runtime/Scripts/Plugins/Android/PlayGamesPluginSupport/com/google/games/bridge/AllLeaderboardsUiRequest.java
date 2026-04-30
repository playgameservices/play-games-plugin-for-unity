package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.games.LeaderboardsClient;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.tasks.Task;

class AllLeaderboardsUiRequest extends SimpleUiRequest {
    private static final String TAG = "AllLeaderboardsUiRequest";

    @Override
    protected Task<Intent> getIntent(Activity activity) {
        LeaderboardsClient client = PlayGames.getLeaderboardsClient(activity);
        return client.getAllLeaderboardsIntent();
    }
}
