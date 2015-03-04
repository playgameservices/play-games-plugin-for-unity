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
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace GooglePlayGames {

internal delegate void ReportProgress(string id,double progress,Action<bool> callback);

/// <summary>
/// Represents a Google Play Games achievement. It can be used to report an achievement
/// to the API, offering identical functionality as <see cref="PlayGamesPlatform.ReportProgress" />.
/// </summary>
internal class PlayGamesAchievement : IAchievement {
    private readonly ReportProgress mProgressCallback;
    private string mId = "";
    private double mPercentComplete = 0.0f;
    private static readonly DateTime _sentinel = new DateTime(1970, 1, 1, 0, 0, 0, 0);

    internal PlayGamesAchievement() : this(PlayGamesPlatform.Instance.ReportProgress) {
    }

    internal PlayGamesAchievement(ReportProgress progressCallback) {
        mProgressCallback = progressCallback;
    }

    /// <summary>
    /// Reveals, unlocks or increment achievement. Call after setting
    /// <see cref="id" /> and <see cref="percentCompleted" />. Equivalent to calling
    /// <see cref="PlayGamesPlatform.ReportProgress" />.
    /// </summary>
    public void ReportProgress(Action<bool> callback) {
        mProgressCallback.Invoke(mId, mPercentComplete, callback);
    }

    /// <summary>
    /// Gets or sets the id of this achievement.
    /// </summary>
    /// <returns>
    /// The identifier.
    /// </returns>
    public string id {
        get {
            return mId;
        }
        set {
            mId = value;
        }
    }

    /// <summary>
    /// Gets or sets the percent completed.
    /// </summary>
    /// <returns>
    /// The percent completed.
    /// </returns>
    public double percentCompleted {
        get {
            return mPercentComplete;
        }
        set {
            mPercentComplete = value;
        }
    }

    /// <summary>
    /// Not implemented. Always returns false.
    /// </summary>
    public bool completed {
        get {
            return false;
        }
    }

    /// <summary>
    /// Not implemented. Always returns false.
    /// </summary>
    public bool hidden {
        get {
            return false;
        }
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    /// <returns>
    /// Not implemented. Always returns Jan 01, 1970, 00:00:00.
    /// </returns>
    public DateTime lastReportedDate {
        get {
            // NOTE: we don't implement this field. We always return
            // 1970-01-01 00:00:00
            return _sentinel;
        }
    }
}
}

