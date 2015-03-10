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
#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.OurUtils;
using GooglePlayGames.Native.PInvoke;

using Types = GooglePlayGames.Native.Cwrapper.Types;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;

namespace GooglePlayGames.Native {
internal class NativeSavedGameClient : ISavedGameClient {

    // Regex for a valid filename. Valid file names are between 1 and 100 characters (inclusive)
    // and only include URL-safe characters: a-z, A-Z, 0-9, or the symbols "-", ".", "_", or "~".
    // This regex is guarded by \A and \Z which guarantee that the entire string matches this
    // regex. If these were omitted, then illegal strings containing legal subsequences would be
    // allowed (since the regex would match those subsequences).
    private static readonly Regex ValidFilenameRegex = new Regex(@"\A[a-zA-Z0-9-._~]{1,100}\Z");

    private readonly SnapshotManager mSnapshotManager;

    internal NativeSavedGameClient(SnapshotManager manager) {
        this.mSnapshotManager = Misc.CheckNotNull(manager);
    }

    public void OpenWithAutomaticConflictResolution(string filename, DataSource source,
        ConflictResolutionStrategy resolutionStrategy,
        Action<SavedGameRequestStatus, ISavedGameMetadata> callback) {
        Misc.CheckNotNull(filename);
        Misc.CheckNotNull(callback);

        callback = ToOnGameThread(callback);

        if (!IsValidFilename(filename)) {
            Logger.e("Received invalid filename: " + filename);
            callback(SavedGameRequestStatus.BadInputError, null);
            return;
        }

        OpenWithManualConflictResolution(
            filename, source, false,
            (resolver, original, originalData, unmerged, unmergedData) => {
                switch (resolutionStrategy) {
                    case ConflictResolutionStrategy.UseOriginal:
                        resolver.ChooseMetadata(original);
                        return;
                    case ConflictResolutionStrategy.UseUnmerged:
                        resolver.ChooseMetadata(unmerged);
                        return;
                    case ConflictResolutionStrategy.UseLongestPlaytime:
                        if (original.TotalTimePlayed >= unmerged.TotalTimePlayed) {
                            resolver.ChooseMetadata(original);
                        } else {
                            resolver.ChooseMetadata(unmerged);
                        }
                        return;
                    default:
                        Logger.e("Unhandled strategy " + resolutionStrategy);
                        callback(SavedGameRequestStatus.InternalError, null);
                        return;
                }
            },
            callback
        );
    }

    /// <summary>
    /// A helper class that encapsulates the state around resolving a file conflict. It holds all
    /// the state that is necessary to invoke <see cref="SnapshotManager.Resolve"/> as well as a
    /// callback that will re-attempt to open the file after the resolution concludes.
    /// </summary>
    private class NativeConflictResolver : IConflictResolver {
        private readonly SnapshotManager mManager;
        private readonly string mConflictId;
        private readonly NativeSnapshotMetadata mOriginal;
        private readonly NativeSnapshotMetadata mUnmerged;
        private readonly Action<SavedGameRequestStatus, ISavedGameMetadata> mCompleteCallback;
        private readonly Action mRetryFileOpen;

        internal NativeConflictResolver(SnapshotManager manager, string conflictId,
            NativeSnapshotMetadata original, NativeSnapshotMetadata unmerged,
            Action<SavedGameRequestStatus, ISavedGameMetadata> completeCallback, Action retryOpen) {
            this.mManager = Misc.CheckNotNull(manager);
            this.mConflictId = Misc.CheckNotNull(conflictId);
            this.mOriginal = Misc.CheckNotNull(original);
            this.mUnmerged = Misc.CheckNotNull(unmerged);
            this.mCompleteCallback = Misc.CheckNotNull(completeCallback);
            this.mRetryFileOpen = Misc.CheckNotNull(retryOpen);
        }

