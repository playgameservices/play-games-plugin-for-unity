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

import android.app.Activity;
import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.Intent;
import android.util.Log;
import android.view.View;
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.auth.api.signin.GoogleSignInClient;
import com.google.android.gms.auth.api.signin.GoogleSignInOptions;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.Scope;
import com.google.android.gms.games.multiplayer.realtime.Room;
import com.google.android.gms.tasks.Task;

/**
 * Activity fragment with no UI added to the parent activity in order to manage
 * requests with required onActivityResult handling.
 */
public class HelperFragment extends Fragment
{
    private static final String TAG = "HelperFragment";

    private static final String FRAGMENT_TAG = "gpg.HelperFragment";
    static final int RC_SIGN_IN = 9002;
    static final int RC_SIMPLE_UI = 9003;
    static final int RC_SELECT_SNAPSHOT_UI = 9004;
    static final int RC_CAPTURE_OVERLAY_UI = 9005;
    static final int RC_SELECT_OPPONENTS_UI = 9006;
    static final int RC_INBOX_UI = 9007;
    static final int RC_SHOW_WAITING_ROOM_UI = 9008;
    static final int RC_SHOW_INVITATION_INBOX_UI = 9009;
  static final int RC_SHOW_REQUEST_PERMISSIONS_UI = 9010;

    // Pending token request.  There can be only one outstanding request at a
    // time.
    private static final Object lock = new Object();
    private static Request pendingRequest, runningRequest;
    private static HelperFragment helperFragment;

    private static boolean mStartUpSignInCheckPerformed = false;
    /**
     * External entry point for getting tokens and email address.  This
     * creates the fragment if needed and queues up the request.  The fragment, once
     * active processes the list of requests.
     *
     * @param parentActivity - the parent activity to attach the fragment to.
     * @param requestAuthCode - request a server auth code to exchange on the
     *                        server for an OAuth token.
     * @param requestEmail - request the email address of the user.  This
     *                     requires the consent of the user.
     * @param requestIdToken - request an OAuth ID token for the user.  This
     *                       requires the consent of the user.
     * @param webClientId - the client id of the associated web application.
     *                    This is required when requesting auth code or id
     *                    token.  The client id must be associated with the same
     *                    client as this application.
     * @param forceRefreshToken - force refreshing the token when requesting
     *                          a server auth code.  This causes the consent
     *                          screen to be presented to the user, so it
     *                          should be used only when necessary (if at all).
     * @param additionalScopes - Additional scopes to request.  These will most
     *                         likely require the consent of the user.
     * @param hidePopups - Hides the popups during authentication and game
     *                   services events.  This done by calling
     *                   GamesOptions.setShowConnectingPopup. This is usful for
     *                   VR apps.
     * @param accountName - if non-null, the account name to use when
     *                    authenticating.

     *
     * @return PendingResult for retrieving the results when ready.
     */
    public static PendingResult fetchToken(Activity parentActivity,
                                           boolean silent,
                                           boolean requestAuthCode,
                                           boolean requestEmail,
                                           boolean requestIdToken,
                                           String webClientId,
                                           boolean forceRefreshToken,
                                           String[] additionalScopes,
                                           boolean hidePopups,
                                           String accountName) {
        SignInRequest request = new SignInRequest(silent, requestAuthCode, requestEmail,
                requestIdToken, webClientId, forceRefreshToken, additionalScopes, hidePopups,
                accountName);

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setFailure(CommonStatusCodes.DEVELOPER_ERROR);
        }

