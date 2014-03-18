package com.google.example.games.pluginsupport;

import android.os.Bundle;

import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.multiplayer.realtime.RealTimeMessageReceivedListener;
import com.google.android.gms.games.multiplayer.realtime.RoomConfig;
import com.google.android.gms.games.multiplayer.realtime.RoomStatusUpdateListener;
import com.google.android.gms.games.multiplayer.realtime.RoomUpdateListener;
import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatchConfig;
import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMultiplayer;

import java.util.ArrayList;

public class TbmpUtils {
    public static PendingResult<TurnBasedMultiplayer.InitiateMatchResult> createQuickMatch(
            GoogleApiClient apiClient, int minOpponents, int maxOpponents, int variant) {
        Bundle autoMatchCriteria = TurnBasedMatchConfig.createAutoMatchCriteria(minOpponents,
                maxOpponents, 0);
        return create(apiClient, null, variant, autoMatchCriteria);
    }

    public static PendingResult<TurnBasedMultiplayer.InitiateMatchResult> create(
            GoogleApiClient apiClient, ArrayList<String> playersToInvite, int variant,
            Bundle autoMatchCriteria) {

        TurnBasedMatchConfig.Builder builder = TurnBasedMatchConfig.builder();

        if (variant > 0) {
            builder.setVariant(variant);
        }
        if (playersToInvite != null) {
            builder.addInvitedPlayers(playersToInvite);
        }
        builder.setAutoMatchCriteria(autoMatchCriteria);
        return Games.TurnBasedMultiplayer.createMatch(apiClient, builder.build());
    }
}
