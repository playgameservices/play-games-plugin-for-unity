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
import android.app.PendingIntent;
import android.content.Intent;
import android.util.Log;
import android.view.View;
import com.google.android.gms.common.api.ResolvableApiException;
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
    static final int RC_SELECT_OPPONENTS_UI = 9006;
    static final int RC_SHOW_REQUEST_PERMISSIONS_UI = 9010;
    static final int RC_RESOLUTION_DIALOG = 9011;

    // Pending token request.  There can be only one outstanding request at a
    // time.
    private static final Object lock = new Object();
    private static Request pendingRequest, runningRequest;
    private static HelperFragment helperFragment;

    public static Task<Integer> showAchievementUi(Activity parentActivity) {
        AchievementUiRequest request = new AchievementUiRequest();

        if(!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
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

    public static boolean isResolutionRequired(Exception exception) {
        if (exception instanceof ResolvableApiException) {
            return true;
        }
        return false;
    }

    public static Task<Integer> askForLoadFriendsResolution(
            Activity parentActivity, PendingIntent pendingIntent) {
        GenericResolutionUiRequest request = new GenericResolutionUiRequest(pendingIntent);

        if (!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }
        return request.getTask();
    }

    public static Task<Integer> showCompareProfileWithAlternativeNameHintsUI(
            Activity parentActivity,
            String playerId,
            String otherPlayerInGameName,
            String currentPlayerInGameName) {
        CompareProfileUiRequest request = new CompareProfileUiRequest(
                playerId,
                otherPlayerInGameName,
                currentPlayerInGameName);

        if (!HelperFragment.startRequest(parentActivity, request)) {
            request.setResult(CommonUIStatus.UI_BUSY);
        }

        return request.getTask();
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
            if (runningRequest == request) {
                runningRequest = null;
            }
        }
    }

    interface Request {
        void process(HelperFragment helperFragment);
        void onActivityResult(int requestCode, int resultCode, Intent data);
    };

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
