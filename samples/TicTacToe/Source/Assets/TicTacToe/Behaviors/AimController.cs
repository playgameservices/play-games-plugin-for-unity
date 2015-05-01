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

public class AimController : MonoBehaviour {
    private const string ThrowSfxName = "ThrowSfx";

    Action mFireDelegate = null;
    const float TouchSensivity = 5.0f;
    const float MinDistanceToFire = 0.35f;
    const float ForceFactor = 5000.0f;
    const float ForceUpFactor = 500.0f;

    bool mArmed = false;
    int mFingerId = -1;
    Vector2 mTouchAnchor;
    Vector3 mStartPos;
    AudioClip mThrowSfx = null;

    public void SetFireDelegate(Action a) {
        mFireDelegate = a;
    }

    void Start () {
        // initially, disable the object's physics
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().useGravity = false;

        // store the initial position
        mStartPos = gameObject.transform.position;

        mThrowSfx = (AudioClip) Resources.Load(ThrowSfxName);
    }

    // Update is called once per frame
    void Update () {
        if (mArmed) {
            Touch controlFinger = new Touch();
            if (!GetControlFinger(ref controlFinger)) {
                // finger is off the screen -- fire!
                AttemptToFire();
            } else {
                // finger moved -- adjust position
                AdjustAim(controlFinger.position);
            }
        } else {
            AcquireFinger();
        }
    }

    private bool GetControlFinger(ref Touch result) {
        if (mArmed) {
            foreach (Touch t in Input.touches) {
                if (t.fingerId == mFingerId) {
                    result = t;
                    return true;
                }
            }
            return false;
        } else {
            return false;
        }
    }

    private void AcquireFinger() {
        foreach (Touch t in Input.touches) {
            if (t.phase == TouchPhase.Began) {
                mFingerId = t.fingerId;
                mArmed = true;
                mTouchAnchor = t.position;
            }
        }
    }

    private void AdjustAim(Vector2 fingerPos) {
        float factor = TouchSensivity / Screen.width;
        Vector2 delta = fingerPos - mTouchAnchor;
        float targetX = mStartPos.x + delta.x * factor;
        float targetZ = mStartPos.z + delta.y * factor;
        MoveTo(targetX, targetZ);
    }

    private void MoveTo(float x, float z) {
        float diffX = x - gameObject.transform.position.x;
        float diffZ = z - gameObject.transform.position.z;
        gameObject.transform.Translate(new Vector3(diffX, 0, diffZ));
    }

    private void AttemptToFire() {
        float displacement = Vector3.Distance(mStartPos, gameObject.transform.position);
        if (displacement < MinDistanceToFire) {
            mArmed = false;
            MoveTo(mStartPos.x, mStartPos.z);
        } else {
            Vector3 force = (mStartPos - gameObject.transform.position) * ForceFactor;
            force += displacement * new Vector3(0.0f, ForceUpFactor, 0.0f);
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            gameObject.GetComponent<Rigidbody>().AddForce(force);

            if (mThrowSfx != null) {
                AudioSource.PlayClipAtPoint(mThrowSfx, Vector3.zero);
            }

            this.enabled = false;
            if (mFireDelegate != null) {
                mFireDelegate.Invoke();
            }
        }
    }
}
