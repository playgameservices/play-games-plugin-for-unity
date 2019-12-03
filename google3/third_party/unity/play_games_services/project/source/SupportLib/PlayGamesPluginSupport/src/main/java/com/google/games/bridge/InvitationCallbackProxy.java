package com.google.games.bridge;

import com.google.android.gms.games.multiplayer.Invitation;
import com.google.android.gms.games.multiplayer.InvitationCallback;

public class InvitationCallbackProxy extends InvitationCallback {
    private Callback callback;

    public InvitationCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onInvitationReceived(/* @NonNull */ Invitation invitation) {
        callback.onInvitationReceived(invitation);
    }

    public void onInvitationRemoved(/* @NonNull */ String invitationId) {
        callback.onInvitationRemoved(invitationId);
    }

    public interface Callback {
        void onInvitationReceived(/* @NonNull */ Invitation invitation);
        void onInvitationRemoved(/* @NonNull */ String invitationId);
    }
}
