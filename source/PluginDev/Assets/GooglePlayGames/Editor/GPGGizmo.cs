// <copyright file="GPGGizmo.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.  All Rights Reserved.
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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// GPG gizmo. Displays in the top left of the scene view a warning that
    /// GPGS has not been setup.
    /// </summary>
    [ExecuteInEditMode]
    [CustomEditor(typeof(GameObject))]
    [CanEditMultipleObjects]
    public class GPGGizmo : UnityEditor.Editor
    {
        /// <summary>
        /// SceneGUI drawing handler.  Called by UnityEditor
        /// </summary>
        public void OnSceneGUI()
        {
            GUIStyle s = new GUIStyle(GUI.skin.label);
            s.active.textColor = Color.blue;
            s.active.background = Texture2D.blackTexture;
            s.normal.textColor = Color.blue;
            s.richText = true;
            Rect rect = new Rect(10, 10, Screen.width - 100, 50);
            bool doneSetup = GPGSUtil.IsSetupDone();

            if (!doneSetup)
            {
                Handles.BeginGUI();

                if (GUI.Button(
                        rect,
                        "<color=red>Warning! You need to configure </color><b>Google Play Game Services</b> ",
                        s))
                {
                    #if UNITY_ANDROID
                    GPGSAndroidSetupUI.MenuItemFileGPGSAndroidSetup();
                    #elif (UNITY_IPHONE && !NO_GPGS)
                    GPGSIOSSetupUI.MenuItemGPGSIOSSetup();
                    #endif
                }

                Handles.EndGUI();
            }
        }
    }
}
