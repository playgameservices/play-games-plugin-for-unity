package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.content.Intent;

import com.google.example.games.basegameutils.GameHelper;

public class SignInHelperManager {
    private static SignInHelperManager sInstance = new SignInHelperManager();

    GameHelper.GameHelperListener mListener = null;

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

    public static void launchSignIn(Activity act, GameHelper.GameHelperListener listener) {
        sInstance.setGameHelperListener(listener);
        act.startActivity(new Intent(act, SignInHelperActivity.class));
    }
}
