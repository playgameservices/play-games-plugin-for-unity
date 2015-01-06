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
using System.Collections.Generic;

public class LevelController : MonoBehaviour {
    // GUI skin
    public GUISkin GuiSkin;

    // level generation script
    private List<string> mScript = new List<string>();
    private int mScriptIp = 0; // instruction pointer (next instruction to execute)
    private Countdown mCountdownToNextInstruction = new Countdown(false, 0.0f);

    // level generation script entry points and probability weights
    private List<int> mScriptEntryPoints = new List<int>();
    private List<int> mScriptEntryPointProbWeight = new List<int>();

    // spawnable enemy prefabs
    public GameObject[] SpawnablePrefabs;

    // possible BGM selections
    public AudioClip[] BgmChoices;

    // audio to play when player clears level
    public AudioClip LevelClearSfx;

    // dictionary from prefab name to prefab instance, used when spawning enemies
    private Dictionary<string, GameObject> mPrefabDict = new Dictionary<string, GameObject>();

    // is the player dead?
    public enum GameState {
        Playing, GameOver, Won
    }
    private GameState mGameState = GameState.Playing;
    private Countdown mAutoAdvanceCountdown = new Countdown(false, GameConsts.AutoAdvanceTime);

    // if state is GameOver, what is the reason?
    public enum GameOverReason {
        PlayerDied, CiviliansDied
    }
    private GameOverReason mGameOverReason = GameOverReason.PlayerDied;

    // time remaining
    private Countdown mLevelTime = new Countdown(false, GameConsts.LevelDuration);

    // score
    private int mScore = 0;

    // the score that we display (smoothed for a cool animation effect)
    private SmoothValue mDisplayedScore = new SmoothValue(0.0f,
        GameConsts.Hud.ScoreDisplayChangeSpeed);

    // message we're currently showing
    private string mMessage = "";
    private Countdown mMessageCd = new Countdown(GameConsts.Hud.MessageDuration);

    // player accuracy: total shots and shots hit
    private int mShotsTotal = 0;
    private int mShotsHit = 0;

    // combo counter (goes up every time player hits an enemy, goes to 0
    // when player misses)
    private int mCombo = 0;

    // combo cooldown countdown -- when it expires, the combo gets reset to 0
    private Countdown mComboCd = new Countdown(false, GameConsts.ComboCooldown);

    // counts seconds of gameplay for the purposes of unlocking the gameplay achievements
    // It triggers every 5 seconds, and then we know we have to push 5 increments to
    // the gameplay time achievements.
    private Countdown mGameplaySecondCd = new Countdown(false, 5.0f);

    void Start() {
        // load level instructions
        TextAsset ta = (TextAsset) Resources.Load("Level"  + GameManager.Instance.Level);
        if (ta == null) {
            Debug.LogError("Can't find level asset for level " + GameManager.Instance.Level);
            return;
        }
        string[] lines = ta.text.Replace("\r", "").Split(new char[] { '\n' });
        int lineNo = 0;
        foreach (string line in lines) {
            ++lineNo;
            if (line.Trim().Length > 0 && !line.Trim().StartsWith("#")) {
                mScript.Add(line.Trim());
                if (line.StartsWith(":")) {
                    // this is an entry point -- record it
                    mScriptEntryPoints.Add(mScript.Count - 1);
                    mScriptEntryPointProbWeight.Add(System.Convert.ToInt32(line.Substring(1)));
                }
            }
        }

        // prepare the dictionary that maps name --> prefab
        foreach (GameObject prefab in SpawnablePrefabs) {
            mPrefabDict[prefab.name] = prefab;
        }

        // start the level timer
        mLevelTime.Start();

        // start the gameplay reporting timer (for achievement unlocking purposes)
        mGameplaySecondCd.Start();

        // select the music for this level and start playing it
        if (BgmChoices.Length > 0) {
            AudioClip bgm = BgmChoices[GameManager.Instance.Level % BgmChoices.Length];
            audio.clip = bgm;
            audio.Play();
        }
    }

