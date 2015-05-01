// <copyright file="ItemState.cs" company="Google Inc.">
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
    using System;
    using UnityEngine;

    /// <summary>
    /// Item state of a game object in the system.
    /// This is used to communicate which objects should be created
    /// and their position. 
    /// </summary>
    [Serializable]
    public class ItemState
    {
        // Known sets of prefab tiles in the game.
        public static string EnemyTileSet = "Enemies";
        public static string DropTileSet = "Drops";
        public static string DeadlyTileSet = "Deadlies";
        public static string ExitTileSet = "exit";
        public static string PlayerTileSet = "players";
       
        private bool enabled;
        private string name;
        private string tileSetName;
        private int prefabIndex;

        // don't serialize the vector3, but keep it for
        // ease of use.
        [NonSerialized]
        private Vector3 position = Vector3.zero;
        private float x;
        private float y;
        private float z;

        // just of ease of use.
        [NonSerialized]
        private Quaternion rotation = Quaternion.identity;
        private float qw;
        private float qz;
        private float qy;
        private float qx;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NearbyDroids.ItemState"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the object.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the tile set.
        /// </summary>
        /// <value>The name of the tile set.</value>
        public string TileSetName
        {
            get
            {
                return tileSetName;
            }

            set
            {
                tileSetName = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the prefab within the tile set.
        /// </summary>
        /// <value>The index of the prefab.</value>
        public int PrefabIndex
        {
            get
            {
                return prefabIndex;
            }

            set
            {
                prefabIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector3 Position
        {
            get
            {
                if (position == Vector3.zero &&
                    Mathf.Abs(x + y + z) > float.Epsilon)
                {
                    position = new Vector3(x, y, z);
                }

                return this.position;
            }

            set
            {
                position = value;
               
                x = position.x;
                y = position.y;
                z = position.z;
            }
        }

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>The rotation.</value>
        public Quaternion Rotation
        {
            get
            {
                if (rotation == Quaternion.identity &&
                    (Mathf.Abs(qx - Quaternion.identity.x) > float.Epsilon ||
                        Mathf.Abs(qy - Quaternion.identity.y) > float.Epsilon ||
                        Mathf.Abs(qz - Quaternion.identity.z) > float.Epsilon ||
                        Mathf.Abs(qw - Quaternion.identity.w) > float.Epsilon))
                {
                    rotation = new Quaternion(qx, qy, qz, qw);
                }

                return this.rotation;
            }

            set
            {
                rotation = value;
                qx = rotation.x;
                qy = rotation.y;
                qz = rotation.z;
                qw = rotation.w;
            }
        }
    }
}
