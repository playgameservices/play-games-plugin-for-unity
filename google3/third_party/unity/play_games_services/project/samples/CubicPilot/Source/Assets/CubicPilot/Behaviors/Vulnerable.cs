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

namespace CubicPilot.Behaviors
{
    using CubicPilot.GameLogic;
    using CubicPilot.UtilCode;
    using UnityEngine;

    public class Vulnerable : MonoBehaviour
    {
        public int LifePoints = 1;
        public GameObject DestroyEffect;
        public AudioClip DestroySfx;
        public AudioClip TakeDamageSfx;
        public int ScoreValue = 50;
        public GameObject ScoreToastPrefab;
        private LevelController mLevelController;
        public Color ScoreToastColor;

        public bool ExplodeOnCivilian = true;

        private Countdown mHurtColorCountdown = new Countdown(false,
                                                    GameConsts.EnemyHitColorDuration);

        void Start()
        {
            mLevelController = GameObject.FindGameObjectWithTag("LevelController")
            .GetComponent<LevelController>();
        }

        void OnTriggerEnter(Collider c)
        {
            CausesDamage cd = c.gameObject.GetComponent<CausesDamage>();
            if (cd != null)
            {
                LifePoints -= cd.Damage;
                if (LifePoints <= 0)
                {
                    // enemy died!
                    CreateScoreToast(ReportKill());
                    Kaboom();
                    return;
                }
                else
                {
                    // take damage, but don't die
                    TakeDamage();
                }
            }

            if (ExplodeOnCivilian && c.gameObject.tag == "Civilian")
            {
                Kaboom();
                return;
            }
        }

        void CreateScoreToast(int earned)
        {
            GameObject o = (GameObject)Instantiate(ScoreToastPrefab);
            o.transform.Translate(gameObject.transform.position);
            ScoreToastController c = o.GetComponent<ScoreToastController>();
            c.Value = earned;
            c.ToastColor = ScoreToastColor;
        }

        int ReportKill()
        {
            // report score to level controller
            return mLevelController.ReportKill(ScoreValue, gameObject.transform.position.x);
        }

        void MultColor(GameObject o, float factor)
        {
            Color c = o.GetComponent<Renderer>().material.color;
            o.GetComponent<Renderer>().material.color = new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
        }

        void ToggleHurtColor(bool turnOn)
        {
            float factor = turnOn ? 0.25f : 4.0f;
            MultColor(gameObject, factor);
            int i = 0;
            for (i = 0; i < gameObject.transform.childCount; ++i)
            {
                MultColor(gameObject.transform.GetChild(i).gameObject, factor);
            }
        }

        void Update()
        {
            mHurtColorCountdown.Update(Time.deltaTime, false);
            if (mHurtColorCountdown.Expired)
            {
                ToggleHurtColor(false);
                mHurtColorCountdown.Stop();
            }
        }

        void Kaboom()
        {
            if (DestroyEffect != null)
            {
                GameObject o = (GameObject)Instantiate(DestroyEffect);
                o.transform.Translate(gameObject.transform.position);
            }

            Destroy(gameObject);
            AudioSource.PlayClipAtPoint(DestroySfx, Vector3.zero);
        }

        void TakeDamage()
        {
            // take damage, but don't die yet
            if (!mHurtColorCountdown.Active)
            {
                ToggleHurtColor(true);
            }

            mHurtColorCountdown.Start();
            AudioSource.PlayClipAtPoint(TakeDamageSfx, Vector3.zero);
        }
    }
}
