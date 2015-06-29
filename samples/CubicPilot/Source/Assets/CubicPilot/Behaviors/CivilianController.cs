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

public class CivilianController : MonoBehaviour {
    public GameObject ExplodeEffect;
    public AudioClip ExplodeSfx;
    public GameObject LevelController;

    void OnTriggerEnter(Collider c) {
        Vulnerable v = c.gameObject.GetComponent<Vulnerable>();
        if (v != null && v.ExplodeOnCivilian) {
            Destroy(gameObject);
            GameObject o = (GameObject) Instantiate(ExplodeEffect);
            o.transform.Translate(gameObject.transform.position);
            AudioSource.PlayClipAtPoint(ExplodeSfx, Vector3.zero);

            LevelController lc = LevelController.GetComponent<LevelController>();
            lc.HandleCivilianDestroyed();
        }
    }
}
