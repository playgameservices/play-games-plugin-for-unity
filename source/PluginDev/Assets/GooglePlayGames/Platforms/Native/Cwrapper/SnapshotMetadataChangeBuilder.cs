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
using System.Runtime.InteropServices;
using System.Text;

namespace GooglePlayGames.Native.Cwrapper {
internal static class SnapshotMetadataChangeBuilder {
    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void SnapshotMetadataChange_Builder_SetDescription(
        HandleRef self,
         /* from(char const *) */ string description);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(SnapshotMetadataChange_Builder_t) */ IntPtr SnapshotMetadataChange_Builder_Construct();

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void SnapshotMetadataChange_Builder_SetPlayedTime(
        HandleRef self,
         /* from(uint64_t) */ ulong played_time);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void SnapshotMetadataChange_Builder_SetCoverImageFromPngData(
        HandleRef self,
         /* from(uint8_t const *) */ byte[] png_data,
         /* from(size_t) */ UIntPtr png_data_size);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern /* from(SnapshotMetadataChange_t) */ IntPtr SnapshotMetadataChange_Builder_Create(
        HandleRef self);

    [DllImport(SymbolLocation.NativeSymbolLocation)]
    internal static extern void SnapshotMetadataChange_Builder_Dispose(
        HandleRef self);
}
}
#endif // (UNITY_ANDROID || UNITY_IPHONE)
