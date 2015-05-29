using System;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using GooglePlayGames.Editor;

namespace GooglePlayGames {

    public class GPGSInstructionWindow : EditorWindow {

        private Vector2 mScrollPosition = Vector2.zero;

        void OnGUI() {
            var iosInstructions = GPGSUtil.ReadFile("Assets/GooglePlayGames/Editor/ios_instructions");
            mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
            GUILayout.TextArea(iosInstructions);
            EditorGUILayout.EndScrollView();
        }
    }
}

