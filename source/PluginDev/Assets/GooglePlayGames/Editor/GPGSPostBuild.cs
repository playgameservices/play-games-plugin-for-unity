// <copyright file="GPGSPostBuild.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
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

#if UNITY_ANDROID

namespace GooglePlayGames.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor.Callbacks;
    using UnityEditor;
    using UnityEngine;

    public static class GPGSPostBuild
    {
        [PostProcessBuild (99999)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
#if UNITY_5_3_OR_NEWER
            if (target != BuildTarget.iOS)
            {
                if (!GPGSProjectSettings.Instance.GetBool(GPGSUtil.ANDROIDSETUPDONEKEY, false))
                {
                    EditorUtility.DisplayDialog("Google Play Games not configured!",
                        "Warning!!  Google Play Games was not configured, Game Services will not work correctly.",
                        "OK");
                }
                return;
            }
#endif
        }
    }
}
#endif
