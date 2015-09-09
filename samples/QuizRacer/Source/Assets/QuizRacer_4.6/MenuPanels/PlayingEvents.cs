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
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayingEvents : MonoBehaviour
{
        // associated sound to play when correct.
        public AudioClip CorrectClip;

        // assocated sound to play when incorrect.
        public AudioClip WrongClip;

        // associated sound to play when displaying a new question.
        public AudioClip QuestionClip;

        // seconds between the questions.
        const float IntervalBetweenQuestions = 2.0f;

        // scoring values.
        const int MaxPointsPerQuestion = 15;
        const int MinPointsPerQuestion = 5;
        const float PointDecreaseSpeed = 1.0f;
        const float WrongAnswerPenaltyFactor = 0.3f;


        string[] Ranks = {
            "Please wait...",
            "You WON!",
            "You got 2nd place",
            "You got 3rd place",
            "You got 4th place"
        };

        // Associated game objects for displaying the question, current point value and answers.
        public Text questionText;
        public Text pointText;
        public Button[] answers;

        // Associated quit button.
        public Button quitButton;

        // question we're currently displaying
        QuizQuestion mCurQuestion = null;
        // current value of the question
        float mQuestionValue = MaxPointsPerQuestion;

        // how long til the next question
        float mNextQuestionCountdown = 0.0f;

        // are we showing the "right/wrong" feedback prompt?
        bool mShowingFeedback = false;

        // ID of last question, this is used to avoid double answering.
        int mLastQuestion = 0;
        bool hasGamepad = false;
        bool done = false;

        // Use this for initialization
        void Start () {
          Debug.Log ("Starting Playing!! Race state is " + RaceManager.Instance.State);
        }

        // Reset the UI to the initial game state.
        private void ResetUI () {

            hasGamepad = DetectGamepad ();
            EnableGamepad (hasGamepad);

            Text txt = quitButton.GetComponentsInChildren<Text> () [0].GetComponent<Text> ();
            txt.text = "Quit";
            foreach (Button b in answers) {
                b.gameObject.SetActive (true);
            }
            done = false;
            questionText.text = "";

            mLastQuestion = 0;

        }

        // Update is called once per frame
        void Update () {

            if (RaceManager.Instance == null) {
                return;
            }

            hasGamepad = DetectGamepad ();
            EnableGamepad (hasGamepad && !done);

            // handle gamepad input here,
            // this avoids the need to navigate to the correct answer.
            if (hasGamepad && !done) {

                if (Input.GetKey (KeyCode.JoystickButton0)) {
                    EventSystem.current.SetSelectedGameObject (answers [0].gameObject);
                    ExecuteEvents.Execute (answers [0].gameObject, null,
                                            ExecuteEvents.submitHandler);
                }
                if (Input.GetKey (KeyCode.JoystickButton1)) {
                    EventSystem.current.SetSelectedGameObject (answers [1].gameObject);
                    ExecuteEvents.Execute (answers [1].gameObject, null,
                                            ExecuteEvents.submitHandler);
                }
                if (Input.GetKey (KeyCode.JoystickButton2)) {
                    EventSystem.current.SetSelectedGameObject (answers [2].gameObject);
                    ExecuteEvents.Execute (answers [2].gameObject, null,
                                            ExecuteEvents.submitHandler);
                }
                if (Input.GetKey (KeyCode.JoystickButton3)) {
                    EventSystem.current.SetSelectedGameObject (answers [3].gameObject);
                    ExecuteEvents.Execute (answers [3].gameObject, null,
                                            ExecuteEvents.submitHandler);
                }
            }

            switch (RaceManager.Instance.State) {

            case RaceManager.RaceState.SetupFailed:
                Debug.Log ("RaceManager.Instance.State = " + RaceManager.Instance.State);
                // call the local version of the method to handle the gamepad state.
                ShowMainMenu ();
            break;
            case RaceManager.RaceState.Aborted:
                Debug.Log ("RaceManager.Instance.State = " + RaceManager.Instance.State);
                // call the local version of the method to handle the gamepad state.
                ShowMainMenu ();
            break;
            case RaceManager.RaceState.Finished:
                ShowResults ();
                Debug.Log ("RaceManager.Instance.State = " + RaceManager.Instance.State);
            break;
            case RaceManager.RaceState.Playing:
                if (done) {
                    ResetUI ();
                }
                HandleGamePlay ();
            break;
            default:
                Debug.Log ("RaceManager.Instance.State = " + RaceManager.Instance.State);
            break;
        }

    }

    // Look for a gamepad in the names of joysticks.
    bool DetectGamepad () {
        string[] names = Input.GetJoystickNames ();
        for (int i=0; i<names.Length; i++) {
            if (names [i].ToLower ().Contains ("gamepad")) {
                return true;
            }
        }
        return false;
    }

    // Display the gamepad images on the panel (or hide them).
    void EnableGamepad (bool flag) {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("gamepad")) {
            Image img = (obj != null) ? obj.GetComponent<Image> () : null;
            if (img != null) {
                img.enabled = flag;
            }
        }
    }

    // check the answer submitted.  Associate this handler with each answer button.
    public void CheckAnswer (GameObject gameobj) {
        //reset the quit confirmation.
        resetQuit ();

        // check the button passed in is in the array of
        // answer buttons.
        Button obj = gameobj.GetComponent<Button> ();

        int idx = -1;
        for (int i =0; i<answers.Length; i++) {
            if (answers [i].Equals (obj)) {
                idx = i;
                break;
            }
        }

        CheckAnswer (idx);
    }

    // Check the answer and take appropriate action.
    void CheckAnswer (int idx) {

        if (mShowingFeedback || mLastQuestion == mCurQuestion.ID) {
            return;
        }

        mLastQuestion = mCurQuestion.ID;
        mShowingFeedback = true;
        if (mCurQuestion.RightAnswerIndex == idx) {
            PlaySfx (CorrectClip);
            mNextQuestionCountdown = IntervalBetweenQuestions;
            RaceManager.Instance.UpdateSelf (0.0f, Mathf.FloorToInt (mQuestionValue));
        } else {
            PlaySfx (WrongClip);
            mNextQuestionCountdown = IntervalBetweenQuestions;
            RaceManager.Instance.UpdateSelf (0.0f, GetWrongAnswerPenalty ());
        }
    }

    //Return the penalty for a wrong answer.
    // it is a percentage of the current value for a correct answer.
    int GetWrongAnswerPenalty () {
        int penalty = -Mathf.RoundToInt (WrongAnswerPenaltyFactor * mQuestionValue);
        return penalty > -1 ? -1 : penalty;
    }

    // Handler for the quit button.  This should be associated with
    // the quit button on the playing panel.
    // To avoid accidental quitting, the button text is updated to
    // prompt for confirmation.
    public void OnQuit () {
        Text txt;
        if (quitButton.GetComponentsInChildren<Text> ().Length > 0) {
            txt = quitButton.GetComponentsInChildren<Text> () [0].GetComponent<Text> ();
            if (txt != null) {
                if (txt.text.EndsWith ("Really?") || done) {
                    if (RaceManager.Instance != null) {
                        RaceManager.Instance.CleanUp ();
                    }

                    // call the local version of the method to handle the gamepad state.
                    ShowMainMenu ();
                } else {
                    txt.text = txt.text + " Really?";
                    txt.color = new Color (1f, .5f, 0, .95f);
                }
            }
        }
    }

    // Removes the confirmation on the quit button.
    void resetQuit () {
        Text txt;
        if (quitButton.GetComponentsInChildren<Text> ().Length > 0) {
            txt = quitButton.GetComponentsInChildren<Text> () [0].GetComponent<Text> ();
            if (txt != null) {
                if (txt.text.EndsWith ("Really?")) {
                    txt.text = txt.text.Substring (0, txt.text.IndexOf (' '));
                    txt.color = Color.white;
                }
            }
        }
    }

    //Handles resetting the gamepad state then navigating to the main menu.
    void ShowMainMenu () {
        EnableGamepad (false);
        NavigationUtil.ShowMainMenu();
    }

    //Shows the final results of the race.
    void ShowResults () {
        if (done) {
            return;
        }
        done = true;
        EnableGamepad (false);
        ShowStatus ("Done! " + Ranks [RaceManager.Instance.FinishRank]);
        Text txt = quitButton.GetComponentsInChildren<Text> () [0].GetComponent<Text> ();
        txt.text = "OK";
        EventSystem.current.SetSelectedGameObject (quitButton.gameObject);
    }

    // Shows the status message.
    void ShowStatus (string message) {
        questionText.text = message;
        pointText.text = "";
        foreach (Button b in answers) {
            b.gameObject.SetActive (false);
        }
    }

    //Handles game play during Update()
    void HandleGamePlay () {
        if (mCurQuestion == null) {
            Debug.Log ("Generating question");
            mCurQuestion = QuizQuestion.GenerateQuestion ();
        }

        RaceManager.Instance.UpdateSelf (Time.deltaTime, 0);
        if (mShowingFeedback) {
            mNextQuestionCountdown -= Time.deltaTime;
            if (mNextQuestionCountdown < 0 || mCurQuestion == null) {
                mCurQuestion = QuizQuestion.GenerateQuestion ();
                mQuestionValue = MaxPointsPerQuestion;
                mShowingFeedback = false;
                PlaySfx (QuestionClip);
            }
        } else {
            ShowQuestion ();
            mQuestionValue -= PointDecreaseSpeed * Time.deltaTime;
            if (mQuestionValue < MinPointsPerQuestion) {
                mQuestionValue = MinPointsPerQuestion;
            }
        }
    }

    void ShowQuestion () {
        questionText.text = mCurQuestion.Question;
        pointText.text = Mathf.FloorToInt (mQuestionValue).ToString () + " pts.";
        for (int i=0; i<answers.Length; i++) {

            if (answers [i].GetComponentsInChildren<Text> ().Length > 0) {
                answers [i].GetComponentsInChildren<Text> () [0].GetComponent<Text> ().text =
                    mCurQuestion.Answers [i];
                answers [i].enabled = true;
            }
        }
    }

    void PlaySfx (AudioClip clip) {
        if (clip != null) {
            AudioSource.PlayClipAtPoint (clip, Vector3.zero);
        }
    }

}
