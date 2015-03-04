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

public class BaseGui : MonoBehaviour {
    protected WidgetConfig CenterLabelCfg = new WidgetConfig(0.0f, 0.0f, 0.9f, 0.9f, 60, "Please wait...");
    protected WidgetConfig UpButtonCfg = new WidgetConfig(WidgetConfig.WidgetAnchor.Top,
        -0.3f, 0.1f, 0.35f, 0.15f, TextAnchor.MiddleCenter, 50, "<<");

    public GUISkin GuiSkin;

    public class WidgetConfig {
        public float CenterX, CenterY, Width, Height;
        public TextAnchor ContentAnchor;
        public string Text;
        public float FontSize;
        public WidgetAnchor Anchor;

        public enum WidgetAnchor {
            Center = 0,
            Top = 1,
            Bottom = 2
        };

        public WidgetConfig(WidgetAnchor widgetAnchor, float centerX, float centerY,
                float width, float height, TextAnchor contentAnchor, float fontSize,
                string text) {
            CenterX = centerX;
            CenterY = centerY;
            Width = width;
            Height = height;
            ContentAnchor = contentAnchor;
            Text = text;
            FontSize = fontSize;
            Anchor = widgetAnchor;
        }

        public WidgetConfig(float centerX, float centerY,
            float width, float height, float fontSize, string text) : this(WidgetAnchor.Center, centerX,
                centerY, width, height, TextAnchor.MiddleCenter, fontSize, text) {}
    }

    bool mStandBy = false;
    string mStandByMessage = "";


    // convert our device-independent logical coordinate system to the screen coordinate system
    protected int ScreenY(float y) {
        return (int)(Screen.height / 2 + y * Screen.width);
    }
    protected int ScreenX(float x) {
        return (int)(Screen.width / 2 + x * Screen.width);
    }
    protected int ScreenDim(float dim) {
        return (int)(dim * Screen.width);
    }

    protected float GetScreenHeight() {
        return Screen.height / (float) Screen.width;
    }

    protected float GetScreenBottom() {
        return GetScreenHeight() / 2;
    }

    protected float GetScreenTop() {
        return -GetScreenHeight() / 2;
    }

    protected Rect CenteredRect(float centerX, float centerY, float width, float height) {
        return new Rect(ScreenX(centerX - width/2), ScreenY(centerY - height/2), ScreenDim(width), ScreenDim(height));
    }

    protected virtual void OnGUI() {
        GUI.skin = GuiSkin;
        if (mStandBy) {
            GuiLabel_internal(CenterLabelCfg, mStandByMessage);
        }
        DoGUI();
    }

    protected virtual void DoGUI() {
    }

    protected void GuiLabel(WidgetConfig config) {
        GuiLabel(config, null);
    }

    protected void GuiProgressBar(WidgetConfig cfg, Texture fgTexture, Texture bgTexture, float percent) {
        Rect r = GetWidgetRect(cfg);
        GUI.DrawTexture(r, bgTexture);
        r.width = r.width * percent * 0.01f;
        GUI.DrawTexture(r, fgTexture);
    }

    protected int ScreenFontSize(float fontSize) {
        return ScreenDim(fontSize * 0.001f);
    }

    private Rect GetWidgetRect(WidgetConfig cfg) {
        float centerY = cfg.Anchor == WidgetConfig.WidgetAnchor.Top ? GetScreenTop() + cfg.CenterY :
                cfg.Anchor == WidgetConfig.WidgetAnchor.Bottom ? GetScreenBottom () + cfg.CenterY :
                cfg.CenterY;

        return CenteredRect(cfg.CenterX, centerY, cfg.Width, cfg.Height);
    }

    private void GuiLabel_internal(WidgetConfig config, string overrideText) {
        GUI.skin.label.alignment = config.ContentAnchor;
        GUI.skin.label.fontSize = ScreenFontSize(config.FontSize);
        GUI.Label(GetWidgetRect(config), overrideText == null ? config.Text : overrideText);
    }

    protected void GuiLabel(WidgetConfig config, string overrideText) {
        if (!mStandBy) {
            GuiLabel_internal(config, overrideText);
        }
    }

    protected bool GuiButton(WidgetConfig config, string overrideText) {
        if (!mStandBy) {
            GUI.skin.button.alignment = config.ContentAnchor;
            GUI.skin.button.fontSize = ScreenFontSize(config.FontSize);
            return GUI.Button(GetWidgetRect(config), overrideText == null ? config.Text : overrideText);
        } else {
            return false;
        }
    }

    protected bool GuiButton(WidgetConfig config) {
        return GuiButton(config, null);
    }

    public void MakeActive() {
        foreach (Component comp in gameObject.GetComponents(typeof(BaseGui))) {
            if (comp is BaseGui) {
                BaseGui baseGui = (BaseGui) comp;
                if (baseGui.enabled && baseGui != this) {
                    baseGui.enabled = false;
                }
            }
        }
        this.enabled = true;
    }

    protected void SetStandBy(string message) {
        mStandBy = true;
        mStandByMessage = message;
    }

    protected void EndStandBy() {
        mStandBy = false;
    }
}
