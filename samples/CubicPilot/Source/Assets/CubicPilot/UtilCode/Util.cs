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
    using CubicPilot.GameLogic;
    using UnityEngine;

    public class Util : MonoBehaviour
    {
        public static float Clamp(float f, float min, float max)
        {
            return f < min ? min : f > max ? max : f;
        }

        public static int Clamp(int f, int min, int max)
        {
            return f < min ? min : f > max ? max : f;
        }

        public static float Interpolate(float x1, float y1, float x2, float y2, float x)
        {
            return x < x1 ? y1 : x > x2 ? y2 : y1 + (y2 - y1) * (x - x1) / (x2 - x1);
        }

        // Evaluates a function with a trapezoid shape (increases from 0.0 to 1.0,
        // then stays flat for a while, then decreases from 1.0 to 0.0).
        // f(x) will vary between 0.0 and 1.0 when x varies between 0 and climbEndX
        // f(x) is 1.0 between climbEndX and length - climbEndX
        // f(x) decreases from 1.0 to 0.0 when x decreases from length - climbEndX to length
        // f(x) is 0.0 for all other x.
        public static float Trapezoid(float length, float climbEndX, float x)
        {
            return x < 0.0f ? 0.0f : x > length ? 0.0f :
            x < climbEndX ? (x / climbEndX) :
            x > length - climbEndX ? (length - x) / climbEndX : 1.0f;
        }

        // The laziest star graphics ever made! :-D
        public static string MakeStars(int stars)
        {
            if (stars <= 0)
            {
                return "";
            }
            const string YesStar = "\u2605";
            const string NoStar = "\u2606";
            string s = "";
            int i;
            for (i = 0; i < 3; i++)
            {
                s += (stars > i ? YesStar : NoStar) + " ";
            }
            return s.Trim();
        }

        public static float SineWave(float amplitude, float period, float phase)
        {
            float t = Time.timeSinceLevelLoad + phase * period;
            return amplitude * Mathf.Sin(t * 2 * Mathf.PI / period);
        }

        public static bool CalcTargetedDisplacement(float startValue, float target,
                                                float maxDisplac, out float outDisplac)
        {
            maxDisplac = Mathf.Abs(maxDisplac);
            if (Mathf.Abs(startValue - target) <= maxDisplac)
            {
                outDisplac = target - startValue;
                return true;
            }
            else if (target > startValue)
            {
                outDisplac = maxDisplac;
                return false;
            }
            else
            {
                outDisplac = -maxDisplac;
                return false;
            }
        }

        public static string GetLevelLetter(int levelNo)
        {
            return (levelNo >= 0 && levelNo <= GameConsts.MaxLevel) ?
            ((char)(65 + levelNo)).ToString() : "?";
        }

        public static void ShowObject(GameObject o, bool show)
        {
            if (o.GetComponent<Renderer>() != null)
            {
                o.GetComponent<Renderer>().enabled = show;
            }
            int i;
            for (i = 0; i < o.transform.childCount; i++)
            {
                ShowObject(o.transform.GetChild(i).gameObject, show);
            }
        }

        public static void ShowObject(GameObject o)
        {
            ShowObject(o, true);
        }

        public static void HideObject(GameObject o)
        {
            ShowObject(o, false);
        }

        public static bool BlinkFunc(float period, float phase)
        {
            return SineWave(1.0f, period, phase) >= 0;
        }
    }
}