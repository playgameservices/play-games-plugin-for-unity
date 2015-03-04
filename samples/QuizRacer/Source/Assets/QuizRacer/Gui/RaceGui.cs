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
using System.Collections.Generic;

public class RaceGui : BaseGui {
    public AudioClip CorrectClip;
    public AudioClip WrongClip;
    public AudioClip QuestionClip;
    public Texture ProgressBarBgTexture;
    public Texture ProgressBarFgTexture;

    const float IntervalBetweenQuestions = 2.0f;

    const int MaxPointsPerQuestion = 15;
    const int MinPointsPerQuestion = 5;
    const float PointDecreaseSpeed = 1.0f;

    const float WrongAnswerPenaltyFactor = 0.3f;

    WidgetConfig OkButtonCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Bottom, 0.0f, -0.25f, 0.5f, 0.2f,
            TextAnchor.MiddleCenter, 60, "OK");

    WidgetConfig QuestionCfg = new WidgetConfig(0.0f, 0.0f, 0.8f, 0.3f, 50, "Question");
    WidgetConfig PointsCfg = new WidgetConfig(0.0f, -0.05f, 0.8f, 0.3f, 30, "Value: X points");
    WidgetConfig ProgressBarCfg = new WidgetConfig(0.0f, 0.15f, 0.7f, 0.05f, 10, "");

    WidgetConfig[] AnswerCfg = {
        new WidgetConfig(-0.25f, 0.2f, 0.4f, 0.2f, 60, "Choice A"),
        new WidgetConfig(0.25f, 0.2f, 0.4f, 0.2f, 60, "Choice B"),
        new WidgetConfig(-0.25f, 0.5f, 0.4f, 0.2f, 60, "Choice C"),
        new WidgetConfig(0.25f, 0.5f, 0.4f, 0.2f, 60, "Choice D"),
    };

    // question we're currently displaying
    QuizQuestion mCurQuestion = null;

    // current value of the question
    float mQuestionValue = MaxPointsPerQuestion;

    // are we showing the "right/wrong" feedback prompt?
    bool mShowingFeedback = false;
    bool mWasRightAnswer = false;

    // how long til the next questino
    float mNextQuestionCountdown = 0.0f;

    void Start() {
        mCurQuestion = QuizQuestion.GenerateQuestion();
    }

    protected override void DoGUI() {
        if (RaceManager.Instance == null) {
            return;
        }

        switch (RaceManager.Instance.State) {
            case RaceManager.RaceState.SettingUp:
                ShowSettingUp();
                break;
            case RaceManager.RaceState.SetupFailed:
                ShowError("Race setup failed.");
                break;
            case RaceManager.RaceState.Aborted:
                ShowError("Race aborted.");
                break;
            case RaceManager.RaceState.Finished:
                ShowResult();
                break;
            case RaceManager.RaceState.Playing:
                if (mShowingFeedback) {
                    ShowFeedback();
                } else {
                    ShowQuestion();
                }
                break;
            default:
                break;
        }
    }

    void ShowAbortButton(string label) {
        if (GuiButton(UpButtonCfg, label)) {
            RaceManager.Instance.CleanUp();
            gameObject.GetComponent<MainMenuGui>().MakeActive();
        }
    }

    void ShowSettingUp() {
        GuiLabel(CenterLabelCfg, "Waiting for opponents...");

        GuiProgressBar(ProgressBarCfg, ProgressBarFgTexture, ProgressBarBgTexture,
            RaceManager.Instance.RoomSetupProgress);

        ShowAbortButton("<< Abort");
    }

    void ShowError(string error) {
        EndStandBy();
        GuiLabel(CenterLabelCfg, error);
        if (GuiButton(OkButtonCfg)) {
            RaceManager.Instance.CleanUp();
            gameObject.GetComponent<MainMenuGui>().MakeActive();
        }
    }

    void PlaySfx(AudioClip clip) {
        if (clip != null) {
            AudioSource.PlayClipAtPoint(clip, Vector3.zero);
        }
    }

    void Update() {
        if (RaceManager.Instance != null && RaceManager.Instance.State == RaceManager.RaceState.Playing) {
            RaceManager.Instance.UpdateSelf(Time.deltaTime, 0);

            if (mShowingFeedback) {
                mNextQuestionCountdown -= Time.deltaTime;
                if (mNextQuestionCountdown < 0 || mCurQuestion == null) {
                    mCurQuestion = QuizQuestion.GenerateQuestion();
                    mQuestionValue = MaxPointsPerQuestion;
                    mShowingFeedback = false;
                    PlaySfx(QuestionClip);
                }
            } else {
                mQuestionValue -= PointDecreaseSpeed * Time.deltaTime;
                if (mQuestionValue < MinPointsPerQuestion) {
                    mQuestionValue = MinPointsPerQuestion;
                }
            }
        }
    }

    void ShowQuestion() {
        GuiLabel(QuestionCfg, mCurQuestion.Question);
        GuiLabel(PointsCfg, "Value: " + Mathf.FloorToInt(mQuestionValue).ToString() + " points");
        for (int i = 0; i < AnswerCfg.Length; i++) {
            if (GuiButton(AnswerCfg[i], mCurQuestion.Answers[i])) {
                mWasRightAnswer = i == mCurQuestion.RightAnswerIndex;
                PlaySfx(mWasRightAnswer ? CorrectClip : WrongClip);
                mShowingFeedback = true;
                mNextQuestionCountdown = IntervalBetweenQuestions;
                RaceManager.Instance.UpdateSelf(0.0f, mWasRightAnswer ?
                        Mathf.FloorToInt(mQuestionValue) : GetWrongAnswerPenalty());
            }
        }
        ShowAbortButton("<< Leave");
    }

    string[] Ranks = { "Please wait...", "You WON!", "You got 2nd place", "You got 3rd place", "You got 4th place" };
    void ShowResult() {
        int rank = RaceManager.Instance.FinishRank;
        GuiLabel(CenterLabelCfg, Ranks[rank]);
        if (rank > 0 && GuiButton(OkButtonCfg)) {
            RaceManager.Instance.CleanUp();
            gameObject.GetComponent<MainMenuGui>().MakeActive();
        }
    }

    int GetWrongAnswerPenalty() {
        int penalty = -Mathf.RoundToInt(WrongAnswerPenaltyFactor * mQuestionValue);
        return penalty > -1 ? -1 : penalty;
    }

    void ShowFeedback() {
        GuiLabel(QuestionCfg, mWasRightAnswer ? "Right answer!" : "Wrong! It was " + mCurQuestion.RightAnswer);
        ShowAbortButton("<< Leave");
    }
}
