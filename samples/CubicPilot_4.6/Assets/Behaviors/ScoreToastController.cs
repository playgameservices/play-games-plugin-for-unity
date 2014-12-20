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

public class ScoreToastController : MonoBehaviour {
    public GUISkin GuiSkin;
    public int Value = 50;
    public Color ToastColor = Color.black;

    void Update () {

    }

    void OnGUI() {
        GUI.skin = GuiSkin;
        Vector3 pos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Gu.SetColor(ToastColor);
        Gu.Label((int)pos.x, Screen.height - (int)pos.y, Gu.Dim(GameConsts.ScoreToastFontSize),
            Value.ToString());
    }
}
