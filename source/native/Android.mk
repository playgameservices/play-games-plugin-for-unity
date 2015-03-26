LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := gpg

LOCAL_MODULE_FILENAME := libgpg

LOCAL_STATIC_LIBRARIES := libgpg-1
LOCAL_SRC_FILES := plugin_shim.cc bridge.cc
LOCAL_LDLIBS := -llog

include $(BUILD_SHARED_LIBRARY)

$(call import-add-path,.)
$(call import-module,gpg-cpp-sdk/android)
