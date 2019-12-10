package com.google.games.bridge;

import com.google.android.gms.games.multiplayer.realtime.RealTimeMessage;
import com.google.android.gms.games.multiplayer.realtime.OnRealTimeMessageReceivedListener;

public class RealTimeMessageReceivedListenerProxy implements OnRealTimeMessageReceivedListener {
    private Callback callback;

    public RealTimeMessageReceivedListenerProxy(Callback callback) {
        this.callback = callback;
    }

    public void onRealTimeMessageReceived(/* @NonNull */ RealTimeMessage message) {
        callback.onRealTimeMessageReceived(message.isReliable(),
            message.getSenderParticipantId(), message.getMessageData());
    }

    public interface Callback {
        void onRealTimeMessageReceived(boolean isReliable, String senderId, byte[] data);
    }
}
