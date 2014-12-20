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

public class ShowTutorial : MonoBehaviour {
    public GUISkin GuiSkin;
    public Texture2D ArrowTex;
    public float DelayBeforeStart;

    private Countdown mCountdown = new Countdown(false,
        GameConsts.Tutorial.PhaseDuration);
    private Countdown mTransitionCd = new Countdown(false,
        GameConsts.Menu.TransitionDuration);
    int phase = 0;
    const int Phases = 3;

    void Start() {
        phase = 0;
        mCountdown.Start();
        mTransitionCd.Start();

        bool hasController = Input.GetJoystickNames().Length > 0;
        // skip over the touch interface steps if using a controller.
        if(hasController) {
            phase = 3;
        }
        if (GameManager.Instance.Level > 0) {
            // no tutorial
            this.enabled = false;
        }
    }

    void Update () {
        DelayBeforeStart -= Time.deltaTime;
        if (DelayBeforeStart > 0) {
            return;
        }

        mCountdown.Update(Time.deltaTime);
        mTransitionCd.Update(Time.deltaTime);
        if (mCountdown.Expired) {
            ++phase;
            if (phase >= Phases) {
                this.enabled = false;
            }
            mCountdown.Start();
            mTransitionCd.Start();
        }
    }

    void OnGUI() {

        if (Time.timeScale == 0) {
                        return;
        }
        
        if (DelayBeforeStart > 0) {
            return;
        }

        GUI.skin = GuiSkin;
        switch (phase) {
        case 0:
            float aspect = ArrowTex.width / (float)ArrowTex.height;
            GUI.DrawTexture(new Rect(Gu.Left(GameConsts.Tutorial.SteerArrowX),
                Gu.Top(0), aspect * Screen.height,
                (int)(mTransitionCd.NormalizedElapsed * Screen.height)), ArrowTex);
            Gu.Label(
                Gu.Left(GameConsts.Tutorial.SteerTextX),
                (int)Util.Interpolate(0.0f, 0.0f, 1.0f, Gu.Middle(0),
                    mTransitionCd.NormalizedElapsed),
                Gu.Dim(GameConsts.Tutorial.FontSize),
                Strings.Tutorial1);
            break;
        case 1:
            Gu.Label(Gu.Right(GameConsts.Tutorial.FireTextX),
                (int)Util.Interpolate(0.0f, Screen.height, 1.0f, Gu.Middle(0),
                    mTransitionCd.NormalizedElapsed),
                Gu.Dim(GameConsts.Tutorial.FontSize),
                Strings.Tutorial2);
            break;
        default:
            Gu.Label(Gu.Center(0),
                (int)(Util.Interpolate(0.0f, 0.0f, 1.0f, Gu.Middle(0),
                    mTransitionCd.NormalizedElapsed)),
                Gu.Dim(GameConsts.Tutorial.FontSize),
                Strings.Tutorial3);
            break;
        }
    }
}
