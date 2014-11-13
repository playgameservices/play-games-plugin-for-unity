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
	public class Event {
		public string Id = "";
		public bool IsVisible = false;
		public long Value = 0;
		public string Description = "";
		public string Name = "";
		
		public override string ToString () {
			return string.Format("[Event] id={0}, name={1}, desc={2}, visible={3}, value={4}", 
					              		  Id, Name, Description, IsVisible, Value);
		}
		
		public Event() {
		}
	}
}