        public void ChooseMetadata(ISavedGameMetadata chosenMetadata) {
            NativeSnapshotMetadata convertedMetadata = chosenMetadata as NativeSnapshotMetadata;

            if (convertedMetadata != mOriginal && convertedMetadata != mUnmerged) {
                Logger.e("Caller attempted to choose a version of the metadata that was not part " +
                    "of the conflict");
                mCompleteCallback(SavedGameRequestStatus.BadInputError, null);
                return;
            }

            mManager.Resolve(convertedMetadata,
                new NativeSnapshotMetadataChange.Builder().Build(), // Construct an empty update
                mConflictId,
                response => {
                    // If the resolution didn't succeed, propagate the failure to the client.
                    if (!response.RequestSucceeded()) {
                        mCompleteCallback(AsRequestStatus(response.ResponseStatus()), null);
                        return;
                    }

                    // Otherwise, retry opening the file.
                    mRetryFileOpen();
                });
        }
    }

    private ConflictCallback ToOnGameThread(ConflictCallback conflictCallback) {
        return (resolver, original, originalData, unmerged, unmergedData) => {
            Logger.d("Invoking conflict callback");
            PlayGamesHelperObject.RunOnGameThread(() =>
                conflictCallback(resolver, original, originalData, unmerged, unmergedData));
        };
    }

    public void OpenWithManualConflictResolution(string filename, DataSource source,
        bool prefetchDataOnConflict, ConflictCallback conflictCallback,
        Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback) {
        Misc.CheckNotNull(filename);
        Misc.CheckNotNull(conflictCallback);
        Misc.CheckNotNull(completedCallback);

        conflictCallback = ToOnGameThread(conflictCallback);
        completedCallback = ToOnGameThread(completedCallback);

        if (!IsValidFilename(filename)) {
            Logger.e("Received invalid filename: " + filename);
            completedCallback(SavedGameRequestStatus.BadInputError, null);
            return;
        }

        InternalManualOpen(filename, source, prefetchDataOnConflict, conflictCallback,
            completedCallback);
    }

    private void InternalManualOpen(string filename, DataSource source,
        bool prefetchDataOnConflict, ConflictCallback conflictCallback,
        Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback) {

        mSnapshotManager.Open(filename, AsDataSource(source), Types.SnapshotConflictPolicy.MANUAL,
            response => {
                if (!response.RequestSucceeded()) {
                    completedCallback(AsRequestStatus(response.ResponseStatus()), null);
                } else if(response.ResponseStatus() == Status.SnapshotOpenStatus.VALID) {
                    completedCallback(SavedGameRequestStatus.Success, response.Data());
                } else if(response.ResponseStatus() ==
                    Status.SnapshotOpenStatus.VALID_WITH_CONFLICT) {
                    // If we get here, manual conflict resolution is required.
                    NativeSnapshotMetadata original = response.ConflictOriginal();
                    NativeSnapshotMetadata unmerged = response.ConflictUnmerged();

                    // Instantiate the conflict resolver. Note that the retry callback closes over
                    // all the parameters we need to retry the open attempt. Once the conflict is
                    // resolved by invoking the appropriate resolution method on
                    // NativeConflictResolver, the resolver will invoke this callback, which will
                    // result in this method being re-executed. This recursion will continue until
                    // all conflicts are resolved or an error occurs.
                    NativeConflictResolver resolver = new NativeConflictResolver(
                        mSnapshotManager,
                        response.ConflictId(),
                        original,
                        unmerged,
                        completedCallback,
                        () => InternalManualOpen(filename, source, prefetchDataOnConflict,
                            conflictCallback, completedCallback)
                    );

                    // If we don't have to pre-fetch the saved games' binary data, we can
                    // immediately invoke the conflict callback. Note that this callback is
                    // constructed to execute on the game thread in
                    // OpenWithManualConflictResolution.
                    if (!prefetchDataOnConflict) {
                        conflictCallback(resolver, original, null, unmerged, null);
                        return;
                    }

                    // If we have to prefetch the data, we delegate invoking the conflict resolution
                    // callback to the joiner instance (once both callbacks resolve, the joiner will
                    // invoke the lambda that we declare here, using the fetched data).
                    Prefetcher joiner = new Prefetcher((originalData, unmergedData) =>
                        conflictCallback(resolver, original, originalData, unmerged, unmergedData),
                        completedCallback);

                    // Kick off the read calls.
                    mSnapshotManager.Read(original, joiner.OnOriginalDataRead);
                    mSnapshotManager.Read(unmerged, joiner.OnUnmergedDataRead);
                } else {
                    Logger.e("Unhandled response status");
                    completedCallback(SavedGameRequestStatus.InternalError, null);
                }
            });
    }

