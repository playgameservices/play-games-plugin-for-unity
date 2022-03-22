package com.google.games.bridge;

final class CommonUIStatus {

    /**
      * Should be aligned to:
      * PluginDev/Assets/GooglePlayGames/BasicApi/CommonTypes.cs enum UIStatus
      * */
    static final int VALID = 1;
    static final int INTERNAL_ERROR = -2;
    static final int NOT_AUTHORIZED = -3;
    static final int CANCELLED = -6;
    static final int UI_BUSY = -12;

    private CommonUIStatus() {
    }

}
