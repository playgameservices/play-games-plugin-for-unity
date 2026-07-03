package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.games.AchievementsClient;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.tasks.Task;

class AchievementUiRequest extends SimpleUiRequest {
    private static final String TAG = "AchievementUiRequest";

    @Override
    protected Task<Intent> getIntent(Activity activity) {
        AchievementsClient achievementClient = PlayGames.getAchievementsClient(activity);
        return achievementClient.getAchievementsIntent();
    }
}
