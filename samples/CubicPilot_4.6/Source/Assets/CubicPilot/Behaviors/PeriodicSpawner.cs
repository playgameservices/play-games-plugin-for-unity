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

    public class PeriodicSpawner : MonoBehaviour
    {
        public GameObject[] Spawnables;
        public float Interval = 2.0f;
        public float Countdown = 0.0f;
        public int WarmUpIterations = 0;

        void Start()
        {
            int i;
            for (i = 0; i < WarmUpIterations; i++)
            {
                GameObject o = SpawnOne();
                ConstVelocity cv = o.GetComponent<ConstVelocity>();
                if (cv != null)
                {
                    o.transform.Translate(cv.Velocity * i * Interval);
                }
            }
        }

        void Update()
        {
            Countdown -= Time.deltaTime;
            if (Countdown <= 0)
            {
                Countdown = Interval;
                SpawnOne();
            }
        }

        GameObject SpawnOne()
        {
            GameObject prefab = Spawnables[Random.Range(0, Spawnables.Length)];
            GameObject o = (GameObject)Instantiate(prefab);
            o.transform.Translate(gameObject.transform.position);
            return o;
        }
    }
}