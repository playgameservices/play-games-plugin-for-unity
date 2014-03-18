package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Intent;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.games.multiplayer.Invitation;
import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatch;
import com.google.example.games.basegameutils.GameHelper;

public class SignInHelperManager {
    private static SignInHelperManager sInstance = new SignInHelperManager();

    private static final int RC_UNUSED = 99999;
    private static final String TAG = "SignInHelperManager";

    GameHelper.GameHelperListener mListener = null;
    int mSignInErrorActivityResponse = 0;
    int mSignInErrorCode = 0;
    Invitation mInvitation = null;
    TurnBasedMatch mMatch = null;

    private SignInHelperManager() {}

    public void setGameHelperListener(GameHelper.GameHelperListener listener) {
        mListener = listener;
    }

    public GameHelper.GameHelperListener getGameHelperListener() {
        return mListener;
    }

    public static SignInHelperManager getInstance() {
        return sInstance;
    }

    public void setInvitation(Invitation inv) {
        mInvitation = inv;
    }
    public void setTurnBasedMatch(TurnBasedMatch m) {
        mMatch = m;
    }

    public Invitation getInvitation() {
        return mInvitation;
    }
    public TurnBasedMatch getTurnBasedMatch() {
        return mMatch;
    }

    public boolean hasInvitation() {
        return mInvitation != null;
    }

    public boolean hasTurnBasedMatch() {
        return mMatch != null;
    }

    public void forgetInvitation() {
        mInvitation = null;
    }

    public void forgetTurnBasedMatch() {
        mMatch = null;
    }

    void setSignInErrorReason(GameHelper.SignInFailureReason reason) {
        if (reason != null) {
            mSignInErrorActivityResponse = reason.getActivityResultCode();
            mSignInErrorCode = reason.getServiceErrorCode();
        } else {
            mSignInErrorActivityResponse = Activity.RESULT_OK;
            mSignInErrorCode = 0;
        }
    }

    public static void launchSignIn(Activity act, GameHelper.GameHelperListener listener,
                                       boolean debugEnabled) {
        sInstance.setGameHelperListener(listener);
        Intent i = new Intent(act, SignInHelperActivity.class);
        i.putExtra(SignInHelperActivity.EXTRA_DEBUG_ENABLED, debugEnabled);
        act.startActivity(i);
    }

    public static void showErrorDialog(Activity act) {
        if (sInstance.mSignInErrorCode != 0) {
            GameHelper.showFailureDialog(act, sInstance.mSignInErrorActivityResponse,
                    sInstance.mSignInErrorCode);
        }
    }
}
