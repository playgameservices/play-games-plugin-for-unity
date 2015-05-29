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

#if (UNITY_ANDROID || UNITY_IPHONE)
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.OurUtils;
using System;
using System.Collections.Generic;

namespace GooglePlayGames.Native {
internal class UnsupportedSavedGamesClient : ISavedGameClient {

    private readonly string mMessage;

    public UnsupportedSavedGamesClient(string message) {
        this.mMessage = Misc.CheckNotNull(message);
    }

    public void OpenWithAutomaticConflictResolution(string filename, DataSource source,
        ConflictResolutionStrategy resolutionStrategy,
        Action<SavedGameRequestStatus, ISavedGameMetadata> callback) {
        throw new NotImplementedException(mMessage);
    }

    public void OpenWithManualConflictResolution(string filename, DataSource source,
        bool prefetchDataOnConflict, ConflictCallback conflictCallback,
        Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback) {
        throw new NotImplementedException(mMessage);
    }

    public void ReadBinaryData(ISavedGameMetadata metadata,
        Action<SavedGameRequestStatus, byte[]> completedCallback) {
        throw new NotImplementedException(mMessage);
    }

    public void ShowSelectSavedGameUI(string uiTitle, uint maxDisplayedSavedGames,
        bool showCreateSaveUI, bool showDeleteSaveUI, Action<SelectUIStatus, ISavedGameMetadata> callback) {
        throw new NotImplementedException(mMessage);
    }

    public void CommitUpdate(ISavedGameMetadata metadata, SavedGameMetadataUpdate updateForMetadata,
        byte[] updatedBinaryData, Action<SavedGameRequestStatus, ISavedGameMetadata> callback) {
        throw new NotImplementedException(mMessage);
    }

    public void FetchAllSavedGames(DataSource source, Action<SavedGameRequestStatus,
        List<ISavedGameMetadata>> callback) {
        throw new NotImplementedException(mMessage);
    }
}
}
#endif
