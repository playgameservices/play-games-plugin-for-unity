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
using GooglePlayGames.BasicApi.Multiplayer;

public class MainMenuGui : BaseGui {
    WidgetConfig TitleCfg = new WidgetConfig(0.0f, -0.4f, 1.0f, 0.2f, 100, "QuizRacer");
    WidgetConfig QuickMatchCfg = new WidgetConfig(0.0f, -0.2f, 0.8f, 0.2f, 60, "Quick Match");
    WidgetConfig InviteCfg = new WidgetConfig(0.0f, 0.05f, 0.8f, 0.2f, 60, "Send Invitation");
    WidgetConfig InboxCfg = new WidgetConfig(0.0f, 0.30f, 0.8f, 0.2f, 60, "Accept Invitation");
    WidgetConfig SignOutCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Bottom, 0.2f, -0.15f, 0.4f, 0.15f,
            TextAnchor.MiddleCenter, 45, "Sign Out");

    public void Start() {
        // no op
    }

    public void Update() {
        // if an invitation arrived, switch to the "invitation incoming" GUI
        // or directly to the game, if the invitation came from the notification
        Invitation inv = InvitationManager.Instance.Invitation;
        if (inv != null) {
            if (InvitationManager.Instance.ShouldAutoAccept) {
                // jump straight into the game, since the user already indicated
                // they want to accept the invitation!
                InvitationManager.Instance.Clear();
                RaceManager.AcceptInvitation(inv.InvitationId);
                gameObject.GetComponent<RaceGui>().MakeActive();
            } else {
                // show the "incoming invitation" screen
                gameObject.GetComponent<IncomingInvitationGui>().MakeActive();
            }
        }
    }

    protected override void DoGUI() {
        GuiLabel(TitleCfg);

        if (GuiButton(QuickMatchCfg)) {
            RaceManager.CreateQuickGame();
            gameObject.GetComponent<RaceGui>().MakeActive();
        } else if (GuiButton(InviteCfg)) {
            RaceManager.CreateWithInvitationScreen();
            gameObject.GetComponent<RaceGui>().MakeActive();
        } else if (GuiButton(InboxCfg)) {
            RaceManager.AcceptFromInbox();
            gameObject.GetComponent<RaceGui>().MakeActive();
        } else if (GuiButton(SignOutCfg)) {
            DoSignOut();
        }
    }

    void DoSignOut() {
        PlayGamesPlatform.Instance.SignOut();
        gameObject.GetComponent<WelcomeGui>().SetAuthOnStart(false);
        gameObject.GetComponent<WelcomeGui>().MakeActive();
    }
}
