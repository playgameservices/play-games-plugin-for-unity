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

namespace CubicPilot.Gui
{
    using CubicPilot.GameLogic;
    using UnityEngine;
    using UnityEngine.UI;
    using System;

    public class MainMenuEvents : MonoBehaviour
    {

        public AudioClip UiBeepFx;
        public GameObject playButton;
        public GameObject signinButton;
        public GameObject signinMessage;
        public GameObject achievementButton;
        public GameObject highScoreButton;
        public GameObject loadButton;
        public GameObject saveButton;
        public GameObject levelSelectionPanel;


        static bool sAutoAuthenticate = true;

        // Use this for initialization
        void Start()
        {

            // if this is the first time we're running, bring up the sign in flow
            if (sAutoAuthenticate)
            {
                GameManager.Instance.Authenticate();
                sAutoAuthenticate = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            bool standBy = GameManager.Instance.Authenticating;
            bool authenticated = GameManager.Instance.Authenticated;

            Text msg = signinMessage.GetComponent<Text>();
            Button signin = signinButton.GetComponent<Button>();

            if (standBy)
            {
                msg.text = Strings.SigningIn;
                signin.interactable = false;
            }
            else if (authenticated)
            {
                msg.text = Strings.SignedInBlurb;
                signin.interactable = !levelSelectionPanel.activeSelf;
                signinButton.GetComponentInChildren<Text>().text = Strings.SignOut;
            }
            else
            {
                msg.text = Strings.SignInBlurb;
                signin.interactable = !levelSelectionPanel.activeSelf;
                signinButton.GetComponentInChildren<Text>().text = Strings.SignIn;

            }
            achievementButton.SetActive(authenticated);
            highScoreButton.SetActive(authenticated);
            loadButton.SetActive(authenticated);
            saveButton.SetActive(authenticated);
        }

        public void OnSignIn()
        {
            if (GameManager.Instance.Authenticating)
            {
                return;
            }
            Beep();

            if (GameManager.Instance.Authenticated)
            {
                Beep();
                GameManager.Instance.SignOut();
            }
            else
            {
                GameManager.Instance.Authenticate();
            }
        }

        public void OnPlay()
        {
            Beep();

            if (!GameManager.Instance.Progress.IsLevelUnlocked(1))
            {
                // If level 0 is the only possibility, don't bother to show the
                // level selection screen, just go straight into the level.
                GameManager.Instance.GoToLevel(0);
            }
            else
            {
                // Show the level selection screen
                // gameObject.
                // this.enabled = false;
                ShowLevelSelection();
            }
        }

        public void OnLoadProgress()
        {
            Beep();
            GameManager.Instance.LoadFromCloud();
        }

        public void OnSaveProgress()
        {
            Beep();
            GameManager.Instance.SaveProgress();
        }

        void ShowLevelSelection()
        {
            levelSelectionPanel.SetActive(true);

            playButton.GetComponent<Button>().interactable = false;
            achievementButton.GetComponent<Button>().interactable = false;
            highScoreButton.GetComponent<Button>().interactable = false;
            signinButton.GetComponent<Button>().interactable = false;
            loadButton.GetComponent<Button>().interactable = false;


            Button[] levels = levelSelectionPanel.GetComponentsInChildren<Button>();
            Text[] texts = levelSelectionPanel.GetComponentsInChildren<Text>();
            for (int i = 0; i < levels.Length; i++)
            {
                // create new local var for closure for click listener
                int level = i;
                texts[i].text = "Sector " + Convert.ToChar('A' + i) +
                "\n" + GameManager.Instance.Progress.GetLevelProgress(i).Score;
                levels[i].interactable =
                GameManager.Instance.Progress.IsLevelUnlocked(i);
                levels[i].onClick.AddListener(() =>
                    {
                        Debug.Log("Playing level " + level);
                        GameManager.Instance.GoToLevel(level);
                    });
            }
        }

        public void OnAchievements()
        {
            Beep();
            GameManager.Instance.ShowAchievementsUI();
        }

        public void OnHighScores()
        {
            Beep();
            GameManager.Instance.ShowLeaderboardUI();
        }

        void Beep()
        {
            AudioSource.PlayClipAtPoint(UiBeepFx, Vector3.zero);
        }
    }
}
