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
    using CubicPilot.UtilCode;
    using UnityEngine;


    public class Pulsate : MonoBehaviour
    {
        public bool XAxis = false;
        public bool YAxis = false;
        public bool ZAxis = false;
        public float Amount = 0.1f;
        public float Period = 1.0f;
        public float Phase = 0.0f;

        private Vector3 mStartScale;

        // Use this for initialization
        void Start()
        {
            mStartScale = gameObject.transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            float f = 1.0f + Util.SineWave(Amount, Period, Phase);
            gameObject.transform.localScale = new Vector3(
                mStartScale.x * (XAxis ? f : 1.0f),
                mStartScale.y * (YAxis ? f : 1.0f),
                mStartScale.z * (ZAxis ? f : 1.0f));
        }
    }
}