    void Update() {
        mCountdownToNextInstruction.Update(Time.deltaTime, true);
        mAutoAdvanceCountdown.Update(Time.deltaTime, false);
        mLevelTime.Update(Time.deltaTime, false);
        mDisplayedScore.PullTowards(mScore, Time.deltaTime);
        mMessageCd.Update(Time.deltaTime, true);
        mComboCd.Update(Time.deltaTime, true);
        mGameplaySecondCd.Update(Time.deltaTime, false);

        if (!mLevelTime.Expired && !mCountdownToNextInstruction.Active &&
                mScriptIp < mScript.Count) {
            ExecuteNextInstruction();
        }

        if (!mComboCd.Active) {
            mCombo = 0;
        }

        if (mGameplaySecondCd.Expired) {
            IncrementGameplayTimeAchievements((int)mGameplaySecondCd.Initial);
            mGameplaySecondCd.Start();
        }

        CheckAutoAdvance();
        CheckLevelCleared();
    }

    void IncrementGameplayTimeAchievements(int seconds) {
        foreach (string ach in GameIds.Achievements.IncGameplaySeconds) {
            GameManager.Instance.IncrementAchievement(ach, seconds);
        }
    }

    void IncrementGameplayRoundsAchievements() {
        foreach (string ach in GameIds.Achievements.IncGameplayRounds) {
            GameManager.Instance.IncrementAchievement(ach, 1);
        }
    }

    void CheckAutoAdvance() {
        if (!IsPlaying && mAutoAdvanceCountdown.Expired) {
            // increment achievements that count # of rounds played
            IncrementGameplayRoundsAchievements();

            if (mGameState == GameState.GameOver) {
                // game over -- restart current level
                StartCoroutine(CaptureScreenshot());
                GameManager.Instance.RestartLevel();
            } else {
                // level cleared! Record progress and advance to next.
                GameManager.Instance.FinishLevelAndGoToNext(mScore, CalcStars());
            }
        }
    }

    IEnumerator CaptureScreenshot() {
        yield return new WaitForEndOfFrame();
        GameManager.Instance.CaptureScreenshot();
    }

    int CalcStars() {
        int acc = GetAccuracyPercent();
        return acc >= GameConsts.AccuracyThreeStars ? 3 :
            acc >= GameConsts.AccuracyTwoStars ? 2 : 1;
    }

    void CheckLevelCleared() {
        if (IsPlaying && mLevelTime.Expired &&
                GameObject.FindGameObjectsWithTag("Enemy").Length == 0) {
            // level cleared!
            mGameState = GameState.Won;
            mAutoAdvanceCountdown.Start(GameConsts.AutoAdvanceTime);

            // did the player finish with 100% accuracy? If so, unlock achivement
            if (mShotsHit >= mShotsTotal) {
                GameManager.Instance.UnlockAchievement(GameIds.Achievements.PerfectAccuracy);
            }

            audio.Stop();
            AudioSource.PlayClipAtPoint(LevelClearSfx, Vector3.zero);
        }
    }

    void JumpScriptToRandomEntryPoint() {
        // select an entry point randomly, but respecting probability weights
        int totalWeight = 0;
        foreach (int w in mScriptEntryPointProbWeight) {
            totalWeight += w;
        }
        int roll = Random.Range(0, totalWeight);
        int i;
        for (i = 0; i < mScriptEntryPoints.Count; ++i) {
            roll -= mScriptEntryPointProbWeight[i];
            if (roll <= 0 || i >= mScriptEntryPoints.Count - 1) {
                // select this entry point
                break;
            }
        }

        // i is the selected entry point
        mScriptIp = mScriptEntryPoints[i];
        mCountdownToNextInstruction.Stop();
    }

