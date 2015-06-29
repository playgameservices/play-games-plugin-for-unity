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

public class LevelSelGUI : MonoBehaviour {
    public GUISkin GuiSkin;
    public Texture2D DisabledTex;
    public AudioClip UiBeepFx;

    private const int Rows = GameConsts.Menu.LevelSelRows;
    private const int Cols = GameConsts.Menu.LevelSelCols;
    private const float LevelSize = GameConsts.Menu.LevelButtonSize;
    private const float StartX = GameConsts.Menu.LevelGridStartX;
    private const float StartY = GameConsts.Menu.LevelGridStartY;
    private const float Stride = LevelSize + GameConsts.Menu.LevelButtonSpacing;
    private const float LevelFontSize = GameConsts.Menu.LevelFontSize;

    private Countdown mTransition = new Countdown(GameConsts.Menu.TransitionDuration);

    private const float ScoreOffsetX = GameConsts.Menu.LevelScoreOffsetX;
    private const float ScoreOffsetY = GameConsts.Menu.LevelScoreOffsetY;
    private const float ScoreFontSize = GameConsts.Menu.LevelScoreFontSize;

    void OnEnable() {
        mTransition.Start();
        PilotStatsGUI g = gameObject.GetComponent<PilotStatsGUI>();
        if (g != null) {
            g.ShowPilotLevel = false;
        }
    }

    void OnDisable() {
        PilotStatsGUI g = gameObject.GetComponent<PilotStatsGUI>();
        if (g != null) {
            g.ShowPilotLevel = true;
        }
    }

    void Update() {
        mTransition.Update(Time.deltaTime);
    }

    void Beep() {
        AudioSource.PlayClipAtPoint(UiBeepFx, Vector3.zero);
    }

    void OnGUI() {
        GUI.skin = GuiSkin;

        int i, j;
        for (i = 0; i < Rows; i++) {
            for (j = 0; j < Cols; j++) {
                int level = i * Cols + j;
                if (DrawLevelButton(level, j, i)) {
                    // level clicked
                    Beep();
                    GameManager.Instance.GoToLevel(level);
                    return;
                }
            }
        }

        Gu.SetColor(Color.white);
        if (DrawUpButton()) {
            this.enabled = false;
            gameObject.GetComponent<MainMenuGUI>().enabled = true;
            Beep();
        }
    }

    bool DrawUpButton() {
        return Gu.Button(
            Gu.Left(GameConsts.Menu.UpButtonLeft),
            Gu.Top(GameConsts.Menu.UpButtonTop),
            Gu.Dim(GameConsts.Menu.UpButtonWidth),
            Gu.Dim(GameConsts.Menu.UpButtonHeight),
            Gu.Dim(GameConsts.Menu.UpButtonFontSize),
            "<<");
    }

    bool DrawLevelButton(int levelNo, int col, int row) {
        LevelProgress lp = GameManager.Instance.Progress.GetLevelProgress(levelNo);
        if (lp == null) {
            return false;
        }

        float centerX = StartX + Stride * col;
        float centerY = StartY + Stride * row;
        centerX = Util.Interpolate(0.0f, 0.0f, 1.0f, centerX, mTransition.NormalizedElapsed);

        float left = centerX - LevelSize * 0.5f;
        float top = centerY - LevelSize * 0.5f;

        if (GameManager.Instance.Progress.IsLevelUnlocked(levelNo)) {
            Gu.SetColor(Color.white);
            bool r = Gu.Button(Gu.Center(left), Gu.Middle(top),
                    Gu.Dim(LevelSize), Gu.Dim(LevelSize),
                    Gu.Dim(LevelFontSize), Util.GetLevelLetter(levelNo));
            Gu.SetColor(Color.white);

            if (lp.Cleared) {
                Gu.Label(Gu.Center(centerX + ScoreOffsetX), Gu.Middle(centerY + ScoreOffsetY),
                    Gu.Dim(ScoreFontSize), lp.Score.ToString("D5"));
                Gu.Label(Gu.Center(centerX + ScoreOffsetX), Gu.Middle(centerY - ScoreOffsetY),
                    Gu.Dim(ScoreFontSize), Util.MakeStars(lp.Stars));
            }
            return r;
        } else {
            GUI.DrawTexture(new Rect(Gu.Center(left), Gu.Middle(top),
                    Gu.Dim(LevelSize), Gu.Dim(LevelSize)), DisabledTex);
            return false;
        }
    }
}
