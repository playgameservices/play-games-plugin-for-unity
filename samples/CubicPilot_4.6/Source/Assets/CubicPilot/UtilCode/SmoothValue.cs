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

namespace CubicPilot.UtilCode
{
    using System;

    public class SmoothValue
    {
        private float mValue = 0.0f;
        private float mMaxChangeRate = 0.0f;
        private float mMin = float.NegativeInfinity;
        private float mMax = float.PositiveInfinity;
        private int mFilterSamples;
        private float mFilteredValue = 0.0f;

        public SmoothValue(float initialValue, float maxChangeRate)
        {
            mValue = initialValue;
            mMaxChangeRate = maxChangeRate;
        }

        public SmoothValue(float initialValue, float maxChangeRate, float min,
                       float max, int filterSamples)
        {
            mValue = initialValue;
            mMaxChangeRate = maxChangeRate;
            mMin = min;
            mMax = max;
            mFilterSamples = filterSamples < 0 ? 0 : filterSamples;
        }

        public void SetBounds(float min, float max)
        {
            mMin = min;
            mMax = max;
        }

        public float PullTowards(float target, float deltaT)
        {
            mFilteredValue = (mFilteredValue * mFilterSamples + target) /
            (mFilterSamples + 1);
            target = mFilteredValue;

            float displac = deltaT * mMaxChangeRate;
            if (Math.Abs(target - mValue) <= displac)
            {
                mValue = target;
            }
            else
            {
                mValue = Util.Clamp(mValue > target ? mValue - displac : mValue + displac,
                    mMin, mMax);
            }
            return mValue;
        }

        public float Value
        {
            get
            {
                return mValue;
            }
        }
    }
}