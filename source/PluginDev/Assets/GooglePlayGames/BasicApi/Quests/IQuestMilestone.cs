// <copyright file="IQuestMilestone.cs" company="Google Inc.">
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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.BasicApi.Quests
{
    public enum MilestoneState
    {
        NotStarted = 1,
        NotCompleted = 2,
        CompletedNotClaimed = 3,
        Claimed = 4,
    }

    /// <summary>
    /// An interface for a quest milestone.
    ///
    /// <para>See online <a href="https://developers.google.com/games/services/common/concepts/quests">
    /// documentation for Quests and Events</a> for more information.</para>
    /// </summary>
    public interface IQuestMilestone
    {
        /// <summary>
        /// The ID of the milestone.
        /// </summary>
        string Id
        {
            get;
        }

        /// <summary>
        /// The ID of the event whose progress counts towards completion of this milestone.
        /// </summary>
        string EventId
        {
            get;
        }

        /// <summary>
        /// The quest that contains this milestone.
        /// </summary>
        string QuestId
        {
            get;
        }

        /// <summary>
        /// The current progress towards completion of this milestone.
        /// </summary>
        /// <value>The current count.</value>
        ulong CurrentCount
        {
            get;
        }

        /// <summary>
        /// The count that must be reached before the quest is considered complete.
        /// </summary>
        /// <value>The target count.</value>
        ulong TargetCount
        {
            get;
        }

        /// <summary>
        /// Developer-specified binary data representing the reward for completing this milestone.
        /// The format and content is completed specified by you, the developer in the developer
        /// console.
        /// </summary>
        byte[] CompletionRewardData
        {
            get;
        }

        /// <summary>
        /// The current state of the milestone.
        /// </summary>
        MilestoneState State
        {
            get;
        }
    }
}
#endif
