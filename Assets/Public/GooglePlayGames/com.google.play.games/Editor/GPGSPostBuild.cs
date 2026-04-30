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
    using System.Linq;

    using UnityEditor;
    using UnityEditor.Callbacks;


    public class GPGSPostBuild : AssetPostprocessor
    {
        [PostProcessBuild(99999)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (!GPGSProjectSettings.Instance.GetBool(GPGSUtil.ANDROIDSETUPDONEKEY, false))
            {
                EditorUtility.DisplayDialog("Google Play Games not configured!",
                    "Warning!!  Google Play Games was not configured, Game Services will not work correctly.",
                    "OK");
            }

            return;
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if(didDomainReload || importedAssets.Concat(deletedAssets).Concat(movedAssets).Any((path) => path.EndsWith("AndroidManifest.xml")))
                GPGSUtil.PatchAndroidManifest();
        }
    }
}
#endif //UNITY_ANDROID