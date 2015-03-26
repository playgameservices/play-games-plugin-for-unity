// <copyright file="SnapshotManager.cs" company="Google Inc.">
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

#if (UNITY_ANDROID || (UNITY_IPHONE && !NO_GPGS))

namespace GooglePlayGames.Native.Cwrapper
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class SnapshotManager
    {
        internal delegate void FetchAllCallback(
        /* from(SnapshotManager_FetchAllResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void OpenCallback(
        /* from(SnapshotManager_OpenResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void CommitCallback(
        /* from(SnapshotManager_CommitResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void ReadCallback(
        /* from(SnapshotManager_ReadResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        internal delegate void SnapshotSelectUICallback(
        /* from(SnapshotManager_SnapshotSelectUIResponse_t) */ IntPtr arg0,
        /* from(void *) */ IntPtr arg1);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_FetchAll(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(SnapshotManager_FetchAllCallback_t) */FetchAllCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_ShowSelectUIOperation(
            HandleRef self,
            [MarshalAs(UnmanagedType.I1)] /* from(bool) */ bool allow_create,
            [MarshalAs(UnmanagedType.I1)] /* from(bool) */ bool allow_delete,
         /* from(uint32_t) */uint max_snapshots,
         /* from(char const *) */string title,
         /* from(SnapshotManager_SnapshotSelectUICallback_t) */SnapshotSelectUICallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_Read(
            HandleRef self,
         /* from(SnapshotMetadata_t) */IntPtr snapshot_metadata,
         /* from(SnapshotManager_ReadCallback_t) */ReadCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_Commit(
            HandleRef self,
         /* from(SnapshotMetadata_t) */IntPtr snapshot_metadata,
         /* from(SnapshotMetadataChange_t) */IntPtr metadata_change,
         /* from(uint8_t const *) */byte[] data,
         /* from(size_t) */UIntPtr data_size,
         /* from(SnapshotManager_CommitCallback_t) */CommitCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_Open(
            HandleRef self,
         /* from(DataSource_t) */Types.DataSource data_source,
         /* from(char const *) */string file_name,
         /* from(SnapshotConflictPolicy_t) */Types.SnapshotConflictPolicy conflict_policy,
         /* from(SnapshotManager_OpenCallback_t) */OpenCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_ResolveConflict(
            HandleRef self,
         /* from(SnapshotMetadata_t) */IntPtr snapshot_metadata,
         /* from(SnapshotMetadataChange_t) */IntPtr metadata_change,
         /* from(char const *) */string conflict_id,
         /* from(SnapshotManager_CommitCallback_t) */CommitCallback callback,
         /* from(void *) */IntPtr callback_arg);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_Delete(
            HandleRef self,
         /* from(SnapshotMetadata_t) */IntPtr snapshot_metadata);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_FetchAllResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus SnapshotManager_FetchAllResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr SnapshotManager_FetchAllResponse_GetData_Length(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(SnapshotMetadata_t) */ IntPtr SnapshotManager_FetchAllResponse_GetData_GetElement(
            HandleRef self,
         /* from(size_t) */UIntPtr index);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_OpenResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(SnapshotOpenStatus_t) */ CommonErrorStatus.SnapshotOpenStatus SnapshotManager_OpenResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(SnapshotMetadata_t) */ IntPtr SnapshotManager_OpenResponse_GetData(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr SnapshotManager_OpenResponse_GetConflictId(
            HandleRef self,
         /* from(char *) */StringBuilder out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(SnapshotMetadata_t) */ IntPtr SnapshotManager_OpenResponse_GetConflictOriginal(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(SnapshotMetadata_t) */ IntPtr SnapshotManager_OpenResponse_GetConflictUnmerged(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_CommitResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus SnapshotManager_CommitResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(SnapshotMetadata_t) */ IntPtr SnapshotManager_CommitResponse_GetData(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_ReadResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(ResponseStatus_t) */ CommonErrorStatus.ResponseStatus SnapshotManager_ReadResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(size_t) */ UIntPtr SnapshotManager_ReadResponse_GetData(
            HandleRef self,
            [In, Out] /* from(uint8_t *) */ byte[] out_arg,
         /* from(size_t) */UIntPtr out_size);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern void SnapshotManager_SnapshotSelectUIResponse_Dispose(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(UIStatus_t) */ CommonErrorStatus.UIStatus SnapshotManager_SnapshotSelectUIResponse_GetStatus(
            HandleRef self);

        [DllImport(SymbolLocation.NativeSymbolLocation)]
        internal static extern /* from(SnapshotMetadata_t) */ IntPtr SnapshotManager_SnapshotSelectUIResponse_GetData(
            HandleRef self);
    }
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
