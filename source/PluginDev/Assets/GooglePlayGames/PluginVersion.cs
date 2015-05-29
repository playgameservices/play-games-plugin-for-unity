/*
 * Copyright (C) 2014 Google Inc.
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

using System;

namespace GooglePlayGames {
    public class PluginVersion {
        
        // older versions, used when upgrading to other versions
        public const string VersionKeyCPP = "00911";
        public const string VersionKeyU5 = "00915";
        
        public const int VersionInt = 0x0915;
        public const string VersionString = "0.9.15";
        public const string VersionKey = VersionKeyU5;

        // only needed to upgrade to 00915
        public const int MinGmsCoreVersionCode = 0;
        
    }
}

