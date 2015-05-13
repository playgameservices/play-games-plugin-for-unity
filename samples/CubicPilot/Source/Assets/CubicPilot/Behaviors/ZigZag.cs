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

using UnityEngine;
using System.Collections;

public class ZigZag : MonoBehaviour {
    public float Amplitude = 1.0f;
    public float LatSpeed = 5.0f;

    private bool mIncreasing = false;
    private float mDelta = 0.0f;

    void Update () {
        float diff;
        if (Util.CalcTargetedDisplacement(mDelta, mIncreasing ? Amplitude : -Amplitude,
                Time.deltaTime * LatSpeed, out diff)) {
            // we hit the target coord, so reverse movement
            mIncreasing = !mIncreasing;
        }
        gameObject.transform.Translate(0.0f, diff, 0.0f, Space.World);
        mDelta += diff;
    }
}
