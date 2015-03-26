/**
 * @file plugin_shim.cpp
 * @copyright Copyright 2014 Google Inc. All Rights Reserved.
 * @brief Tiny shim that enables initialization of GPG SDK
 */

#include <android/log.h>
#include <jni.h>
#include "gpg/android_initialization.h"


#define LOG_TAG "GamesUnitySDK"
#define LOGD(...) __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)

extern "C" {

/**
 * A trivial implementation of JNI_OnLoad that initializes the Google Play Games
 * SDK when the shared library containing this method definition is loaded.
 */
jint JNI_OnLoad(JavaVM *vm, void *reserved) {
  LOGD("Performing Android initialization of the GPG SDK");
  gpg::AndroidInitialization::JNI_OnLoad(vm);
  return JNI_VERSION_1_6;
}

}  // extern "C"
