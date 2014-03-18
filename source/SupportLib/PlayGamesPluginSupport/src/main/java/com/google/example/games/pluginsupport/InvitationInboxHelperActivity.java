package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.google.android.gms.games.Games;
import com.google.android.gms.games.multiplayer.Invitation;
import com.google.android.gms.games.multiplayer.Multiplayer;
import com.google.android.gms.games.multiplayer.realtime.RoomConfig;
import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatch;

import java.util.ArrayList;

public class InvitationInboxHelperActivity extends UiHelperActivity {
    private static final String EXTRA_IS_RTMP = "EXTRA_IS_RTMP";

    public interface Listener {
        public void onInvitationInboxResult(boolean success, String invitationId);
        public void onTurnBasedMatch(TurnBasedMatch match);
    };

    static Listener sListener = null;
    public static void setListener(Listener listener) {
        sListener = listener;
    }

    @Override
    protected void deliverFailure() {
        if (sListener != null) {
            debugLog("Delivering failure to listener.");
            sListener.onInvitationInboxResult(false, "");
            sListener = null;
        }
    }

    @Override
    protected void deliverSuccess(Intent data) {
        debugLog("Parsing invitation/match from UI's returned data.");
        Invitation inv = data.getExtras().getParcelable(Multiplayer.EXTRA_INVITATION);
        TurnBasedMatch match = data.getExtras().getParcelable(Multiplayer.EXTRA_TURN_BASED_MATCH);
        debugLog("Invitation: " + ((inv == null) ? "null" : inv.toString()));
        debugLog("Match: " + ((match == null) ? "null": "[TurnBasedMatch]"));

        if (inv != null) {
            debugLog("Calling listener to deliver invitation.");
            sListener.onInvitationInboxResult(true, inv.getInvitationId());
        } else if (match != null) {
            debugLog("Calling listener to deliver match.");
            sListener.onTurnBasedMatch(match);
        } else {
            Log.w(TAG, "Invitation inbox result came with no invitation and no match!");
            debugLog("Calling listener to deliver failure.");
            sListener.onInvitationInboxResult(false, null);
        }

        sListener = null;
    }

    @Override
    protected Intent getUiIntent() {
        boolean isRtmp = getIntent().getBooleanExtra(EXTRA_IS_RTMP, true);
        return isRtmp ? Games.Invitations.getInvitationInboxIntent(mHelper.getApiClient()) :
                Games.TurnBasedMultiplayer.getInboxIntent(mHelper.getApiClient());
    }

    public static void launch(boolean isRtmp, Activity activity, Listener listener,
                boolean debugEnabled) {
        setListener(listener);
        Intent i = new Intent(activity, InvitationInboxHelperActivity.class);
        i.putExtra(EXTRA_IS_RTMP, isRtmp);
        i.putExtra(EXTRA_DEBUG_ENABLED, debugEnabled);
        activity.startActivity(i);
    }
}
