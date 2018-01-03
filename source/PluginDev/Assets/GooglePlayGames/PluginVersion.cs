// <copyright file="PluginVersion.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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
  public class PluginVersion
  {
    // older versions, used when upgrading to other versions
    public const string VersionKeyCPP = "00911";
    public const string VersionKeyU5 = "00915";
    // patched 0.9.27 version for Unity 5.3 changes.
    public const string VersionKey27Patch = "00927a";

    public const string VersionKeyJarResolver = "00928";
    public const string VersionKeyNativeCRM = "00930";

    // Using JNI to get spendprobability - so don't delete the Games.cs files.
    public const string VersionKeyJNIStats = "00934";

    // New and improved jar resolver
    public const string VersionKeyJarResolverDLL = "00935";

    // Current Version.
    public const int VersionInt = 0x0950;
    public const string VersionString = "0.9.50";
    public const string VersionKey = "00950";

    // used to check for the correct min version or play services: 10.2
    public const int MinGmsCoreVersionCode = 10200000;

    // used to get the right version of dependencies.
    public const string PlayServicesVersionConstraint = "10+";
  }
}
