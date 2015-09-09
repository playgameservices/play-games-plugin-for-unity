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

    public class PauseMenuEvents : MonoBehaviour
    {

        public GameObject pausedMenu;

        private bool mIsPaused;

        // Use this for initialization
        void Start()
        {
            mIsPaused = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!mIsPaused)
                {
                    DoPause();
                }
                else
                {
                    OnResume();
                }
            }
        }


        void DoPause()
        {
            mIsPaused = true;
            Time.timeScale = 0;
            pausedMenu.SetActive(true);
        }

        public void OnResume()
        {
            mIsPaused = false;
            Time.timeScale = 1;
            pausedMenu.SetActive(false);
        }

        public void OnQuit()
        {
            Time.timeScale = 1;
            pausedMenu.SetActive(false);
            GameManager.Instance.QuitToMenu();
        }
    }
}
