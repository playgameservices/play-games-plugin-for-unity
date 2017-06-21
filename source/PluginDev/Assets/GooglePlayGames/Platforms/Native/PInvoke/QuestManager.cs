// <copyright file="QuestManager.cs" company="Google Inc.">
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

namespace GooglePlayGames.Native.PInvoke
{
    using GooglePlayGames.OurUtils;
    using System;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using C = GooglePlayGames.Native.Cwrapper.QuestManager;
    using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
    using Types = GooglePlayGames.Native.Cwrapper.Types;

    [Obsolete("Quests are being removed in 2018.")]
    internal class QuestManager
    {
        private readonly GameServices mServices;

        [Obsolete("Quests are being removed in 2018.")]
        internal QuestManager(GameServices services)
        {
            mServices = Misc.CheckNotNull(services);
        }

        internal void Fetch(Types.DataSource source, string questId, Action<FetchResponse> callback)
        {
            C.QuestManager_Fetch(
                mServices.AsHandle(),
                source,
                questId,
                InternalFetchCallback,
                Callbacks.ToIntPtr<FetchResponse>(callback, FetchResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchCallback))]
        internal static void InternalFetchCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "QuestManager#FetchCallback", Callbacks.Type.Temporary, response, data);
        }

        internal void FetchList(Types.DataSource source, int fetchFlags,
                            Action<FetchListResponse> callback)
        {
            C.QuestManager_FetchList(
                mServices.AsHandle(),
                source,
                fetchFlags,
                InternalFetchListCallback,
                Callbacks.ToIntPtr<FetchListResponse>(callback, FetchListResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.FetchListCallback))]
        internal static void InternalFetchListCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "QuestManager#FetchListCallback", Callbacks.Type.Temporary, response, data);
        }

        internal void ShowAllQuestUI(Action<QuestUIResponse> callback)
        {
            C.QuestManager_ShowAllUI(
                mServices.AsHandle(),
                InternalQuestUICallback,
                Callbacks.ToIntPtr<QuestUIResponse>(callback, QuestUIResponse.FromPointer));
        }

        internal void ShowQuestUI(NativeQuest quest, Action<QuestUIResponse> callback)
        {
            C.QuestManager_ShowUI(
                mServices.AsHandle(),
                quest.AsPointer(),
                InternalQuestUICallback,
                Callbacks.ToIntPtr<QuestUIResponse>(
                    callback, QuestUIResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.QuestUICallback))]
        internal static void InternalQuestUICallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "QuestManager#QuestUICallback", Callbacks.Type.Temporary, response, data);
        }

        internal void Accept(NativeQuest quest, Action<AcceptResponse> callback)
        {
            C.QuestManager_Accept(
                mServices.AsHandle(),
                quest.AsPointer(),
                InternalAcceptCallback,
                Callbacks.ToIntPtr<AcceptResponse>(callback, AcceptResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.AcceptCallback))]
        internal static void InternalAcceptCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "QuestManager#AcceptCallback", Callbacks.Type.Temporary, response, data);
        }

        [Obsolete("Quests are being removed in 2018.")]
        internal void ClaimMilestone(NativeQuestMilestone milestone,
                                 Action<ClaimMilestoneResponse> callback)
        {
            C.QuestManager_ClaimMilestone(
                mServices.AsHandle(),
                milestone.AsPointer(),
                InternalClaimMilestoneCallback,
                Callbacks.ToIntPtr<ClaimMilestoneResponse>(
                    callback, ClaimMilestoneResponse.FromPointer));
        }

        [AOT.MonoPInvokeCallback(typeof(C.ClaimMilestoneCallback))]
        internal static void InternalClaimMilestoneCallback(IntPtr response, IntPtr data)
        {
            Callbacks.PerformInternalCallback(
                "QuestManager#ClaimMilestoneCallback", Callbacks.Type.Temporary, response, data);
        }

        internal class FetchResponse : BaseReferenceHolder
        {
            internal FetchResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.ResponseStatus ResponseStatus()
            {
                return C.QuestManager_FetchResponse_GetStatus(SelfPtr());
            }

            internal NativeQuest Data()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                return new NativeQuest(C.QuestManager_FetchResponse_GetData(SelfPtr()));
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.QuestManager_FetchResponse_Dispose(selfPointer);
            }

            internal static FetchResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new FetchResponse(pointer);
            }
        }

        internal class FetchListResponse : BaseReferenceHolder
        {
            internal FetchListResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.ResponseStatus ResponseStatus()
            {
                return C.QuestManager_FetchListResponse_GetStatus(SelfPtr());
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            internal IEnumerable<NativeQuest> Data()
            {
                return PInvokeUtilities.ToEnumerable<NativeQuest>(
                    C.QuestManager_FetchListResponse_GetData_Length(SelfPtr()),
                    index => new NativeQuest(
                        C.QuestManager_FetchListResponse_GetData_GetElement(SelfPtr(), index)));
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.QuestManager_FetchListResponse_Dispose(selfPointer);
            }

            internal static FetchListResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new FetchListResponse(pointer);
            }
        }

        internal class ClaimMilestoneResponse : BaseReferenceHolder
        {
            internal ClaimMilestoneResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.QuestClaimMilestoneStatus ResponseStatus()
            {
                return C.QuestManager_ClaimMilestoneResponse_GetStatus(SelfPtr());
            }

            internal NativeQuest Quest()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                var quest = new NativeQuest(C.QuestManager_ClaimMilestoneResponse_GetQuest(SelfPtr()));

                if (quest.Valid())
                {
                    return quest;
                }
                else
                {
                    quest.Dispose();
                    return null;
                }
            }

            internal NativeQuestMilestone ClaimedMilestone()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                var milestone = new NativeQuestMilestone(
                                C.QuestManager_ClaimMilestoneResponse_GetClaimedMilestone(SelfPtr()));

                if (milestone.Valid())
                {
                    return milestone;
                }
                else
                {
                    milestone.Dispose();
                    return null;
                }
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.QuestManager_ClaimMilestoneResponse_Dispose(selfPointer);
            }

            internal static ClaimMilestoneResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new ClaimMilestoneResponse(pointer);
            }
        }

        internal class AcceptResponse : BaseReferenceHolder
        {
            internal AcceptResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.QuestAcceptStatus ResponseStatus()
            {
                return C.QuestManager_AcceptResponse_GetStatus(SelfPtr());
            }

            internal NativeQuest AcceptedQuest()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                return new NativeQuest(C.QuestManager_AcceptResponse_GetAcceptedQuest(SelfPtr()));
            }

            internal bool RequestSucceeded()
            {
                return ResponseStatus() > 0;
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.QuestManager_AcceptResponse_Dispose(selfPointer);
            }

            internal static AcceptResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new AcceptResponse(pointer);
            }
        }

        internal class QuestUIResponse : BaseReferenceHolder
        {
            internal QuestUIResponse(IntPtr selfPointer)
                : base(selfPointer)
            {
            }

            internal Status.UIStatus RequestStatus()
            {
                return C.QuestManager_QuestUIResponse_GetStatus(SelfPtr());
            }

            internal bool RequestSucceeded()
            {
                return RequestStatus() > 0;
            }

            internal NativeQuest AcceptedQuest()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                var quest = new NativeQuest(
                            C.QuestManager_QuestUIResponse_GetAcceptedQuest(SelfPtr()));

                if (quest.Valid())
                {
                    return quest;
                }
                else
                {
                    quest.Dispose();
                    return null;
                }
            }

            internal NativeQuestMilestone MilestoneToClaim()
            {
                if (!RequestSucceeded())
                {
                    return null;
                }

                var milestone = new NativeQuestMilestone(
                                C.QuestManager_QuestUIResponse_GetMilestoneToClaim(SelfPtr()));

                if (milestone.Valid())
                {
                    return milestone;
                }
                else
                {
                    milestone.Dispose();
                    return null;
                }
            }

            protected override void CallDispose(HandleRef selfPointer)
            {
                C.QuestManager_QuestUIResponse_Dispose(selfPointer);
            }

            internal static QuestUIResponse FromPointer(IntPtr pointer)
            {
                if (pointer.Equals(IntPtr.Zero))
                {
                    return null;
                }

                return new QuestUIResponse(pointer);
            }
        }
    }
}

#endif // (UNITY_ANDROID || UNITY_IPHONE)
