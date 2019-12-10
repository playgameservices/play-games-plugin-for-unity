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
    using UnityEngine;

    public class CheatCodes : MonoBehaviour
    {
        void Update()
        {
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.X))
            {
                GameManager.Instance.Progress.ForceLevelUp();
                GameObject.Find("Player").GetComponent<PlayerController>().HandleLevelUp();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                GameManager.Instance.Progress.ForceLevelDown();
                GameObject.Find("Player").GetComponent<PlayerController>().HandleLevelUp();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                GameManager.Instance.FinishLevelAndGoToNext(123, 2);
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                LevelController lc = GameObject.Find("LevelController").GetComponent<LevelController>();
                lc.CutTime(10);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                PlayerPrefs.DeleteAll();
                Application.Quit();
            }
            #endif
        }
    }
}
