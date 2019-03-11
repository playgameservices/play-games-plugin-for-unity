/*
 * Copyright (C) Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.google.games.bridge;

import android.support.annotation.NonNull;
import com.google.android.gms.games.multiplayer.Invitation;
import com.google.android.gms.games.multiplayer.InvitationCallback;

// can be removed if com.google.android.gms.games.multiplayer.InvitationCallback becomes interface
public class InvitationCallbackProxy extends InvitationCallback {
    private Callback callback;

    public InvitationCallbackProxy(Callback callback) {
        this.callback = callback;
    }

    public void onInvitationReceived(@NonNull Invitation invitation) {
        callback.onInvitationReceived(invitation);
    }

    public void onInvitationRemoved(@NonNull String invitationId) {
        callback.onInvitationRemoved(invitationId);
    }

    public interface Callback {
        public abstract void onInvitationReceived(@NonNull Invitation invitation);
        public abstract void onInvitationRemoved(@NonNull String invitationId);        
    }
}
