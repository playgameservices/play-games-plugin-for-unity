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
namespace CubicPilot.GameLogic
{
    using System;
    using UnityEngine;

    public class LevelProgress
    {
        private int mScore, mStars;

        public int Score
        {
            get
            {
                return mScore;
            }
            set
            {
                mScore = value;
            }
        }

        public int Stars
        {
            get
            {
                return mStars;
            }
            set
            {
                mStars = value;
            }
        }

        public bool Cleared
        {
            get
            {
                return mScore > 0;
            }
        }

        public LevelProgress()
        {
            mScore = mStars = 0;
        }

        public LevelProgress(int score, int stars)
        {
            mScore = score;
            mStars = stars;
        }

        public override string ToString()
        {
            return string.Format("LP {0} {1}", Score, Stars);
        }

        public void SetFromString(string s)
        {
            string[] p = s.Split(new char[] { ' ' });
            if (p.Length != 3 || !p[0].Equals("LP"))
            {
                Debug.LogError("Failed to parse level progress from: " + s);
                mStars = mScore = 0;
            }
            mScore = Convert.ToInt32(p[1]);
            mStars = Convert.ToInt32(p[2]);
        }

        public static LevelProgress FromString(string s)
        {
            LevelProgress lp = new LevelProgress();
            lp.SetFromString(s);
            return lp;
        }

        public bool MergeWith(LevelProgress other)
        {
            bool modified = false;
            if (other.mScore > mScore)
            {
                mScore = other.mScore;
                modified = true;
            }
            if (other.mStars > mStars)
            {
                mStars = other.mStars;
                modified = true;
            }
            return modified;
        }
    }
}