        return request.getPendingResponse();
    }

    public static Task<Integer> showAchievementUi(Activity parentActivity) {
        AchievementUiRequest request = new AchievementUiRequest();

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
    }

    public static void showCaptureOverlayUi(Activity parentActivity) {
        CaptureOverlayUiRequest request = new CaptureOverlayUiRequest();

        HelperFragment.startRequest(parentActivity, request);
    }

    public static Task<Integer> showAllLeaderboardsUi(Activity parentActivity) {
        AllLeaderboardsUiRequest request = new AllLeaderboardsUiRequest();

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
    }

    public static Task<Integer> showLeaderboardUi(Activity parentActivity, String leaderboardId, int timeSpan) {
        LeaderboardUiRequest request = new LeaderboardUiRequest(leaderboardId, timeSpan);

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
    }

    public static Task<SelectSnapshotUiRequest.Result> showSelectSnapshotUi(
            Activity parentActivity, /* @NonNull */ String title, boolean allowAddButton, boolean allowDelete, int maxSnapshots) {
        SelectSnapshotUiRequest request = new SelectSnapshotUiRequest(title, allowAddButton, allowDelete, maxSnapshots);

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(SelectSnapshotUiRequest.SELECT_UI_STATUS_UI_BUSY);
        }

        return request.getTask();
    }

    public static Task<BaseSelectOpponentsUiRequest.Result> showRtmpSelectOpponentsUi(Activity parentActivity, int minOpponents, int maxOpponents) {
        RtmpSelectOpponentsUiRequest request = new RtmpSelectOpponentsUiRequest(minOpponents, maxOpponents);

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
    }

    public static Task<BaseSelectOpponentsUiRequest.Result> showTbmpSelectOpponentsUi(Activity parentActivity, int minOpponents, int maxOpponents) {
        TbmpSelectOpponentsUiRequest request = new TbmpSelectOpponentsUiRequest(minOpponents, maxOpponents);

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
    }

    public static Task<ShowWaitingRoomUiRequest.Result> showWaitingRoomUI(Activity parentActivity, Room room, int minParticipantsToStart) {
        ShowWaitingRoomUiRequest request = new ShowWaitingRoomUiRequest(room, minParticipantsToStart);

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(ShowWaitingRoomUiRequest.UI_STATUS_BUSY, null);
        }

        return request.getTask();
    }

    public static Task<ShowInvitationInboxUIRequest.Result> showInvitationInboxUI(Activity parentActivity) {
        ShowInvitationInboxUIRequest request = new ShowInvitationInboxUIRequest();

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
    }

  public static Task<InboxUiRequest.Result> showInboxUi(Activity parentActivity) {
        InboxUiRequest request = new InboxUiRequest();

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
    }

  public static Task<GoogleSignInAccount> showRequestPermissionsUi(
      Activity parentActivity, String[] scopes) {
    RequestPermissionsRequest request = new RequestPermissionsRequest(toScopeList(scopes));

    if (!HelperFragment.startRequest(parentActivity, request)) {
      request.setFailure(CommonUIStatus.UI_BUSY);
    }

    return request.getTask();
  }

  public static boolean hasPermissions(Activity parentActivity, String[] scopes) {
    return GoogleSignIn.hasPermissions(getAccount(parentActivity), toScopeList(scopes));
  }

  private static Scope[] toScopeList(String[] scopeUris) {
    Scope[] scopes = new Scope[scopeUris.length];
    for (int i = 0; i < scopeUris.length; i++) {
      scopes[i] = new Scope(scopeUris[i]);
    }
    return scopes;
  }

    public static void signOut(Activity activity) {
        GoogleSignInClient signInClient = GoogleSignIn.getClient(activity, GoogleSignInOptions.DEFAULT_GAMES_SIGN_IN);
        signInClient.signOut();
        synchronized (lock) {
            pendingRequest = null;
            runningRequest = null;
        }
    }

    private static boolean startRequest(Activity parentActivity, Request request) {
        boolean ok = false;
        synchronized (lock) {
            if (pendingRequest == null && runningRequest == null) {
                pendingRequest = request;
                ok = true;
            }
        }
        if (ok) {
            HelperFragment helperFragment = HelperFragment.getHelperFragment(parentActivity);
            if (helperFragment.isResumed()) {
                helperFragment.processRequest();
            }
        }
        return ok;
    }

    private static HelperFragment getHelperFragment(Activity parentActivity) {
        HelperFragment fragment = (HelperFragment)
                parentActivity.getFragmentManager().findFragmentByTag(FRAGMENT_TAG);

        if (fragment == null) {
            try {
                Log.d(TAG, "Creating fragment");
                fragment = new HelperFragment();
                FragmentTransaction trans = parentActivity.getFragmentManager().beginTransaction();
                trans.add(fragment, FRAGMENT_TAG);
                trans.commit();
            } catch (Throwable th) {
                Log.e(TAG, "Cannot launch token fragment:" + th.getMessage(), th);
                return null;
            }
        }
        return fragment;
    }
    /**
     * Processes the token requests that are queued up.
     */
    private void processRequest() {
        Request request;
        synchronized (lock) {
            if (runningRequest != null) {
                return;
            }
            request = pendingRequest;
            pendingRequest = null;
            runningRequest = request;
        }
        // no request, no need to continue.
        if (request == null) {
            return;
        }

        request.process(this);
    }

    /**
     * Receive the result from a previous call to
     * {@link #startActivityForResult(Intent, int)}.  This follows the
     * related Activity API as described there in
     * {@link Activity#onActivityResult(int, int, Intent)}.
     *
     * @param requestCode The integer request code originally supplied to
     *                    startActivityForResult(), allowing you to identify who this
     *                    result came from.
     * @param resultCode  The integer result code returned by the child activity
     *                    through its setResult().
     * @param data        An Intent, which can return result data to the caller
     */
    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        Request request;
        synchronized (lock) {
            request = runningRequest;
        }
        // no request, no need to continue.
        if (request == null) {
            return;
        }
        request.onActivityResult(requestCode, resultCode, data);
    }

    /**
     * Called when the fragment is visible to the user and actively running.
     * This is generally
     * tied to {@link Activity#onResume() Activity.onResume} of the containing
     * Activity's lifecycle.
     */
    @Override
    public void onResume() {
        Log.d(TAG, "onResume called");
        super.onResume();
        if (helperFragment == null) {
            helperFragment = this;
        }
        processRequest();
    }

    static void finishRequest(Request request) {
        synchronized (lock) {
            if(runningRequest == request) {
                runningRequest = null;
            }
        }
    }

    interface Request {
        void process(HelperFragment helperFragment);
        void onActivityResult(int requestCode, int resultCode, Intent data);
    };

    public static GoogleSignInAccount getAccount(Activity activity) {
        return GoogleSignIn.getLastSignedInAccount(activity);
    }

    public static View createInvisibleView(Activity parentActivity) {
        View view = new View(parentActivity);
        view.setVisibility(View.INVISIBLE);
        view.setClickable(false);
        return view;
    }

    public static View getDecorView(Activity activity) {
        return activity.getWindow().getDecorView();
    }
}
