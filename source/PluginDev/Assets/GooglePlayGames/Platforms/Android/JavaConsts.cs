/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if UNITY_ANDROID
using System;

namespace GooglePlayGames.Android {
    internal class JavaConsts {
        // achievement states
        public const int STATE_HIDDEN = 2;
        public const int STATE_REVEALED = 1;
        public const int STATE_UNLOCKED = 0;

        // achievement types
        public const int TYPE_INCREMENTAL = 1;
        public const int TYPE_STANDARD = 0;

        // status codes
        public const int STATUS_OK = 0;
        public const int STATUS_STALE_DATA = 3;
        public const int STATUS_NO_DATA = 4;
        public const int STATUS_KEY_NOT_FOUND = 2002;
    }
}
#endif
