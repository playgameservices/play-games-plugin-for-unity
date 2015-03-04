#!/bin/sh

if [[ -z "${ANDROID_SDK_ROOT}" ]]; then
  echo "ANDROID_SDK_ROOT must point to the to the Android SDK"
  exit 1
fi

SOURCE_ROOT=../../../../../../
ANDROID_JAR=${ANDROID_SDK_ROOT}/platforms/android-19/android.jar
SUPPORT_JAR=$SOURCE_ROOT/SupportLib/PlayGamesPluginSupport/build/bundles/debug/classes.jar


javah -jni -classpath $SUPPORT_JAR:$ANDROID_JAR com.google.games.bridge.NativeBridgeActivity

mv com_google_games_bridge_NativeBridgeActivity.h bridge.h
