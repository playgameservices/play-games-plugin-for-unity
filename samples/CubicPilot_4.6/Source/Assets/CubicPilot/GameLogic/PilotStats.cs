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
    using CubicPilot.UtilCode;

    public class PilotStats
    {
        int mLevel;

        public PilotStats(int level)
        {
            mLevel = Util.Clamp(level, 1, GameConsts.Progression.MaxLevel);
        }

        public string Title
        {
            get
            {
                int l = Util.Clamp(mLevel, 1, GameConsts.Progression.Titles.Length - 1);
                return GameConsts.Progression.Titles[l];
            }
        }

        public float FireCooldown
        {
            get
            {
                int l = Util.Clamp(mLevel, 1, GameConsts.Progression.FireCooldown.Length - 1);
                return GameConsts.Progression.FireCooldown[l];
            }
        }

        public int ShieldPoints
        {
            get
            {
                int l = Util.Clamp(mLevel, 1, GameConsts.Progression.ShieldPoints.Length - 1);
                return GameConsts.Progression.ShieldPoints[l];
            }
        }

        public float LaserSpeed
        {
            get
            {
                int l = Util.Clamp(mLevel, 1, GameConsts.Progression.LaserSpeed.Length - 1);
                return GameConsts.Progression.LaserSpeed[l];
            }
        }

        public int Level
        {
            get
            {
                return mLevel;
            }
        }

        public int Damage
        {
            get
            {
                int l = Util.Clamp(mLevel, 1, GameConsts.Progression.Damage.Length - 1);
                return GameConsts.Progression.Damage[l];
            }
        }
    }
}