package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.games.LeaderboardsClient;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.tasks.Task;

class LeaderboardUiRequest extends SimpleUiRequest {
    private static final String TAG = "AllLeaderboardsUiRequest";
    private final String leaderboardId;
    private final int timeSpan;

    LeaderboardUiRequest(String leaderboardId, int timeSpan) {
        this.leaderboardId = leaderboardId;
        this.timeSpan = timeSpan;
    }

    @Override
    protected Task<Intent> getIntent(Activity activity) {
        LeaderboardsClient client = PlayGames.getLeaderboardsClient(activity);
        return client.getLeaderboardIntent(leaderboardId, timeSpan);
    }
}