    public void ReadBinaryData(ISavedGameMetadata metadata,
        Action<SavedGameRequestStatus, byte[]> completedCallback) {
        Misc.CheckNotNull(metadata);
        Misc.CheckNotNull(completedCallback);
        completedCallback = ToOnGameThread(completedCallback);

        NativeSnapshotMetadata convertedMetadata = metadata as NativeSnapshotMetadata;

        if (convertedMetadata == null) {
            Logger.e("Encountered metadata that was not generated by this ISavedGameClient");
            completedCallback(SavedGameRequestStatus.BadInputError, null);
            return;
        }

        if (!convertedMetadata.IsOpen) {
            Logger.e("This method requires an open ISavedGameMetadata.");
            completedCallback(SavedGameRequestStatus.BadInputError, null);
            return;
        }

        mSnapshotManager.Read(convertedMetadata,
            response => {
                if (!response.RequestSucceeded()) {
                    completedCallback(AsRequestStatus(response.ResponseStatus()), null);
                } else {
                    completedCallback(SavedGameRequestStatus.Success, response.Data());
                }
            }
        );
    }

    public void ShowSelectSavedGameUI(string uiTitle, uint maxDisplayedSavedGames,
        bool showCreateSaveUI, bool showDeleteSaveUI,
        Action<SelectUIStatus, ISavedGameMetadata> callback) {
        Misc.CheckNotNull(uiTitle);
        Misc.CheckNotNull(callback);

        callback = ToOnGameThread(callback);

        if (!(maxDisplayedSavedGames > 0)) {
            Logger.e("maxDisplayedSavedGames must be greater than 0");
            callback(SelectUIStatus.BadInputError, null);
            return;
        }

        mSnapshotManager.SnapshotSelectUI(showCreateSaveUI, showDeleteSaveUI,
            maxDisplayedSavedGames, uiTitle,
            response => callback(
                AsUIStatus(response.RequestStatus()),
                response.RequestSucceeded() ? response.Data() : null));
    }

    public void CommitUpdate(ISavedGameMetadata metadata, SavedGameMetadataUpdate updateForMetadata,
        byte[] updatedBinaryData, Action<SavedGameRequestStatus, ISavedGameMetadata> callback) {
        Misc.CheckNotNull(metadata);
        Misc.CheckNotNull(updatedBinaryData);
        Misc.CheckNotNull(callback);

        callback = ToOnGameThread(callback);

        NativeSnapshotMetadata convertedMetadata = metadata as NativeSnapshotMetadata;

        if (convertedMetadata == null) {
            Logger.e("Encountered metadata that was not generated by this ISavedGameClient");
            callback(SavedGameRequestStatus.BadInputError, null);
            return;
        }

        if (!convertedMetadata.IsOpen) {
            Logger.e("This method requires an open ISavedGameMetadata.");
            callback(SavedGameRequestStatus.BadInputError, null);
            return;
        }

        mSnapshotManager.Commit(convertedMetadata,
            AsMetadataChange(updateForMetadata),
            updatedBinaryData,
            response => {
                if (!response.RequestSucceeded()) {
                    callback(AsRequestStatus(response.ResponseStatus()), null);
                } else {
                    callback(SavedGameRequestStatus.Success, response.Data());
                }
            }
        );
    }