    void ExecuteNextInstruction() {
        if (mScriptIp >= mScript.Count) {
            JumpScriptToRandomEntryPoint();
            return;
        }

        string instr = mScript[mScriptIp++];

        if (instr.StartsWith(":")) {
            // this is a label, so skip it
            return;
        }

        string[] p = instr.Split(new char[] { ' ' }, 2);
        string verb = p.Length > 0 ? p[0].Trim() : "";
        string arg = p.Length > 1 ? p[1].Trim() : "";

        if (verb.Equals("spawn")) {
            ExecuteSpawn(arg);
        } else if (verb.Equals("wait")) {
            ExecuteWait(arg);
        } else if (verb.Equals("end")) {
            JumpScriptToRandomEntryPoint();
        } else if (verb.Equals("dur")) {
            mLevelTime.Start(System.Convert.ToSingle(arg));
        } else if (verb.Equals("msg")) {
            mMessage = arg.Trim();
            mMessageCd.Start();
        } else {
            Debug.LogError("Invalid instruction: " + arg);
        }
    }

    void ExecuteSpawn(string arg) {
        string[] p = arg.Split(new char[] { ' ' }, 2);
        string enemyName = p[0];
        float enemyY = System.Convert.ToSingle(p[1]);
        if (!mPrefabDict.ContainsKey(enemyName)) {
            Debug.LogError("Prefab not found in level prefab dict: " + enemyName);
            return;
        }
        GameObject o = (GameObject) Instantiate(mPrefabDict[enemyName]);
        o.transform.Translate(GameConsts.EnemySpawnX, enemyY, 0.0f, Space.World);
    }

    void ExecuteWait(string arg) {
        mCountdownToNextInstruction.Start(System.Convert.ToSingle(arg));
    }

    public void HandlePlayerDied() {
        if (IsPlaying) {
            mGameOverReason = GameOverReason.PlayerDied;
            mGameState = GameState.GameOver;
            mAutoAdvanceCountdown.Start(GameConsts.AutoRetryTime);
        }
        audio.Stop();
    }

    public void HandleCivilianDestroyed() {
        if (IsPlaying) {
            // civilians killed - end level
            mGameOverReason = GameOverReason.CiviliansDied;
            mGameState = GameState.GameOver;
            mAutoAdvanceCountdown.Start(GameConsts.AutoRetryTime);

            // destroy player
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            p.GetComponent<PlayerController>().KillPlayer();
        }
    }

    void ShowCenteredMessage(string centerMessage, string restartMessage) {
        int rem = (int) Mathf.Ceil(mAutoAdvanceCountdown.Remaining);
        Gu.SetColor(Color.black);
        Gu.Label(Gu.Center(0), Gu.Middle(0), Gu.Dim(GameConsts.GameOverFontSize), centerMessage);
        Gu.Label(Gu.Center(0), Gu.Middle(GameConsts.RetryTextOffset),
            Gu.Dim(GameConsts.RetryTextFontSize), restartMessage + " " + rem);
    }

    void OnGUI() {
        int stars;
        GUI.skin = GuiSkin;

        DrawHud();

        switch (mGameState) {
        case GameState.GameOver:
            ShowCenteredMessage(mGameOverReason == GameOverReason.CiviliansDied ?
                Strings.GameOverCiviliansDied : Strings.GameOverPlayerDied,
                Strings.RetryingIn);
            GameManager.Instance.AutoSave();
            break;
        case GameState.Won:
            // show the "Level Cleared" message
            ShowCenteredMessage(Strings.LevelCleared, Strings.NextLevelIn);

            // show # of stars earned
            stars = CalcStars();
            Gu.Label(Gu.Center(0), Gu.Middle(GameConsts.StarsY),
                Gu.Dim(GameConsts.StarsFontSize), Util.MakeStars(stars));

            // show message based on # of stars
            Gu.SetColor(GameConsts.StarsMessageColor);
            Gu.Label(Gu.Center(0), Gu.Middle(GameConsts.StarsMessageY),
                Gu.Dim(GameConsts.StarsMessageFontSize),
                Strings.StarsMessage[stars]);
            GameManager.Instance.AutoSave();
            break;
        }
    }

