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
using System;

public class CollisionSfx : MonoBehaviour {
    private const string SfxName = "HitSfx";
    private AudioClip mSfx;
    private int mMaxTimes = 6;

    void Start() {
        mSfx = (AudioClip) Resources.Load(SfxName);
    }

    public void OnCollisionEnter(Collision c) {
        if (c.gameObject == null || mMaxTimes <= 0 || mSfx == null) {
            return;
        }

        mMaxTimes--;
           AudioSource.PlayClipAtPoint(mSfx, Vector3.zero);
    }
}
