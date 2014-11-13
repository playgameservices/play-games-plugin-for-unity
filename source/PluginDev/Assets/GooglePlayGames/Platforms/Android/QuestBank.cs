/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apqueste License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apqueste.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.Android {
	internal class QuestBank {
		Dictionary<string, Quest> mQuests = new Dictionary<string, Quest>();
		
		internal QuestBank () {
		}
		
		internal void ProcessBuffer(AndroidJavaObject questBuffer) {
			int i, count;
			
			Logger.d("QuestBank: processing quest buffer given as Java object.");
			
			if (questBuffer == null) {
				Logger.w("QuestBank: given buffer was null. Ignoring.");
				return;
			}
			
			count = questBuffer.Call<int>("getCount");
			Logger.d("QuestBank: buffer contains " + count + " quests.");
			
			for (i = 0; i < count; ++i) {
				Logger.d("QuestBank: processing quest #" + i);
				Quest quest = new Quest();
				AndroidJavaObject questObj = questBuffer.Call<AndroidJavaObject>("get", i);
				
				if (questObj == null) {
					Logger.w("Quest #" + i + " was null. Ignoring.");
					continue;
				}
				
				quest.Id = questObj.Call<string>("getQuestId");
				quest.Name = questObj.Call<string>("getName");
				quest.Description = questObj.Call<string>("getDescription");
				
				quest.StartTimestamp = questObj.Call<long>("getStartTimestamp");
				quest.AcceptedTimestamp = questObj.Call<long>("getAcceptedTimestamp");
				quest.EndTimestamp = questObj.Call<long>("getEndTimestamp");
				
				quest.State = questObj.Call<int>("getState");
				
				AndroidJavaObject mileObj = JavaUtil.CallNullSafeObjectMethod(questObj, "getCurrentMilestone");
				
				if( mileObj != null ) {
					Milestone milestone = new Milestone();
					
					milestone.Id = mileObj.Call<string>("getMilestoneId");
					milestone.EventId = mileObj.Call<string>("getEventId");
					
					milestone.CurrentProgress = mileObj.Call<long>("getCurrentProgress");
					milestone.TargetProgress = mileObj.Call<long>("getTargetProgress");
					milestone.CompletionRewardData = mileObj.Call<byte[]>("getCompletionRewardData");
					
					milestone.State = mileObj.Call<int>("getState");
					
					quest.Milestone = milestone;
				}
				
				Logger.d("QuestBank: processed: " + quest.ToString());
				
				if( quest.Milestone != null ) {
					Logger.d("QuestBank: milestone: " + quest.Milestone.ToString());
				}
				
				if (quest.Id != null && quest.Id.Length > 0) {
					mQuests[quest.Id] = quest;
				} else {
					Logger.w("Quest w/ missing ID received. Ignoring.");
				}
			}
			
			Logger.d("QuestBank: bank now contains " + mQuests.Count + " entries.");
		}
		
		internal Quest GetQuest(string id) {
			if (mQuests.ContainsKey(id)) {
				return mQuests[id];
			} else {
				Logger.w("Quest ID not found in bank: id " + id);
				return null;
			}
		}
		
		internal List<Quest> GetQuests() {
			List<Quest> list = new List<Quest>();
			foreach (Quest a in mQuests.Values) {
				list.Add(a);
			}
			return list;
		}
	}
}

#endif


