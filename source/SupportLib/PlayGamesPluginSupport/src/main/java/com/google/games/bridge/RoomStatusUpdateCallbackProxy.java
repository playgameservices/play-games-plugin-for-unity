package com.google.games.bridge;

import com.google.android.gms.games.multiplayer.realtime.Room;
import com.google.android.gms.games.multiplayer.realtime.RoomStatusUpdateCallback;
import java.util.List;

public class RoomStatusUpdateCallbackProxy extends RoomStatusUpdateCallback {
    private Callback callback;

    public RoomStatusUpdateCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onRoomConnecting(/* @Nullable */ Room room) {
        callback.onRoomConnecting(room);
    }

    public void onRoomAutoMatching(/* @Nullable */ Room room) {
        callback.onRoomAutoMatching(room);
    }

    public void onPeerInvitedToRoom(
        /* @Nullable */ Room room, /* @NonNull */ List<String> participantIds) {
        callback.onPeerInvitedToRoom(room, participantIds);
    }

    public void onPeerDeclined(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds) {
        callback.onPeerDeclined(room, participantIds);
    }

    public void onPeerJoined(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds) {
        callback.onPeerJoined(room, participantIds);
    }

    public void onPeerLeft(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds) {
        callback.onPeerLeft(room, participantIds);
    }

    public void onConnectedToRoom(/* @Nullable */ Room room) {
        callback.onConnectedToRoom(room);
    }

    public void onDisconnectedFromRoom(/* @Nullable */ Room room) {
        callback.onDisconnectedFromRoom(room);
    }

    public void onPeersConnected(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds) {
        callback.onPeersConnected(room, participantIds);
    }

    public void onPeersDisconnected(
        /* @Nullable */ Room room, /* @NonNull */ List<String> participantIds) {
            callback.onPeersDisconnected(room, participantIds);
        }

    public void onP2PConnected(/* @NonNull */ String participantId) {
        callback.onP2PConnected(participantId);
    }

    public void onP2PDisconnected(/* @NonNull */ String participantId) {
        callback.onP2PDisconnected(participantId);
    }

    public interface Callback {
        void onRoomConnecting(/* @Nullable */ Room room);
        void onRoomAutoMatching(/* @Nullable */ Room room);
        void onPeerInvitedToRoom(
            /* @Nullable */ Room room, /* @NonNull */ List<String> participantIds);
        void onPeerDeclined(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds);
        void onPeerJoined(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds);
        void onPeerLeft(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds);
        void onConnectedToRoom(/* @Nullable */ Room room);
        void onDisconnectedFromRoom(/* @Nullable */ Room room);
        void onPeersConnected(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds);
        void onPeersDisconnected(/* @Nullable */ Room room, /* @NonNull */ List<String> participantIds);
        void onP2PConnected(/* @NonNull */ String participantId);
        void onP2PDisconnected(/* @NonNull */ String participantId);
    }
}
