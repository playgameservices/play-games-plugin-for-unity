package com.google.games.bridge;

import android.app.Activity;
import android.content.Intent;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.AchievementsClient;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;


class AchievementUiRequest extends SimpleUiRequest {
    private static final String TAG = "AchievementUiRequest";

    @Override
    protected Task<Intent> getIntent(Activity activity) {
        GoogleSignInAccount account = HelperFragment.getAccount(activity);
        AchievementsClient achievementClient = Games.getAchievementsClient(activity, account);
        return achievementClient.getAchievementsIntent();
    }
}
