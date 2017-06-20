// <copyright file="IQuest.cs" company="Google Inc.">
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

namespace GooglePlayGames.BasicApi.Quests
{
    using System;

    public enum QuestState
    {
        Upcoming = 1,
        Open = 2,
        Accepted = 3,
        Completed = 4,
        Expired = 5,
        Failed = 6,
    }

    /// <summary>
    /// An interface for a quest.
    ///
    /// <para>See online <a href="https://developers.google.com/games/services/common/concepts/quests">
    /// documentation for Quests and Events</a> for more information.</para>
    /// </summary>
    public interface IQuest
    {
        /// <summary>
        /// The ID of the quest.
        /// </summary>
        string Id
        {
            get;
        }

        /// <summary>
        /// The human-readable name of the quest.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// The human-readable description of the quest.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// The URL containing the banner image for the quest. May be empty if no banner image is
        /// specified in the play console (this is allowed prior to quest publication).
        /// </summary>
        string BannerUrl
        {
            get;
        }

        /// <summary>
        /// The URL containing the icon image for the quest. May be empty if no banner image is
        /// specified in the play console (this is allowed prior to quest publication).
        /// </summary>
        string IconUrl
        {
            get;
        }

        /// <summary>
        /// Gets the start time of the quest, i.e. the time after which the quest may be accepted
        /// by the player.
        /// </summary>
        DateTime StartTime
        {
            get;
        }

        /// <summary>
        /// The time after which quest is no longer eligible for acceptance.
        /// </summary>
        DateTime ExpirationTime
        {
            get;
        }

        /// <summary>
        /// The time at which the user accepted the quest. Absent if the quest has not been accepted.
        /// </summary>
        /// <value>The accepted time.</value>
        DateTime? AcceptedTime
        {
            get;
        }

        /// <summary>
        /// The current milestone for the quest. This represents the next goal that players should hit
        /// on their way to completing the quest.
        /// </summary>
        [Obsolete("Quests are being removed in 2018.")]
        IQuestMilestone Milestone
        {
            get;
        }

        /// <summary>
        /// The current state of the quest.
        /// </summary>
        QuestState State
        {
            get;
        }
    }
}
