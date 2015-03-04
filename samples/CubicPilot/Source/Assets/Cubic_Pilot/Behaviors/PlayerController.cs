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

public class PlayerController : MonoBehaviour {
    public GameObject LevelController;
    public GameObject[] LaserPrefab;
    public AudioClip LaserSfx;
    public AudioClip KaboomSfx;
    public GameObject KaboomEffect;
    public GameObject[] AimObjs;
    public GameObject[] ShieldObjs;
    public GameObject ShieldBreakEffect;
    public AudioClip HurtSfx;

    private LevelController mLevelController;

    // shield points left
    private int mShieldLeft = 0;

    // current pilot stats
    PilotStats mStats;

    // player motion:
    float mTargetY = 0.0f;
    SmoothValue mPlayerY = new SmoothValue(0.0f, GameConsts.MaxPlayerYSpeed,
        GameConsts.PlayerMinY, GameConsts.PlayerMaxY, 1);

    // fire cooldown countdown
    Countdown mFireCountdown = new Countdown(false);

    // last touch Y that we accounted for when steering ship
    float mLastTouchY = 0.0f;
    int mFingerId = -1; // the ID of the finger that's steering

    void Start() {
        mLevelController = LevelController.GetComponent<LevelController>();
        mStats = GameManager.Instance.Progress.CurPilotStats;
        mShieldLeft = mStats.ShieldPoints;
        mFireCountdown = new Countdown(false, mStats.FireCooldown);
        UpdateAim();
        UpdateShield();
        UnlockRankBasedAchievements();
    }

    void UpdateAim() {
        int d = mStats.Damage;
        GameObject aim = AimObjs[Util.Clamp(d - 1, 0, AimObjs.Length - 1)];
        foreach (GameObject o in AimObjs) {
            Util.ShowObject(o, !mFireCountdown.Active && o == aim);
        }
    }

    void UpdateShield() {
        int i = 0;
        for (i = 0; i < ShieldObjs.Length; ++i) {
            Util.ShowObject(ShieldObjs[i], mShieldLeft > i);
        }
    }

    void Update () {
        if (mLevelController.IsPlaying) {
            MovePlayer();
            UpdateAim();
            CheckFireShot();
        }
    }

    void ProcessTouchSteer(Touch[] touches) {
        foreach (Touch t in Input.touches) {
            if (t.position.x < Screen.width / 2) {
                if (mFingerId < 0) {
                    mLastTouchY = t.position.y;
                    mFingerId = t.fingerId;
                } else if (mFingerId == t.fingerId && t.phase == TouchPhase.Moved) {
                    float delta = t.position.y - mLastTouchY;
                    mLastTouchY = t.position.y;
                    mTargetY += (delta / (float)Screen.height) *
                            (GameConsts.PlayerMaxY - GameConsts.PlayerMinY) *
                            GameConsts.TouchSensivity;
                } else if (mFingerId == t.fingerId && t.phase == TouchPhase.Ended) {
                    mFingerId = -1;
                }
            }
        }
    }

    void MovePlayer() {
        if (Input.touchCount > 0) {
            ProcessTouchSteer(Input.touches);
        } else {
            mTargetY = mTargetY + GameConsts.MaxPlayerYSpeed *
                Input.GetAxis("Vertical") * Time.deltaTime;
        }
        mTargetY = Util.Clamp(mTargetY, GameConsts.PlayerMinY, GameConsts.PlayerMaxY);
        mPlayerY.PullTowards(mTargetY, Time.deltaTime);
        float diff = mPlayerY.Value - gameObject.transform.position.y;
        gameObject.transform.Translate(0.0f, diff, 0.0f, Space.World);
    }

    void CheckFireShot() {
        // update fire countdown
        mFireCountdown.Update(Time.deltaTime, true);

        // if the fire countdown is still going on, can't fire
        if (mFireCountdown.Active) {
            // can't fire now!
            return;
        }

        // check if player wants to fire a shot
        bool wantToFire = false;
        if (Input.touchCount > 0) {
            // a touch on the right side of the screen means "fire"
            foreach (Touch t in Input.touches) {
                if (t.position.x > Screen.width / 2) {
                    wantToFire = true;
                    break;
                }
            }
        } else {
            wantToFire = Input.GetButton("Fire1");
        }

        if (wantToFire) {
            // create the laser object
            int idx = Util.Clamp(mStats.Damage - 1, 0, LaserPrefab.Length - 1);
            GameObject laser = (GameObject) Instantiate(LaserPrefab[idx]);

            // position it
            laser.transform.Translate(0, gameObject.transform.position.y, 0, Space.World);

            // start the countdown
            mFireCountdown.Start(mStats.FireCooldown);

            // play the sound
            AudioSource.PlayClipAtPoint(LaserSfx, Vector3.zero);
        }
    }

    void OnTriggerEnter(Collider c) {
        if (mLevelController.IsPlaying) {
            HurtsPlayer hp = c.gameObject.GetComponent<HurtsPlayer>();
            if (hp != null) {
                TakeDamage(hp.Damage);
            }
        }
    }

    public void KillPlayer() {
        mLevelController.HandlePlayerDied();
        Util.HideObject(gameObject);
        AudioSource.PlayClipAtPoint(KaboomSfx, gameObject.transform.position);
        GameObject o = (GameObject) Instantiate(KaboomEffect);
        o.transform.Translate(gameObject.transform.position);
    }

    void DoShieldBreakFx() {
        AudioSource.PlayClipAtPoint(HurtSfx, Vector3.zero);
        GameObject o = (GameObject) Instantiate(ShieldBreakEffect);
        o.transform.Translate(gameObject.transform.position);
    }

    void TakeDamage(int damage) {
        if (mShieldLeft > 0) {
            mShieldLeft--;
            UpdateShield();
            DoShieldBreakFx();
        } else {
            KillPlayer();
        }
    }

    // Called when the pilot level goes up
    public void HandleLevelUp() {
        // update stats and recover shields
        mStats = GameManager.Instance.Progress.CurPilotStats;
        mShieldLeft = mStats.ShieldPoints;
        UpdateShield();
        UpdateAim();
        UnlockRankBasedAchievements();
    }

    void UnlockRankBasedAchievements() {
        int i;
        for (i = 0; i < GameIds.Achievements.ForRank.Length; i++) {
            int r = GameIds.Achievements.RankRequired[i];
            if (mStats.Level >= r) {
                GameManager.Instance.UnlockAchievement(GameIds.Achievements.ForRank[i]);
            }
        }
    }
}
