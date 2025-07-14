// <copyright file="ISavedGameClient.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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

namespace GooglePlayGames.BasicApi.SavedGame
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An enum for the different strategies that can be used to resolve saved game conflicts (i.e.
    /// conflicts produced by two or more separate writes to the same saved game at once).
    /// </summary>
    /// <remarks>
    /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public enum ConflictResolutionStrategy
    {
        /// <summary>
        /// Choose which saved game should be used on the basis of which one has the longest recorded
        /// play time. In other words, in the case of a conflicting write, the saved game with the
        /// longest play time will be considered cannonical. If play time has not been provided by the
        /// developer, or in the case of two saved games with equal play times,
        /// <see cref="UseOriginal"/> will be used instead.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        UseLongestPlaytime,

        /// <summary>
        /// Choose the version of the saved game that existed before any conflicting write occurred.
        /// Consider the following case:
        /// - An initial version of a save game ("X") is written from a device ("Dev_A")
        /// - The save game X is downloaded by another device ("Dev_B").
        /// - Dev_A writes a new version of the save game to the cloud ("Y")
        /// - Dev_B does not see the new save game Y, and attempts to write a new save game ("Z").
        /// - Since Dev_B is performing a write using out of date information, a conflict is generated.
        ///
        /// In this situation, we can resolve the conflict by declaring either keeping Y as the
        /// canonical version of the saved game (i.e. choose "original" aka <see cref="UseOriginal"/>),
        /// or by overwriting it with conflicting value, Z (i.e. choose "unmerged" aka
        /// <see cref="UseUnmerged"/>).
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        UseOriginal,

        /// <summary>
        /// See the documentation for <see cref="UseOriginal"/>
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        UseUnmerged,

        /// <summary>
        /// Manual resolution, no automatic resolution is attempted.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        UseManual,

        /// <summary>
        /// The use last known good snapshot to resolve conflicts automatically.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        UseLastKnownGood,

        /// <summary>
        /// The use most recently saved snapshot to resolve conflicts automatically.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        UseMostRecentlySaved
    }

    public enum SavedGameRequestStatus
    {
        Success = 1,

        /// <summary>
        /// The request failed due to a timeout.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        TimeoutError = -1,

        /// <summary>
        /// An unexpected internal error. Check the log for error messages.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        InternalError = -2,

        /// <summary>
        /// A error related to authentication. This is probably due to the user being signed out
        /// before the request could be issued.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        AuthenticationError = -3,

        /// <summary>
        /// The request failed because it was given bad input (e.g. a filename with 200 characters).
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        BadInputError = -4
    }

    public enum SelectUIStatus
    {
        /// <summary>
        /// The user selected a saved game.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        SavedGameSelected = 1,

        /// <summary>
        /// The user closed the UI without selecting a saved game.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        UserClosedUI = 2,

        /// <summary>
        /// An unexpected internal error. Check the log for error messages.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        InternalError = -1,

        /// <summary>
        /// There was a timeout while displaying the UI.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        TimeoutError = -2,

        /// <summary>
        /// A error related to authentication. This is probably due to the user being signed out
        /// before the request could be issued.
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        AuthenticationError = -3,

        /// <summary>
        /// The request failed because it was given bad input (e.g. a filename with 200 characters).
        /// </summary>
        /// <remarks>
        /// @deprecated This enum will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        BadInputError = -4,

        UiBusy = -5
    }

    ///
    /// <summary>
    /// A delegate that is invoked when we encounter a conflict during execution of
    /// <see cref="ISavedGameClient.OpenWithAutomaticConflictResolution"/>. The caller must resolve the
    /// conflict using the passed <see cref="IConflictResolver"/>. All passed metadata is open.
    /// If <see cref="ISavedGameClient.OpenWithAutomaticConflictResolution"/> was invoked with
    /// <c>prefetchDataOnConflict</c> set to <c>true</c>, the <paramref name="originalData"/> and
    /// <paramref name="unmergedData"/> will be equal to the binary data of the "original" and
    /// "unmerged" saved game respectively (and null otherwise). Since conflict files may be generated
    /// by other clients, it is possible that neither of the passed saved games were originally written
    /// by the current device. Consequently, any conflict resolution strategy should not rely on local
    /// data that is not part of the binary data of the passed saved games - this data will not be
    /// present if conflict resolution occurs on a different device. In addition, since a given saved
    /// game may have multiple conflicts, this callback must be designed to handle multiple invocations.
    /// </summary>
    /// <remarks>
    /// @deprecated This delegate will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public delegate void ConflictCallback(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData);

    /// <summary>
    /// The main entry point for interacting with saved games. Saved games are persisted in the cloud
    /// along with several game-specific properties (<see cref="ISavedGameMetadata"/> for more
    /// information). There are several core concepts involved with saved games:
    ///
    /// <para><strong>Filenames</strong> - act as unique identifiers for saved games. Two devices
    /// performing a read or write using the same filename will end up reading or modifying the same
    /// file (i.e. filenames are not device specific).
    /// </para>
    ///
    /// <para><strong>Saved Game Metadata</strong> are represented by <see cref="ISavedGameMetadata"/>.
    /// The instances allow access to metadata properties about the underlying saved game (e.g.
    /// description). In addition, metadata functions as a handle that are required to read and
    /// manipulate saved game contents. Lastly, metadata may be "Open". Open metadata instances are
    /// required to manipulate the underlying binary data of the saved game. See method comments to
    /// determine whether a specific method requires or returns an open saved game.
    /// </para>
    ///
    /// <para><strong>Conflicts</strong> occur when multiple devices attempt to write to the same file
    /// at the same time. The saved game system guarantees that no conflicting writes will be lost or
    /// silently overwritten. Instead, they must be handled the next time the file with a conflict is
    /// Opened. Conflicts can be handled automatically (
    /// <see cref="OpenWithAutomaticConflictResolution"/>) or can be manuallyhandled by the developer
    /// (<see cref="OpenWithManualConflictResolution"/>). See the Open methods for more discussion.
    /// </para>
    ///
    /// <para>Saved games will generally be used in the following workflow:</para>
    /// <list type="number">
    /// <item><description>Determine which saved game to use (either using a hardcoded filename or
    /// ShowSelectSavedGameUI)</description></item>
    /// <item><description>Open the file using OpenWithManualConflictResolution or
    /// OpenWithAutomaticConflictResolution</description></item>
    /// <item><description>Read the binary data of the saved game using ReadBinaryData handle it
    /// as appropriate for your game.</description></item>
    /// <item><description>When you have updates, persist them in the cloud using CommitUpdate. Note
    /// that writing to the cloud is relatively expensive, and shouldn't be done frequently.
    /// </description></item>
    /// </list>
    ///
    /// <para>See online <a href="https://developers.google.com/games/services/common/concepts/savedgames">
    /// documentation for Saved Games</a> for more information.</para>
    /// </summary>
    /// <remarks>
    /// @deprecated This interface will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public interface ISavedGameClient
    {
        /// <summary>
        /// Opens the file with the indicated name and data source. If the file has an outstanding
        /// conflict, it will be resolved using the specified conflict resolution strategy. The
        /// metadata returned by this method will be "Open" - it can be used as a parameter for
        /// <see cref="CommitUpdate"/> and <see cref="ResolveConflictByChoosingMetadata"/>.
        /// </summary>
        /// <param name="filename">The name of the file to open. Filenames must consist of
        /// only non-URL reserved characters (i.e. a-z, A-Z, 0-9, or the symbols "-", ".", "_", or "~")
        /// be between 1 and 100 characters in length (inclusive).</param>
        /// <param name="source">The data source to use. <see cref="DataSource"/> for a description
        /// of the available options here.</param>
        /// <param name="resolutionStrategy">The conflict resolution that should be used if any
        /// conflicts are encountered while opening the file.
        /// <see cref="ConflictResolutionStrategy"/> for a description of these strategies.</param>
        /// <param name="callback">The callback that is invoked when this operation finishes. The
        /// returned metadata will only be non-null if the open succeeded. This callback will always
        /// execute on the game thread and the returned metadata (if any) will be "Open".</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void OpenWithAutomaticConflictResolution(string filename, DataSource source,
            ConflictResolutionStrategy resolutionStrategy,
            Action<SavedGameRequestStatus, ISavedGameMetadata> callback);

        /// <summary>
        /// Opens the file with the indicated name and data source. If there is a conflict that
        /// requires resolution, it will be resolved manually using the passed conflict callback. Once
        /// all pending conflicts are resolved, the completed callback will be invoked with the
        /// retrieved data. In the event of an error, the completed callback will be invoked with the
        /// corresponding error status. All callbacks will be executed on the game thread.
        /// </summary>
        /// <param name="filename">The name of the file to open. Filenames must consist of
        /// only non-URL reserved characters (i.e. a-z, A-Z, 0-9, or the symbols "-", ".", "_", or "~")
        /// be between 1 and 100 characters in length (inclusive).</param>
        /// <param name="source">The data source to use. <see cref="DataSource"/> for a description
        /// of the available options here.</param>
        /// <param name="prefetchDataOnConflict">If set to <c>true</c>, the data for the two
        /// conflicting files will be automatically retrieved and passed as parameters in
        /// <paramref name="conflictCallback"/>. If set to <c>false</c>, <c>null</c> binary data
        /// will be passed into <paramref name="conflictCallback"/> and the caller will have to fetch
        /// it themselves.</param>
        /// <param name="conflictCallback">The callback that will be invoked if one or more conflict is
        /// encountered while executing this method. Note that more than one conflict may be present
        /// and that this callback might be executed more than once to resolve multiple conflicts.
        /// This callback is always executed on the game thread.</param>
        /// <param name="completedCallback">The callback that is invoked when this operation finishes.
        /// The returned metadata will only be non-null if the open succeeded. If an error is
        /// encountered during conflict resolution, that error will be reflected here. This callback
        /// will always execute on the game thread and the returned metadata (if any) will be "Open".
        /// </param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void OpenWithManualConflictResolution(string filename, DataSource source,
            bool prefetchDataOnConflict, ConflictCallback conflictCallback,
            Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback);

        /// <summary>
        /// Reads the binary data of the passed saved game. The passed metadata must be opened (i.e.
        /// <see cref="ISavedGameMetadata.IsOpen"/> returns true). The callback will always be executed
        /// on the game thread.
        /// </summary>
        /// <param name="metadata">The metadata for the saved game whose binary data we want to read.
        /// This metadata must be open. If it is not open, the method will immediately fail with status
        /// <see cref="SelectUIStatus.BadInputError"/>.
        /// </param>
        /// <param name="completedCallback">The callback that is invoked when the read finishes. If the
        /// read completed without error, the passed status will be <see cref="SavedGameRequestStatus.Success"/> and the passed
        /// bytes will correspond to the binary data for the file. In the case of
        /// </param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void ReadBinaryData(ISavedGameMetadata metadata,
            Action<SavedGameRequestStatus, byte[]> completedCallback);

        /// <summary>
        /// Shows the select saved game UI with the indicated configuration. If the user selects a
        /// saved game in that UI, it will be returned in the passed callback. This metadata will be
        /// unopened and must be passed to either <see cref="OpenWithManualConflictResolution"/> or
        /// <see cref="OpenWithAutomaticConflictResolution"/> in order to retrieve the binary data.
        /// The callback will always be executed on the game thread.
        /// </summary>
        /// <param name="uiTitle">The user-visible title of the displayed selection UI.</param>
        /// <param name="maxDisplayedSavedGames">The maximum number of saved games the UI may display.
        /// This value must be greater than 0.</param>
        /// <param name="showCreateSaveUI">If set to <c>true</c>, show UI that will allow the user to
        /// create a new saved game.</param>
        /// <param name="showDeleteSaveUI">If set to <c>true</c> show UI that will allow the user to
        /// delete a saved game.</param>
        /// <param name="callback">The callback that is invoked when an error occurs or if the user
        /// finishes interacting with the UI. If the user selected a saved game, this will be passed
        /// into the callback along with the <see cref="SelectUIStatus.SavedGameSelected"/> status. This saved game
        /// will not be Open, and must be opened before it can be written to or its binary data can be
        /// read. If the user backs out of the UI without selecting a saved game, this callback will
        /// receive <see cref="UserClosedUI"/> and a null saved game. This callback will always execute
        /// on the game thread.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void ShowSelectSavedGameUI(string uiTitle, uint maxDisplayedSavedGames, bool showCreateSaveUI,
            bool showDeleteSaveUI, Action<SelectUIStatus, ISavedGameMetadata> callback);

        /// <summary>
        /// Durably commits an update to the passed saved game. When this method returns successfully,
        /// the data is durably persisted to disk and will eventually be uploaded to the cloud (in
        /// practice, this will happen very quickly unless the device does not have a network
        /// connection). If an update to the saved game has occurred after the metadata was retrieved
        /// from the cloud, this update will produce a conflict (this commonly occurs if two different
        /// devices are writing to the cloud at the same time). All conflicts must be handled the next
        /// time this saved game is opened. See <see cref="OpenWithManualConflictResolution"/> and
        /// <see cref="OpenWithAutomaticConflictResolution"/> for more information.
        /// </summary>
        /// <param name="metadata">The metadata for the saved game to update. This metadata must be
        /// Open (i.e. <see cref="ISavedGameMetadata.IsOpen"/> returns true)."/> If it is not open, the
        /// method will immediately fail with status <see cref="SelectUIStatus.BadInputError"/></param>
        /// <param name="updateForMetadata">All updates that should be applied to the saved game
        /// metadata.</param>
        /// <param name="updatedBinaryData">The new binary content of the saved game</param>
        /// <param name="callback">The callback that is invoked when this operation finishes.
        /// The returned metadata will only be non-null if the commit succeeded. If an error is
        /// encountered during conflict resolution, that error will be reflected here. This callback
        /// will always execute on the game thread and the returned metadata (if any) will NOT be
        /// "Open" (i.e. commiting an update closes the metadata).</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void CommitUpdate(ISavedGameMetadata metadata, SavedGameMetadataUpdate updateForMetadata,
            byte[] updatedBinaryData, Action<SavedGameRequestStatus, ISavedGameMetadata> callback);

        /// <summary>
        /// Returns the metadata for all known saved games for this game. All returned saved games are
        /// not open, and must be opened before they can be used for writes or binary data reads. The
        /// callback will always occur on the game thread.
        /// </summary>
        /// <param name="source">The data source to use. <see cref="DataSource"/> for a description
        /// of the available options here.</param>
        /// <param name="callback">The callback that is invoked when this operation finishes.
        /// The returned metadata will only be non-empty if the commit succeeded. If an error is
        /// encountered during the fetch, that error will be reflected here. This callback
        /// will always execute on the game thread and the returned metadata (if any) will NOT be
        /// "Open".</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void FetchAllSavedGames(DataSource source,
            Action<SavedGameRequestStatus, List<ISavedGameMetadata>> callback);

        /// <summary>
        /// Delete the specified snapshot.
        /// This will delete the data of the snapshot locally and on the server.
        /// </summary>
        /// <param name="metadata">the saved game metadata identifying the data to
        /// delete.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void Delete(ISavedGameMetadata metadata);
    }

    /// <summary>
    /// An interface that allows developers to resolve metadata conflicts that may be encountered while
    /// opening saved games.
    /// </summary>
    /// <remarks>
    /// @deprecated This interface will be removed in the future in favor of Unity Games V2 Plugin.
    /// </remarks>
    public interface IConflictResolver
    {
        /// <summary>
        /// Resolves the conflict by choosing the passed metadata to be canonical. The passed metadata
        /// must be one of the two instances passed as parameters into <see cref="ConflictCallback"/> -
        /// this instance will be kept as the cannonical value in the cloud.
        /// </summary>
        /// <param name="chosenMetadata">The chosen metadata. This metadata must be open. If it is not
        /// open, the invokation of <see cref="NativeSavedGameClient.OpenWithManualConflictResolution"/> that produced this
        /// ConflictResolver will immediately fail with <see cref="SelectUIStatus.BadInputError"/>.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void ChooseMetadata(ISavedGameMetadata chosenMetadata);

        /// <summary>
        /// Resolves the conflict and updates the data.
        /// </summary>
        /// <param name="chosenMetadata">Metadata for the chosen version.  This is either the
        /// original or unmerged metadata provided when the callback is invoked.</param>
        /// <param name="metadataUpdate">Metadata update, same as when committing changes.</param>
        /// <param name="updatedData">Updated data to use when resolving the conflict.</param>
        /// <remarks>
        /// @deprecated This method will be removed in the future in favor of Unity Games V2 Plugin.
        /// </remarks>
        void ResolveConflict(ISavedGameMetadata chosenMetadata, SavedGameMetadataUpdate metadataUpdate,
            byte[] updatedData);
    }
}