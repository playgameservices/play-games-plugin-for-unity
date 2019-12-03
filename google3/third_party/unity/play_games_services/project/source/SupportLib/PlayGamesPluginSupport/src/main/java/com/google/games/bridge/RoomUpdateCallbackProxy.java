package com.google.games.bridge;

import com.google.android.gms.games.multiplayer.realtime.Room;
import com.google.android.gms.games.multiplayer.realtime.RoomUpdateCallback;
import com.google.android.gms.games.GamesCallbackStatusCodes.OnJoinedRoomStatusCodes;
import com.google.android.gms.games.GamesCallbackStatusCodes.OnLeftRoomStatusCodes;
import com.google.android.gms.games.GamesCallbackStatusCodes.OnRoomConnectedStatusCodes;
import com.google.android.gms.games.GamesCallbackStatusCodes.OnRoomCreatedStatusCodes;

public class RoomUpdateCallbackProxy extends RoomUpdateCallback {
    private Callback callback;

    public RoomUpdateCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onRoomCreated(@OnRoomCreatedStatusCodes int statusCode, /* @Nullable */ Room room) {
        callback.onRoomCreated(statusCode, room);
    }

    public void onJoinedRoom(@OnJoinedRoomStatusCodes int statusCode, /* @Nullable */ Room room) {
        callback.onJoinedRoom(statusCode, room);
    }

    public void onLeftRoom(@OnLeftRoomStatusCodes int statusCode, /* @NonNull */ String roomId) {
        callback.onLeftRoom(statusCode, roomId);
    }

    public void onRoomConnected(@OnRoomConnectedStatusCodes int statusCode, /* @Nullable */ Room room) {
        callback.onRoomConnected(statusCode, room);
    }

    public interface Callback {
        void onRoomCreated(@OnRoomCreatedStatusCodes int statusCode, /* @Nullable */ Room room);
        void onJoinedRoom(@OnJoinedRoomStatusCodes int statusCode, /* @Nullable */ Room room);
        void onLeftRoom(@OnLeftRoomStatusCodes int statusCode, /* @NonNull */ String roomId);
        void onRoomConnected(@OnRoomConnectedStatusCodes int statusCode, /* @Nullable */ Room room);
    }
}
