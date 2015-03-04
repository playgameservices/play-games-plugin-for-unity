/*
 * Copyright (C) 2014 Google Inc.
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GooglePlayGames.BasicApi.Multiplayer;

public class Util {
    public static void MakeVisible(GameObject obj, bool visible) {
        if (obj == null) {
            return;
        }
        if (obj.renderer != null) {
            obj.renderer.enabled = visible;
        }
        int i;
        for (i = 0; i < obj.transform.childCount; i++) {
            GameObject child = obj.transform.GetChild(i).gameObject;
            MakeVisible(child, visible);
        }
    }

    public static Participant GetOpponent(TurnBasedMatch match) {
    	foreach (Participant p in match.Participants) {
    		if (!p.ParticipantId.Equals(match.SelfParticipantId)) {
    			return p;
    		}
    	}
    	return null;
    }

    public static string GetOpponentName(TurnBasedMatch match) {
    	Participant p = GetOpponent(match);
    	return p == null ? "(anonymous)" : p.DisplayName;
    }
}
