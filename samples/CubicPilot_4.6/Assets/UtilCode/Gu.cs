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

// GUI Utils
public class Gu {
    public static void SetColor(Color c) {
        GUI.skin.label.normal.textColor = c;
        GUI.skin.button.normal.textColor = c;
    }

    public static void SetColor(float r, float g, float b, float a) {
        SetColor(new Color(r, g, b, a));
    }

    public static void SetColor(float r, float g, float b) {
        SetColor(r, g, b, 1.0f);
    }

    public static void Label(int x, int y, int fontSize, string text, bool center) {
        const int largeVal = 2000;
        GUI.skin.label.fontSize = fontSize;
        if (center) {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            // center text on x,y:
            GUI.Label(new Rect(x - largeVal, y - largeVal, 2*largeVal, 2*largeVal),
                text);
        } else {
            // top left is at x,y
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(x, y, 2*largeVal, 2*largeVal), text);
        }
    }

    public static void Label(int x, int y, int fontSize, string text) {
        Label(x, y, fontSize, text, true);
    }

    public static bool Button(int x, int y, int w, int h, Texture2D tex) {
        return GUI.Button(new Rect(x,y,w,h), tex);
    }

    public static bool Button(int x, int y, int w, int h, int fontSize, string text, GUIStyle style) {
        GUI.skin.button.fontSize = fontSize;
        if (style != null) {
            style.fontSize = fontSize;
            return GUI.Button(new Rect(x,y,w,h), text, style);
        } else {
            return GUI.Button(new Rect(x,y,w,h), text);
        }
    }

    public static bool Button(int x, int y, int w, int h, int fontSize, string text) {
        return Button(x, y, w, h, fontSize, text, null);
    }

    public static int Dim(float units) {
        return (int)(units * Screen.height / 1000);
    }

    public static int Left(float units) {
        return Dim (units);
    }

    public static int Right(float units) {
        return Screen.width - Dim(units);
    }

    public static int Center(float units) {
        return Screen.width / 2 + Dim(units);
    }

    public static int Middle(float units) {
        return Screen.height / 2 + Dim(units);
    }

    public static int Top(float units) { return Dim(units); }

    public static int Bottom(float units) {
        return Screen.height - Dim(units);
    }

    public static Vector2 Measure(int fontSize, string s) {
        GUI.skin.label.fontSize = fontSize;
        return GUI.skin.customStyles[0].CalcSize(new GUIContent(s));
    }
}
