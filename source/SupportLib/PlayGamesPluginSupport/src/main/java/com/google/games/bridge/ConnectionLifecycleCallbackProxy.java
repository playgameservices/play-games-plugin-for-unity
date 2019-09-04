package com.google.games.bridge;

import com.google.android.gms.nearby.connection.ConnectionInfo;
import com.google.android.gms.nearby.connection.ConnectionLifecycleCallback;
import com.google.android.gms.nearby.connection.ConnectionResolution;

public class ConnectionLifecycleCallbackProxy extends ConnectionLifecycleCallback {
    private Callback callback;

    public ConnectionLifecycleCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onConnectionResult(/* @NonNull */ String endpointId, /* @NonNull */ ConnectionResolution resolution) {
        callback.onConnectionResult(endpointId, resolution);
    }

    public void onDisconnected(/* @NonNull */ String endpointId) {
        callback.onDisconnected(endpointId);
    }

    public void onConnectionInitiated(/* @NonNull */ String endpointId, /* @NonNull */ ConnectionInfo connectionInfo) {
        callback.onConnectionInitiated(endpointId, connectionInfo);
    }

    public interface Callback {
        void onConnectionResult(/* @NonNull */ String endpointId, /* @NonNull */ ConnectionResolution resolution);
        void onDisconnected(/* @NonNull */ String endpointId);
        void onConnectionInitiated(/* @NonNull */ String endpointId, /* @NonNull */ ConnectionInfo connectionInfo);
    }
}
