package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.view.View;
import android.view.Window;

import com.google.example.games.basegameutils.BaseGameActivity;
import com.google.example.games.basegameutils.GameHelper;


public class SignInHelperActivity extends Activity implements GameHelper.GameHelperListener {
    static final int BG_COLOR = 0x40ffffff;
    GameHelper mHelper = null;
    boolean mAttempted = false;
    Handler mHandler = new Handler();

    @Override
    protected void onStop() {
        mHelper.onStop();
        super.onStop();
    }

    @Override
    protected void onStart() {
        mHelper.onStart(this);
        super.onStart();
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        mHelper.onActivityResult(requestCode, resultCode, data);
        super.onActivityResult(requestCode, resultCode, data);
    }

    public void onCreate(Bundle savedInstanceState) {
        mHelper = new GameHelper(this);
        mHelper.setRequestedClients(GameHelper.CLIENT_ALL);
        mHelper.setup(this);
        View v = new View(this);
        v.setBackgroundColor(BG_COLOR);
        setContentView(v);
        super.onCreate(savedInstanceState);
    }

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
            callListener(false);
            finish();
        }
    }

    @Override
    public void onSignInSucceeded() {
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
