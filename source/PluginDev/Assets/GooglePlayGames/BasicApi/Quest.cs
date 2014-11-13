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

namespace GooglePlayGames.BasicApi {
	public class Quest {
		public string Id = "";
		public string Name = "";
		public string Description = "";
		public long StartTimestamp = 0;
		public long AcceptedTimestamp = 0;
		public long EndTimestamp = 0;
		public int State = QuestsConsts.STATE_UPCOMING;
		public GooglePlayGames.BasicApi.Milestone Milestone = null;
		
		public override string ToString () {
			return string.Format("[Event] id={0}, name={1}, desc={2}, state={3}, " +
								 "start_time={4}, accepted_time={5}, end_time={6}",
			                     Id, Name, Description, State, 
			                     StartTimestamp, AcceptedTimestamp, EndTimestamp);
		}
		
		public Quest() {
		}
	}
}

