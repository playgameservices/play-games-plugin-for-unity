// <copyright file="ForceNoGpgsForIOS.cs" company="Google Inc.">
// Copyright (C) 2018 Google Inc.
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

namespace GooglePlayGames.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    #if UNITY_2017
    using UnityEditor.Build;
    #endif
    using UnityEditor;

    [InitializeOnLoad]
    public class ForceNoGpgsForIOS 
    #if UNITY_2017
        : IActiveBuildTargetChanged
    #endif
    {
        static ForceNoGpgsForIOS ()
        {
            setNoGPGS ();
        }

        public void OnActiveBuildTargetChanged (BuildTarget previousTarget, BuildTarget newTarget)
        {
            if (newTarget == BuildTarget.iOS) {
                setNoGPGS ();
            }
        }

        public int callbackOrder { get { return 0; } }

        private static void setNoGPGS ()
        {
            Debug.Log ("Forcing NO_GPGS to be defined for iOS builds.");
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS);
            if (string.IsNullOrEmpty (symbols)) {
                symbols = "NO_GPGS";
            } else if (!symbols.Contains ("NO_GPGS")) {
                symbols += ";NO_GPGS";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup (BuildTargetGroup.iOS, symbols);
        }
    }
}
