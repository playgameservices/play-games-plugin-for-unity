#!/bin/sh

if [[ -z "${ANDROID_NDK_ROOT}" ]]; then
  echo "ANDROID_NDK_ROOT must point to the to the Android NDK"
  exit 1
fi

echo ""
echo "Compiling libgpg.so"
$ANDROID_NDK_ROOT/ndk-build
cp -r ../libs ../../

echo ""
echo "Cleaning up / removing build folders..."
rm -rf ../obj
rm -rf ../libs

echo ""
echo "Done!"
