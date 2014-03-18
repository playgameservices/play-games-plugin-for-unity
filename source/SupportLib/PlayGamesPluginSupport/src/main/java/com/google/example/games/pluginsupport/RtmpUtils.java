package com.google.example.games.pluginsupport;

import android.os.Bundle;

import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.multiplayer.realtime.RealTimeMessageReceivedListener;
import com.google.android.gms.games.multiplayer.realtime.RoomConfig;
import com.google.android.gms.games.multiplayer.realtime.RoomStatusUpdateListener;
import com.google.android.gms.games.multiplayer.realtime.RoomUpdateListener;

import java.util.ArrayList;

public class RtmpUtils {
    public static void createQuickGame(GoogleApiClient apiClient,
                int minOpponents, int maxOpponents, int variant,
                RoomUpdateListener roomUpdateListener,
                RoomStatusUpdateListener roomStatusUpdateListener,
                RealTimeMessageReceivedListener messageReceivedListener) {
        Bundle autoMatchCriteria = RoomConfig.createAutoMatchCriteria(minOpponents, maxOpponents, 0);
        create(apiClient, null, variant, autoMatchCriteria, roomUpdateListener,
                roomStatusUpdateListener, messageReceivedListener);
    }

    public static void create(GoogleApiClient apiClient,
            ArrayList<String> playersToInvite, int variant,
            Bundle autoMatchCriteria,
            RoomUpdateListener roomUpdateListener,
            RoomStatusUpdateListener roomStatusUpdateListener,
            RealTimeMessageReceivedListener messageReceivedListener) {

        RoomConfig.Builder builder = makeRoomConfigBuilder(roomUpdateListener,
                roomStatusUpdateListener, messageReceivedListener);
        if (variant > 0) {
            builder.setVariant(variant);
        }
        if (playersToInvite != null) {
            builder.addPlayersToInvite(playersToInvite);
        }
        builder.setAutoMatchCriteria(autoMatchCriteria);
        Games.RealTimeMultiplayer.create(apiClient, builder.build());
    }

    private static RoomConfig.Builder makeRoomConfigBuilder(RoomUpdateListener roomUpdateListener,
            RoomStatusUpdateListener roomStatusUpdateListener,
            RealTimeMessageReceivedListener messageReceivedListener) {

        RoomConfig.Builder builder = RoomConfig.builder(roomUpdateListener);
        builder.setMessageReceivedListener(messageReceivedListener);
        builder.setRoomStatusUpdateListener(roomStatusUpdateListener);
        return builder;
    }

    public static void accept(GoogleApiClient apiClient, String invitationId,
            RoomUpdateListener roomUpdateListener,
            RoomStatusUpdateListener roomStatusUpdateListener,
            RealTimeMessageReceivedListener messageReceivedListener) {

        RoomConfig.Builder builder = makeRoomConfigBuilder(roomUpdateListener,
                roomStatusUpdateListener, messageReceivedListener);
        builder.setInvitationIdToAccept(invitationId);
        Games.RealTimeMultiplayer.join(apiClient, builder.build());
    }
}
