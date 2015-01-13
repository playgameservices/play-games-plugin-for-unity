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

public class GameProgress {
    private const string PlayerPrefsKey = "simpleshooter-game-progress";

    public const int LevelCount = 12;
    private PilotStats[] mPilotStats = new PilotStats[LevelCount];

    private int mPilotExp = 0; // pilot experience points
    private LevelProgress[] mProgress;

    private TimeSpan mPlayingTime;
    private DateTime mLoadedTime;

    // do we have modifications to write to disk/cloud?
    private bool mDirty = false;

    public GameProgress () {
        mProgress = new LevelProgress[LevelCount];
        int i;
        for (i = 0; i < LevelCount; i++) {
            mProgress[i] = new LevelProgress();
        }
        for (i = 0; i < LevelCount; i++) {
            mPilotStats[i] = new PilotStats(i);
        }
    }

    public LevelProgress GetLevelProgress(int level) {
        return level >= 0 && level < LevelCount ? mProgress[level] : null;
    }

    public void SetLevelProgress(int level, int score, int stars) {
        if (level >= 0 && level < LevelCount) {
            if (mProgress[level].Score < score) {
                mProgress[level].Score = score;
                mDirty = true;
            }
            if (mProgress[level].Stars < stars) {
                mProgress[level].Stars = stars;
                mDirty = true;
            }
        }
    }

    public static GameProgress LoadFromDisk() {
        string s = PlayerPrefs.GetString(PlayerPrefsKey, "");
        if (s == null || s.Trim().Length == 0) {
            return new GameProgress();
        }
        return GameProgress.FromString(s);
    }

    public static GameProgress FromBytes(byte[] b) {
        return GameProgress.FromString(System.Text.ASCIIEncoding.Default.GetString(b));
    }

    public void SaveToDisk() {
        PlayerPrefs.SetString(PlayerPrefsKey, ToString());
        mDirty = false;
    }

    public void MergeWith(GameProgress other) {
        int i;
        for (i = 0; i < LevelCount; i++) {
            if (mProgress[i].MergeWith(other.mProgress[i])) {
                mDirty = true;
            }
        }
        if (other.mPilotExp > mPilotExp) {
            mPilotExp = other.mPilotExp;
            mDirty = true;
        }
        if (other.mPlayingTime  > mPlayingTime) {
             mPlayingTime = other.mPlayingTime;
        }
    }

    public TimeSpan TotalPlayingTime {
        get {
                TimeSpan delta = DateTime.Now.Subtract(mLoadedTime);
                return mPlayingTime.Add(delta);
        }
    }

    public override string ToString () {
        string s = "GPv3:" + mPilotExp.ToString();
        int i;
        for (i = 0; i < LevelCount; i++) {
            s += ":" + mProgress[i].ToString();
        }
        s += ":" + TotalPlayingTime.TotalMilliseconds;
        return s;
    }

    public byte[] ToBytes() {
        return System.Text.ASCIIEncoding.Default.GetBytes(ToString());
    }

    public static GameProgress FromString(string s) {
        GameProgress gp = new GameProgress();
        string[] p = s.Split(new char[] { ':' });
        if (!p[0].StartsWith("GPv")) {
            Debug.LogError("Failed to parse game progress from: " + s);
            return gp;
        }
        gp.mPilotExp = System.Convert.ToInt32(p[1]);
        int i;
        for (i = 2; i < p.Length && i - 2 < LevelCount; i++) {
            gp.GetLevelProgress(i - 2).SetFromString(p[i]);
        }
        if(p[0].Equals("GPv3")) {
            double val = Double.Parse(p[p.Length -1]);
            gp.mPlayingTime = TimeSpan.FromMilliseconds(val>0f?val:0f);
        } else {
            gp.mPlayingTime = new TimeSpan();
        }
        gp.mLoadedTime = DateTime.Now;
        return gp;
    }

    public bool IsLevelUnlocked(int level) {
        LevelProgress prev = GetLevelProgress(level - 1);
        return level == 0 || (prev != null && prev.Cleared);
    }

    public bool AreAllLevelsCleared() {
        int i;
        for (i = 0; i < LevelCount; i++) {
            if (!GetLevelProgress(i).Cleared) {
                return false;
            }
        }
        return true;
    }

    public int PilotExperience {
        get {
            return mPilotExp;
        }
    }

    public bool Dirty {
        get {
            return mDirty;
        }
        set {
            mDirty = value;
        }
    }

    public int PilotLevel {
        get {
            return GetPilotLevel(mPilotExp);
        }
    }

    public bool AddPilotExperience(int points) {
        if (points > 0) {
            int levelBefore = PilotLevel;
            mPilotExp += points;
            mDirty = true;
            return PilotLevel > levelBefore;
        } else {
            return false;
        }
    }

    public static int GetPilotLevel(int expPoints) {
        int i;
        for (i = GameConsts.Progression.ExpForLevel.Length - 1; i >= 0; --i) {
            if (GameConsts.Progression.ExpForLevel[i] <= expPoints) {
                break;
            }
        }
        return Util.Clamp(i, 1, GameConsts.Progression.MaxLevel);
    }

    public bool IsMaxLevel() {
        return PilotLevel >= GameConsts.Progression.MaxLevel;
    }

    public int GetExpForNextLevel() {
        return IsMaxLevel() ? -1 : GameConsts.Progression.ExpForLevel[PilotLevel + 1];
    }

    public PilotStats CurPilotStats {
        get {
            return mPilotStats[PilotLevel];
        }
    }

    public int TotalScore {
        get {
            int sum = 0;
            foreach (LevelProgress lp in mProgress) {
                sum += lp.Score;
            }
            return sum;
        }
    }

    public int TotalStars {
        get {
            int sum = 0;
            foreach (LevelProgress lp in mProgress) {
                sum += lp.Stars;
            }
            return sum;
        }
    }

    // Mostly for debug purposes
    public void ForceLevelUp() {
        mPilotExp = GetExpForNextLevel();
    }

    // Mostly for debug purposes
    public void ForceLevelDown() {
        int level = PilotLevel;
        if (level > 1) {
            mPilotExp = GameConsts.Progression.ExpForLevel[level - 1];
        }
    }
}

