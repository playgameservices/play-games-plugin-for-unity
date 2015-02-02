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
using UnityEngine.UI;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Multiplayer;


// Handles the prompting of accepting or declining an invitation to play.
// This should be attached to the InvitationPanel object in the main scene.
public class InvitationEvents : MonoBehaviour {

    // associated Text component to display the message.
    public Text message;

    // the invitation object being processed.
    private Invitation inv;

    private bool processed = false;
    private string inviterName = null;

    // Update is called once per frame
    void Update () {

      inv = (inv != null) ? inv : InvitationManager.Instance.Invitation;
      if (inv == null && !processed) {
          Debug.Log("No Invite -- back to main");
          NavigationUtil.ShowMainMenu();
          return;
      }

      if(inviterName == null) {
          inviterName = (inv.Inviter == null || inv.Inviter.DisplayName == null) ? "Someone" :
          inv.Inviter.DisplayName;
          message.text =  inviterName + " is challenging you to a quiz race!";
      }

      if(RaceManager.Instance != null) {
          switch(RaceManager.Instance.State) {
          case RaceManager.RaceState.Aborted:
              Debug.Log("Aborted -- back to main");
              NavigationUtil.ShowMainMenu();
          break;
              case RaceManager.RaceState.Finished:
              Debug.Log("Finished-- back to main");
              NavigationUtil.ShowMainMenu();
          break;
              case RaceManager.RaceState.Playing:
              NavigationUtil.ShowPlayingPanel();
          break;
          case RaceManager.RaceState.SettingUp:
              message.text = "Setting up Race...";
          break;
          case RaceManager.RaceState.SetupFailed:
              Debug.Log("Failed -- back to main");
              NavigationUtil.ShowMainMenu();
          break;
          }
       }
    }

    // Handler script for the Accept button.  This method should be added
    // to the On Click list for the accept button.
    public void OnAccept() {

        if (processed) {
            return;
        }

        processed = true;
        InvitationManager.Instance.Clear();

        RaceManager.AcceptInvitation(inv.InvitationId);
        Debug.Log("Accepted! RaceManager state is now " + RaceManager.Instance.State);

    }

    // Handler script for the decline button.
    public void OnDecline() {

        if (processed) {
            return;
        }

        processed = true;
        InvitationManager.Instance.DeclineInvitation();

        NavigationUtil.ShowMainMenu();
    }
}