    void DrawHud() {
        Gu.SetColor(Color.black);

        // draw score
        Gu.Label(Gu.Right(GameConsts.Hud.ScoreX), Gu.Top(GameConsts.Hud.ScoreY),
            Gu.Dim(GameConsts.Hud.ScoreFontSize), ((int)mDisplayedScore.Value).ToString("D5"));

        // draw % level complete
        int pc = Util.Clamp((int)(mLevelTime.NormalizedElapsed * 100), 0, 100);
        if (mGameState == GameState.Playing && pc == 100) {
            // while playing, we never get to 100% :-D
            pc = 99;
        }

        // draw level # and % complete
        Gu.Label(Gu.Left(GameConsts.Hud.StageX), Gu.Bottom(GameConsts.Hud.StageY),
            Gu.Dim(GameConsts.Hud.StageFontSize),
            string.Format(Strings.StageFmt, Util.GetLevelLetter(GameManager.Instance.Level),
            pc));

        // draw accuracy
        Gu.Label(Gu.Right(GameConsts.Hud.AccuracyX), Gu.Top(GameConsts.Hud.AccuracyY),
            Gu.Dim(GameConsts.Hud.AccuracyFontSize),
            string.Format(Strings.AccuracyFmt, GetAccuracyPercent()));

        // draw combo counter
        if (mCombo > 1 && Util.BlinkFunc(GameConsts.BlinkPeriod, 0.0f)) {
            Gu.SetColor(GameConsts.Hud.ComboColor);
            Gu.Label(Gu.Center(GameConsts.Hud.ComboX), Gu.Top(GameConsts.Hud.ComboY),
                Gu.Dim(GameConsts.Hud.ComboFontSize),
                string.Format(Strings.ComboFmt, GetComboMult()));
        }

        // if there's a message being displayed, draw it now
        if (mMessageCd.Active && mMessage != null) {
            Gu.SetColor(0.0f, 0.0f, 0.0f,
                Util.Trapezoid(GameConsts.Hud.MessageDuration,
                    GameConsts.Hud.MessageTransitionDuration, mMessageCd.Elapsed));
            Gu.Label(Gu.Center(0), Gu.Middle(0),
                Gu.Dim(GameConsts.Hud.MessageFontSize), mMessage);
        }
    }

    private float GetComboMult() {
        return GameConsts.ComboMult[Util.Clamp(mCombo, 0, GameConsts.ComboMult.Length - 1)];
    }

    private int GetAccuracyPercent() {
        return mShotsTotal > 0 ? Util.Clamp(mShotsHit * 100 / mShotsTotal, 0, 100) : 100;
    }

    public int ReportKill(int score, float enemyX) {
        // increase combo counter
        mCombo++;
        mComboCd.Start();

        // calculate actual value of kill and add it to the score
        int actualValue = (int)(score * GetComboMult());
        mScore += actualValue;

        // add experience point to player
        if (GameManager.Instance.Progress.AddPilotExperience(1)) {
            // level up! Inform player of this fact.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<PlayerController>().HandleLevelUp();
        }

        // unlock any achievements as appropriate
        UnlockKillRelatedAchievements(enemyX);

        return actualValue;
    }

    private void UnlockKillRelatedAchievements(float enemyX) {
        // unlock the "not a disaster" achievement, if not unlocked yet
        GameManager.Instance.UnlockAchievement(GameIds.Achievements.NotADisaster);

        // unlock the "close range kill" achievement, if applicable
        if (enemyX < GameConsts.EnemyCloseRangeXThresh) {
            GameManager.Instance.UnlockAchievement(GameIds.Achievements.PointBlank);
        }

        // unlock the "full combo" achievement if the combo maxed out
        if (mCombo >= GameConsts.ComboMult.Length - 1) {
            GameManager.Instance.UnlockAchievement(GameIds.Achievements.FullCombo);
        }
    }

    public bool IsPlaying {
        get {
            return mGameState == GameState.Playing;
        }
    }

    public void HandleLaserMissed() {
        mShotsTotal++;
        mCombo = 0;
        mComboCd.Stop();
    }

    public void HandleLaserHit() {
        mShotsTotal++;
        mShotsHit++;
    }

    // primarily for debug purposes:
    public void CutTime(float seconds) {
        mLevelTime.Update(seconds);
    }
}
