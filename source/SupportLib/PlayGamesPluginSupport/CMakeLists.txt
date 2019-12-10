# Copyright (C) 2018 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
##

cmake_minimum_required(VERSION 3.4.1)

add_library(gpg_sdk STATIC IMPORTED)
set_target_properties(gpg_sdk PROPERTIES IMPORTED_LOCATION
    ${GPG_SDK_PATH}/lib/c++/${ANDROID_ABI}/libgpg.a)


add_library(gpg
             SHARED
             src/main/cpp/bridge.cc
             src/main/cpp/plugin_shim.cc
           )


target_include_directories(gpg PRIVATE
     ${GPG_SDK_PATH}/include
)
target_link_libraries(gpg
                       gpg_sdk
                       android
                       log)
