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

#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.Android {
    internal class AchievementBank {
        Dictionary<string, Achievement> mAchievements = new Dictionary<string, Achievement>();

        internal AchievementBank () {
        }

        internal void ProcessBuffer(AndroidJavaObject achBuffer) {
            int i, count;

            Logger.d("AchievementBank: processing achievement buffer given as Java object.");

            if (achBuffer == null) {
                Logger.w("AchievementBank: given buffer was null. Ignoring.");
                return;
            }

            count = achBuffer.Call<int>("getCount");
            Logger.d("AchievementBank: buffer contains " + count + " achievements.");

            for (i = 0; i < count; ++i) {
                Logger.d("AchievementBank: processing achievement #" + i);
                Achievement ach = new Achievement();
                AndroidJavaObject achObj = achBuffer.Call<AndroidJavaObject>("get", i);

                if (achObj == null) {
                    Logger.w("Achievement #" + i + " was null. Ignoring.");
                    continue;
                }

                ach.Id = achObj.Call<string>("getAchievementId");
                ach.IsIncremental = achObj.Call<int>("getType") == JavaConsts.TYPE_INCREMENTAL;
                int state = achObj.Call<int>("getState");
                ach.IsRevealed = state != JavaConsts.STATE_HIDDEN;
                ach.IsUnlocked = state == JavaConsts.STATE_UNLOCKED;
                ach.Name = achObj.Call<string>("getName");
                ach.Description = achObj.Call<string>("getDescription");
                if (ach.IsIncremental) {
                    ach.CurrentSteps = achObj.Call<int>("getCurrentSteps");
                    ach.TotalSteps = achObj.Call<int>("getTotalSteps");
                }
                Logger.d("AchievementBank: processed: " + ach.ToString());
                if (ach.Id != null && ach.Id.Length > 0) {
                    mAchievements[ach.Id] = ach;
                } else {
                    Logger.w("Achievement w/ missing ID received. Ignoring.");
                }
            }

            Logger.d("AchievementBank: bank now contains " + mAchievements.Count + " entries.");
        }

        internal Achievement GetAchievement(string id) {
            if (mAchievements.ContainsKey(id)) {
                return mAchievements[id];
            } else {
                Logger.w("Achievement ID not found in bank: id " + id);
                return null;
            }
        }

        internal List<Achievement> GetAchievements() {
            List<Achievement> list = new List<Achievement>();
            foreach (Achievement a in mAchievements.Values) {
                list.Add(a);
            }
            return list;
        }
    }
}

#endif
