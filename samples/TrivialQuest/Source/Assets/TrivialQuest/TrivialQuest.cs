// <copyright file="TrivialQuest.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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

namespace TrivialQuest
{
    using UnityEngine;
    using UnityEngine.UI;
    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Quests;

    public class TrivialQuest : MonoBehaviour
    {
        private GameObject[] signedInObjects;
        private GameObject[] signedOutObjects;
        private Text statusText;

        void Start()
        {
            // Lock the screen to landscape
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            // Buttons
            signedInObjects = GameObject.FindGameObjectsWithTag("SignedIn");
            signedOutObjects = GameObject.FindGameObjectsWithTag("SignedOut");
            statusText = GameObject.Find("statusText").GetComponent<Text>();

            // Google Play Games
            PlayGamesClientConfiguration config = 
                new PlayGamesClientConfiguration.Builder().Build();
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();

            // Try silent sign-in
            UpdateButtonVisibility(false);
            PlayGamesPlatform.Instance.Authenticate(SignInCallback, true);
        }

        private void UpdateButtonVisibility(bool signedIn)
        {
            Debug.Log("UpdateButtonVisibility:signedIn:" + signedIn);

            // GameObjects tagged as 'SignedIn' should be shown only when we are signed in
            foreach (GameObject go in signedInObjects)
            {
                go.SetActive(signedIn);
            }

            // GameObjects tagged as 'SignedOut' should be shown only when we are signed out
            foreach (GameObject go in signedOutObjects)
            {
                go.SetActive(!signedIn);
            }
        }

        private void SignInCallback(bool success)
        {
            if (success)
            {
                statusText.text = "Signed In: " +
                    PlayGamesPlatform.Instance.localUser.userName;
            }
            else
            {
                statusText.text = "Sign-in failed.";
            }
            UpdateButtonVisibility(success);
        }

        public void SignIn()
        {
            Debug.Log("clicked:SignIn");
            PlayGamesPlatform.Instance.Authenticate(SignInCallback);
        }

        public void SignOut()
        {
            Debug.Log("clicked:SignOut");
            PlayGamesPlatform.Instance.SignOut();

            statusText.text = "Signed Out";
            UpdateButtonVisibility(false);
        }

        public void ViewQuests()
        {
            Debug.Log("clicked:ViewQuests");
            PlayGamesPlatform.Instance.Quests.ShowAllQuestsUI(
                (QuestUiResult result, IQuest quest, IQuestMilestone milestone) =>
                {
                    if (result == QuestUiResult.UserRequestsQuestAcceptance)
                    {
                        Debug.Log("User Requests Quest Acceptance");
                        AcceptQuest(quest);
                    }

                    if (result == QuestUiResult.UserRequestsMilestoneClaiming)
                    {
                        Debug.Log("User Requests Milestone Claim");
                        ClaimMilestone(milestone);
                    }
                });
        }

        private void AcceptQuest(IQuest toAccept)
        {
            Debug.Log("Accepting Quest: " + toAccept);
            PlayGamesPlatform.Instance.Quests.Accept(toAccept,
                (QuestAcceptStatus status, IQuest quest) =>
                {
                    if (status == QuestAcceptStatus.Success)
                    {
                        statusText.text = "Quest Accepted: " + quest.Name;
                    }
                    else
                    {
                        statusText.text = "Quest Accept Failed: " + status;
                    }
                });
        }

        private void ClaimMilestone(IQuestMilestone toClaim)
        {
            Debug.Log("Claiming Milestone: " + toClaim);
            PlayGamesPlatform.Instance.Quests.ClaimMilestone(toClaim,
                (QuestClaimMilestoneStatus status, IQuest quest, IQuestMilestone milestone) =>
                {
                    if (status == QuestClaimMilestoneStatus.Success)
                    {
                        statusText.text = "Milestone Claimed";
                    }
                    else
                    {
                        statusText.text = "Milestone Claim Failed: " + status;
                    }
                });
        }

        public void AttackRed()
        {
            Debug.Log("clicked:AttackRed");
            PlayGamesPlatform.Instance.Events.IncrementEvent(GPGSIds.event_red, 1);
        }

        public void AttackYellow()
        {
            Debug.Log("clicked:AttackYellow");
            PlayGamesPlatform.Instance.Events.IncrementEvent(GPGSIds.event_yellow, 1);
        }

        public void AttackBlue()
        {
            Debug.Log("clicked:AttackBlue");
            PlayGamesPlatform.Instance.Events.IncrementEvent(GPGSIds.event_blue, 1);
        }

        public void AttackGreen()
        {
            Debug.Log("clicked:AttackGreen");
            PlayGamesPlatform.Instance.Events.IncrementEvent(GPGSIds.event_green, 1);
        }
    }
}
