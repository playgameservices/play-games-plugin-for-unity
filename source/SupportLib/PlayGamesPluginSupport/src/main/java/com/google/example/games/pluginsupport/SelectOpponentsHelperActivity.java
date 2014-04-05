package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.google.android.gms.games.Games;
import com.google.android.gms.games.multiplayer.Multiplayer;
import com.google.android.gms.games.multiplayer.realtime.RoomConfig;
import com.google.example.games.basegameutils.GameHelper;

import java.util.ArrayList;

public class SelectOpponentsHelperActivity extends UiHelperActivity {
    public static final String EXTRA_MIN_OPPONENTS = "EXTRA_MIN_OPPONENTS";
    public static final String EXTRA_MAX_OPPONENTS = "EXTRA_MAX_OPPONENTS";
    public static final String EXTRA_IS_RTMP = "EXTRA_IS_RTMP";

    // Unity's AndroidJavaProxy crashes when you pass null to an Object argument of a listener,
    // so instead of passing nulls, we have to pass this "null object"
    Object mDummyObject = new Object();

    public interface Listener {
        public void onSelectOpponentsResult(boolean success, Object opponents,
                boolean hasAutomatch, Object automatchCriteria);
    };

    static Listener sListener = null;
    public static void setListener(Listener listener) {
        sListener = listener;
    }

    @Override
    protected void deliverFailure() {
        if (sListener != null) {
            debugLog("Delivering failure to listener.");
            sListener.onSelectOpponentsResult(false, mDummyObject, false, mDummyObject);
            sListener = null;
        }
    }

    @Override
    protected void deliverSuccess(Intent data) {
        // get the invitee list
        final ArrayList<String> invitees = data.getStringArrayListExtra(Games.EXTRA_PLAYER_IDS);
        debugLog("Invitee count: " + invitees.size());

        // get the automatch criteria
        Bundle autoMatchCriteria = null;
        int minAutoMatchPlayers = data.getIntExtra(Multiplayer.EXTRA_MIN_AUTOMATCH_PLAYERS, 0);
        int maxAutoMatchPlayers = data.getIntExtra(Multiplayer.EXTRA_MAX_AUTOMATCH_PLAYERS, 0);
        if (minAutoMatchPlayers > 0 || maxAutoMatchPlayers > 0) {
            autoMatchCriteria = RoomConfig.createAutoMatchCriteria(
                    minAutoMatchPlayers, maxAutoMatchPlayers, 0);
            debugLog("Automatch criteria: " + autoMatchCriteria);
        } else {
            debugLog("No automatch criteria.");
        }

        if (sListener != null) {
            debugLog("Calling listener.");
            sListener.onSelectOpponentsResult(true, invitees, autoMatchCriteria != null,
                    autoMatchCriteria == null ? mDummyObject : autoMatchCriteria);
            sListener = null;
        }
    }

    @Override
    protected Intent getUiIntent() {
        int minOpponents = getIntent().getIntExtra(EXTRA_MIN_OPPONENTS, 1);
        int maxOpponents = getIntent().getIntExtra(EXTRA_MAX_OPPONENTS, minOpponents);
        boolean isRtmp = getIntent().getBooleanExtra(EXTRA_IS_RTMP, true);
        if (isRtmp) {
            return Games.RealTimeMultiplayer.getSelectOpponentsIntent(mHelper.getApiClient(),
                    minOpponents, maxOpponents);
        } else {
            return Games.TurnBasedMultiplayer.getSelectOpponentsIntent(mHelper.getApiClient(),
                    minOpponents, maxOpponents, true);
        }
    }

    public static void launch(boolean isRtmp, Activity activity, Listener listener,
                boolean debugEnabled, int minOpponents, int maxOpponents) {
        setListener(listener);
        Intent i = new Intent(activity, SelectOpponentsHelperActivity.class);
        i.putExtra(EXTRA_DEBUG_ENABLED, debugEnabled);
        i.putExtra(EXTRA_IS_RTMP, isRtmp);
        i.putExtra(EXTRA_MIN_OPPONENTS, minOpponents);
        i.putExtra(EXTRA_MAX_OPPONENTS, maxOpponents);
        activity.startActivity(i);
    }
}
