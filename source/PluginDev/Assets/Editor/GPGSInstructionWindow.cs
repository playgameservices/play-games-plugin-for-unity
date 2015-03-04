using System;
using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.Editor;

public class GPGSInstructionWindow : EditorWindow {

    private Vector2 mScrollPosition = Vector2.zero;

    void OnGUI() {
        var iosInstructions = GPGSUtil.ReadFile("Assets/Editor/ios_instructions");
        mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
        GUILayout.TextArea(iosInstructions);
        EditorGUILayout.EndScrollView();
    }
}

