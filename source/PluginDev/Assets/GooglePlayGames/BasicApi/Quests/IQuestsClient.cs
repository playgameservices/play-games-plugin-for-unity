// <copyright file="IQuestsClient.cs" company="Google Inc.">
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
    using System.Collections.Generic;

    /// <summary>
    /// Flags for determining the state of quests to be fetched.
    /// </summary>
    [Flags]
    public enum QuestFetchFlags
    {
        Upcoming = 1 << 0,
        Open = 1 << 1,
        Accepted = 1 << 2,
        Completed = 1 << 3,
        CompletedNotClaimed = 1 << 4,
        Expired = 1 << 5,
        EndingSoon = 1 << 6,
        Failed = 1 << 7,
        All = -1,
    }

    /// <summary>
    /// Result statuses for quest acceptance.
    /// </summary>
    public enum QuestAcceptStatus
    {
        Success,
        BadInput,
        InternalError,
        NotAuthorized,
        Timeout,
        QuestNoLongerAvailable,
        QuestNotStarted,
    }

    /// <summary>
    /// Result statuses for claiming quest milestones.
    /// </summary>
    public enum QuestClaimMilestoneStatus
    {
        Success,
        BadInput,
        InternalError,
        NotAuthorized,
        Timeout,
        MilestoneAlreadyClaimed,
        MilestoneClaimFailed,
    }

    /// <summary>
    /// Result statuses for the quest UI.
    /// </summary>
    public enum QuestUiResult
    {
        UserRequestsQuestAcceptance,
        UserRequestsMilestoneClaiming,
        BadInput,
        InternalError,
        UserCanceled,
        NotAuthorized,
        VersionUpdateRequired,
        Timeout,
        UiBusy,
    }

    /// <summary>
    /// <para>An interface for interacting with quests.</para>
    ///
    /// <para>See online <a href="https://developers.google.com/games/services/common/concepts/quests">
    /// documentation for Quests and Events</a> for more information.</para>
    ///
    /// All callbacks in this interface must be invoked on the game thread.
    /// </summary>
    public interface IQuestsClient
    {
        /// <summary>
        /// Retrieves the quest with the indicated ID (if any).
        /// </summary>
        /// <param name="source">The source of the quest (i.e. whether we can return stale cached
        /// values).</param>
        /// <param name="questId">The ID of the Quest.</param>
        /// <param name="callback">The callback for the results. The passed quest will be non-null if
        /// and only if the request succeeded. This callback will be invoked on the game thread.</param>
        void Fetch(DataSource source, string questId, Action<ResponseStatus, IQuest> callback);

        /// <summary>
        /// Fetches all quests that are in one of the states indicated by the passed flags.
        /// </summary>
        /// <param name="source">The source of the quest (i.e. whether we can return stale cached
        /// values).
        /// <param name="flags">The bitwise 'or' of all of states we wish to match against.</param>
        /// <param name="callback">A callback containing the status of the callback, and the matching
        /// quests (if any). If the request failed, the list passed into the callback will be empty.
        /// This callback will be invoked on the game thread.</param>
        void FetchMatchingState(DataSource source, QuestFetchFlags flags,
                                Action<ResponseStatus, List<IQuest>> callback);

        /// <summary>
        /// Displays the UI for displaying all relevant quests. This UI allows users to view upcoming
        /// quests, select a quest to accept, and claim a milestone on a completed quest. If the user
        /// wants to accept a quest or claim a milestone, this action should be performed when the
        /// callback fires using <see cref="Accept"/> and <see cref="ClaimMilestone"/> respectively.
        /// </summary>
        /// <param name="callback">Callback indicating the result from the UI. If the status is
        /// <see cref="QuestUiResult.UserRequestsQuestAcceptance"/>, the passed quest will be non-null.
        /// If the status is <see cref="QuestUiResult.UserRequestsMilestoneClaiming"/>, the passed
        /// milestone will be non-null. This callback will be invoked on the game thread.</param>
        void ShowAllQuestsUI(Action<QuestUiResult, IQuest, IQuestMilestone> callback);

        /// <summary>
        /// Like <see cref="ShowAllQuestsUI"/> but for a specified quest.
        /// </summary>
        /// <param name="quest">The quest to show UI for.</param>
        /// <param name="callback">Callback indicating the result from the UI. If the status is
        /// <see cref="QuestUiResult.UserRequestsQuestAcceptance"/>, the passed quest will be non-null.
        /// If the status is <see cref="QuestUiResult.UserRequestsMilestoneClaiming"/>, the passed
        /// milestone will be non-null. This callback will be invoked on the game thread.</param>
        void ShowSpecificQuestUI(IQuest quest, Action<QuestUiResult, IQuest, IQuestMilestone> callback);

        /// <summary>
        /// Accepts the specified quest. Once a quest is accepted, subsequent events count towards
        /// completion of that quest. This will often be invoked using a quest received from
        /// <see cref="ShowAllQuestsUI"/> or <see cref="ShowSpecificQuestUI"/>.
        /// </summary>
        /// <param name="quest">The quest to accept.</param>
        /// <param name="callback">Callback indicating the result of the quest acceptance. If the
        /// request was successful, the passed quest will be non-null and reflect the post-acceptance
        /// status of the quest.</param>
        void Accept(IQuest quest, Action<QuestAcceptStatus, IQuest> callback);

        /// <summary>
        /// Claims a completed milestone. Claiming a milestone will often correspond to awarding an
        /// in-game reward for the player - the content of this milestone will correspond to the data
        /// returned by <see cref="IQuestMilestone.CompletionRewardData"/>.
        /// </summary>
        /// <param name="milestone">The milestone to claim.</param>
        /// <param name="callback">A callback reflecting the result of claiming a milestone. If the
        /// request failed, the passed quest and milestone will both be null. If the request succeeded,
        /// both the quest and milestone will be non-null and will reflect the new state of the quest
        /// and milestone.</param>
        void ClaimMilestone(IQuestMilestone milestone,
                            Action<QuestClaimMilestoneStatus, IQuest, IQuestMilestone> callback);
    }
}
