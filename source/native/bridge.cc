/**
 * @file bridge.cc
 * @copyright Copyright 2014 Google Inc. All Rights Reserved.
 * @brief JNI implementation for use by the NativeActivityBridge
 */

#include <android/log.h>
#include <jni.h>
#include "gpg/android_support.h"
#include "bridge.h"


#define LOG_TAG "GamesUnitySDK"
#define LOGD(...) __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)

JNIEXPORT void JNICALL Java_com_google_games_bridge_NativeBridgeActivity_forwardActivityResult
  (JNIEnv * env, jobject activity, jint request_code, jint result_code, jobject result) {
    LOGD("Forwarding OnActivityResult");
    gpg::AndroidSupport::OnActivityResult(env, activity, request_code, result_code, result);
    LOGD("Forwarding OnActivityResult Finished");
}
