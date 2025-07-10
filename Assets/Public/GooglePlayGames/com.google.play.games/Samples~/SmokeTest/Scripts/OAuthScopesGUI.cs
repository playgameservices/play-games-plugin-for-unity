// <copyright file="MainGui.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

// <copyright file="MainGui.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not really use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

namespace SmokeTest
{
    using UnityEngine;
    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using System.Collections.Generic;

    public class OAuthScopesGUI : MonoBehaviour
    {
        private MainGui mOwner;
        private string mStatus;

        // Boolean variables for the checkboxes
        private bool openIdChecked = false;
        private bool profileChecked = false;
        private bool emailChecked = false;
        private bool forceRefreshTokenChecked = false;

        private List<AuthScope> selectedScopes = new List<AuthScope>();

        internal OAuthScopesGUI(MainGui owner)
        {
            mOwner = owner;
            mStatus = "";
        }

        internal void OnGUI()
        {
            float height = Screen.height / 11f;
            GUILayout.BeginVertical(GUILayout.Height(Screen.height), GUILayout.Width(Screen.width));
            GUILayout.Label("SmokeTest: OAuthScopes", GUILayout.Height(height));

            // "Select scopes to request" label
            GUILayout.Label("Select scopes to request", GUILayout.Height(height * 0.5f));

            // Checkboxes
            openIdChecked = GUILayout.Toggle(openIdChecked, "openid_scope", GUILayout.Height(height * 0.5f));
            profileChecked = GUILayout.Toggle(profileChecked, "profile_scope", GUILayout.Height(height * 0.5f));
            emailChecked = GUILayout.Toggle(emailChecked, "email_scope", GUILayout.Height(height * 0.5f));

            // "is_force_refresh_token_enabled" label
            GUILayout.Label("Is force refresh token enabled", GUILayout.Height(height * 0.5f));

            // Force refresh token checkbox
            forceRefreshTokenChecked = GUILayout.Toggle(forceRefreshTokenChecked, "Force refresh token", GUILayout.Height(height * 0.5f));

            GUILayout.BeginHorizontal(GUILayout.Height(height));
            if (GUILayout.Button("Request Server Side Access With Scopes", GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                GetRequestServerSideAccessWithScopes();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(50f);
            GUILayout.BeginHorizontal(GUILayout.Height(height));

            if (GUILayout.Button("Back", GUILayout.ExpandHeight(true),
                GUILayout.Height(height), GUILayout.ExpandWidth(true)))
            {
                mOwner.SetUI(MainGui.Ui.Main);
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(mStatus);
            GUILayout.EndVertical();
        }

        void SetStandBy(string msg)
        {
            mStatus = msg;
        }

        void EndStandBy()
        {
            mStatus += " (Done!)";
        }

        internal void ShowEffect(bool success)
        {
            Camera.main.backgroundColor =
                success ? new Color(0.0f, 0.0f, 0.8f, 1.0f) : new Color(0.8f, 0.0f, 0.0f, 1.0f);
        }

        internal void GetRequestServerSideAccessWithScopes()
        {
            SetStandBy("Fetching server side access......");


            selectedScopes.Clear();

            if (openIdChecked)
            {
                selectedScopes.Add(AuthScope.OPEN_ID);
            }
            if (profileChecked)
            {
                selectedScopes.Add(AuthScope.PROFILE);
            }
            if (emailChecked)
            {
                selectedScopes.Add(AuthScope.EMAIL);
            }

            string selectedScopesText = selectedScopes.Count > 0 ? string.Join(", ", selectedScopes.ToArray()) : "None";
            string forceRefreshText = forceRefreshTokenChecked ? "Enabled" : "Disabled";

            mStatus = $"Selected scopes: {selectedScopes.Count} ({selectedScopesText})\nForce Refresh Token: {forceRefreshText}";

            PlayGamesPlatform.Instance.RequestServerSideAccess(/* forceRefreshToken= */ forceRefreshTokenChecked, selectedScopes,
                (AuthResponse authResponse) =>
                {
                    EndStandBy();
                    Debug.Log("Auth code fetched successfully");
                    mStatus = authResponse.ToString();
                }
            );
        }
    }
}