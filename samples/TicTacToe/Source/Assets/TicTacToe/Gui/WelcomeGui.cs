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

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using UnityEngine;

public class WelcomeGui : BaseGui {
    WidgetConfig TitleCfg = new WidgetConfig(0.0f, -0.3f, 1.0f, 0.2f, 100, "Tic Tac Toss");
    WidgetConfig PlayCfg = new WidgetConfig(0.0f, 0.0f, 0.8f, 0.2f, 60, "Play!");
    bool mAuthOnStart = true;
    bool mInMatch = false;
    System.Action<bool> mAuthCallback;

    void Start() {
        Screen.orientation = ScreenOrientation.Portrait;
        mAuthCallback = (bool success) => {
            EndStandBy();
            if (success && !mInMatch) {
                SwitchToMain();
            }
        };

        PlayGamesClientConfiguration config = 
              new PlayGamesClientConfiguration.Builder()
                .WithInvitationDelegate(OnGotInvitation)
                .WithMatchDelegate(OnGotMatch)
                .Build();

        PlayGamesPlatform.InitializeInstance(config);
        // make Play Games the default social implementation
        PlayGamesPlatform.Activate();

        // enable debug logs (note: we do this because this is a sample; on your production
        // app, you probably don't want this turned on by default, as it will fill the user's
        // logs with debug info).
        PlayGamesPlatform.DebugLogEnabled = true;

        // try silent authentication
        if (mAuthOnStart) {
            SetStandBy("Please wait...");
            PlayGamesPlatform.Instance.Authenticate(mAuthCallback, true);
        }
    }

    protected override void DoGUI() {
        GuiLabel(TitleCfg);
        if (GuiButton(PlayCfg)) {
            SetStandBy("Signing in...");
            PlayGamesPlatform.Instance.localUser.Authenticate(mAuthCallback);
        }
    }


    protected void OnGotInvitation(Invitation invitation, bool shouldAutoAccept) {
        if (invitation.InvitationType != Invitation.InvType.TurnBased) {
            // wrong type of invitation!
            return;
        }
        mInMatch = true;
        gameObject.GetComponent<MainMenuGui>().HandleInvitation(invitation, shouldAutoAccept);
    }

    protected void OnGotMatch(TurnBasedMatch match, bool shouldAutoLaunch) {
        mInMatch = true;
        gameObject.GetComponent<MainMenuGui>().HandleMatchTurn(match, shouldAutoLaunch);
    }


    void SwitchToMain() {
        gameObject.GetComponent<MainMenuGui>().MakeActive();
    }

    public void SetAuthOnStart(bool authOnStart) {
        mAuthOnStart = authOnStart;
    }
}
