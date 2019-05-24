package com.google.games.bridge;

import com.google.android.gms.games.RealTimeMultiplayerClient;
import com.google.android.gms.games.GamesCallbackStatusCodes.OnRealTimeMessageSentStatusCodes;

public class ReliableMessageSentCallbackProxy implements RealTimeMultiplayerClient.ReliableMessageSentCallback {

    Callback callback;

    public ReliableMessageSentCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onRealTimeMessageSent(
        @OnRealTimeMessageSentStatusCodes int statusCode,
        int tokenId,
        String recipientParticipantId) {
        callback.onRealTimeMessageSent(statusCode, tokenId, recipientParticipantId);
    }

    public interface Callback {
        void onRealTimeMessageSent(int statusCode, int tokenId, String recipientParticipantId);
    }
}
