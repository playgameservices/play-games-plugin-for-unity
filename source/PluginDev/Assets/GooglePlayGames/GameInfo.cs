// <copyright file="GameInfo.cs" company="Google Inc.">
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

/// <summary>
/// File containing information about the game. This is automatically updated by running the
/// platform-appropriate setup commands in the Unity editor (which does a simple search / replace
/// on the IDs in the form "__ID__"). We can check whether any particular field has been updated
/// by checking whether it still retains its initial value - we prevent the constants from being
/// replaced in the aforementioned search/replace by stripping off the leading and trailing "__".
/// </summary>
namespace GooglePlayGames
{
  using System;

  public static class GameInfo
  {
    // Filled in automatically
    public const string ApplicationId = "__APPID_";

    // Filled in automatically
    public const string IosClientId = "__CLIENTID__";

    private const string UnescapedApplicationId = "APPID";
    private const string UnescapedIosClientId = "CLIENTID";

    public static bool ApplicationIdInitialized()
    {
      return !ApplicationId.Equals(ToEscapedToken(UnescapedApplicationId));
    }

    public static bool IosClientIdInitialized()
    {
      return !IosClientId.Equals(ToEscapedToken(UnescapedIosClientId));
    }

    /// <summary>
    /// Returns an escaped token (i.e. one flanked with "__") for the passed token
    /// </summary>
    /// <returns>The escaped token.</returns>
    /// <param name="token">The Token</param>
    private static string ToEscapedToken(string token)
    {
      return string.Format("__{0}__", token);
    }
  }
}
