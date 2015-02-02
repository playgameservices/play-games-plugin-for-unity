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

public class LaserController : MonoBehaviour {
    private float mSpeed;

    void Start() {
        mSpeed = GameManager.Instance.Progress.CurPilotStats.LaserSpeed;
    }

    void Update () {
        gameObject.transform.Translate(mSpeed * Time.deltaTime,
            0, 0, Space.World);
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.GetComponent<Vulnerable>() != null) {
            LevelController controller =
              GameObject.Find("LevelController").GetComponent<LevelController>();
            controller.HandleLaserHit();
            Destroy(gameObject);
        }
    }
}
