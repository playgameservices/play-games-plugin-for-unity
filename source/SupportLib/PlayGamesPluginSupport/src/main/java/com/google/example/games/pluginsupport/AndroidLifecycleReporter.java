package com.google.example.games.pluginsupport;

@Deprecated
public interface AndroidLifecycleReporter {
    public void registerAndroidLifecycleListener(AndroidLifecycleListener listener);
    public void unregisterAndroidLifecycleListener(AndroidLifecycleListener listener);
    public int getAndroidLifecycleReporterVersion();
}
