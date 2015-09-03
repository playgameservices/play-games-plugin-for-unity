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
    using UnityEngine;

    public class Shoots : MonoBehaviour
    {
        public Vector3 ShotVelocity = new Vector3(-10, 0, 0);
        public GameObject ProjectilePrefab;
        public Vector3 SpawnOffset;
        public float Interval = 2.0f;
        public float Countdown = 0.0f;

        void Update()
        {
            if ((Countdown -= Time.deltaTime) < 0)
            {
                Fire();
                Countdown = Interval;
            }
        }

        void Fire()
        {
            GameObject o = (GameObject)Instantiate(ProjectilePrefab);
            o.transform.Translate(gameObject.transform.position + SpawnOffset);
            ConstVelocity c = o.GetComponent<ConstVelocity>();
            c.Velocity = ShotVelocity;
        }
    }
}
