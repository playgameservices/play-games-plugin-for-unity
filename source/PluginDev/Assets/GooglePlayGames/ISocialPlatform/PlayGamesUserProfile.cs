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
using UnityEngine.SocialPlatforms;

namespace GooglePlayGames {
/// <summary>
/// Represents a Google Play Games user profile. In the current implementation,
/// this is only used as a base class of <see cref="PlayGamesLocalUser" />
/// and should not be used directly.
/// </summary>
public class PlayGamesUserProfile : IUserProfile {
    internal PlayGamesUserProfile() {
    }

    public string userName {
        get {
            return "";
        }
    }

    public string id {
        get {
            return "";
        }
    }

    public bool isFriend {
        get {
            return false;
        }
    }

    public UserState state {
        get {
            return UserState.Online;
        }
    }

    public UnityEngine.Texture2D image {
        get {
            return null;
        }
    }
}
}

