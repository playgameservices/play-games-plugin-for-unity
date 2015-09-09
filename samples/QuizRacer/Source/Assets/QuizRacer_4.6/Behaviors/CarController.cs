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

public class CarController : MonoBehaviour {
    const float MaxAnimSpeed = 2.0f;
    public float StartX;
    public float EndX;

    private bool mBlink = false;
    private string mParticipantId = null;

    void Update () {
        if (mParticipantId == null) {
            MakeVisible(false);
            return;
        }

        RaceManager mgr = RaceManager.Instance;
        if (mParticipantId != null && mgr != null) {
            float progress = mgr.GetRacerProgress(mParticipantId);
            float diff = StartX + (EndX - StartX) * progress - gameObject.transform.position.x;
            if (diff > MaxAnimSpeed * Time.deltaTime) {
                diff = MaxAnimSpeed * Time.deltaTime;
            } else if (diff < -MaxAnimSpeed * Time.deltaTime) {
                diff = -MaxAnimSpeed * Time.deltaTime;
            }
            gameObject.transform.Translate(new Vector3(diff, 0.0f, 0.0f), Space.World);
            if (mBlink) {
                MakeVisible(!mBlink || 0 != (int)(Time.time / 0.25f) % 3);
            }
        }
    }

    public void SetParticipantId(string id) {
        mParticipantId = id;
    }

    public void SetBlinking(bool blink) {
        mBlink = blink;
    }

    private void MakeVisible(bool visible) {
        BehaviorUtils.MakeVisible(gameObject, visible);
    }

    public string ParticipantId {
        get {
            return mParticipantId;
        }
    }

    public void Reset() {
        mParticipantId = null;
        mBlink = false;
        float diff = StartX - gameObject.transform.position.x;
        gameObject.transform.Translate(new Vector3(diff, 0.0f, 0.0f), Space.World);
    }
}
