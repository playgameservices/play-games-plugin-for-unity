/*
 * Copyright (C) 2013 Google Inc.
 *
 * Licensed under the Apevee License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apevee.org/licenses/LICENSE-2.0
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

using Event = GooglePlayGames.BasicApi.Event;

namespace GooglePlayGames.Android {
	internal class EventBank {
		Dictionary<string, Event> mEvents = new Dictionary<string, Event>();
		
		internal EventBank () {
		}
		
		internal void ProcessBuffer(AndroidJavaObject eveBuffer) {
			int i, count;
			
			Logger.d("EventBank: processing event buffer given as Java object.");
			
			if (eveBuffer == null) {
				Logger.w("EventBank: given buffer was null. Ignoring.");
				return;
			}
			
			count = eveBuffer.Call<int>("getCount");
			Logger.d("EventBank: buffer contains " + count + " events.");
			
			for (i = 0; i < count; ++i) {
				Logger.d("EventBank: processing event #" + i);
				Event eve = new Event();
				AndroidJavaObject eveObj = eveBuffer.Call<AndroidJavaObject>("get", i);
				
				if (eveObj == null) {
					Logger.w("Event #" + i + " was null. Ignoring.");
					continue;
				}
				
				eve.Id = eveObj.Call<string>("getEventId");
				eve.Value = eveObj.Call<long>("getValue");
				eve.IsVisible = eveObj.Call<bool>("isVisible");
				eve.Name = eveObj.Call<string>("getName");
				eve.Description = eveObj.Call<string>("getDescription");
				
				Logger.d("EventBank: processed: " + eve.ToString());
				if (eve.Id != null && eve.Id.Length > 0) {
					mEvents[eve.Id] = eve;
				} else {
					Logger.w("Event w/ missing ID received. Ignoring.");
				}
			}
			
			Logger.d("EventBank: bank now contains " + mEvents.Count + " entries.");
		}
		
		internal Event GetEvent(string id) {
			if (mEvents.ContainsKey(id)) {
				return mEvents[id];
			} else {
				Logger.w("Event ID not found in bank: id " + id);
				return null;
			}
		}
		
		internal List<Event> GetEvents() {
			List<Event> list = new List<Event>();
			foreach (Event a in mEvents.Values) {
				list.Add(a);
			}
			return list;
		}
	}
}

#endif

