// <copyright file="SharableObject.cs" company="Google Inc.">
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
    /// Sharable object.  This enables a game object to keep
    /// an ItemState object that can be used to send to a remote player.
    /// </summary>
    public class Shareable : MonoBehaviour
    {
        private ItemState state;

        public ItemState State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
            }
        }

        public void UpdateState()
        {
            if (state == null)
            {
                state = new ItemState();
            }

            state.Enabled = gameObject.activeSelf;
            state.Name = gameObject.name;
            state.Position = gameObject.transform.position;
            state.Rotation = gameObject.transform.rotation;
        }
    }
}