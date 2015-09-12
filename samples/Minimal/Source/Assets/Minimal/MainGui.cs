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
using UnityEngine.SocialPlatforms;

public class MainGui : MonoBehaviour {
    private const float FontSizeMult = 0.05f;
    private bool mWaitingForAuth = false;
    private string mStatusText = "Ready.";

    void Start () {
        // Select the Google Play Games platform as our social platform implementation
        GooglePlayGames.PlayGamesPlatform.Activate();
    }

    void OnGUI() {
        GUI.skin.button.fontSize = (int)(FontSizeMult * Screen.height);
        GUI.skin.label.fontSize = (int)(FontSizeMult * Screen.height);

        GUI.Label(new Rect(20, 20, Screen.width, Screen.height * 0.25f),
                  mStatusText);

        Rect buttonRect = new Rect(0.25f * Screen.width, 0.10f * Screen.height,
                          0.5f * Screen.width, 0.25f * Screen.height);
        Rect imageRect = new Rect(buttonRect.x+buttonRect.width/4f,
                                  buttonRect.y + buttonRect.height * 1.1f,
                                  buttonRect.width/2f, buttonRect.width/2f);

        if (mWaitingForAuth) {
            return;
        }

        string buttonLabel;


        if (Social.localUser.authenticated) {
          buttonLabel = "Sign Out";
          if (Social.localUser.image != null) {
            GUI.DrawTexture(imageRect, Social.localUser.image,
                            ScaleMode.ScaleToFit);
          } else {
            GUI.Label(imageRect, "No image available");
          }

          mStatusText = "Ready";
        } else {
          buttonLabel = "Authenticate";
        }

        if (GUI.Button(buttonRect, buttonLabel)) {
            if (!Social.localUser.authenticated) {
                // Authenticate
                mWaitingForAuth = true;
                mStatusText = "Authenticating...";
                Social.localUser.Authenticate((bool success) => {
                    mWaitingForAuth = false;
                    if (success) {
                      mStatusText = "Welcome " + Social.localUser.userName;
                      string token = GooglePlayGames.PlayGamesPlatform.Instance.GetToken();
                      Debug.Log(token);
                    } else {
                      mStatusText = "Authentication failed.";
                    }
                });
            } else {
                // Sign out!
                mStatusText = "Signing out.";
                ((GooglePlayGames.PlayGamesPlatform) Social.Active).SignOut();
            }
        }
    }
}
