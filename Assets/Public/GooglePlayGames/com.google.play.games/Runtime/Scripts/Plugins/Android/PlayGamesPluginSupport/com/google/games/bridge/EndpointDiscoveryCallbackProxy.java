package com.google.games.bridge;

import com.google.android.gms.nearby.connection.DiscoveredEndpointInfo;
import com.google.android.gms.nearby.connection.EndpointDiscoveryCallback;

public class EndpointDiscoveryCallbackProxy extends EndpointDiscoveryCallback {
    private Callback callback;

    public EndpointDiscoveryCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onEndpointFound(/* @NonNull */ String endpointId, /* @NonNull */ DiscoveredEndpointInfo info) {
        callback.onEndpointFound(endpointId, info);
    }

    public void onEndpointLost(/* @NonNull */ String endpointId) {
        callback.onEndpointLost(endpointId);
    }

    public interface Callback {
        void onEndpointFound(/* @NonNull */ String endpointId, /* @NonNull */ DiscoveredEndpointInfo info);
        void onEndpointLost(/* @NonNull */ String endpointId);
    }
}
