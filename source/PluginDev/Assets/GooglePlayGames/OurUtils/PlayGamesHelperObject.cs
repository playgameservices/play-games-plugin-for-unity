// <copyright file="PlayGamesHelperObject.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

namespace GooglePlayGames.OurUtils
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;

    public class PlayGamesHelperObject : MonoBehaviour
    {
        // our (singleton) instance
        private static PlayGamesHelperObject instance = null;

        // are we a dummy instance (used in the editor?)
        private static bool sIsDummy = false;

        // queue of actions to run on the game thread
        private static List<System.Action> sQueue = new List<System.Action>();

        // flag that alerts us that we should check the queue
        // (we do this just so we don't have to lock() the queue every
        // frame to check if it's empty or not).
        private volatile static bool sQueueEmpty = true;

        // callback for application pause and focus events
        private static Action<bool> sPauseCallback = null;
        private static Action<bool> sFocusCallback = null;

        // Call this once from the game thread
        public static void CreateObject()
        {
            if (instance != null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                // add an invisible game object to the scene
                GameObject obj = new GameObject("PlayGames_QueueRunner");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<PlayGamesHelperObject>();
            }
            else
            {
                instance = new PlayGamesHelperObject();
                sIsDummy = true;
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static void RunOnGameThread(System.Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (sIsDummy)
            {
                return;
            }

            lock (sQueue)
            {
                sQueue.Add(action);
                sQueueEmpty = false;
            }
        }

         public void Update()
        {
            if (sIsDummy || sQueueEmpty)
            {
                return;
            }
            // first copy the shared queue into a local queue
            List<System.Action> q = new List<System.Action>();
            lock (sQueue)
            {
                // transfer the whole queue to our local queue
                q.AddRange(sQueue);
                sQueue.Clear();
                sQueueEmpty = true;
            }

            // execute queued actions (from local queue)
            q.ForEach(a => a.Invoke());
        }

        public void OnApplicationFocus(bool focused)
        {
            if (null != sFocusCallback)
            {
                sFocusCallback.Invoke(focused);
            }
        }

        public void OnApplicationPause(bool paused)
        {
            if (null != sPauseCallback)
            {
                sPauseCallback.Invoke(paused);
            }
        }

        public static void SetFocusCallback(Action<bool> callback)
        {
            sFocusCallback = callback;
        }

        public static void SetPauseCallback(Action<bool> callback)
        {
            sPauseCallback = callback;
        }

        #if UNITY_ANDROID
        public static int GetStatusCode(AndroidJavaObject result)
        {
            if (result == null)
            {
                return -1;
            }
            AndroidJavaObject status = result.Call<AndroidJavaObject>("getStatus");
            return status.Call<int>("getStatusCode");
        }
        #endif 
    }
}