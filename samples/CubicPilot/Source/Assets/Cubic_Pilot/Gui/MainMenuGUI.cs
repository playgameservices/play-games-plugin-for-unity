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
using GooglePlayGames;

public class MainMenuGUI : MonoBehaviour {
    public GUISkin GuiSkin;
    public GUISkin SignInButtonGuiSkin;
    public Countdown mTransition = new Countdown(false, GameConsts.Menu.TransitionDuration);
    public Texture2D GooglePlusTex;
    public Texture2D SignInBarTex;
    public AudioClip UiBeepFx;
    private static bool sAutoAuthenticate = true;

    void OnEnable() {
        mTransition.Start();
    }

    void Beep() {
        AudioSource.PlayClipAtPoint(UiBeepFx, Vector3.zero);
    }

    void Start() {
        // if this is the first time we're running, bring up the sign in flow
        if (sAutoAuthenticate) {
            GameManager.Instance.Authenticate();
            sAutoAuthenticate = false;
        }
    }

    void OnGUI() {
        bool standBy = GameManager.Instance.Authenticating;
        bool authenticated = GameManager.Instance.Authenticated;

        GUI.skin = GuiSkin;

        DrawTitle();

        if (standBy) {
            DrawPleaseWait();
            return;
        }

        if (DrawPlayButton()) {
            Beep();
            if (!GameManager.Instance.Progress.IsLevelUnlocked(1)) {
                // If level 0 is the only possibility, don't bother to show the
                // level selection screen, just go straight into the level.
                GameManager.Instance.GoToLevel(0);
            } else {
                // Show the level selection screen
                gameObject.GetComponent<LevelSelGUI>().enabled = true;
                this.enabled = false;
            }
        }
  
        if (authenticated && DrawAchButton()) {
            Beep();
            GameManager.Instance.ShowAchievementsUI();
        }

        if (authenticated && DrawLbButton()) {
            Beep();
            GameManager.Instance.ShowLeaderboardUI();
        }

        if (authenticated && DrawLoadButton()) {
             Beep ();
             GameManager.Instance.LoadFromCloud();
        }

        if (!authenticated && DrawSignInButton()) {
            Beep();
            GameManager.Instance.Authenticate();
        }

        if (authenticated && DrawSignOutButton()) {
            Beep();
            GameManager.Instance.SignOut();
        }

        if (authenticated) {
            DrawSignedInBlurb();
        }

        DrawBuildString();
    }

    void DrawPleaseWait() {
        Gu.SetColor(Color.black);
        Gu.Label(Gu.Center(0), Gu.Middle(0), Gu.Dim(GameConsts.Menu.PleaseWaitFontSize),
            GameManager.Instance.AuthProgressMessage);
    }

    void DrawBuildString() {
        Gu.SetColor(Color.black);
        Gu.Label(Gu.Left(GameConsts.Menu.BuildStringX), Gu.Bottom(GameConsts.Menu.BuildStringY),
            Gu.Dim(GameConsts.Menu.BuildStringFontSize), Strings.BuildString, false);
    }

    void Update() {
        mTransition.Update(Time.deltaTime);
    }

    void DrawTitle() {
        Gu.SetColor(GameConsts.ThemeColor);
        Gu.Label(Gu.Center(0),
            (int) Util.Interpolate(0.0f, 0.0f, 1.0f, Gu.Top(GameConsts.Menu.TitleY),
                mTransition.NormalizedElapsed),
            Gu.Dim(GameConsts.Menu.TitleFontSize), Strings.GameTitle);
    }

    bool DrawPlayButton() {
        float w = GameConsts.Menu.PlayButtonWidth;
        float h = GameConsts.Menu.PlayButtonHeight;
        Gu.SetColor(Color.white);
        return Gu.Button(
            Gu.Center(-w/2),
            (int)Util.Interpolate(0.0f, Screen.height, 1.0f, Gu.Middle(-h/2) + 50f,
                mTransition.NormalizedElapsed),
            Gu.Dim(w), Gu.Dim(h),
            Gu.Dim(GameConsts.Menu.PlayButtonFontSize),
            Strings.Play);
    }

    bool DrawAchButton() {
        float w = GameConsts.Menu.AchButtonWidth;
        float h = GameConsts.Menu.AchButtonHeight;
        float x = GameConsts.Menu.AchButtonX;
        float y = GameConsts.Menu.AchButtonY;
        return Gu.Button(Gu.Center(x),
            (int)Util.Interpolate(0.0f, Screen.height, 1.0f, Gu.Middle(y),
                mTransition.NormalizedElapsed),
            Gu.Dim(w), Gu.Dim(h),
            Gu.Dim(GameConsts.Menu.AchFontSize),
            Strings.Achievements);
    }

