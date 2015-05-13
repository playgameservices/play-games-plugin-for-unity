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


public class Countdown {
    private bool mActive = false;
    private float mInitial = 1.0f;
    private float mRemaining = 1.0f;

    public Countdown(float initial) : this(true, initial) {}
    public Countdown(bool active) : this(active, 1.0f) {}
    public Countdown() : this(false, 1.0f) {}
    public Countdown(bool active, float initial) {
        mActive = active;
        mInitial = mRemaining = initial;
    }

    public void Update(float deltaT, bool autoStopOnExpired) {
        if (mActive) {
            mRemaining = Util.Clamp(mRemaining - deltaT, 0, mInitial);
        }
        if (autoStopOnExpired && Expired) {
            Stop();
        }
    }

    public void Update(float deltaT) {
        Update(deltaT, false);
    }

    public void Start() {
        mRemaining = mInitial;
        mActive = true;
    }

    public void Start(float initial) {
        mRemaining = mInitial = initial;
        mActive = true;
    }

    public void Stop() {
        mRemaining = mInitial;
        mActive = false;
    }

    public void Pause() {
        mActive = false;
    }

    public void Resume() {
        mActive = true;
    }

    public bool Expired {
        get {
            return mActive && mRemaining <= 0;
        }
    }

    public float Initial {
        get {
            return mInitial;
        }
    }

    public float Remaining {
        get {
            return mRemaining;
        }
    }

    public float Elapsed {
        get {
            return mInitial - mRemaining;
        }
    }

    public float NormalizedElapsed {
        get {
            return Elapsed / mInitial;
        }
    }

    public float NormalizedRemaining {
        get {
            return Remaining / mInitial;
        }
    }

    public bool Active {
        get {
            return mActive;
        }
    }

    public override string ToString () {
        return string.Format("[Countdown: active={0}, initial={1}, remaining={2}]", mActive,
            mInitial, mRemaining);
    }
}


