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

public class TheEndGui : MonoBehaviour {
    public GUISkin GuiSkin;
    private Countdown mTransitionCd = new Countdown(false, GameConsts.Menu.TransitionDuration);

    void Start() {
        mTransitionCd.Start();
    }

    void Update() {
        mTransitionCd.Update(Time.deltaTime, false);
    }

    void OnGUI() {
        GUI.skin = GuiSkin;
        Gu.SetColor(Color.black);
        Gu.Label(Gu.Center(GameConsts.EndScreen.EndTextX),
            (int)Util.Interpolate(0.0f, Screen.height, 1.0f,
                Gu.Middle(GameConsts.EndScreen.EndTextY),
                mTransitionCd.NormalizedElapsed),
            Gu.Dim(GameConsts.EndScreen.EndTextFontSize),
            Strings.EndText, false);

        bool wantOut = DrawUpButton();
        if (mTransitionCd.Expired && wantOut) {
            GameManager.Instance.QuitToMenu();
        }
    }

    bool DrawUpButton() {
        Gu.SetColor(Color.white);
        return Gu.Button(
            Gu.Left(GameConsts.Menu.UpButtonLeft),
            Gu.Top(GameConsts.Menu.UpButtonTop),
            Gu.Dim(GameConsts.Menu.UpButtonWidth),
            Gu.Dim(GameConsts.Menu.UpButtonHeight),
            Gu.Dim(GameConsts.Menu.UpButtonFontSize),
            "<<");
    }
}
