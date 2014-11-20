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
using GooglePlayGames.BasicApi.Multiplayer;
using System.Collections.Generic;

public class PlayGui : BaseGui {
    public GameObject Playfield;

    private const string BlocksRootName = "BlocksRoot";
    private const string XBlockPrefabName = "XBlock";
    private const string OBlockPrefabName = "OBlock";
    private const string LargeMarkPrefix = "LargeMark";  // + "X00", "X01", "O00", etc

    WidgetConfig OkButtonCfg = new WidgetConfig(0.0f, 0.4f, 0.4f, 0.2f, 60, "OK");

    private TurnBasedMatch mMatch = null;
    private MatchData mMatchData = null;
    private string mFinalMessage = null;
    private char mMyMark = '\0';

    // has a block been shot and is in flight?
    private bool mBlockInFlight = false;

    // the countdown to check if all blocks are at rest
    private float mRestCheckCountdown = 0.0f;
    private const float RestCheckInterval = 1.0f;

    // maximum time to wait for all blocks to come to rest
    private const float MaxTurnTime = 8.0f;
    private float mEndTurnCountdown = MaxTurnTime;

    // tile size (for calculating which large mark to attribute to each box)
    private const float TileSize = 1.3f;

    private bool mEndingTurn = false;

    // mass of the player's block (it's more massive than other blocks to
    // make for more interesting gameplay)
    private const float PlayerBlockMass = 10.0f;

    // countdown to hide instructions
    private bool mShowInstructions = false;

    private void Reset() {
        mMatch = null;
        mMatchData = null;
        mFinalMessage = null;
        mMyMark = '\0';
        mEndTurnCountdown = MaxTurnTime;
        mBlockInFlight = false;
        mEndingTurn = false;
        mShowInstructions = false;
        Util.MakeVisible(Playfield, false);
        UpdateLargeMarks();

        foreach (GameObject o in GetAllGameBlocks()) {
            GameObject.Destroy(o);
        }
    }

    public GameObject GetBlocksRoot() {
        return GameObject.Find(BlocksRootName);
    }

    public void LaunchMatch(TurnBasedMatch match) {
        Reset();
        mMatch = match;
        MakeActive();

        if (mMatch == null) {
            throw new System.Exception("PlayGui can't be started without a match!");
        }
        try {
            // Note that mMatch.Data might be null (when we are starting a new match).
            // MatchData.MatchData() correctly deals with that and initializes a
            // brand-new match in that case.
            mMatchData = new MatchData(mMatch.Data);
        } catch (MatchData.UnsupportedMatchFormatException ex) {
            mFinalMessage = "Your game is out of date. Please update your game\n" +
                "in order to play this match.";
            Debug.LogWarning("Failed to parse board data: " + ex.Message);
            return;
        }

        // determine if I'm the 'X' or the 'O' player
        mMyMark = mMatchData.GetMyMark(match.SelfParticipantId);

        bool canPlay = (mMatch.Status == TurnBasedMatch.MatchStatus.Active &&
                mMatch.TurnStatus == TurnBasedMatch.MatchTurnStatus.MyTurn);

        if (canPlay) {
            mShowInstructions = true;
        } else {
            mFinalMessage = ExplainWhyICantPlay();
        }

        // if the match is in the completed state, acknowledge it
        if (mMatch.Status == TurnBasedMatch.MatchStatus.Complete) {
            PlayGamesPlatform.Instance.TurnBased.AcknowledgeFinished(mMatch,
                    (bool success) => {
                if (!success) {
                    Debug.LogError("Error acknowledging match finish.");
                }
            });
        }

        // set up the objects to show the match to the player
        SetupObjects(canPlay);
    }

    private string ExplainWhyICantPlay() {
        switch (mMatch.Status) {
            case TurnBasedMatch.MatchStatus.Active:
                break;
            case TurnBasedMatch.MatchStatus.Complete:
                return mMatchData.Winner == mMyMark ? "Match finished. YOU WIN!" :
                        "Match finished. YOU LOST!";
            case TurnBasedMatch.MatchStatus.Cancelled:
            case TurnBasedMatch.MatchStatus.Expired:
                return "This match was cancelled.";
            case TurnBasedMatch.MatchStatus.AutoMatching:
                return "This match is awaiting players.";
            default:
                return "This match can't continue due to an error.";
        }

        if (mMatch.TurnStatus != TurnBasedMatch.MatchTurnStatus.MyTurn) {
            return "It's not your turn yet!";
        }

        return "Error";
    }

