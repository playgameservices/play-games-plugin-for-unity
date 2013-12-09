package com.google.example.games.pluginsupport;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

@Deprecated
public interface AndroidLifecycleListener {
    public void onStart(Activity act);
    public void onStop(Activity act);
    public void onPause(Activity act);
    public void onResume(Activity act);
    public void onDestroy(Activity act);
    public void onActivityResult(Activity act, int request, int response, Intent data);
}
