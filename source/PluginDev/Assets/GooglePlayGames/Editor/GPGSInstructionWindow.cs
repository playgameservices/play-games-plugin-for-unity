// <copyright file="GPGSInstructionWindow.cs" company="Google Inc.">
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

namespace GooglePlayGames
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class GPGSInstructionWindow : EditorWindow
    {
        private Vector2 mScrollPosition = Vector2.zero;

        private bool usingCocoaPod = false;
        private string instructions;

        public void OnGUI()
        {

                if (!UsingCocoaPod)
                {
                    instructions = GPGSUtil.ReadFile("Assets/GooglePlayGames/Editor/ios_instructions");
                }
                else
                {
                    instructions = GPGSUtil.ReadFile("Assets/GooglePlayGames/Editor/cocoapod_instructions");
                }

            mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
            GUILayout.TextArea(instructions);
            EditorGUILayout.EndScrollView();
        }

        public bool UsingCocoaPod
        {
            get
            {
                return usingCocoaPod;
            }
            set
            {
                usingCocoaPod = value;
            }
        }
    }
}
