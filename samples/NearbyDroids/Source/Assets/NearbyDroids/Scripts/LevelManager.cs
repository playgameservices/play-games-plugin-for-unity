// <copyright file="LevelManager.cs" company="Google Inc.">
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
    using System.Collections.Generic;
    using UnityEngine;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Level manager. Generates the levels
    /// Heavily inspired by http://unity3d.com/learn/tutorials/projects/2d-roguelike
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        // Size of our board
        public int columns = 8;
        public int rows = 8;

        // how many coins/drops per level
        public Count dropCount = new Count(1, 5);

        // Prefabs
        public GameObject[] playerAvatars;
        public GameObject[] floorTiles;
        public GameObject[] dropTiles;
        public GameObject[] enemyTiles;
        public GameObject[] outerWallTiles;
        public GameObject[] deadlyTiles;
        public GameObject exitTile;

        // the instance of the exit.
        private GameObject exitObj;

        // holds the board and floor background.
        private Transform boardHolder;

        // all the possible positions on the board, used 
        // to make sure random positions do not overlap.
        private List<Vector3> gridPositions = new List<Vector3>();

        // a dictionary of objects created by the level manager.  This
        // is used to have a quick lookup of the objects so they can be moved
        // as updates are received from the other players.
        private Dictionary<string, GameObject> myObjects =
            new Dictionary<string, GameObject>();

        /// <summary>
        /// Initialize this instance by clearing
        ///  our list gridPositions and prepares it to generate a new board.
        /// </summary>
        internal void Initialize()
        {
            // Clear our list gridPositions.
            gridPositions.Clear();

            // Loop through x axis (columns).
            for (int x = 1; x < columns - 1; x++)
            {
                // Within each column, loop through y axis (rows).
                for (int y = 1; y < rows - 1; y++)
                {
                    // At each index add a new Vector3 to our
                    // list with the x and y coordinates of that position.
                    gridPositions.Add(new Vector3(x, y, 0f));
                }
            }
        }

        /// <summary>
        /// Destroies the board.
        /// </summary>
        public void DestroyBoard()
        {
            if (boardHolder != null)
            {
                boardHolder.gameObject.SetActive(false);
                DestroyObject(boardHolder.gameObject);
                boardHolder = null;
            }

            // clear everything we created.
            myObjects.Clear();
        }

        // Sets up the outer walls and floor (background) of the game board.
        internal void BoardSetup()
        {
            if (boardHolder != null)
            {
                Debug.Log("Boardholder exists, so skipping setup!");
                return;
            }

            // Instantiate Board and set boardHolder to its transform.
            boardHolder = new GameObject("Board").transform;

            // Loop along x axis, starting from -1 (to fill corner)
            // with floor or outerwall edge tiles.
            for (int x = -1; x < columns + 1; x++)
            {
                // Loop along y axis, starting from -1 to place
                // floor or outerwall tiles.
                for (int y = -1; y < rows + 1; y++)
                {
                    // Choose a random tile from our array of floor tile
                    // prefabs and prepare to instantiate it.
                    GameObject toInstantiate = 
                        floorTiles[Random.Range(0, floorTiles.Length)];
                    Quaternion rot = Quaternion.identity;

                    // Check if we current position is at board edge
                    if (x == -1 || x == columns)
                    {
                        toInstantiate = outerWallTiles[1];
                        rot = Quaternion.Euler(0, 0, 90);
                    }
                        
                    if (y == -1 || y == rows)
                    {
                        toInstantiate = outerWallTiles[0];
                    }

                    // Instantiate the GameObject instance using the
                    // prefab chosen for
                    // toInstantiate at the Vector3 corresponding to
                    // current grid position in loop, cast it to GameObject.
                    GameObject instance = Instantiate(
                                              toInstantiate,
                                              new Vector3(x, y, 0f),
                                              rot) as GameObject;

                    // Set the parent of our newly instantiated object
                    // instance to boardHolder,
                    // this is just organizational to avoid cluttering hierarchy.
                    instance.transform.SetParent(boardHolder);
                }
            }
        }

        /// <summary>
        /// Randoms the position.
        /// RandomPosition returns a random position from our list gridPositions.
        /// each position can only be returned once.
        /// </summary>
        /// <returns>The position.</returns>
        public Vector3 RandomPosition()
        {
            // Declare an integer randomIndex, set it's value to a random
            // number between 0 and the count of items in our List gridPositions.
            int randomIndex = Random.Range(0, gridPositions.Count);

            // Declare a variable of type Vector3 called randomPosition, set
            // it's value to the entry at randomIndex 
            // from our List gridPositions.
            Vector3 randomPosition = gridPositions[randomIndex];

            // Remove the entry at randomIndex from the list so that it
            // can't be re-used.
            gridPositions.RemoveAt(randomIndex);

            // Return the randomly selected Vector3 position.
            return randomPosition;
        }

        /// <summary>
        /// Layouts the object at random.
        /// LayoutObjectAtRandom accepts an array of game objects to choose
        /// from along with a minimum and maximum range for the number
        /// of objects to create.
        /// </summary>
        /// <param name="tileSet">the key of which set of tiles to randomly layout</param>
        /// <param name="tileArray">the array of prefabs to select from</param> 
        /// <param name="minimum">Minimum number of tiles to layout</param>
        /// <param name="maximum">Maximum number of tiles to layout</param>
        /// <returns>List of itemStates of the created objects</returns>
        internal List<ItemState> LayoutObjectAtRandom(
            string tileSet,
            GameObject[] tileArray,
            int minimum,
            int maximum)
        {
            // Choose a random number of objects to instantiate
            // within the minimum and maximum limits
            int objectCount = Random.Range(minimum, maximum + 1);

            List<ItemState> items = new List<ItemState>();

            // Instantiate objects until the randomly chosen limit
            // objectCount is reached
            for (int i = 0; i < objectCount; i++)
            {
                // Choose a position for randomPosition by getting
                // a random position from our list of available Vector3s
                // stored in gridPosition
                Vector3 randomPosition = RandomPosition();

                // Choose a random tile from tileArray and assign
                // it to tileChoice
                int index = Random.Range(0, tileArray.Length);
                GameObject tileChoice = tileArray[index];

                // Instantiate tileChoice at the position returned by
                // RandomPosition with no change in rotation
                GameObject obj = Instantiate(
                                     tileChoice,
                                     randomPosition,
                                     Quaternion.identity) as GameObject;
                
                string objName = tileSet + "_" + randomPosition.x +
                                 "_" + randomPosition.y;
                
                obj.name = objName;

                ItemState item = new ItemState();
                item.Enabled = true;
                item.Name = objName;
                item.Position = randomPosition;
                item.Rotation = Quaternion.identity;
                item.PrefabIndex = index;
                item.TileSetName = tileSet;
                obj.GetComponent<Shareable>().State = item;
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Show the exit object which is initially hidden.
        /// </summary>
        /// <returns>The exit.</returns>
        public GameObject ShowExit()
        {
            exitObj.SetActive(true);
            return exitObj;
        }

        /// <summary>
        /// Find the object with the specified name.
        /// </summary>
        /// <param name="name">Name.</param>
        public GameObject Find(string name)
        {
            GameObject retval;
            return myObjects.TryGetValue(name, out retval) ? retval : null;
        }

        /// <summary>
        /// Gets the player object. with the given suffix (usually a deviceId)
        /// </summary>
        /// <returns>The player object.</returns>
        /// <param name="suffix">Suffix.</param>
        public GameObject GetPlayerObject(string suffix)
        {
            return Find(ItemState.PlayerTileSet + suffix);
        }

        /// <summary>
        /// Creates the item.
        /// </summary>
        /// <returns>The item.</returns>
        /// <param name="item">Item.</param>
        public GameObject CreateItem(ItemState item)
        {
            GameObject prefab;

            if (myObjects.ContainsKey(item.Name) && myObjects[item.Name] != null)
            {
                return myObjects[item.Name];
            }

            if (item.TileSetName == ItemState.DropTileSet)
            {
                prefab = dropTiles[item.PrefabIndex];
            }
            else if (item.TileSetName == ItemState.DeadlyTileSet)
            {
                prefab = deadlyTiles[item.PrefabIndex];
            }
            else if (item.TileSetName == ItemState.EnemyTileSet)
            {
                prefab = enemyTiles[item.PrefabIndex];
            }
            else if (item.TileSetName == ItemState.ExitTileSet)
            {
                prefab = exitTile;
            }
            else if (item.TileSetName == ItemState.PlayerTileSet)
            {
                prefab = playerAvatars[item.PrefabIndex % playerAvatars.Length];
            }
            else
            {
                Debug.LogWarning("Unknown tile set: " + item.TileSetName + " for " + item.Name);
                return null;
            }

            GameObject obj = Instantiate(prefab, item.Position, item.Rotation) as GameObject;
            obj.name = item.Name;
            obj.GetComponent<Shareable>().State = item;
            myObjects[obj.name] = obj;

            return obj;
        }

        /// <summary>
        /// Setups the scene.
        /// SetupScene initializes our level and calls the previous
        /// functions to lay out the game board
        /// </summary>
        /// <param name="level">Level number being laid out.</param>
        /// <returns>The list of itemState objects representing the scene.</returns>
        public List<ItemState> SetupScene(int level)
        {
            // Creates the outer walls and floor.
            BoardSetup();

            // Reset our list of gridpositions.
            Initialize();

            List<ItemState> items = new List<ItemState>();

            // Instantiate a random number of food tiles based on
            // minimum and maximum, at randomized positions.
            items.AddRange(
                LayoutObjectAtRandom(
                    ItemState.DropTileSet,
                    dropTiles,
                    dropCount.minimum,
                    dropCount.maximum));

            // use a log progression to get harder
            int logval = (int)Mathf.Log(level, 2f);

            int enemyCount = logval * 3;

            // Instantiate a random number of enemies based on minimum
            // and maximum, at randomized positions.
            items.AddRange(
                LayoutObjectAtRandom(
                    ItemState.EnemyTileSet,
                    enemyTiles,
                    enemyCount,
                    enemyCount));

            // layout the buzzers
            int buzzerCount = Random.Range(1, logval);
            items.AddRange(
                LayoutObjectAtRandom(
                    ItemState.DeadlyTileSet,
                    deadlyTiles,
                    buzzerCount,
                    buzzerCount));

            // put the exit somewhere, but keep don't activate it.
            Vector3 pos = RandomPosition();
            exitObj = Instantiate(exitTile, pos, Quaternion.identity) as GameObject;
            exitObj.SetActive(false);
            exitObj.name = ItemState.ExitTileSet + "_" + pos.x + "_" + pos.y;
            ItemState item = new ItemState();
            item.Enabled = false;
            item.Name = exitObj.name;
            item.Position = pos;
            item.Rotation = Quaternion.identity;
            item.PrefabIndex = 0;
            item.TileSetName = ItemState.ExitTileSet;
            exitObj.GetComponent<Shareable>().State = item;
            items.Add(item);

            return items;
        }

        /// <summary>
        /// Setups the scene based on the list of level data items.
        /// </summary>
        /// <param name="level">Level.</param>
        /// <param name="levelData">Level data.</param>
        public void SetupScene(int level, List<ItemState> levelData)
        {
            // Creates the outer walls and floor (this is the same all the time)
            BoardSetup();

            foreach (ItemState item in levelData)
            {
                // skip known objects
                if (myObjects.ContainsKey(item.Name) && myObjects[item.Name] != null)
                {
                    Debug.Log("Using existing object: " + item.Name);
                    GameObject reusedObj = myObjects[item.Name];
                    reusedObj.SetActive(item.Enabled);
                    continue;
                }

                GameObject prefab;
                if (item.TileSetName == ItemState.DropTileSet)
                {
                    prefab = dropTiles[item.PrefabIndex];
                }
                else if (item.TileSetName == ItemState.DeadlyTileSet)
                {
                    prefab = deadlyTiles[item.PrefabIndex];
                }
                else if (item.TileSetName == ItemState.EnemyTileSet)
                {
                    prefab = enemyTiles[item.PrefabIndex];
                }
                else if (item.TileSetName == ItemState.ExitTileSet)
                {
                    prefab = exitTile;
                }
                else if (item.TileSetName == ItemState.PlayerTileSet)
                {
                    prefab = playerAvatars[item.PrefabIndex % playerAvatars.Length];
                }
                else
                {
                    Debug.LogWarning("Unknown tile set: " + 
                        item.TileSetName + " for item " + item.Name);
                    continue;
                }

                GameObject obj = Instantiate(
                                     prefab,
                                     item.Position,
                                     item.Rotation) as GameObject;
                obj.name = item.Name;
                Debug.Log("Creating " + obj.name + " at " + obj.transform.position);
                obj.SetActive(item.Enabled);
                if (item.TileSetName == ItemState.ExitTileSet)
                {
                    exitObj = obj;
                }

                myObjects[item.Name] = obj;
                obj.GetComponent<Shareable>().State = item;
            }
        }

        /// <summary>
        /// Creates the player.
        /// </summary>
        /// <returns>The player.</returns>
        /// <param name="index">Index.</param>
        /// <param name="suffix">Suffix.</param>
        public GameObject CreatePlayer(int index, string suffix)
        {
            GameObject obj = GetPlayerObject(suffix);
            if (obj == null)
            {
                obj = Instantiate(
                    playerAvatars[index % playerAvatars.Length],
                    RandomPosition(),
                    Quaternion.identity) as GameObject;
                obj.name = ItemState.PlayerTileSet + suffix;
                myObjects[obj.name] = obj;

                ItemState item = new ItemState();
                item.Enabled = obj.activeSelf;
                item.Name = obj.name;
                item.Position = obj.transform.position;
                item.Rotation = obj.transform.rotation;
                item.PrefabIndex = index;
                item.TileSetName = ItemState.PlayerTileSet;
                obj.GetComponent<Shareable>().State = item;
            }

            return obj;
        }

        /// <summary>
        /// Count.
        /// Using Serializable allows us to embed a class with sub
        /// properties in the inspector.
        /// </summary>
        [Serializable]
        public class Count
        {
            public int minimum;
            public int maximum;

            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }
    }
}