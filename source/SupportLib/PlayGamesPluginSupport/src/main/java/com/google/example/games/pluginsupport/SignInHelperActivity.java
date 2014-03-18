package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.view.View;
import android.view.Window;

import com.google.android.gms.games.multiplayer.Invitation;
import com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatch;
import com.google.example.games.basegameutils.BaseGameActivity;
import com.google.example.games.basegameutils.GameHelper;

public class SignInHelperActivity extends HelperActivity {
    boolean mAttempted = false;
    Handler mHandler = new Handler();

    private void callListener(boolean succeeded) {
        GameHelper.GameHelperListener listener =
                SignInHelperManager.getInstance().getGameHelperListener();
        SignInHelperManager.getInstance().setGameHelperListener(null);
        if (listener != null) {
            if (succeeded) {
                listener.onSignInSucceeded();
            } else {
                listener.onSignInFailed();
            }
        }
    }

    @Override
    public void onSignInFailed() {
        if (!mAttempted) {
            mAttempted = true;
            mHelper.beginUserInitiatedSignIn();
        } else {
            // relay the failure reason to the plugin
            GameHelper.SignInFailureReason reason = mHelper.getSignInError();
            SignInHelperManager.getInstance().setSignInErrorReason(reason);
            failAndFinish();
        }
    }

    void failAndFinish() {
        callListener(false);
        finish();
    }

    @Override
    public void onSignInSucceeded() {
        Invitation inv = mHelper.getInvitation();
        if (inv != null) {
            SignInHelperManager.getInstance().setInvitation(inv);
        }

        TurnBasedMatch match = mHelper.getTurnBasedMatch();
        if (match != null) {
            SignInHelperManager.getInstance().setTurnBasedMatch(match);
        }

        final int DELAY = 1000;
        mHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                callListener(true);
                finish();
            }
        }, DELAY);
    }
}
