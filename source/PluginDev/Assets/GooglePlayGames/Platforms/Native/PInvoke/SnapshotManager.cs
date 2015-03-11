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
using GooglePlayGames.OurUtils;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;


#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

using C = GooglePlayGames.Native.Cwrapper.SnapshotManager;
using Status = GooglePlayGames.Native.Cwrapper.CommonErrorStatus;
using Types = GooglePlayGames.Native.Cwrapper.Types;

namespace GooglePlayGames.Native.PInvoke {
internal class SnapshotManager {
    private readonly GameServices mServices;

    internal SnapshotManager(GameServices services) {
        mServices = Misc.CheckNotNull(services);
    }

    internal void FetchAll(Types.DataSource source,
                           Action<FetchAllResponse> callback) {
        C.SnapshotManager_FetchAll(
            mServices.AsHandle(),
            source,
            InternalFetchAllCallback,
            Callbacks.ToIntPtr<FetchAllResponse>(callback, FetchAllResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.FetchAllCallback))]
    internal static void InternalFetchAllCallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "SnapshotManager#FetchAllCallback", Callbacks.Type.Temporary, response, data);
    }

    internal void SnapshotSelectUI(
        bool allowCreate,
        bool allowDelete,
        uint maxSnapshots,
        string uiTitle,
        Action<SnapshotSelectUIResponse> callback) {
        C.SnapshotManager_ShowSelectUIOperation(
            mServices.AsHandle(),
            allowCreate,
            allowDelete,
            maxSnapshots,
            uiTitle,
            InternalSnapshotSelectUICallback,
            Callbacks.ToIntPtr<SnapshotSelectUIResponse>(
                callback, SnapshotSelectUIResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.SnapshotSelectUICallback))]
    internal static void InternalSnapshotSelectUICallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "SnapshotManager#SnapshotSelectUICallback", Callbacks.Type.Temporary, response, data);
    }

    internal void Open(string fileName, Types.DataSource source,
        Types.SnapshotConflictPolicy conflictPolicy, Action<OpenResponse> callback) {
        Misc.CheckNotNull(fileName);
        Misc.CheckNotNull(callback);
        C.SnapshotManager_Open(mServices.AsHandle(),
            source,
            fileName,
            conflictPolicy,
            InternalOpenCallback,
            Callbacks.ToIntPtr<OpenResponse>(callback, OpenResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.OpenCallback))]
    internal static void InternalOpenCallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "SnapshotManager#OpenCallback", Callbacks.Type.Temporary, response, data);
    }

    internal void Commit(NativeSnapshotMetadata metadata, NativeSnapshotMetadataChange metadataChange,
        byte[] updatedData, Action<CommitResponse> callback) {
        Misc.CheckNotNull(metadata);
        Misc.CheckNotNull(metadataChange);
        C.SnapshotManager_Commit(
            mServices.AsHandle(),
            metadata.AsPointer(),
            metadataChange.AsPointer(),
            updatedData,
            new UIntPtr((ulong) updatedData.Length),
            InternalCommitCallback,
            Callbacks.ToIntPtr<CommitResponse>(callback, CommitResponse.FromPointer));
    }

    internal void Resolve(NativeSnapshotMetadata metadata,
        NativeSnapshotMetadataChange metadataChange,
        string conflictId, Action<CommitResponse> callback) {
        Misc.CheckNotNull(metadata);
        Misc.CheckNotNull(metadataChange);
        Misc.CheckNotNull(conflictId);

        C.SnapshotManager_ResolveConflict(
            mServices.AsHandle(),
            metadata.AsPointer(),
            metadataChange.AsPointer(),
            conflictId,
            InternalCommitCallback,
            Callbacks.ToIntPtr<CommitResponse>(callback, CommitResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.CommitCallback))]
    internal static void InternalCommitCallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "SnapshotManager#CommitCallback", Callbacks.Type.Temporary, response, data);
    }

    internal void Delete(NativeSnapshotMetadata metadata) {
        Misc.CheckNotNull(metadata);

        C.SnapshotManager_Delete(
            mServices.AsHandle(),
            metadata.AsPointer());
    }

    internal void Read(NativeSnapshotMetadata metadata, Action<ReadResponse> callback) {
        Misc.CheckNotNull(metadata);
        Misc.CheckNotNull(callback);

        C.SnapshotManager_Read(
            mServices.AsHandle(),
            metadata.AsPointer(),
            InternalReadCallback,
            Callbacks.ToIntPtr<ReadResponse>(callback, ReadResponse.FromPointer));
    }

    [AOT.MonoPInvokeCallback(typeof(C.ReadCallback))]
    internal static void InternalReadCallback(IntPtr response, IntPtr data) {
        Callbacks.PerformInternalCallback(
            "SnapshotManager#ReadCallback", Callbacks.Type.Temporary, response, data);
    }

    internal class OpenResponse : BaseReferenceHolder {
        internal OpenResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        internal bool RequestSucceeded() {
            return ResponseStatus() > 0;
        }

        internal Status.SnapshotOpenStatus ResponseStatus() {
            return C.SnapshotManager_OpenResponse_GetStatus(SelfPtr());
        }

        internal string ConflictId() {
            if (ResponseStatus() != Status.SnapshotOpenStatus.VALID_WITH_CONFLICT) {
                throw new InvalidOperationException("OpenResponse did not have a conflict");
            }

            return PInvokeUtilities.OutParamsToString(
                (out_string, out_size) => C.SnapshotManager_OpenResponse_GetConflictId(
                    SelfPtr(), out_string, out_size));
        }

        internal NativeSnapshotMetadata Data() {
            if (ResponseStatus() != Status.SnapshotOpenStatus.VALID) {
                throw new InvalidOperationException("OpenResponse had a conflict");
            }

            return new NativeSnapshotMetadata(C.SnapshotManager_OpenResponse_GetData(SelfPtr()));
        }

        internal NativeSnapshotMetadata ConflictOriginal() {
            if (ResponseStatus() != Status.SnapshotOpenStatus.VALID_WITH_CONFLICT) {
                throw new InvalidOperationException("OpenResponse did not have a conflict");
            }

            return new NativeSnapshotMetadata(
                C.SnapshotManager_OpenResponse_GetConflictOriginal(SelfPtr()));
        }

        internal NativeSnapshotMetadata ConflictUnmerged() {
            if (ResponseStatus() != Status.SnapshotOpenStatus.VALID_WITH_CONFLICT) {
                throw new InvalidOperationException("OpenResponse did not have a conflict");
            }

            return new NativeSnapshotMetadata(
                C.SnapshotManager_OpenResponse_GetConflictUnmerged(SelfPtr()));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.SnapshotManager_OpenResponse_Dispose(selfPointer);
        }

        internal static OpenResponse FromPointer(IntPtr pointer) {
            if (pointer.Equals(IntPtr.Zero)) {
                return null;
            }

            return new OpenResponse(pointer);
        }
    }

    internal class FetchAllResponse : BaseReferenceHolder {
        internal FetchAllResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        internal Status.ResponseStatus ResponseStatus() {
            return C.SnapshotManager_FetchAllResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded() {
            return ResponseStatus() > 0;
        }

        internal IEnumerable<NativeSnapshotMetadata> Data() {
            return PInvokeUtilities.ToEnumerable<NativeSnapshotMetadata>(
                C.SnapshotManager_FetchAllResponse_GetData_Length(SelfPtr()),
                index => new NativeSnapshotMetadata(
                    C.SnapshotManager_FetchAllResponse_GetData_GetElement(SelfPtr(), index)));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.SnapshotManager_FetchAllResponse_Dispose(selfPointer);
        }

        internal static FetchAllResponse FromPointer(IntPtr pointer) {
            if (pointer.Equals(IntPtr.Zero)) {
                return null;
            }

            return new FetchAllResponse(pointer);
        }
    }

    internal class CommitResponse : BaseReferenceHolder {
        internal CommitResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        internal Status.ResponseStatus ResponseStatus() {
            return C.SnapshotManager_CommitResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded() {
            return ResponseStatus() > 0;
        }

        internal NativeSnapshotMetadata Data() {
            if (!RequestSucceeded()) {
                throw new InvalidOperationException("Request did not succeed");
            }

            return new NativeSnapshotMetadata(C.SnapshotManager_CommitResponse_GetData(SelfPtr()));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.SnapshotManager_CommitResponse_Dispose(selfPointer);
        }

        internal static CommitResponse FromPointer(IntPtr pointer) {
            if (pointer.Equals(IntPtr.Zero)) {
                return null;
            }

            return new CommitResponse(pointer);
        }
    }

    internal class ReadResponse : BaseReferenceHolder {
        internal ReadResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        internal Status.ResponseStatus ResponseStatus() {
            return C.SnapshotManager_CommitResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded() {
            return ResponseStatus() > 0;
        }

        internal byte[] Data() {
            if (!RequestSucceeded()) {
                throw new InvalidOperationException("Request did not succeed");
            }

            return PInvokeUtilities.OutParamsToBytes(
                (out_bytes, out_size) => C.SnapshotManager_ReadResponse_GetData(
                    SelfPtr(), out_bytes, out_size));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.SnapshotManager_ReadResponse_Dispose(selfPointer);
        }

        internal static ReadResponse FromPointer(IntPtr pointer) {
            if (pointer.Equals(IntPtr.Zero)) {
                return null;
            }

            return new ReadResponse(pointer);
        }
    }

    internal class SnapshotSelectUIResponse : BaseReferenceHolder {
        internal SnapshotSelectUIResponse(IntPtr selfPointer) : base(selfPointer) {
        }

        internal Status.UIStatus RequestStatus() {
            return C.SnapshotManager_SnapshotSelectUIResponse_GetStatus(SelfPtr());
        }

        internal bool RequestSucceeded() {
            return RequestStatus() > 0;
        }

        internal NativeSnapshotMetadata Data() {
            if (!RequestSucceeded()) {
                throw new InvalidOperationException("Request did not succeed");
            }

            return new NativeSnapshotMetadata(
                C.SnapshotManager_SnapshotSelectUIResponse_GetData(SelfPtr()));
        }

        protected override void CallDispose(HandleRef selfPointer) {
            C.SnapshotManager_SnapshotSelectUIResponse_Dispose(selfPointer);
        }

        internal static SnapshotSelectUIResponse FromPointer(IntPtr pointer) {
            if (pointer.Equals(IntPtr.Zero)) {
                return null;
            }

            return new SnapshotSelectUIResponse(pointer);
        }
    }
}
}

#endif // (UNITY_ANDROID || UNITY_IPHONE)
