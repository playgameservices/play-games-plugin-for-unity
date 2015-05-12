// <copyright file="Loader.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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

namespace NearbyDroids
{
    using UnityEngine;

    /// <summary>
    /// Loader. A helper object that loads the static instance of the GameManager prefab.
    /// This allows us to have a decoupled GameManager from the scene.
    /// Heavily inspired by http://unity3d.com/learn/tutorials/projects/2d-roguelike 
    /// </summary>
    public class Loader : MonoBehaviour
    {
        public GameObject gameManager;

        internal void Awake()
        {
            // Check if a GameManager has already been assigned to
            // static variable GameManager.instance or if it's still null
            if (GameManager.Instance == null)
            {
                // Instantiate gameManager prefab
                Instantiate(gameManager);
            }
        }
    }
}