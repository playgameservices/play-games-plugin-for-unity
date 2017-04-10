APP_STL := gnustl_static  # Or c++_shared, gnustl_shared, or gnustl_static
APP_CPPFLAGS := -std=c++11
APP_PLATFORM := android-9

ifndef APP_ABI
  APP_ABI := armeabi-v7a x86
endif
