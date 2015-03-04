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
using GooglePlayGames.BasicApi;

// Script for handling events on the GameMenu panel.  This script
// should be associated with the GameMenu panel in the main scene.
public class GameMenuEvents : MonoBehaviour {

    private  System.Action<bool> mAuthCallback;
    private bool mAuthOnStart = true;
    private bool mSigningIn = false;

    // Use this for initialization
    void Start () {
        mAuthCallback = (bool success) => {

            Debug.Log("In Auth callback, success = " + success);

            mSigningIn = false;
            if (success) {
                NavigationUtil.ShowMainMenu();
            } else {
                Debug.Log("Auth failed!!");
            }
        };

        // enable debug logs (note: we do this because this is a sample;
        // on your production app, you probably don't want this turned 
        // on by default, as it will fill the user's logs with debug info).
        var config = new PlayGamesClientConfiguration.Builder()
            .WithInvitationDelegate(InvitationManager.Instance.OnInvitationReceived)
            .Build();
        PlayGamesPlatform.InitializeInstance(config);        
        PlayGamesPlatform.DebugLogEnabled = true;

        // try silent authentication
        if (mAuthOnStart) {
            Authorize(true);
        }

    }

    // Link this to the play button on click event list.
    // This starts a "non-silent" login process.
    public void OnPlayClicked () {
        Authorize(false);
    }

    //Starts the signin process.
    void Authorize (bool silent)
    {
        if (!mSigningIn) {
            Debug.Log("Starting sign-in...");
            PlayGamesPlatform.Instance.Authenticate(mAuthCallback, silent);
        } else {
          Debug.Log("Already started signing in");
        }
    }
}
