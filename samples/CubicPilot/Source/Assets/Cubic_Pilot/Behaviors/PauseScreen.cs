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

public class PauseScreen : MonoBehaviour {
    private bool mIsPaused = false;
    public GUISkin GuiSkin;
    public Texture2D BlackoutTexture;
    public Texture2D PauseButtonTexture;

    void Start () {

    }

    void DoPause() {
        mIsPaused = true;
        Time.timeScale = 0;
    }

    void Update () {
        if (!mIsPaused && Input.GetKeyDown(KeyCode.Escape)) {
            DoPause();
        }
    }

#if UNITY_ANDROID
    bool DrawPauseButton() {
        // Android doesn't have a pause button -- use the back button instead.
        return false;
    }
#else
    bool DrawPauseButton() {
        return GUI.Button(new Rect(
            Gu.Right(GameConsts.PauseScreen.PauseButtonX),
            Gu.Bottom(GameConsts.PauseScreen.PauseButtonY),
            Gu.Dim(GameConsts.PauseScreen.PauseButtonSize),
            Gu.Dim(GameConsts.PauseScreen.PauseButtonSize)), PauseButtonTexture);
    }
#endif

    void OnGUI() {
        GUI.skin = GuiSkin;
        GUI.depth = 0;

        if (!mIsPaused && DrawPauseButton()) {
            DoPause();
            return;
        }

        if (!mIsPaused) {
            return;
        }


        DrawBackground();
        DrawTitle();
        if (DrawResumeButton()) {
            mIsPaused = false;
            Time.timeScale = 1;
            return;
        }

        if (DrawQuitButton()) {
            Time.timeScale = 1;
            GameManager.Instance.QuitToMenu();
            return;
        }
    }

    void DrawBackground() {
        GUI.DrawTexture(new Rect(
            Gu.Left(0), Gu.Top(0), Screen.width, Screen.height), BlackoutTexture);
    }

    void DrawTitle() {
        Gu.SetColor(Color.black);
        Gu.Label(Gu.Center(0), Gu.Middle(GameConsts.PauseScreen.TitleY),
            Gu.Dim(GameConsts.PauseScreen.TitleFontSize),
            Strings.GamePaused);
    }

    bool DrawResumeButton() {
        Gu.SetColor(Color.white);
        return Gu.Button(Gu.Center(GameConsts.PauseScreen.ResumeX),
            Gu.Middle(GameConsts.PauseScreen.ResumeY),
            Gu.Dim(GameConsts.PauseScreen.ButtonWidth),
            Gu.Dim(GameConsts.PauseScreen.ButtonHeight),
            Gu.Dim(GameConsts.PauseScreen.ButtonFontSize),
            Strings.ResumeGame);
    }

    bool DrawQuitButton() {
        Gu.SetColor(Color.white);
        return Gu.Button(Gu.Center(GameConsts.PauseScreen.QuitX),
            Gu.Middle(GameConsts.PauseScreen.QuitY),
            Gu.Dim(GameConsts.PauseScreen.ButtonWidth),
            Gu.Dim(GameConsts.PauseScreen.ButtonHeight),
            Gu.Dim(GameConsts.PauseScreen.ButtonFontSize),
            Strings.QuitGame);
    }
}
