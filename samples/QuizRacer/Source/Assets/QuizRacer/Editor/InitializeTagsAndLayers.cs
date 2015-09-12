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

namespace QuizRacer.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Initialize tags and layers.
    /// This is used to initialize the tags and layers used by the
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
            "gamepad"
        };


        /// <summary>
        /// Initializes static members of the class.
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

            // save our work!
            tagManager.ApplyModifiedProperties();
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