    protected override void DoGUI() {
        if (mFinalMessage != null) {
            GuiLabel(CenterLabelCfg, mFinalMessage);
            if (GuiButton(OkButtonCfg)) {
                Reset();
                gameObject.GetComponent<MainMenuGui>().MakeActive();
            }
            return;
        }

        if (mShowInstructions) {
            GuiLabel(CenterLabelCfg, "Pull box back and release to shoot!\n(like a slingshot)");
        }
    }

    private static GameObject Spawn(GameObject parent, string prefabName) {
        GameObject o = (GameObject) GameObject.Instantiate(Resources.Load(prefabName));
        if (parent != null) {
            o.transform.parent = parent.transform;
        }
        return o;
    }

    private static GameObject Spawn(GameObject parent, string prefabName, Vector3 position, Quaternion rotation) {
        GameObject o = (GameObject) GameObject.Instantiate(Resources.Load(prefabName), position, rotation);
        if (parent != null) {
            o.transform.parent = parent.transform;
        }
        return o;
    }

    private void SetupObjects(bool canPlay) {
        // show the play field
        Util.MakeVisible(Playfield, true);

        // create the blocks
        foreach (MatchData.BlockDesc desc in mMatchData.BlockDescs) {
            Spawn(GetBlocksRoot(), desc.mark == MatchData.MarkX ?
                XBlockPrefabName : OBlockPrefabName, desc.position, desc.rotation);
        }

        // create the block the player is shooting, if applicable
        if (canPlay) {
            // the block prefabs get instantiated in the right
            // position for this, so we don't need to translate
            GameObject o = Spawn(GetBlocksRoot(), mMyMark == MatchData.MarkX ?
                    XBlockPrefabName : OBlockPrefabName);
            o.AddComponent<AimController>().SetFireDelegate(OnBlockFired);
            o.AddComponent<CollisionSfx>();
            o.rigidbody.mass = PlayerBlockMass;
        }

        RecalculateMarks();
        UpdateLargeMarks();
    }

    // notifies us that a block was fired (and is currently in flight)
    void OnBlockFired() {
        mBlockInFlight = true;
        mShowInstructions = false;
    }

    void Update() {
        if (mEndingTurn) {
            return;
        }

        mRestCheckCountdown -= Time.deltaTime;

        if (mBlockInFlight) {
            RecalculateMarks();
            UpdateLargeMarks();
            mEndTurnCountdown -= Time.deltaTime;

            // are all the blocks at rest?
            if (mRestCheckCountdown < 0) {
                if (AllBlocksAtRest() || mEndTurnCountdown < 0) {
                    // all blocks are at rest -- we are ready to take the turn
                    EndTurn();
                } else {
                    // check again momentarily
                    mRestCheckCountdown = RestCheckInterval;
                }
            }
        }
    }

    private List<GameObject> GetAllGameBlocks() {
        List<GameObject> blocks = new List<GameObject>();
        GameObject root = GetBlocksRoot();
        for (int i = 0; i < root.transform.childCount; ++i) {
            blocks.Add(root.transform.GetChild(i).gameObject);
        }
        return blocks;
    }

    bool AllBlocksAtRest() {
        foreach (GameObject o in GetAllGameBlocks()) {
            if (!o.GetComponent<MotionDetect>().IsAtRest) {
                return false;
            }
        }
        return true;
    }

    string DecideNextToPlay() {
        if (mMatch.AvailableAutomatchSlots > 0) {
            // hand over to an automatch player
            return null;
        } else {
            // hand over to our (only) opponent
            Participant opponent = Util.GetOpponent(mMatch);
            return opponent == null ? null : opponent.ParticipantId;
        }
    }

    void EndTurn() {
        mEndingTurn = true;

        // save current state of the blocks into our match data
        mMatchData.ClearBlockDescs();
        foreach (GameObject o in GetAllGameBlocks()) {
            char mark = o.tag == "MarkX" ? MatchData.MarkX : MatchData.MarkO;
            mMatchData.AddBlockDesc(mark, o.transform.position, o.transform.rotation);
        }

        // recalculate who owns each mark
        RecalculateMarks();
        UpdateLargeMarks();

        // do we have a winner?
        if (mMatchData.HasWinner) {
            FinishMatch();
        } else {
            TakeTurn();
        }
    }