    public void FetchAllSavedGames(DataSource source,
        Action<SavedGameRequestStatus, List<ISavedGameMetadata>> callback) {
        Misc.CheckNotNull(callback);

        callback = ToOnGameThread(callback);

        mSnapshotManager.FetchAll(AsDataSource(source),
            response => {
                if (!response.RequestSucceeded()) {
                    callback(AsRequestStatus(response.ResponseStatus()),
                        new List<ISavedGameMetadata>());
                } else {
                    callback(SavedGameRequestStatus.Success,
                        response.Data().Cast<ISavedGameMetadata>().ToList());
                }
            });
    }

    /// <summary>
    /// A helper class that is responsible for managing the pre-fetching callbacks for binary data.
    /// This class functions by tracking when the read operations for both original and unmerged
    /// have completed successfully (at which point we invoke the resolution callback) or
    /// determining if there is an error during either fetch.
    /// </summary>
    private class Prefetcher {
        private readonly object mLock = new object();
        private bool mOriginalDataFetched;
        private byte[] mOriginalData;
        private bool mUnmergedDataFetched;
        private byte[] mUnmergedData;
        private Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback;
        private readonly Action<byte[], byte[]> mDataFetchedCallback;

        internal Prefetcher(Action<byte[], byte[]> dataFetchedCallback,
            Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback) {
            this.mDataFetchedCallback = Misc.CheckNotNull(dataFetchedCallback);
            this.completedCallback = Misc.CheckNotNull(completedCallback);
        }

        internal void OnOriginalDataRead(SnapshotManager.ReadResponse readResponse) {
            lock (mLock) {
                // If the request doesn't succeed, report the error and make the callback a noop
                // so that we don't invoke the callback twice if both reads fail.
                if (!readResponse.RequestSucceeded()) {
                    Logger.e("Encountered error while prefetching original data.");
                    completedCallback(AsRequestStatus(readResponse.ResponseStatus()), null);
                    completedCallback = delegate {};
                } else {
                    Logger.d("Successfully fetched original data");
                    mOriginalDataFetched = true;
                    mOriginalData = readResponse.Data();
                    MaybeProceed();
                }
            }
        }

        internal void OnUnmergedDataRead(SnapshotManager.ReadResponse readResponse) {
            lock (mLock) {
                // If the request doesn't succeed, report the error and make the callback a noop
                // so that we don't invoke the callback twice if both reads fail.
                if (!readResponse.RequestSucceeded()) {
                    Logger.e("Encountered error while prefetching unmerged data.");
                    completedCallback(AsRequestStatus(readResponse.ResponseStatus()), null);
                    completedCallback = delegate {};
                } else {
                    Logger.d("Successfully fetched unmerged data");
                    mUnmergedDataFetched = true;
                    mUnmergedData = readResponse.Data();
                    MaybeProceed();
                }
            }
        }

        private void MaybeProceed() {
            if (mOriginalDataFetched && mUnmergedDataFetched) {
                Logger.d("Fetched data for original and unmerged, proceeding");
                mDataFetchedCallback(mOriginalData, mUnmergedData);
            } else {
                Logger.d("Not all data fetched - original:" + mOriginalDataFetched +
                    " unmerged:" + mUnmergedDataFetched);
            }
        }
    }

    internal static bool IsValidFilename(string filename) {
        if (filename == null) {
            return false;
        }

        return ValidFilenameRegex.IsMatch(filename);
    }

    private static Types.SnapshotConflictPolicy AsConflictPolicy(
        ConflictResolutionStrategy strategy) {
        switch(strategy) {
            case ConflictResolutionStrategy.UseLongestPlaytime:
                return Types.SnapshotConflictPolicy.LONGEST_PLAYTIME;
            case ConflictResolutionStrategy.UseOriginal:
                return Types.SnapshotConflictPolicy.LAST_KNOWN_GOOD;
            case ConflictResolutionStrategy.UseUnmerged:
                return Types.SnapshotConflictPolicy.MOST_RECENTLY_MODIFIED;
            default:
                throw new InvalidOperationException("Found unhandled strategy: " + strategy);
        }
    }

