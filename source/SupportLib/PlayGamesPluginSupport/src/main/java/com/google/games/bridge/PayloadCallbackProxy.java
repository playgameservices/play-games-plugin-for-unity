package com.google.games.bridge;

import com.google.android.gms.nearby.connection.Payload;
import com.google.android.gms.nearby.connection.PayloadCallback;
import com.google.android.gms.nearby.connection.PayloadTransferUpdate;

public class PayloadCallbackProxy extends PayloadCallback {
    private Callback callback;

    public PayloadCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onPayloadReceived(/* @NonNull */ String endpointId, /* @NonNull */ Payload payload) {
        callback.onPayloadReceived(endpointId, payload);
    }

    public void onPayloadTransferUpdate(/* @NonNull */ String endpointId, /* @NonNull */ PayloadTransferUpdate update) {
    }

    public interface Callback {
        void onPayloadReceived(/* @NonNull */ String endpointId, /* @NonNull */ Payload payload);
    }
}
