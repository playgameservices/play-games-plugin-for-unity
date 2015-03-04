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

public class MainMenuEvents : MonoBehaviour {

    // seconds for showing an error message;
    private const float ERROR_STATUS_TIMEOUT = 10.0f;

    // seconds for showing an info message;
    private const float INFO_STATUS_TIMEOUT = 2.0f;

    // remaining time to display message.
    private float mStatusCountdown = 0f;

    // associated Text object for the status message.
    public GameObject statusText;
    private string mStatusMsg = null;

    // state info, to avoid processing events multiple times.
    private bool processed = false;

    // Start the script.  The main menu needs to setup the invitation
    // manager in order to detect incoming invitiations.
    void Start () {

    }

    // Update is called once per frame
    void Update () {

        HandleStatusUpdate();

        UpdateInvitation();

        if (RaceManager.Instance == null) {
            return;
        }
        switch (RaceManager.Instance.State) {
        case RaceManager.RaceState.SettingUp:
            if(statusText != null) {
              //reset the timer, we can stay here for a long time.
              mStatusMsg = null;
              ShowStatus("Waiting for opponents...",false);
            }
        break;
        case RaceManager.RaceState.SetupFailed:
            ShowStatus( "Game setup failed", true);
            RaceManager.Instance.CleanUp();
            processed = false;
        break;
        case RaceManager.RaceState.Aborted:
            ShowStatus( "Race Aborted.", true);
            RaceManager.Instance.CleanUp();
            processed = false;
        break;
        case RaceManager.RaceState.Finished:
            // really should not see this on the main menu page,
            // so go to playing panel to display the final outcome of the race.
            NavigationUtil.ShowPlayingPanel();
            processed = false;
        break;
        case RaceManager.RaceState.Playing:
            NavigationUtil.ShowPlayingPanel();
            processed = false;
        break;
        default:
            Debug.Log("RaceManager.Instance.State = " + RaceManager.Instance.State);
        break;
        }
    }

    // Handle the displaying and clearing of the status message.
    void HandleStatusUpdate() {
        if(statusText.activeSelf) {
            mStatusCountdown -= Time.deltaTime;
            if(mStatusCountdown <= 0) {
                statusText.SetActive(false);
            }
        }
    }

    // Handle detecting incoming invitations.
    public void UpdateInvitation() {

        if (InvitationManager.Instance == null) {
          return;
        }

        // if an invitation arrived, switch to the "invitation incoming" GUI
        // or directly to the game, if the invitation came from the notification
        Invitation inv = InvitationManager.Instance.Invitation;
        if (inv != null) {
            if (InvitationManager.Instance.ShouldAutoAccept) {
                // jump straight into the game, since the user already indicated
                // they want to accept the invitation!
                InvitationManager.Instance.Clear();
                RaceManager.AcceptInvitation(inv.InvitationId);
                NavigationUtil.ShowPlayingPanel();
            } else {
                // show the "incoming invitation" screen
                NavigationUtil.ShowInvitationPanel();
            }
        }
    }

    //Shows a status message.  Errors are displayed differently.
    void ShowStatus(string msg, bool error) {
        if (msg !=  mStatusMsg) {
            mStatusMsg = msg;
            statusText.SetActive(true);
            Text txt = statusText.GetComponentInChildren<Text>();
            txt.text = msg;
            if(error) {
                Color c = statusText.GetComponent<Image>().color;
                c.a = 1.0f;
                statusText.GetComponent<Image>().color = c;
                mStatusCountdown = ERROR_STATUS_TIMEOUT;
            } else {
                Color c = statusText.GetComponent<Image>().color;
                c.a = 0.0f;
                statusText.GetComponent<Image>().color = c;
                mStatusCountdown = INFO_STATUS_TIMEOUT;
            }
        }
    }

    //Handler for the Quick Match button.
    public void OnQuickMatch() {
        if (processed) {
            return;
        }
        processed = true;
        RaceManager.CreateQuickGame();
    }

    //Handler for the send initation button.
    public void OnInvite() {
        RaceManager.CreateWithInvitationScreen();
    }

    //Handler for the inbox button.
    public void OnInboxClicked() {
        if(processed) {
            return;
        }
        processed = true;
        RaceManager.AcceptFromInbox();
    }

    //Handler for the signout button.
    public void OnSignoutClicked () {
        if(RaceManager.Instance != null) {
            RaceManager.Instance.CleanUp();
        }
        Debug.Log("Signing out...");
        if (PlayGamesPlatform.Instance != null) {
            PlayGamesPlatform.Instance.SignOut();
        }
        else {
            Debug.Log("PG Instance is null!");
        }
        NavigationUtil.ShowGameMenu();
    }

}
