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

public class AutoDelete : MonoBehaviour {
    public bool ObjectMovesForward = false;
    public float Countdown = float.PositiveInfinity;
    public float AdditionalTolerance = 0.0f;

    void Update () {
        Vector3 pos = gameObject.transform.position;

        Countdown -= Time.deltaTime;

        float a = AdditionalTolerance; // shorthand
        bool destroy = (Countdown <= 0) ||
            (ObjectMovesForward && pos.x > GameConsts.ArenaMaxX + a) ||
            (pos.x < GameConsts.ArenaMinX - a) ||
            (pos.y < GameConsts.ArenaMinY - a) ||
            (pos.y > GameConsts.ArenaMaxY + a);

        if (destroy) {
            Destroy(gameObject);
        }
    }
}