    string GetAdversaryParticipantId() {
        foreach (Participant p in mMatch.Participants) {
            if (!p.ParticipantId.Equals(mMatch.SelfParticipantId)) {
                return p.ParticipantId;
            }
        }
        Debug.LogError("Match has no adversary (bug)");
        return null;
    }

    void FinishMatch() {
        bool winnerIsMe = mMatchData.Winner == mMyMark;

        // define the match's outcome
        MatchOutcome outcome = new MatchOutcome();
        outcome.SetParticipantResult(mMatch.SelfParticipantId,
            winnerIsMe ? MatchOutcome.ParticipantResult.Win : MatchOutcome.ParticipantResult.Loss);
        outcome.SetParticipantResult(GetAdversaryParticipantId(),
            winnerIsMe ? MatchOutcome.ParticipantResult.Loss : MatchOutcome.ParticipantResult.Win);

        // finish the match
        SetStandBy("Sending...");
        PlayGamesPlatform.Instance.TurnBased.Finish(mMatch, mMatchData.ToBytes(),
                    outcome, (bool success) => {
            EndStandBy();
            mFinalMessage = success ? (winnerIsMe ? "YOU WON!" : "YOU LOST!") :
                "ERROR finishing match.";
        });
    }

    void TakeTurn() {
        SetStandBy("Sending...");
        PlayGamesPlatform.Instance.TurnBased.TakeTurn(mMatch, mMatchData.ToBytes(),
                    DecideNextToPlay(), (bool success) => {
            EndStandBy();
            mFinalMessage = success ? "Done for now!" : "ERROR sending turn.";
        });
    }

    void UpdateLargeMarks() {
        UpdateLargeMarks(MatchData.MarkX, "X");
        UpdateLargeMarks(MatchData.MarkO, "O");
        UpdateLargeMarks(MatchData.MarkNone, "N");
        UpdateLargeMarks(MatchData.MarkConflict, "C");
    }

    void UpdateLargeMarks(char mark, string markName) {
        int x, y;
        for (x = 0; x < MatchData.BoardSize; x++) {
            for (y = 0; y < MatchData.BoardSize; y++) {
                string objName = LargeMarkPrefix + markName + x + y;
                GameObject.Find(objName).renderer.enabled = (mMatchData != null) ?
                        (mMatchData.GetMark(x, y) == mark) : false;
            }
        }
    }

    int[][] mMarkCount;
    bool[][] mMarkUsed;
    void RecalculateMarks() {
        int x, y;
        mMatchData.ResetMarks();

        if (mMarkCount == null) {
            mMarkCount = new int[MatchData.BoardSize][];
            mMarkUsed = new bool[MatchData.BoardSize][];
            for (x = 0; x < MatchData.BoardSize; x++) {
                mMarkCount[x] = new int[MatchData.BoardSize];
                mMarkUsed[x] = new bool[MatchData.BoardSize];
            }
        }

        for (x = 0; x < MatchData.BoardSize; x++) {
            for (y = 0; y < MatchData.BoardSize; y++) {
                mMarkCount[x][y] = 0;
                mMarkUsed[x][y] = false;
            }
        }

        foreach (GameObject obj in GetAllGameBlocks()) {
            int col = Mathf.FloorToInt((obj.transform.position.x + 1.5f * TileSize) / TileSize);
            int row = Mathf.FloorToInt((-obj.transform.position.z + 1.5f * TileSize) / TileSize);
            if (col >= 0 && col < MatchData.BoardSize && row >= 0 && row < MatchData.BoardSize) {
                mMarkCount[col][row] += (obj.tag == "MarkX") ? 1 : -1;
                mMarkUsed[col][row] = true;
            }
        }

        for (x = 0; x < MatchData.BoardSize; x++) {
            for (y = 0; y < MatchData.BoardSize; y++) {
                mMatchData.SetMark(x, y, mMarkCount[x][y] > 0 ? MatchData.MarkX :
                    mMarkCount[x][y] < 0 ? MatchData.MarkO :
                    mMarkUsed[x][y] ? MatchData.MarkConflict : MatchData.MarkNone);
            }
        }
    }
}