    bool DrawLoadButton() {
                float w = GameConsts.Menu.LoadButtonWidth;
                float h = GameConsts.Menu.LoadButtonHeight;
                float x = GameConsts.Menu.LoadButtonX;
                float y = GameConsts.Menu.LoadButtonY;
                return Gu.Button(Gu.Center(x),
                                 (int)Util.Interpolate(0.0f, Screen.height, 1.0f, Gu.Middle(y),
                                      mTransition.NormalizedElapsed),
                                 Gu.Dim(w), Gu.Dim(h),
                                 Gu.Dim(GameConsts.Menu.AchFontSize),
                                 Strings.LoadGame);
    }

    bool DrawLbButton() {
        float w = GameConsts.Menu.LbButtonWidth;
        float h = GameConsts.Menu.LbButtonHeight;
        float x = GameConsts.Menu.LbButtonX;
        float y = GameConsts.Menu.LbButtonY;
        return Gu.Button(Gu.Center(x),
            (int)Util.Interpolate(0.0f, Screen.height, 1.0f, Gu.Middle(y),
                mTransition.NormalizedElapsed),
            Gu.Dim(w), Gu.Dim(h),
            Gu.Dim(GameConsts.Menu.LbFontSize),
            Strings.Leaderboards);
    }

    void DrawSignInBar() {
        // draw the sign-in bar (the white bar behind the sign in button)
        Rect r = new Rect(Gu.Left(0), Gu.Bottom(GameConsts.Menu.SignInBarY),
            (int)Util.Interpolate(0.0f, 0.0f, 1.0f, Screen.width,
                mTransition.NormalizedElapsed),
            Gu.Dim(GameConsts.Menu.SignInBarHeight));
        GUI.DrawTexture(r, SignInBarTex);
    }

    void DrawSignInBlurb(string text) {
        bool authenticated = GameManager.Instance.Authenticated;
        float x = authenticated ? 0.0f : GameConsts.Menu.SignInBlurbX;

        // draw sign in explanation text
        Gu.SetColor(authenticated ? GameConsts.Menu.SignedInBlurbColor :
            GameConsts.Menu.SignInBlurbColor);
        Gu.Label(Gu.Center(x),
            Gu.Bottom(GameConsts.Menu.SignInBlurbY),
            Gu.Dim(GameConsts.Menu.SignInBlurbFontSize),
            text);
    }

    bool DrawSignInButton() {
        DrawSignInBar();

        // draw the sign in button
        GUI.skin = SignInButtonGuiSkin;
        bool result = Gu.Button(Gu.Center(GameConsts.Menu.SignInButtonX),
            Gu.Bottom(GameConsts.Menu.SignInButtonY),
            Gu.Dim(GameConsts.Menu.SignInButtonWidth),
            Gu.Dim(GameConsts.Menu.SignInButtonHeight),
            Gu.Dim(GameConsts.Menu.SignInButtonFontSize),
            "     " + Strings.SignIn);
        GUI.skin = GuiSkin;

        // draw the Google+ logo
        GUI.DrawTexture(new Rect(Gu.Center(GameConsts.Menu.GooglePlusLogoX),
            Gu.Bottom(GameConsts.Menu.GooglePlusLogoY),
            Gu.Dim(GameConsts.Menu.GooglePlusLogoSize),
            Gu.Dim(GameConsts.Menu.GooglePlusLogoSize)), GooglePlusTex);

        // draw sign in encouragement text
        DrawSignInBlurb(Strings.SignInBlurb);

        return result;
    }

    void DrawSignedInBlurb() {
        DrawSignInBar();
        DrawSignInBlurb(Strings.SignedInBlurb);
    }

    bool DrawSignOutButton() {
        return Gu.Button(Gu.Right(GameConsts.Menu.SignOutButtonX),
            Gu.Bottom(GameConsts.Menu.SignOutButtonY),
            Gu.Dim(GameConsts.Menu.SignOutButtonWidth),
            Gu.Dim(GameConsts.Menu.SignOutButtonHeight),
            Gu.Dim(GameConsts.Menu.SignInButtonFontSize),
            Strings.SignOut);
    }
}