    private static SavedGameRequestStatus AsRequestStatus(Status.SnapshotOpenStatus status) {
        switch (status) {
            case Status.SnapshotOpenStatus.VALID:
                return SavedGameRequestStatus.Success;
            case Status.SnapshotOpenStatus.ERROR_NOT_AUTHORIZED:
                return SavedGameRequestStatus.AuthenticationError;
            case Status.SnapshotOpenStatus.ERROR_TIMEOUT:
                return SavedGameRequestStatus.TimeoutError;
            default:
                Logger.e("Encountered unknown status: " + status);
                return SavedGameRequestStatus.InternalError;
        }
    }

    private static Types.DataSource AsDataSource(DataSource source) {
        switch (source) {
            case DataSource.ReadCacheOrNetwork:
                return Types.DataSource.CACHE_OR_NETWORK;
            case DataSource.ReadNetworkOnly:
                return Types.DataSource.NETWORK_ONLY;
            default:
                throw new InvalidOperationException("Found unhandled DataSource: " + source);

        }
    }

    private static SavedGameRequestStatus AsRequestStatus(Status.ResponseStatus status) {
        switch (status) {
            case Status.ResponseStatus.ERROR_INTERNAL:
                return SavedGameRequestStatus.InternalError;
            case Status.ResponseStatus.ERROR_LICENSE_CHECK_FAILED:
                Logger.e("User attempted to use the game without a valid license.");
                return SavedGameRequestStatus.AuthenticationError;
            case Status.ResponseStatus.ERROR_NOT_AUTHORIZED:
                Logger.e("User was not authorized (they were probably not logged in).");
                return SavedGameRequestStatus.AuthenticationError;
            case Status.ResponseStatus.ERROR_TIMEOUT:
                return SavedGameRequestStatus.TimeoutError;
            case Status.ResponseStatus.VALID:
            case Status.ResponseStatus.VALID_BUT_STALE:
                return SavedGameRequestStatus.Success;
            default:
                Logger.e("Unknown status: " + status);
                return SavedGameRequestStatus.InternalError;
        }
    }

    private static SelectUIStatus AsUIStatus(Status.UIStatus uiStatus) {
        switch (uiStatus) {
            case Status.UIStatus.VALID:
                return SelectUIStatus.SavedGameSelected;
            case Status.UIStatus.ERROR_CANCELED:
                return SelectUIStatus.UserClosedUI;
            case Status.UIStatus.ERROR_INTERNAL:
                return SelectUIStatus.InternalError;
            case Status.UIStatus.ERROR_NOT_AUTHORIZED:
                return SelectUIStatus.AuthenticationError;
            case Status.UIStatus.ERROR_TIMEOUT:
                return SelectUIStatus.TimeoutError;
            default:
                Logger.e("Encountered unknown UI Status: " + uiStatus);
                return SelectUIStatus.InternalError;
        }
    }

    private static NativeSnapshotMetadataChange AsMetadataChange(SavedGameMetadataUpdate update) {
        var builder = new NativeSnapshotMetadataChange.Builder();

        if (update.IsCoverImageUpdated) {
            builder.SetCoverImageFromPngData(update.UpdatedPngCoverImage);
        }

        if (update.IsDescriptionUpdated) {
            builder.SetDescription(update.UpdatedDescription);
        }

        if (update.IsPlayedTimeUpdated) {
            builder.SetPlayedTime((ulong)update.UpdatedPlayedTime.Value.TotalMilliseconds);
        }

        return builder.Build();
    }

    private static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert) {
        return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
    }
}
}

#endif
