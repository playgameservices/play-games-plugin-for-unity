/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

// TODO: Update on event change
namespace GooglePlayGames.BasicApi {
	public class Milestone {
		public string Id = "";
		public string EventId = "";
		public long CurrentProgress = 0;
		public long TargetProgress  = 0;
		public int State = QuestsConsts.STATE_NOT_STARTED;
		public byte[] CompletionRewardData = null;
		
		public override string ToString () {
			return string.Format("[Event] id={0}, event_id={1}, state={2}, " +
								 "current_progress={3}, target_progress={4}, reward={5}",
			                     Id, EventId, State, 
			                     CurrentProgress, TargetProgress, CompletionRewardData);
		}
		
		public Milestone() {
		}
	}
}
