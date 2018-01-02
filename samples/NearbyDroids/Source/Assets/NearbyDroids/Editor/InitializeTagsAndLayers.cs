// <copyright file="InitializeTagsAndLayers.cs" company="Google Inc.">
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

namespace NearbyDroids
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Initialize tags and layers.
    /// This is used to initialize the tags and layers used by the NearbyDroids
    /// sample.  Exporting packages does not include the tags and layers,
    /// so this script is run in the editor to make sure the tags and layers
    /// are created.
    /// </summary>
    [InitializeOnLoad]
    public class InitializeTagsAndLayers
    {
        /// <summary>
        /// The required tags for the NearbyDroid sample.
        /// </summary>
        private static string[] requiredTags =
        {
            "powerup",
            "enemy",
            "deadly",
            "exit"
        };

        /// <summary>
        /// The required layers for the NearbyDroid sample.
        /// </summary>
        private static string[] requiredLayers =
        {
            "Blocking Layer"
        };

        /// <summary>
        /// The required sorting layers for the NearbyDroid sample.
        /// </summary>
        private static string[] sortingLayers =
        {
            "Units",
            "Items",
            "Floor",
        };

        /// <summary>
        /// The sorting layer identifiers for the NearbyDroid sample.
        /// </summary>
        private static long[] sortingLayerIds =
        {
            2591762383,
            1718506405,
            812571243
        };

        /// <summary>
        /// Initializes static members of the <see cref="NearbyDroids.InitializeTagsAndLayers"/> class.
        /// The static constructor is called by the Unity editor. because of the
        /// initializeOnLoad directive.
        /// </summary>
        static InitializeTagsAndLayers()
        {
            Debug.Log("Checking for custom tags and layers");

            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(
                    "ProjectSettings/TagManager.asset")[0]);

            CheckTags(tagManager);

            #if UNITY_5 || UNITY_5_3_OR_NEWER
            CheckLayers(tagManager);
            #else
            Debug.LogError("WARNING!! You are using an older version of Unity, " +
                "Please make sure the tags, layers, and sorting layers are set correcty!");
            CheckLayersOld(tagManager);
            #endif

            // save our work!
            tagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Checks the layers and adds them if needed.
        /// </summary>
        /// <param name="tagManager">Tag manager.</param>
        private static void CheckLayers(SerializedObject tagManager)
        {
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            int start = 8;
            for (int i = 0; i < requiredLayers.Length; i++)
            {
                if (layersProp.arraySize > start + i)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(start + i);
                    if (sp != null && sp.stringValue != requiredLayers[i])
                    {
                        sp.stringValue = requiredLayers[i];
                        Debug.Log("Adding layer " + requiredLayers[i]);
                    }
                }
            }

            // now do sorting layers
            layersProp = tagManager.FindProperty("m_SortingLayers");
            Debug.Log("Found " + layersProp);
            for (int index = 0; index < sortingLayers.Length; index++)
            {
                string name = sortingLayers[index];
                bool found = false;
                for (int i = 0; i < layersProp.arraySize; i++)
                {
                    SerializedProperty t = layersProp.GetArrayElementAtIndex(i);
                    SerializedProperty n = t.FindPropertyRelative("name");
                    if (n.stringValue == name)
                    {
                        found = true;
                        t.FindPropertyRelative("uniqueID").longValue = sortingLayerIds[index];
                        break;
                    }
                }

                if (!found)
                {
                    layersProp.InsertArrayElementAtIndex(0);
                    SerializedProperty t = layersProp.GetArrayElementAtIndex(0);
                    t.FindPropertyRelative("name").stringValue = name;
                    t.FindPropertyRelative("uniqueID").longValue = sortingLayerIds[index];
                    Debug.Log("Adding sorting layer: " + name);
                }
            }
        }

        /// <summary>
        /// Checks the layers in the pre-unity 5.0 layout
        /// NOTE:  This is untested!!!
        /// </summary>
        /// <param name="tagManager">Tag manager.</param>
        private static void CheckLayersOld(SerializedObject tagManager)
        {
            // user defined layers start at 8
            int start = 8;
            for (int i = 0; i < requiredLayers.Length; i++)
            {
                string nm = "User Layer " + (i + start);
                SerializedProperty sp = tagManager.FindProperty(nm);
                if (sp != null)
                {
                    sp.stringValue = requiredLayers[i];
                }
            }
        }

        /// <summary>
        /// Checks the tags to make sure they are defined.
        /// </summary>
        /// <param name="tagManager">Tag manager.</param>
        private static void CheckTags(SerializedObject tagManager)
        {
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            for (int index = 0; index < requiredTags.Length; index++)
            {
                string tag = requiredTags[index];
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                    if (t.stringValue == tag)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                    n.stringValue = tag;
                    Debug.Log("Adding tag: " + tag);
                }
            }
        }
    }
}
