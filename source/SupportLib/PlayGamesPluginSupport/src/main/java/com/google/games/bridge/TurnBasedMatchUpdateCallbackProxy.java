package com.google.games.bridge;

import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatch;
import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatchUpdateCallback;

public class TurnBasedMatchUpdateCallbackProxy extends TurnBasedMatchUpdateCallback {
    private Callback callback;

    public TurnBasedMatchUpdateCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onTurnBasedMatchReceived(/* @NonNull */ TurnBasedMatch match) {
        callback.onTurnBasedMatchReceived(match);
    }

    public void onTurnBasedMatchRemoved(/* @NonNull */ String matchId) {
        callback.onTurnBasedMatchRemoved(matchId);
    }

    public interface Callback {
        void onTurnBasedMatchReceived(/* @NonNull */ TurnBasedMatch match);
        void onTurnBasedMatchRemoved(/* @NonNull */ String matchId);
    }
}
