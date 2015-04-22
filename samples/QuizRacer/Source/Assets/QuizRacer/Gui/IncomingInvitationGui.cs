// <copyright file="IncomingInvitationGui.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

using UnityEngine;
using GooglePlayGames.BasicApi.Multiplayer;

public class IncomingInvitationGui : BaseGui
{
    WidgetConfig AcceptButtonCfg = new WidgetConfig(
                                       WidgetConfig.WidgetAnchor.Bottom,
                                       0.25f,
                                       -0.15f,
                                       0.4f,
                                       0.2f,
                                       TextAnchor.MiddleCenter,
                                       45,
                                       "Accept");
    
    WidgetConfig DeclineButtonCfg = new WidgetConfig(
                                        WidgetConfig.WidgetAnchor.Bottom,
                                        -0.25f,
                                        -0.15f,
                                        0.4f,
                                        0.2f,
                                        TextAnchor.MiddleCenter, 45, "Decline");

    protected override void DoGUI()
    {
        Invitation inv = InvitationManager.Instance.Invitation;
        if (inv == null)
        {
            gameObject.GetComponent<MainMenuGui>().MakeActive();
            return;
        }

        string inviterName = null;
        inviterName = (inv.Inviter == null || inv.Inviter.DisplayName == null) ? "Someone" :
                inv.Inviter.DisplayName;

        WidgetConfig msgConfig = new WidgetConfig(0.0f, -0.2f, 1.0f, 0.2f, 35,
                                     inviterName + " is challenging you to a quiz race!");
        GuiLabel(msgConfig);
        if (GuiButton(AcceptButtonCfg))
        {
            InvitationManager.Instance.Clear();
            RaceManager.AcceptInvitation(inv.InvitationId);
            gameObject.GetComponent<RaceGui>().MakeActive();
        }
        else if (GuiButton(DeclineButtonCfg))
        {
            InvitationManager.Instance.DeclineInvitation();
            gameObject.GetComponent<MainMenuGui>().MakeActive();
        }
    }
}
